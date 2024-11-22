using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Common;
using MixedReality.Toolkit.UX;
using MrPlatform.Scripts.Network.Client;
using Network.Client;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class AssetPanelManager : MonoBehaviour
    {
        public GameObject assetItemPrefab;
        public Transform contentTransform;
        public PressableButton assetButton;
        public Transform anchorRoot;

        private readonly List<AssetInfo> _assetInfoList = new();
        public static AssetPanelManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        public void ReadLocalAbXml()
        {
            var fileName = AssetBundleManager.Instance.OssAbListXmlURL[(AssetBundleManager.Instance.OssAbListXmlURL.LastIndexOf('/') + 1)..];
            if (!File.Exists(AssetBundleManager.Instance.LocalResPath + "/" + fileName))
            {
                print(fileName + "The local file does not exist");
                return;
            }

            var xmlDoc = new XmlDocument();
            var info = new FileInfo(AssetBundleManager.Instance.LocalResPath + "/" + fileName);
            Stream stream = info.OpenRead();
            xmlDoc.Load(stream);
            stream.Close();
            stream.Dispose();

            var rootNode = xmlDoc.DocumentElement;
            if (rootNode != null)
            {
                var list = rootNode.ChildNodes;

                _assetInfoList.Clear();

                foreach (XmlNode item in list)
                {
                    var ab = item.ChildNodes;
                    var assetInfo = new AssetInfo();
                    foreach (XmlNode item2 in ab)
                    {
                        switch (item2.Name)
                        {
                            case "AbType":
                                assetInfo.AbType = int.Parse(item2.InnerText);
                                break;
                            case "AbID":
                                assetInfo.AbID = int.Parse(item2.InnerText);
                                break;
                            case "TitleName":
                                assetInfo.TitleName = item2.InnerText;
                                break;
                            case "Desc":
                                assetInfo.Desc = item2.InnerText;
                                break;
                            case "AbFileName":
                                assetInfo.AbFileName = item2.InnerText;
                                break;
                            case "ObjName":
                                assetInfo.ObjName = item2.InnerText;
                                break;
                            case "AbPicFileName":
                                assetInfo.AbPicFileName = item2.InnerText;
                                break;
                        }
                    }

                    _assetInfoList.Add(assetInfo);
                }
            }

            CreatAssetList(_assetInfoList);
        }

        private void CreatAssetList(List<AssetInfo> list)
        {
            for (var i = 0; i < contentTransform.childCount; i++)
            {
                Destroy(contentTransform.GetChild(i).gameObject);
            }

            // var group = contentTransform.GetComponent<ToggleGroup>();
            var pressableToggleGroup = contentTransform.GetComponent<PressableToggleGroup>();
            foreach (var assetInfo in list)
            {
                var go = Instantiate(assetItemPrefab, contentTransform);
                pressableToggleGroup.pressableToggles.Add(go.GetComponentInChildren<PressableButton>());
                // go.GetComponent<Toggle>().group = group;
                go.GetComponent<AssetItem>().SetValue(assetInfo);
            }
        }

        public GameObject GetAsset(int assetID)
        {
            var asset = _assetInfoList.Find(info => info.AbID == assetID);
            return asset != null ? AssetBundleManager.Instance.LoadAssetbundleFromFile(asset.AbFileName, asset.ObjName) : null;
        }

        public void LoadAsset()
        {
            // var group = contentTransform.GetComponent<ToggleGroup>();
            var pressableToggleGroup = contentTransform.GetComponent<PressableToggleGroup>();
            if (pressableToggleGroup.AnyPressableTogglesOn())
            {
                // var info = group.ActiveToggles().First().transform.GetComponent<AssetItem>();
                var info = pressableToggleGroup.ActivePressableToggles().First().transform.parent.GetComponent<AssetItem>();
                
                pressableToggleGroup.SetAllPressableTogglesOff();
                
                if (Camera.main != null)
                {
                    var position = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
                    var direction = Camera.main.transform.position - position;
                    direction.y = 0;

                    var go = new GameObject();
                    go.transform.SetParent(anchorRoot);
                    go.transform.SetPositionAndRotation(position, Quaternion.LookRotation(-direction));
                    Destroy(go, 0.1f);
                    // Notifies all clients to load the asset

                    print("LoadAsset");
                    var broadcastInfo = new BroadcastInfo
                    {
                        Type = (int)BroadcastType.CrteateAssetByID
                    };
                    var assetID = info.abId;
                    var updateID = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);

                    var data = new byte[36];
                    Array.Copy(BitConverter.GetBytes(assetID), 0, data, 0, 4);
                    Array.Copy(BitConverter.GetBytes(updateID), 0, data, 4, 8);

                    Array.Copy(BitConverter.GetBytes(go.transform.localPosition.x), 0, data, 12, 4);
                    Array.Copy(BitConverter.GetBytes(go.transform.localPosition.y), 0, data, 16, 4);
                    Array.Copy(BitConverter.GetBytes(go.transform.localPosition.z), 0, data, 20, 4);

                    Array.Copy(BitConverter.GetBytes(go.transform.localEulerAngles.x), 0, data, 24, 4);
                    Array.Copy(BitConverter.GetBytes(go.transform.localEulerAngles.y), 0, data, 28, 4);
                    Array.Copy(BitConverter.GetBytes(go.transform.localEulerAngles.z), 0, data, 32, 4);

                    broadcastInfo.Data = data;
                    print("broadcastInfo.Data = data;");
                    SendDataManager.SendBroadcastAll(broadcastInfo);
                    print("SendDataManager.Instance.SendBroadcastAll(broadcastInfo);");
                }

                assetButton.ForceSetToggled(false);
            }
            else
            {
                print("Please select a resource\\n!");
                // ShowMessageManager.Instance.ShowMessage("Please select a resource\n!");
            }
        }

        public void RemoveAllAsset()
        {
            // ShowMessageManager.Instance.ShowSelectBox("Whether to delete all resources?", () =>
            // {
                var info = new BroadcastInfo
                {
                    Type = (int)BroadcastType.RemoveAllAsset
                };
                SendDataManager.SendBroadcastAll(info);
                assetButton.ForceSetToggled(false);
                print("RemoveAllAsset");
            // });
        }

        public void RemoveAssetByID(long id)
        {
            var info = new BroadcastInfo
            {
                Type = (int)BroadcastType.RemoveAssetByID,
                Data = BitConverter.GetBytes(id)
            };
            SendDataManager.SendBroadcastAll(info);
        }
    }

    public class AssetInfo
    {
        public int AbType;
        public int AbID;
        public string TitleName;
        public string Desc;
        public string AbFileName;
        public string ObjName;
        public string AbPicFileName;
    }
}