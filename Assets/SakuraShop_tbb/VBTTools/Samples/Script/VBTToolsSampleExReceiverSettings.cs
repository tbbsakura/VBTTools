
using EVMC4U;

namespace SakuraScript.VBTTool
{
    [System.Serializable]
    public class ExRecSetting {
        //[SerializeField, Label("ルート位置反映")]
        public bool RootPositionSynchronize = true; //ルート座標同期(ルームスケール移動)
        //[SerializeField, Label("ルート回転反映")]
        public bool RootRotationSynchronize = true; //ルート回転同期

        //[SerializeField, Label("CutBone 有効")]
        public bool CutBonesEnable = false;

        //[Header("Cut bones(Head)")]
        public bool CutBoneNeck = false;
        public bool CutBoneHead = false;
        public bool CutBoneLeftEye = true;
        public bool CutBoneRightEye = true;
        public bool CutBoneJaw = false;

        //[Header("Cut bones(Body)")]
        public bool CutBoneHips = true;
        public bool CutBoneSpine = true;
        public bool CutBoneChest = true;
        public bool CutBoneUpperChest = true;

        // [Header("Cut bones(Left Arm)")]
        public bool CutBoneLeftShoulder = false;
        public bool CutBoneLeftUpperArm = false;
        public bool CutBoneLeftLowerArm = false;
        public bool CutBoneLeftHand = false;

        // [Header("Cut bones(Right Arm)")]
        public bool CutBoneRightShoulder = false;
        public bool CutBoneRightUpperArm = false;
        public bool CutBoneRightLowerArm = false;
        public bool CutBoneRightHand = false;

        // [Header("Cut bones(Left Leg)")]
        public bool CutBoneLeftUpperLeg = true;
        public bool CutBoneLeftLowerLeg = true;
        public bool CutBoneLeftFoot = true;
        public bool CutBoneLeftToes = true;

        //[Header("Cut bones(Right Leg)")]
        public bool CutBoneRightUpperLeg = true;
        public bool CutBoneRightLowerLeg = true;
        public bool CutBoneRightFoot = true;
        public bool CutBoneRightToes = true;

        public bool CutBoneLeftThumbProximal = false;
        public bool CutBoneLeftThumbIntermediate = false;
        public bool CutBoneLeftThumbDistal = false;
        public bool CutBoneLeftIndexProximal = false;
        public bool CutBoneLeftIndexIntermediate = false;
        public bool CutBoneLeftIndexDistal = false;
        public bool CutBoneLeftMiddleProximal = false;
        public bool CutBoneLeftMiddleIntermediate = false;
        public bool CutBoneLeftMiddleDistal = false;
        public bool CutBoneLeftRingProximal = false;
        public bool CutBoneLeftRingIntermediate = false;
        public bool CutBoneLeftRingDistal = false;
        public bool CutBoneLeftLittleProximal = false;
        public bool CutBoneLeftLittleIntermediate = false;
        public bool CutBoneLeftLittleDistal = false;

        public bool CutBoneRightThumbProximal = false;
        public bool CutBoneRightThumbIntermediate = false;
        public bool CutBoneRightThumbDistal = false;
        public bool CutBoneRightIndexProximal = false;
        public bool CutBoneRightIndexIntermediate = false;
        public bool CutBoneRightIndexDistal = false;
        public bool CutBoneRightMiddleProximal = false;
        public bool CutBoneRightMiddleIntermediate = false;
        public bool CutBoneRightMiddleDistal = false;
        public bool CutBoneRightRingProximal = false;
        public bool CutBoneRightRingIntermediate = false;
        public bool CutBoneRightRingDistal = false;
        public bool CutBoneRightLittleProximal = false;
        public bool CutBoneRightLittleIntermediate = false;
        public bool CutBoneRightLittleDistal = false;

