//
// Almost all of this code is derived from uOSCClient.cs
//

using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

using uOSC;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;

namespace SakuraScript.VBTTool
{
    public class VBTOpenTrackUDPClient : MonoBehaviour
    {
        [SerializeField]
        Transform _headObject;
        public Transform HeadObject {
            get => _headObject; 
            set => _headObject = value;
        }

        [SerializeField]
        public string address = "127.0.0.1";

        [SerializeField]
        public int port = 4242;

        [SerializeField]
        public int maxQueueSize = 100;

        [SerializeField, Tooltip("milliseconds")]
        public float dataTransimissionInterval = 0f;

        private class PosRot {
            public Vector3 position;
            public Quaternion rotation;
        }

    #if NETFX_CORE
        Udp udp_ = new uOSC.Uwp.Udp();
        Thread thread_ = new uOSC.Uwp.Thread();
    #else
        Udp udp_ = new uOSC.DotNet.Udp();
        Thread thread_ = new uOSC.DotNet.Thread();
    #endif
        Queue<PosRot> messages_ = new Queue<PosRot>();
        object lockObject_ = new object();

        public ClientStartEvent onClientStarted = new ClientStartEvent();
        public ClientStopEvent onClientStopped = new ClientStopEvent();

        string address_ = "";
        int port_ = 0;

        public bool isRunning
        {
            get { return udp_.isRunning; }
        }

        void OnEnable()
        {
            StartClient();
        }

        void OnDisable()
        {
            StopClient();
        }

        public void StartClient()
        {
            udp_.StartClient(address, port);
            thread_.Start(UpdateSend);
            address_ = address;
            port_ = port;
            onClientStarted.Invoke(address, port);
        }

        public void StopClient()
        {
            thread_.Stop();
            udp_.Stop();
            onClientStopped.Invoke(address, port);
        }

        void Update()
        {
            UpdateChangePortAndAddress();
            if (_headObject != null) Add(_headObject);
        }

        void UpdateChangePortAndAddress()
        {
            if (port_ == port && address_ == address) return;

            StopClient();
            StartClient();
        }

        void CopyDoubleToByteArray( double src, byte [] dest, int destPos )
        {
            byte [] byteBuf =  BitConverter.GetBytes(src);
            Buffer.BlockCopy(byteBuf, 0, dest, destPos, byteBuf.Length);
        }

        void UpdateSend()
        {
            while (messages_.Count > 0)
            {
                var sw = Stopwatch.StartNew();

                PosRot message;
                lock (lockObject_)
                {
                    message = messages_.Dequeue();
                }

                const int doubleSize = 8;
                const int otUdpLen = doubleSize * 6;
                byte [] byteArray = new byte [otUdpLen];
                CopyDoubleToByteArray((double)message.position.x * -100, byteArray,  0);
                CopyDoubleToByteArray((double)message.position.z * -100, byteArray,  doubleSize);
                CopyDoubleToByteArray((double)message.position.y * 100 , byteArray,  doubleSize*2);

                CopyDoubleToByteArray((double)message.rotation.eulerAngles.y, byteArray,  doubleSize*3);
                CopyDoubleToByteArray((double)message.rotation.eulerAngles.x * -1, byteArray,  doubleSize*4);
                CopyDoubleToByteArray((double)message.rotation.eulerAngles.z, byteArray,  doubleSize*5);
                udp_.Send(byteArray, otUdpLen);

                if (dataTransimissionInterval > 0f)
                {
                    var ticks = (long)Mathf.Round(dataTransimissionInterval / 1000f * Stopwatch.Frequency);
                    while (sw.ElapsedTicks < ticks);
                }
            }
        }

        public void Add(Transform data)
        {
            PosRot t = new PosRot();
            t.position = data.position;
            t.rotation = data.rotation;
            lock (lockObject_)
            {
                messages_.Enqueue(t);

                while (messages_.Count > maxQueueSize)
                {
                    messages_.Dequeue();
                }
            }
        }
    }    
}
