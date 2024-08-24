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

namespace SakuraScript.VBTTool
{
    [RequireComponent(typeof(VBTSkeletalTrack))]
    [RequireComponent(typeof(VBTHandPosTrack))]
    public class VBTToolsSample : MonoBehaviour
    {
        [SerializeField] private EVMC4U.ExternalReceiver _exr = null;
        private uOscServer _server = null;
        [SerializeField] private uOscClient _client = null;
        [SerializeField] private uOscServer _serverVMT = null;

        private VBTSkeletalTrack _vbtSkeletalTrack;
        [NonSerialized] public VBTHandPosTrack _vbtHandPosTrack;

        [Tooltip("VRMアバターを入れます。変更時はStartのチェックをオフにしてください")]
        public Animator _animationTarget;
        private HumanPose _targetHumanPose;
        private HumanPoseHandler _handler;

        // UI
        private Text _topText;

        // Server UI
        private Toggle _toggleServer;
        private Toggle _toggleServerCutEye;
        private InputField _inputFieldListenPort;

        private const int _portListen = 39544;

        // Client UI
        private Toggle _toggleClient;
        public Image _imgRecvHMD; // as a RX LED
        private InputField _inputFieldIP;
        private InputField _inputFieldDestPort;
        private InputField _inputFieldListenPortVMT;

        // test object
        private bool [] toggleFingers = new bool [5];
        private bool toggleLeft = false;
        private bool _musmode = true;  

        [SerializeField] private GameObject _ui1Panel;
        [SerializeField] private GameObject _ui2JoyCon;
        [SerializeField] private GameObject _testUI;

        // Adjust setting UI
        [System.Serializable]
        private class VBTToolsAdjustSetting
        {
            public Vector3 PosL;
            public Vector3 RotEuL;
            public Vector3 PosR;
            public Vector3 RotEuR;
            public Vector3 HandPosL;
            public Vector3 HandPosR;

            public Vector3 RootPosL;
            public Vector3 RootRotL;
            public Vector3 WristPosL;
            public Vector3 WristRotL;
            public Vector3 RootPosR;
            public Vector3 RootRotR;
            public Vector3 WristPosR;
            public Vector3 WristRotR;
        }

        public GameObject _adjustingUI;
        public GameObject _adjustingUISkeL;
        public GameObject _adjustingUISkeR;
        private bool _wristRotate = false;
        private bool _wristRotateRestartVMCPListen = false;

        public Toggle _wristRotateUI1;
        public Toggle _wristRotateUI2;
        public Toggle _wristRotateUI3;

        [SerializeField] VBTToolsAdjustSetting _adjSetting;

        // Display adjusting values
        public Text _adjustTextPosL;
        public Text _adjustTextRotL;
        public Text _adjustTextPosR;
        public Text _adjustTextRotR;
        public Text _adjustTextHandPosL;        
        public Text _adjustTextHandPosR;        

        public Text _adjustTextRootPosL; 
        public Text _adjustTextRootRotL; 
        public Text _adjustTextWristPosL;
        public Text _adjustTextWristRotL;

        public Text _adjustTextRootPosR;
        public Text _adjustTextRootRotR;
        public Text _adjustTextWristPosR;
        public Text _adjustTextWristRotR;

        [NonSerialized] public Transform _sphereL;
        [NonSerialized] public Transform _sphereR;


