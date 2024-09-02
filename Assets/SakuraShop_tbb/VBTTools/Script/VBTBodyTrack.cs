// Copyright (c) 2024 Sakura(さくら) / tbbsakura
// MIT License. See "LICENSE" file.

using UnityEngine;
using uOSC;

namespace SakuraScript.VBTTool
{
    public class VBTBodyTrack : MonoBehaviour
    {
        [SerializeField, Tooltip("OSCServer Receiving from VMT. VMTからの情報を受け取るOSCサーバー")]
        uOscServer _server;
        [SerializeField, Tooltip("OSCServer Sending to VMT. VMTへ情報を送信するOSCクライアント")]
        uOscClient _client;

        Animator _animationTarget;
        public Animator AnimationTarget {
            get => _animationTarget;
            set => _animationTarget = value;
        }

        Transform _transformVirtualLController;
        public Transform TransformVirtualLController {
            get => _transformVirtualLController; set => _transformVirtualLController = value;
        }
        Transform _transformVirtualRController;
        public Transform TransformVirtualRController {
            get => _transformVirtualRController; set => _transformVirtualRController = value;
        }

        [Space]
        [SerializeField, Tooltip("VMTから受信したHMDのTransformを設定するオブジェクト")]
        Transform _transformHMD;
        [SerializeField, Tooltip("VMTに送信する左コントローラーのTransformを設定するオブジェクト")]
        Transform _transformLController;
        [SerializeField, Tooltip("VMTに送信する右コントローラーのTransformを設定するオブジェクト")]
        Transform _transformRController;

        [Space]
        [SerializeField, Tooltip("左手の表示位置補正(VRM Local)")]
        Vector3 _handPosOffsetL = new Vector3( 0f, 0f, 0f);
        public Vector3 HandPosOffsetL {
            get => _handPosOffsetL; set => _handPosOffsetL = value;
        }
        [SerializeField, Tooltip("右手の表示位置補正(VRM Local)")]
        Vector3 _handPosOffsetR = new Vector3( 0f, 0f, 0f);
        public Vector3 HandPosOffsetR {
            get => _handPosOffsetR; set => _handPosOffsetR = value;
        }

        [SerializeField, Tooltip("左手の回転補正(Global) 基本ゼロで")]
        Vector3 _handEulerOffsetL = new Vector3( 0, 0, 0 );
        [SerializeField, Tooltip("右手の回転補正(Global) 基本ゼロで")]
        Vector3 _handEulerOffsetR = new Vector3( 0, 0 ,0 );

        [Space]
        [SerializeField, Tooltip("VMTに渡すパラメーター/左手のindex")]
        int _VMTIndexLeft = 1;
        [SerializeField, Tooltip("VMTに渡すパラメーター/左手のenable")]
        int _VMTEnableLeft = 5;
        [SerializeField, Tooltip("VMTに渡すパラメーター/右手のindex")]
        int _VMTIndexRight = 2;
        [SerializeField, Tooltip("VMTに渡すパラメーター/右手のenable")]
        int _VMTEnableRight = 6;

        const int _VMTEnableCompatibleTracker = 7; // vive tracker 互換
        [Space]

#if VMT_HMD_OVERRIDE
        private Vector3 _baseHMDPos; // VMTオーバーライド開始時のHMD位置
        private Quaternion _baseHMDRot; // VMTオーバーライド開始時のHMD回転
#endif

        [SerializeField] bool _enableHead = false;
        [SerializeField] bool _enableHand = false;

        [SerializeField] bool _enableWaistTrack = false;
        [SerializeField] bool _enableLeftFootTrack = false;
        [SerializeField] bool _enableRightFootTrack = false;

