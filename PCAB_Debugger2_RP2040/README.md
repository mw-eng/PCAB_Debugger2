# PCAB Debugger2 RP2040
これはPCAB(LX00-0004-00)に書込むRP2040用の本体ファームウェアです。
再構築に際して1ワイヤ温度センサの取得時間改善を行っています。

## フラッシュROM領域
ファームウェアはフラッシュROMの先頭に格納され、ファームウェアのサイズに因って使用される領域は異なります。<br>
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
SetSN {x} | *Can only be changed in maintenance mode.*<br>Set Bord SN.<br>{x} : Serial Number strings.
RROM {x-yz} | Read page data from ROM.<br>{x-yz} : Specify the *block number(x), *sector number(y) + page number(z)* in hexadecimal format, separated by "-".
WROM {x-yz} {HEX} | Write page data to ROM.<br>{x-yz} : Specify the *block number(x), *sector number(y) + page number(z)* in hexadecimal format, separated by "-".<br>{HEX} : HEX data to write.<br>*Data will not be erased.*
EROM {x-y} | Erase page data from ROM.<br>{x} : Specify the *block number(x)* and *sector number(y)* as hexadecimal format separated by "-".
OROM {x-yz} {HEX} | Overwrite sector data to ROM.<br>{x-yz} : Specify the *block number(x)* and *sector number(y) + page number(z)* as hexadecimal format separated by "-".<br>{HEX} : HEX data to write.<br>*Data is written after erasing.*

</details>

<details>
<summary>バイナリ通信モードコマンド</summary>

コマンド | 内容
:--|:--
0xC0 | Frame end code.
0xFF | SerialNumber separator code.
0xB0 {Byte} | Write Byte data to the input attenuator.
0xC1 {Binary} | Write binary data to the digital step attenuator.<br>The binary data must be specified in the order of DSA numbers 1 to 15, and each DSA setting must be specified in 8 bits ( i.e. 15 bytes of data ).
0xC2 {Binary} | Write binary data to the digital phase sifter.<br>The binary data must be specified in the order of DPS numbers 1 to 15, and each DPS setting must be specified in 8 bits ( i.e. 15 bytes of data ).
0xC3 {0x00/0x01} | Set AMP STBY.<br>{0x00} : Run MODE<br>{0x01} : Standby MODE
0xC4 {0x00/0x01} | Set DRA STBY.<br>{0x00} : Run MODE<br>{0x01} : Standby MODE
0xC5 {0x00/0x01} | Set LNA STBY.<br>{0x00} : Run MODE<br>{0x01} : Standby MODE
0xC6 {0x00/0x01} | Set low power mode.<br>{0x00} : Full Power MODE<br>{0x01} : Low Power MODE
0xD0 | Get input attenuator settings.<br>The response data is in the same binary format as it was written.
0xD1 | Get digital step attenuator settings.<br>The response data is in the same binary format as it was written.
0xD2 | Get digital phase sifter settins.<br>The response data is in the same binary format as it was written.
0xD3 | Get AMP STBY.<br>The response data is in the same binary format as it was written.
0xD4 | Get DRA STBY.<br>The response data is in the same binary format as it was written.
0xD5 | Get LNA STBY.<br>The response data is in the same binary format as it was written.
0xD6 | Get low power mode.<br>The response data is in the same binary format as it was written.
0xE1 | Get all temperature sensor IDs.<br> 8byte * 15
0xE2 | Get all temperature data.<br>The response data is 2 bytes * 15 of raw data.
0xE3 | Get CPU Temperature(AD Value).<br>The response data is 2 bytes of raw data.
0xE4 | Get Vd Value.<br>The response data is 2 bytes of raw data.
0xE5 | Get Id Value.<br>The response data is 2 bytes of raw data.
0xE6 | Get Vin Value.<br>The response data is 2 bytes of raw data.
0xE7 | Get Pin Value.<br>The response data is 2 bytes of raw data.
0xEA | Get Mode.<br>The response data is 1 bytes of raw data.
0xEE | Get all Analog values.<br>The responce data is {Vd(2byte) + Id(2byte) + Vin(2byte) + Pin(2byte) + CPU Temp(2byte)} of raw data.
0xEF | Get all sensor values.<br>The responce data is {AnalogValues(10byte)+TempratureData(2byte * 15)}
0xF0 | Get device identification character.
0xFA | Restore factory default settings.<br>PS all 0<br>DSA all 2dB(No,0 = 0dB)<br>STB all 0(RUN MODE)<br>LPM 0(Full Power MODE)
0xFB {Address} | Save state to memory(ROM).<br>However, whether or not it can be saved depends on the boot mode.<br>To save the default settings, set {Address} to 0x00 or leave it unspecified. ({Address} can be specified from 0x00 to 0x03.)<br>If you specify the sector number (4 bits), setting number (4 bits), and setting number (specified in one byte from 0x00 to 0x03), it will be written to the specified setting number. (Default is {0xE0}{0x00})<br>The range that can be specified is the same as for WR.
0xFC {Address} | Load state from memory(ROM).<br>Argument are the same as 0xFB.
0xFD {BAUD RATE} | BAUD RATE chages.<br>{BAUD RATE} : Specify the value in 4bytes.
0xAA {Address} | Read sector data from ROM.<br>{Address(3byte)} : Specify the address to read (sector by sector)
0xBB {Address} {Binary} | Overwrite sector data to ROM.<br>{Address(3byte)} : Specify the address to write (sector by sector).<br>{Binary(4096byte)} * Specify the sector data to write.
0xFE | Switch to ASCII communication mode.