        // Start is called before the first frame update
        void Start()
        {
            _vbtSkeletalTrack = GetComponent<VBTSkeletalTrack>();
            _vbtHandPosTrack = GetComponent<VBTHandPosTrack>();

            _server =  _exr.GetComponent<uOscServer>();
            _server.port = _portListen;
            _inputFieldListenPort = GameObject.Find("InputField_ListenPort").GetComponent<InputField>();
            _inputFieldListenPort.text = _portListen.ToString();
            
            _inputFieldIP = GameObject.Find("InputField_IP").GetComponent<InputField>();
            _inputFieldIP.text =  _client.address.ToString(); // _ipAddress;
            _inputFieldDestPort = GameObject.Find("InputField_Port").GetComponent<InputField>();
            _inputFieldDestPort.text = _client.port.ToString();
            _inputFieldListenPortVMT = GameObject.Find("InputField_ListenPortVMT").GetComponent<InputField>();
            _inputFieldListenPortVMT.text = _serverVMT.port.ToString();

            _toggleServer = GameObject.Find("ToggleServer").GetComponent<Toggle>();
            _toggleServer.isOn = true; // ExternalReceiverが Start してしまうので

            _toggleServerCutEye = GameObject.Find("ToggleServerCutEye").GetComponent<Toggle>();
            _toggleServerCutEye.isOn = true; 

            _toggleClient = GameObject.Find("ToggleVMTClient").GetComponent<Toggle>();
            _toggleClient.isOn = false; 

            _topText = GameObject.Find("TopText").GetComponent<Text>();

            // Read default adjusting values
            var sensorTemplateL = GameObject.Find("/origLeftHand/ControllerSensorL");
            var sensorTemplateR = GameObject.Find("/origRightHand/ControllerSensorR");
            _adjSetting.PosL = sensorTemplateL.transform.localPosition;
            _adjSetting.RotEuL= sensorTemplateL.transform.localRotation.eulerAngles;
            _adjSetting.PosR = sensorTemplateR.transform.localPosition;
            _adjSetting.RotEuR = sensorTemplateR.transform.localRotation.eulerAngles;
            _adjSetting.HandPosL = _vbtHandPosTrack._handPosOffsetL;
            _adjSetting.HandPosR = _vbtHandPosTrack._handPosOffsetR;

            string path = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');//EXEを実行したカレントディレクトリ (ショートカット等でカレントディレクトリが変わるのでこの方式で)
            path += "\\default.json";
            if (System.IO.File.Exists(path) ) {
                LoadSettingFile(path);
            }

            InitSliders();
            _adjustingUI.SetActive(false); 
            _adjustingUISkeL.SetActive(false); 
            _adjustingUISkeR.SetActive(false); 
        }

        private void InitSliders()
        {
            bool ui1View = _adjustingUI.activeInHierarchy;
            bool ui2View = _adjustingUISkeL.activeInHierarchy;
            bool ui3View = _adjustingUISkeR.activeInHierarchy;
            _adjustingUI.SetActive(true); 
            _adjustingUISkeL.SetActive(true); 
            _adjustingUISkeR.SetActive(true); 
            SetAdjustSliderVal("LeftGroup/", _adjSetting.PosL, _adjSetting.RotEuL, false );
            SetAdjustSliderVal("RightGroup/", _adjSetting.PosR, _adjSetting.RotEuR, false );
            SetAdjustSliderVal("HandPosGroupL/", _adjSetting.HandPosL, Vector3.zero, true );
            SetAdjustSliderVal("HandPosGroupR/", _adjSetting.HandPosR, Vector3.zero, true );
            SetAdjustSliderVal("RootGroupL/", _adjSetting.RootPosL, _adjSetting.RootRotL, false );
            SetAdjustSliderVal("WristGroupL/", _adjSetting.WristPosL, _adjSetting.WristRotL, false );
            SetAdjustSliderVal("RootGroupR/", _adjSetting.RootPosR, _adjSetting.RootRotR, false );
            SetAdjustSliderVal("WristGroupR/", _adjSetting.WristPosR, _adjSetting.WristRotR, false );
            _adjustingUI.SetActive(ui1View); 
            _adjustingUISkeL.SetActive(ui2View); 
            _adjustingUISkeR.SetActive(ui3View); 
        }

        public void OnVRMLoaded(Animator animator)
        {
            var sensorTemplateL = GameObject.Find("/origLeftHand/ControllerSensorL");
            var sensorTemplateR = GameObject.Find("/origRightHand/ControllerSensorR");

            _animationTarget = animator;
            SetHandler();

            var leftsensor = new GameObject("LeftSensor");
            leftsensor.transform.parent = animator.GetBoneTransform( HumanBodyBones.LeftHand );
            leftsensor.transform.localPosition = sensorTemplateL.transform.localPosition;
            leftsensor.transform.localRotation = sensorTemplateL.transform.localRotation;
            _vbtHandPosTrack._transformVirtualLController = leftsensor.transform;
            var sl = GameObject.Find("/origLeftHand/ControllerSensorL/Sphere");
            if (sl) {
                sl.transform.parent = leftsensor.transform;
                sl.transform.localPosition = Vector3.zero;
                _sphereL = sl.transform;
            }

            var rightsensor = new GameObject("RightSensor");
            rightsensor.transform.parent = animator.GetBoneTransform( HumanBodyBones.RightHand );
            rightsensor.transform.localPosition = sensorTemplateR.transform.localPosition;
            rightsensor.transform.localRotation = sensorTemplateR.transform.localRotation;
            _vbtHandPosTrack._transformVirtualRController = rightsensor.transform;
            var sr = GameObject.Find("/origLeftHand/ControllerSensorR/Sphere");
            if (sr) {
                sr.transform.parent = rightsensor.transform;
                sr.transform.localPosition = Vector3.zero;
                _sphereR = sr.transform;
            }

            UpdateAdjust(0);
        }

