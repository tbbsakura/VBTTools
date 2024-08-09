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
        private Vector3 _adjustPos = Vector3.zero;
        private Vector3 _adjustRotEu = Vector3.zero;

        [NonSerialized] public Transform _sphereL;
        [NonSerialized] public Transform _sphereR;
        public Text _adjustTextPos;
        public Text _adjustTextRot;

        public Image _imgRecvHMD;
        private int _rxMarkValue = 0;


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

                _adjustTextPos.text = $"pos {_adjustPos}";
                _animationTarget.GetBoneTransform(HumanBodyBones.LeftHand).localPosition = _adjustPos;
                _animationTarget.GetBoneTransform(HumanBodyBones.LeftHand).localRotation =  Quaternion.Euler(_adjustRotEu);

                _adjustTextRot.text = $"pos {_adjustRotEu}";
                _animationTarget.GetBoneTransform(HumanBodyBones.RightHand).localPosition
                            = new Vector3(-_adjustPos.x,_adjustPos.y,_adjustPos.z);
                _animationTarget.GetBoneTransform(HumanBodyBones.RightHand).localRotation
                            =  Quaternion.Euler(_adjustRotEu.x, 360f - _adjustRotEu.y, 360f - _adjustRotEu.z );
            }
        }

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

        public void OnToggleChangeAdjustUI(bool val) {_adjustingUI.SetActive(val);}
        public void OnSliderPosXChanged(float val) { _adjustPos.x = val; }
        public void OnSliderPosYChanged(float val) { _adjustPos.y = val; }
        public void OnSliderPosZChanged(float val) { _adjustPos.z = val; }
        public void OnSliderRotXChanged(float val) { _adjustRotEu.x = val; }
        public void OnSliderRotYChanged(float val) { _adjustRotEu.y = val; }
        public void OnSliderRotZChanged(float val) { _adjustRotEu.z = val; }

    };   // class end
}
