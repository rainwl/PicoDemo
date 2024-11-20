using Common;
using MrPlatform.Scripts.Network.Client;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class ClientItem : MonoBehaviour
    {
        public GameObject subPanel;
        public int type;
        public int id;
        public TextMeshProUGUI userText;
        public TextMeshProUGUI ipText;
        public Image typeIcon;
        public Sprite[] typeSprites;
        private RuntimePlatform _platform;

        public void SetValue(int id, int type, string name, string ip)
        {
            this.id = id;
            this.type = type;
            int spriteIndex = 0;
            _platform = (RuntimePlatform)type;
            if (_platform == RuntimePlatform.OSXEditor
                || _platform == RuntimePlatform.WindowsEditor
                || _platform == RuntimePlatform.WindowsPlayer
                || _platform == RuntimePlatform.OSXPlayer)
            {
                spriteIndex = 1;
            }
            else if (_platform == RuntimePlatform.IPhonePlayer)
            {
                spriteIndex = 2;
            }
            else if (_platform == RuntimePlatform.Android)
            {
                spriteIndex = 3;
            }
            else if (_platform == RuntimePlatform.WSAPlayerX86)
            {
                spriteIndex = 4;
            }
            else if (_platform == RuntimePlatform.WSAPlayerARM)
            {
                spriteIndex = 5;
            }

            userText.text = name;
            ipText.text = ip;
            typeIcon.sprite = typeSprites[spriteIndex];
        }

        private void OnEnable()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.GetComponent<RectTransform>());
        }

        public void OnClick()
        {
            subPanel.SetActive(!subPanel.activeInHierarchy);
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        }

        public void UploadAnchor()
        {
            if (_platform == RuntimePlatform.WSAPlayerX86 || _platform == RuntimePlatform.WSAPlayerARM)
            {
                ARManager.Instance.UploadAnchor();
            }
            else
            {
                ShowMessageManager.Instance.ShowMessage("Please select HMD device!");
            }
        }

        public void DownLoadAnchor()
        {
            if (_platform == RuntimePlatform.WSAPlayerX86 || _platform == RuntimePlatform.WSAPlayerARM)
            {
                ARManager.Instance.DownLoadAnchor();
            }
            else
            {
                ShowMessageManager.Instance.ShowMessage("Please select HMD device!");
            }
        }

        public void Calibration()
        {
            if (id == ClientNetworkManager.Instance.UserId || ServerNetworkManager.Instance == null || !ServerNetworkManager.Instance.IsStartServer)
            {
                ShowMessageManager.Instance.ShowMessage("Permission denied!");
                return;
            }

            ShowMessageManager.Instance.ShowSelectBox("Calibration client?", () => { ARManager.Instance.SendCallibrationRequest(id); });
        }
    }
}