        public bool SetHandler()
        {
            //Debug.Log("Initializing HumanPoseHandler");
            _handler = new HumanPoseHandler( _animationTarget.avatar, _animationTarget.transform);
            if ( _handler == null ) {
                _topText.text = "HumanPoseHandler preparation failed.";
                //Debug.Log("HumanPoseHandler preparation failed.");
                return false;
            }
            return true;
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
            _vbtHandPosTrack.StopTrack();
            _vbtSkeletalTrack._isOn = false;
        }

        public void OnServerCutEyeToggleChanged(bool value) 
        {
            if ( _exr == null ) return;

            _exr.CutBoneNeck = false;
            _exr.CutBoneHead = false;
            _exr.CutBoneLeftEye = true;
            _exr.CutBoneRightEye = true;
            _exr.CutBoneJaw = false;

            _exr.CutBoneHips = false;
            _exr.CutBoneSpine = false;
            _exr.CutBoneChest = false;
            _exr.CutBoneUpperChest = false;

            _exr.CutBoneLeftUpperLeg = false;
            _exr.CutBoneLeftLowerLeg = false;
            _exr.CutBoneLeftFoot = false;
            _exr.CutBoneLeftToes = false;

            _exr.CutBoneRightUpperLeg = false;
            _exr.CutBoneRightLowerLeg = false;
            _exr.CutBoneRightFoot = false;
            _exr.CutBoneRightToes = false;

            _exr.CutBonesEnable = value;                
        }

        public void OnServerToggleChanged(bool value) 
        {
            //Debug.Log( $"OnServerToggleChanged: {value}");
            if (_server == null) return;
            if ( _toggleServer.isOn == false ) {
                _server.StopServer();
                _topText.text = "OSC server stopped.";
                _testUI.SetActive(true);
            }
            else 
            {
                int port = GetValidPortFromStr(_inputFieldListenPort.text);
                if ( port > 0 ) 
                {            
                    _server.port = port;
                    _server.StartServer();
                    if ( _topText != null ) _topText.text = "OSC Server started.";
                    _testUI.SetActive(false);
                }
                else {
                    _toggleServer.isOn = false;
                    _topText.text = "Invalid server port.";
                }
            }
        }

        public void SendEnable()
        {
            _client.Send("/VMT/Room/Unity", 1, 5, 0.0f, 0f,0f,0f, 0f,0f,0f,0f);
            _client.Send("/VMT/Room/Unity", 2, 6, 0.0f, 0f,0f,0f, 0f,0f,0f,0f);
        }

        public void SendDisable()
        {
            _client.Send("/VMT/Room/Unity", 1, 0, 0.0f, 0f,0f,0f, 0f,0f,0f,0f);
            _client.Send("/VMT/Room/Unity", 2, 0, 0.0f, 0f,0f,0f, 0f,0f,0f,0f);
        }

        // 全てのVMTトラッカーの電源をオフにする
        public void SendReset() 
        {
            _client.Send("/VMT/Reset");
        }

