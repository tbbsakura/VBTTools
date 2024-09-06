# VRChatでHMDレスで3点トラッキングする設定

[![使用例（youtube 動画）](img/youtube3pts-tn-play.jpg)](https://youtu.be/JelWOjQbNso)

頭と手の位置把握をWebCamで、コントローラー操作をJoy-Conで行います。
PCがBluetooth対応である必要があります(Joy-Conのため)
カメラはある程度視野角が広くないとうまくいかないようです。
51度だとうまくいきませんでした。筆者は  Buffalo BSW305MBK(購入時2400円)を使っています。

## I. 設定
### 1. OpenTrack の準備
[こちら](./OpenTrackWithIDD.md)の通り設定してください。
マルチモニタでない場合はリンク先の説明に従い仮想モニタも設定してください。

### 2. VMT の準備
[こちら](./VMTSetting.md)の通り設定してください。

### 3. Joy-Con 接続
[こちら](./JoyconConnect.md)の通り設定してください。

### 4. VRigUnity 
OpenSourceで今後使いやすそうなのでVRigUnityにしていますが
TDPTなどでも同じように送信先ポートを設定すれば使えます。

VRigUnityの場合は、[こちら](https://github.com/Kariaro/VRigUnity/releases/tag/v0.5.0)から、VRigUnity-Windows.zipをダウンロードして展開しておきます（もしくはインストーラー版でインストール）。使うときに中の VRigUnity.exe を起動します。
初回起動時はSettingボタンを押して以下の設定をします。
1. カメラ設定<br />
Cameraタブで、Sourceのところのカメラを選びます。また、左右反転したほうが使いやすい場合は、Is Horizontally Flipped にチェックを入れます。<br /><img width="60%" src="./img/vrigunity_camera.png" />
2. 送信先ポート設定<br />
AdvancedタブでVMC Sender の Port を39544 にします(VBTToolsのSettingでのListenPortと一致していれば変更してもOKです。)<br /><img width="60%" src="./img/vrigunity_port.png" />
3. 別のVRMモデルを表示したいときは、ModelタブでSelect Modelします。

設定できたら赤い×ボタンを押します。

使うときは、Start Camera と Start Sender VMC ボタンを有効化します。有効になっているとボタンが赤くなり、Stop～に文言が変わります。あらかじめその状態にしておいてOKです。<br />
<img width="60%" src="./img/vrigunity_running.png" />

### 5. VBTTools (0.2.0以降)
#### 5-1. インストールと起動
[リリースページ](https://github.com/tbbsakura/VBTTools/releases) からzipファイルをダウンロードして展開して、VBTTools.exe を起動します。
起動すると自動的にVRMモデルがロードされて、4でVRigUnityを有効にしていれば、同じ姿勢になっているはずです。なっていない場合は、VRigUnity の送信先ポート番号の設定がVBTToolsのSettingにおける受信ポートと一致しているか、Windows設定でVBTToolsの受信を許可しそこなって禁止していないか([詳しい確認方法はこちら](./check.md))、を確認します。<br />
<img width="50%" src="./img//vbttools_listenSetting.png" />

#### 5-2. 手の動きの動作確認
SteamVRを起動してから、右上のStart Sendingの Hand Pos to VMT のほうのチェックを入れると、手の動きが SteamVRに反映されます。（この時、Recvという赤い四角が濃い色になっていないと正しく動作しません。
Webカメラのほう、顔の前にもってきたときに、キューブ型のオブジェクトが見えていればOKです。

SteamVRのダッシュボード画面で手の位置とコントローラーの位置が若干(数cm～10cm程度)ズレるのは仕様です(プレー画面と位置が変わります)

全然画面内にこないときは、SteamVRとVMTのルームセットアップを再確認してください。(VMT Managerでの Room setup をしなおすだけで直ることもあります。)
それでもダメなときは、[こちら](./check.md)の内容を確認してください。

#### 5-3. 頭の動きの動作確認
右上のStart Sending の Head pos to OpenTrackのほうのチェックを入れます。設定が適切であれば、頭の動きに応じてSteamVRの画面が傾いたり向きを変えたりするはずです。

うまくいかない場合は [設定説明](docs/OpenTrackWithIDD.md)のとおりに設定できているか確認して、それでもダメなときは、[こちら](./check.md)の内容を確認してください。

#### 5-4. Joy-Conの利用
接続したJoy-Conを使う場合はVBTToolsの下方にある Use Joy Con (LR) にチェックを入れます。VBTTools起動後の初回チェック時は認識のため数秒固まります。認識時に長く(10秒以上等)固まった後に動かない場合、一度オフにしてオンにすると動く場合があります。(よくあります。PC再起動後には数秒で認識するように戻る事が多いです。)

Joy-Con接続ができると、左のYボタンあるいは右の◀ボタンを押すと、TestUIが出たり消えたりするはずです。
またHome/Captureボタンを押すと、SteamVRではダッシュボードが出たり消えたりするはずです。
うまくいかない場合はいったんのUse JoyCon(LR)のチェックを外して入れなおしてみてください。それでもダメな場合はBluetooth接続が「接続済み」になっているか再確認してください。

基本的に、VRアプリ側ではコントローラーをIndexコントローラーであると認識するように設定します。Joy-Con利用時にボタンと動作が異なる場合（スティックを倒したはずなのにAボタンの動作をする等の場合）はSteamVRがIndexコントローラーの設定になっているかを確認してください。

## II. VRChat を起動する
設定おつかれさまでした。いよいよVRChatを起動してみましょう。
### 1. VRChatコントローラ設定
基本的に、VRアプリ側ではコントローラーをIndexコントローラーであると認識するように設定します。

その上で、指を動かしたい場合は、VRChat の設定画面で"Controls"(日本語だと「コントロール」) の項目の先頭にSteamVR とある部分で、以下の設定をします。
- **Exclusive Finger Tracking Mode オプションはオフ**（オンでも指は動きますがボタン等が効かなくなります）
- **Avatars Use Finger Tracking オプションをオン**(オフだと指が動かなくなります)

SteamVRでのバインディング設定でIndexコントローラーのスケルトンの入力を無効化すると動かないので気を付けてください。

### 2. Joy-Con機能
割り当て詳細は[こちら](docs/JoyConAssign.md)を参照してください。

#### 2-1. Joy-Con の一時停止機能
Joy-Con利用時は、通常のSteamVRのコントローラーとしての機能に加えて、**Y/左◀ボタンでトラッキング(VMCP受信)の一時停止とスティックでの手の位置移動機能**が使えます。
また**A/右▶のボタンはトラッキングの停止と同時に、VRChatでのメニューが操作しやすい場所に手が移動します**。メニューの呼び出しは、Y/左で一時停止した後に呼び出すこともできます。

一時停止中は、TestUIが表示されており、スライダーでも手の位置を変更できます。
一時停止はY/左ボタンで解除できます。解除すると位置補正もなくなり通常の手の位置に戻ります。(A/右は後からメニューを呼び出せる機能があるため、一時停止解除には使えません)

また、左コントローラーのSRボタンを使うと、ヘッドトラッキングだけをオンオフできます。頭のトラッキングをしていると、頭を傾けたときにモニタは傾かないため画像がかなり揺れる感じになります。オフにしておいたほうが使いやすいことが多いので、オンオフできるようにしてあります。
こちらはオフにしたとき、その時点の傾きが維持されるので、なるべく頭をまっすぐにしてからオフにすると良いでしょう。


