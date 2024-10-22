﻿// Copyright (c) 2024 Sakura(さくら) / tbbsakura
// MIT License. See "LICENSE" file.

using UnityEngine;
using OgLikeVMT;

namespace SakuraScript.VBTTool
{
    public class VBTSkeletalTrack : MonoBehaviour
    {
        [SerializeField, Tooltip("VMT Client Component")]
        OgLikeVMTClient _vmtclient = null; // Inspector指定必須

        [SerializeField, Tooltip("Enable sending : 送出オンオフ切り替え")]
        bool _isOn = false;
        public bool IsOn {
            get => _isOn;
            set => _isOn = value;
        }

        Animator _animationTarget;
        public Animator AnimationTarget { 
            get => _animationTarget;
            set {
                _handler = new HumanPoseHandler( value.avatar, value.transform );
                _animationTarget = value;
            } 
        }

        [SerializeField, Tooltip("Scalar mode if checked : チェック時は一軸モード")]
        bool _scalarModeThumb = true;
        [SerializeField, Tooltip("Scalar mode if checked : チェック時は一軸モード")]
        bool _scalarModeIndex = false;
        [SerializeField, Tooltip("Scalar mode if checked : チェック時は一軸モード")]
        bool _scalarModeMiddle = false;
        [SerializeField, Tooltip("Scalar mode if checked : チェック時は一軸モード")]
        bool _scalarModeRing = false;
        [SerializeField, Tooltip("Scalar mode if checked : チェック時は一軸モード")]
        bool _scalarModePinky = false;

        HumanPoseHandler _handler;
        HumanPose _targetHumanPose;

        const int NUM_UNITY_FINGER_JOINT = 3;

        // Start is called before the first frame update
        void Start()
        {
        }

        /////////////////////////////////////////////////
        // Mode management funtions
        private bool GetScalarMode( int fingerIndex ) {
            switch (fingerIndex) {
                case 0: return _scalarModeThumb;
                case 1: return _scalarModeIndex;
                case 2: return _scalarModeMiddle;
                case 3: return _scalarModeRing;
                case 4: return _scalarModePinky;
            }
            return false;
        }

        private int GetScalarModeInt()
        {
            int r = 0;
            if (_scalarModeThumb)  r += 1 ;
            if (_scalarModeIndex)  r += 2 ;
            if (_scalarModeMiddle) r += 4 ;
            if (_scalarModeRing)   r += 8 ;
            if (_scalarModePinky)  r += 16;
            return r;
        }

        /////////////////////////////////////////////////
        // Skeletal Curl/Splay setting Functions
        public void SetJointCurl( bool left, float valOg, int fingerIndex, int jointIndex ) { 
            OgLikeHandData data = (left) ? _vmtclient.LeftHand : _vmtclient.RightHand;
            data.SetJointFlexion((FingerIndex)fingerIndex, jointIndex, valOg );
        }

        public void SetSplay( bool left, float valOg, int fingerIndex ) { 
            OgLikeHandData data = (left) ? _vmtclient.LeftHand : _vmtclient.RightHand;
            data.SetSplay((FingerIndex)fingerIndex, valOg );
        }

        public void SetRootWristOffset( bool left, Vector3 rootPos, Vector3 rootRotEuler, Vector3 wristPos, Vector3 wristRotEuler )
        {
            OgLikeHandData data = (left) ? _vmtclient.LeftHand : _vmtclient.RightHand;
            data.SkeletalRootPosOffset = rootPos;
            data.SkeletalRootRotOffset = rootRotEuler;
            data.SkeletalWristPosOffset = wristPos;
            data.SkeletalWristRotOffset = wristRotEuler;
        }

        public void GetRootWristOffset( bool left, ref Vector3 rootPos, ref Vector3 rootRotEuler, ref Vector3 wristPos, ref Vector3 wristRotEuler )
        {
            OgLikeHandData data = (left) ? _vmtclient.LeftHand : _vmtclient.RightHand;
            rootPos = data.SkeletalRootPosOffset;
            rootRotEuler = data.SkeletalRootRotOffset;
            wristPos = data.SkeletalWristPosOffset;
            wristRotEuler = data.SkeletalWristRotOffset;
        }

        /////////////////////////////////////////////////
        // Update functions
        void Update()
        {
            if (!_isOn) return;
            if (_vmtclient == null ) return;
            if ( _animationTarget == null ) return;

            if (_handler == null ) {
                _handler = new HumanPoseHandler( _animationTarget.avatar, _animationTarget.transform );
                if ( _handler == null ) {
                    return;
                }
            }

            _handler.GetHumanPose(ref _targetHumanPose);   
            for ( int leftright = 0; leftright < 2; leftright++ ) { // left is zero
                OgLikeHandData data = (leftright==0) ? _vmtclient.LeftHand : _vmtclient.RightHand;
                for ( int iFinger = 0; iFinger  < (int)FingerIndex.COUNT; iFinger++ ) {
                    float sumCurl = 0f;
                    for (int iJoint = 0; iJoint < NUM_UNITY_FINGER_JOINT; iJoint++ ) {
                        int musIndex = GetHumanoidStretchMuscleIndex(leftright==0, iFinger , iJoint);
                        float stretchValue = _targetHumanPose.muscles[musIndex];
                        float curl = 1.0f - (stretchValue / 2f + 0.5f); 
                        sumCurl += curl;
                        this.SetJointCurl( leftright == 0 , curl, iFinger, iJoint+(iFinger == 0 ? 0 : 1) );
                    }
                    if (GetScalarMode(iFinger)) {
                        float cnt = (float)NUM_UNITY_FINGER_JOINT;
                        data.SetFingerScalarFlexion((FingerIndex)iFinger, sumCurl / cnt );
                    }
                    int musIndexSpread = GetHumanoidSpreadMuscleIndex(leftright==0, iFinger );
                    float spreadValue = _targetHumanPose.muscles[musIndexSpread]; 
                    float splay = spreadValue * GetHumanoidSpreadSign(iFinger); 
                    this.SetSplay( leftright == 0, splay, iFinger ); 
                }
            }
            SendLeftData();
            SendRightData();
        }

        private void SendLeftData()  { if (_vmtclient != null ) _vmtclient.SendLeftHandData( GetScalarModeInt()); }
        private void SendRightData() { if (_vmtclient != null ) _vmtclient.SendRightHandData(GetScalarModeInt()); }


        /////////////////////////////////////////////////
        // Humanoid Info static functions
        public static int GetHumanoidSpreadMuscleIndex( bool left, int fingerIndex )
        {
            return (56 + 4 * fingerIndex) + (left ? 0 : 20);
        }

        public static int GetHumanoidStretchMuscleIndex( bool left, int fingerIndex, int humanoidJointIndex )
        {
            // humanoidJointIndex : 0,1,2 
            int n = (55 + 4 * fingerIndex) + (left ? 0 : 20);
            if (humanoidJointIndex != 0) n += humanoidJointIndex + 1;
            return n;
        }

        public static float GetHumanoidSpreadSign( int fingerIndex ) {
            switch (fingerIndex) {
                case 0: 
                    return 1f;
                case 1: 
                case 2: 
                    return -1f;
                case 3: 
                case 4: 
                    return 1f;
            }
            return -1f;
        }

    };   // class end
}
