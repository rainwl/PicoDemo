using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Common
{
    public class ShowMessageManager : MonoBehaviour
    {
        public static ShowMessageManager Instance;

        public GameObject messagePrefab;
        public Transform messageLayout;

        public GameObject selectBox;
        public TextMeshProUGUI warnText;

        private Action _yesAction;
        private Action _noAction;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            //if (Input.GetKeyUp(KeyCode.M))
            //{
            //    ShowMessage("0123456789abc".Substring(UnityEngine.Random.Range(0,10)));
            //}
            //if (Input.GetKeyUp(KeyCode.B))
            //{
            //    ShowSelectBox("是否删除?",null,null);
            //}
        }

        public void ShowMessage(string mes, float time = 2)
        {
            var go = Instantiate(messagePrefab, messageLayout);
            go.SetActive(true);
            var text = go.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            text.text = mes;
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(text.preferredWidth + 10, rect.sizeDelta.y);
            Destroy(go, time);
        }

        public void ShowSelectBox(string warn, Action yes, Action no = null)
        {
            selectBox.SetActive(true);
            warnText.text = warn;
            _yesAction = yes;
            _noAction = no;
        }

        public void Yes()
        {
            _yesAction?.Invoke();
            warnText.text = "";
            selectBox.SetActive(false);
        }

        public void No()
        {
            _noAction?.Invoke();
            warnText.text = "";
            selectBox.SetActive(false);
        }
    }
}