Return Code | Description
:--|:--
0xC0 | Frame end code.
0x00 | Successfull code.
0xF1 | Command not found error code.
0xF2 | Data length error code.
0xFE | Other errors code.
{binary} | binary data.

</details>

## Hardware Switch Configuration
List of settings by onboard hardware switch (SW1) status.<br>
*In v1.3.0 and later, sw1 and sw2 are assigned to output pins and must be fixed in the OFF state.*

<img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_RP2040/assets/SW1.png?raw=true" width="100px"> 0 = OFF(H) / 1 = ON(L)  
  
<details open>
<summary>Switch status 0x00 to 0x0F</summary>

Number | SW6 | SW5 | SW4 | SW3 | HEX | Stateus | Description
:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--
0 | 0 | 0 | 0 | 0 | 0x00 | Default | Default Status Boot.<br>*DPS(@All) = 0deg*<br>*DSA(@Input) = 0dB*<br>*DSA(@All except input) = 2dB*<br>*ALL Active Mode*
1 | 0 | 0 | 0 | 1 | 0x01 | Auto Load Boot. | Load the state (0) saved in ROM and boot.<br>*Picture Stateus*
2 | 0 | 0 | 1 | 0 | 0x02 | Allow settings to be saved. | Settins can be write in ROM.
3 | 0 | 0 | 1 | 1 | 0x03 | Auto Load Boot.<br>and<br>Allow settings to be saved. | Allows to start autoload and save settings.<br>*Basic usage conditions *
4 | 0 | 1 | 0 | 0 | 0x04 | State4 | Unused.
5 | 0 | 1 | 0 | 1 | 0x05 | State5 | Unused.
6 | 0 | 1 | 1 | 0 | 0x06 | State6 | Unused.
7 | 0 | 1 | 1 | 1 | 0x07 | State7 | Unused.
8 | 1 | 0 | 0 | 0 | 0x08 | State8 | Unused.
9 | 1 | 0 | 0 | 1 | 0x09 | State9 | Unused.
10 | 1 | 0 | 1 | 0 | 0x0A | State10 | Factory reset on boot. | If the switch is in this state at startup, the system boots to factory defaults and restore the autoload settings to their initial state.<br>*Settings outside the default settings area will not be changed.*
11 | 1 | 0 | 1 | 1 | 0x0B | State11 | Unused.
12 | 1 | 1 | 0 | 0 | 0x0C | State12 | Unused.
13 | 1 | 1 | 0 | 1 | 0x0D | State13 | Unused.
14 | 1 | 1 | 1 | 0 | 0x0E | State14 | Unused.
15 | 1 | 1 | 1 | 1 | 0x0F | State15 | Boot in the maintenance mode. | If the switch is in this state at startup, it boots in the administrator mode.<br>As general rule, do not use it as it may overwrite the ROM area where factory settings and serial numbers are stored.

</details>