        bool InitClient()
        {
            if (_client == null ) {
                _topText.text = "OSC Client for VMT not specified.";
                Debug.Log(_topText.text);
                return false;
            }

            if ( _animationTarget == null ) {
                _topText.text = "Cannot init client, no VRM loaded.";
                Debug.Log(_topText.text);
                return false;
            }
            if ( _animationTarget != null  ) { // When VRM model loaded, prepare handler
                if (!SetHandler()) {
                    _topText.text = "Failed in initializing HumanPoseHandler.";
                    return false;
                }
            }

            int destport = GetValidPortFromStr(_inputFieldDestPort.text);
            if (destport != -1 ) 
            {
                _client.port = destport;
            }
            else 
            {
                _topText.text = "Invalid Dest Port.";
                Debug.Log(_topText.text);
                return false;
            }

            int vmtListenPort = GetValidPortFromStr(_inputFieldListenPortVMT.text);
            if (vmtListenPort != -1 ) 
            {
                if ( _serverVMT.isRunning ) {
                    _serverVMT.StopServer();
                }
                _serverVMT.port = vmtListenPort;
                _serverVMT.StartServer();
                _vbtHandPosTrack._animationTarget = this._animationTarget;
                _vbtHandPosTrack.StartTrack(vmtListenPort);
            }
            else 
            {
                _topText.text = "Invalid VMT listening Port.";
                Debug.Log(_topText.text);
                return false;
            }

            if ( IsValidIpAddr(_inputFieldIP.text)) {
                _client.address = _inputFieldIP.text;
            }
            else {
                _topText.text = "Invalid IP Address";
                Debug.Log(_topText.text);
                return false;
            }
            _vbtSkeletalTrack._animationTarget = this._animationTarget;
            _vbtSkeletalTrack._isOn = true;


            _topText.text = "Client started.";
            Debug.Log(_topText.text);
            if ( _handler == null ) Debug.Log( "_handler is null" );
            return true;
        }
   
        public void OnJoyConToggleChanged(bool value) {
            _ui2JoyCon.SetActive(value);
        }

        public void OnUIPanelToggleChanged(bool value) {
            _ui1Panel.SetActive(value);
        }

        public void OnClientToggleChanged(bool value)
        {
            Debug.Log( "OnClientToggleChanged: " + value.ToString());
            if ( value == true ) {
                if (!InitClient()) 
                {
                    SetClientTogglesOff();
                }
            }
            else {
                // VMT の controller enable (種類)を 0: disable にする
                SendDisable();
                _toggleClient.isOn = false;
                _vbtHandPosTrack.StopTrack();
                _vbtSkeletalTrack._isOn = false;
                _topText.text = "Client stopped.";
            }
        }

        public void UodateRecvHMD( float alpha ) {
            Color color =_imgRecvHMD.color;
            color.a = alpha;
            _imgRecvHMD.color = color;
        }

        void Update()
        {
            if ( _vbtHandPosTrack ) {
                UodateRecvHMD(_vbtHandPosTrack.RxLED);
            }
            else {
                UodateRecvHMD(0.2f);
            }

            if (_wristRotate) {
                if ( _handler == null ) {
                    //Debug.Log( "_handler is null" );
                }
                else {
                    _handler.GetHumanPose(ref _targetHumanPose);   
                    // lower arm twist: 43/52,   upper arm twist: 41/50
                    float newVal = GetNextRotateTwist();
                    _targetHumanPose.muscles[43] = newVal;
                    _targetHumanPose.muscles[52] = newVal;
                    //_targetHumanPose.muscles[41] = newVal; // This would move the wrist position.
                    //_targetHumanPose.muscles[50] = newVal; // This would move the wrist position. 
                    _handler.SetHumanPose(ref _targetHumanPose);   
                }
            }
        }

        // // // // // // // // // // // // // // // // //
        // VMT TEST UI
        // 2ndTrack Skeletal / UI mixed
        public void OnJointCurlSliderChanged( float val, int fingerIndex, int jointIndex ) { 
            Debug.Log( $"OnJointCurlSliderChanged( val = {val}, fingerIndex = {fingerIndex}, jointIndex = {jointIndex} )");
            _vbtSkeletalTrack.SetJointCurl(toggleLeft, val, fingerIndex, jointIndex);
        }

        public void OnSplaySliderChanged( float val, int fingerIndex ) { 
            _vbtSkeletalTrack.SetSplay(toggleLeft, (val * 2.0f - 1.0f), fingerIndex );
        }

