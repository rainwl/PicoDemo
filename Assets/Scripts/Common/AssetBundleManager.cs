using System.Collections;
using System.IO;
using System.Xml;
using UI;
using UnityEngine;
using UnityEngine.Networking;

namespace Common
{
    public class AssetBundleManager : MonoBehaviour
    {
        public static AssetBundleManager Instance;

        public string OssResXmlURL = "https://rainmr.oss-cn-beijing.aliyuncs.com/ResourceSite.xml";
        public string OssAbListXmlURL = "";
        public string OssAbPath;
        public string OssPicPath;

        public string LocalResPath;
        public string LocalAssetBundlePath;
        public string LocalPicPath;

        XmlDocument xmlDoc = new XmlDocument();
        private void Awake()
        {
            Instance = this;
            if (Application.isEditor)
            {
                string path = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
                LocalResPath = path + "/MrRes";
                LocalAssetBundlePath = path + "/MrRes/AB";
                LocalPicPath = path + "/MrRes/Pic";
            }
            else
            {
                LocalResPath = Application.persistentDataPath + "/MrRes";
                LocalAssetBundlePath = Application.persistentDataPath + "/MrRes/AB";
                LocalPicPath = Application.persistentDataPath + "/MrRes/Pic";
            }
            if (!Directory.Exists(LocalResPath)) Directory.CreateDirectory(LocalResPath);
            if (!Directory.Exists(LocalAssetBundlePath)) Directory.CreateDirectory(LocalAssetBundlePath);
            if (!Directory.Exists(LocalPicPath)) Directory.CreateDirectory(LocalPicPath);
        }

        private void Start()
        {
            if (ARManager.Instance.UseLocalAsset) return;
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                //读取本地资源
                ReadLocalResXml();
                ReadLocalAbListXml();
            }
            else
            {
                //检查OSS资源更新
                StartCoroutine(CheckResUpdate());
            }
        }

