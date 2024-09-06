# VMT(Virtual Motion Tracker)の設定
## 1. VMT(Virtual Motion Tracker)のインストール・設定
[VMT - Virtual Motion Tracker v0.15](https://github.com/gpsnmeajp/VirtualMotionTracker/releases/tag/v0.15)

VMTは仮想コントローラーの処理に必要です。v0.15 で開発しています。
[VMTのドキュメント](https://gpsnmeajp.github.io/VirtualMotionTrackerDocument/setup/) に従いインストールと初期設定(ルームセットアップ)を行ってください。

## 2. VMTManagerの設定と、SteamVRトラッカーの設定
重要：この設定はVBTToolsを**起動していない状態**で設定します。
1. SteamVR を起動します。<br>
HMDがある状態で起動してください。ない場合はOpenTrackを先に設定してください。
2. SteamVRの左上の三本線をクリックして出るメニューから「設定」を選び、スタートアップ／シャットダウン設定画面の「アドオンの管理」を開きます。ここでvmtを使う設定になっていない場合は、設定変更します（SteamVR再起動が必要になります）<br><img src="img_vmt/vmt_SteamVR_ADDON.png" /><br>

3. SteamVRが起動した状態で、Windowsのスタートメニューから Virtual Motion Tracker を選択します。VMT付属のソフト VMT Manager が起動しますので、以下の設定をします。
- VMT Manager の Controlタブで Always Combatible のON を一度クリックします。（見た目は変わりません）<br><img src="img_vmt/vmt_alwaysCompatible.png" /><br>
- Input タブで Left(1)をクリックしてから [5]Left Compatibleをクリック。さらにRight(2)をクリックしてから [6]Right Compatibleをクリックします。<br><img src="img_vmt/vmt_mamanger_1526.png" /><br>
ここで、 SteamVR のパネルに VMTアイコンが2つあることを確認します。<br><img src="img_vmt/vmt_steamVr.png" /><br>

4. SteamVRの左上の三本線をクリックして出るメニューから「設定」を選び、VMT_1, VMT_2 のトラッカーをハンドヘルドの左手/右手に割り当てます。<table>
<tr>
<td><img width="100%" src="img_vmt/tracker_setting.png" />
</td>
<td>→</td>
<td><img width="100%" src="img_vmt/vmt1_setting.png" /></td>
<td><img width="100%" src="img_vmt/vmt2_setting.png" /></td>
</tr></table>
ここまで設定できたらVMTManager は閉じておきます。

## 3.VMTの注意点
- index1/2 (VMT_1, VMT_2) をIndexコントローラー互換として使うので他と被ると使えません。（非互換コントローラーを自動起動するような設定にしないでください）
- VMT Manager と VBTTools はどちらか一方,1つしか同時には使えません。設定時にはVMT Managerを使うのでVBTToolsは閉じておき、VBTToolsを使うときはVMT Managerを閉じておきます。また、複数起動しないように注意してください。
