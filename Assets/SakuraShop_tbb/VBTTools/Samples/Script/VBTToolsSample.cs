﻿// Copyright (c) 2023-2024 Sakura(さくら) / tbbsakura
// MIT License. See "LICENSE" file.

using System.Net;
using UnityEngine;
using UnityEngine.UI;
using uOSC;
using System;
using SakuraScript.Utils;
using SFB;

namespace SakuraScript.VBTTool
{
    [RequireComponent(typeof(VBTSkeletalTrack))]
    [RequireComponent(typeof(VBTBodyTrack))]
    public class VBTToolsSample : MonoBehaviour
    {
        [SerializeField] private EVMC4U.ExternalReceiver _exr = null;
        private uOscServer _server = null;
        [SerializeField] private uOscClient _client = null;
        [SerializeField] private uOscServer _serverVMT = null;
        [SerializeField] private VBTOpenTrackUDPClient _clientOpentrack;

        private VBTSkeletalTrack _vbtSkeletalTrack;
        private VBTBodyTrack _vbtBodyTrack;
        VBTTools_FileDragAndDrop _loader; 

        [Tooltip("VRMアバターを入れます。変更時はStartのチェックをオフにしてください")]
        private Animator _animationTarget;
        public Animator AnimationTarget { 
            get => _animationTarget;
            set {
                _animationTarget = value;
                if ( !SetHandler() ) Debug.Log("SetHandler failed");
                _vbtBodyTrack.AnimationTarget = _animationTarget;
                _vbtSkeletalTrack.AnimationTarget = _animationTarget;
            }
        }

        private HumanPose _targetHumanPose;
        private HumanPoseHandler _handler;

        // UI
        private Text _topText;

        // Server UI
        private Toggle _toggleServer;
        private Toggle _toggleServerCutEye;

        // Client UI
        private Toggle _toggleVMTClient;
        [SerializeField] private Image _imgRecvHMD; // as a RX LED

        private Toggle _toggleOpentrackClient; 

        // Network Setting UI
        private InputField _inputFieldListenPort;

        private InputField _inputFieldIP;
        private InputField _inputFieldDestPort;
        private InputField _inputFieldListenPortVMT;

        private InputField _inputFieldIPOpentrack;
        private InputField _inputFieldDestPortOpentrack;

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
        [SerializeField] private GameObject _ui2JoyConSub1;
        [SerializeField] private JoyconToVMT _joyconToVMTInstance;

        // Adjust setting UI
        [SerializeField] private GameObject _adjustingUI;
        [SerializeField] private GameObject _adjustingUISkeL;
        [SerializeField] private GameObject _adjustingUISkeR;
        private bool _wristRotate = false;
        private bool _wristRotateRestartVMCPListen = false;

        [SerializeField] private Toggle _wristRotateUI1;
        [SerializeField] private Toggle _wristRotateUI2;
        [SerializeField] private Toggle _wristRotateUI3;

        [SerializeField] VBTToolsSetting _setting;
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
        Vector3 _pauseHandRotOffsetL = Vector3.zero;
        Vector3 _pauseHandRotOffsetR = Vector3.zero;

        // Network setting UI panel
        [SerializeField] GameObject _networkSetting;
        Text _text_NWSetError;