        // _MuslteTest では、 VRMモデルの muscle を操作するだけ
        // (client started であれば、モデルの変更が _vbtSkeletalTrack の Update で VMTで送出される)
        public void OnJointCurlSliderChanged_MuscleTest( float val, int iFinger, int iJoint ) { 
            if (_handler == null ) return;
            _handler.GetHumanPose(ref _targetHumanPose);   
            int musIndex = VBTSkeletalTrack.GetHumanoidStretchMuscleIndex( toggleLeft, iFinger, iJoint);
            float before = _targetHumanPose.muscles[musIndex];
            _targetHumanPose.muscles[musIndex] = -(val * 2.0f - 1.0f);
            _handler.SetHumanPose(ref _targetHumanPose);   
            //Debug.Log( $"OnJointCurlSliderChanged_MuscleTest: val = {val}, musIndex = {musIndex}, before = {before}");
        }

        public void OnSplaySliderChanged_MuscleTest( float val, int iFinger ) { 
            if (_handler == null ) return;
            _handler.GetHumanPose(ref _targetHumanPose);   
            int musIndex = VBTSkeletalTrack.GetHumanoidSpreadMuscleIndex( toggleLeft, iFinger );
            float before = _targetHumanPose.muscles[musIndex];
            _targetHumanPose.muscles[musIndex] = (val * 2.0f - 1.0f);
            _handler.SetHumanPose(ref _targetHumanPose);   
            //Debug.Log( $"OnSplaySliderChanged_MuscleTest: val = {val}, musIndex = {musIndex}, before = {before}");
        }

        public void OnSliderChangedA0(float val) { 
            for (int i = 0 ; i < 5; i++ ) {
                if (toggleFingers[i]) {
                    if (_musmode)
                        OnJointCurlSliderChanged_MuscleTest(val,i,0);
                    else
                        OnJointCurlSliderChanged(val,i,0);
                }
            }
        }

        public void OnSliderChangedA1(float val) { 
            for (int i = 0 ; i < 5; i++ ) {
                if (toggleFingers[i]) {
                    if (_musmode)
                        OnJointCurlSliderChanged_MuscleTest(val,i,1);
                    else
                        OnJointCurlSliderChanged(val,i,1);
                }
            }
        }

        public void OnSliderChangedA2(float val) { 
            for (int i = 0 ; i < 5; i++ ) {
                if (toggleFingers[i]) {
                    if (_musmode)
                        OnJointCurlSliderChanged_MuscleTest(val,i,2);
                    else
                        OnJointCurlSliderChanged(val,i,2);
                }
            }
        }

        public void OnSliderChangedA3(float val) 
        {
            if (!_musmode ) // Humanoid has 3 bones on each finger, although Skeletal has 4.
                for (int i = 0 ; i < 5; i++ ) {
                    if (toggleFingers[i]) OnJointCurlSliderChanged(val,i,3);
                }
        }

        public void OnSliderChangedA4(float val) 
        {
            for (int i = 0 ; i < 5; i++ ) {
                if (toggleFingers[i]) {
                    if (_musmode)
                        OnSplaySliderChanged_MuscleTest(val,i);
                    else
                        OnSplaySliderChanged(val,i);
                }
            }
        }

        public void OnSliderChangedA5(float val) {
            for (int i = 0 ; i < 5; i++ ) {
                if (toggleFingers[i]) {
                    for ( int j = 0 ; j < (_musmode ? 3:4); j++ ) {
                        if (_musmode)
                            OnJointCurlSliderChanged_MuscleTest(val,i,j);
                        else
                            OnJointCurlSliderChanged(val,i,j);
                    }
                }
            }
        }
        public void OnSliderChangedA6(float val) { }
        public void OnSliderChangedA7(float val) { }
        public void OnSliderChangedA8(float val) { }
        public void OnSliderChangedA9(float val) { }
        public void OnSliderChangedA10(float val) {}
        public void OnSliderChangedA11(float val) {}

        public void OnToggleChangedThumb(bool val) { toggleFingers[0] = val ;}
        public void OnToggleChangedIndex(bool val) { toggleFingers[1] = val ;}
        public void OnToggleChangedMiddle(bool val) { toggleFingers[2] = val ;}
        public void OnToggleChangedRing(bool val) { toggleFingers[3] = val ;}
        public void OnToggleChangedPinky(bool val) { toggleFingers[4] = val ;}
        public void OnToggleChangedLR(bool val) { toggleLeft = val ;}
        
        public void OnToggleChangedMusmode(bool val) { _musmode = val ; }

