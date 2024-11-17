using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Xml;
using MrPlatform.Scripts.Common;
using MrPlatform.Scripts.Network.Client;
using UnityEngine;
using UnityEngine.UI;

public class AssetPanelManager : MonoBehaviour
{
    public static AssetPanelManager Instance;
    public GameObject AssetItemPrefab;
    public Transform Content;
    public Toggle AssetTog;
    public List<AssetInfo> AssetInfoList = new List<AssetInfo>();
    private void Awake()
    {
        Instance = this;
    }
    /// <summary>
    /// 加载abXml生产资产列表
    /// </summary>
    public void ReadLocalAbXml()
    {
        string fileName = AssetBundleManager.Instance.OssAbListXmlURL.Substring(AssetBundleManager.Instance.OssAbListXmlURL.LastIndexOf('/') + 1);
        if (!File.Exists(AssetBundleManager.Instance.LocalResPath + "/" + fileName))
        {
            print(fileName + "--本地文件不存在!");
            return;
        }

        XmlDocument xmlDoc = new XmlDocument();
        Stream stream;
        FileInfo info = new FileInfo(AssetBundleManager.Instance.LocalResPath + "/" + fileName);
        stream = info.OpenRead();
        xmlDoc.Load(stream);
        stream.Close();
        stream.Dispose();

        XmlElement rootNode = xmlDoc.DocumentElement;
        XmlNodeList list = rootNode.ChildNodes;

        AssetInfoList.Clear();

        foreach (XmlNode item in list)
        {
            XmlNodeList ab = item.ChildNodes;
            AssetInfo assetInfo = new AssetInfo(); 
            foreach (XmlNode item2 in ab)
            {
                if (item2.Name == "AbType")
                {
                    assetInfo.AbType = int.Parse(item2.InnerText);
                }
                else if (item2.Name == "AbID")
                {
                    assetInfo.AbID = int.Parse(item2.InnerText);
                }
                else if (item2.Name == "TitleName")
                {
                    assetInfo.TitleName = item2.InnerText;
                }
                else if (item2.Name == "Desc") 
                {
                    assetInfo.Desc = item2.InnerText;
                }
                else if (item2.Name == "AbFileName")
                {
                    assetInfo.AbFileName = item2.InnerText;
                }
                else if (item2.Name == "ObjName") 
                {
                    assetInfo.ObjName = item2.InnerText;
                }
                else if (item2.Name == "AbPicFileName")
                {
                    assetInfo.AbPicFileName = item2.InnerText; 
                }
            }
            AssetInfoList.Add(assetInfo);
        }
        CreatAssetList(AssetInfoList);
    }

    void CreatAssetList(List<AssetInfo>list) 
    {
        for (int i = 0; i < Content.childCount; i++)
        {
            Destroy(Content.GetChild(i).gameObject);
        }
        ToggleGroup group = Content.GetComponent<ToggleGroup>();
        for (int i = 0; i < list.Count; i++)
        {
            GameObject go = Instantiate(AssetItemPrefab,Content);
            go.GetComponent<Toggle>().group = group;
            go.GetComponent<AssetItem>().SetValue(list[i]);
        }
    }
    public GameObject GetAsset(int assetID) 
    {
        AssetInfo asset= AssetInfoList.Find((AssetInfo info) => { return info.AbID == assetID; });
        if (asset != null)
        {
           return AssetBundleManager.Instance.LoadAssetbundleFromFile(asset.AbFileName,asset.ObjName);
        }
        else 
        {
            return null;
        }
    }
    public Transform AnchorRoot;
    public void LoadAsset() 
    {
        ToggleGroup group = Content.GetComponent<ToggleGroup>();
        if (group.AnyTogglesOn())
        {
            AssetItem info= group.ActiveToggles().First().transform.GetComponent<AssetItem>();
            // todo 换成按钮的,不用toggle了
            group.SetAllTogglesOff();

            Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
            Vector3 direction = Camera.main.transform.position - position;
            direction.y = 0;

            GameObject go = new GameObject();
            go.transform.SetParent(AnchorRoot);
            go.transform.position = position;
            go.transform.rotation = Quaternion.LookRotation(-direction);
            Destroy(go,0.1f);
            //通知所有客户端加载该资产
            BroadcastInfo broadcastInfo = new BroadcastInfo();
            broadcastInfo.Type = (int)BroadcastType.CrteateAssetByID;
            int assetID = info.AbId;
            long updateID= BitConverter.ToInt64(Guid.NewGuid().ToByteArray(),0);

            byte[] data = new byte[36];
            Array.Copy(BitConverter.GetBytes(assetID),0, data,0,4);
            Array.Copy(BitConverter.GetBytes(updateID), 0, data, 4, 8);

            Array.Copy(BitConverter.GetBytes(go.transform.localPosition.x), 0, data, 12, 4);
            Array.Copy(BitConverter.GetBytes(go.transform.localPosition.y), 0, data, 16, 4);
            Array.Copy(BitConverter.GetBytes(go.transform.localPosition.z), 0, data, 20, 4);

            Array.Copy(BitConverter.GetBytes(go.transform.localEulerAngles.x), 0, data, 24, 4);
            Array.Copy(BitConverter.GetBytes(go.transform.localEulerAngles.y), 0, data, 28, 4);
            Array.Copy(BitConverter.GetBytes(go.transform.localEulerAngles.z), 0, data, 32, 4);

            broadcastInfo.Data = data;
            SendDataManager.Instance.SendBroadcastAll(broadcastInfo);
            AssetTog.isOn = false;
        }
        else 
        {
            //print("请选择一个资源!");
            ShowMessageManager.Instance.ShowMessage("请选择一个资源!");
        }
    }

    public void RemoveAllAsset() 
    {
        ShowMessageManager.Instance.ShowSelectBox("是否删除所有资源?", () =>
        {
            BroadcastInfo info = new BroadcastInfo();
            info.Type = (int)BroadcastType.RemoveAllAsset;
            SendDataManager.Instance.SendBroadcastAll(info);
            AssetTog.isOn = false;

        });
    }

    public void RemoveAssetByID(long id) 
    {
        BroadcastInfo info = new BroadcastInfo();
        info.Type = (int)BroadcastType.RemoveAssetByID;
        info.Data = BitConverter.GetBytes(id);
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
