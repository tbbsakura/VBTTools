# VBTTools
Virtual Body (VRM Body) Tracking Tools v0.1.0

[![使用例（youtube 動画6秒）](https://github.com/tbbsakura/VBTTools/blob/main/Assets/SakuraShop_tbb/VBTTools/etc/youtube_tn01_960x540.jpg)](https://www.youtube.com/watch?v=X4_1aNCIf7s)

リアルのボディにトラッカーを付けてトラッキングするのではなく、VRMモデルの姿勢をトラッキングして、[Virtual Motion Tracker(VMT)](https://github.com/gpsnmeajp/VirtualMotionTracker)に情報を渡して、SteamVR の仮想コントローラーとして利用しようとするものです。現状、以下のコントローラー情報(VBTToolsから見ると出力)に対応しています。

- VRMモデルの指の動きを読みとって、Indexコントローラー互換コントローラー(Skeletal Input対応)として使用
- VRMモデルの頭と手の相対位置およびHMDの位置情報から、Indexコントローラー互換コントローラーの位置・向きを計算して使用
- JoyCon を使用して/もしくは画面上のuiで、ボタン・スティックの操作
- JoyCon 利用時は、手のトラッキングを一時停止して、スティック操作で手を動かす機能

VMCProtocol(VMCP) を受信できるので、TDPT(ThreeD Pose Tracker)、VSeeFace、Webcam Motion Capture などのソフトでWebカメラの情報で手の動きをトラッキングしてVRMに反映し、VRChat等で手を動かせます。位置情報を設定できるので、手首にVIVEトラッカーをつけたりする必要がありません。

まだコントローラーしか対応していない(足などのトラッカーに対応していない)ので Virtual Body (VRM Body) "Hand/Finger" Tracking Tools と言うべき所ですが、開発者の方であれば、コントローラー以外のトラッカーもあわせて利用するのは割と簡単にできると思います。
（たとえば、VRMの頭の位置を仮想トラッカーに反映して、Tracking Override等できるはず）

## 暫定公開
まだユーザーが少なく、運用された環境が偏っています。不便だなと思う部分も完全には修正できていません。説明（この文書）もまだ不十分な部分があります。

## 1. 使用方法
### 1-1. セットアップ
#### 1-1-1. VMT(Virtual Motion Tracker)のインストール・設定
事前に VMT (Virtual Motion Tracker)を入れておきます。

[VMT - Virtual Motion Tracker v0.15](https://github.com/gpsnmeajp/VirtualMotionTracker/releases/tag/v0.15)

全面的にお世話になっています。v0.15 で開発しています。
[VMTのドキュメント](https://gpsnmeajp.github.io/VirtualMotionTrackerDocument/setup/) に従いインストールと初期設定(ルームセットアップ)を行ってください。また、VMT Manager の Controlタブで Always Combatible を一度クリックして設定しておくことを推奨します。indexコントローラー互換になり、自分でバインド設定をしなくても利用できます。なお、ボタンを押しても設定ファイルに反映されるのみで、アプリ上の見た目は変わりません。
設定が終わったらVMTManager は終了させておきます（必須）

#### 1-1-2. VMTの注意点
- index1/2 (VMT_1, VMT_2) をIndexコントローラー互換(Enable 5/6)として使うので他と被ると使えません。（非互換コントローラーを自動起動するような設定にしないでください）
- HMDの位置情報をVMTから受け取る必要があるので、先にVMT Managerが起動していて VMT用ポートを listen していると使えません。

#### 1-1-3. VBTTools の起動テスト
続いて、VBTTools の [Releases](https://github.com/tbbsakura/VBTTools/releases) で公開している VBTTools の zip を解凍し、VBTTools.exe を起動します。
初回起動時はサーバーとして機能するためWindowsが確認ウィンドウを出すので通信許可をしてください。
VRMモデル(VRoid Studio Beta 用モデルとしてCC0で公開されている HairSample_Male くん)がロードされると思いますが、いったん終了します。
VBTTools.exe と同じフォルダに同じ名前でVRM0のファイルを置けば違うVRMも使えます。また、.VRMファイルをドラッグ＆ドロップすることで起動後に変更することもできます。(VRM1は非対応)

#### 1-1-4. 実際に使うアプリを起動、SteamVRトラッカー初期設定
1. VMCProtocol でポーズを送信できるアプリ(使う場合)。出力ポートは39544としてください（VBTTool側も変えられるので一致していればOK）udpなので、送信側が先に起動していてOKですし、後から起動でもOKです。
2. SteamVR を起動します。Quest+VirtualDesktop(VD)の場合は、VDが認識するように起動してください。
3. VMT ManagerでVMTが有効であることを確認します。また、HMD のかわりにNull Driverでテストすることもできます。必要ならSteamVRを再起動したあと「VMT managerを終了」させてください。(慣れたらこの手順は省略できます)
4. JoyConを使う場合は、PCとBluetooth接続させてください。ペアリング済みではなく、**接続済み** になる必要があります。毎回一度削除して接続しなおさないと接続済みにならないようです。
5. VBTTools.exe を起動します。起動すると VMCProtocol の受信は即座に始まります。他のチェックは起動直後はオフになっています。VMCProtocol以外(Sending to VMTや JoyCon)は、通信相手の準備ができている状態でチェックをいれてください（先にチェックを入れてしまった場合、いったんオフにして入れなおせばOKです）
6. 初回起動時はSteamVRの設定で VMT_1, VMT_2 のトラッカーをハンドヘルドの左手/右手に割り当てる必要があります。(手首に設定した場合は後述の位置関係の調整が必要になります)

![Image 1](https://github.com/tbbsakura/VBTTools/blob/main/Assets/SakuraShop_tbb/VBTTools/etc/tracker_setting.png)
![Image 2](https://github.com/tbbsakura/VBTTools/blob/main/Assets/SakuraShop_tbb/VBTTools/etc/vmt1_setting.png)
![Image 3](https://github.com/tbbsakura/VBTTools/blob/main/Assets/SakuraShop_tbb/VBTTools/etc/vmt2_setting.png)


1でVMCProtocolのソフトを起動していれば、手の位置等がVBTToolsのVBMモデルに反映されるはずです。VMCProtocolのソフトを入れてない場合は、左上のListen To VMCPのチェックを外すと、手のボーンを動かすためのTest UIが表示されます。
Test UIには手の位置を動かす画面上のUIはまだありません。(JoyConの一時停止＋手を動かす機能で上下左右は動かせます。調整機能で手の位置を動かすことはできます)手を動かさないテストには後述の SkeletonPoseTester を使うと良いと思います。

## 2. 利用方法
### 2-1. 起動後の手順
VMTへの送信を開始する場合は右上のほうの Start Sending To にチェックを入れます（ポート番号等はVMT標準なのでそのままを推奨）Recvというところの赤い■が濃くなっていればHMDの位置情報を受信できています。できていない場合は色が薄くなってしまうので

- VMTが無効になっていないか（起動時アドオンが無効になっていないか）
- VMT Managerが起動していないか
- VBTTools.exe の受信を禁止していないか,複数起動していないか

を確認して、SteamVR、VBTTools の順番で起動しなおしてください。

JoyConが無い場合は下方の Use Button Panelを押すと画面でボタン等操作できます。(JoyConがうまく動かない場合もこちらを使ってください)
JoyConを使う場合は Use Joy Con (LR) にチェックを入れます。VBTTools起動後の初回チェック時は認識のため数秒固まります。認識時に長く(10秒以上等)固まった後に動かない場合、一度オフにしてオンにすると動く場合があります。(PC再起動後には数秒で認識するように戻る事が多いです)

JoyCon利用時は、通常のSteamVRのコントローラーとしての機能に加えて、**Y/左ボタンでトラッキング(VMCP受信)の一時停止とスティックでの手の位置移動機能**が使えます。一時停止を解除すると位置補正もなくなり通常の手の位置に戻ります。こちらの機能の概要は、[このXに投稿した動画](https://x.com/tbbsakura1/status/1827616092560486466) を参考にしてください。（2024/8/29以降のgithub最新版では、ListenToVMCPをオフにすると出てくるTestUIのスライダーでも位置調整できます）

SteamVRのダッシュボード画面で手の位置とコントローラーの位置が若干(数cm～10cm程度)ズレるのは仕様です(プレー画面と位置が変わります)。VMTのキューブ(コントローラー位置を示す)が頭・HMDと全然違う位置に出る場合(1m以上離れているような場合)はSteamVRとVMTのルームセットアップを確認してください。

### 2-2. SteamVR(Skeletal Input対応)アプリの起動
実際にSkeletal Input対応アプリを起動して動作を確認します。
VRMモデルの手をうごかせない状態（webカメラトラッキング等を設定していない場合）でTestUIを使う場合は [SkeletonPoseTester](https://github.com/gpsnmeajp/SkeletonPoseTester) を使って指の動きをテストできます。

手をうごかせる場合は(VMCProtocol送信アプリを使っている場合等) v0.1.0rc1 以降の Release ページで配布している [SteamVRHandTest](https://github.com/tbbsakura/VBTTools/releases/download/v0.1.0/SteamVRHandTest_v0.0.1.zip) を使うと向きの調整もしやすいです。
これらのアプリはSteam のUIからは起動できないので、SteamVR起動中に .exe ファイルを直接起動してください。

手を動かせる場合は、VRChat や Moondust Knuckles Tech Demos などで動作を確認できます。

### 2-3. 指が動かないときの確認事項
基本的に、VRアプリ側ではコントローラーをIndexコントローラーであると認識するように設定します。
JoyCon利用時にボタンと動作が異なる場合はここを確認してください。
SteamVRでのバインディング設定でIndexコントローラーのスケルトンの入力を無効化すると動かないので気を付けてください。

Quest + Virtual Desktop(VD) での利用の場合 VD での設定で Forward Tracking Data to PC はオフにします。
（オンにすると別の仮想コントローラーが認識されると思いますが、VMTの仮想コントローラーが優先されていればオンでも動作します）

VRChat で利用する場合は、設定画面で"Controls"(日本語だと「コントロール」) の項目の先頭にSteamVR とある部分で、以下の設定をします。
- **Exclusive Finger Tracking Mode オプションはオフ**（オンでも指は動きますがボタン等が効かなくなります）
- **Avatars Use Finger Tracking オプションをオン**(オフだと指が動かなくなります)

### 2-4. 手の位置と向きの調整
手の位置がおかしい場合は調整が必要です。(v0.1.0で、それ以前と設定値が変わっているため再調整が必要です)
また、この調整内容は、ゲーム中でのVRで選択するためのレイ(光線)ポインターの向きにも影響します。
Adjust UIのチェック3種類のいずれかを入れてスライダーを動かして調整でき、調整内容はjsonファイルに保存できます。VBTTools.exe と同じフォルダの default.jsonは起動時に読み込まれる設定値になります。
v0.1.0rc1と v0.1.0でもdefault.jsonの内容を変えており、最適を追求している最中です。

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

ユーザーとしての使い方は以上です。


## 3. Build方法(開発者向け)
Unity 2022.3.22f1 で開発しています。
必要物を一式プロジェクトにインポートしてから後述のシーンを開いてBuildします。

### 3-1. Unity project に別途入れる必要があるもの
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


### 3-2. MITライセンスで公開されているものの改変版が入っているもの
参考のためURL書いておきますが、後から入れると改変が上書きされてしまうので入れないでください。

1. [UnityWindowsFileDrag&Drop](https://github.com/Bunny83/UnityWindowsFileDrag-Drop)

2. [JoyConLib](https://github.com/Looking-Glass/JoyconLib/releases)


### 3-3. シーンファイル等
VBTTools.exe は　Assets/SakuraShop_tbb/VBTTools/Samples/VBTSample.unity を buildしたものです。
PlayerSetting 等はウィンドウサイズ可変にする等していますが、特殊な設定は特にしていないので普通にbuildできるかと思います。

### 3-4. OgLikeVMTと開発者向けサンプル
VBTTools.exe そのものがサンプルではありますが、OpenGlovesライクにVMTを使うための機能である OgLikeVMT だけのサンプルを別途用意してあります。

- VBTTools/Sample/SimpleOgLikeVMTSample.unity (シーンファイル) 
- VBTTools/Sample/Script/SimpleOgLikeVMTSample.cs (スクリプト)

詳細説明は [こちら](docs/OgLikeVMT.md)


