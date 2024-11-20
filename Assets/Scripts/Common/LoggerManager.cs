using System;
using System.Collections.Generic;
using System.IO;
using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Common
{
    public class LoggerManager : MonoBehaviour
    {
        public TextMeshProUGUI logText;
        public bool logEnable;
        public bool onlyError;
        public int logCount = 20;
        public PressableButton logEnableButton;
        public PressableButton onlyErrorButton;

        private RectTransform _rect;
        private string _logFilePath;
        private readonly List<string> _logList = new();

        private void Awake()
        {
            _logFilePath = Application.persistentDataPath + "/" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            if (!File.Exists(_logFilePath))
            {
                File.Create(_logFilePath).Close();
            }

            logText.text = "";
            _rect = logText.GetComponent<RectTransform>();
            Application.logMessageReceived += LOGMessageReceived;
            if (PlayerPrefs.HasKey("LogEnableKey"))
            {
                logEnable = PlayerPrefs.GetInt("LogEnableKey") == 1;
                logEnableButton.ForceSetToggled(logEnable);
            }

            if (PlayerPrefs.HasKey("OnlyErrorKey"))
            {
                onlyError = PlayerPrefs.GetInt("OnlyErrorKey") == 1;
                onlyErrorButton.ForceSetToggled(onlyError);
            }

            Debug.unityLogger.logEnabled = logEnable;
            Debug.Log("==================");
            Debug.Log(_logFilePath);
        }

        private void LOGMessageReceived(string condition, string stackTrace, LogType type)
        {
            //LogText.text += condition + "," + type + "/n";
            var logTime = "<color #00FF00>" + DateTime.Now.ToString("MM-dd HH:mm:ss") + "</color>" + "\n";
            var log = "";
            switch (type)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    logTime = "<color #FF0000>" + DateTime.Now.ToString("MM-dd HH:mm:ss") + "</color>" + "\n";
                    if (string.IsNullOrEmpty(stackTrace)) stackTrace = Environment.StackTrace;
                    log = string.Format("{0}\n{1}\n", condition, stackTrace);
                    break;
                case LogType.Warning:
                    return;
                case LogType.Log:
                    if (onlyError) return;
                    log = condition + "\n";
                    break;
                default:
                    break;
            }

            _logList.Add(logTime + log);
            if (_logList.Count >= logCount)
            {
                string str = string.Concat(_logList.ToArray());
                logText.text = str;
                WriteLog(str);
                _logList.Clear();
            }
            else
            {
                logText.text += logTime + log;
            }

            if (logText != null)
            {
                // _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, logText.preferredHeight + 50);
                // _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, _rect.sizeDelta.y);
            }
        }

        public void OnLogTogOn(PressableButton tog)
        {
            if (tog.isSelected)
            {
                _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, logText.preferredHeight + 50);
                _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, _rect.sizeDelta.y);
            }
        }

        public void OnLogEnableTog(PressableButton tog)
        {
            logEnable = tog.IsToggled;
            Debug.unityLogger.logEnabled = logEnable;
            PlayerPrefs.SetInt("LogEnableKey", logEnable ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void OnOnlyErrorTog(PressableButton tog)
        {
            onlyError = tog.IsToggled;
            PlayerPrefs.SetInt("OnlyErrorKey", onlyError ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void OnDestroy()
        {
            if (_logList.Count > 0)
            {
                WriteLog(string.Concat(_logList.ToArray()));
            }
        }

        private void WriteLog(string str)
        {
            if (File.Exists(_logFilePath))
            {
                var fs = File.Open(_logFilePath, FileMode.Append);
                using (var writer = new StreamWriter(fs))
                {
                    writer.WriteLine(str);
                }

                fs.Close();
            }
        }
    }
}