# うまくいかないときの確認事項
## Network全般
1. 受信側のWindowsOSでの通信許可は適切か？つまり、この画面できちんと許可したかどうか。<br /><img src="./img/network_permit.png" width="60%"/><br/>
一度キャンセルすると受信禁止になるので、記憶があいまいなとき・わからないときは、ウィンドウズのファイヤーウォールの設定を確認します。<br />
<img src="./img/win-defend.png" width="30%"> <img src="./img/defender_setting.png" width="65%" /><br />画像のように受信の規則を見て、アプリケーションの一覧で受信がブロックされていないかを確認し、されている場合は、その行を選択して削除します。（なお、「パブリック」の設定はブロックで問題ありません）削除とアプリを起動しなおせばまた確認画面が出ますので、許可をします。

2. 送信元ツールの送信先IPアドレスが127.0.0.1等正しく設定されているか(同一PCであれば常にこれでOK)
3. 送信元の送信先ポートと、受信側の受信(listen)ポートは一致しているか

## VMCP送信ツール(VRigUnity, TDPT, etc.)と VBTTools の接続確認
1. カメラの選択は適切か
2. Network全般の設定(特に1,3)は適切か
3. VBTToolsを複数起動していないか
4. 双方でVRMモデルが表示されているか
5. 表示されているVRMモデルは同じポーズになっているか

## VBTTools と VMT の接続確認
1. SteeamVR の設定で、スタートアップ／シャットダウン設定画面の「アドオンの管理」でvmtは有効になっているか
2. VBTTools起動前に VMT Manager を閉じているか
3. Network全般の設定(特に1,3)は適切か
4. VMTのルームセットアップを適切に行っているか
5. VBTTools の Hand Position to VMT のチェックは入っているか
6. VBTTools 右上の Recvという赤い四角が濃い色になっているか
7. VBTTools で Use Button Panel をチェックして出てくるSystemボタンを押したとき、SteamVRでダッシュボードがオンオフされるか
8. VBTTools の Listen to VMCP のチェックを外して、TestUI で下のほうの Left または Right Hand Posのスライダーをいろいろ動かしたときに、手またはVMTのキューブが見えることがあるか


## OpenTrack 全般
1. SteeamVR の設定で、スタートアップ／シャットダウン設定画面の「アドオンの管理」でOpenTrackは有効になっているか
2. SteamVRのVRビューを開いているか
3. VRビューが赤画面になっていないか（OpenTrackのHeadset Windowsが特定のモニタでアクティブになっているか）

## VBTTools と  OpenVR-OpenTrack の接続確認(UDP版の場合)
1. OpenTrack.exe を起動していないか（UDP版 OpenVR-OpenTrackを使う場合は閉じておくのが安定）
2. Network全般の設定(特に1,3)は適切か

## VBTTools と OpenVR-OpenTrack の接続確認(FreeTrack版の場合)
1. OpenTrack.exe を経由する場合、OpenVR-OpenTrackは freetrack 版を入れているか
2. OpenTrack.exeが必要な場合、起動しているか
3. OpenTrack.exeのinput/outputはUDP over Netowrk/freetrackにしているか。
4. OpenTrack.exe の Input設定で、port が 4242 になっているか。また、VBTTools の Setting で OpenTrackの portが4242になっているか

