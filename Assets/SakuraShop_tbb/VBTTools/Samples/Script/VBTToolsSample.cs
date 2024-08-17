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

        private const int _portListen = 39544;

        private InputField _inputFieldIP;
        private InputField _inputFieldDestPort;
        private InputField _inputFieldListenPort;
        private InputField _inputFieldListenPortVMT;

        private Toggle _toggleServer;
        private Toggle _toggleServerCutEye;
        private Toggle _toggleClient;

        private Text _topText;

        // test object
        private bool [] toggleFingers = new bool [5];
        private bool toggleLeft = false;
        private bool _musmode = true;  

        [SerializeField] private GameObject _ui1Panel;
        [SerializeField] private GameObject _ui2JoyCon;
        [SerializeField] private GameObject _testUI;

        public GameObject _adjustingUI;
        private Vector3 _adjustPosL = Vector3.zero;
        private Vector3 _adjustRotEuL = Vector3.zero;
        private Vector3 _adjustPosR = Vector3.zero;
        private Vector3 _adjustRotEuR = Vector3.zero;
        public Text _adjustTextPosL;
        public Text _adjustTextRotL;
        public Text _adjustTextPosR;
        public Text _adjustTextRotR;
        
        [NonSerialized] public Transform _sphereL;
        [NonSerialized] public Transform _sphereR;
        private bool _wristRotate = false;

        public Image _imgRecvHMD;

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

            InitSliders();
        }

        private void InitSliders()
        {
            var sensorTemplateL = GameObject.Find("/origLeftHand/ControllerSensorL");
            var sensorTemplateR = GameObject.Find("/origRightHand/ControllerSensorR");
            _adjustPosL = sensorTemplateL.transform.localPosition;
            _adjustRotEuL= sensorTemplateL.transform.localRotation.eulerAngles;
            _adjustPosR = sensorTemplateR.transform.localPosition;
            _adjustRotEuR = sensorTemplateR.transform.localRotation.eulerAngles;
            SetAdjustSliderVal(true, _adjustPosL, _adjustRotEuL );
            SetAdjustSliderVal(false, _adjustPosR, _adjustRotEuR );
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
        }

        public bool SetHandler()
        {
            Debug.Log("Initializing HumanPoseHandler");
            _handler = new HumanPoseHandler( _animationTarget.avatar, _animationTarget.transform);
            if ( _handler == null ) {
                _topText.text = "HumanPoseHandler preparation failed.";
                Debug.Log("HumanPoseHandler preparation failed.");
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

            if (_adjustingUI.activeInHierarchy) {
                if (_animationTarget == null ) return;
                _vbtHandPosTrack._transformVirtualLController.localPosition = _adjustPosL;
                _vbtHandPosTrack._transformVirtualLController.localRotation =  Quaternion.Euler(_adjustRotEuL);
                _vbtHandPosTrack._transformVirtualRController.localPosition = _adjustPosR;
                _vbtHandPosTrack._transformVirtualRController.localRotation =  Quaternion.Euler(_adjustRotEuR);
                _adjustTextPosL.text = $"Left pos {_adjustPosL}";
                _adjustTextRotL.text = $"Left rot {_adjustRotEuL}";
                _adjustTextPosR.text = $"Right pos {_adjustPosR}";
                _adjustTextRotR.text = $"Right rot {_adjustRotEuR}";
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
        // Left
        public void OnSliderPosXChanged(float val) { _adjustPosL.x = val; }
        public void OnSliderPosYChanged(float val) { _adjustPosL.y = val; }
        public void OnSliderPosZChanged(float val) { _adjustPosL.z = val; }
        public void OnSliderRotXChanged(float val) { _adjustRotEuL.x = val; }
        public void OnSliderRotYChanged(float val) { _adjustRotEuL.y = val; }
        public void OnSliderRotZChanged(float val) { _adjustRotEuL.z = val; }
        // Right
        public void OnSliderPosXChangedR(float val) { _adjustPosR.x = val; }
        public void OnSliderPosYChangedR(float val) { _adjustPosR.y = val; }
        public void OnSliderPosZChangedR(float val) { _adjustPosR.z = val; }
        public void OnSliderRotXChangedR(float val) { _adjustRotEuR.x = val; }
        public void OnSliderRotYChangedR(float val) { _adjustRotEuR.y = val; }
        public void OnSliderRotZChangedR(float val) { _adjustRotEuR.z = val; }

        public void OnToggleChangeWristRotate(bool val) {
            _wristRotate = val;
            _exr.gameObject.SetActive(!val); // Preventing external receiver from adjusting Hips pos.
            if ( val ) {
                _toggleServer.isOn = false; // stop server
            }
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

        void SetAdjustSliderVal(bool isLeft, Vector3 pos, Vector3 rotEular ){
            _adjustingUI.SetActive(true); 
            string lr = isLeft ? "LeftGroup/" : "RightGroup/";
            GameObject.Find(lr+"SliderBx").GetComponent<Slider>().value = pos.x;
            GameObject.Find(lr+"SliderBy").GetComponent<Slider>().value = pos.y;
            GameObject.Find(lr+"SliderBz").GetComponent<Slider>().value = pos.z;
            GameObject.Find(lr+"SliderEux").GetComponent<Slider>().value = rotEular.x;
            GameObject.Find(lr+"SliderEuy").GetComponent<Slider>().value = rotEular.y;
            GameObject.Find(lr+"SliderEuz").GetComponent<Slider>().value = rotEular.z;
            _adjustingUI.SetActive(false); 
        }

    };   // class end
}
