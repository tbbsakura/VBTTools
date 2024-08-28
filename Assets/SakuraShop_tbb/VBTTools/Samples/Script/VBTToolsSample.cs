﻿// Copyright (c) 2023-2024 Sakura(さくら) / tbbsakura
// MIT License. See "LICENSE" file.

using System.Net;
using UnityEngine;
using UnityEngine.UI;
using uOSC;
using System.IO;
using System;
using SFB;
using SakuraScript.Utils;

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
        [SerializeField] private Image _imgRecvHMD; // as a RX LED
        private InputField _inputFieldIP;
        private InputField _inputFieldDestPort;
        private InputField _inputFieldListenPortVMT;

        // test object
        private bool [] toggleFingers = new bool [5];
        private bool toggleLeft = false;
        private bool _musmode = true;  
        [SerializeField] private GameObject _testUI;
        [SerializeField] private TransformSliders _leftTransformSliders;
        [SerializeField] private TransformSliders _rightTransformSliders;

        // Controller UI panel
        [SerializeField] private GameObject _ui1Panel;
        [SerializeField] private GameObject _ui2JoyCon;
        [SerializeField] private JoyconToVMT _joyconToVMTInstance;

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

        [SerializeField] private GameObject _adjustingUI;
        [SerializeField] private GameObject _adjustingUISkeL;
        [SerializeField] private GameObject _adjustingUISkeR;
        private bool _wristRotate = false;
        private bool _wristRotateRestartVMCPListen = false;

        [SerializeField] private Toggle _wristRotateUI1;
        [SerializeField] private Toggle _wristRotateUI2;
        [SerializeField] private Toggle _wristRotateUI3;

        [SerializeField] VBTToolsAdjustSetting _adjSetting;

        // Display adjusting values
        [SerializeField] private TransformSliders _tfsADSG1_LeftSlidersA;
        [SerializeField] private TransformSliders _tfsADSG1_RightSlidersA;
        [SerializeField] private TransformSliders _tfsADSG1_LeftSlidersB_HandPos;
        [SerializeField] private TransformSliders _tfsADSG1_RightSlidersB_HandPos;

        [SerializeField] private TransformSliders _tfsADSG2_LeftRoot;
        [SerializeField] private TransformSliders _tfsADSG2_LeftWrist;

        [SerializeField] private TransformSliders _tfsADSG3_RightRoot;
        [SerializeField] private TransformSliders _tfsADSG3_RightWrist;

        // PauseHandPosAdjust : 一時停止＆手の一時的位置移動機能関連
        Vector3 _pauseHandPosOffsetL = Vector3.zero;
        Vector3 _pauseHandPosOffsetR = Vector3.zero;

        // // // // // // // // // // // // // // // // // // 
        // Start, Update and initializing functions
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
            // v0.0.4以降では default.json があれば優先される
            var sensorTemplateL = GameObject.Find("/origLeftHand/ControllerSensorL");
            var sensorTemplateR = GameObject.Find("/origRightHand/ControllerSensorR");
            _adjSetting.PosL = sensorTemplateL.transform.localPosition;
            _adjSetting.RotEuL= sensorTemplateL.transform.localRotation.eulerAngles;
            _adjSetting.PosR = sensorTemplateR.transform.localPosition;
            _adjSetting.RotEuR = sensorTemplateR.transform.localRotation.eulerAngles;
            _adjSetting.HandPosL = _vbtHandPosTrack._handPosOffsetL;
            _adjSetting.HandPosR = _vbtHandPosTrack._handPosOffsetR;

#if UNITY_EDITOR
            string path = "Assets\\SakuraShop_tbb\\VBTTools\\etc\\setting";
#else
            string path = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');//EXEを実行したカレントディレクトリ (ショートカット等でカレントディレクトリが変わるのでこの方式で)
