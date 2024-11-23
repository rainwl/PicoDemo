using System.Collections.Generic;
using Microsoft.MixedReality.OpenXR;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.Interaction.Toolkit;


namespace Dock
{
    /// <summary>
    /// Add a Dockable component to any object that has a <see cref="Dockable"/> and an <see cref="ObjectManipulator"/>
    /// or <see cref="Microsoft.MixedReality.Toolkit.UI.ManipulationHandler"/> to make it dockable in Docks. That allows this object to be used
    /// as part of a palette, shelf or navigation bar together with other objects.
    /// </summary>
    /// <seealso cref="Dock"/>
    /// <seealso cref="DockPosition"/>
    [AddComponentMenu("Scripts/MRTK/Experimental/Dock/Dockable")]
    public class Dockable : MonoBehaviour
    {
        #region 0.默认字段

        //[Experimental]
        [SerializeField, ReadOnly] [Tooltip("Current state of this dockable in regards to a dock.")]
        private DockingState dockingState = DockingState.Undocked;

        [SerializeField] [Tooltip("Time to animate any move/scale into or out of the dock.")]
        private float moveLerpTime = 0.1f;

        [SerializeField] [Tooltip("Time to animate an element when it's following the dock (use 0 for tight attachment)")]
        private float moveLerpTimeWhenDocked = 0.05f;

        /// <summary>
        /// True if this object can currently be docked, false otherwise.
        /// </summary>
        public bool CanDock => dockingState == DockingState.Undocked || dockingState == DockingState.Undocking;

        /// <summary>
        /// True if this object can currently be undocked, false otherwise.
        /// </summary>
        public bool CanUndock => dockingState == DockingState.Docked;

        // Constants
        private const float DistanceTolerance = 0.01f; // in meters
        private const float AngleTolerance = 3.0f; // in degrees
        private const float ScaleTolerance = 0.01f; // in percentage

        private DockPosition dockedPosition = null;
        private Vector3 dockedPositionScale = Vector3.one;

        private HashSet<DockPosition> overlappingPositions = new HashSet<DockPosition>();
        private Vector3 originalScale = Vector3.one;
        private bool isDragging = false;

        public bool IsDragging
        {
            get => isDragging;
            set => isDragging = value;
        }

        private ObjectManipulator objectManipulator;
        public string partName;

        //public GameObject obj1;
        //public ARManager sc;
        private void Start()
        {
            partName = this.name;
            //GameObject obj1 = GameObject.Find("AssetManipulate");
            //     sc = obj1.GetComponent<AssetManipulate>();
        }

        #endregion

        #region 1.已经注释掉的On/Dis enable方法

        /// <summary>
        /// Subscribes to manipulation events.
        /// </summary>
        private void OnEnable()
        {
            objectManipulator = gameObject.GetComponent<ObjectManipulator>();
            if (objectManipulator != null)
            {
                objectManipulator.firstSelectEntered.AddListener(OnManipulationStarted);
                objectManipulator.lastSelectExited.AddListener(OnManipulationEnded);
            }


            Assert.IsTrue(objectManipulator != null, "A Dockable object must have either an ObjectManipulator or a ManipulationHandler component.");

            Assert.IsNotNull(gameObject.GetComponent<Collider>(), "A Dockable object must have a Collider component.");
        }

        /// <summary>
        /// Unsubscribes from manipulation events.
        /// </summary>
        private void OnDisable()
        {
            if (objectManipulator != null)
            {
                objectManipulator.firstSelectEntered.RemoveListener(OnManipulationStarted);
                objectManipulator.lastSelectExited.RemoveListener(OnManipulationEnded);

                objectManipulator = null;
            }


            if (dockedPosition != null)
            {
                dockedPosition.DockedObject = null;
                dockedPosition = null;
            }

            overlappingPositions.Clear();
            dockingState = DockingState.Undocked;
        }

        #endregion


