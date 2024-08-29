// Copyright (c) 2024 Sakura(さくら) / tbbsakura
// MIT License. See "LICENSE" file.

using UnityEngine;
using uOSC;

namespace SakuraScript.VBTTool
{
    public class VBTHandPosTrack : MonoBehaviour
    {
        
        [SerializeField, Tooltip("OSCServer Receiving from VMT. VMTからの情報を受け取るOSCサーバー")]
        uOscServer _server;
        [SerializeField, Tooltip("OSCServer Sending to VMT. VMTへ情報を送信するOSCクライアント")]
        uOscClient _client;

        bool _isOn = false;

        Animator _animationTarget;
        public Animator AnimationTarget {
            get => _animationTarget;
            set => _animationTarget = value;
        }

        [SerializeField, Tooltip("VRMモデルが持つ左コントローラーオブジェクト")]
        Transform _transformVirtualLController;
        public Transform TransformVirtualLController {
            get => _transformVirtualLController; set => _transformVirtualLController = value;
        }
        [SerializeField, Tooltip("VRMモデルが持つ右コントローラーオブジェクト")]
        Transform _transformVirtualRController;
        public Transform TransformVirtualRController {
            get => _transformVirtualRController; set => _transformVirtualRController = value;
        }

        [SerializeField, Tooltip("VMTから受信したHMDのTransformを設定するオブジェクト")]
        Transform _transformHMD;
        [SerializeField, Tooltip("VMTに送信する左コントローラーのTransformを設定するオブジェクト")]
        Transform _transformLController;
        [SerializeField, Tooltip("VMTに送信する右コントローラーのTransformを設定するオブジェクト")]
        Transform _transformRController;

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

        [SerializeField, Tooltip("VMTに渡すパラメーター/左手のindex")]
        int _VMTIndexLeft = 1;
        [SerializeField, Tooltip("VMTに渡すパラメーター/左手のenable")]
        int _VMTEnableLeft = 5;
        [SerializeField, Tooltip("VMTに渡すパラメーター/右手のindex")]
        int _VMTIndexRight = 2;
        [SerializeField, Tooltip("VMTに渡すパラメーター/右手のenable")]
        int _VMTEnableRight = 6;

        readonly string _serialHMD = "HMD";

        float _rxLED = 0.0f;
        public float RxLED {
            get{ return _rxLED; }
        }

        // Start is called before the first frame update
        void Start()
        {
            _server.onDataReceived.AddListener(OnDataReceived);
        }

        void OnDisable()
        {
            if (_client != null && _isOn) StopTrack();
        }

        public void StartTrack(int vmtListenPort)
        {
            if (_client != null) {
                _client.Send("/VMT/Set/Destination", "127.0.0.1", vmtListenPort );
                _client.Send("/VMT/Subscribe/Device", _serialHMD);
                _isOn = true;
            }
        }

        public void StopTrack()
        {
            if (_client != null) _client.Send("/VMT/Unsubscribe/Device", _serialHMD);
            _isOn = false;
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

            if ( !_isOn ) return;
            if ( _animationTarget == null ) return;


            float hmdYaw = _transformHMD.eulerAngles.y; 
            Quaternion qRotHMDYaw = Quaternion.AngleAxis( hmdYaw, Vector3.up );
            Quaternion qRotOffsetL = Quaternion.Euler( _handEulerOffsetL );
            Quaternion qRotOffsetR = Quaternion.Euler( _handEulerOffsetR );

            // left hand
            Transform lsrc = _transformVirtualLController;
            Vector3 pos = lsrc.position 
                            - _animationTarget.GetBoneTransform(HumanBodyBones.Head).position
                            + _handPosOffsetL;
            _transformLController.position = qRotHMDYaw * pos + _transformHMD.position;
            _transformLController.rotation = qRotHMDYaw * lsrc.rotation * qRotOffsetL;
            SendControllerTransform(true); // L

            // right hand
            Transform rsrc = _transformVirtualRController;
            pos = rsrc.position 
                        - _animationTarget.GetBoneTransform(HumanBodyBones.Head).position
                        + _handPosOffsetR;
            _transformRController.position = qRotHMDYaw * pos + _transformHMD.position;
            _transformRController.rotation = qRotHMDYaw * rsrc.rotation * qRotOffsetR;
            SendControllerTransform(false); // R
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
