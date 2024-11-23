using System;
using System.Collections.Generic;
using Network.Client;
using UnityEngine;

namespace Common
{
    public class UpdateTransformManager : MonoBehaviour
    {
        public static UpdateTransformManager Instance;
        public List<UpdateTransform> TransformItmeList = new List<UpdateTransform>();
        public float Threshold = 0.01f;
        public float UpdateTime = 0.05f;
        float lastTime;
        private void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (Time.time - lastTime < UpdateTime) return;
            lastTime = Time.time;

            List<byte> data = new List<byte>();
            for (int i = 0; i < TransformItmeList.Count; i++)
            {
                if (TransformItmeList[i].NeedUpdate()) 
                {
                    data.AddRange(BitConverter.GetBytes(TransformItmeList[i].updateID));
                    data.AddRange(BitConverter.GetBytes(TransformItmeList[i].lastPosition.x));
                    data.AddRange(BitConverter.GetBytes(TransformItmeList[i].lastPosition.y));
                    data.AddRange(BitConverter.GetBytes(TransformItmeList[i].lastPosition.z));

                    data.AddRange(BitConverter.GetBytes(TransformItmeList[i].lastEulerAngles.x));
                    data.AddRange(BitConverter.GetBytes(TransformItmeList[i].lastEulerAngles.y));
                    data.AddRange(BitConverter.GetBytes(TransformItmeList[i].lastEulerAngles.z));

                    data.AddRange(BitConverter.GetBytes(TransformItmeList[i].lastScale.x));
                    data.AddRange(BitConverter.GetBytes(TransformItmeList[i].lastScale.y));
                    data.AddRange(BitConverter.GetBytes(TransformItmeList[i].lastScale.z));
                }
            }
            if (data.Count > 0) 
            {
                BroadcastInfo info = new BroadcastInfo();
                info.Type = (int)BroadcastType.UpdateTransform;
                info.Data = data.ToArray();
                SendDataManager.SendBroadcastAll(info);
            }
        }

        internal void UpdateTransform(byte[] data)
        {
            int count = data.Length / 44;
            for (int i = 0; i < count; i++)
            {
                long updateID = BitConverter.ToInt64(data, i * 44);
                float px = BitConverter.ToSingle(data, 8 + i * 44);
                float py = BitConverter.ToSingle(data, 12 + i * 44);
                float pz = BitConverter.ToSingle(data, 16 + i * 44);

                float rx = BitConverter.ToSingle(data, 20 + i * 44);
                float ry = BitConverter.ToSingle(data, 24 + i * 44);
                float rz = BitConverter.ToSingle(data, 28 + i * 44);

                float sx = BitConverter.ToSingle(data, 32 + i * 44);
                float sy = BitConverter.ToSingle(data, 36 + i * 44);
                float sz = BitConverter.ToSingle(data, 40 + i * 44);

                UpdateTransform ut = TransformItmeList.Find((UpdateTransform t)=> { return t.updateID == updateID; });
                if (ut != null) ut.UpdateData(px,py,pz,rx,ry,rz,sx,sy,sz);
            }
        }
    }
}
