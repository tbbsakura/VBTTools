# VBTTools
Virtual Body (VRM Body) Tracking Tools v0.3.0<br>
(最新のソースを利用する場合の説明は、[ReadMeUpdateブランチのReadme](https://github.com/tbbsakura/VBTTools/blob/ReadMeUpdate/README.md)を参照してください)

[![使用例（youtube 動画6秒）](docs/img/youtube_tn01_960x540.jpg)](https://www.youtube.com/watch?v=X4_1aNCIf7s)

リアルのボディにトラッカーを付けてトラッキングするのではなく、VRMモデルの姿勢をトラッキングして、[Virtual Motion Tracker(VMT)](https://github.com/gpsnmeajp/VirtualMotionTracker)や [OpenVR-OpenTrack](https://github.com/r57zone/OpenVR-OpenTrack)に情報を渡して、SteamVR の仮想HMD/コントローラーとして利用しようとするものです。

VMCProtocol(VMCP) を受信できるので、VRigUnity、TDPT(ThreeD Pose Tracker)、VSeeFace、Webcam Motion Capture などのソフトでWebカメラの情報で手の動きをトラッキングしてVRMに反映し、VRChat等で頭と手を動かす3点トラッキングができ、指もうごかせます。位置情報を設定できるので、手首にVIVEトラッカーをつけたりする必要がありません。

使用例としては以下のようなものがあります。
- [WebカメラとJoyConを利用したHMDレス3点トラッキング](./docs/VRC3points.md)
- Quest等のHMDを利用しつつ、コントローラーをSkeletal Input対応のカメラトラッキングに置き換える(LeapMotion等でない普通のカメラではあまり実用的ではないです)

現状、以下のHMD/コントローラー情報(VBTToolsから見ると出力)に対応しています。

- VRMモデルの頭の位置を読み取って、仮想HMDの位置・向きに反映(v0.2.0より)
- VRMモデルの指の動きを読みとって、Indexコントローラー互換コントローラー(Skeletal Input対応)として使用
- VRMモデルの頭と手の相対位置およびHMDの位置情報から、Indexコントローラー互換コントローラーの位置・向きを計算して使用
- JoyCon を使用して/もしくは画面上のuiで、ボタン・スティックの操作

また、JoyCon 利用時は、VRChat でのメニュー操作を補助する機能があります。
- 手のトラッキングを一時停止して、スティック操作で手を動かす機能
- 手のトラッキング一時停止中に、メニューを操作しやすい位置に手を動かす機能

### 暫定公開
まだユーザーが少なく、運用された環境が偏っています。不便だなと思う部分も完全には修正できていません。既知の不便な部分としては

- レイ（光線）ポインターの飛ぶ向きが不自然で使い勝手が悪い
- 加えてカメラのハンドトラッキングでは手がブレがちで、物を持つ等が難しい

があります。

## 1. 使用方法
[VRChatでWebCamとJoyConで3点トラッキングをする場合の説明](./docs/VRC3points.md)もあり、あちらではVRigUnityを使って具体的に全ての設定を説明していますので、解りやすいかもしれません。
一方で、このReadMeのほうは汎用的な分、やや解りにくい説明になっています。

### 1-1. セットアップ
事前に VMT (Virtual Motion Tracker)と、使う場合はOpenTrackとOpenVR-OpenTrack を入れておきます。

#### 1-1-1. OpenTrackのインストール・設定
OpenTrack は仮想HMD処理のために必要です。HMDを持っていない場合は最初にこれを設定してください。

逆に言えば、以下の場合は不要です。
- 実際のHMDを使う場合
- 実際のHMDを一時停止してSteamVRのVRビューで画面を見る場合
- VMT付属の Null HMD Driver (頭が動かせない仮想HMD)などの他の仮想HMDを利用する場合

OpenTrack は opentrack 2023.3.0 、OpenVR Driver は 1.1 で開発しています。
- [OpenTrack](https://github.com/opentrack/opentrack/releases)
- [OpenVR-OpenTrack](https://github.com/r57zone/OpenVR-OpenTrack/releases)

また、使う場合はマルチモニタが必要です。（仮想モニタでも使えます）
[OpenTrackを使用する場合の設定方法は、まず仮想モニタの設定方法を含め、こちら](docs/OpenTrackWithIDD.md) を参照して設定してください。

#### 1-1-2. VMT(Virtual Motion Tracker)のインストール・設定
VMTは Skeletal Input対応仮想コントローラー処理のために必要です。
[VMT設定はこちら](./docs/VMTSetting.md)の説明に従ってください。

VMTの注意点は以下の通りです。（リンク先にも書いてありますが、再掲しています）
- index1/2 (VMT_1, VMT_2) をIndexコントローラー互換(Enable 5/6)として使うので他と被ると使えません。（非互換コントローラーを自動起動するような設定にしないでください）
- HMDの位置情報をVMTから受け取る必要があるので、先にVMT Managerが起動していて VMT用ポートを listen していると使えません。

#### 1-1-3. VBTTools の起動テスト
続いて、VBTTools の [Releases](https://github.com/tbbsakura/VBTTools/releases) で公開している VBTTools の zip を解凍し、VBTTools.exe を起動します。
初回起動時はサーバーとして機能するためWindowsが確認ウィンドウを出すので通信許可をしてください。

#### 1-1-4. VMCProtocolのアプリ、中継アプリ等一式を起動

1. VMCProtocol でポーズを送信できるアプリ(使う場合)。出力ポートは39544として送信開始してください。<br>(これがよくわからないという方は、入れないでTestUIを使うか、もしくは[VRChatでWebCamとJoyConで3点トラッキングをする場合の説明](./docs/VRC3points.md)でVRigUnityでの説明があるので、そちらを参考にしてください。)

2. HMD(リアルまたはOpenTrack等の仮想HMD)がある状態で SteamVR を起動します。Quest+VirtualDesktop(VD)の場合は、VDが認識するように起動してください。

3. OpenTrackを使っている場合、SteamVRドライバーがfreetrack版の場合は、OpenTrack.exe を起動して、入力を UDP over network、出力を freetrack 2.0 enhanced にします。(OpenTrackのSteamVRドライバーがUDP版の場合はOpenTrack.exeを終了させるか、入力/出力を UDP over network以外にします)

4. VBTTools を起動します。<br />
VBTToolsは起動すると VMCProtocol の受信は即座に始まっていますので、手の位置等がVBTToolsのVRMモデルに反映されているはずです。VMCProtocolのソフトを入れてない場合は、左上のListen To VMCPのチェックを外すと、手のボーンを動かすためのTest UIが表示されます。<br />
VBTTools の受信ポート等のネットワーク設定は左上または右上のSettingボタンの中で変更できます。

## 2. Skeletal Input 仮想コントローラーとしての利用方法
### 2-1. 起動後の手順
手や指の位置情報をVMTへの送信を開始する場合は右上のほうの Start Sending のHand Pos To VMT にチェックを入れます。送信先のIPアドレスやポート番号は Setting で設定できますが、通常変更不要です。Recvというところの赤い■が濃くなっていればHMDの位置情報を受信できています。できていない場合は色が薄くなってしまうので

- VMTが無効になっていないか（起動時アドオンが無効になっていないか）
- VMT Managerが起動していないか
- VBTTools.exe の受信を禁止していないか,複数起動していないか

を確認して、SteamVR、VBTTools の順番で起動しなおしてください。

### 2-2. ダッシュボード画面での確認
下方の Use Button Panelを押すと画面でボタン等操作できます。左右いずれかのSystemボタンでダッシュボードを出したり消したりできます。（JoyCon等も使えますが、初期設定時の最低限としてはこちらで十分です）

SteamVRのダッシュボード画面で手の位置とコントローラーの位置が若干(数cm～10cm程度)ズレるのは仕様です(プレー画面と位置が変わります)。VMTのキューブ(コントローラー位置を示す)が頭・HMDと全然違う位置に出る場合(1m以上離れているような場合)はSteamVRとVMTのルームセットアップを確認してください。

### 2-2. SteamVR(Skeletal Input対応)アプリの起動
実際にSkeletal Input対応アプリを起動して動作を確認します。
VRMモデルの手をうごかせない状態（webカメラトラッキング等を設定していない場合）でTestUIを使う場合は [SkeletonPoseTester](https://github.com/gpsnmeajp/SkeletonPoseTester) を使って指の動きをテストできます。

手をうごかせる場合は(VMCProtocol送信アプリを使っている場合等) v0.1.0 の Release ページで配布している [SteamVRHandTest](https://github.com/tbbsakura/VBTTools/releases/download/v0.1.0/SteamVRHandTest_v0.0.1.zip) を使うと向きの調整もしやすいです。
これらのアプリはSteam のUIからは起動できないので、SteamVR起動中に .exe ファイルを直接起動してください。

手を動かせる場合は、VRChat や Moondust Knuckles Tech Demos などで動作を確認できます。

### 2-3. 指が動かないときの確認事項
基本的に、VRアプリ側ではコントローラーをIndexコントローラーであると認識するように設定します。
SteamVRでのバインディング設定でIndexコントローラーのスケルトンの入力を無効化すると動かないので気を付けてください。

Quest + Virtual Desktop(VD) での利用の場合 VD での設定で Forward Tracking Data to PC はオフにします。
（オンにすると別の仮想コントローラーが認識されると思いますが、VMTの仮想コントローラーが優先されていればオンでも動作します）

VRChat で利用する場合は、設定画面で"Controls"(日本語だと「コントロール」) の項目の先頭にSteamVR とある部分で、以下の設定をします。
- **Exclusive Finger Tracking Mode オプションはオフ**（オンでも指は動きますがボタン等が効かなくなります）
- **Avatars Use Finger Tracking オプションをオン**(オフだと指が動かなくなります)

### 2-4. 手の位置と向きの調整
HMDをかぶって利用する場合、手の位置がおかしい場合は調整が必要です。(v0.1.0で、それ以前と設定値が変わっているため再調整が必要です)
[設定方法はこちらを参照してください。](./docs/HandAdjust.md)

HMDレスで使う場合は必要性を感じることはほぼないと思いますので、必要なければ次に進んでください。


## 3. 仮想HMDのトラッキング有効化
右上の Start Sending の所の Head Pos To OpenTrack にチェックを入れておけば、VRMの頭の位置と向きを OpenTrackに送信します。
うまくいかない場合は以下を確認します。

- [こちら](docs/OpenTrackWithIDD.md)のとおりに設定できているか
- OpenTrackの通信受信を禁止設定していないか
- SteamVRのアドオンでOpenTrackが無効になっていないか

次項のようにJoyConを使っている場合は左のSRボタンでオンオフできます。

## 4. JoyCon の利用、ButtonPanelの利用
### 4-1. ButtonPanelの利用
JoyConが無い場合は下方の Use Button Panelを押すと画面でボタン等操作できます。(JoyConがうまく動かない場合もこちらを使ってください)
Systemボタン、A/Bボタン、トリガー、グリップ、サムスティックが扱えます。

### 4-2. JoyCon の接続と利用
[こちら](./JoyconConnect.md)の通り設定・接続してください。

接続したJoyConを使う場合はVBTToolsの下方にある Use Joy Con (LR) にチェックを入れます。VBTTools起動後の初回チェック時は認識のため数秒固まります。認識時に長く(10秒以上等)固まった後に動かない場合、一度オフにしてオンにすると動く場合があります。(よくあります。PC再起動後には数秒で認識するように戻る事が多いです。)

基本的に、VRアプリ側ではコントローラーをIndexコントローラーであると認識するように設定します。JoyCon利用時にボタンと動作が異なる場合はここを確認してください。

### 4-2. JoyCon の一時停止機能
JoyCon利用時は、通常のSteamVRのコントローラーとしての機能に加えて、**Y/左◀ボタンでトラッキング(VMCP受信)の一時停止とスティックでの手の位置移動機能**が使えます。
また**A/右▶のボタンはトラッキングの停止と同時に、VRChatでのメニューが操作しやすい場所に手が移動します**。メニューの呼び出しは、Y/左で一時停止した後に呼び出すこともできます。

一時停止中は、TestUIが表示されており、スライダーでも手の位置を変更できます。
一時停止はY/左ボタンで解除できます。解除すると位置補正もなくなり通常の手の位置に戻ります。(A/右は後からメニューを呼び出せる機能があるため、一時停止解除には使えません)

### 4-3. JoyCon機能割り当て詳細
[こちら](docs/JoyConAssign.md)を参照してください。

## 5. その他の機能
### 5-1. OpenVRM ボタン
画面下方の OpenVRMボタンで異なるVRMモデルを読むことができます。（VRM1はv0.3.0で対応）
次回起動時は最後に読んだVRMが起動時に読み込まれますが、前回のファイルがなくなっている場合などはデフォルトモデルを読み込みます。

### 5-2. Setting ボタン
ネットワークの設定(UDP送信先アドレスや、送受信に使うポート番号)を設定できます。
左上と右上どちらから開いても同じです。
OKを押すと、それまでの通信と設定が変わっている部分は反映されます。Cancelの場合は変更はなかったことになります。

前回起動時の設定を保存するので、一時的変更をした場合はこの画面で戻す必要があります。

## 6. Build方法(開発者向け)
Unity 2022.3.22f1 で開発しています。
必要物を一式プロジェクトにインポートしてから後述のシーンを開いてBuildします。

### 6-1. Unity project に別途入れる必要があるもの
テスト時バージョンと同じ unitypackage を入れるのが無難かと思います。全てMIT License で公開されているものです。
1. [EVMCP4U (Eazy Virtual Motion Capture for Unity), External Receiver Pack](https://booth.pm/ja/items/1801535)
    - テスト時バージョン(v0.3.0以降): ExternalReceiverPack_v5_0b.unitypackage
    - テスト時バージョン: ExternalReceiverPack_v4_1.unitypackage

2. [UniVRM (必要に応じて)](https://github.com/vrm-c/UniVRM/releases)
    - 1 でエラー等でなければ入れなくていいと思いますが、最新にしたい場合等は入れます。
    - テスト時バージョン: External Receiver付属の物
    - (v0.2.xまでは)UniVRM-0.125.0_f812.unitypackage VRM0

3. [uOSC](https://github.com/hecomi/uOSC)
    - (v0.3.0~) こちらもExternal Receiver付属のでOKです
    - (~v0.2.x) テスト時バージョン： uOSC-v2.2.0.unitypackage

4. [UnityStandaloneFileBrowser](https://github.com/gkngkc/UnityStandaloneFileBrowser)
    - テスト時バージョン：  1.2

新規のプロジェクトに1~4を入れてから、こちらのリポジトリの内容を追加すれば Build できると思います。


### 6-2. MITライセンスで公開されているものの改変版が入っているもの
参考のためURL書いておきますが、後から入れると改変が上書きされてしまうので入れないでください。

1. [UnityWindowsFileDrag&Drop](https://github.com/Bunny83/UnityWindowsFileDrag-Drop)

2. [JoyConLib](https://github.com/Looking-Glass/JoyconLib/releases)


### 6-3. シーンファイル等
VBTTools.exe は　Assets/SakuraShop_tbb/VBTTools/Samples/VBTSample.unity を buildしたものです。
PlayerSetting 等はウィンドウサイズ可変にする等していますが、特殊な設定は特にしていないので普通にbuildできるかと思います。

### 6-4. OgLikeVMTと開発者向けサンプル
VBTTools.exe そのものがサンプルではありますが、OpenGlovesライクにVMTを使うための機能である OgLikeVMT だけのサンプルを別途用意してあります。

- VBTTools/Sample/SimpleOgLikeVMTSample.unity (シーンファイル) 
- VBTTools/Sample/Script/SimpleOgLikeVMTSample.cs (スクリプト)

詳細説明は [こちら](docs/OgLikeVMT.md)