        // // // // // // // // // // // // // // // // // // 
        // Start, Update and initializing functions
        void Start()
        {
            // Setting befor loading setting file
            _vbtSkeletalTrack = GetComponent<VBTSkeletalTrack>();
            _vbtBodyTrack = GetComponent<VBTBodyTrack>();
            _loader = GetComponent<VBTTools_FileDragAndDrop>();
            _setting._exrecSetting.CopyFromExReceiver( _exr );
            _server =  _exr.GetComponent<uOscServer>();

            _networkSetting.SetActive(true);
            _inputFieldListenPort = GameObject.Find("InputField_ListenPort").GetComponent<InputField>();
            _inputFieldIP = GameObject.Find("InputField_VMT_IP").GetComponent<InputField>();
            _inputFieldDestPort = GameObject.Find("InputField_VMT_Port").GetComponent<InputField>();
            _inputFieldListenPortVMT = GameObject.Find("InputField_ListenPortVMT").GetComponent<InputField>();
            _inputFieldIPOpentrack = GameObject.Find("InputField_IP(OT)").GetComponent<InputField>();
            _inputFieldDestPortOpentrack = GameObject.Find("InputField_Port(OT)").GetComponent<InputField>();
            _text_NWSetError = GameObject.Find("Text_NWSetError").GetComponent<Text>();
            _networkSetting.SetActive(false);

            _toggleServer = GameObject.Find("ToggleServer").GetComponent<Toggle>();
            _toggleServerCutEye = GameObject.Find("ToggleServerCutEye").GetComponent<Toggle>();
            _toggleVMTClient = GameObject.Find("ToggleVMTClient").GetComponent<Toggle>();
            _toggleOpentrackClient = GameObject.Find("ToggleOpenTrackClient").GetComponent<Toggle>();
            _topText = GameObject.Find("TopText").GetComponent<Text>();
            // Read default adjusting values 
            // v0.0.4以降では default.json があれば優先される
            var sensorTemplateL = GameObject.Find("/origLeftHand/ControllerSensorL");
            var sensorTemplateR = GameObject.Find("/origRightHand/ControllerSensorR");
            _adjSetting.PosL = sensorTemplateL.transform.localPosition;
            _adjSetting.RotEuL= sensorTemplateL.transform.localRotation.eulerAngles;
            _adjSetting.PosR = sensorTemplateR.transform.localPosition;
            _adjSetting.RotEuR = sensorTemplateR.transform.localRotation.eulerAngles;
            _adjSetting.HandPosL = _vbtBodyTrack.HandPosOffsetL;
            _adjSetting.HandPosR = _vbtBodyTrack.HandPosOffsetR;

            // loading setting file
            string adjpath = GetDefaultAdjSettingFilePath();
            if (System.IO.File.Exists(adjpath) ) {
                VBTSetting<VBTToolsAdjustSetting> loader = new VBTSetting<VBTToolsAdjustSetting>();
                if ( loader.LoadFromFile(adjpath) ) _adjSetting = loader.Data;
            }
            else {
                Debug.Log($"File not found: {adjpath}");
            }
            string pathSetting = GetMainSettingFilePath();
            if (System.IO.File.Exists(pathSetting) ) {
                VBTMainSetting loader = new VBTMainSetting();
                if ( loader.LoadFromFile(pathSetting) ) _setting = loader.Data;
            }
            else {
                _setting.SetVersionAsCurrent();
            }

            // Setting after loaded setting file
            ApplyNetworkSetting();

            _toggleServer.isOn = true; // ExternalReceiverが Start してしまうので
            _toggleServerCutEye.isOn = true; 
            _toggleVMTClient.isOn = false; 

            // 初期設定値を AdjustingUI のスライダーに反映後、非表示に
            InitSliders();
            _adjustingUI.SetActive(false); 
            _adjustingUISkeL.SetActive(false); 
            _adjustingUISkeR.SetActive(false); 

            // デフォルトモデルのロード
            _loader.OpenVRM(DefaultVRMPath);
        }

        private void OnDestroy() {
            Destroy( _ui2JoyConSub1 );
            Destroy( _ui2JoyCon );
            VBTMainSetting saver = new VBTMainSetting();
            saver.Data = _setting;
            saver.SaveToFile(GetMainSettingFilePath());
        }