#endif
            path += "\\default.json";
            if (System.IO.File.Exists(path) ) {
                LoadSettingFile(path);
            }
            else {
                Debug.Log($"File not found: {path}");
            }

            // 初期設定値を AdjustingUI のスライダーに反映後、非表示に
            InitSliders();
            _adjustingUI.SetActive(false); 
            _adjustingUISkeL.SetActive(false); 
            _adjustingUISkeR.SetActive(false); 
        }

        // // // // // // // // // // // // // // // // // // 
        // Update
        void Update()
        {
            if ( _vbtHandPosTrack ) {
                UpdateRecvHMD(_vbtHandPosTrack.RxLED);
            }
            else {
                UpdateRecvHMD(0.2f);
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

        // 設定ファイル読み込み後に adjustingUIs のスライダーを再設定する
        private void InitSliders()
        {
            _tfsADSG1_LeftSlidersA.SetValue( _adjSetting.PosL, _adjSetting.RotEuL );
            _tfsADSG1_RightSlidersA.SetValue( _adjSetting.PosR, _adjSetting.RotEuR );
            _tfsADSG1_LeftSlidersB_HandPos.SetValue( _adjSetting.HandPosL, Vector3.zero );
            _tfsADSG1_RightSlidersB_HandPos.SetValue( _adjSetting.HandPosR, Vector3.zero );
            _tfsADSG2_LeftRoot.SetValue( _adjSetting.RootPosL, _adjSetting.RootRotL );
            _tfsADSG2_LeftWrist.SetValue( _adjSetting.WristPosL, _adjSetting.WristRotL );
            _tfsADSG3_RightRoot.SetValue( _adjSetting.RootPosR, _adjSetting.RootRotR );
            _tfsADSG3_RightWrist.SetValue( _adjSetting.WristPosR, _adjSetting.WristRotR );
        }

        // VRMファイル読み込み後の処理
        public void OnVRMLoaded(Animator animator)
        {
            // animationtarget, HumanPoseHandler 変数更新
            _animationTarget = animator;
            SetHandler();

            // トラッカー位置を示すオブジェクト(left/rightsensor)を手の子にして、Pos/Rot Adjustを適用
            var leftsensor = new GameObject("LeftSensor");
            leftsensor.transform.parent = animator.GetBoneTransform( HumanBodyBones.LeftHand );
            _vbtHandPosTrack._transformVirtualLController = leftsensor.transform;

            var rightsensor = new GameObject("RightSensor");
            rightsensor.transform.parent = animator.GetBoneTransform( HumanBodyBones.RightHand );
            _vbtHandPosTrack._transformVirtualRController = rightsensor.transform;

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

        // // // // // // // // // // // // // // // // // // // // 
        // Client related functions
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

        // client toggle on をキャンセルする場合等に呼ぶ
        public void SetClientTogglesOff()
        {
            _toggleClient.isOn = false;
            _vbtHandPosTrack.StopTrack();
            _vbtSkeletalTrack._isOn = false;
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
   
        // // // // // // // // // // // // // // // // // // 
        // Client UI functions
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

        // HMDパケット受信インジケーターの処理
        public void UpdateRecvHMD( float alpha ) {
            Color color =_imgRecvHMD.color;
            color.a = alpha;
            _imgRecvHMD.color = color;
        }

        // // // // // // // // // // // // // // // // // // 
        // Server UI functions
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
            ResetPauseOffset();
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
            _joyconToVMTInstance.enableStickMove = _toggleServer.isOn;
        }

        // // // // // // // // // // // // // // // // // // 
        // Controller UI functions
        public void OnJoyConToggleChanged(bool value) {
            _ui2JoyCon.SetActive(value);
        }

        public void OnUIPanelToggleChanged(bool value) {
            _ui1Panel.SetActive(value);
        }

        // // // // // // // // // // // // // // // // //
        // Testing UI functions
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

        public void OnToggleChangedThumb(bool val) { toggleFingers[0] = val ;}
        public void OnToggleChangedIndex(bool val) { toggleFingers[1] = val ;}
        public void OnToggleChangedMiddle(bool val) { toggleFingers[2] = val ;}
        public void OnToggleChangedRing(bool val) { toggleFingers[3] = val ;}
        public void OnToggleChangedPinky(bool val) { toggleFingers[4] = val ;}
        public void OnToggleChangedLR(bool val) { toggleLeft = val ;}
        
        public void OnToggleChangedMusmode(bool val) { _musmode = val ; }

        // // // // // // // // // // // // // // // // // 
        // Adjusting UI
        public void OnToggleChangeAdjustUI(bool val) { _adjustingUI.SetActive(val); }
        public void OnToggleChangeAdjustUI2(bool val) { _adjustingUISkeL.SetActive(val); }
        public void OnToggleChangeAdjustUI3(bool val) { _adjustingUISkeR.SetActive(val); }

        // Adjusting UI1 Left : on-Slider-changed Callback
        public void OnADSG1LeftSlidersAChanged(Vector3 pos, Vector3 rot){
            _adjSetting.PosL = pos;
            _adjSetting.RotEuL = rot;
            UpdateAdjust(1);
        }
        public void OnADSG1RightSlidersAChanged(Vector3 pos, Vector3 rot){
            _adjSetting.PosR = pos;
            _adjSetting.RotEuR = rot;
            UpdateAdjust(1);
        }
        public void OnADSG1LeftSlidersBChanged(Vector3 pos, Vector3 rot){
            _adjSetting.HandPosL = pos;
            UpdateAdjust(1);
        }
        public void OnADSG1RightSlidersBChanged(Vector3 pos, Vector3 rot){
            _adjSetting.HandPosR = pos;
            UpdateAdjust(1);
        }
        public void OnADSG2RootSlidersChanged(Vector3 pos, Vector3 rot){
            _adjSetting.RootPosL = pos;
            _adjSetting.RootRotL = rot;
            UpdateAdjust(2);
        }
        public void OnADSG2WristSlidersChanged(Vector3 pos, Vector3 rot){
            _adjSetting.WristPosL = pos;
            _adjSetting.WristRotL = rot;
            UpdateAdjust(2);
        }
        public void OnADSG3RootSlidersChanged(Vector3 pos, Vector3 rot){
            _adjSetting.RootPosR = pos;
            _adjSetting.RootRotR = rot;
            UpdateAdjust(3);
        }
        public void OnADSG3WristSlidersChanged(Vector3 pos, Vector3 rot){
            _adjSetting.WristPosR = pos;
            _adjSetting.WristRotR = rot;
            UpdateAdjust(3);
        }

        private void UpdateAdjust( int uiNum ) { // if 0 , all 
            if ( uiNum == 0 || uiNum == 1)  {
                if ( _animationTarget != null ) {
                    _vbtHandPosTrack._transformVirtualLController.localPosition = _adjSetting.PosL;
                    _vbtHandPosTrack._transformVirtualLController.localRotation =  Quaternion.Euler(_adjSetting.RotEuL);
                    _vbtHandPosTrack._transformVirtualRController.localPosition = _adjSetting.PosR;
                    _vbtHandPosTrack._transformVirtualRController.localRotation =  Quaternion.Euler(_adjSetting.RotEuR);
                    _vbtHandPosTrack._handPosOffsetL = _adjSetting.HandPosL + _pauseHandPosOffsetL;
                    _vbtHandPosTrack._handPosOffsetR = _adjSetting.HandPosR + _pauseHandPosOffsetR;
                }
            }
            if ( uiNum == 0 || uiNum == 2)  {
                _vbtSkeletalTrack.SetRootWristOffset( true, _adjSetting.RootPosL, _adjSetting.RootRotL, _adjSetting.WristPosL, _adjSetting.WristRotL );
            }
            if ( uiNum == 0 || uiNum == 3)  {
                _vbtSkeletalTrack.SetRootWristOffset( false, _adjSetting.RootPosR, _adjSetting.RootRotR, _adjSetting.WristPosR, _adjSetting.WristRotR );
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
        // Adjusting UI - Wrist rotation
        readonly float [] _wristRotateArray = new float[] { -1f, -0.75f, -0.5f, -0.25f, 0f, 0.25f, 0.5f, 0.75f, 1f };
        private int _rotateCurrentIndex = 0;
        private float _rotateWristRate = 0.1f;
        private float _nextRotate = 0.0f;
        private int _rotateDirection = 1;

        private float GetNextRotateTwist() {
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

        // // // // // // // // // // // // // // // // // // 
        // Adjusting UI - Setting File Save/Load
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
            Debug.Log( $"LoadSettingFile: {path}" ); 
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
        
        // // // // // // // // // // // // // // // // // // 
        // PauseHandPosAdjust related functions
        private const  float _movePos = 0.01f; // 1cm ずつ

        // 初期導入では、pause = ServerOff : 将来は変更する可能性あり
        public bool IsPauseMode {
            get  { return !_toggleServer.isOn; }
        }

        public void OnJoyconButtonDown( bool isLeftCon, Int32 buttonIndex) {
            //Debug.Log($"Joycon Button Down {buttonIndex}");
            if ( buttonIndex == 2 ) {
                _toggleServer.isOn = !_toggleServer.isOn; // 反転
                _joyconToVMTInstance.enableStickMove = _toggleServer.isOn;
            }
        }

        public void OnJoyconStick( bool isLeftCon, Int32 stickIndex) {
            if ( IsPauseMode ) {
                switch (stickIndex) {  
                    case 2: 
                        if (isLeftCon) {
                            PauseLeftHandPosDown();
                        }
                        else {
                            PauseRightHandPosDown(); 
                        }
                        UpdateAdjust(0);
                        break;
                    case 4: 
                        if (isLeftCon) {
                            PauseLeftHandPosLeft();
                        }
                        else {
                            PauseRightHandPosLeft(); 
                        }
                        UpdateAdjust(0);
                        break;
                    case 6:  
                        if (isLeftCon) {
                            PauseLeftHandPosRight();
                        }
                        else {
                            PauseRightHandPosRight(); 
                        }
                        UpdateAdjust(0);
                        break;
                    case 8: 
                        if (isLeftCon) {
                            PauseLeftHandPosUp();
                        }
                        else {
                            PauseRightHandPosUp(); 
                        }
                        UpdateAdjust(0);
                        break;
                    default: Debug.Log($"Stick {stickIndex}");break;
                }
            }
        }

        public void ResetPauseOffset()
        {
            _pauseHandPosOffsetL = Vector3.zero;
            _pauseHandPosOffsetR = Vector3.zero;
            _leftTransformSliders.SetValue( Vector3.zero, Vector3.zero ); 
            _rightTransformSliders.SetValue( Vector3.zero, Vector3.zero ); 
            UpdateAdjust(0);
        }

        
        void PauseLeftHandPosUp()    { _pauseHandPosOffsetL.y += _movePos; UpdatePauseHandPosSlidersLeft();}
        void PauseLeftHandPosDown()  { _pauseHandPosOffsetL.y -= _movePos; UpdatePauseHandPosSlidersLeft();}
        void PauseLeftHandPosLeft()  { _pauseHandPosOffsetL.x -= _movePos; UpdatePauseHandPosSlidersLeft();}
        void PauseLeftHandPosRight() { _pauseHandPosOffsetL.x += _movePos; UpdatePauseHandPosSlidersLeft();}
        void PauseRightHandPosUp()    { _pauseHandPosOffsetR.y += _movePos; UpdatePauseHandPosSlidersRight();}
        void PauseRightHandPosDown()  { _pauseHandPosOffsetR.y -= _movePos; UpdatePauseHandPosSlidersRight();}
        void PauseRightHandPosLeft()  { _pauseHandPosOffsetR.x -= _movePos; UpdatePauseHandPosSlidersRight();}
        void PauseRightHandPosRight() { _pauseHandPosOffsetR.x += _movePos; UpdatePauseHandPosSlidersRight();}

        // JoyCon側で変更→Test UI Slidersに反映
        void UpdatePauseHandPosSlidersLeft() { _leftTransformSliders.SetValue( _pauseHandPosOffsetL, Vector3.zero ); }
        void UpdatePauseHandPosSlidersRight() { _rightTransformSliders.SetValue( _pauseHandPosOffsetR, Vector3.zero ); }
        // TestUI の Slider側で変更
        public void OnUpdatePauseHandPosSlidersLeft( Vector3 pos, Vector3 rot ) { _pauseHandPosOffsetL = pos; UpdateAdjust(0);}
        public void OnUpdatePauseHandPosSlidersRight( Vector3 pos, Vector3 rot ) { _pauseHandPosOffsetR = pos; UpdateAdjust(0);}

        // // // // // // // // // // // // // // // // // // 
        // VMT Enable/Disable functions
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
    };   // class end
}
