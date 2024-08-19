// Copyright (c) 2023-2024 Sakura(さくら) / tbbsakura
// MIT License. See "LICENSE" file.

using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using uOSC;
using System.IO;
using System;
using SFB;

using SakuraScript.ModifiedVMTSample;
using OgLikeVMT;
using UniGLTF;

namespace OgLikeVMT.SimpleSample
{
    [RequireComponent(typeof(uOSC.uOscClient))]
    [RequireComponent(typeof(OgLikeVMTClient))]
    public class SimpleOgLikeVMTSample : MonoBehaviour
    {
        private uOscClient _client = null;
        private OgLikeVMTClient _vmtclient = null; 

        public int _scalarMode = 0;

        // Client UI
        private Toggle _toggleClient;
        private InputField _inputFieldIP;
        private InputField _inputFieldDestPort;

        // test object
        public bool [] toggleFingers = new bool [5];
        public bool toggleLeft = false;

        void Start()
        {
            _client = GetComponent<uOscClient>();
            _vmtclient = GetComponent<OgLikeVMTClient>();

            _inputFieldIP = GameObject.Find("InputField_IP").GetComponent<InputField>();
            _inputFieldIP.text =  _client.address.ToString(); // _ipAddress;
            _inputFieldDestPort = GameObject.Find("InputField_Port").GetComponent<InputField>();
            _inputFieldDestPort.text = _client.port.ToString();

            _toggleClient = GameObject.Find("ToggleVMTClient").GetComponent<Toggle>();
            _toggleClient.isOn = false; 
        }

        void ChangeScalarMode( int fingerIndex, bool on )
        {
            if ( fingerIndex < 0 || fingerIndex >= (int)FingerIndex.COUNT ) return;
            int shifted = 1 << fingerIndex;
            if (on)
                _scalarMode |= shifted;
            else {
                _scalarMode = _scalarMode & (~(0xff & shifted));
            }
        }

        bool IsValidIpAddr( string ipString ) {
            IPAddress address;
            return (IPAddress.TryParse(ipString, out address));
        }

        int GetValidPortFromStr( string portstr )
        {
            int port;
            if (!int.TryParse(portstr, out port)) {
                return -1;
            }
            if (port <= 0 || port > 65535 ) {
                return -1;
            }
            return port;
        }

        public void SetClientTogglesOff()
        {
            _toggleClient.isOn = false;
        }

        bool InitClient()
        {
            if (_client == null ) {
                Debug.Log("OSC Client for VMT not specified.");
                return false;
            }

            int destport = GetValidPortFromStr(_inputFieldDestPort.text);
            if (destport != -1 ) 
            {
                _client.port = destport;
            }
            else 
            {
                Debug.Log("Invalid Dest Port.");
                return false;
            }

            if ( IsValidIpAddr(_inputFieldIP.text)) {
                _client.address = _inputFieldIP.text;
            }
            else {
                Debug.Log( "Invalid IP Address" );
                return false;
            }

            Debug.Log( "Client started." );
            return true;
        }
   
        void Update()
        {
            if (_vmtclient != null ) {
                if (toggleLeft)
                    _vmtclient.SendLeftHandData( _scalarMode); 
                else
                    _vmtclient.SendRightHandData(_scalarMode);
            }
        }

        public void OnJointCurlSliderChanged( float val, int fingerIndex, int jointIndex ) { 
            OgLikeHandData data = (toggleLeft) ? _vmtclient._leftHand : _vmtclient._rightHand;
            data.SetJointFlexion((FingerIndex)fingerIndex, jointIndex, val );
        }

        public void OnSplaySliderChanged( float val, int fingerIndex ) { 
            OgLikeHandData data = (toggleLeft) ? _vmtclient._leftHand : _vmtclient._rightHand;
            data.SetSplay((FingerIndex)fingerIndex, val );
        }

        public void OnScalarCurlSliderChanged( float val, int fingerIndex  ) { 
            OgLikeHandData data = (toggleLeft) ? _vmtclient._leftHand : _vmtclient._rightHand;
            data.SetFingerScalarFlexion((FingerIndex)fingerIndex, val );
        }

        public void OnSliderChangedAx(float val, int num) { 
            for (int i = 0 ; i < 5; i++ ) {
                if (toggleFingers[i]) {
                    OnJointCurlSliderChanged(val,i,num);
                    ChangeScalarMode(i,false);
                }
            }
        }

        public void OnSliderChangedA0(float val) { OnSliderChangedAx(val, 0); }
        public void OnSliderChangedA1(float val) { OnSliderChangedAx(val, 1); }
        public void OnSliderChangedA2(float val) { OnSliderChangedAx(val, 2); }
        public void OnSliderChangedA3(float val) { OnSliderChangedAx(val, 3); } 

        public void OnSliderChangedA4(float val) {
            for (int i = 0 ; i < 5; i++ ) {
                if (toggleFingers[i]) {
                    ChangeScalarMode(i,true);
                    OnScalarCurlSliderChanged( val, i);
                }
            }
        }

        public void OnSliderChangedA5(float val) 
        {
            for (int i = 0 ; i < 5; i++ ) {
                if (toggleFingers[i]) {
                    OnSplaySliderChanged(val,i);
                }
            }
        }
 
        public void OnToggleChangedThumb(bool val) { toggleFingers[0] = val ;}
        public void OnToggleChangedIndex(bool val) { toggleFingers[1] = val ;}
        public void OnToggleChangedMiddle(bool val) { toggleFingers[2] = val ;}
        public void OnToggleChangedRing(bool val) { toggleFingers[3] = val ;}
        public void OnToggleChangedPinky(bool val) { toggleFingers[4] = val ;}
        public void OnToggleChangedLR(bool val) { toggleLeft = val ;}
        
    };   // class end
}