        //AssetManipulate assetManipulate;
        /// <summary>
        /// Updates the transform and state of this object every frame, depending on 
        /// manipulations and docking state.
        /// </summary>
        public void Update()
        {
            /* 这里是ipadpro的sever端的update里用的Asset Manipulate的selecting，但是这里用的是MRTK，所以注释掉
            //将AssetManipulate脚本中的selecting bool值传递到本脚本的isdragging中

            if(sc.Selecting == true)
            {
                isDragging = true;
                if (CanUndock)
                {
                    Undock();//切换成了网络方法
                             //Undock();
                }
            }
            else if(sc.Selecting == false)
            {
                isDragging = false;
                isDragging = false;

                if (overlappingPositions.Count > 0 && CanDock)
                {
                    var closestPosition = GetClosestPosition();



                    Dock(closestPosition);
                }

            }
            */

            //将下面注释了，是因为不需要能够给自由移动到其他位置
            //if (isDragging && overlappingPositions.Count > 0)
            //{
            //    var closestPosition = GetClosestPosition();
            //    if (closestPosition.IsOccupied)
            //    {
            //        closestPosition.GetComponentInParent<Dock>().TryMoveToFreeSpace(closestPosition);
            //    }
            //}

            if (dockingState == DockingState.Docked || dockingState == DockingState.Docking)
            {
                Assert.IsNotNull(dockedPosition, "When a dockable is docked, its dockedPosition must be valid.");
                Assert.AreEqual(dockedPosition.DockedObject, this, "When a dockable is docked, its dockedPosition reference the dockable.");

                var lerpTime = dockingState == DockingState.Docked ? moveLerpTimeWhenDocked : moveLerpTime;

                if (!isDragging)
                {
                    // Don't override dragging
                    transform.position = Solver.SmoothTo(transform.position, dockedPosition.transform.position, Time.deltaTime, lerpTime);
                    transform.rotation = Solver.SmoothTo(transform.rotation, dockedPosition.transform.rotation, Time.deltaTime, lerpTime);
                }

                transform.localScale = Solver.SmoothTo(transform.localScale, dockedPositionScale, Time.deltaTime, lerpTime);

                if (VectorExtensions.CloseEnough(dockedPosition.transform.position, transform.position, DistanceTolerance) &&
                    QuaternionExtensions.AlignedEnough(dockedPosition.transform.rotation, transform.rotation, AngleTolerance) &&
                    AboutTheSameSize(dockedPositionScale.x, transform.localScale.x))
                {
                    // Finished docking
                    dockingState = DockingState.Docked;

                    // Snap to position
                    transform.position = dockedPosition.transform.position;
                    transform.rotation = dockedPosition.transform.rotation;
                    transform.localScale = dockedPositionScale;
                }
            }
            else if (dockedPosition == null && dockingState == DockingState.Undocking)
            {
                transform.localScale = Solver.SmoothTo(transform.localScale, originalScale, Time.deltaTime, moveLerpTime);

                if (AboutTheSameSize(originalScale.x, transform.localScale.x))
                {
                    // Finished undocking
                    dockingState = DockingState.Undocked;

                    // Snap to size
                    transform.localScale = originalScale;
                }
            }
        }

        /// <summary>
        /// Docks this object in a given <see cref="DockPosition"/>.
        /// </summary>
        /// <param name="position">The <see cref="DockPosition"/> where we'd like to dock this object.</param>
        public void Dock(DockPosition position)
        {
            if (!CanDock)
            {
                Debug.LogError($"Trying to dock an object that was not undocked. State = {dockingState}");
                return;
            }

            Debug.Log($"Docking object {gameObject.name} on position {position.gameObject.name}");

            dockedPosition = position;

            //修改的脚本
            if (dockedPosition.name == partName)
            {
                dockedPosition.DockedObject = this;
                dockingState = DockingState.Docking; //状态转换为正在锁定
            }


            //dockedPosition.DockedObject = this;


            //这里注释掉的原因是，Hololens2端，零件归位的时候，大一不一致
            /*
            float scaleToFit = gameObject.GetComponent<Collider>().bounds.GetScaleToFitInside(dockedPosition.GetComponent<Collider>().bounds);
            dockedPositionScale = transform.localScale * scaleToFit;
            */


            if (dockingState == DockingState.Undocked)
            {
                // Only register the original scale when first docking
                originalScale = transform.localScale;
            }

            //dockingState = DockingState.Docking;//因为在上面的那个判断里面我加了这句话，所以这里注释了
        }

