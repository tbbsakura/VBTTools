# OgLikeVMT
Og(OpenGloves) ライクに VMT(Virtual Motion Tracker)を使うためのライブラリです。
VMTは 31あるSkeletal Bones の位置および向きをパラメーターとして要求しますが、それらをどのように計算するかという問題があります。
一方でオープンソースの Og は指の関節ごとの曲げや、指の開きの値を受け取り、位置と向きを計算するコードを公開しています。
これを参考に、「関節ごとの曲げや、指の開きの値を指定して、VMT仮想Skeletal Inputコントローラーの指を動かす」ことを目的として開発しました。

2024/8/19 時点で、4つのファイルから構成されています。
- ogLikeVMTClient.cs 
- ogLikeHandData.cs
- ogLikeHandAnim.cs
- ogLikeCommon.cs

含まれるクラスの主な使い方は以下のとおりです。

## 1. class OgLikeVMTClient
このクラスと、このクラスが持つ左手/右手のメンバーを中心に操作します。

### 1-1. 利用準備
GameObjectの componentとして attach します。
uOSCClient が自動的に attach されますので、送信先ポートをVMTのポート(デフォルトは39570)に変更しておきます。

### 1-2. public メンバ
- `OgLikeHandData _leftHand;`
- `OgLikeHandData _rightHand;`

- `void SendLeftHandData(int scalarModeInt)`
- `void SendRightHandData(int scalarModeInt)`

使い方としては、_letHand/_rightHand に曲げや開きの値を設定しておいて、Send系関数で VMT に送ることになります。`OgLikeHandData` については 2 を参照してください。

### 1-3. OgLikeVMTClient.Send系関数のパラメーター
`scalarModeInt` は5本の指のどれをscalarModeにするかをビットで示す整数です。
指毎に「曲げ値を関節毎に送るか、スカラ値1つとして送るか」を示します。

以下のように5つのbool値を元に scalarModeInt を計算できます。
```C#
    public bool _scalarModeThumb = true;
    public bool _scalarModeIndex = false;
    public bool _scalarModeMiddle = false;
    public bool _scalarModeRing = false;
    public bool _scalarModePinky = false;

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
```

## 2. class OgLikeHandData
OgLikeVMTClient のメンバとして左手/右手の曲げ・開きの情報を保持するクラスです。

### 2-1. 初期化用publicメソッド

（`class OgLikeVMTClient`を通じて利用する場合は意識する必要はありません）
- `OgLikeHandData( bool isLeft )` コンストラクタは左右どちらの手かを指定します
- `void InitBones(OgLikeHandAnim anim)` 曲げ具合と関節位置のアニメーションを指定して初期化します。

### 2-2. 情報取得publicメソッド/プロパティ

|メソッド/プロパティ|説明|
| ---- | ---- |
|`void GetBoneTransform( ref SkeletalBoneTransform tfm, int boneIndex )`|特定のボーン(31あるskeletal boneの1つ)をindexで指定して、position と rotation の情報を含む SkeletalBoneTransform を得ます。<br>この情報は InitBonesで指定したアニメーションを使用して計算されます。<br>boneIndex は、0-30の範囲で、ogLikeCommon.cs で定義されている `enum HandSkeletonBone` を int にキャストしたものが使えます。|
|`bool IsLeftHand;`|左手/右手どちらであるかを返します|

### 2-3. 情報設定publicメソッド
FingerIndex(enum, 後述) は5本の指のどれであるかを指定し、jointIndex は 0-3の整数で手首に近い方からの関節の番号を指定します。

全て void で返値はありません。

|メソッド|説明|
| ---- | ---- |
|`SetSplay( FingerIndex idx, float value )`|  og's (AB) (BB) (CB) (DB) and (EB) に対応<br>指の広がりを valueで指定します。<br>-1.0f ～ +1.0f の範囲が、Unity Humanoid Boneの標準的な可動域(親指±25度、人差し指±20度、中指と薬指±7.5度、小指±20度)に対応しますが、-1未満または1超の数値を設定した場合もそれ以上の拡げとして処理されます。(OpenGlovesは全ての指で20度なので、互換性はありません。)<br>プライベートメンバ `_maxSplayAngleHumanoid` を書き換えることで指ごとの角度設定値を変更できます。|
|`SetJointFlexion( FingerIndex idx, int jointIndex, float value )`|og's (AAB)(BAB),etc. に対応<br>関節毎の曲度合いを value(0.0f ～ +1.0f)で指定します。<br>曲げ度合いは定義アニメーションに基づき、OpenGlovesに近いものとなります。|
|`SetFingerScalarFlexion( FingerIndex idx, float value )`| og's A B C D E に対応<br>指の曲げ度合いを全体として1つのスカラ値 value( -1.0f ～ +1.0f ) で指定します。<br>曲げ度合いは定義アニメーションに基づき、OpenGlovesに近いものとなります。|
|`SetFingerScalarFlexion( FingerIndex idx )`| value 値を指定しない場合、4つの関節ごとの曲げ度合いの平均値で設定します|

### 2-4. デバッグ出力メソッド
`public void DebugPrintFlexion()`  
　Debug.Log でその時点での曲げ値の設定内容を出力します。

## 3. class SkeletalBoneTransform
ogLikeCommon.cs で定義されています。position と rotation だけを含みます。
```C#
    public class SkeletalBoneTransform {
        public Vector3 _position;
        public Quaternion _rotation = Quaternion.identity;
    };
```

## 4. enum FingerIndex
ogLikeCommon.cs で定義されています。
```C#
    public enum FingerIndex{
        Thumb = 0,
        IndexFinger = 1,
        MiddleFinger = 2,
        RingFinger = 3,
        PinkyFinger = 4,
        COUNT = 5,
        Unknown = -1
    };
```

## 5. enum HandSkeletonBone
0-30の数値がSkeletonBoneに割り当てられています。詳細はogLikeCommon.cs を参照してください。

## 6. class OgLikeHandAnim
OpenGloves に近い変換アニメーションロジックを備えており、中身を変更する必要はありません。

## 7. 具体的な実装例
VBTSkeletalTrack が実装例ですが、VBTTools (class VBT*)は VRMモデルのロードを前提としているため、サンプルとしては必要以上に複雑です。
よりシンプルな例として、以下のものを用意しました。
VBTTools/Sample/SimpleOgLikeVMTSample.unity (シーンファイル)
VBTTools/Sample/Script/SimpleOgLikeVMTSample.cs (スクリプト)

あらかじめ、Skeleton Pose Tester などを立ち上げておいて、UnityでPlayし、Start Sending to VMTにチェックを入れて、いくつかの指のチェックを入れてスライダーを動かすと、Skeleton Pose Testerの指が動かせます。
(Skeleton Pose Tester .. というか SteamVR をあとから立ち上げた場合は、Start Sending to VMTをいったんオフにしてからオンにすれば動きます)