        [Header("Test : FBT VMT indices and enables")]
        [SerializeField, Tooltip("VMTに渡すパラメーター/左肘のindex")]
        int _VMTIndexLeftElbow = 7;
        [SerializeField, Tooltip("VMTに渡すパラメーター/右肘のindex")]
        int _VMTIndexRightElbow = 8;
        [SerializeField, Tooltip("VMTに渡すパラメーター/胸のindex")]
        int _VMTIndexBreast = 9;
        [SerializeField, Tooltip("VMTに渡すパラメーター/腰のindex")]
        int _VMTIndexWaist = 10;
        [SerializeField, Tooltip("VMTに渡すパラメーター/左膝のindex")]
        int _VMTIndexLeftKnee = 11;
        [SerializeField, Tooltip("VMTに渡すパラメーター/右膝のindex")]
        int _VMTIndexRightKnee = 12;
        [SerializeField, Tooltip("VMTに渡すパラメーター/左足のindex")]
        int _VMTIndexLeftFoot = 13;
        [SerializeField, Tooltip("VMTに渡すパラメーター/右足のindex")]
        int _VMTIndexRightFoot = 14;

        const int _VMTIndexHMD = 0;
        private readonly string _serialHMD = "HMD";

        private float _rxLED = 0.0f;
        public float RxLED => _rxLED;


        private VBTOpenTrackUDPClient _opentrackClient;
        
        // Start is called before the first frame update
        void Start()
        {
            _server.onDataReceived.AddListener(OnDataReceived);
            _opentrackClient = GetComponent<VBTOpenTrackUDPClient>();
        }

        void OnDisable()
        {
            if (_client != null && _enableHand) StopHandTrack();
        }

        public void StartHandTrack(int vmtListenPort)
        {
            if (_client != null) {
                _client.Send("/VMT/Set/Destination", "127.0.0.1", vmtListenPort );
                _client.Send("/VMT/Subscribe/Device", _serialHMD);
                _enableHand = true;
            }
        }

        public void StopHandTrack()
        {
            if (_client != null) _client.Send("/VMT/Unsubscribe/Device", _serialHMD);
            _enableHand = false;
        }

        public void EnableHeadTrack(bool isOn)
        {
            _enableHead = isOn;
        }

        public void OnDataReceived(uOSC.Message message)
        {
            if (message.address == "/VMT/Out/SubscribedDevice") {
                string strSerial = (string)message.values[0];
                if ( strSerial != _serialHMD ) return; // HMD以外の情報が来た

                // 受信LEDの処理
                _rxLED += 0.1f; 
                if (_rxLED > 1.0f) _rxLED = 1.0f;

                if ( _transformHMD == null ) return; // 設定対象がない
                Vector3 posReceived = new Vector3( 
                    (float)message.values[1],
                    (float)message.values[2], 
                    (float)message.values[3] 
                );
                Quaternion qReceived = new Quaternion(
                    (float)message.values[4],
                    (float)message.values[5],
                    (float)message.values[6],
                    (float)message.values[7]
                );

                // 受信するときはpos.z, q.z, q.w の符号を反転する
                // 送信するときは /VMT/Room/Unity で送ればVMT側で変換してくれる
                _transformHMD.position = new Vector3(posReceived.x, posReceived.y, -posReceived.z); 
                _transformHMD.rotation = new Quaternion(qReceived.x, qReceived.y, -qReceived.z, -qReceived.w);
            }
        }

