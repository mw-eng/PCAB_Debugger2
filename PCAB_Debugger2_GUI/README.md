# PCAB Debugger GUI
これは基本となるGUI制御プログラムです。<br>
再構築に際してGPIB制御部および一部I/Fライブラリの見直しを行っていますが、メインの処理等はVer1.4.5から変更はありません。

## 動作検証環境
* Windows11 Professional x64
* .NET Framework 4.7.2 or later.
* VNA A.17.20.09 (64 bit)
* Keygisht IO Libraries Suite 18.2.27313.1 or later. @[Keysight](https://www.keysight.com/zz/en/lib/software-detail/computer-software/io-libraries-suite-downloads-2175637.html)
* NI-VISA 18.5 @[National Instruments](https://www.ni.com/ja/support/downloads/drivers/download.ni-visa.html#306043)

## 使用手順

### メインウィンドウ
設定および制御は本画面のみで行います。<br>
*画像は1対1通信時のウィンドウで、1対多通信時はユニット毎に"CONTROL"と"AUTO"で複数タブが生成されます。*<br>
<br><img src="https://github.com/mw-eng/PCAB_Debugger2/blob/master/PCAB_Debugger2_GUI/assets/UI1_Ex.png?raw=true" width="600px"><br>
<br>
${\color{Aqua} 1. \space 設定画面}$<br>
No | 項目 | 内容
:--|:--|:--
1-1 | Connect Button | PCABと通信を開始
1-2 | Serial Port | PCABに接続されているシリアルポートを選択する(チェックボックスにチェックが入っている物が有効となり最大3ポートまで同時に使用可能)
1-3 | Baud Rate | PCABと通信するボーレート(*BAUD RATE*)を選択
1-4 | Monitor loop inberval | センサモニタ(温度や電圧等)のデータ取得間隔をミリ秒で指定
1-5 | Serial Numbers | 通信対象のシリアル番号を指定<br>1対多通信を行う場合は","で区切り指定する。<br>ポートの行列表示順を指定する場合は@で区切り"SN1@Z,SN2@R"の様に指定する事も可能(表示指定コードを以下に示す)<br>Z : default<br>R : Rotate right 90 degrees<br>L : Rotate left 90 degrees<br>H : Rotate 180 degrees<br>ZM : default mirror<br>R : Rotate right 90 degrees and then mirror<br>L : Rotate left 90 degrees and then mirror<br>H : Rotate 180 degrees and then mirror


<br><img src="https://github.com/mw-eng/PCAB_Debugger2/blob/master/PCAB_Debugger2_GUI/assets/UI1.png?raw=true" width="600px"><br>
<br>
${\color{Gold} 2. \space CONTROL \space TAB}$<br>
No | 項目 | 内容
:--|:--|:--
2-1 | Standby AMP | チェックを入れた場合Standbyモードに設定
2-2 | Standby DRA | チェックを入れた場合Standbyモードに設定
2-3 | Low Power Mode | チェックを入れた場合Low Powerモードに設定
2-4 | View | ポートの行列表示順を変更
2-5 | Phase and ATT Config | DSAおよびDPSの設定値を選択<br>チェックボックスで設定する項目とは異なり書込み指示を行うまで設定が適用されないため注意
2-6 | Standby LNA | チェックを入れた場合Standbyモードに設定
2-7 | Set ATT Button | DSAの設定値をPCABに書込み
2-8 | Set PHA Button | DPSの設定値をPCABに書込み
2-9 | Set Button | DSAおよびDPSの設定値をPCABに書込み
2-1 | Read Button | 現在PCABに設定されている設定値を取得
2-1 | Save and Load Target | 設定値をメモリ(ROM)へ書込みと保存<br>0は自動読込み領域(0xE0 - 0)を指定
2-1 | Load MEM Button | 設定値をメモリ(ROM)へ書込み
2-1 | Save MEM Button | 設定値をメモリ(ROM)から読込み
2-1 | Preset Button | プリセットをメモリから読込み(工場出荷時の設定値を読込み)


<br><img src="https://github.com/mw-eng/PCAB_Debugger2/blob/master/PCAB_Debugger2_GUI/assets/UI2.png?raw=true" width="600px"><br>
<br>
${\color{LightSkyBlue} 3. \space AUTO \space TAB \space}$ Automatic measurement control<br>
No | 項目 | 内容
:--|:--|:--
3-1 | VISA Address | 計測器のVISAアドレスを指定
3-2 | Check Button | 計測器の接続確認
3-3 | Channel | データ取得を行うチャンネルを選択
3-4 | Mode Settings | Automatic trigger settingsにチェックを入れるとSingleトリガを実行してからデータ取得を行う
3-5 | Save target | データ取得を行うターゲットを選択<br>Screen:画像キャプチャ<br>Trace:トレースの数値データ(csv)
3-6 | File Name Header | 取得データのファイルヘッダ名を指定
3-7 | DPS Loop | DPSのループ処理を有効化
3-8 | DPS step | DPSのループ間隔を選択
3-9 | DPSn | DPSのループ対象系統を選択
3-10 | DSA Loop | DSAのループ処理を有効化
3-11 | DSA step | DSAのループ間隔を選択
3-12 | DSAn | DSAのループ対象系統を選択
3-13 | Waite Time | DPSおよびDSA設定変更後からデータ取得までの待ち時間をミリ秒で指定
3-14 | START Button | 自動測定開始ボタン

<br><br>

### モニタウィンドウ
センサモニタ(温度や電圧等)の取得データ表示画面<br>
ウィンドウはPCABと通信開始後に自動起動し、通信終了時に自動で閉じられます。<br>
*画像は1対1通信時のウィンドウで、1対多通信時はユニット毎に行列で表示されます。*<br>
<br>
<br><img src="https://github.com/mw-eng/PCAB_Debugger2/blob/master/PCAB_Debugger2_GUI/assets/UI3.png?raw=true" width="600px"><br>
<br>
${\color{Magenta} 4. \space Sensor \space display \space Window}$<br>
No | 項目 | 内容
:--|:--|:--
4-1 | CPU TMP | CPU温度(単位：℃)
4-2 | SNS Vin | Vin電圧(単位：V)
4-3 | SNS Pin | PreAMP検波電圧(単位：V)
4-4 | SNS Id | Id電流(単位：A)
4-5 | SNS Vd | Vd電圧(単位：V)
4-6 | TMP AVG | 1wire温度センサの平均値(単位：℃)
4-7 | Tempurature | 1wire温度センサのID(HEX値)と温度(単位：℃)

