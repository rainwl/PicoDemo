using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Dock
{
    public class AssetLocalManager : MonoBehaviour
    {
        public List<AssetInfoLocal> AssetInfoLocalList = new List<AssetInfoLocal>();

        public static AssetLocalManager Instance { get; internal set; }
        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            if (!ARManager.Instance.UseLocalAsset) return;

            CreatAssetList();
        }

        void CreatAssetList()
        {
            for (int i = 0; i < AssetPanelManager.Instance.Content.childCount; i++)
            {
                Destroy(AssetPanelManager.Instance.Content.GetChild(i).gameObject);
            }
            ToggleGroup group = AssetPanelManager.Instance.Content.GetComponent<ToggleGroup>();
            for (int i = 0; i < AssetInfoLocalList.Count; i++)
            {
                GameObject go = Instantiate(AssetPanelManager.Instance.AssetItemPrefab, AssetPanelManager.Instance.Content);
                go.GetComponent<Toggle>().group = group;

                AssetInfo assetinfo = new AssetInfo();
                assetinfo.AbType = AssetInfoLocalList[i].AbType;
                assetinfo.AbID = AssetInfoLocalList[i].AbID;
                assetinfo.TitleName = AssetInfoLocalList[i].TitleName;
                assetinfo.Desc = AssetInfoLocalList[i].Desc;

                go.GetComponent<AssetItem>().SetValue(assetinfo);
                go.GetComponent<AssetItem>().ThumbImage.sprite = AssetInfoLocalList[i].AbPicSprite;
            }
        }
        public GameObject GetAsset(int assetID)
        {
            AssetInfoLocal asset = AssetInfoLocalList.Find((AssetInfoLocal info) => { return info.AbID == assetID; });
            if (asset != null)
            {
                return asset.Prefab;
            }
            else
            {
                return null;
            }
        }
    }
    [Serializable]
    public class AssetInfoLocal
    {
        public int AbType;
        public int AbID;
        public string TitleName;
        public string Desc;
        public GameObject Prefab;
        public Sprite AbPicSprite;
    }
}
