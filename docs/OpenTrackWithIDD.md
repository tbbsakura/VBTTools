# OpenTrack with 仮想モニタ(Virtual Monitor Device: IDD Sample)
VBTTools というツールを制作するにあたり、SteamVRの仮想HMDとして利用している OpenTrack、OpenVR-OpenTrack および IDDSample についての説明です。
SteamVRで仮想HMDを使いたい方向けの説明です(VBTToolsを使わない方が読むことも想定して書いています。)

## 1.OpenTrack(.exe)
### 1-1. OpenTrack の概要
**OpenTrack** はオープンソースのヘッドトラッキングツールで、もともとSteamVR用ではなく、FreeTrack というプロトコルに対応したフライトシミュレーター等を想定して開発されたものっぽい雰囲気です。後述のSteamVR Driver **OpenVR-OpenTrack**と紛らわしいので、以下 文中では **OpenTrack.exe** と .exe をつけて呼びます。

### 1-2. OpenTrack のセットアップ
セットアップは [githubのReleaseページ](https://github.com/opentrack/opentrack/releases)からWindows用のsetup.exe をダウンロードして実行、起動するだけです。(現時点の2024年最新はバグが多いと書いてあるので、[安定板の2023.3.0](https://github.com/opentrack/opentrack/releases/tag/opentrack-2023.3.0)を使っています。)
UDPを受信する場合があるので、初回起動時または機能有効化時に、ネットワーク受信を有効にするかどうかの確認画面が出る場合があります。出た場合は許可する設定にしてください。

### 1-3. OpenTrack の入力と出力の設定、動作テスト
OpenTrack.exe の画面と Input選択肢<br>
<img width="40%" src="img_opentrack_idd/opentrack.exe.1.png" /> <img width="40%" src="img_opentrack_idd/opentrack.exe.2_input.png" /><br>

様々な入力に対応しており、検索するとマーカーを作る必要があるタイプで説明されていたりしますが、2024年8月時点ではWebcamがあればInputを neuralnet trackerにしてすぐトラッキングできます。Droidcamのようなスマホをカメラ化するツールでも使えます。<br>
ためしに、Outputは適当にfreetrackなどにしておいて使ってみましょう。Input を neuralnet trackerにして、Inputの設定画面を開いて、Trackerタブでカメラと解像度を選択してOKを押し、メイン画面右下のOKボタンを押せば、トラッキングが開始してタコの絵が頭にあわせて動くと思います。<br>
<img width="50%" src="img_opentrack_idd/opentrack.exe.4_TrackSetting.png" /><br>
回転の向きが逆になってしまう場合は、設定画面の Outputタブで、pitch,roll,yawそれぞれについて、Invertにチェックを入れることで反転させられます。位置x/y/zも反転させられるので、設定によってミラーにもできると思います。<br>
<img width="50%" src="img_opentrack_idd/opentrack.exe.3_outputsetting.png" />
<br>

Mappingをカスタマイズできるようですが特に必要性を感じなかったです。Filterは Accelaというデフォルトのをいじらずそのままでも問題なかったです。

OpenTrack.exeは出力先も複数対応しています(JoyStick やマウスの代わりにもできるっぽい？)が、SteamVRで仮想HMDとして使うには、2のOpenVR-OpenTrackを入れる必要があります。

## 2.OpenVR-OpenTrack (OpenTrackを SteamVR で使うための SteamVR Driver)
### 2-1. OpenVR-OpenTrack の概要
[**OpenVR-OpenTrack**](https://github.com/r57zone/OpenVR-OpenTrack) は、OpenTrackのトラッキング情報をSteamVRの仮想HMDとして使うための SteamVRドライバーです。
入力（OpenTrack.exeから見れば出力）は、FreeTrack 2.0 enhanced と UDP over network の2種類に対応しています。これらの入力ができれば、OpenTrack.exe は起動しなくても動作します。(OpenTrack.exeなしの動作は、筆者は実際の動作はUDPのみ確認済み)
どちらかのSteamVRドライバーを入れて使うので排他利用です。切り替えたい場合は Freetrack 版を入れておいて、OpenTrack.exe のほうで「Input = UDP over Network, Output = FreeTrack 2.0 enhanced」のようにすれば input を切り替えて使うことができます。
OpenTrack.exeを通すことで反転やマッピングカーブ定義等できることが増えるというメリットもあります。デメリットは起動アプリが増えると管理が面倒ということです。(VBTTools で UDP送信をする、以外の用途はだいたい FreeTrack版のほうが良いと思います)

### 2-2. OpenVR-OpenTrack の注意点
**OpenVR-OpenTrackはモニタを1つ占有する必要があります**。そのモニタで HeadView Windowを最大化していないとスリープモード（赤というか茶色一色の表示）になって、ゲーム画面が表示されなくなります。モニタを複数持っていない場合等は仮想モニタでも使えます。(詳細は後述)

### 2-3. OpenVR-OpenTrack のセットアップ、OpenTrack.exe の設定
SteamVRとOpenTrack.exe が既にインストールされていることが前提になります。

FreeTrack または UDP版の Driverの zip を [GithubのReleaseページ](https://github.com/r57zone/OpenVR-OpenTrack/releases)の下のほう、Assets のところからダウンロードして、SteamVR Driver のフォルダに入れます。
SteamVR Driver のフォルダは、普通に Steam/SteamVRをインストールしていれば
`C:\Program Files (x86)\Steam\steamapps\common\SteamVR\drivers\`
になりますが、インストール先を Program files (x86)以外に指定されている場合は適宜読み替えてください。

`SteamVR\drivers\` フォルダ内に opentrackフォルダができるよう入れればOKです。

OpenTrack.exe の設定は
FreeTrack版はOutputを freetrack 2.0 enhanced にすればOKです。
UDP版を入れた場合は、OpenTrack.exe のOutput は UDP over network とし、送信先IPアドレスを127.0.0.1 (localhost)に設定する必要があります。また、OpenTrack.exe の Input/Output どちらも UDP over network にする場合はポートは被らないよう一方を変更します。

### 2-4. SteamVR 側の設定（ルームセットアップ等）
他のHMDを接続していない状態でSteamVR を起動します。

SteamVR のステータスウィンドウ（HMDやトラッカーのアイコンが出るウィンドウ）のメニューからルームセットアップを行います。
いくつか選択が必要ですが、「小さい部屋」でキャリブレーションし、高さを入れるところは「170 cm」にします。
設定が終わったら、SteamVRチュートリアルは閉じて構いません。SteamVR HOME もSteamVR 設定で無効化しておけます。

OpenTrackを有効にして SteamVRを起動すると、左右2つの映像が表示されるウィンドウ(HeadView)が出てきます。
出ない場合は起動時のアドオンが無効になっている可能性があるので確認します。

HeadViewの配置を変更したい場合は OpenVR-OpenTrack の設定ファイルを編集する必要があります。
仮想モニタを使う場合は先にセットアップしてから行いますが、リアルモニタを使う場合は、後述 4 まで進んでください。

## 3. 仮想モニタ(IDD)の利用
### 3-1. IDD(Indirect Display Driver)の概要
2-2で書いた通り、OpenVR-OpenTrack はモニタを1つ占有する必要があります。
ここでは、Indirect Display Driver (IDD)の [ge9さん版fork](https://github.com/ge9/IddSampleDriver)でSteamVR起動を確認できました。
[経緯記事(日本語)](https://turgenev.cloudfree.jp/wiki/Windows%E3%81%AB%E3%81%8A%E3%81%91%E3%82%8B%E4%BB%AE%E6%83%B3%E7%9A%84%E3%83%87%E3%82%A3%E3%82%B9%E3%83%97%E3%83%AC%E3%82%A4)もあります。

### 3-2. IDD のインストール
1. [リリースページ](https://github.com/ge9/IddSampleDriver/releases)からドライバのzip(IddSampleDriver.zip)をダウンロードして解凍します。
2. **ドライバを入れる前**に、C:\IddSampleDriver\option.txt を置きます（重要、だそうです）<br>C:\の直下にzipを展開すると IddSampleDriver フォルダが出来て、中に options.txt が入っているのでちょうど良いと思います。

3. 次にドライバ署名確認をクリアするため、コマンドプロンプトを管理者権限で起動します。(起動の仕方がわからないときは Windowsキー押した後に半角で cmd とタイプしてみてください)
<br><img width="50%" src="img_opentrack_idd/cmd_prompt_idd.png" /><br>
画像のうち、黄色い部分が入力した内容です。最初はC以外のドライブに展開した場合はそのドライブに移動してます。展開先がZドライブだったで `z: [Enter]` で移動しています。(Windowsのパスは大文字小文字を区別しません)<br>
さらに展開したディレクトリに`cd`コマンドで移動します。<br>移動先ディレクトリ名は Exlporerの上部でコピーし、コマンドプロンプトを右クリックすれば貼り付けできます。<br>
移動できたら `installCert.bat[enter]`と押せば証明書の設定が完了します。<br>コマンドを全部打つのがだるい場合 `ins[tab][enter]`でいけると思います。(`in[tab]連打`でもそのうち出てきます)<br>
終わったら×ボタンもしくは `exit[enter]` で終了します。

4. (infを触ったりせずに)デバイスマネージャーからドライバをインストールします。まずWindowsタスクバーのWindowsアイコン右クリックメニュー等からデバイスマネージャーを開き、どれか1つデバイスを選んで（どれでもOK）から、<br>
メニューから操作(A)、レガシ ハードウェアの追加、を選びます。<br>
次へを押して、さらに「一覧から選択したハードウェアをインストールする」を選び次へ。<br>
すべてのデバイスを表示、をダブルクリックし、ディスク使用をクリックし、参照を押します。<br>
ファイル選択になるので、1で展開したフォルダに行き、iddsampledriver.inf を選んで前向きな回答を続けて完了させます(筆者環境では「開く」「OK」「次へ」「次へ」「完了」)<br>
(Windows10などで、デバイスの種類を選択する場合があるようで、その場合はディスプレイアダプターを選択します)

5. 再起動を求められたら再起動します。

## 4. モニタ配置とOpenVR-OpenTrackの設定ファイルの修正
以下の説明は、仮想モニタでもリアルモニタでも同じですので、リアルモニタが複数ある場合はいったんリアルモニタで試すとわかりやすいかもしれません。

## 4-1.モニタの配置例：
<img width="50%" src="img_opentrack_idd/idd_right_to_primary.png" />

2がプライマリモニタで、1は現実のサブモニタ（説明には使わないので無視してください）、3が仮想モニタです。右があいてない場合もあるかと思いますが、設定が簡単になる条件は
- 仮想モニタをプライマリモニタと**隣接**した場所に配置
- プライマリの**上か下に置く場合は左をあわせる、左か右に置く場合は上をあわせる**

です。以下、その4パターンのどれかであること前提で説明します。

また数値例として、プライマリモニタの解像度を 1920 x 1080、仮想モニタを 640 x 480 であるとして記載しますので、違う解像度のモニタの場合は書き換えてください。

## 4-2. 設定ファイルの場所
モニタの位置関係やサイズに応じて、OpenVR-OpenTrackの設定ファイルをメモ帳などで編集します。
OpenVR-OpenTrackの設定ファイルは2-3で手動で入れた時の drivers フォルダから見て `drivers\opentrack\resources\settings\default.vrsettings`
にあります。<br>
C:\Program Files (x86) に入っている場合は、<br>
`C:\Program Files (x86)\Steam\steamapps\common\SteamVR\drivers\opentrack\resources\settings\default.vrsettings` です。

## 4-3. 設定内容
編集する項目は4つあります。うち2つ、`windowHeight`と`windowWidth`は仮想モニタのサイズにあわせます。比率は気にしなくてOKです。
```json
      "windowHeight" : 480,
      "windowWidth" : 640,
```
このファイルを編集するときに、**数字の後ろにカンマがあるかないかは重要**なので、元のファイルを変更しないように気を付けてください。(おかしくなったらzipの中の元ファイルからやりなおしましょう)

さらに、`windowX` と `windowsY` を設定しますが、これは**プライマリモニタの左上から見た、仮想モニタ左上の座標** を記述します。解りにくいので4つの配置例ごとに説明します。
<hr>

**プライマリモニタの右に仮想モニタ**を置いた場合、プラマリモニタの幅1個分右(+1920)の場所に仮想モニタがあり、Y方向(上下)は一致させてあれば差異ゼロなので
```json
      "windowX" : 1920,
      "windowY" : 0
```
となります。
<hr>

**プライマリモニタの左に仮想モニタ**を置いて上が一致している場合は、マイナス方向に仮想モニタ1個の幅分左(-640)で
```json
      "windowX" : -640,
      "windowY" : 0
```
<hr>

**プライマリモニタの上に仮想モニタ**の場合、左をあわせていればXはゼロでよくて、Yは仮想モニタ1個分上(-480)になります。
```json
      "windowX" : 0,
      "windowY" : -480
```
<hr> 

**プライマリモニタの下に仮想モニタ**の場合、Yはプライマリモニタ1個分下なので
```json
      "windowX" : 0,
      "windowY" : 1080
```
です。

プライマリモニタと隣接していない場合や基準位置（上か左）を合わせていないときは設定が複雑になりますが、プライマリモニタの左上から見た、仮想モニタ左上の座標を適切に設定すれば動作すると思います。

### 4-4. 参考情報
OpenVR-OpenTrack の設定ファイルの更新に関しては以下のサイトを参考にしました。
違う説明のほうがわかりやすいかもしれないので、適宜参照してください。

[XREAL AirをWindows PCでSteamVRのHMDとして使う](https://note.com/domtaro/n/nbdf732223dfc)