        // // // // // // // // // // // // // // // // //
        // Adjust UI
        public void OnToggleChangeAdjustUI(bool val) { _adjustingUI.SetActive(val); }
        public void OnToggleChangeAdjustUI2(bool val) { _adjustingUISkeL.SetActive(val); }
        public void OnToggleChangeAdjustUI3(bool val) { _adjustingUISkeR.SetActive(val); }

        // Adjusting UI1 Left
        public void OnSliderPosXChanged(float val) { _adjSetting.PosL.x = val; UpdateAdjust(1); }
        public void OnSliderPosYChanged(float val) { _adjSetting.PosL.y = val; UpdateAdjust(1); }
        public void OnSliderPosZChanged(float val) { _adjSetting.PosL.z = val; UpdateAdjust(1); }
        public void OnSliderRotXChanged(float val) { _adjSetting.RotEuL.x = val; UpdateAdjust(1); }
        public void OnSliderRotYChanged(float val) { _adjSetting.RotEuL.y = val; UpdateAdjust(1); }
        public void OnSliderRotZChanged(float val) { _adjSetting.RotEuL.z = val; UpdateAdjust(1); }
        // Adjusting UI1 Right
        public void OnSliderPosXChangedR(float val) { _adjSetting.PosR.x = val; UpdateAdjust(1); }
        public void OnSliderPosYChangedR(float val) { _adjSetting.PosR.y = val; UpdateAdjust(1); }
        public void OnSliderPosZChangedR(float val) { _adjSetting.PosR.z = val; UpdateAdjust(1); }
        public void OnSliderRotXChangedR(float val) { _adjSetting.RotEuR.x = val; UpdateAdjust(1); }
        public void OnSliderRotYChangedR(float val) { _adjSetting.RotEuR.y = val; UpdateAdjust(1); }
        public void OnSliderRotZChangedR(float val) { _adjSetting.RotEuR.z = val; UpdateAdjust(1); }
        // Adjusting UI1 HandPos L
        public void OnSliderHandPosXChangedL(float val) { _adjSetting.HandPosL.x = val; UpdateAdjust(1); }
        public void OnSliderHandPosYChangedL(float val) { _adjSetting.HandPosL.y = val; UpdateAdjust(1); }
        public void OnSliderHandPosZChangedL(float val) { _adjSetting.HandPosL.z = val; UpdateAdjust(1); }
        // Adjusting UI1 HandPos R
        public void OnSliderHandPosXChangedR(float val) { _adjSetting.HandPosR.x = val; UpdateAdjust(1); }
        public void OnSliderHandPosYChangedR(float val) { _adjSetting.HandPosR.y = val; UpdateAdjust(1); }
        public void OnSliderHandPosZChangedR(float val) { _adjSetting.HandPosR.z = val; UpdateAdjust(1); }

        // Adjusting UI2 Root
        public void OnSliderRootPosXChangedL(float val) { _adjSetting.RootPosL.x = val; UpdateAdjust(2); }
        public void OnSliderRootPosYChangedL(float val) { _adjSetting.RootPosL.y = val; UpdateAdjust(2); }
        public void OnSliderRootPosZChangedL(float val) { _adjSetting.RootPosL.z = val; UpdateAdjust(2); }
        public void OnSliderRootRotXChangedL(float val) { _adjSetting.RootRotL.x = val; UpdateAdjust(2); }
        public void OnSliderRootRotYChangedL(float val) { _adjSetting.RootRotL.y = val; UpdateAdjust(2); }
        public void OnSliderRootRotZChangedL(float val) { _adjSetting.RootRotL.z = val; UpdateAdjust(2); }
        // Adjusting UI2 Wrist
        public void OnSliderWristPosXChangedL(float val) { _adjSetting.WristPosL.x = val; UpdateAdjust(2); }
        public void OnSliderWristPosYChangedL(float val) { _adjSetting.WristPosL.y = val; UpdateAdjust(2); }
        public void OnSliderWristPosZChangedL(float val) { _adjSetting.WristPosL.z = val; UpdateAdjust(2); }
        public void OnSliderWristRotXChangedL(float val) { _adjSetting.WristRotL.x = val; UpdateAdjust(2); }
        public void OnSliderWristRotYChangedL(float val) { _adjSetting.WristRotL.y = val; UpdateAdjust(2); }
        public void OnSliderWristRotZChangedL(float val) { _adjSetting.WristRotL.z = val; UpdateAdjust(2); }

