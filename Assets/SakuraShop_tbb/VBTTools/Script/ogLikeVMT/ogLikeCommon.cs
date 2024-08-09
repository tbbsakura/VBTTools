// Copyright (c) 2024 Sakura(さくら) / tbbsakura
// MIT License. 

using UnityEngine;

namespace OgLikeVMT
{
    public enum BoneCurveType {
        PosX, PosY, PosZ, RotX, RotY, RotZ, RotW, COUNT
    };
    
    public class BoneCurves {
        public AnimationCurve [] _curves = new AnimationCurve[(int)BoneCurveType.COUNT];
    };

    public enum FingerIndex{
        Thumb = 0,
        IndexFinger,
        MiddleFinger,
        RingFinger,
        PinkyFinger,
        COUNT,
        Unknown = -1
    };

    // OpenGloves において、flexion は5本の指について、各々4つの曲げ値を持っていて、
    // 指のルートボーン(GetRootFingerBoneFromFingerIndexで取れる)を0 として 0-3 までの曲げ数値を個別に持てる
    // このクラスは指1本分
    public class FingerFlexion {
        public const int FLEXION_COUNT = 4;
        public float[] _values = new float[FLEXION_COUNT];
    };

    public class SkeletalBoneTransform {
        public Vector3 _position;
        public Quaternion _rotation = Quaternion.identity;
    };

    public enum HandSkeletonBone {
        Root = 0,
        Wrist,
        Thumb0,
        Thumb1, 
        Thumb2, 
        Thumb3,
        IndexFinger0, 
        IndexFinger1, 
        IndexFinger2, 
        IndexFinger3, 
        IndexFinger4,
        MiddleFinger0, 
        MiddleFinger1, 
        MiddleFinger2, 
        MiddleFinger3, 
        MiddleFinger4,
        RingFinger0, 
        RingFinger1, 
        RingFinger2, 
        RingFinger3, 
        RingFinger4,
        PinkyFinger0, 
        PinkyFinger1, 
        PinkyFinger2, 
        PinkyFinger3, 
        PinkyFinger4,
        AuxThumb, 
        AuxIndexFinger, 
        AuxMiddleFinger, 
        AuxRingFinger, 
        AuxPinkyFinger,
        COUNT,
        Unknown = -1,
    };
}

