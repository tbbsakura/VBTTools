# VBTTools
Virtual Body (VRM Body) Tracking Tools v0.2.0 に向けてのドラフト<br>
(最新のソースを利用する場合の説明は、[ReadMeUpdateブランチのReadme](https://github.com/tbbsakura/VBTTools/blob/ReadMeUpdate/README.md)を参照してください)

[![使用例（youtube 動画6秒）](https://github.com/tbbsakura/VBTTools/blob/main/Assets/SakuraShop_tbb/VBTTools/etc/youtube_tn01_960x540.jpg)](https://www.youtube.com/watch?v=X4_1aNCIf7s)

リアルのボディにトラッカーを付けてトラッキングするのではなく、VRMモデルの姿勢をトラッキングして、[Virtual Motion Tracker(VMT)](https://github.com/gpsnmeajp/VirtualMotionTracker)に情報を渡して、SteamVR の仮想HMD/コントローラーとして利用しようとするものです。

VMCProtocol(VMCP) を受信できるので、VRigUnity、TDPT(ThreeD Pose Tracker)、VSeeFace、Webcam Motion Capture などのソフトでWebカメラの情報で手の動きをトラッキングしてVRMに反映し、VRChat等で頭と手を動かす3点トラッキングができ、指もうごかせます。位置情報を設定できるので、手首にVIVEトラッカーをつけたりする必要がありません。

使用例としては以下のようなものがあります。
- WebカメラとJoyConを利用したHMDレス3点トラッキング
- Quest等のHMDを利用しつつ、コントローラーをSkeletal Input対応のカメラトラッキングに置き換える(LeapMotion等でない普通のカメラではあまり実用的ではないです)

現状、以下のHMD/コントローラー情報(VBTToolsから見ると出力)に対応しています。

- VRMモデルの頭の位置を読み取って、仮想HMDの位置・向きに反映(v0.2.0より)
- VRMモデルの指の動きを読みとって、Indexコントローラー互換コントローラー(Skeletal Input対応)として使用
- VRMモデルの頭と手の相対位置およびHMDの位置情報から、Indexコントローラー互換コントローラーの位置・向きを計算して使用
- JoyCon を使用して/もしくは画面上のuiで、ボタン・スティックの操作

また、JoyCon 利用時は、VRChat でのメニュー操作を補助する機能があります。
- 手のトラッキングを一時停止して、スティック操作で手を動かす機能
- 手のトラッキング一時停止中に、メニューを操作しやすい位置に手を動かす機能

## 暫定公開
まだユーザーが少なく、運用された環境が偏っています。不便だなと思う部分も完全には修正できていません。説明（この文書）もまだ不十分な部分があります。

## 1. 使用方法
### 1-1. セットアップ
事前に VMT (Virtual Motion Tracker)と、使う場合はOpenTrackとOpenVR-OpenTrack を入れておきます。
- VMTは Skeletal Input対応仮想コントローラー処理のために必要
- OpenTrack は仮想HMD処理のために必要(マルチモニタが必要です)(**要確認：仮想モニタでできるかどうか**)

VBTTools、VMT、OpenTrack それぞれ UDPでの通信を受信するため、初回起動時にットワーク受信を許可するかどうか質問されますので、許可するよう設定します。

#### 1-1-1. VMT(Virtual Motion Tracker)のインストール・設定
[VMT - Virtual Motion Tracker v0.15](https://github.com/gpsnmeajp/VirtualMotionTracker/releases/tag/v0.15)

VMTは仮想コントローラーの処理に必要です。v0.15 で開発しています。
[VMTのドキュメント](https://gpsnmeajp.github.io/VirtualMotionTrackerDocument/setup/) に従いインストールと初期設定(ルームセットアップ)を行ってください。また、VMT Manager の Controlタブで Always Combatible を一度クリックして設定しておくことを推奨します。indexコントローラー互換になり、自分でバインド設定をしなくても利用できます。なお、ボタンを押しても設定ファイルに反映されるのみで、アプリ上の見た目は変わりません。
設定が終わったらVMTManager は終了させておきます（必須）

#### 1-1-2. VMTの注意点
- index1/2 (VMT_1, VMT_2) をIndexコントローラー互換(Enable 5/6)として使うので他と被ると使えません。（非互換コントローラーを自動起動するような設定にしないでください）
- HMDの位置情報をVMTから受け取る必要があるので、先にVMT Managerが起動していて VMT用ポートを listen していると使えません。

#### 1-1-3. OpenTrackのインストール・設定
OpenTrack は opentrack 2023.3.0 、OpenVR Driver は 1.1 で開発しています。
- [OpenTrack](https://github.com/opentrack/opentrack/releases)
- [OpenVR-OpenTrack](https://github.com/r57zone/OpenVR-OpenTrack/releases)

OpenTrack は仮想HMDを利用する場合に必要です。逆に言えば、以下の場合は不要です。
- 実際のHMDを使う場合
- 実際のHMDを一時停止してSteamVRのVRビューで画面を見る場合
- VMT付属の Null HMD Driver (頭が動かせない仮想HMD)などの他の仮想HMDを利用する場合

使う場合はマルチモニタが必要です。（仮想モニタでも使えます）
OpenTrackを使用する場合の設定方法は、まず仮想モニタの設定方法を含め、[こちら](docs/OpenTrackWithIDD.md) を参照して設定してください。

さらに、VBTTools のための設定ですが、リンク先のインストールでOpenVR-OpenTrackを入れる際、2種類どちらを入れたかで設定は変わってきます。

1. FreeTrack版の場合は、OpenTrack.exe で「InputをUDP over network、OutputをFreeTrack」にする
2. UDP版にした場合は、OpenTrack.exe を起動しない(起動する場合、InputはUDP over netork以外にする)


#### 1-1-4. VBTTools の起動テスト
続いて、VBTTools の [Releases](https://github.com/tbbsakura/VBTTools/releases) で公開している VBTTools の zip を解凍し、VBTTools.exe を起動します。
初回起動時はサーバーとして機能するためWindowsが確認ウィンドウを出すので通信許可をしてください。
VRMモデル(VRoid Studio Beta 用モデルとしてCC0で公開されている HairSample_Male くん)がロードされると思いますが、いったん終了します。

#### 1-1-5. 実際に使うアプリを起動、SteamVRトラッカー初期設定
1. VMCProtocol でポーズを送信できるアプリ(使う場合)。出力ポートは39544としてください（VBTTool側も変えられるので一致していればOK）udpなので、送信側が先に起動していてOKですし、後から起動でもOKです。
2. SteamVR を起動します。Quest+VirtualDesktop(VD)の場合は、VDが認識するように起動してください。
3. VMTとOpenTrackを使う設定になっていない場合は、設定変更します（SteamVR再起動が必要になります）
4. VMT ManagerでVMTが有効であることを確認します。また、HMD のかわりにNull Driverでテストすることもできます。必要ならSteamVRを再起動したあと「VMT managerを終了」させてください。(慣れたらこの手順は省略できます)
5. OpenTrackのSteamVRドライバーがfreetrack版の場合は、OpenTrack.exe を起動して、入力を UDP over network、出力を freetrack 2.0 enhanced にします。(OpenTrackのSteamVRドライバーがUDP版の場合はOpenTrack.exeを終了させるか、入力を UDP over network以外にします)
6. VBTTools.exe を起動します。起動すると VMCProtocol の受信は即座に始まります(左上のチェックボックスがオンになっています。)受信待機状態なので送信側の準備が未了でもOKです。一方、送信側（右上）のチェックは起動直後はオフになっていますが、。VMTのチェックはVMT側の準備ができている状態でチェックをいれてください。先にチェックを入れてしまった場合、いったんオフにして入れなおせばOKです。OpenTrackへの送信は先に開始していてもOKです（無駄なパケットが飛びますが）
7. 初回起動時はVMTの送信にチェックを入れた後、SteamVRの設定で VMT_1, VMT_2 のトラッカーをハンドヘルドの左手/右手に割り当てる必要があります。(手首に設定した場合は後述の位置関係の調整が必要になります)

![Image 1](https://github.com/tbbsakura/VBTTools/blob/main/Assets/SakuraShop_tbb/VBTTools/etc/tracker_setting.png)
![Image 2](https://github.com/tbbsakura/VBTTools/blob/main/Assets/SakuraShop_tbb/VBTTools/etc/vmt1_setting.png)
![Image 3](https://github.com/tbbsakura/VBTTools/blob/main/Assets/SakuraShop_tbb/VBTTools/etc/vmt2_setting.png)


1でVMCProtocolのソフトを起動していれば、手の位置等がVBTToolsのVBMモデルに反映されるはずです。VMCProtocolのソフトを入れてない場合は、左上のListen To VMCPのチェックを外すと、手のボーンを動かすためのTest UIが表示されます。

受信ポート等のネットワーク設定は左上または右上のSettingボタンの中で変更できます。

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
HMDレスで使う場合は必要性を感じることはほぼないと思いますので、必要なければ次に進んでください。

HMDをかぶって利用する場合、手の位置がおかしい場合は調整が必要です。(v0.1.0で、それ以前と設定値が変わっているため再調整が必要です)

また、この調整内容は、ゲーム中でのVRで選択するためのレイ(光線)ポインターの向きにも影響します。
Adjust UIのチェック3種類のいずれかを入れてスライダーを動かして調整でき、調整内容はjsonファイルに保存できます。VBTTools.exe と同じフォルダの default.jsonは起動時に読み込まれる設定値になります。

SteamVRHandTest で調整する場合、起動して手が見える位置にある状態にします。（画面内に手が出てこない場合はSteamVRとVMTのルームセットアップを確認してください）
 **VMTの仮想コントローラーの位置と向きを示すCubeと、手のモデルがセットで表示されます。** 

調整は、後述の通り、回転を調整したあと位置を調整します。回転を調整するまでは、2-1.後半に記載の手の位置移動機能を活用して画面内に手がある状態で一時停止しておくと便利です。（位置を調整するときは解除してください）

次にまず手首の位置をうごかさず回転させてみます。VMTのCubeがくるくる回りますが、それに連動する手の位置や向きがおかしい場合に、修正をします。

調整設定画面は3つに分かれていて、1つめ"Controller Hand Offset"は左右の **「Cubeの挙動を修正」** するもの、2つめと3つめ(Skeletal Root/Wrist Lと同R)はそれぞれ左手と右手のもので **「Cubeは変更せず、Cubeと手の位置関係・向きを調整」** するものです。

1つめのUIでのController Hand Offsetの調整(Cubeの調整)は、基本的にはPosition の調整値を0の状態で Rotationの調整を先に行い、最後にPosition を調整すると良いと思います。

Cubeの調整は、以下のことに留意して行います。
- レイ（光線）ポインターは青矢印の方向に出る
- VRChat のランチパッドやメニューは緑の矢印のほうに出る

Cube の向きと挙動を調整すると、手の向きが真逆になったりするので、2つめ3つめのUI画面で手を回転させ(rotationを調整する)実際の手の表示される向きを修正します。

手の動きは悪くない感じに調整できたら、HMDを被ってVMCP受信状態(一時的な手の移動がキャンセルされた状態)にして、実際の手と、HMDで投影されている手の位置を確認します。この段階でズレに違和感がある場合は、1つめのController Hand Offset調整画面の一番下のほうにある位置調整を行います。
これは単純に手の表示位置を平行移動するものなので、自分の手の位置にあわせて移動させて調整します。なお、VMCP受信を停止にした状態でのTestUIやJoyConでの一時的な手の移動はVMCP受信を再開するとリセットされますが、調整UIで調整した分は常時適用されます。

調整ができたら、Saveボタンで設定を json ファイルに保存します。自動的に読み込みたい場合は vbtools.exe と同じ場所にある default.jsonを上書きします（戻したい時はzipから展開して上書きしてください）。何種類か設定を持ちたい場合は別のファイル名で保存して、調整画面のLoadボタンで読み込むこともできます。

## 3. 仮想HMDの利用方法
右上の Start Sending の所の Head Pos To OpenTrack にチェックを入れておけば、VRMの頭の位置と向きを OpenTrackに送信します。
うまくいかない場合は

- 1-1-3. のとおりに設定できているか
- OpenTrackの受信を禁止設定していないか
- SteamVRのアドオンでOpenTrackが無効になっていないか

## 4. JoyCon の利用、ButtonPanelの利用
### 4-1. ButtonPanelの利用
JoyConが無い場合は下方の Use Button Panelを押すと画面でボタン等操作できます。(JoyConがうまく動かない場合もこちらを使ってください)
Systemボタン、A/Bボタン、トリガー、グリップ、サムスティックが扱えます。

### 4-2. JoyCon の利用
JoyConを使う場合は、あらかじめPCとBluetooth接続させてください。ペアリング済みではなく、**接続済み** になる必要があります。毎回一度削除して接続しなおさないと接続済みにならないようです。

JoyConを使う場合は Use Joy Con (LR) にチェックを入れます。VBTTools起動後の初回チェック時は認識のため数秒固まります。認識時に長く(10秒以上等)固まった後に動かない場合、一度オフにしてオンにすると動く場合があります。(PC再起動後には数秒で認識するように戻る事が多いです)

基本的に、VRアプリ側ではコントローラーをIndexコントローラーであると認識するように設定します。
JoyCon利用時にボタンと動作が異なる場合はここを確認してください。

### 4-2. JoyCon の一時停止機能
JoyCon利用時は、通常のSteamVRのコントローラーとしての機能に加えて、**Y/左ボタンでトラッキング(VMCP受信)の一時停止とスティックでの手の位置移動機能**が使えます。
また**A/右のボタンはトラッキングの停止と同時に、VRChatでのメニューが操作しやすい場所に手が移動します**。メニューの呼び出しは、Y/左で一時停止した後に呼び出すこともできます。

一時停止中は、TestUIが表示されており、スライダーでも手の位置を変更できます。
一時停止はY/左ボタンで解除できます。解除すると位置補正もなくなり通常の手の位置に戻ります。(A/右は後からメニューを呼び出せる機能があるため、一時停止解除には使えません)

## 5. その他の機能
### 5-1. OpenVRM ボタン
画面下方の OpenVRMボタンで異なるVRMモデルを読むことができます。（現状、VRM1は対応していないのでVRM0限定です）
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
    - テスト時バージョン: ExternalReceiverPack_v4_1.unitypackage

2. [UniVRM (必要に応じて)](https://github.com/vrm-c/UniVRM/releases)
    - 1 でエラー等でなければ入れなくていいと思いますが、
最新にしたい場合等は入れます。
    - テスト時バージョン: UniVRM-0.125.0_f812.unitypackage VRM0

3. [uOSC](https://github.com/hecomi/uOSC)
    - テスト時バージョン： uOSC-v2.2.0.unitypackage

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