        // // // // // // // // // // // // // // // // // // 
        // Update
        void Update()
        {
            if ( _vbtBodyTrack ) {
                UpdateRecvHMD(_vbtBodyTrack.RxLED);
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

        // // // // // // // // // // // // // //
        // VRM Loading 関連
        // Buttonによるload
        public void OnVRMLoadButton()
        {
            var extensions = new[] { new ExtensionFilter("VRM Files", "vrm" ), };
            var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);
            if (paths.Length > 0 && paths[0].Length > 0) {
                _loader.OpenVRM(paths[0]);
            }
        }

        // デフォルトロードファイルのパスを返す。_setting のファイルがなければ HairSample_Male.vrm 
        // Unity Project 上から Play すると、 exe の場所 = unity の exe の場所になるので
        // ファイルが実際にはそこに無いこともあるため、確認して無い場合は "" を返す。
        private string DefaultVRMPath {
            get {
                if (_setting._defaultVRM.Length > 0 && System.IO.File.Exists(_setting._defaultVRM)){
                    return _setting._defaultVRM;
                }
#if UNITY_EDITOR
                const char separatorChar = '/';
                string modelFilepath = "Assets/SakuraShop_tbb/VRM_CC0/HairSample_Male.vrm"; //CC0 model
                modelFilepath = modelFilepath.Replace( separatorChar, System.IO.Path.DirectorySeparatorChar );
#else
                string modelFilepath = "HairSample_Male.vrm"; //CC0 model
#endif
                if ( System.IO.File.Exists(modelFilepath) ) return modelFilepath;
                return "";
            }
        }

        // VRMファイル読み込み後の処理
        public void OnVRMLoaded(Animator animator)
        {
            // animationtarget, HumanPoseHandler 変数更新
            AnimationTarget = animator;
            _setting._defaultVRM = _loader.LastLoadedFile; // 最後に読めたファイルを次回読むファイルにする

            // トラッカー位置を示すオブジェクト(left/rightsensor)を手の子にして、Pos/Rot Adjustを適用
            var leftsensor = new GameObject("LeftSensor");
            leftsensor.transform.parent = animator.GetBoneTransform( HumanBodyBones.LeftHand );
            _vbtBodyTrack.TransformVirtualLController = leftsensor.transform;

            var rightsensor = new GameObject("RightSensor");
            rightsensor.transform.parent = animator.GetBoneTransform( HumanBodyBones.RightHand );
            _vbtBodyTrack.TransformVirtualRController = rightsensor.transform;

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
            _toggleVMTClient.isOn = false;
            _vbtBodyTrack.StopHandTrack();
            _vbtSkeletalTrack.IsOn = false;
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

            SetHandler();
            if ( _handler == null ) {
                _topText.text = "Cannot init client, invalid HumanPoseHandler.";
                Debug.Log(_topText.text);
                return false;
            }
            _serverVMT.StartServer();
            _vbtBodyTrack.StartHandTrack(_setting._networkSetting._vmtListenPort);

            _vbtSkeletalTrack.IsOn = true;

            _topText.text = "Client for VMT started.";
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
                _toggleVMTClient.isOn = false;
                _vbtBodyTrack.StopHandTrack();
                _vbtSkeletalTrack.IsOn = false;
                _topText.text = "Client stopped.";
            }
        }

        // HMDパケット受信インジケーターの処理
        public void UpdateRecvHMD( float alpha ) {
            Color color =_imgRecvHMD.color;
            color.a = alpha;
            _imgRecvHMD.color = color;
        }

        public void OnClientOpentrackToggleChanged (bool value) {
            _vbtBodyTrack.EnableHeadTrack(value);
        }

        // // // // // // // // // // // // // // // // // // 
        // Server UI functions
        public void OnServerCutEyeToggleChanged(bool value) 
        {
            if ( _exr == null ) return;
            _setting._exrecSetting.SetCutBonesIsOn(true);
            _setting._exrecSetting.SetCutEyeIsOn(value);
            _setting._exrecSetting.CopyToExReceiver(_exr);
        }

        public void OnServerFBTModeChanged(bool value) 
        {
            if ( _exr == null ) return;
            _setting._exrecSetting.SetCutBonesIsOn(true);
            _setting._exrecSetting.SetCutTorsoIsOn(!value); 
            _setting._exrecSetting.SetCutLegFootIsOn(!value);
            _setting._exrecSetting.SetRootSyncIsOn(value); 
            _setting._exrecSetting.CopyToExReceiver(_exr);
        }

        // always no-cut
        //_setting._exrecSetting.SetCutHeadNeckIsOn(false);
        //_setting._exrecSetting.SetCutArmHandIsOn(false);
        //_setting._exrecSetting.SetCutSkeletalIsOn(false);

        public void OnServerToggleChanged(bool value) 
        {
            if (_server == null) return;
            ResetPauseOffset();
            if ( _toggleServer.isOn == false ) {
                _server.StopServer();
                _topText.text = "OSC (VMCP) server stopped. Test UI available.";
                _testUI.SetActive(true);
            }
            else 
            {
                _server.StartServer();
                if ( _topText != null ) _topText.text = "OSC (VMCP) Server started.";
                _testUI.SetActive(false);
            }
            _joyconToVMTInstance.EnableStickMove = _toggleServer.isOn;
        }