        public void CopyFromExReceiver ( ExternalReceiver ex ) {
            RootPositionSynchronize = ex.RootPositionSynchronize; 
            RootRotationSynchronize = ex.RootRotationSynchronize; 
            CutBonesEnable = ex.CutBonesEnable; 
            CutBoneNeck = ex.CutBoneNeck; 
            CutBoneHead = ex.CutBoneHead; 
            CutBoneLeftEye = ex.CutBoneLeftEye; 
            CutBoneRightEye = ex.CutBoneRightEye; 
            CutBoneJaw = ex.CutBoneJaw; 
            CutBoneHips = ex.CutBoneHips; 
            CutBoneSpine = ex.CutBoneSpine; 
            CutBoneChest = ex.CutBoneChest; 
            CutBoneUpperChest = ex.CutBoneUpperChest; 
            CutBoneLeftShoulder = ex.CutBoneLeftShoulder; 
            CutBoneLeftUpperArm = ex.CutBoneLeftUpperArm; 
            CutBoneLeftLowerArm = ex.CutBoneLeftLowerArm; 
            CutBoneLeftHand = ex.CutBoneLeftHand; 
            CutBoneRightShoulder = ex.CutBoneRightShoulder; 
            CutBoneRightUpperArm = ex.CutBoneRightUpperArm; 
            CutBoneRightLowerArm = ex.CutBoneRightLowerArm; 
            CutBoneRightHand = ex.CutBoneRightHand; 
            CutBoneLeftUpperLeg = ex.CutBoneLeftUpperLeg; 
            CutBoneLeftLowerLeg = ex.CutBoneLeftLowerLeg; 
            CutBoneLeftFoot = ex.CutBoneLeftFoot; 
            CutBoneLeftToes = ex.CutBoneLeftToes; 
            CutBoneRightUpperLeg = ex.CutBoneRightUpperLeg; 
            CutBoneRightLowerLeg = ex.CutBoneRightLowerLeg; 
            CutBoneRightFoot = ex.CutBoneRightFoot; 
            CutBoneRightToes = ex.CutBoneRightToes; 

            CutBoneLeftThumbProximal = ex.CutBoneLeftThumbProximal; 
            CutBoneLeftThumbIntermediate = ex.CutBoneLeftThumbIntermediate; 
            CutBoneLeftThumbDistal = ex.CutBoneLeftThumbDistal; 
            CutBoneLeftIndexProximal = ex.CutBoneLeftIndexProximal; 
            CutBoneLeftIndexIntermediate = ex.CutBoneLeftIndexIntermediate; 
            CutBoneLeftIndexDistal = ex.CutBoneLeftIndexDistal; 
            CutBoneLeftMiddleProximal = ex.CutBoneLeftMiddleProximal; 
            CutBoneLeftMiddleIntermediate = ex.CutBoneLeftMiddleIntermediate; 
            CutBoneLeftMiddleDistal = ex.CutBoneLeftMiddleDistal; 
            CutBoneLeftRingProximal = ex.CutBoneLeftRingProximal; 
            CutBoneLeftRingIntermediate = ex.CutBoneLeftRingIntermediate; 
            CutBoneLeftRingDistal = ex.CutBoneLeftRingDistal; 
            CutBoneLeftLittleProximal = ex.CutBoneLeftLittleProximal; 
            CutBoneLeftLittleIntermediate = ex.CutBoneLeftLittleIntermediate; 
            CutBoneLeftLittleDistal = ex.CutBoneLeftLittleDistal; 
            CutBoneRightThumbProximal = ex.CutBoneRightThumbProximal; 
            CutBoneRightThumbIntermediate = ex.CutBoneRightThumbIntermediate; 
            CutBoneRightThumbDistal = ex.CutBoneRightThumbDistal; 
            CutBoneRightIndexProximal = ex.CutBoneRightIndexProximal; 
            CutBoneRightIndexIntermediate = ex.CutBoneRightIndexIntermediate; 
            CutBoneRightIndexDistal = ex.CutBoneRightIndexDistal; 
            CutBoneRightMiddleProximal = ex.CutBoneRightMiddleProximal; 
            CutBoneRightMiddleIntermediate = ex.CutBoneRightMiddleIntermediate; 
            CutBoneRightMiddleDistal = ex.CutBoneRightMiddleDistal; 
            CutBoneRightRingProximal = ex.CutBoneRightRingProximal; 
            CutBoneRightRingIntermediate = ex.CutBoneRightRingIntermediate; 
            CutBoneRightRingDistal = ex.CutBoneRightRingDistal; 
            CutBoneRightLittleProximal = ex.CutBoneRightLittleProximal; 
            CutBoneRightLittleIntermediate = ex.CutBoneRightLittleIntermediate; 
            CutBoneRightLittleDistal = ex.CutBoneRightLittleDistal; 
        }
 