        // Adjusting UI3 Root
        public void OnSliderRootPosXChangedR(float val) { _adjSetting.RootPosR.x = val; UpdateAdjust(3); }
        public void OnSliderRootPosYChangedR(float val) { _adjSetting.RootPosR.y = val; UpdateAdjust(3); }
        public void OnSliderRootPosZChangedR(float val) { _adjSetting.RootPosR.z = val; UpdateAdjust(3); }
        public void OnSliderRootRotXChangedR(float val) { _adjSetting.RootRotR.x = val; UpdateAdjust(3); }
        public void OnSliderRootRotYChangedR(float val) { _adjSetting.RootRotR.y = val; UpdateAdjust(3); }
        public void OnSliderRootRotZChangedR(float val) { _adjSetting.RootRotR.z = val; UpdateAdjust(3); }
        // Adjusting UI3 Wrist
        public void OnSliderWristPosXChangedR(float val) { _adjSetting.WristPosR.x = val; UpdateAdjust(3); }
        public void OnSliderWristPosYChangedR(float val) { _adjSetting.WristPosR.y = val; UpdateAdjust(3); }
        public void OnSliderWristPosZChangedR(float val) { _adjSetting.WristPosR.z = val; UpdateAdjust(3); }
        public void OnSliderWristRotXChangedR(float val) { _adjSetting.WristRotR.x = val; UpdateAdjust(3); }
        public void OnSliderWristRotYChangedR(float val) { _adjSetting.WristRotR.y = val; UpdateAdjust(3); }
        public void OnSliderWristRotZChangedR(float val) { _adjSetting.WristRotR.z = val; UpdateAdjust(3); }

        private void UpdateAdjust( int uiNum ) { // if 0 , all 
            if ( uiNum == 0 || uiNum == 1)  {
                if ( _animationTarget != null ) {
                    _vbtHandPosTrack._transformVirtualLController.localPosition = _adjSetting.PosL;
                    _vbtHandPosTrack._transformVirtualLController.localRotation =  Quaternion.Euler(_adjSetting.RotEuL);
                    _vbtHandPosTrack._transformVirtualRController.localPosition = _adjSetting.PosR;
                    _vbtHandPosTrack._transformVirtualRController.localRotation =  Quaternion.Euler(_adjSetting.RotEuR);
                    _vbtHandPosTrack._handPosOffsetL = _adjSetting.HandPosL;
                    _vbtHandPosTrack._handPosOffsetR = _adjSetting.HandPosR;
                }

                _adjustTextPosL.text = $"Left pos {_adjSetting.PosL}";
                _adjustTextRotL.text = $"Left rot {_adjSetting.RotEuL}";
                _adjustTextPosR.text = $"Right pos {_adjSetting.PosR}";
                _adjustTextRotR.text = $"Right rot {_adjSetting.RotEuR}";
                _adjustTextHandPosL.text = $"Left Hand pos {_adjSetting.HandPosL}";
                _adjustTextHandPosR.text = $"Right Hand pos {_adjSetting.HandPosR}";
            }
            if ( uiNum == 0 || uiNum == 2)  {
                _vbtSkeletalTrack.SetRootWristOffset( true, _adjSetting.RootPosL, _adjSetting.RootRotL, _adjSetting.WristPosL, _adjSetting.WristRotL );
                _adjustTextRootPosL.text = $"Root pos {_adjSetting.RootPosL}";
                _adjustTextRootRotL.text = $"Root rot {_adjSetting.RootRotL}";
                _adjustTextWristPosL.text = $"Wrist pos {_adjSetting.WristPosL}";
                _adjustTextWristRotL.text = $"Wrist rot {_adjSetting.WristRotL}";
            }
            if ( uiNum == 0 || uiNum == 3)  {
                _vbtSkeletalTrack.SetRootWristOffset( false, _adjSetting.RootPosR, _adjSetting.RootRotR, _adjSetting.WristPosR, _adjSetting.WristRotR );
                _adjustTextRootPosR.text = $"Root pos {_adjSetting.RootPosR}";
                _adjustTextRootRotR.text = $"Root rot {_adjSetting.RootRotR}";
                _adjustTextWristPosR.text = $"Wrist pos {_adjSetting.WristPosR}";
                _adjustTextWristRotR.text = $"Wrist rot {_adjSetting.WristRotR}";
            }
        }

