// Copyright (c) 2024 Sakura(さくら) / tbbsakura
// MIT License. 

using UnityEngine;

namespace OgLikeVMT
{
    public class OgLikeHandData {
        private bool _isLeftHand = false;
        private SkeletalBoneTransform [] _boneTransforms = new SkeletalBoneTransform[(int)HandSkeletonBone.COUNT];
        private FingerFlexion [] _flexion = new FingerFlexion[(int)FingerIndex.COUNT]; 
        private float [] _splay = new float[(int)FingerIndex.COUNT];

        private const float k_max_splay_angle = 20.0f; // OpenGloves
        float [] _maxSplayAngleHumanoid = new float [(int)FingerIndex.COUNT] { 25f, 20f, 7.5f, 7.5f, 20f };
        private OgLikeHandAnim _animCalc;

        // Hand-Root offset
        public Vector3 _skeletalRootPosOffset = Vector3.zero; 
        public Vector3 _skeletalRootRotOffset = Vector3.zero;
        // Root-Wrist offset
        public Vector3 _skeletalWristPosOffset = Vector3.zero; 
        public Vector3 _skeletalWristRotOffset = Vector3.zero; 

        ///////////////////////////////////////////////////////////////////////////////
        // constructor, initializer
        public OgLikeHandData( bool isLeft ) {  
            _isLeftHand = isLeft; 
            for ( int i = 0; i < (int)HandSkeletonBone.COUNT ; i++ ) {
                _boneTransforms[i] = new SkeletalBoneTransform();
            }
            for ( int i = 0; i < (int)FingerIndex.COUNT ; i++ ) {
                _flexion[i] = new FingerFlexion();
            }
        }

        ///////////////////////////////////////////////////////////////////////////////
        // if this is left hand, it returns converted value
        public void GetBoneTransform( ref SkeletalBoneTransform tfm,  int boneIndex ) {
            ComputeBoneTransforms();
            tfm._position = _boneTransforms[boneIndex]._position;
            tfm._rotation = _boneTransforms[boneIndex]._rotation;
        }

        public bool IsLeftHand { 
            get {return _isLeftHand;}
        }

        private void TransformLeftBone(ref Quaternion q, ref Vector3 pos, HandSkeletonBone bone_index) {
            switch (bone_index) {
                case HandSkeletonBone.Root: {
                    return;
                }
                case HandSkeletonBone.Thumb0:
                case HandSkeletonBone.IndexFinger0:
                case HandSkeletonBone.MiddleFinger0:
                case HandSkeletonBone.RingFinger0:
                case HandSkeletonBone.PinkyFinger0: { // RootFingerBones
                    Quaternion quat = q;
                    q.w = -quat.x;
                    q.x = quat.w;
                    q.y = -quat.z;
                    q.z = quat.y;
                    break;
                }
                case HandSkeletonBone.Wrist:
                case HandSkeletonBone.AuxIndexFinger:
                case HandSkeletonBone.AuxThumb:
                case HandSkeletonBone.AuxMiddleFinger:
                case HandSkeletonBone.AuxRingFinger:
                case HandSkeletonBone.AuxPinkyFinger: {
                    q.y *= -1;
                    q.z *= -1;
                    break;
                }
                default: {
                    pos.y *= -1;
                    pos.z *= -1;
                    break;
                }
            }
            pos.x *= -1;
        }

        ///////////////////////////////////////////////////////////////////////////////
        // value-setting functions
        public void SetSplay( FingerIndex idx, float value ) { // og's (AB) (BB) (CB) (DB) and (EB)
            if ( !(idx >= FingerIndex.Thumb && idx < FingerIndex.COUNT) ) return;
            _splay[(int)idx] = value;
            //Debug.Log( "SetSplay: " + idx.ToString() + " = " + value.ToString());
        }

