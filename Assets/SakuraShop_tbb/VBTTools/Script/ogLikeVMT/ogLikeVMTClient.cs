// Copyright (c) 2024 Sakura(さくら) / tbbsakura
// MIT License. 

using UnityEngine;
using uOSC;

namespace OgLikeVMT
{
    [RequireComponent(typeof(uOSC.uOscClient))]
    [AddComponentMenu("OSC/OgLikeVMTClient", 37)]
    public class OgLikeVMTClient  : MonoBehaviour {
        // OpenGloves has an animation for the purpose of bone transform calculation.
        // This clip must be a modified version of "Take 001" in glove_anim.glb.
        // Original Take 001 does not include properties that will not be changed, because pos/rot of them can be known from 3D model.
        // Modified anim need to include all pos/rot info of all 31 bones, so that this script can know all the info from a single source.
        public OgLikeHandAnim _ogLikeHandAnim; 

        public OgLikeHandData _leftHand = new OgLikeHandData(true);
        public OgLikeHandData _rightHand = new OgLikeHandData(false);

        private uOscClient _client;

        [System.NonSerialized]
        public HandSkeletonBone _logBoneIndex; // Unknown なら出力しない、 .COUNT の場合は全て

        public void OnEnable()
        {
            _logBoneIndex = HandSkeletonBone.Unknown;

            _client = GetComponent<uOscClient>();
            _ogLikeHandAnim = new OgLikeHandAnim();
            _leftHand.InitBones(_ogLikeHandAnim);
            _rightHand.InitBones(_ogLikeHandAnim);
            SendLeftHandData(0);
            SendRightHandData(0);
        }

        private bool GetScalarMode(FingerIndex fi, int modeInt)
        {
            if (fi == FingerIndex.Unknown || fi >= FingerIndex.COUNT ) return false;
            return GetScalarMode((int)fi, modeInt);
        }

        private bool GetScalarMode(int iFinger, int modeInt)
        {
            return  ( (modeInt & (1 << iFinger)) > 0 );
        }

        // Send 31 bones' pos/rot and  HandBone's pos/rot         
        public void SendLeftHandData(int scalarModeInt) {
            SendOgLikeHandDataToVMT( _leftHand, scalarModeInt ); 
        }
        public void SendRightHandData(int scalarModeInt) { 
            SendOgLikeHandDataToVMT( _rightHand, scalarModeInt ); 
        }

        public void SendOgLikeHandDataToVMT( OgLikeHandData data, int scalarModeInt )
        {
            for ( int i = 0; i < (int)HandSkeletonBone.COUNT; i++ ) {
                SkeletalBoneTransform tfm = new SkeletalBoneTransform();
                data.GetBoneTransform( ref tfm, i );
                FingerIndex fi = OgLikeHandData.GetFingerFromBoneIndex((HandSkeletonBone)i);
                bool scalarMode = GetScalarMode(fi, scalarModeInt);
                if (scalarMode) {
                    if (OgLikeHandData.GetRootFingerBoneFromFingerIndex(fi) == (HandSkeletonBone)i) {
                        float value = data.GetAverageFingerCurlValue(fi);
                        _client.Send("/VMT/Skeleton/Scalar",  data.IsLeftHand? 1 : 2, (int)fi+1, 1f - value, 0, 0);
                    }
                    if ( OgLikeHandData.IsBoneSplayable((HandSkeletonBone)i) ) {
                        _client.Send("/VMT/Skeleton/Unity", data.IsLeftHand? 1 : 2, (int)i,
                            (float)tfm._position.x, (float)tfm._position.y, (float)tfm._position.z,
                            (float)tfm._rotation.x, (float)tfm._rotation.y, (float)tfm._rotation.z, (float)tfm._rotation.w
                        );
                    }
                }
                else {
                    _client.Send("/VMT/Skeleton/Unity", data.IsLeftHand? 1 : 2, (int)i,
                        (float)tfm._position.x, (float)tfm._position.y, (float)tfm._position.z,
                        (float)tfm._rotation.x, (float)tfm._rotation.y, (float)tfm._rotation.z, (float)tfm._rotation.w
                    );
                }
            }
            
            _client.Send("/VMT/Skeleton/Apply", data.IsLeftHand? 1:2, (float)0);
        }
    };
}

