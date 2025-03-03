# PCAB_Debugger2
本プログラムはPCAB(LX00-0004-00)動作検証用プログラムです。<br>
本プログラムは[PCAB_Debugger](https://github.com/mw-eng/PCAB_Debugger)から不要となった機能(POS連動制御等)の削除とプログラム構成の見直しを行い再構築しました。<br>
製品化を目的として作成したものではありませんので、評価のみでご使用ください。本プログラムに関連して生じた、いかなる損害についても一切の責任を負いません。<br>
本プログラムは本体ファームウェアと制御を行うGUI部分から構成されており、シリアルインターフェースを使用したコマンドによる制御も可能です。<br>
コマンドについては、PCAB_Debugger2_RP2040 [README.md](./PCAB_Debugger2_RP2040/README.md)を参照してください。<br>

## PCAB_Debugger2_GUI
これは基本となるGUI制御プログラムです。
[README.md](./PCAB_Debugger2_GUI/README.md)

## PCAB_Debugger2_RP2040
これはPCAB(LX00-0004-00)に書込む本体ファームウェアです。
[README.md](./PCAB_Debugger2_RP2040/README.md)

## 開発環境構築の参考URL
IDE > [Visual Studio](https://visualstudio.microsoft.com/downloads/)  
Document > [Getting started with Raspberry Pi Pico](https://datasheets.raspberrypi.com/pico/getting-started-with-pico.pdf)  
GitHub > [raspberrypi](https://github.com/raspberrypi)  
Repository > [raspberrypi/pico-setup-windows](https://github.com/raspberrypi/pico-setup-windows)  
### Windows Installer
Raspberry Pi Pico > [pico-setup-windows-x64-standalone.exe](https://github.com/raspberrypi/pico-setup-windows/releases/latest/download/pico-setup-windows-x64-standalone.exe)  
Visual Studio Community 2022 > [VisualStudioSetup.exe](https://visualstudio.microsoft.com/ja/thank-you-downloading-visual-studio/?sku=Community&channel=Release&version=VS2022&source=VSLandingPage&cid=2030&passive=false)  