        public void SetJointFlexion( FingerIndex idx, int jointIndex, float value ) { // e.g. og's (AAB)
            if ( !(idx >= FingerIndex.Thumb && idx < FingerIndex.COUNT) ) return;
            if ( !(jointIndex >= 0 && jointIndex < FingerFlexion.FLEXION_COUNT ) ) return;
            _flexion[(int)idx]._values[jointIndex] = value;
        }

        public void SetFingerScalarFlexion( FingerIndex idx ) {
            SetFingerScalarFlexion( idx, GetAverageFingerCurlValue(idx) );
        }

        public void SetFingerScalarFlexion( FingerIndex idx, float value ) { // og's A B C D E
            if ( !(idx >= FingerIndex.Thumb && idx < FingerIndex.COUNT) ) return;
            for ( int i = 0 ; i < FingerFlexion.FLEXION_COUNT; i ++ ) {
                _flexion[(int)idx]._values[i] = value;
            }
            //Debug.Log($"SetFingerScalarFlexion value = {value}");
            //DebugPrintFlexion();
        }

        private string _lastFlexionlog = "";
        public void DebugPrintFlexion()
        {
            string tex = "";
            for ( int i = 0; i < (int)FingerIndex.COUNT ; i++ ) {
                for (int j = 0 ; j < FingerFlexion.FLEXION_COUNT; j ++ ) {
                    tex += $"{_flexion[i]._values[j]},";
                }
                tex += "/";
            }
            if ( tex != _lastFlexionlog ) {
                Debug.Log( tex );
                _lastFlexionlog = tex;
            }
        }

