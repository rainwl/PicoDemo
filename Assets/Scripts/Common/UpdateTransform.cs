using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using MixedReality.Toolkit;
using Unity.XR.PXR;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Common
{
    public class UpdateTransform : MonoBehaviour,IXRSelectInteractor,IXRHoverInteractor
    {
        public long updateID;
        public Vector3 lastPosition;
        public Vector3 lastEulerAngles;
        public Vector3 lastScale;
        
        private bool _isPointDown;
        private bool _isFocus;

        private void Start()
        {
            lastPosition = transform.localPosition;
            lastEulerAngles = transform.localEulerAngles;
            lastScale = transform.localScale;
            UpdateTransformManager.Instance.TransformItmeList.Add(this);
        }

        private void OnDestroy()
        {
            UpdateTransformManager.Instance.TransformItmeList.Remove(this);
        }

        internal bool NeedUpdate()
        {
            if (!_isPointDown) return false;
            if (Vector3.Distance(lastPosition, transform.localPosition) > UpdateTransformManager.Instance.Threshold ||
                Mathf.Abs(transform.localEulerAngles.x - lastEulerAngles.x) > 0 ||
                Mathf.Abs(transform.localEulerAngles.y - lastEulerAngles.y) > 0 ||
                Mathf.Abs(transform.localEulerAngles.z - lastEulerAngles.z) > 0 ||
                Mathf.Abs(transform.localScale.x - lastScale.x) > 0 ||
                Mathf.Abs(transform.localScale.y - lastScale.y) > 0 ||
                Mathf.Abs(transform.localScale.z - lastScale.z) > 0
               )
            {
                lastPosition = transform.localPosition;
                lastEulerAngles = transform.localEulerAngles;
                lastScale = transform.localScale;
                return true;
            }
            else
            {
                return false;
            }
        }

        internal void UpdateData(float px, float py, float pz, float rx, float ry, float rz, float sx, float sy, float sz)
        {
            if (_isPointDown) return;


            transform.localPosition = new Vector3(px, py, pz);
            transform.localEulerAngles = new Vector3(rx, ry, rz);
            transform.localScale = new Vector3(sx, sy, sz);

            lastPosition = transform.localPosition;
            lastEulerAngles = transform.localEulerAngles;
            lastScale = transform.localScale;
        }
        
        IEnumerator FocusTimmer(Vector3 position, Quaternion rot)
        {
            for (int i = 0; i < 100; i++)
            {
                if (!_isFocus) yield break;
                yield return new WaitForSeconds(0.02f);
            }

            ARManager.Instance.ShowAniButtons(this, position, rot);
        }


        public SelectEnterEvent selectEntered { get; }
        public SelectExitEvent selectExited { get; }
        public List<IXRSelectInteractable> interactablesSelected { get; }
        public IXRSelectInteractable firstInteractableSelected { get; }
        public bool hasSelection { get; }
        public bool isSelectActive { get; }
        public bool keepSelectedTargetValid { get; }
        public bool CanSelect(IXRSelectInteractable interactable)
        {
            throw new NotImplementedException();
        }

        public bool IsSelecting(IXRSelectInteractable interactable)
        {
            throw new NotImplementedException();
        }

        public Pose GetAttachPoseOnSelect(IXRSelectInteractable interactable)
        {
            throw new NotImplementedException();
        }

        public Pose GetLocalAttachPoseOnSelect(IXRSelectInteractable interactable)
        {
            throw new NotImplementedException();
        }

        public void OnSelectEntering(SelectEnterEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void OnSelectEntered(SelectEnterEventArgs args)
        {
            _isPointDown = true;
        }

        public void OnSelectExiting(SelectExitEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void OnSelectExited(SelectExitEventArgs args)
        {
            _isPointDown = false;
        }

        public HoverEnterEvent hoverEntered { get; }
        public HoverExitEvent hoverExited { get; }
        public List<IXRHoverInteractable> interactablesHovered { get; }
        public bool hasHover { get; }
        public bool isHoverActive { get; }
        public bool CanHover(IXRHoverInteractable interactable)
        {
            throw new NotImplementedException();
        }

        public bool IsHovering(IXRHoverInteractable interactable)
        {
            throw new NotImplementedException();
        }

        public void OnHoverEntering(HoverEnterEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void OnHoverEntered(HoverEnterEventArgs args)
        {
            if (GetComponentInChildren<Animator>() != null)
            {
                _isFocus = true;
                var interactor = args.interactorObject;
                if (interactor is XRBaseInteractor xrInteractor)
                {
                    var hitInfo = xrInteractor.transform;
                    var pos = hitInfo.position;
                    var rot = Quaternion.LookRotation(hitInfo.forward);
                    StartCoroutine(FocusTimmer(pos, rot));
                }
            }
        }

        public void OnHoverExiting(HoverExitEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void OnHoverExited(HoverExitEventArgs args)
        {
            _isFocus = false;
        }

        public event Action<InteractorRegisteredEventArgs> registered;
        public event Action<InteractorUnregisteredEventArgs> unregistered;
        public InteractionLayerMask interactionLayers { get; }
        public InteractorHandedness handedness { get; }
        public Transform GetAttachTransform(IXRInteractable interactable)
        {
            throw new NotImplementedException();
        }

        public void GetValidTargets(List<IXRInteractable> targets)
        {
            throw new NotImplementedException();
        }

        public void OnRegistered(InteractorRegisteredEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void OnUnregistered(InteractorUnregisteredEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            throw new NotImplementedException();
        }

        public void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            throw new NotImplementedException();
        }
    }
}