# VRChatでHMDレスで3点トラッキングする設定
頭と手の位置把握をWebCamで、コントローラー操作をJoyConで行います。
PCがBluetooth対応である必要があります(JoyConのため)
カメラはある程度視野角が広くないとうまくいかないようです。
51度だとうまくいきませんでした。筆者は  Buffalo BSW305MBK(購入時2400円)を使っています。

## 設定
### 1. OpenTrack の準備
[こちら](./OpenTrackWithIDD.md)の通り設定してください。
マルチモニタでない場合はリンク先の説明に従い仮想モニタも設定してください。

### 2. VMT の準備
[こちら](./VMTSetting.md)の通り設定してください。

### 3. JoyCon 接続
[こちら](./JoyconConnect.md)の通り設定してください。

### 4. VRigUnity 
OpenSourceで今後使いやすそうなのでVRigUnityにしていますが
TDPTなどでも同じように送信先ポートを設定すれば使えます。

VRigUnityの場合は、[こちら](https://github.com/Kariaro/VRigUnity/releases/tag/v0.5.0)から、VRigUnity-Windows.zipをダウンロードして展開しておきます（もしくはインストーラー版でインストール）。使うときに中の VRigUnity.exe を起動します。
初回起動時はSettingボタンを押して以下の設定をします。
1. カメラ設定<br>
Cameraタブで、Sourceのところのカメラを選びます。また、左右反転したほうが使いやすい場合は、Is Horizontally Flipped にチェックを入れます。<br /><img width="60%" src="./img/vrigunity_camera.png" />
2. 送信先ポート設定<br>
AdvancedタブでVMC Sender の Port を39544 にします(VBTToolsのListenPortと一致していれば変更してもOKです。)<br /><img width="60%" src="./img/vrigunity_port.png" />
3. 別のVRMモデルを表示したいときは、ModelタブでSelect Modelします。

設定できたら赤い×ボタンを押します。

使うときは、Start Camera と Start Sender VMC ボタンを有効化します。有効になっているとボタンが赤くなり、Stop～に文言が変わります
<img width="60%" src="./img/vrigunity_running.png" />

### 5. VBTTools (0.2.0以降)

to be described...

SteamVRのダッシュボード画面で手の位置とコントローラーの位置が若干(数cm～10cm程度)ズレるのは仕様です(プレー画面と位置が変わります)。VMTのキューブ(コントローラー位置を示す)が頭・HMDと全然違う位置に出る場合(1m以上離れているような場合)はSteamVRとVMTのルームセットアップを確認してください。