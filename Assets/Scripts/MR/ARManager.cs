using System;
using System.Collections;
using Common;
using Dock;
using MixedReality.Toolkit.SpatialManipulation;
using Network.Client;
using UI;
using UnityEngine;

namespace MR
{
    public class ARManager : MonoBehaviour
    {
        public Transform anchorRoot;
        public int scanOutTime = 15;
        public GameObject calibrationCube;
        public GameObject aniButtons;
        public float scaleMaximum = 10;
        public float scaleMinimum = 0.2f;
        public bool useLocalAsset;
        public bool canChildMove;

        public static ARManager Instance;
        private int _serverClientId;
        private bool _isScanning;
        private Vector3 _trackedPos;
        private Quaternion _trackedRot;
        private UpdateTransform _selectedUpdateTransform;
        private float _lastTime;

        private void Awake()
        {
            Instance = this;
        }

        public void OnRequestCalibration(BroadcastInfo info)
        {
            ShowMessageManager.Instance.ShowSelectBox("Start Calibration?", () => { StartCalibration(info); });
            PlaySoundManager.Instance.PlayEffect("StartScan");
            StartCalibration(info);
        }

        private void StartCalibration(BroadcastInfo info)
        {
            print("StartARCalibration");
            var height = BitConverter.ToSingle(info.Data, 0);
            _serverClientId = info.UserId;

            if (height < 0.1f)
            {
                ShowMessageManager.Instance.ShowMessage("The screen height is incorrect");
                return;
            }

            try
            {
                // ImageTarget_Callibration.SetHeight(0.01f*height);
                // Vuforia.enabled = true;
                // Debug.Log("ImageTarget Size W:"+ ImageTarget_Callibration.GetSize().x+",H:" 
                //     + ImageTarget_Callibration.GetSize().y);
                // StartCoroutine(CheckScan());
            }
            catch (Exception e)
            {
                Debug.LogError("StartCalibration error:" + e.Message);
            }
        }

        private IEnumerator CheckScan()
        {
            _isScanning = false;
            yield return new WaitForSeconds(2);
            _isScanning = true;
            yield return new WaitForSeconds(scanOutTime);
            if (_isScanning)
            {
                Debug.Log("Scan timeout");
                ShowMessageManager.Instance.ShowMessage("Scan timeout");
                StopScan();
            }
        }

        public void OnFinishedScan(string targetName)
        {
            if (!_isScanning) return;
            if (targetName == "Calibration")
            {
                PlaySoundManager.Instance.PlayEffect("FinishScan");
                Debug.Log("OnFinishedScan");
                // trackedPos = ImageTarget_Calibration.transform.position;
                // trackedRot = ImageTarget_Calibration.transform.rotation;
                StopScan();
                ConfirmCalibration();
            }
        }

        private void StopScan()
        {
            _isScanning = false;
            // Vuforia.enabled = false;
        }


        private void ConfirmCalibration()
        {
            var go = GameObject.Instantiate(calibrationCube);
            go.SetActive(true);
            // go.transform.localScale = new Vector3(ImageTarget_Callibration.GetSize().x,1, ImageTarget_Callibration.GetSize().y);
            go.transform.rotation = _trackedRot;
            go.transform.position = _trackedPos;
            Destroy(go, 10);

            var info = new BroadcastInfo
            {
                Type = (int)BroadcastType.ConfirmCallibration,
                UserId = ClientNetworkManager.Instance.UserId,
                PeerId = _serverClientId
            };
            SendDataManager.SendBroadcastById(info);
        }


        internal void ShowAniButtons(UpdateTransform updateTransform, Vector3 position, Quaternion rot)
        {
            _lastTime = Time.time;
            aniButtons.SetActive(true);
            aniButtons.transform.SetPositionAndRotation(position, rot);
            _selectedUpdateTransform = updateTransform;
        }

        public void PlayAniNext()
        {
            var info = new BroadcastInfo
            {
                Type = (int)BroadcastType.PlayAniNextByID,
                Data = BitConverter.GetBytes(_selectedUpdateTransform.updateID)
            };
            SendDataManager.SendBroadcastAll(info);
        }

        public void PlayAniLast()
        {
            var info = new BroadcastInfo
            {
                Type = (int)BroadcastType.PlyAniLastByID,
                Data = BitConverter.GetBytes(_selectedUpdateTransform.updateID)
            };
            SendDataManager.SendBroadcastAll(info);
        }

        public void Dock()
        {
            var info = new BroadcastInfo
            {
                Type = (int)BroadcastType.DockByID,
                Data = BitConverter.GetBytes(_selectedUpdateTransform.updateID)
            };
            SendDataManager.SendBroadcastAll(info);
        }

        public void Undock()
        {
            var info = new BroadcastInfo
            {
                Type = (int)BroadcastType.UndockByID,
                Data = BitConverter.GetBytes(_selectedUpdateTransform.updateID)
            };
            SendDataManager.SendBroadcastAll(info);
        }

        private void Update()
        {
            if (Time.time - _lastTime > 5)
            {
                aniButtons.SetActive(false);
            }
        }

