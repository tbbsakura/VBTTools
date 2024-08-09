// Copyright (c) 2024 Sakura(さくら) / tbbsakura
// MIT License. See "LICENSE" file.

using UnityEngine;
using uOSC;

namespace SakuraScript.VBTTool
{
    public class VBTHandPosTrack : MonoBehaviour
    {
        
        [Tooltip("OSCServer Receiving from VMT. VMTからの情報を受け取るOSCサーバー")]
        public uOscServer _server;
        [Tooltip("OSCServer Sending to VMT. VMTへ情報を送信するOSCクライアント")]
        public uOscClient _client;

        private bool _isOn = false;

        [Tooltip("Animator of VRM to be tracked : トラッキングするVRMのAnimator")]
        public Animator _animationTarget;

        [Tooltip("VRMモデルが持つ左コントローラーオブジェクト")]
        public Transform _transformVirtualLController;
        [Tooltip("VRMモデルが持つ右コントローラーオブジェクト")]
        public Transform _transformVirtualRController;

        [Tooltip("VMTから受信したHMDのTransformを設定するオブジェクト")]
        public Transform _transformHMD;
        [Tooltip("VMTに送信する左コントローラーのTransformを設定するオブジェクト")]
        public Transform _transformLController;
        [Tooltip("VMTに送信する右コントローラーのTransformを設定するオブジェクト")]
        public Transform _transformRController;

        [Tooltip("左手のpre位置補正(VRM Local)")]
        public Vector3 _handPosOffsetL = new Vector3( 0f, 0f, 0f);
        [Tooltip("右手のpre位置補正(VRM Local)")]
        public Vector3 _handPosOffsetR = new Vector3( 0f, 0f, 0f);

        [Tooltip("左手の回転補正(Global)")]
        public Vector3 _handEulerOffsetL = new Vector3( 0,180,0 );
        [Tooltip("右手の回転補正(Global)")]
        public Vector3 _handEulerOffsetR = new Vector3( 180, 0 ,0 );

        [Tooltip("VMTに渡すパラメーター/左手のindex")]
        public int _VMTIndexLeft = 1;
        [Tooltip("VMTに渡すパラメーター/左手のenable")]
        public int _VMTEnableLeft = 5;
        [Tooltip("VMTに渡すパラメーター/右手のindex")]
        public int _VMTIndexRight = 2;
        [Tooltip("VMTに渡すパラメーター/右手のenable")]
        public int _VMTEnableRight = 6;

        private readonly string _serialHMD = "HMD";
        private float _rxLED = 0.0f;

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
            SendHandTransform(true); // L

            // right hand
            Transform rsrc = _transformVirtualRController;
            pos = rsrc.position 
                        - _animationTarget.GetBoneTransform(HumanBodyBones.Head).position
                        + _handPosOffsetR;
            _transformRController.position = qRotHMDYaw * pos + _transformHMD.position;
            _transformRController.rotation = qRotHMDYaw * rsrc.rotation * qRotOffsetR;
            SendHandTransform(false); // R
        }

        void SendHandTransform(bool isLeft) {
            Vector3 pos =  (isLeft) ? _transformLController.position : _transformRController.position;
            Quaternion q =  (isLeft) ? _transformLController.rotation : _transformRController.rotation;

            _client.Send("/VMT/Room/Unity", 
                isLeft? _VMTIndexLeft:_VMTIndexRight, 
                isLeft?_VMTEnableLeft:_VMTEnableRight, (float)0,
                (float)pos.x, (float)pos.y, (float)pos.z, (float)q.x, (float)q.y, (float)q.z, (float)q.w );
        }
    };   // class end
}
