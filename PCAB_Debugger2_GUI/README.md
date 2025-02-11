# PCAB Debugger GUI
Control GUI program.

## Requirements / Supported Platforms
* .NET Framework 4.7.2 or later.
* VNA A.17.20.09 (64 bit)
* Keygisht IO Libraries Suite 18.2.27313.1 or later. @[Keysight](https://www.keysight.com/zz/en/lib/software-detail/computer-software/io-libraries-suite-downloads-2175637.html)
* NI-VISA 18.5 @[National Instruments](https://www.ni.com/ja/support/downloads/drivers/download.ni-visa.html#306043)

## How to use

### Main window
This is how to use it when communicating one-to-one.<br>
When communicating one-to-many, each SN is displayed in a tab.<br>
<br><img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_GUI/assets/UI1_Ex.png?raw=true" width="600px"><br>
<br>
${\color{Aqua} 1. \space Settings \space before \space connection}$<br>
|No|Item|Description|
|:--|:--|:--|
|1-1|Serial Port			|Select the *Serial Port* coneected to PCAB.
|1-2|Baud Rate				|Select the *BAUD RATE* for the main communication to the PCAB.
|1-3|Monitor loop inberval	|Specifies the acquisition interval for monitors (temperature, etc.) in milliseconds.|
|1-4|Serial Numbers			|Enter the serial number of the connect target.<br>separate with "," when connecting multiple units.<br>If you want to specify a view display, enter the code separated by "@".<br>Z : default<br>R : Rotate right 90 degrees<br>L : Rotate left 90 degrees<br>H : Rotate 180 degrees<br>ZM : default mirror<br>R : Rotate right 90 degrees and then mirror<br>L : Rotate left 90 degrees and then mirror<br>H : Rotate 180 degrees and then mirror |


<br><img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_GUI/assets/UI1.png?raw=true" width="600px"><br>
<br>
${\color{Gold} 2. \space CONTROL \space TAB}$<br>
|No|Item|Description|
|:--|:--|:--|
|2-1|Standby AMP			|If checked, it will go into standby mode.|
|2-2|Standby DRA			|If checked, it will go into standby mode.|
|2-3|Low Power Mode			|If checked, it will go into low power mode.|
|2-4|View					|Select the port display order.|
|2-5|Phase and ATT Config	|Setting the phase delay and attenation.|
|2-6|Standby LNA			|If checked, it will go into standby mode.|
|2-7|Set ATT Config			|Write attenation config.|
|2-8|Set Phase Config		|Write phase delay config.|
|2-9|Set Config				|Write phase delay and attnation config.|
|2-10|Read Config			|Read the currently set state.|
|2-11|Save and Load Target	|Memory number to save and load. <br>0 specifies the autoload region {0xE0 - 0}.|
|2-12|Load Memory			|Load state to memory(ROM).|
|2-13|Save Memory			|Save state to memory(ROM).|
|2-14|Preset Config			|Preset config load.(Load factory defaults.)|


<br><img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_GUI/assets/UI2.png?raw=true" width="600px"><br>
<br>
${\color{LightSkyBlue} 3. \space AUTO \space TAB \space}$ Automatic measurement control<br>
|No|Item|Description|
|:--|:--|:--|
|3-1|VISA Address		|Instrument visa address.|
|3-2|Check Button		|Instrument communication check.|
|3-3|Channel			|Config collection channels.|
|3-4|Mode Settings		|Automatic trigger settings.|
|3-5|Save target		|Select save target.|
|3-6|File Name Header	|Save file name header.|
|3-7|DPS Loop 			|Enable the phase delay step loop.|
|3-8|DPS step			|Select the phase delay step interval.|
|3-9|DPSn				|Select loop target.|
|3-10|DSA Loop 			|Enable the attenation step loop.|
|3-11|DSA step			|Select the attenation step interval.|
|3-12|DSAn				|Select loop target.|
|3-13|Waite Time		|Waite time befor next step.|
|3-14|START Button		|Automatic loop control execution button.|

<br><br>

### Monitor window
This is the display for one-to-one communication.<br>
For one-to-many communication, it is displayed as a list.<br>
<br>
<br><img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_GUI/assets/UI3.png?raw=true" width="600px"><br>
<br>
${\color{Magenta} 4. \space Sensor \space display \space Window}$<br>
|No|Item|Description|
|:--|:--|:--|
|4-1|CPU TMP      |CPU internal temperature value [degreeC].|
|4-2|SNS Vin      |Vin voltage value [V].|
|4-3|SNS Pin      |PreAMP Detector voltage value [V].|
|4-4|SNS Id       |Id current value [A].|
|4-5|SNS Vd       |Vd voltage value [V].|
|4-6|TMP AVG      |1-wire sensor temperature avarage [degreeC].|
|4-7|Tempurature  |1-wire sensor temperature id(HEX) and value [degreeC].|