        internal void OnCalibrationData(BroadcastInfo info)
        {
            var posX = BitConverter.ToSingle(info.Data, 0);
            var posY = BitConverter.ToSingle(info.Data, 4);
            var posZ = BitConverter.ToSingle(info.Data, 8);
            var rotX = BitConverter.ToSingle(info.Data, 12);
            var rotY = BitConverter.ToSingle(info.Data, 16);
            var rotZ = BitConverter.ToSingle(info.Data, 20);

            var screen = new GameObject();
            screen.transform.SetParent(anchorRoot);
            screen.transform.localEulerAngles = new Vector3(rotX, rotY, rotZ);
            screen.transform.localPosition = new Vector3(posX, posY, posZ);

            screen.transform.SetParent(null);
            anchorRoot.SetParent(screen.transform);
            screen.transform.SetPositionAndRotation(_trackedPos, _trackedRot);

            anchorRoot.SetParent(null);

            Destroy(screen, 1);
            Debug.Log("Calibration complete");
            ShowMessageManager.Instance.ShowMessage("Calibration complete");
        }

        internal void DownLoadAnchor()
        {
            SendDataManager.SendDownloadAnchor();
            ClientHandlerCenter.Instance.OnOnDownloadAnchorData = OnOnDownloadAnchorData;
        }

        private void OnOnDownloadAnchorData(byte[] data)
        {
            HoloLensAnchorManager.Instance.ImportAnchorData(data, anchorRoot.gameObject);
            ClientHandlerCenter.Instance.OnOnDownloadAnchorData = null;
        }

        internal void UploadAnchor()
        {
            // SendDataManager.Instance.SendUploadAnchor(new byte[1024]);
            HoloLensAnchorManager.Instance.ExprotAnchorData(anchorRoot.gameObject, SendDataManager.SendUploadAnchor);
        }

        internal static void SendCalibrationRequest(int id)
        {
        }

        internal static void OnConfirmCalibration(int id)
        {
        }

        public void CreateAssetByID(byte[] data)
        {
            int assetID = BitConverter.ToInt32(data, 0);
            long updateID = BitConverter.ToInt64(data, 4);

            float pox = BitConverter.ToSingle(data, 12);
            float poy = BitConverter.ToSingle(data, 16);
            float poz = BitConverter.ToSingle(data, 20);
            float rotx = BitConverter.ToSingle(data, 24);
            float roty = BitConverter.ToSingle(data, 28);
            float rotz = BitConverter.ToSingle(data, 32);

            GameObject asset = null;
            if (useLocalAsset)
            {
            }
            else
            {
                asset = AssetPanelManager.Instance.GetAsset(assetID);
            }

            if (asset != null)
            {
                GameObject go = Instantiate(asset, AssetPanelManager.Instance.anchorRoot);
                go.transform.localPosition = new Vector3(pox, poy, poz);
                go.transform.localEulerAngles = new Vector3(rotx, roty, rotz);
                go.AddComponent<UpdateTransform>().updateID = updateID;
                go.AddComponent<PlayAnimation>();
                go.AddComponent<Dockable>();

                var om = go.AddComponent<ObjectManipulator>();

                var cm = go.AddComponent<ConstraintManager>();


                var scale = go.AddComponent<MinMaxScaleConstraint>();
                scale.MaximumScale = new Vector3(scaleMaximum, scaleMaximum, scaleMaximum);
                scale.MinimumScale = new Vector3(scaleMinimum, scaleMinimum, scaleMinimum);

                scale.HandType = ManipulationHandFlags.TwoHanded;
                scale.ProximityType = ManipulationProximityFlags.Near | ManipulationProximityFlags.Far;
                om.MoveLerpTime = UpdateTransformManager.Instance.UpdateTime;
                om.RotateLerpTime = UpdateTransformManager.Instance.UpdateTime;
                om.ScaleLerpTime = UpdateTransformManager.Instance.UpdateTime;


                if (canChildMove)
                {
                    var children = go.transform.GetComponentsInChildren<Transform>(true); //true的意思是是否包含了隐藏的物体，如果子物体隐藏状态，也是添加这个组件的
                    for (int i = 0; i < children.Length; i++)
                    {
                        if (children[i].GetComponent<Collider>() != null)
                        {
                            children[i].gameObject.AddComponent<UpdateTransform>().updateID = updateID + i + 1;
                            // children[i].gameObject.AddComponent<Dockable>();

                            var om2 = children[i].gameObject.AddComponent<ObjectManipulator>();

                            var scale2 = children[i].gameObject.AddComponent<MinMaxScaleConstraint>();
                            scale2.MaximumScale = new Vector3(scaleMaximum, scaleMaximum, scaleMaximum);
                            scale2.MinimumScale = new Vector3(scaleMinimum, scaleMinimum, scaleMinimum);

                            om2.MoveLerpTime = UpdateTransformManager.Instance.UpdateTime;
                            om2.RotateLerpTime = UpdateTransformManager.Instance.UpdateTime;
                            om2.ScaleLerpTime = UpdateTransformManager.Instance.UpdateTime;
                        }
                    }
                }

                print("CreateAssetByID:" + assetID);
            }
            else
            {
                print("Non-existence of assets: " + assetID);
            }
        }

        public void SendHoloViewData(byte[] data)
        {
            var info = new BroadcastInfo
            {
                Type = (int)BroadcastType.HoloviewData,
                UserId = ClientNetworkManager.Instance.UserId,
                PeerId = _serverClientId,
                Data = data
            };
            SendDataManager.SendBroadcastById(info);
        }

        internal static void OnStartHoloView(BroadcastInfo info)
        {
            HoloCaptureManager.Instance.StartCapture();
        }

        internal static void OnHoloViewData(BroadcastInfo info)
        {
        }

        internal static void OnStopHoloView(BroadcastInfo info)
        {
            HoloCaptureManager.Instance.StopCapture();
        }
    }
}