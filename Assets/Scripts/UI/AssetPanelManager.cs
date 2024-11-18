﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using MrPlatform.Scripts.Common;
using MrPlatform.Scripts.Network.Client;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AssetPanelManager : MonoBehaviour
    {
        public static AssetPanelManager Instance;
        public GameObject assetItemPrefab;
        public Transform content;
        public Toggle assetTog;
        public Transform anchorRoot;
        private readonly List<AssetInfo> _assetInfoList = new();

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
            for (var i = 0; i < content.childCount; i++)
            {
                Destroy(content.GetChild(i).gameObject);
            }

            var group = content.GetComponent<ToggleGroup>();
            foreach (var assetInfo in list)
            {
                var go = Instantiate(assetItemPrefab, content);
                go.GetComponent<Toggle>().group = group;
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
            var group = content.GetComponent<ToggleGroup>();
            if (group.AnyTogglesOn())
            {
                var info = group.ActiveToggles().First().transform.GetComponent<AssetItem>();
                // todo 换成按钮的,不用toggle了
                group.SetAllTogglesOff();

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

                    var broadcastInfo = new BroadcastInfo
                    {
                        Type = (int)BroadcastType.CrteateAssetByID
                    };
                    var assetID = info.AbId;
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
                    SendDataManager.Instance.SendBroadcastAll(broadcastInfo);
                }

                assetTog.isOn = false;
            }
            else
            {
                ShowMessageManager.Instance.ShowMessage("Please select a resource\n!");
            }
        }

        public void RemoveAllAsset()
        {
            ShowMessageManager.Instance.ShowSelectBox("Whether to delete all resources?", () =>
            {
                var info = new BroadcastInfo
                {
                    Type = (int)BroadcastType.RemoveAllAsset
                };
                SendDataManager.Instance.SendBroadcastAll(info);
                assetTog.isOn = false;
            });
        }

        public void RemoveAssetByID(long id)
        {
            var info = new BroadcastInfo
            {
                Type = (int)BroadcastType.RemoveAssetByID,
                Data = BitConverter.GetBytes(id)
            };
            SendDataManager.Instance.SendBroadcastAll(info);
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