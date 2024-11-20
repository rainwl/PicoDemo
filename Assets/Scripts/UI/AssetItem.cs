using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Common;
using UI;
using UnityEngine.Serialization;

public class AssetItem : MonoBehaviour
{
    public Image thumbImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public int abType;
    public int abId;
    public string abFileName;
    public string objName;

    internal void SetValue(AssetInfo assetInfo)
    {
        abType = assetInfo.AbType;
        abId = assetInfo.AbID;
        abFileName = assetInfo.AbFileName;
        objName = assetInfo.ObjName;

        nameText.text = assetInfo.TitleName;
        descText.text = assetInfo.Desc;
        LoadSprite(thumbImage, assetInfo.AbPicFileName);
    }

    private static void LoadSprite(Image image, string fileName)
    {
        var filePath = AssetBundleManager.Instance.LocalPicPath + "/" + fileName;
        if (File.Exists(filePath))
        {
            var data = File.ReadAllBytes(filePath);
            var t = new Texture2D(2, 2);
            if (t.LoadImage(data))
            {
                var sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
                image.sprite = sprite;
            }
            else
            {
                print("Image loading failure: " + fileName);
            }
        }
        else
        {
            print("The picture does not exist: " + fileName);
        }
    }
}