        // // // // // // // // // // // // // // // // // // 
        // Controller UI functions
        public void OnJoyConToggleChanged(bool value) {
            if (value) {
                _ui2JoyCon.SetActive(value);
                _ui2JoyConSub1.SetActive(value);
            }
            else {
                _ui2JoyConSub1.SetActive(value);
                _ui2JoyCon.SetActive(value);
            }
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
                    _vbtBodyTrack.TransformVirtualLController.localPosition = _adjSetting.PosL;
                    _vbtBodyTrack.TransformVirtualLController.localRotation =  Quaternion.Euler(_adjSetting.RotEuL);
                    _vbtBodyTrack.TransformVirtualRController.localPosition = _adjSetting.PosR;
                    _vbtBodyTrack.TransformVirtualRController.localRotation =  Quaternion.Euler(_adjSetting.RotEuR);
                    _vbtBodyTrack.HandPosOffsetL = _adjSetting.HandPosL + _pauseHandPosOffsetL;
                    _vbtBodyTrack.HandPosOffsetR = _adjSetting.HandPosR + _pauseHandPosOffsetR;
                    _vbtBodyTrack.HandEulerOffsetL = _pauseHandRotOffsetL;
                    _vbtBodyTrack.HandEulerOffsetR = _pauseHandRotOffsetR;
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
            VBTSetting<VBTToolsAdjustSetting> saver = new VBTSetting<VBTToolsAdjustSetting>();
            saver.Data = _adjSetting;
            saver.SaveToFile();
        }

        public void OnLoadButton()
        {
            VBTSetting<VBTToolsAdjustSetting> loader = new VBTSetting<VBTToolsAdjustSetting>();
            if (loader.LoadFromFile()) {
                _adjSetting = loader.Data;
                InitSliders();
            }
        }
        
        // // // // // // // // // // // // // // // // // // 
        // PauseHandPosAdjust related functions
        private const  float _movePos = 0.003f; // 3mm ずつ

        public bool IsPauseMode {
            get  { return !_toggleServer.isOn; }
        }

