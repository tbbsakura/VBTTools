# VBTTools
Virtual Body (VRM Body) Tracking Tool v0.0.3

リアルのボディにトラッカーを付けてトラッキングするのではなく、VRMモデルの姿勢をトラッキングして、Virtual Motion Tracker(VMT)に情報を渡して、SteamVR の仮想コントローラーとして利用しようとするものです。今のところ、以下のコントローラー入力に対応しています。

- VRMモデルの指の動きを読みとって、Indexコントローラー互換コントローラー(Skeletal Input対応)で出力
- VRMモデルの頭と手の相対位置およびHMDの位置情報から、Indexコントローラー互換コントローラーの位置・向きを出力
- JoyCon を使用して/もしくは画面上のuiで、ボタン・スティックの操作

VMCProtocol を受信できるので、TDPT、VSeeFace、などのソフトでWebカメラの情報で手の動きをトラッキングしてVRMに反映し、VRChat等で手を動かせます。位置情報を設定できるので、手首にVIVEトラッカーをつけたりする必要がありません。

v0.0.3 時点ではコントローラーしか対応していないので Virtual Body (VRM Body) "Hand/Finger" Tracking Tool と言うべき所ですが、開発者の方であれば、コントローラー以外のトラッカーもあわせて利用するのは割と簡単にできると思います。
（たとえば、VRMの頭の位置を仮想トラッカーに反映して、Tracking Override等できるはず）

## 暫定公開
v0.0.1～3 はまだ十分テスト等行えていません。不便だなと思う部分も修正できておらず、説明（この文書）も不十分です。プログラム的にカスタマイズ可能なこともUIに実装されていなかったりします。
すぐにでも試したい方、ソースコードを読んで自分で活用できる方向けです。


## 使用方法
事前に VMT (Virtual Motion Tracker)を入れておきます。

VMT - Virtual Motion Tracker
https://github.com/gpsnmeajp/VirtualMotionTracker/releases/tag/v0.15

全面的にお世話になっています。v0.15 で開発しています。
exe でインストーラーを走らせてください。

