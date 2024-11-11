using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System.IO;
using UnityEngine.UI;

public class LoggerManager : MonoBehaviour
{
    public TextMeshProUGUI LogText;
    public bool LogEnable;
    public bool OnlyError;
    public int LogCount=20;
    RectTransform Rect;
    string logFilePath;
    public Toggle LogEnableTog;
    public Toggle OnlyErrorTog; 
    void Awake()
    {
        logFilePath = Application.persistentDataPath + "/" + DateTime.Now.ToString("yyyy-MM-dd")+".txt";
        if (!File.Exists(logFilePath)) 
        {
            File.Create(logFilePath).Close();
        }
        LogText.text = "";
        Rect = LogText.GetComponent<RectTransform>();
        Application.logMessageReceived += logMessageReceived;
        if (PlayerPrefs.HasKey("LogEnableKey")) 
        {
            LogEnable = PlayerPrefs.GetInt("LogEnableKey")==1;
            LogEnableTog.isOn = LogEnable;
        }
        if (PlayerPrefs.HasKey("OnlyErrorKey"))
        {
            OnlyError = PlayerPrefs.GetInt("OnlyErrorKey") == 1;
            OnlyErrorTog.isOn = OnlyError;
        }
        Debug.unityLogger.logEnabled = LogEnable;
        Debug.Log("==================");
        Debug.Log(logFilePath);
    }
    List<string> logList = new List<string>();
    private void logMessageReceived(string condition, string stackTrace, LogType type)
    {
        //LogText.text += condition + "," + type + "/n";
        string logTime = "<color #00FF00>" + DateTime.Now.ToString("MM-dd HH:mm:ss") + "</color>" + "\n";
        string log="";
        switch (type)
        {
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception:
                logTime = "<color #FF0000>" + DateTime.Now.ToString("MM-dd HH:mm:ss")+ "</color>" + "\n";
                if (string.IsNullOrEmpty(stackTrace))stackTrace = Environment.StackTrace;
                log = string.Format("{0}\n{1}\n",condition,stackTrace);
                break;
            case LogType.Warning:
                return;
            case LogType.Log:
                if (OnlyError) return;
                log = condition+"\n";
                break;
            default:
                break;
        }
        logList.Add(logTime+log);
        if (logList.Count >= LogCount)
        {
            string str = string.Concat(logList.ToArray());
            LogText.text = str;
            WriteLog(str);
            logList.Clear();
        }
        else
        {
            LogText.text += logTime + log;
        }

        if (LogText != null) 
        {
            Rect.sizeDelta = new Vector2(Rect.sizeDelta.x, LogText.preferredHeight + 50);
            Rect.anchoredPosition = new Vector2(Rect.anchoredPosition.x, Rect.sizeDelta.y);
        }
    }
    public void OnLogTogOn(Toggle tog) 
    {
        if (tog.isOn) 
        {
            Rect.sizeDelta = new Vector2(Rect.sizeDelta.x, LogText.preferredHeight + 50);
            Rect.anchoredPosition = new Vector2(Rect.anchoredPosition.x, Rect.sizeDelta.y);
        }
    }
    public void OnLogEnableTog(Toggle tog) 
    {
        LogEnable = tog.isOn;
        Debug.unityLogger.logEnabled = LogEnable;
        PlayerPrefs.SetInt("LogEnableKey", LogEnable?1:0);
        PlayerPrefs.Save();
    }
    public void OnOnlyErrorTog(Toggle tog)
    {
        OnlyError = tog.isOn;
        PlayerPrefs.SetInt("OnlyErrorKey", OnlyError ? 1 : 0);
        PlayerPrefs.Save();
    }
    private void OnDestroy()
    {
        if (logList.Count > 0) 
        {
            WriteLog(string.Concat(logList.ToArray()));
        }
    }
    void WriteLog(string str) 
    {
        if (File.Exists(logFilePath)) 
        {
            FileStream fs = File.Open(logFilePath,FileMode.Append);
            using (StreamWriter writer=new StreamWriter (fs)) 
            {
                writer.WriteLine(str);
            }
            fs.Close();
        }
    }
}