        public void CopyToExReceiver ( ExternalReceiver ex ) {
            ex.RootPositionSynchronize = RootPositionSynchronize; 
            ex.RootRotationSynchronize = RootRotationSynchronize; 
            ex.CutBonesEnable = CutBonesEnable; 
            ex.CutBoneNeck = CutBoneNeck; 
            ex.CutBoneHead = CutBoneHead; 
            ex.CutBoneLeftEye = CutBoneLeftEye; 
            ex.CutBoneRightEye = CutBoneRightEye; 
            ex.CutBoneJaw = CutBoneJaw; 
            ex.CutBoneHips = CutBoneHips; 
            ex.CutBoneSpine = CutBoneSpine; 
            ex.CutBoneChest = CutBoneChest; 
            ex.CutBoneUpperChest = CutBoneUpperChest; 
            ex.CutBoneLeftShoulder = CutBoneLeftShoulder; 
            ex.CutBoneLeftUpperArm = CutBoneLeftUpperArm; 
            ex.CutBoneLeftLowerArm = CutBoneLeftLowerArm; 
            ex.CutBoneLeftHand = CutBoneLeftHand; 
            ex.CutBoneRightShoulder = CutBoneRightShoulder; 
            ex.CutBoneRightUpperArm = CutBoneRightUpperArm; 
            ex.CutBoneRightLowerArm = CutBoneRightLowerArm; 
            ex.CutBoneRightHand = CutBoneRightHand; 
            ex.CutBoneLeftUpperLeg = CutBoneLeftUpperLeg; 
            ex.CutBoneLeftLowerLeg = CutBoneLeftLowerLeg; 
            ex.CutBoneLeftFoot = CutBoneLeftFoot; 
            ex.CutBoneLeftToes = CutBoneLeftToes; 
            ex.CutBoneRightUpperLeg = CutBoneRightUpperLeg; 
            ex.CutBoneRightLowerLeg = CutBoneRightLowerLeg; 
            ex.CutBoneRightFoot = CutBoneRightFoot; 
            ex.CutBoneRightToes = CutBoneRightToes; 
            ex.CutBoneLeftThumbProximal = CutBoneLeftThumbProximal; 
            ex.CutBoneLeftThumbIntermediate = CutBoneLeftThumbIntermediate; 
            ex.CutBoneLeftThumbDistal = CutBoneLeftThumbDistal; 
            ex.CutBoneLeftIndexProximal = CutBoneLeftIndexProximal; 
            ex.CutBoneLeftIndexIntermediate = CutBoneLeftIndexIntermediate; 
            ex.CutBoneLeftIndexDistal = CutBoneLeftIndexDistal; 
            ex.CutBoneLeftMiddleProximal = CutBoneLeftMiddleProximal; 
            ex.CutBoneLeftMiddleIntermediate = CutBoneLeftMiddleIntermediate; 
            ex.CutBoneLeftMiddleDistal = CutBoneLeftMiddleDistal; 
            ex.CutBoneLeftRingProximal = CutBoneLeftRingProximal; 
            ex.CutBoneLeftRingIntermediate = CutBoneLeftRingIntermediate; 
            ex.CutBoneLeftRingDistal = CutBoneLeftRingDistal; 
            ex.CutBoneLeftLittleProximal = CutBoneLeftLittleProximal; 
            ex.CutBoneLeftLittleIntermediate = CutBoneLeftLittleIntermediate; 
            ex.CutBoneLeftLittleDistal = CutBoneLeftLittleDistal; 
            ex.CutBoneRightThumbProximal = CutBoneRightThumbProximal; 
            ex.CutBoneRightThumbIntermediate = CutBoneRightThumbIntermediate; 
            ex.CutBoneRightThumbDistal = CutBoneRightThumbDistal; 
            ex.CutBoneRightIndexProximal = CutBoneRightIndexProximal; 
            ex.CutBoneRightIndexIntermediate = CutBoneRightIndexIntermediate; 
            ex.CutBoneRightIndexDistal = CutBoneRightIndexDistal; 
            ex.CutBoneRightMiddleProximal = CutBoneRightMiddleProximal; 
            ex.CutBoneRightMiddleIntermediate = CutBoneRightMiddleIntermediate; 
            ex.CutBoneRightMiddleDistal = CutBoneRightMiddleDistal; 
            ex.CutBoneRightRingProximal = CutBoneRightRingProximal; 
            ex.CutBoneRightRingIntermediate = CutBoneRightRingIntermediate; 
            ex.CutBoneRightRingDistal = CutBoneRightRingDistal; 
            ex.CutBoneRightLittleProximal = CutBoneRightLittleProximal; 
            ex.CutBoneRightLittleIntermediate = CutBoneRightLittleIntermediate; 
            ex.CutBoneRightLittleDistal = CutBoneRightLittleDistal; 
        }