        public void OnJoyconButtonDown( bool isLeftCon, Int32 buttonIndex) {
            //Debug.Log($"Joycon Button Down {buttonIndex}");
            if ( buttonIndex == (int)Joycon.Button.DPAD_LEFT ) {
                _toggleServer.isOn = !_toggleServer.isOn; // 反転
                _joyconToVMTInstance.EnableStickMove = _toggleServer.isOn;
            }
            if ( buttonIndex == (int)Joycon.Button.DPAD_RIGHT) {
                _toggleServer.isOn = false; // 停止確定
                _joyconToVMTInstance.EnableStickMove = _toggleServer.isOn;
                SetMenuPauseOffset();
                UpdateAdjust(0);
            }
            if ( isLeftCon && buttonIndex == (int)Joycon.Button.SR ) {
                _toggleOpentrackClient.isOn = !_toggleOpentrackClient.isOn;
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

         // Menu操作しやすい手の位置、Headとの相対位置
        Vector3 RightMenuPos = new Vector3(-1.05639941e-11f,0.0339747667f,0.0025523901f); // new Vector3(0.234467059f,-0.156040668f,0.15293619f);
        Vector3 RightMenuRot = new Vector3(357.278076f,280.557037f,93.1913147f);
        Vector3 LeftMenuPos = new Vector3(-0.229998678f,-0.0037421513f,0.37446329f);
        Vector3 LeftMenuRot = new Vector3(287.08432f,121.202736f,232.910187f);

        void SetMenuPauseOffset()
        {
            Vector3 hmdPos = _vbtBodyTrack.TransformHMD.position; //  _animationTarget.GetBoneTransform(HumanBodyBones.Head).position;
            Quaternion hmdRot = _vbtBodyTrack.TransformHMD.rotation; // _animationTarget.GetBoneTransform(HumanBodyBones.Head).rotation;
            float hmdYaw = _vbtBodyTrack.TransformHMD.eulerAngles.y; 
            _pauseHandPosOffsetL = hmdPos + LeftMenuPos - _vbtBodyTrack.TransformVirtualLController.position;
            _pauseHandPosOffsetR = hmdPos + RightMenuPos - _vbtBodyTrack.TransformVirtualRController.position;
            Quaternion goalLeft = Quaternion.Euler(LeftMenuRot) * hmdRot; // なるべき姿勢
            _pauseHandRotOffsetL = (Quaternion.Inverse(_vbtBodyTrack.TransformVirtualLController.rotation) * goalLeft).eulerAngles;
            Quaternion goalRight = Quaternion.Euler(RightMenuRot) * hmdRot; // なるべき姿勢
            _pauseHandRotOffsetR = (Quaternion.Inverse(_vbtBodyTrack.TransformVirtualRController.rotation) * goalRight).eulerAngles;
        }

        public void ResetPauseOffset()
        {
            _pauseHandPosOffsetL = Vector3.zero;
            _pauseHandPosOffsetR = Vector3.zero;
            _pauseHandRotOffsetL = Vector3.zero;
            _pauseHandRotOffsetR = Vector3.zero;
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

        // // // // // // // // // // // // // // // // // // 
        // Network setting UI panel
        void ApplyNetworkSetting()
        {
            _server.port = _setting._networkSetting._vmcpListenPort;
            _client.address = _setting._networkSetting._vmtSendAddress;
            _client.port = _setting._networkSetting._vmtSendPort;
            _serverVMT.port = _setting._networkSetting._vmtListenPort;
            _clientOpentrack.address = _setting._networkSetting._opentrackSendAddress;
            _clientOpentrack.port = _setting._networkSetting._opentrackSendPort;
        }

        public void NWSettingOpen()
        {
            // Set to input field
            _inputFieldListenPort.text = _setting._networkSetting._vmcpListenPort.ToString();
            _inputFieldIP.text =  _setting._networkSetting._vmtSendAddress; 
            _inputFieldDestPort.text = _setting._networkSetting._vmtSendPort.ToString();
            _inputFieldListenPortVMT.text = _setting._networkSetting._vmtListenPort.ToString();
            _inputFieldIPOpentrack.text = _setting._networkSetting._opentrackSendAddress;
            _inputFieldDestPortOpentrack.text = _setting._networkSetting._opentrackSendPort.ToString();
            _text_NWSetError.text = "";
            _networkSetting.SetActive(true);
        }

        public void OnCancel_NWSetting()
        {
            _networkSetting.SetActive(false);
        }

        public void OnOK_NWSetting()
        {
            if ( IsValidIpAddr(_inputFieldIP.text) == false ) {
                _text_NWSetError.text = "Error: VMT IP Address を適切に設定してください。";
                return;                
            } 
            if (IsValidIpAddr(_inputFieldIPOpentrack.text ) == false) {
                _text_NWSetError.text = "Error: Opentrack IP Address を適切に設定してください。";
                return;                
            }

            int vmcpListenPort = GetValidPortFromStr(_inputFieldListenPort.text);
            int vmtSendPort = GetValidPortFromStr(_inputFieldDestPort.text);
            int vmtListenPort = GetValidPortFromStr(_inputFieldListenPortVMT.text);
            int opentrackSendPort = GetValidPortFromStr(_inputFieldDestPortOpentrack.text);
            if ( vmcpListenPort == -1 || vmtSendPort == -1 || vmtListenPort == -1 || opentrackSendPort == -1 ) {
                _text_NWSetError.text = "Error: ポート番号は 0-65535 の範囲で設定してください。";
                return;                
            }

            // _setting と server/client に即時適用
            _setting._networkSetting._vmtSendAddress = _inputFieldIP.text; 
            _setting._networkSetting._opentrackSendAddress = _inputFieldIPOpentrack.text;

            if ( vmcpListenPort != _setting._networkSetting._vmcpListenPort ) {
                _setting._networkSetting._vmcpListenPort = vmcpListenPort;
            }
            if ( vmtSendPort != _setting._networkSetting._vmtSendPort ) {
                _setting._networkSetting._vmtSendPort = vmtSendPort;
            }
            if ( vmtListenPort != _setting._networkSetting._vmtListenPort ) {
                _setting._networkSetting._vmtListenPort = vmtListenPort;
            }
            if ( opentrackSendPort != _setting._networkSetting._opentrackSendPort ) {
                _setting._networkSetting._opentrackSendPort = opentrackSendPort;
            }

            ApplyNetworkSetting();
            _networkSetting.SetActive(false);
        }

        string GetJsonDirectory()
        {
#if UNITY_EDITOR
            string path = "Assets\\SakuraShop_tbb\\VBTTools\\etc\\setting";
#else
            string path = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');//EXEを実行したカレントディレクトリ (ショートカット等でカレントディレクトリが変わるのでこの方式で)
#endif
            return path;
        }

        string GetMainSettingFilePath()
        {
            string path = GetJsonDirectory();
            return  path + "\\VBTTools.setting.json";
        }

        string GetDefaultAdjSettingFilePath()
        {
            string path = GetJsonDirectory();
            return path + "\\default.json";
        }
    };   // class end
}