        /// <summary>
        /// Undocks this <see cref="Dockable"/> from the current <see cref="DockPosition"/> where it is docked.
        /// </summary>
        public void Undock()
        {
            if (!CanUndock)
            {
                Debug.LogError($"Trying to undock an object that was not docked. State = {dockingState}");
                return;
            }

            Debug.Log($"Undocking object {gameObject.name} from position {dockedPosition.gameObject.name}");

            dockedPosition.DockedObject = null;
            dockedPosition = null;
            dockedPositionScale = Vector3.one;
            dockingState = DockingState.Undocking;
        }

        #region Collision events

        void OnTriggerEnter(Collider collider)
        {
            var dockPosition = collider.gameObject.GetComponent<DockPosition>();
            if (dockPosition != null)
            {
                overlappingPositions.Add(dockPosition);
                Debug.Log($"{gameObject.name} collided with {dockPosition.name}");
            }
        }

        void OnTriggerExit(Collider collider)
        {
            var dockPosition = collider.gameObject.GetComponent<DockPosition>();
            if (overlappingPositions.Contains(dockPosition))
            {
                overlappingPositions.Remove(dockPosition);
            }
        }

        #endregion

        #region Manipulation events

        private void OnManipulationStarted(SelectEnterEventArgs e)
        {
            isDragging = true;

            if (CanUndock)
            {
                Undock(); //切换成了网络方法
                //Undock();
            }
        }


        private void OnManipulationEnded(SelectExitEventArgs e)
        {
            //这里被注释掉，是因为用了PUNOnManiEndDock（）方法，为了实现网络同步，如需取消网络同步，直接取消注释，并将下面的PUN方法注释掉即可。

            isDragging = false;

            if (overlappingPositions.Count > 0 && CanDock)
            {
                var closestPosition = GetClosestPosition();
                if (closestPosition.IsOccupied)
                {
                    if (!closestPosition.GetComponentInParent<Dock>().TryMoveToFreeSpace(closestPosition))
                    {
                        return;
                    }
                }

                Dock(closestPosition);
            }
            //OnManiEndDock();
        }


        //这里可以直接不用，因为上面直接写了这个方法，不需要使用photon的网络同步方法
        //自己加一句，为了让OnManipulationEnded的参数去掉，嵌套一层
        public void OnManiEndDock()
        {
            isDragging = false;

            if (overlappingPositions.Count > 0 && CanDock)
            {
                var closestPosition = GetClosestPosition();
                if (closestPosition.IsOccupied)
                {
                    if (!closestPosition.GetComponentInParent<Dock>().TryMoveToFreeSpace(closestPosition))
                    {
                        return;
                    }
                }

                Dock(closestPosition);
            }
        }
        //这个放在[OnManipulationEnded]里面，把原来的内部的语句注释掉，然后换成[PUN]的[OnManiEndDock]

        #endregion


        /// <summary>
        /// Gets the overlapping <see cref="DockPosition"/> that is closest to this Dockable.
        /// </summary>
        /// <returns>The overlapping <see cref="DockPosition"/> that is closest to this <see cref="Dockable"/>, or null if no positions overlap.</returns>
        public DockPosition GetClosestPosition()
        {
            var bounds = gameObject.GetComponent<Collider>().bounds;
            var minDistance = float.MaxValue;
            DockPosition closestPosition = null;
            foreach (var position in overlappingPositions)
            {
                var distance = (position.gameObject.GetComponent<Collider>().bounds.center - bounds.center).sqrMagnitude;
                if (closestPosition == null || distance < minDistance)
                {
                    closestPosition = position;
                    minDistance = distance;
                }
            }

            return closestPosition;
        }

        #region Helpers

        private static bool AboutTheSameSize(float scale1, float scale2)
        {
            Assert.AreNotEqual(0.0f, scale2, "Cannot compare scales with an object that has scale zero.");
            return Mathf.Abs(scale1 / scale2 - 1.0f) < ScaleTolerance;
        }

        #endregion
    }
}