        public void SetRootSyncIsOn( bool isOn ) {
            RootPositionSynchronize = isOn;
            RootRotationSynchronize = isOn;
        }

        public void SetCutBonesIsOn( bool isOn ) {
            CutBonesEnable = isOn;
        }

        public void SetCutEyeIsOn( bool isOn ) {
            CutBoneLeftEye = isOn;
            CutBoneRightEye = isOn;
        }

        public void SetCutHeadNeckIsOn( bool isOn ) {
            CutBoneNeck = isOn;
            CutBoneHead = isOn;
            CutBoneJaw = isOn;
        }

        public void SetCutTorsoIsOn( bool isOn ) {
            CutBoneHips = isOn;
            CutBoneSpine = isOn;
            CutBoneChest = isOn;
            CutBoneUpperChest = isOn;
        }

        public void SetCutArmHandIsOn( bool isOn ) {
            CutBoneLeftShoulder = isOn;
            CutBoneLeftUpperArm = isOn;
            CutBoneLeftLowerArm = isOn;
            CutBoneLeftHand = isOn;
            CutBoneRightShoulder = isOn;
            CutBoneRightUpperArm = isOn;
            CutBoneRightLowerArm = isOn;
            CutBoneRightHand = isOn;
        }

        public void SetCutLegFootIsOn( bool isOn ) {
            CutBoneLeftUpperLeg = isOn;
            CutBoneLeftLowerLeg = isOn;
            CutBoneLeftFoot = isOn;
            CutBoneLeftToes = isOn;
            CutBoneRightUpperLeg = isOn;
            CutBoneRightLowerLeg = isOn;
            CutBoneRightFoot = isOn;
            CutBoneRightToes = isOn;
        }
        
        public void SetCutSkeletalIsOn( bool isOn ) {
            CutBoneLeftThumbProximal = isOn;
            CutBoneLeftThumbIntermediate = isOn;
            CutBoneLeftThumbDistal = isOn;
            CutBoneLeftIndexProximal = isOn;
            CutBoneLeftIndexIntermediate = isOn;
            CutBoneLeftIndexDistal = isOn;
            CutBoneLeftMiddleProximal = isOn;
            CutBoneLeftMiddleIntermediate = isOn;
            CutBoneLeftMiddleDistal = isOn;
            CutBoneLeftRingProximal = isOn;
            CutBoneLeftRingIntermediate = isOn;
            CutBoneLeftRingDistal = isOn;
            CutBoneLeftLittleProximal = isOn;
            CutBoneLeftLittleIntermediate = isOn;
            CutBoneLeftLittleDistal = isOn;

            CutBoneRightThumbProximal = isOn;
            CutBoneRightThumbIntermediate = isOn;
            CutBoneRightThumbDistal = isOn;
            CutBoneRightIndexProximal = isOn;
            CutBoneRightIndexIntermediate = isOn;
            CutBoneRightIndexDistal = isOn;
            CutBoneRightMiddleProximal = isOn;
            CutBoneRightMiddleIntermediate = isOn;
            CutBoneRightMiddleDistal = isOn;
            CutBoneRightRingProximal = isOn;
            CutBoneRightRingIntermediate = isOn;
            CutBoneRightRingDistal = isOn;
            CutBoneRightLittleProximal = isOn;
            CutBoneRightLittleIntermediate = isOn;
            CutBoneRightLittleDistal = isOn;
        }
 
    }

}