        void Update()
        {
            _rxLED -= 0.01f; 
            if (_rxLED < 0.2f ) _rxLED = 0.2f;

            if ( _animationTarget == null ) return;

            // this position may be overriden by VMT_0? or not?: tbc
            Vector3 hmdPos = _animationTarget.GetBoneTransform(HumanBodyBones.Head).position;
            float hmdYaw = _transformHMD.eulerAngles.y; 
            Quaternion qRotHMDYaw = Quaternion.AngleAxis( hmdYaw, Vector3.up );
            Quaternion qRotOffsetL = Quaternion.Euler( _handEulerOffsetL );
            Quaternion qRotOffsetR = Quaternion.Euler( _handEulerOffsetR );

            if (_enableHand) {
                // left hand
                Transform lsrc = _transformVirtualLController;
                Vector3 posBeforYawAdjust = lsrc.position - hmdPos + _handPosOffsetL;
                _transformLController.position = qRotHMDYaw * posBeforYawAdjust + _transformHMD.position;
                _transformLController.rotation = qRotHMDYaw * lsrc.rotation * qRotOffsetL;
                SendControllerTransform(true); // L

                // right hand
                Transform rsrc = _transformVirtualRController;
                posBeforYawAdjust = rsrc.position - hmdPos + _handPosOffsetR;
                _transformRController.position = qRotHMDYaw * posBeforYawAdjust + _transformHMD.position;
                _transformRController.rotation = qRotHMDYaw * rsrc.rotation * qRotOffsetR;
                SendControllerTransform(false); // R
            }

            if ( _enableHead ) {
                _opentrackClient.enabled = true;
                _opentrackClient.HeadObject = _animationTarget.GetBoneTransform(HumanBodyBones.Head);
            }
            else if ( !_enableHead  ) {
                _opentrackClient.enabled = false;
            }

            if (_enableWaistTrack) {
                Transform t = _animationTarget.GetBoneTransform(HumanBodyBones.Spine);// Hips? Chest? 要件等
                SendTrackerTransform(_VMTIndexWaist, t, hmdPos, qRotHMDYaw);
            }
            if (_enableLeftFootTrack) {
                Transform t = _animationTarget.GetBoneTransform(HumanBodyBones.LeftFoot);
                SendTrackerTransform(_VMTIndexLeftFoot, t, hmdPos, qRotHMDYaw);
            }
            if (_enableRightFootTrack) {
                Transform t = _animationTarget.GetBoneTransform(HumanBodyBones.RightFoot);
                SendTrackerTransform(_VMTIndexRightFoot, t, hmdPos, qRotHMDYaw);
            }
        }

#if VMT_HMD_OVERRIDE
        public void StartHMDOverride( bool isOn )
        {
            if ( _enableHead == false && isOn ) { // 完全オフからのオン
                _baseHMDPos = _transformHMD.position;
                _baseHMDRot = _transformHMD.rotation;
            }
            _enableHead = isOn;
        }
#endif

        void SendTrackerTransform( int vmtIndex, Transform t, Vector3 hmdPos, Quaternion qRotHMDYaw )
        {
            Vector3 posBeforYawAdjust = t.position - hmdPos; // 暫定offset なし
            Vector3 posAfterYawAdjust = qRotHMDYaw * posBeforYawAdjust + _transformHMD.position;
            Quaternion qRot = qRotHMDYaw * t.rotation;
            SendTrackerTransform(vmtIndex, posAfterYawAdjust, qRot);
        }

        void SendTrackerTransform(int vmtIndex, Vector3 pos, Quaternion q ) {
            _client.Send("/VMT/Room/Unity", 
                vmtIndex, _VMTEnableCompatibleTracker, (float)0,
                (float)pos.x, (float)pos.y, (float)pos.z, (float)q.x, (float)q.y, (float)q.z, (float)q.w );
        }
        void SendTrackerTransform(int vmtIndex, Vector3 pos, Vector3 rot ) {
            Quaternion q = Quaternion.Euler(rot);
            SendTrackerTransform(vmtIndex, pos, q);
        }

        void SendControllerTransform(bool isLeft) {
            Vector3 pos =  (isLeft) ? _transformLController.position : _transformRController.position;
            Quaternion q =  (isLeft) ? _transformLController.rotation : _transformRController.rotation;

            _client.Send("/VMT/Room/Unity", 
                isLeft? _VMTIndexLeft:_VMTIndexRight, 
                isLeft?_VMTEnableLeft:_VMTEnableRight, (float)0,
                (float)pos.x, (float)pos.y, (float)pos.z, (float)q.x, (float)q.y, (float)q.z, (float)q.w );
        }
    };   // class end
}