        IEnumerator CheckResUpdate()
        {
            UnityWebRequest www = new UnityWebRequest(OssResXmlURL);
            DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
            www.downloadHandler = handler;
            www.timeout = 2;
            yield return www.SendWebRequest();

            if (!string.IsNullOrEmpty(www.error))
            {
                print("www ResXml error:" + www.error);
                //读取本地文件
                ReadLocalResXml();
                ReadLocalAbListXml();
            }
            else
            {
                //保存最新的ResXml文件到本地
                string resXmlFileName = OssResXmlURL.Substring(OssResXmlURL.LastIndexOf('/') + 1);
                SaveFile(LocalResPath, resXmlFileName, www.downloadHandler.data);
                //读取ResXml
                ReadLocalResXml();
                //下载最新的AbListXml文件到本地
                www = new UnityWebRequest(OssAbListXmlURL);
                DownloadHandlerBuffer handler2 = new DownloadHandlerBuffer();
                www.downloadHandler = handler2;
                yield return www.SendWebRequest();

                if (!string.IsNullOrEmpty(www.error))
                {
                    print("www AblistXml error:" + www.error);
                }
                else
                {
                    //保存最新的AbListXml文件到本地 
                    string abXmlFileName = OssAbListXmlURL.Substring(OssAbListXmlURL.LastIndexOf('/') + 1);
                    SaveFile(LocalResPath, abXmlFileName, www.downloadHandler.data);
                }
                //读取ResXml
                ReadLocalAbListXml();
            }
        }
        void ReadLocalResXml()
        {
            string fileName = OssResXmlURL.Substring(OssResXmlURL.LastIndexOf('/') + 1);
            if (!File.Exists(LocalResPath + "/" + fileName))
            {
                print(fileName + " The local file does not exist");
                return;
            }
            Stream stream;
            FileInfo info = new FileInfo(LocalResPath + "/" + fileName);
            stream = info.OpenRead();
            xmlDoc.Load(stream);
            stream.Close();
            stream.Dispose();

            XmlElement rootNode = xmlDoc.DocumentElement;
            XmlNodeList list = rootNode.ChildNodes;
            foreach (XmlNode item in list)
            {
                string platform = "";
                if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    platform = "Windows";
                }
                else if (Application.platform == RuntimePlatform.WSAPlayerARM || Application.platform == RuntimePlatform.WSAPlayerX86)
                {
                    platform = "WSA";
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    platform = "iOS";
                }
                else if (Application.platform == RuntimePlatform.Android)
                {
                    platform = "Android";
                }
                XmlNodeList list2 = item.ChildNodes;
                if (list2.Item(0).InnerText == platform)
                {
                    foreach (XmlNode item2 in list2)
                    {
                        if (item2.Name == "AbPath")
                        {
                            OssAbPath = item2.InnerText;
                        }
                        else if (item2.Name == "PicPath")
                        {
                            OssPicPath = item2.InnerText;
                        }
                        else if (item2.Name == "ABList")
                        {
                            OssAbListXmlURL = item2.InnerText;
                        }
                    }
                }
            }
        }
        void ReadLocalAbListXml()
        {
            string fileName = OssAbListXmlURL.Substring(OssAbListXmlURL.LastIndexOf('/') + 1);
            if (!File.Exists(LocalResPath + "/" + fileName))
            {
                print(fileName + "The local file does not exist");
                return;
            }
            Stream stream;
            FileInfo info = new FileInfo(LocalResPath + "/" + fileName);
            stream = info.OpenRead();
            xmlDoc.Load(stream);
            stream.Close();
            stream.Dispose();

            XmlElement rootNode = xmlDoc.DocumentElement;
            XmlNodeList list = rootNode.ChildNodes;
            foreach (XmlNode item in list)
            {
                XmlNodeList abList = item.ChildNodes;
                foreach (XmlNode ab in abList)
                {
                    if (ab.Name == "AbFileName")
                    {
                        if (!File.Exists(LocalAssetBundlePath + ab.InnerText))
                        {
                            StartCoroutine(DownloadResource(OssAbPath+ ab.InnerText,LocalAssetBundlePath));
                        }
                    }
                    else if (ab.Name == "AbPicFileName")
                    {
                        if (!File.Exists(LocalPicPath + "/" + ab.InnerText))
                        {
                            StartCoroutine(DownloadResource(OssPicPath + "/" + ab.InnerText,LocalPicPath));
                        }
                    }
                }
            }
            print("Resource read complete");
            AssetPanelManager.Instance.ReadLocalAbXml();
        }
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        public void SaveFile(string path, string fileName, byte[] data)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            FileStream fileStream = new FileStream(path + "/" + fileName, FileMode.Create, FileAccess.Write);
            fileStream.Write(data, 0, data.Length);
            fileStream.Close();
            fileStream.Dispose();
            print(fileName + " download completes");
        }

        /// <summary>
        /// 下载并保存文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="savePath"></param>
        /// <returns></returns>
        IEnumerator DownloadResource(string url, string savePath)
        {
            UnityWebRequest www = new UnityWebRequest(url);
            DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
            www.downloadHandler = handler;
            yield return www.SendWebRequest();

            string fileName = url.Substring(url.LastIndexOf('/')+1);
            if (!string.IsNullOrEmpty(www.error))
            {
                print(fileName + " download error:" + www.error + " " + url);
            }
            else 
            {
                SaveFile(savePath, fileName,www.downloadHandler.data);
            }
        }

        /// <summary>
        /// 从本地加载ab包
        /// </summary>
        /// <param name="abFileName"></param>
        /// <param name="objName"></param>
        /// <returns></returns>
        public GameObject LoadAssetbundleFromFile(string abFileName,string objName) 
        {
            if (File.Exists(LocalAssetBundlePath + "/" + abFileName))
            {
                FileStream fileStream = new FileStream(LocalAssetBundlePath + "/" + abFileName,FileMode.Open,FileAccess.Read);
                byte[] data = new byte[fileStream.Length];
                fileStream.Read(data,0,data.Length);
                AssetBundle ab = AssetBundle.LoadFromMemory(data);
                fileStream.Close();
                fileStream.Dispose();
                GameObject go = ab.LoadAsset<GameObject>(objName);
                ab.Unload(false);
                return go;
            }
            else 
            {
                return null;
            }
        }
    }
}