続いて、VBTTools の Releases(https://github.com/tbbsakura/VBTTools/releases) で公開している zip を解凍し、VBTTools.exe を起動します。
初回起動時はサーバーとして機能するためWindowsが確認ウィンドウを出すので通信許可をしてください。
VRMモデル(VRoid Studio Beta 用モデルとしてCC0で公開されている HairSample_Male くん)がロードされると思いますが、いったん終了します。
（VBTTools.exe と同じフォルダに同じ名前でVRMを置けば違うVRMも使えます）

#### VMTの注意点
- index1/2 (VMT_1, VMT_2) をIndexコントローラー互換(Enable 5/6)として使うので他と被ると使えません。（非互換コントローラーを自動起動するような設定にしないでください）
- HMDの位置情報をVMTから受け取る必要があるので、先にVMT Managerが起動していて VMT用ポートを listen していると使えません。

#### 改めて、以下の順番で起動します
1. (使う場合)VMCプロトコルでポーズを送信できるアプリ。出力ポートは39544としてください（VBTTool側も変えられるので一致していればOK）
2. SteamVR
3. VMT ManagerでVMTが有効であることを確認します。また、HMD のかわりにNull Driverでテストすることもできます。必要ならSteamVRを再起動したあと「VMT managerを終了」させてください。
4. JoyConを使う場合は、PCとBluetooth接続させてください。
5. VBTTools.exe を起動します。起動するとVMCPの受信は即座に始まります。
6. 初回起動時はSteamVRの設定で VMT_1, VMT_2 のトラッカーをハンドヘルドの左手/右手に割り当てる必要があります。

![Image 1](https://github.com/tbbsakura/VBTTools/blob/main/Assets/SakuraShop_tbb/VBTTools/etc/tracker_setting.png)
![Image 2](https://github.com/tbbsakura/VBTTools/blob/main/Assets/SakuraShop_tbb/VBTTools/etc/vmt1_setting.png)
![Image 3](https://github.com/tbbsakura/VBTTools/blob/main/Assets/SakuraShop_tbb/VBTTools/etc/vmt2_setting.png)


1でVMCPのソフトを起動していれば、手の位置等が反映されるはずです。VMCPのソフトを入れてない場合は、左上のListen To VMCPのチェックを外すと、手のボーンを動かすためのTest UIが表示されます。
手の位置を動かすTest UIはまだありません、ごめんなさい。手を動かさないテストには後述の SkeletonPoseTester を使うと良いと思います。

起動後の手順として
VMTへの送信を開始する場合は右上のほうの Start Sending To にチェックを入れます（ポート番号等はVMT標準なのでそのままを推奨）Recvというところの赤い■が濃くなっていればHMDの位置情報を受信できています。できていない場合は

- VMTが無効になっていないか（起動時アドオンが無効になっていないか）
- VMT Managerが起動していないか
- VBTTools.exe の受信を禁止していないか,複数起動していないか

を確認して、SteamVR、VBTTools の順番で起動しなおしてください。

JoyConが無い場合は下方の Use Button Panelを押すと画面でボタン等操作できます。(JoyConがうまく動かない場合もこちらを使ってください)

SteamVRの画面で手の位置とコントローラーの位置がかなりズレているのは仕様です。手の位置が表示されていないとかなり変にみえます。

実際にSkeletal Input対応アプリを起動して動作を確認します。
手をうごかせない状態でTestUIを使う場合は SkeletonPoseTester ( https://github.com/gpsnmeajp/SkeletonPoseTester )を使って指の動きをテストできます。
SteamVR起動中に SkeletonPoseTester を起動すると、PC画面で指の動作を確認できます。

VMCPアプリを使っている場合は、VRChat や Moondust Knuckles Tech Demos などで動作を確認できます。

### 問題となりそうな部分
1.
手の位置がおかしい場合は調整が必要です。Adjust UIのチェックを入れてスライダーを動かして調整でき、調整内容はjsonファイルに保存できます。（vbttools.exe と同じフォルダの default.jsonは起動時に読み込まれる設定値になります）
VMT_1/2をハンドヘルドに設定していれば、デフォルトの調整値で大きな差はないと思いますが、利用者が実質自分だけなので、確認できていないケースで必要になるかもしれません。
特に手首の位置をうごかさず回転だけさせたときに、VRでの手首の位置が動いてしまう場合は、手と仮想コンロローラーの位置関係がアプリケーションの想定と合っていない状態となり、かなり非実用的な動きになります。

2.
VRで選択するためのレイ(光線)ポインターの向きが使いにくくなっており、改善が必要だと認識しています。

ユーザーとしての使い方は以上です。


## Build方法
開発者向け情報です。Unity 2022.3.22f1 で開発しています。

### Unity project に別途入れる必要があるもの
テスト時バージョンと同じ unitypackage を入れるのが無難かと思います。全てMIT License で公開されているものです。

1. EVMCP4U (Eazy Virtual Motion Capture for Unity), External Receiver Pack 
 https://booth.pm/ja/items/1801535
    - テスト時バージョン: ExternalReceiverPack_v4_1.unitypackage

2. UniVRM (必要に応じて)
https://github.com/vrm-c/UniVRM/releases
    - 1 でエラー等でなければ入れなくていいと思いますが、
最新にしたい場合等は入れます。
    - テスト時バージョン: UniVRM-0.125.0_f812.unitypackage VRM0

3. uOSC 
https://github.com/hecomi/uOSC
    - テスト時バージョン： uOSC-v2.2.0.unitypackage

4. UnityStandaloneFileBrowser 
https://github.com/gkngkc/UnityStandaloneFileBrowser
    - テスト時バージョン：  1.2

新規のプロジェクトに1~4を入れてから、こちらのリポジトリの内容を追加すれば Build できると思います。


### MITライセンスで公開されているものの改変版が入っているもの
参考のためURL書いておきますが、後から入れると改変が上書きされてしまうので入れないでください。

1. UnityWindowsFileDrag&Drop (MIT License)
https://github.com/Bunny83/UnityWindowsFileDrag-Drop

2. JoyConLib  (MIT License)
https://github.com/Looking-Glass/JoyconLib/releases


## OgLikeVMTと開発者向けサンプル
VBTTools.exe そのものがサンプルではありますが、OpenGlovesライクにVMTを使うための機能である OgLikeVMT だけのサンプルを別途用意してあります。

- VBTTools/Sample/SimpleOgLikeVMTSample.unity (シーンファイル) 
- VBTTools/Sample/Script/SimpleOgLikeVMTSample.cs (スクリプト)

詳細説明は [こちら](docs/OgLikeVMT.md)


