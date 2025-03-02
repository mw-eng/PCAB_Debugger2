# PCAB Debugger2 RP2040
これはPCAB(LX00-0004-00)に書込むRP2040用の本体ファームウェアです。<br>
再構築に際して複数Coreを使用する事で1ワイヤ温度センサの取得時間改善を行っていますが機能やコマンドはVer1.4.xから変更はありません。

## フラッシュROM領域
ファームウェアはフラッシュROMの先頭に格納され、ファームウェアのサイズに応じて使用される領域は異なります。<br>
フラッシュROMのラストブロックにはSTATE情報と工場設定が格納され、最後の16バイトには15文字のシリアル番号がASCIIコードとラストバイトには文字数が格納されます。<br>

## RS485シリアルポート(URAT)の設定
* ボーレート : 1,000,000 [bps] @ 工場出荷時のデフォルト
* データビット : 8 [bit]
* パリティビット : None
* ストップビット : 1 [bit]
* フロー制御 : None

## RS485シリアル通信プロトコル
### ASCII通信モード
*#{SERIAL NUMBER}* または *${ROM ID}*, *{COMMAND}*, *{ARGUMENTS}*の順にスペースで区切り *{EXIT CODE}* を最後に追加してコマンドを送信します。<br>

### Binary通信モード
*#{SERIAL NUMBER}*, *0xFF*, *{COMMAND and ARGUMENT}*, *{EXIT CODE}*の順にバイナリデータを送信します。<br>
ROM IDを使用する場合は、*0x24{ROM ID(8byte)}*, *{COMMAND and ARGUMENT}*, *{EXIT CODE}*の順(0xFFは不要)でバイナリデータを送信します。
※このモードで送受信を行うバイナリデータは[SLIP](https://en.wikipedia.org/wiki/Serial_Line_Internet_Protocol) *(Serial Line Internet Protocol)*に準拠する必要が有ります。<br>

<details>
<summary>各要素の説明</summary>

*{SERIAL NUMBER}* には通信相手のシリアル番号(ASCIIコード)を指定します。但し、*"\*"* を指定した場合は全てのシリアル番号に対して通信を行います。<br.>
*{COMMAND}* と *{ARGUMENTS}* については [*Command Lists*](#コマンドリスト) を参照してください。<br>
*{EXIT CODE}* はASCII通信モードの場合は *\n(Line Feed Code)* または *\r(Carriage Return Code)* または *\r\n* の全てに対応しており、Binary通信モードの場合はSLIPに準拠するため0xC0のみとなります。<br>
※CUIモードでechoが有効となっている場合は *\r(CR)* をGUIモードやCUIモードでechoが無効かつローカルエコーを有効に設定している場合は *\n(LF)* を推奨します。

### コマンド例
- #0010 WrtDPS
- #0001 SetSTB.AMP true
- #* GetIDN

</details>

## コマンドリスト
RS485シリアル通信コマンドリスト

<details>
<summary>Digital Phase Shifter / Digital Step Attenuatorの制御コマンド</summary>
位相器およびデジタル可変ATTの値はバッファに入力後、書込み処理を行うことで設定が行われます。<br>
また、設定値はバイナリ値を10進数で指定します。(6ビット位相器で最下位ビットが5.625degの場合に180degを指定するためには32を指定する)

コマンド | 内容
:--|:--
WrtDPS | 位相器に設定値を書込む
GetDPS {0/1/false/true/bf/now} {x} | 位相器の設定値を取得します。<br>{1/true/now} : 現在設定されている値を取得<br>{0/false/bf} : バッファの設定値を取得<br>{x} : 位相器の番号を指定({0}は全ての値を取得)します。
SetDPS {x} {DEC} | バッファに設定を入力します。<br>{x} : 位相器の番号(1～15)を指定<br>{DEC} : 10進数の設定値
WrtDSA | デジタル可変ATTに設定値を書込む
GetDSA {0/1/false/true/bf/now} {x} | デジタル可変ATTの設定値を取得します。<br>{1/true/now} : 現在設定されている値を取得<br>{0/false/bf} : バッファの設定値を取得<br>{x} : デジタル可変ATTの番号を指定({0}は全ての値を取得、{16}は入力の値を取得)します。
SetDSA {x} {DEC} | バッファに設定を入力します。<br>{x} : デジタル可変ATTの番号(1～16)を指定({16}は入力のデジタル可変ATT番号)<br>{DEC} : 10進数の設定値

</details>
<details>
<summary>モードの設定および取得コマンド</summary>

コマンド | 内容
:--|:--
GetSTB.AMP | AMP STBYの取得
SetSTB.AMP {0/1/false/true}| AMP STBYの設定<br>{1/true} : Standby MODE<br>{0/false} : Run MODE
GetSTB.DRA | DRA STBYの取得
SetSTB.DRA {0/1/false/true}| DRA STBYの設定<br>{1/true} : Standby MODE<br>{0/false} : Run MODE
GetSTB.LNA | LNA STBYの取得
SetSTB.LNA {0/1/false/true}| LNA STBYの設定<br>{1/true} : Standby MODE<br>{0/false} : Run MODE
GetLPM | low power modeの取得
SetLPM {0/1/false/true} | low power modeの設定<br>{1/true} : Low Power MODE<br>{0/false} : Full Power MODE

</details>
<details>
<summary>各センサ情報の取得</summary>

コマンド | 内容
:--|:--
GetTMP.ID {x} | 温度センサIDの取得<br>{x} : 温度センサIC番号(1～15)<br>{0} を指定した場合は全ての温度センサIDを取得
GetTMP.Val {x} | 温度センサ値の取得<br>{x} : 温度センサIC番号(1～15)<br>{0} を指定した場合は全ての温度センサ値を取得
GetTMP.CPU | CPU温度の取得
GetVd | Vd値の取得
GetId | Id値の取得
GetVin | Vin値の取得
GetPin | Pin値の取得

</details>
<details>
<summary>その他のコマンド</summary>

コマンド | 内容
:--|:--
SMEM ({x}) ({y-z}\|{z}) | 設定をメモリ(ROM)に保存<br>但し、保存可否はブートモードに依存<br>{z} を 0 または未指定にすることでデフォルト設定を保存 ({z} は 0~3を指定可能)<br>{y-z}を指定した場合は、指定した設定番号に書込み({y} は0～15を指定可能)<br>{x}はセクタ番号を指定し、ユーザが使用可能なセクタ番号は0～13です。<br>*14はセクター番号を指定しない場合のデフォルト設定領域で15は工場出荷時のデータ格納領域です。*<br>*セクタ番号を指定する事で 15×16×4 (=960) 個の設定が保存可能です。*<br>*自動ロードは未指定の {z} (セクタ番号 14, 設定番号 0 )に保存された設定値を使用します。*
LMEM ({x}) ({y-z}\|{z}) | メモリ(ROM)から設定を読込み<br>引数はSMEMと同一
GetMODE | ブートモードの取得
GetIDN | デバイス識別子(バージョン等)を取得
*IDN? | GetIDNに同じ
GetIDR | ROM識別子を取得
ECHO {0/1/false/true} | エコーモードを設定<br>*複数のユニットを接続(RS485の1対多状態に)している場合は有効にしないこと*<br>{1/true} : エコー有<br>{0/false} : エコー無
CUI {0/1/false/true} | CUIモードの指定<br>{1/true} : CUIモード<br>{0/false} : GUIモード(1行で応答)<br>デフォルト(起動時)はCUIモード
RST | 工場出荷時の設定に戻す<br>*PS all 0<br>DSA all 2dB(No,0 = 0dB)<br>STB all 0(RUN MODE)<br>LPM 0(Full Power MODE)*
*RST | RSTに同じ
Reboot | CPUのリブート
SetBR {x}| ボーレートの変更<br>{x} : ボーレート値
BCM | バイナリ通信モードに切替え

</details>
<details>
<summary>メンテナンスコマンド</summary>

コマンド | 内容
:--|:--
SetSN {x} | *メンテナンスモードでのみ変更可能*<br>ボードのシリアル番号を設定<br>{x} : シリアル番号文字列
RROM {x-yz} | ROMのページデータ読込み<br>{x-yz} : *ブロック番号(x), *セクタ番号(y) + ページ番号(z)* を16進数で指定
WROM {x-yz} {HEX} | ROMのページデータ書込み.<br>{x-yz} : *ブロック番号(x), *セクタ番号(y) + ページ番号(z)* を16進数で指定<br>{HEX} : 書込むデータ(16進数文字列)<br>*データは消去されず上書きのみ(要EROM)*
EROM {x-y} | ROMのページデータ消去<br>{x} : *ブロック番号(x)* と *セクタ番号(y)* を16進数で指定
OROM {x-yz} {HEX} | ROMのセクタデータを上書き<br>{x-yz} : *ブロック番号(x), *セクタ番号(y) + ページ番号(z)* を16進数で指定<br>{HEX} : 書込むデータ(16進数文字列)<br>*データは消去(EROM)後に書込み*

</details>

<details>
<summary>バイナリ通信モードコマンド</summary>

コマンド | 内容
:--|:--
0xC0 | データフレーム終了コード
0xFF | シリアル番号の区切りコード
0xB0 {Byte} | 入力ATTにバイトデータ書込み
0xC1 {Binary} | DSA(Digital Step Attenuator) にバイナリデータを書込み<br>バイナリデータはDSA番号1～15の順に1バイト(8bit)で指定する。(計15byte)<br>ASCII通信モードとは異なり、設定と同時に書込みが実行される
0xC2 {Binary} | DPS(Digital Phase Shifter) にバイナリデータを書込み<br>バイナリデータはDSA番号1～15の順に1バイト(8bit)で指定する。(計15byte)<br>ASCII通信モードとは異なり、設定と同時に書込みが実行される
0xC3 {0x00/0x01} | AMP STBYの設定<br>{0x00} : Run MODE<br>{0x01} : Standby MODE
0xC4 {0x00/0x01} | DRA STBYの設定<br>{0x00} : Run MODE<br>{0x01} : Standby MODE
0xC5 {0x00/0x01} | LNA STBYの設定<br>{0x00} : Run MODE<br>{0x01} : Standby MODE
0xC6 {0x00/0x01} | low power modeの設定<br>{0x00} : Full Power MODE<br>{0x01} : Low Power MODE
0xD0 | 入力ATTの設定値取得<br>応答データは0xB0のバイトデータに同じ
0xD1 | DSAの設定値取得<br>応答データは0xC1に同じ
0xD2 | DPSの設定値取得<br>応答データは0xC2に同じ
0xD3 | AMP STBYの状態取得
0xD4 | DRA STBYの状態取得
0xD5 | LNA STBYの状態取得
0xD6 | low power modeの状態取得
0xE1 | 全ての温度センサICを取得<br>8 byte * 15
0xE2 | 全ての温度センサ値を取得<br>2 bytes * 15 の生値(温度にする場合、27.0f-{[DAT]/16.0f-0.706f}/0.001721f)
0xE3 | CPU温度(AD)値を取得<br>2 bytes の生値(温度にする場合、[DAT]*3.3f/4096)
0xE4 | Vd(AD)値を取得<br>2 bytes の生値(Vd電圧にする場合、[DAT]*3.3f/1024 * 10.091f)
0xE5 | Id(AD)値を取得<br>2 bytes の生値(Id電流にする場合、{[DAT]*3.3f/1024 - 0.08f} / 0.737f)
0xE6 | Vin(AD)値を取得<br>2 bytes の生値(Vin電圧にする場合、[DAT]*3.3f/1024 * 15.0f)
0xE7 | Pin(AD)値を取得<br>2 bytes の生値(電圧にする場合、[DAT]*3.3f/1024)
0xEA | ブートモードの取得<br>1 byte
0xEE | 全てのアナログ値を取得<br>応答データは {Vd(2byte) + Id(2byte) + Vin(2byte) + Pin(2byte) + CPU Temp(2byte)} の生データ
0xEF | 全てのセンサ値を取得<br>応答データは {AnalogValues(10byte)+TempratureData(2byte * 15)}
0xF0 | デバイスの識別文字列を取得(ASCIIコード)
0xFA | 工場出荷時の設定に戻す<br>PS all 0<br>DSA all 2dB(No,0 = 0dB)<br>STB all 0(RUN MODE)<br>LPM 0(Full Power MODE)
0xFB {Address} | 設定をメモリ(ROM)に保存<br>条件等はASCIIのSMEMに同じ<br>{Address} を 0x00 または未指定にすることでデフォルト設定の保存<br>セクタ番号(4 bits), 設定番号 (4 bitsで0x00～0x03まで)をしていすると指定したアドレスに書込まれます。(デフォルトのアドレスは{0xE0}{0x00}になる)
0xFC {Address} | メモリ(ROM)から設定を読込み<br>アドレスは0xFBに同じ
0xFD {BAUD RATE} | ボーレートの変更<br>{BAUD RATE} : ボーレート値を4byteで指定
0xAA {Address} | ROMのセクタデータ読込み<br>{Address(3byte)} : セクタ毎のアドレスを指定
0xBB {Address} {Binary} | ROMのセクタデータ書込み<br>{Address(3byte)} : セクタ毎のアドレスを指定<br>{Binary(4096byte)} : 書込むセクタデータ
0xFE | ASCII通信モードに切替え

応答コード | 内容
:--|:--
0xC0 | データフレーム終了コード
0x00 | 処理に成功
0xF1 | コマンドが存在しないエラーコード
0xF2 | 想定外のデータ長が指定されたエラーコード
0xFE | その他のエラーコード
{binary} | 取得時のバイナリデータ

</details>

## ハードウェアスイッチ構成
オンボードハードウェアスイッチ(SW1) の状態による設定一覧<br>
*SW1およびSW2は出力ピンに割当てられているため、OFF状態に固定する必要が有る*

<img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_RP2040/assets/SW1.png?raw=true" width="100px"> 0 = OFF(H) / 1 = ON(L)  
  
<details open>
<summary>スイッチ状態 0x00 ～ 0x0F</summary>

Number | SW4 | SW3 | SW2 | SW1 | HEX | Stateus | 内容
:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--
0 | 0 | 0 | 0 | 0 | 0x00 | Default | デフォルト状態で起動<br>*DPS(@All) = 0deg*<br>*DSA(@Input) = 0dB*<br>*DSA(@All except input) = 2dB*<br>*ALL Active Mode*
1 | 0 | 0 | 0 | 1 | 0x01 | Auto Load Boot. | ROMのデフォルト値に保存された設定を読込んで起動を有効化
2 | 0 | 0 | 1 | 0 | 0x02 | Allow settings to be saved. | 設定のROMに保存を有効化
3 | 0 | 0 | 1 | 1 | 0x03 | Auto Load Boot.<br>and<br>Allow settings to be saved. | Auto LoadおよびAllow settings to be savedの有効化<br>*基本は本状態で使用する*
4 | 0 | 1 | 0 | 0 | 0x04 | State4 | Unused.
5 | 0 | 1 | 0 | 1 | 0x05 | State5 | Unused.
6 | 0 | 1 | 1 | 0 | 0x06 | State6 | Unused.
7 | 0 | 1 | 1 | 1 | 0x07 | State7 | Unused.
8 | 1 | 0 | 0 | 0 | 0x08 | State8 | Unused.
9 | 1 | 0 | 0 | 1 | 0x09 | State9 | Unused.
10 | 1 | 0 | 1 | 0 | 0x0A | Factory reset on boot. | 工場出荷時のデフォルト設定で起動し、自動ロードの設定を初期状態に復元する<br>*デフォルト設定領域外については変更されない*
11 | 1 | 0 | 1 | 1 | 0x0B | State11 | Unused.
12 | 1 | 1 | 0 | 0 | 0x0C | State12 | Unused.
13 | 1 | 1 | 0 | 1 | 0x0D | State13 | Unused.
14 | 1 | 1 | 1 | 0 | 0x0E | State14 | Unused.
15 | 1 | 1 | 1 | 1 | 0x0F | Boot in the maintenance mode. | 管理者モードで起動<br>工場出荷時の設定やシリアル番号が保存されている領域等も変更できてしまうため*原則使用しない*

</details>