        public void OnToggleChangeWristRotate(bool val) {
            _wristRotate = val;
            // 全てのAdjustUIパネルで選択を一致させる。修正の都度呼ばれるので未一致があった時は即 return する
            if (_wristRotateUI1.isOn != val) { _wristRotateUI1.isOn = val; return; }
            if (_wristRotateUI2.isOn != val) { _wristRotateUI2.isOn = val; return; }
            if (_wristRotateUI3.isOn != val) { _wristRotateUI3.isOn = val; return; }

            if ( val ) {
                _wristRotateRestartVMCPListen = _toggleServer.isOn; // save state
                _toggleServer.isOn = false; // stop server
            }
            else if (_wristRotateRestartVMCPListen) { // restore state
                _toggleServer.isOn = true; // restart server
            }
            _exr.gameObject.SetActive(!val); // Preventing external receiver from adjusting Hips pos.
        }
        // // // // // // // // // // // // // // // // //
        // Wrist rotation
        readonly float [] _wristRotateArray = new float[] { -1f, -0.75f, -0.5f, -0.25f, 0f, 0.25f, 0.5f, 0.75f, 1f };
        int _rotateCurrentIndex = 0;
        private float _rotateWristRate = 0.1f;
        private float _nextRotate = 0.0f;
        private int _rotateDirection = 1;

        public float GetNextRotateTwist() {
            if ( Time.time > _nextRotate ) {
                if (_rotateCurrentIndex == _wristRotateArray.Length -1 ) {
                    _rotateDirection = -1;
                }
                if ( _rotateCurrentIndex == 0 ) {
                    _rotateDirection = 1;
                }
                if ( _rotateCurrentIndex < 0 || _rotateDirection >= _wristRotateArray.Length ) {
                    _rotateCurrentIndex = 0;
                }
                _rotateCurrentIndex += _rotateDirection;
                _nextRotate = Time.time + _rotateWristRate;
                //Debug.Log( $"rotateIndex : {_rotateCurrentIndex}, next : {_nextRotate}");
            }
            return _wristRotateArray[_rotateCurrentIndex];
        }

        void SetAdjustSliderVal(string group, Vector3 pos, Vector3 rotEular, bool posOnly ){
            GameObject.Find(group+"SliderBx").GetComponent<Slider>().value = pos.x;
            GameObject.Find(group+"SliderBy").GetComponent<Slider>().value = pos.y;
            GameObject.Find(group+"SliderBz").GetComponent<Slider>().value = pos.z;
            if ( !posOnly ) {
                GameObject.Find(group+"SliderEux").GetComponent<Slider>().value = rotEular.x;
                GameObject.Find(group+"SliderEuy").GetComponent<Slider>().value = rotEular.y;
                GameObject.Find(group+"SliderEuz").GetComponent<Slider>().value = rotEular.z;
            }
        }

        public void OnSaveButton()
        {
            var extensions = new[] {
                new ExtensionFilter("Json Files", "json" ),
            };

            var path = StandaloneFileBrowser.SaveFilePanel("Save File", "", "default.json", extensions);

            if (path.Length > 0)
            {
                var json = JsonUtility.ToJson(_adjSetting, true);
                //Debug.Log( $"Saving to file {path} : " + json);

                StreamWriter sw = new StreamWriter(path,false); 
                sw.Write(json);
                sw.Flush();
                sw.Close();
            }
        }

        public void OnLoadButton()
        {
            var extensions = new[] {
                new ExtensionFilter("Json Files", "json" ),
            };

            var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);
            if (paths.Length > 0 && paths[0].Length > 0)
            {
                //Debug.Log( $"Opening file {paths[0]}");
                LoadSettingFile(paths[0]);
            }
        }

        private void LoadSettingFile(string path)
        {
            StreamReader sr = new StreamReader(path, false);
            string json = "";
            while(!sr.EndOfStream) {
                json += sr.ReadLine ();
            }
            sr.Close();
            var obj = JsonUtility.FromJson<VBTToolsAdjustSetting>(json);
            _adjSetting = obj;
            InitSliders(); 
        }
        
    };   // class end
}