        public void InitBones(OgLikeHandAnim anim) {
            _animCalc = anim;
            Vector3 v3 = new Vector3();
            Quaternion q = Quaternion.identity;
            for ( int i = 0; i < (int)HandSkeletonBone.COUNT; i++ ) {
                v3.x = _animCalc.GetBoneCurveEvaluateI( i, BoneCurveType.PosX, 0.0f );
                v3.y = _animCalc.GetBoneCurveEvaluateI( i, BoneCurveType.PosY, 0.0f );
                v3.z = _animCalc.GetBoneCurveEvaluateI( i, BoneCurveType.PosZ, 0.0f );
                q.x = _animCalc.GetBoneCurveEvaluateI( i, BoneCurveType.RotX, 0.0f );
                q.y = _animCalc.GetBoneCurveEvaluateI( i, BoneCurveType.RotY, 0.0f );
                q.z = _animCalc.GetBoneCurveEvaluateI( i, BoneCurveType.RotZ, 0.0f );
                q.w = _animCalc.GetBoneCurveEvaluateI( i, BoneCurveType.RotW, 0.0f );

                _boneTransforms[i]._position = v3;
                _boneTransforms[i]._rotation = q;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////
        // general information functions : public static
        public static HandSkeletonBone GetRootFingerBoneFromFingerIndex(FingerIndex finger) {
            switch (finger) {
                case FingerIndex.Thumb:        return HandSkeletonBone.Thumb0;
                case FingerIndex.IndexFinger:  return  HandSkeletonBone.IndexFinger0;
                case FingerIndex.MiddleFinger: return  HandSkeletonBone.MiddleFinger0;
                case FingerIndex.RingFinger:   return  HandSkeletonBone.RingFinger0;
                case FingerIndex.PinkyFinger:  return  HandSkeletonBone.PinkyFinger0;
                default:  return  HandSkeletonBone.Unknown;
            }
        }

        public static HandSkeletonBone GetEndFingerBoneFromFingerIndex(FingerIndex finger) {
            switch (finger) {
                case FingerIndex.Thumb:        return HandSkeletonBone.Thumb3;
                case FingerIndex.IndexFinger:  return  HandSkeletonBone.IndexFinger4;
                case FingerIndex.MiddleFinger: return  HandSkeletonBone.MiddleFinger4;
                case FingerIndex.RingFinger:   return  HandSkeletonBone.RingFinger4;
                case FingerIndex.PinkyFinger:  return  HandSkeletonBone.PinkyFinger4;
                default:  return  HandSkeletonBone.Unknown;
            }
        }

        public static HandSkeletonBone GetAuxFingerBoneFromFingerIndex(FingerIndex finger) {
            switch (finger) {
                case FingerIndex.Thumb:        return HandSkeletonBone.AuxThumb;
                case FingerIndex.IndexFinger:  return  HandSkeletonBone.AuxIndexFinger;
                case FingerIndex.MiddleFinger: return  HandSkeletonBone.AuxMiddleFinger;
                case FingerIndex.RingFinger:   return  HandSkeletonBone.AuxRingFinger;
                case FingerIndex.PinkyFinger:  return  HandSkeletonBone.AuxPinkyFinger;
                default:  return  HandSkeletonBone.Unknown;
            }
        } 

        public static bool IsAuxBone(HandSkeletonBone boneIndex) {
            return boneIndex == HandSkeletonBone.AuxThumb || 
                    boneIndex == HandSkeletonBone.AuxIndexFinger || 
                    boneIndex == HandSkeletonBone.AuxMiddleFinger ||
                    boneIndex == HandSkeletonBone.AuxRingFinger || 
                    boneIndex == HandSkeletonBone.AuxPinkyFinger;
        }   

        public static bool IsBoneSplayable(HandSkeletonBone bone) {
            return bone == HandSkeletonBone.Thumb0 ||
                bone == HandSkeletonBone.IndexFinger1 || 
                bone == HandSkeletonBone.MiddleFinger1 ||
                bone == HandSkeletonBone.RingFinger1 || 
                bone == HandSkeletonBone.PinkyFinger1;
        }

        public static FingerIndex GetFingerFromBoneIndex( HandSkeletonBone bone ) {
            switch (bone) {
                case HandSkeletonBone.Thumb0:
                case HandSkeletonBone.Thumb1:
                case HandSkeletonBone.Thumb2:
                case HandSkeletonBone.AuxThumb:
                return FingerIndex.Thumb;

                case HandSkeletonBone.IndexFinger0:
                case HandSkeletonBone.IndexFinger1:
                case HandSkeletonBone.IndexFinger2:
                case HandSkeletonBone.IndexFinger3:
                case HandSkeletonBone.AuxIndexFinger:
                return FingerIndex.IndexFinger;

                case HandSkeletonBone.MiddleFinger0:
                case HandSkeletonBone.MiddleFinger1:
                case HandSkeletonBone.MiddleFinger2:
                case HandSkeletonBone.MiddleFinger3:
                case HandSkeletonBone.AuxMiddleFinger:
                return FingerIndex.MiddleFinger;

                case HandSkeletonBone.RingFinger0:
                case HandSkeletonBone.RingFinger1:
                case HandSkeletonBone.RingFinger2:
                case HandSkeletonBone.RingFinger3:
                case HandSkeletonBone.AuxRingFinger:
                return FingerIndex.RingFinger;

                case HandSkeletonBone.PinkyFinger0:
                case HandSkeletonBone.PinkyFinger1:
                case HandSkeletonBone.PinkyFinger2:
                case HandSkeletonBone.PinkyFinger3:
                case HandSkeletonBone.AuxPinkyFinger:
                return FingerIndex.PinkyFinger;

                default:
                return FingerIndex.Unknown;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////
        // calc functions
        public float GetAverageFingerCurlValue( FingerIndex idx ) { 
            if ( idx == FingerIndex.Unknown || idx >= FingerIndex.COUNT ) return 0.0f;
            FingerFlexion f = _flexion[(int)idx];
            float sum = 0.0f;
            for ( int i = 0; i < FingerFlexion.FLEXION_COUNT; i++ )  {
                sum += f._values[i];
            }           
            if (idx ==FingerIndex.IndexFinger &&_isLeftHand) {
                //Debug.Log($"GetAverageFingerCurlValue: avg curl: {idx}, {sum/((float)FingerFlexion.FLEXION_COUNT)}");
//                DebugPrintFlexion();
            }
            return sum/((float)FingerFlexion.FLEXION_COUNT);
        }

        void SetTransformForBone( int figerIndex, int boneIndex, float curl, float splay ) {
            // OpenGloves Original params vr::VRBoneTransform_t& bone, const HandSkeletonBone& boneIndex, float curl, float splay, bool rightHand) 
            // We don't clamp this, as chances are if it's invalid we don't really want to use it anyway.
            //if ( curl < 0.0f && curl > 1.0f) return;

            if ( curl < 0.0f ) curl = 0.0f;
            if ( curl > 1.0f ) curl = 1.0f;
            SkeletalBoneTransform bone = _boneTransforms[boneIndex];
            bone._position.x = _animCalc.GetBoneCurveEvaluateI( boneIndex, BoneCurveType.PosX, curl );
            bone._position.y = _animCalc.GetBoneCurveEvaluateI( boneIndex, BoneCurveType.PosY, curl );
            bone._position.z = _animCalc.GetBoneCurveEvaluateI( boneIndex, BoneCurveType.PosZ, curl );
            bone._rotation.x = _animCalc.GetBoneCurveEvaluateI( boneIndex, BoneCurveType.RotX, curl );
            bone._rotation.y = _animCalc.GetBoneCurveEvaluateI( boneIndex, BoneCurveType.RotY, curl );
            bone._rotation.z = _animCalc.GetBoneCurveEvaluateI( boneIndex, BoneCurveType.RotZ, curl );
            bone._rotation.w = _animCalc.GetBoneCurveEvaluateI( boneIndex, BoneCurveType.RotW, curl );

            bool b = IsBoneSplayable((HandSkeletonBone)boneIndex);
            if (b) {
                // Debug.Log( $"SetTransformForBone: splay: {splay}, boneIndex: {boneIndex}, Splayable: {b}");
//                if (splay >= -1.0f && splay <= 1.0f) {
                // only splay one bone (all the rest are done relative to this one)
                // original rotate code: bone.orientation = bone.orientation * EulerToQuaternion(0.0, DEG_TO_RAD(splay * k_max_splay_angle), 0.0);
                    //bone._rotation *= Quaternion.AngleAxis(splay * k_max_splay_angle, Vector3.up); 
                    bone._rotation *= Quaternion.AngleAxis(splay * _maxSplayAngleHumanoid[figerIndex], Vector3.up); 
//                }
            }

            if (_isLeftHand) TransformLeftBone(ref bone._rotation, ref bone._position, (HandSkeletonBone)boneIndex);
        }

        void ComputeBoneTransforms()
        {
            // Root and Wrist
            _boneTransforms[0]._position = _skeletalRootPosOffset;
            _boneTransforms[0]._rotation = Quaternion.Euler(_skeletalRootRotOffset);
            _boneTransforms[1]._position = _skeletalWristPosOffset; 
            _boneTransforms[1]._rotation = Quaternion.Euler(_skeletalWristRotOffset);

            // five fingers
            for (int i = 2; i < (int)HandSkeletonBone.COUNT; i++) { 
                FingerIndex finger = GetFingerFromBoneIndex((HandSkeletonBone)i);
                int iFinger = (int)finger;
                if (finger == FingerIndex.Unknown ) continue; 

                float curl;
                if (IsAuxBone((HandSkeletonBone)i)) {
                    curl = GetAverageFingerCurlValue(finger); 
                }
                else {
                    int n = i - (int)GetRootFingerBoneFromFingerIndex(finger);
                    curl = _flexion[iFinger]._values[n]; // 該当の指の fexion
                    //if ( finger == FingerIndex.Thumb ) Debug.Log( string.Format("ComputeBoneTransforms: boneIndex(i) = {0}, n = {1}, curl = {2}", i, n, curl ));
                }

                float splay = _splay[iFinger]; 
                SetTransformForBone( iFinger, i, curl, splay );
            }
        }
    };
}

