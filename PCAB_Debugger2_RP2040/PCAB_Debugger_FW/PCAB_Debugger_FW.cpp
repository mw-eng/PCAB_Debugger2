#include "PCAB_Debugger_FW.hpp"
#define SNPRINTF_BUFFER_LEN 50
#define NUMBER_OF_SYSTEM 15
#define ROM_BLOCK_NUM PICO_FLASH_SIZE_BYTES / FLASH_BLOCK_SIZE
const static std::string FW_VENDOR = "Orient Microwave Corp.";
const static std::string FW_MODEL = "LX00-0004-00";
const static std::string FW_REV = "2.0.0";

#ifdef DEBUG_RASPICO
    #define DSA_D3_PIN 8
    #define DSA_D4_PIN 9
    #define DSA_D4_PIN 10
    #define SNS_TEMP_PIN 24
    #define SW_2_PIN 3
    #define SW_3_PIN SW_1_PIN
    #define SW_4_PIN SW_1_PIN
    #define SW_5_PIN SW_1_PIN
    #define SW_6_PIN SW_1_PIN
    #define PICO_LED_PIN 25
    // Onewire PIN Configure
    #define SNS_TEMP_PIN 11
#endif

ds18b20 *sens;
adc *analog;
spi *spi_dps;
spi *spi_dsa;
pcabCMD *uart;
bool modeCUI = true;
bool modeECHO = false;
std::string serialNum = "";
uint64_t romID = UINT64_MAX;
uint8_t bootMode = 0;
uint8_t dpsBF[NUMBER_OF_SYSTEM];
uint8_t dpsNOW[NUMBER_OF_SYSTEM];
uint8_t dsaBF[NUMBER_OF_SYSTEM + 1];
uint8_t dsaNOW[NUMBER_OF_SYSTEM + 1];
bool stbAMP = false;
bool stbDRA = false;
bool stbLNA = false;
bool lowMODE = false;
bool modeBCM = false;
uint32_t brNOW;
std::vector<uint64_t> tmpIDs;
std::vector<uint16_t> tmpVALs; //温度センサの値を共有する領域
static semaphore_t sem;

void setup()
{
    stdio_init_all();
    sem_init(&sem, 1, 1); //semaphore初期化
    sens = new ds18b20(pio0, SNS_TEMP_PIN);
    analog = new adc(true, true, true, true , 3.3f);
    spi_dps = new spi(spi0, SPI_CLK, SPI0_CLK_PIN, SPI0_TX_PIN, SPI0_RX_PIN, SPI0_LE_PIN, SPI_BITS, SPI_MODE, SPI_ORDER);
    spi_dsa = new spi(spi1, SPI_CLK, SPI1_CLK_PIN, SPI1_TX_PIN, SPI1_RX_PIN, SPI1_LE_PIN, SPI_BITS, SPI_MODE, SPI_ORDER);
    brNOW = readBR();
#ifdef UART_PARITY_ENABLE
    uart = new pcabCMD(uart0, UART_TX_PIN, UART_RX_PIN, brNOW, UART_DATA_BITS, UART_STOP_BIT, UART_PARITY_EVEN, false, false, "\r\n", RS485_DE_PIN, RS485_DE_ENB_WAITE_TIME, RS485_DE_WAITE_TIME);
#else
    uart = new pcabCMD(uart0, UART_TX_PIN, UART_RX_PIN, brNOW, UART_DATA_BITS, UART_STOP_BIT, UART_PARITY_NONE, false, false, "\r\n", RS485_DE_PIN, RS485_DE_ENB_WAITE_TIME, RS485_DE_WAITE_TIME);
#endif
    //GPIO Setup
    gpio_init(LPW_MOD_PIN);
    gpio_init(STB_DRA_PIN);
    gpio_init(STB_AMP_PIN);
    gpio_init(STB_LNA_PIN);
    gpio_init(DSA_D0_PIN);
    gpio_init(DSA_D1_PIN);
    gpio_init(DSA_D2_PIN);
    gpio_init(DSA_D3_PIN);
    gpio_init(DSA_D4_PIN);
    gpio_init(SW_1_PIN);
    gpio_init(SW_2_PIN);
    gpio_init(SW_3_PIN);
    gpio_init(SW_4_PIN);
    //gpio_init(SW_5_PIN);
    //gpio_init(SW_6_PIN);
    gpio_set_dir(LPW_MOD_PIN ,GPIO_OUT);
    gpio_set_dir(STB_DRA_PIN ,GPIO_OUT);
    gpio_set_dir(STB_AMP_PIN ,GPIO_OUT);
    gpio_set_dir(STB_LNA_PIN ,GPIO_OUT);
    gpio_set_dir(DSA_D0_PIN ,GPIO_OUT);
    gpio_set_dir(DSA_D1_PIN ,GPIO_OUT);
    gpio_set_dir(DSA_D2_PIN ,GPIO_OUT);
    gpio_set_dir(DSA_D3_PIN ,GPIO_OUT);
    gpio_set_dir(DSA_D4_PIN ,GPIO_OUT);
    gpio_set_dir(SW_1_PIN ,GPIO_IN);
    gpio_set_dir(SW_2_PIN ,GPIO_IN);
    gpio_set_dir(SW_3_PIN ,GPIO_IN);
    gpio_set_dir(SW_4_PIN ,GPIO_IN);
    //gpio_set_dir(SW_5_PIN ,GPIO_IN);
    //gpio_set_dir(SW_6_PIN ,GPIO_IN);
#ifdef DEBUG_RASPICO
    gpio_init(PICO_LED_PIN);
    gpio_set_dir(PICO_LED_PIN ,GPIO_OUT);
    gpio_put(PICO_LED_PIN, true);
#endif

    //Get Boot Mode
    bootMode = !gpio_get(SW_1_PIN);
    bootMode += 2 * !gpio_get(SW_2_PIN);
    bootMode += 4 * !gpio_get(SW_3_PIN);
    bootMode += 8 * !gpio_get(SW_4_PIN);
    //bootMode += 16 * !gpio_get(SW_5_PIN);
    //bootMode += 32 * !gpio_get(SW_6_PIN);
#ifdef DEBUG_BOOT_MODE
    bootMode = DEBUG_BOOT_MODE;
#endif

    // Load serial number and ROM id
    serialNum = readSerialNum();
    romID = flash::getID();

    // Resture STATE @ default.
    for(uint i = 0; i < NUMBER_OF_SYSTEM; i++) { dpsBF[i] = 0u; dsaBF[i] = 8u; }
    dsaBF[NUMBER_OF_SYSTEM] = 0u;
    stbAMP = false;
    stbDRA = false;
    stbLNA = false;
    lowMODE = false;
    //Read Factory State
    readSTATE(15u, 0u, 0u);

    if(bootMode == 0x01 || bootMode == 0x03) { readSTATE(14u, 0u, 0u); }
    writeNowSTATE();
    if(bootMode == 0x0A) { uart->setBaudRate(UART_BAUD_RATE); saveSTATE(14u, 0u, 0u); }
#ifdef DEBUG_RASPICO
    tmpIDs.clear();
    tmpIDs.push_back(0xFF00000000000001);
    tmpIDs.push_back(0xFF00000000000002);
    tmpIDs.push_back(0xFF00000000000003);
    tmpIDs.push_back(0xFF00000000000004);
    tmpIDs.push_back(0xFF00000000000005);
    tmpIDs.push_back(0xFF00000000000006);
    tmpIDs.push_back(0xFF00000000000007);
    tmpIDs.push_back(0xFF00000000000008);
    tmpIDs.push_back(0xFF00000000000009);
    tmpIDs.push_back(0xFF0000000000000A);
    tmpIDs.push_back(0xFF0000000000000B);
    tmpIDs.push_back(0xFF0000000000000C);
    tmpIDs.push_back(0xFF0000000000000D);
    tmpIDs.push_back(0xFF0000000000000E);
    tmpIDs.push_back(0xFF0000000000000F);
#else
    tmpIDs = sens->getSENS_ROMCODE();
#endif
#ifdef DEBUG_RASPICO
    uint32_t time_us = time_us_32();
    tmpVALs.clear();
    tmpVALs.push_back(0x0010);
    tmpVALs.push_back(0x0020);
    tmpVALs.push_back(0x0030);
    tmpVALs.push_back(0x0040);
    tmpVALs.push_back(0x0050);
    tmpVALs.push_back(0x0060);
    tmpVALs.push_back(0x0070);
    tmpVALs.push_back(0x0080);
    tmpVALs.push_back(0x0090);
    tmpVALs.push_back(0x00A0);
    tmpVALs.push_back(0x00B0);
    tmpVALs.push_back(0x00C0);
    tmpVALs.push_back(0x00D0);
    tmpVALs.push_back(0x00E0);
    tmpVALs.push_back(0x00F0);
#else
    tmpVALs = sens->readSENS();
#endif
    if(tmpIDs.size()!=tmpVALs.size()){tmpVALs.resize(tmpIDs.size(),0);}
}

int main()
{
    setup();
    multicore_launch_core1(main_core1); //通信処理をcore1で開始
    main_core0(); //1-wire温度センサの値更新をcore0で開始
    return 0;
}

//Core0は1-wire温度センサの値取得と更新を割当
void main_core0()
{
    while (1)
    {
        std::vector<uint16_t> tmpVALsBF = sens->readSENS();
#ifdef DEBUG_RASPICO
        if(tmpVALsBF.size() <= 0)
        {
            uint32_t time_us = time_us_32();
            tmpVALsBF.clear();
            tmpVALsBF.push_back(0x0010);
            tmpVALsBF.push_back(0x0020);
            tmpVALsBF.push_back(0x0030);
            tmpVALsBF.push_back(0x0040);
            tmpVALsBF.push_back(0x0050);
            tmpVALsBF.push_back(0x0060);
            tmpVALsBF.push_back(0x0070);
            tmpVALsBF.push_back(0x0080);
            tmpVALsBF.push_back(0x0090);
            tmpVALsBF.push_back(0x00A0);
            tmpVALsBF.push_back(0x00B0);
            tmpVALsBF.push_back(0x00C0);
            tmpVALsBF.push_back(0x00D0);
            tmpVALsBF.push_back((time_us >> 16) & 0xFFFF);
            tmpVALsBF.push_back(time_us & 0xFFFF);
            sleep_ms(500);
        }
#endif
        sem_acquire_blocking(&sem);
        if(tmpVALsBF.size() == tmpVALs.size()) { tmpVALs = tmpVALsBF; }
        else { tmpVALs.resize(tmpIDs.size(),0); }
        sem_release(&sem);
        sleep_ms(100);
    }
    close_core1();
    return;
}

//Core1では通信と各種設定変更処理を割当
void main_core1()
{
    while (1)
    {
        pcabCMD::CommandLine cmd = uart->readCMD(modeECHO, modeBCM);
        if((cmd.serialNum.size() > 0 && (String::strCompare(cmd.serialNum, "*", true) || String::strCompare(cmd.serialNum, serialNum, true))) || (cmd.serialNum.size() == 0 && cmd.romID == romID))
        {
            if((modeECHO || modeCUI) && !modeBCM){uart->writeLine("");}
            switch (cmd.command)
            {
                case pcabCMD::cmdCode::WrtDPS:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != NUMBER_OF_SYSTEM) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            bool errFLG = false;
                            for(size_t i = 0; i < cmd.argment.size(); i++)
                            {
                                if(cmd.argment[i] > (1u << 6) ) { errFLG = true; break; }
                                dpsBF[i] = cmd.argment[i];
                            }
                            if(errFLG) { uart->writeSLIP_block(retCODE(0xFE)); }
                            else { writeDPS(); uart->writeSLIP_block(retCODE(0x00)); }
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 0) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            writeDPS();
                            uart->writeLine("DONE > Write Digital Phase Shifter Status.");
                        }
                    }
                    break;
                case pcabCMD::cmdCode::GetDPS:
                    if(modeBCM)
                    {
                        std::vector<uint8_t> result;
                        result.clear();
                        if(cmd.argment.size() == 0) { for(uint16_t i= 0; i< NUMBER_OF_SYSTEM; i++) { result.push_back(dpsNOW[i] & 0x3F); } }
                        else { result.push_back(0xF2); }
                        uart->writeSLIP_block(result);
                    }
                    else
                    {
                        if(cmd.argments.size() != 2) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            bool blNOW;
                            if(String::strCompare(cmd.argments[0], "now")) { blNOW = true; }
                            else if(String::strCompare(cmd.argments[0], "bf")) { blNOW = false; }
                            else
                            {
                                if(!Convert::TryToBool(cmd.argments[0], blNOW)) { uart->writeLine("ERR > Argument1 error."); break; }
                            }
                            uint16_t num;
                            if(!Convert::TryToUInt16(cmd.argments[1], 10, num)) { uart->writeLine("ERR > Argument2 error."); break; }
                            if(NUMBER_OF_SYSTEM < num ) { uart->writeLine("ERR > The specified Argument2 is out of range."); break; }
                            if(modeCUI)
                            {
                                char ch[SNPRINTF_BUFFER_LEN];
                                int len;
                                if(blNOW && num == 0)
                                {
                                    for(int i = 0; i < NUMBER_OF_SYSTEM; i++ )
                                    {
                                        len = snprintf(ch, sizeof(ch), "Now DPS[%02d] > %7.3f[deg] (0d%03d, 0b%s)", i + 1, dpsNOW[i] * 5.625f, dpsNOW[i], Convert::ToString(dpsNOW[i], 2, 6).c_str());
                                        uart->writeLine(std::string(ch, len));
                                    }
                                }
                                else if(num == 0)
                                {
                                    for(int i = 0; i < NUMBER_OF_SYSTEM; i++ )
                                    {
                                        len = snprintf(ch, sizeof(ch), "Buffer DPS[%02d] > %7.3f[deg] (0d%03d, 0b%s)", i + 1, dpsBF[i] * 5.625f, dpsBF[i], Convert::ToString(dpsBF[i], 2, 6).c_str());
                                        uart->writeLine(std::string(ch, len));
                                    }
                                }
                                else if(blNOW)
                                {
                                    len = snprintf(ch, sizeof(ch), "Now DPS[%02d] > %7.3f[deg] (0d%03d, 0b%s)", num, dpsNOW[num - 1] * 5.625f, dpsNOW[num - 1], Convert::ToString(dpsNOW[num - 1], 2, 6).c_str());
                                    uart->writeLine(std::string(ch, len));
                                }
                                else
                                {
                                    len = snprintf(ch, sizeof(ch), "Buffer DPS[%02d] > %7.3f[deg] (0d%03d, 0b%s)", num, dpsBF[num - 1] * 5.625f, dpsBF[num - 1], Convert::ToString(dpsBF[num - 1], 2, 6).c_str());
                                    uart->writeLine(std::string(ch, len));
                                }
                            }
                            else
                            {
                                if(blNOW && num == 0)
                                {
                                    uart->write(Convert::ToString(dpsNOW[0], 10, 1));
                                    for(int i = 1; i < NUMBER_OF_SYSTEM; i++ ) { uart->write("," + Convert::ToString(dpsNOW[i], 10, 1)); }
                                    uart->writeLine("");
                                }
                                else if(num == 0)
                                {
                                    uart->write(Convert::ToString(dpsBF[0], 10, 1));
                                    for(int i = 1; i < NUMBER_OF_SYSTEM; i++ ) { uart->write("," + Convert::ToString(dpsBF[i], 10, 1)); }
                                    uart->writeLine("");
                                }
                                else if(blNOW) { uart->writeLine(Convert::ToString(dpsNOW[num - 1], 10, 1)); }
                                else { uart->writeLine(Convert::ToString(dpsBF[num - 1], 10, 1)); }
                            }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::SetDPS:
                    if(cmd.argments.size() != 2) { uart->writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        uint16_t num;
                        uint8_t conf;
                        if(!Convert::TryToUInt16(cmd.argments[0], 10, num)) { uart->writeLine("ERR > Argument1 error."); break; }
                        if(NUMBER_OF_SYSTEM < num) { uart->writeLine("ERR > The specified Argument1 is out of range."); break; }
                        if(!Convert::TryToUInt8(cmd.argments[1], 10, conf)) { uart->writeLine("ERR > Argument2 error."); break; }
                        if(conf > (1u << 6)) { uart->writeLine("ERR > The specified Argument2 is out of range."); break; }
                        dpsBF[num - 1] = conf;
                        uart->writeLine("DONE > Set to buffer.");
                    }
                    break;
                case pcabCMD::cmdCode::WrtDSA:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() == 1)
                        {
                            if(cmd.argment[0] > (1u << 5)) { uart->writeSLIP_block(retCODE(0xFE)); }
                            else { dsaBF[NUMBER_OF_SYSTEM] = cmd.argment[0]; writeDSAin(); uart->writeSLIP_block(retCODE(0x00)); }
                        }
                        else if(cmd.argment.size() == NUMBER_OF_SYSTEM)
                        {
                            bool errFLG = false;
                            for(size_t i = 0; i < cmd.argment.size(); i++)
                            {
                                if(cmd.argment[i] > (1u << 6) ) { errFLG = true; break; }
                                dsaBF[i] = cmd.argment[i];
                            }
                            if(errFLG) { uart->writeSLIP_block(retCODE(0xFE)); }
                            else { writeDSA(); uart->writeSLIP_block(retCODE(0x00)); }
                        }
                        else { uart->writeSLIP_block(retCODE(0xF2)); }
                    }
                    else
                    {
                        if(cmd.argments.size() != 0) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            writeDSAall();
                            uart->writeLine("DONE > Write Digital Phase Shifter Status.");
                        }
                    }
                    break;
                case pcabCMD::cmdCode::GetDSA:
                    if(modeBCM)
                    {
                        std::vector<uint8_t> result;
                        result.clear();
                        if(cmd.argment.size() == 0) { for(uint16_t i= 0; i< NUMBER_OF_SYSTEM; i++) { result.push_back(dsaNOW[i] & 0x3F); } } 
                        else if(cmd.argment.size() == 1) { result.push_back(dsaNOW[NUMBER_OF_SYSTEM]); }
                        else { result.push_back(0xF2); }
                        uart->writeSLIP_block(result);
                    }
                    else
                    {
                        if(cmd.argments.size() != 2) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            bool blNOW;
                            if(String::strCompare(cmd.argments[0], "now")) { blNOW = true; }
                            else if(String::strCompare(cmd.argments[0], "bf")) { blNOW = false; }
                            else
                            {
                                if(!Convert::TryToBool(cmd.argments[0], blNOW)) { uart->writeLine("ERR > Argument1 error."); break; }
                            }
                            uint16_t num;
                            if(String::strCompare(cmd.argments[1], "IN", true)) { num = NUMBER_OF_SYSTEM + 1; }
                            else if(!Convert::TryToUInt16(cmd.argments[1], 10, num)) { uart->writeLine("ERR > Argument2 error."); break; }
                            if(NUMBER_OF_SYSTEM + 1 < num) { uart->writeLine("ERR > The specified Argument2 is out of range."); break; }
                            if(modeCUI)
                            {
                                char ch[SNPRINTF_BUFFER_LEN];
                                int len;
                                if(blNOW && num == 0)
                                {
                                    for(int i = 0; i < NUMBER_OF_SYSTEM; i++ )
                                    {
                                        len = snprintf(ch, sizeof(ch), "Now DPS[%02d] > %7.3f[dB] (0d%03d, 0b%s)", i + 1, dsaNOW[i] * 0.25f, dsaNOW[i], Convert::ToString(dsaNOW[i], 2, 6).c_str());
                                        uart->writeLine(std::string(ch, len));
                                    }
                                }
                                else if(num == 0)
                                {
                                    len = snprintf(ch, sizeof(ch), "Buffer DSA[IN(16)] > %7.3f[dB] (0d%03d, 0b%s)", dsaBF[NUMBER_OF_SYSTEM] * 0.25f, dsaBF[NUMBER_OF_SYSTEM], Convert::ToString(dsaBF[NUMBER_OF_SYSTEM], 2, 6).c_str());
                                    uart->writeLine(std::string(ch, len));
                                    for(int i = 0; i < NUMBER_OF_SYSTEM; i++ )
                                    {
                                        len = snprintf(ch, sizeof(ch), "Buffer DSA[%02d] > %7.3f[dB] (0d%03d, 0b%s)", i + 1, dsaBF[i] * 0.25f, dsaBF[i], Convert::ToString(dsaBF[i], 2, 6).c_str());
                                        uart->writeLine(std::string(ch, len));
                                    }
                                }
                                else if(blNOW)
                                {
                                    if(num == NUMBER_OF_SYSTEM + 1){ len = snprintf(ch, sizeof(ch), "Now DSA[IN(%02d)] > %7.3f[dB] (0d%03d, 0b%s)", num, dsaNOW[num - 1] * 0.25f, dsaNOW[num - 1], Convert::ToString(dsaNOW[num - 1], 2, 6).c_str()); }
                                    else { len = snprintf(ch, sizeof(ch), "Now DSA[%02d] > %7.3f[dB] (0d%03d, 0b%s)", num, dsaNOW[num - 1] * 0.25f, dsaNOW[num - 1], Convert::ToString(dsaNOW[num - 1], 2, 6).c_str()); }
                                    uart->writeLine(std::string(ch, len));
                                }
                                else
                                {
                                    if(num == NUMBER_OF_SYSTEM + 1){len = snprintf(ch, sizeof(ch), "Buffer DSA[IN(%02d)] > %7.3f[dB] (0d%03d, 0b%s)", num, dsaBF[num - 1] * 0.25f, dsaBF[num - 1], Convert::ToString(dsaBF[num - 1], 2, 6).c_str()); }
                                    else { len = snprintf(ch, sizeof(ch), "Buffer DSA[%02d] > %7.3f[dB] (0d%03d, 0b%s)", num, dsaBF[num - 1] * 0.25f, dsaBF[num - 1], Convert::ToString(dsaBF[num - 1], 2, 6).c_str()); }
                                    uart->writeLine(std::string(ch, len));
                                }
                            }
                            else
                            {
                                if(blNOW && num == 0)
                                {
                                    uart->write(Convert::ToString(dsaNOW[0], 10, 1));
                                    for(int i = 1; i < NUMBER_OF_SYSTEM + 1; i++ ) { uart->write("," + Convert::ToString(dsaNOW[i], 10, 1)); }
                                    uart->writeLine("");
                                }
                                else if(num == 0)
                                {
                                    uart->write(Convert::ToString(dsaBF[0], 10, 1));
                                    for(int i = 1; i < NUMBER_OF_SYSTEM + 1; i++ ) { uart->write("," + Convert::ToString(dsaBF[i], 10, 1)); }
                                    uart->writeLine("");
                                }
                                else if(blNOW) { uart->writeLine(Convert::ToString(dsaNOW[num - 1], 10, 1)); }
                                else { uart->writeLine(Convert::ToString(dsaBF[num - 1], 10, 1)); }
                            }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::SetDSA:
                    if(cmd.argments.size() != 2) { uart->writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        uint16_t num;
                        uint8_t conf;
                        if(String::strCompare(cmd.argments[0], "IN", true)) { num = NUMBER_OF_SYSTEM + 1; }
                        else if(!Convert::TryToUInt16(cmd.argments[0], 10, num)) { uart->writeLine("ERR > Argument1 error."); break; }
                        if(NUMBER_OF_SYSTEM + 1 < num) { uart->writeLine("ERR > The specified Argument1 is out of range."); break; }
                        if(!Convert::TryToUInt8(cmd.argments[1], 10, conf)) { uart->writeLine("ERR > Argument2 error."); break; }
                        if(num == NUMBER_OF_SYSTEM + 1 && conf > (1u << 5)) { uart->writeLine("ERR > The specified Argument2 is out of range."); break; }
                        else if(conf > (1u << 6)) { uart->writeLine("ERR > The specified Argument2 is out of range."); break; }
                        dsaBF[num - 1] = conf;
                        uart->writeLine("DONE > Set to buffer.");
                    }
                    break;
                case pcabCMD::cmdCode::GetSTB_AMP:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 0) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            if(stbAMP) { uart->writeSLIP_block(retCODE(0x01)); }
                            else { uart->writeSLIP_block(retCODE(0x00)); }
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 0) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            if(modeCUI)
                            {
                                if(stbAMP) { uart->writeLine("AMP > STANDBY MODE."); }
                                else { uart->writeLine("AMP > RUN MODE."); }
                            }
                            else { uart->writeLine(Convert::ToString(stbAMP)); }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::GetSTB_DRA:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 0) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            if(stbDRA) { uart->writeSLIP_block(retCODE(0x01)); }
                            else { uart->writeSLIP_block(retCODE(0x00)); }
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 0) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            if(modeCUI)
                            {
                                if(stbDRA) { uart->writeLine("DRA > STANDBY MODE."); }
                                else { uart->writeLine("DRA > RUN MODE."); }
                            }
                            else { uart->writeLine(Convert::ToString(stbDRA)); }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::GetSTB_LNA:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 0) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            if(stbLNA) { uart->writeSLIP_block(retCODE(0x01)); }
                            else { uart->writeSLIP_block(retCODE(0x00)); }
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 0) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            if(modeCUI)
                            {
                                if(stbLNA) { uart->writeLine("LNA > STANDBY MODE."); }
                                else { uart->writeLine("LNA > RUN MODE."); }
                            }
                            else { uart->writeLine(Convert::ToString(stbLNA)); }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::GetLPM:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 0) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            if(lowMODE) { uart->writeSLIP_block(retCODE(0x01)); }
                            else { uart->writeSLIP_block(retCODE(0x00)); }
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 0) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            if(modeCUI)
                            {
                                if(lowMODE) { uart->writeLine("HEA > LOW POWER MODE."); }
                                else { uart->writeLine("HEA > FULL POWER MODE."); }
                            }
                            else { uart->writeLine(Convert::ToString(lowMODE)); }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::SetSTB_AMP:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 1) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            if(cmd.argment[0] == 0x00) { stbAMP = false; gpio_put(STB_AMP_PIN, !stbAMP); uart->writeSLIP_block(retCODE(0x00)); }
                            else if(cmd.argment[0] == 0x01) { stbAMP = true; gpio_put(STB_AMP_PIN, !stbAMP); uart->writeSLIP_block(retCODE(0x00)); }
                            else { uart->writeSLIP_block(retCODE(0xFE)); }
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 1) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            bool bitBF;
                            if(!Convert::TryToBool(cmd.argments[0], bitBF)) { uart->writeLine("ERR > Argument error."); break; }
                            stbAMP = bitBF;
                            gpio_put(STB_AMP_PIN, !stbAMP);
                            if(stbAMP) { uart->writeLine("DONE > Setting AMP to standby mode."); }
                            else { uart->writeLine("DONE > Setting AMP to run mode."); }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::SetSTB_DRA:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 1) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            if(cmd.argment[0] == 0x00) { stbDRA = false; gpio_put(STB_DRA_PIN, !stbDRA); uart->writeSLIP_block(retCODE(0x00)); }
                            else if(cmd.argment[0] == 0x01) { stbDRA = true; gpio_put(STB_DRA_PIN, !stbDRA); uart->writeSLIP_block(retCODE(0x00)); }
                            else { uart->writeSLIP_block(retCODE(0xFE)); }
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 1) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            bool bitBF;
                            if(!Convert::TryToBool(cmd.argments[0], bitBF)) { uart->writeLine("ERR > Argument error."); break; }
                            stbDRA = bitBF;
                            gpio_put(STB_DRA_PIN, !stbDRA);
                            if(stbDRA) { uart->writeLine("DONE > Setting DRA to standby mode."); }
                            else { uart->writeLine("DONE > Setting DRA to run mode."); }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::SetSTB_LNA:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 1) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            if(cmd.argment[0] == 0x00) { stbLNA = false; gpio_put(STB_LNA_PIN, !stbLNA); uart->writeSLIP_block(retCODE(0x00)); }
                            else if(cmd.argment[0] == 0x01) { stbLNA = true; gpio_put(STB_LNA_PIN, !stbLNA); uart->writeSLIP_block(retCODE(0x00)); }
                            else { uart->writeSLIP_block(retCODE(0xFE)); }
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 1) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            bool bitBF;
                            if(!Convert::TryToBool(cmd.argments[0], bitBF)) { uart->writeLine("ERR > Argument error."); break; }
                            stbLNA = bitBF;
                            gpio_put(STB_LNA_PIN, !stbLNA);
                            if(stbLNA) { uart->writeLine("DONE > Setting LNA to standby mode."); }
                            else { uart->writeLine("DONE > Setting LNA to run mode."); }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::SetLPM:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 1) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            if(cmd.argment[0] == 0x00) { lowMODE = false; gpio_put(LPW_MOD_PIN, !lowMODE); uart->writeSLIP_block(retCODE(0x00)); }
                            else if(cmd.argment[0] == 0x01) { lowMODE = true; gpio_put(LPW_MOD_PIN, !lowMODE); uart->writeSLIP_block(retCODE(0x00)); }
                            else { uart->writeSLIP_block(retCODE(0xFE)); }
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 1) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            bool bitBF;
                            if(!Convert::TryToBool(cmd.argments[0], bitBF)) { uart->writeLine("ERR > Argument error."); break; }
                            lowMODE = bitBF;
                            gpio_put(LPW_MOD_PIN, !lowMODE);
                            if(lowMODE) { uart->writeLine("DONE > Setting HEA to low power mode."); }
                            else { uart->writeLine("DONE > Setting HEA to full power mode."); }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::GetTMP_ID:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 0) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            std::vector<uint8_t> result;
                            result.clear();
                            if(tmpIDs.size() == 0){result.push_back(0xFE);}
                            for(uint i = 0; i < tmpIDs.size(); i++)
                            {
                                result.push_back((uint8_t)((tmpIDs[i] & 0xFF00000000000000) >> 56));
                                result.push_back((uint8_t)((tmpIDs[i] & 0x00FF000000000000) >> 48));
                                result.push_back((uint8_t)((tmpIDs[i] & 0x0000FF0000000000) >> 40));
                                result.push_back((uint8_t)((tmpIDs[i] & 0x000000FF00000000) >> 32));
                                result.push_back((uint8_t)((tmpIDs[i] & 0x00000000FF000000) >> 24));
                                result.push_back((uint8_t)((tmpIDs[i] & 0x0000000000FF0000) >> 16));
                                result.push_back((uint8_t)((tmpIDs[i] & 0x000000000000FF00) >> 8));
                                result.push_back((uint8_t)(tmpIDs[i] & 0x00000000000000FF) );
                            }
                            uart->writeSLIP_block(result);
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 1) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            uint8_t num;
                            if(!Convert::TryToUInt8(cmd.argments[0], 10, num)) { uart->writeLine("ERR > Argument error."); break; }
                            if(tmpIDs.size() < num || tmpIDs.size() == 0) { uart->writeLine("ERR > Specified sensor does not exist."); break; }
                            if(num == 0)
                            {
                                if(modeCUI)
                                {
                                    char ch[SNPRINTF_BUFFER_LEN];
                                    for(uint8_t i = 0 ; i < tmpIDs.size() ; i++)
                                    {
                                        int len = snprintf(ch, sizeof(ch), "%+3u > %s", i, Convert::ToString(tmpIDs[i], 16, 16).c_str());
                                        uart->writeLine(std::string(ch, len));
                                    }
                                }
                                else
                                {
                                    uart->write(Convert::ToString(tmpIDs[0], 16, 1));
                                    for(uint8_t i = 1 ; i < tmpIDs.size() ; i++)
                                    { uart->write("," + Convert::ToString(tmpIDs[i], 16, 1)); }
                                    uart->writeLine("");
                                }
                            }
                            else
                            {
                                uint64_t code = tmpIDs[num - 1];
                                if(modeCUI)
                                {
                                    char ch[SNPRINTF_BUFFER_LEN];
                                    int len = snprintf(ch, sizeof(ch), "%u > %s", num, Convert::ToString(code, 16, 16).c_str());
                                    uart->writeLine(std::string(ch, len));
                                }
                                else
                                {
                                    uart->writeLine(Convert::ToString(code, 16, 1));
                                }
                            }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::GetTMP_VAL:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 0) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            sem_acquire_blocking(&sem);
                            std::vector<uint8_t> result;
                            result.clear();
                            if(tmpVALs.size() == 0){result.push_back(0xFE);}
                            for(uint i = 0; i < tmpVALs.size(); i++)
                            {
                                result.push_back((uint8_t)((tmpVALs[i] & 0xFF00) >> 8));
                                result.push_back((uint8_t)(tmpVALs[i] & 0x00FF) );
                            }
                            uart->writeSLIP_block(result);
                            sem_release(&sem);
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 1) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            uint8_t num;
                            if(!Convert::TryToUInt8(cmd.argments[0], 10, num)) { uart->writeLine("ERR > Argument error."); break; }
                            if(tmpIDs.size() < num || tmpIDs.size() == 0) { uart->writeLine("ERR > Specified sensor does not exist."); break; }
                            sem_acquire_blocking(&sem);
                            if(num == 0)
                            {
                                if(modeCUI)
                                {
                                    std::vector<float> code = sens->convertTEMP(tmpVALs);
                                    char ch[SNPRINTF_BUFFER_LEN];
                                    for(uint8_t i = 0 ; i < code.size() ; i++)
                                    {
                                        int len = snprintf(ch, sizeof(ch), "%+3u > %.3f [degC]", i, code[i]);
                                        uart->writeLine(std::string(ch, len));
                                    }
                                }
                                else
                                {
                                    std::vector<uint16_t> code = tmpVALs;
                                    uart->write(std::to_string(code[0]));
                                    for(uint8_t i = 1 ; i < code.size() ; i++)
                                    { uart->write("," + std::to_string(code[i])); }
                                    uart->writeLine("");
                                }
                            }
                            else
                            {
                                if(modeCUI)
                                {
                                    float code = sens->convertTEMP(tmpVALs[num - 1]);
                                    char ch[SNPRINTF_BUFFER_LEN];
                                    int len = snprintf(ch, sizeof(ch), "%+3u > %0.000f [degC]", num, code);
                                    uart->writeLine(std::string(ch, len));
                                }
                                else
                                {
                                    int16_t code = tmpVALs[num - 1];
                                    uart->writeLine(std::to_string(code));
                                }
                            }
                            sem_release(&sem);
                        }
                    }
                    break;
                case pcabCMD::cmdCode::GetTMP_CPU:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 0) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            std::vector<uint8_t> result;
                            result.clear();
                            uint16_t tmp = analog->readADC4();
                            result.push_back((uint8_t)((tmp & 0xFF00) >> 8));
                            result.push_back((uint8_t)(tmp & 0x00FF) );
                            uart->writeSLIP_block(result);
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 0) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            if(modeCUI)
                            {
                                char ch[SNPRINTF_BUFFER_LEN];
                                int len = snprintf(ch, sizeof(ch), "CPU TEMP > %.2f [degC]", analog->readTempCPU());
                                uart->writeLine(std::string(ch, len));
                            } else { uart->writeLine(std::to_string(analog->readADC4())); }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::GetVd:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 0) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            std::vector<uint8_t> result;
                            result.clear();
                            uint16_t tmp = analog->readADC0();
                            result.push_back((uint8_t)((tmp & 0xFF00) >> 8));
                            result.push_back((uint8_t)(tmp & 0x00FF) );
                            uart->writeSLIP_block(result);
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 0) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            if(modeCUI)
                            {
                                char ch[SNPRINTF_BUFFER_LEN];
                                int len = snprintf(ch, sizeof(ch), "Vd > %.3f [V]", analog->readVoltageADC0() * 10.091);
                                uart->writeLine(std::string(ch, len));
                            } else { uart->writeLine(std::to_string(analog->readADC0())); }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::GetId:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 0) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            std::vector<uint8_t> result;
                            result.clear();
                            uint16_t tmp = analog->readADC1();
                            result.push_back((uint8_t)((tmp & 0xFF00) >> 8));
                            result.push_back((uint8_t)(tmp & 0x00FF) );
                            uart->writeSLIP_block(result);
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 0) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            if(modeCUI)
                            {
                                char ch[SNPRINTF_BUFFER_LEN];
                                int len = snprintf(ch, sizeof(ch), "Id > %.3f [A]", ( analog->readVoltageADC1() - 0.08) / 0.737);
                                uart->writeLine(std::string(ch, len));
                            } else { uart->writeLine(std::to_string(analog->readADC1())); }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::GetVin:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 0) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            std::vector<uint8_t> result;
                            result.clear();
                            uint16_t tmp = analog->readADC2();
                            result.push_back((uint8_t)((tmp & 0xFF00) >> 8));
                            result.push_back((uint8_t)(tmp & 0x00FF) );
                            uart->writeSLIP_block(result);
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 0) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            if(modeCUI)
                            {
                                char ch[SNPRINTF_BUFFER_LEN];
                                int len = snprintf(ch, sizeof(ch), "Vin > %.3f [V]", analog->readVoltageADC2() * 15.0);
                                uart->writeLine(std::string(ch, len));
                            } else { uart->writeLine(std::to_string(analog->readADC2())); }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::GetPin:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 0) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            std::vector<uint8_t> result;
                            result.clear();
                            uint16_t tmp = analog->readADC3();
                            result.push_back((uint8_t)((tmp & 0xFF00) >> 8));
                            result.push_back((uint8_t)(tmp & 0x00FF) );
                            uart->writeSLIP_block(result);
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 0) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            if(modeCUI)
                            {
                                char ch[SNPRINTF_BUFFER_LEN];
                                int len = snprintf(ch, sizeof(ch), "Pin > %.3f [V]", analog->readVoltageADC3());
                                uart->writeLine(std::string(ch, len));
                            } else { uart->writeLine(std::to_string(analog->readADC3())); }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::GetAD:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 0) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            std::vector<uint8_t> result;
                            result.clear();
                            uint16_t tmp;
                            tmp = analog->readADC0();
                            result.push_back((uint8_t)((tmp & 0xFF00) >> 8));
                            result.push_back((uint8_t)(tmp & 0x00FF) );
                            tmp = analog->readADC1();
                            result.push_back((uint8_t)((tmp & 0xFF00) >> 8));
                            result.push_back((uint8_t)(tmp & 0x00FF) );
                            tmp = analog->readADC2();
                            result.push_back((uint8_t)((tmp & 0xFF00) >> 8));
                            result.push_back((uint8_t)(tmp & 0x00FF) );
                            tmp = analog->readADC3();
                            result.push_back((uint8_t)((tmp & 0xFF00) >> 8));
                            result.push_back((uint8_t)(tmp & 0x00FF) );
                            tmp = analog->readADC4();
                            result.push_back((uint8_t)((tmp & 0xFF00) >> 8));
                            result.push_back((uint8_t)(tmp & 0x00FF) );
                            uart->writeSLIP_block(result);
                        }
                    }
                    else { uart->writeLine("ERR > This command is not supported in ASCII mode."); }
                    break;
                case pcabCMD::cmdCode::GetSENS:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 0) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            std::vector<uint8_t> result;
                            result.clear();
                            uint16_t tmp;
                            tmp = analog->readADC0();
                            result.push_back((uint8_t)((tmp & 0xFF00) >> 8));
                            result.push_back((uint8_t)(tmp & 0x00FF) );
                            tmp = analog->readADC1();
                            result.push_back((uint8_t)((tmp & 0xFF00) >> 8));
                            result.push_back((uint8_t)(tmp & 0x00FF) );
                            tmp = analog->readADC2();
                            result.push_back((uint8_t)((tmp & 0xFF00) >> 8));
                            result.push_back((uint8_t)(tmp & 0x00FF) );
                            tmp = analog->readADC3();
                            result.push_back((uint8_t)((tmp & 0xFF00) >> 8));
                            result.push_back((uint8_t)(tmp & 0x00FF) );
                            tmp = analog->readADC4();
                            result.push_back((uint8_t)((tmp & 0xFF00) >> 8));
                            result.push_back((uint8_t)(tmp & 0x00FF) );

                            sem_acquire_blocking(&sem);
                            std::vector<uint16_t> code;
                            code.clear();
                            for(uint i = 0; i < tmpVALs.size(); i++)
                            {
                                code.push_back(tmpVALs[i]);
                            }
                            sem_release(&sem);
                            for(uint i = 0; i < code.size(); i++)
                            {
                                result.push_back((uint8_t)((code[i] & 0xFF00) >> 8));
                                result.push_back((uint8_t)(code[i] & 0x00FF) );
                            }
                            uart->writeSLIP_block(result);
                        }
                    }
                    else { uart->writeLine("ERR > This command is not supported in ASCII mode."); }
                    break;
                case pcabCMD::cmdCode::SaveMEM:
                    if(modeBCM)
                    {
                        uint8_t sectorNum = 14;
                        uint8_t pageNum = 0;
                        uint8_t stateNum = 0;
                        if(cmd.argment.size() == 1) { stateNum = cmd.argment[0]; }
                        else if(cmd.argment.size() == 2) { sectorNum = ((cmd.argment[0] & 0xF0) >> 4); pageNum = cmd.argment[0] & 0x0F; stateNum = cmd.argment[1]; }
                        else if(cmd.argment.size() != 0) { uart->writeSLIP_block(retCODE(0xF2)); }
                        if(!saveSTATE(sectorNum, pageNum, stateNum)) { uart->writeSLIP_block(retCODE(0xFE));}
                        else { uart->writeSLIP_block(retCODE(0x00)); }
                    }
                    else
                    {
                        if(cmd.argments.size() > 2) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            uint8_t sectorNum = 14;
                            uint8_t pageNum = 0;
                            uint8_t stateNum = 0;
                            std::vector<std::string> strVect;
                            if(cmd.argments.size() == 1) { strVect = String::split(cmd.argments[0], '-'); }
                            else if(cmd.argments.size() == 2)
                            {
                                if(!Convert::TryToUInt4(cmd.argments[0], 10, sectorNum)){uart->writeLine("ERR > Sector number error."); break;}
                                strVect = String::split(cmd.argments[1], '-');
                            }
                            if(strVect.size() == 1) { if(!Convert::TryToUInt2(strVect[0], 10, stateNum)){uart->writeLine("ERR > State number error."); break;} }
                            else if(strVect.size() == 2)
                            {
                                if(!Convert::TryToUInt4(strVect[0], 10, pageNum)){uart->writeLine("ERR > Page number error."); break;}
                                if(!Convert::TryToUInt2(strVect[1], 10, stateNum)){uart->writeLine("ERR > State number error."); break;}
                            }
                            if(!saveSTATE(sectorNum, pageNum, stateNum)) { uart->writeLine("ERR > Specified number is out of range."); break; }
                            uart->writeLine("DONE > Save state. (sector[" + Convert::ToString(sectorNum, 10, 2) + "]-page[" + Convert::ToString(pageNum, 10, 1) + "]-state[" + Convert::ToString(stateNum, 10, 1) + "]");
                        }
                    }
                    break;
                case pcabCMD::cmdCode::LoadMEM:
                    if(modeBCM)
                    {
                        uint8_t sectorNum = 14;
                        uint8_t pageNum = 0;
                        uint8_t stateNum = 0;
                        if(cmd.argment.size() == 1) { stateNum = cmd.argment[0]; }
                        else if(cmd.argment.size() == 2) { sectorNum = ((cmd.argment[0] & 0xF0) >> 4); pageNum = cmd.argment[0] & 0x0F; stateNum = cmd.argment[1]; }
                        else if(cmd.argment.size() != 0) { uart->writeSLIP_block(retCODE(0xF2)); }
                        if(!readSTATE(sectorNum, pageNum, stateNum)) { uart->writeSLIP_block(retCODE(0xFE));}
                        else { writeNowSTATE(); uart->writeSLIP_block(retCODE(0x00)); }
                    }
                    else
                    {
                        if(cmd.argments.size() > 2) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            uint8_t sectorNum = 14;
                            uint8_t pageNum = 0;
                            uint8_t stateNum = 0;
                            std::vector<std::string> strVect;
                            if(cmd.argments.size() == 1) { strVect = String::split(cmd.argments[0], '-'); }
                            else if(cmd.argments.size() == 2)
                            {
                                if(!Convert::TryToUInt4(cmd.argments[0], 10, sectorNum)){uart->writeLine("ERR > Sector number error."); break;}
                                strVect = String::split(cmd.argments[1], '-');
                            }
                            if(strVect.size() == 1) { if(!Convert::TryToUInt2(strVect[0], 10, stateNum)){uart->writeLine("ERR > State number error."); break;} }
                            else if(strVect.size() == 2)
                            {
                                if(!Convert::TryToUInt4(strVect[0], 10, pageNum)){uart->writeLine("ERR > Page number error."); break;}
                                if(!Convert::TryToUInt2(strVect[1], 10, stateNum)){uart->writeLine("ERR > State number error."); break;}
                            }
                            if(!readSTATE(sectorNum, pageNum, stateNum)) { uart->writeLine("ERR > No valid settings were found for the specified address."); break; }
                            writeNowSTATE();
                            uart->writeLine("DONE > Load state. (sector[" + Convert::ToString(sectorNum, 10, 2) + "]-page[" + Convert::ToString(pageNum, 10, 1) + "]-state[" + Convert::ToString(stateNum, 10, 1) + "]");
                        }
                    }
                    break;
                case pcabCMD::cmdCode::ReadROM:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 3) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            if(cmd.argment[2] & 0x0F != 0x00) { uart->writeSLIP_block(retCODE(0xFE)); }
                            else
                            {
                                std::vector<uint8_t> result;
                                result.clear();
                                uint16_t blockNum = (1u << 8) * cmd.argment[0] + cmd.argment[1];
                                uint8_t sectorNum = cmd.argment[2];
                                bool brkFLG = false;
                                for(uint8_t page = 0 ; page < FLASH_SECTOR_SIZE / FLASH_PAGE_SIZE; page++)
                                {
                                    uint8_t romDAT[FLASH_PAGE_SIZE];
                                    if(!flash::readROM(blockNum, sectorNum + page, romDAT)) { uart->writeSLIP_block(retCODE(0xFE)); brkFLG = true; break; }
                                    for(uint16_t i = 0 ; i < FLASH_PAGE_SIZE ; i++ ) { result.push_back(romDAT[i]); }
                                }
                                if(!brkFLG) { uart->writeSLIP_block(result); }
                            }
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 1) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            uint16_t blockNum;
                            uint8_t sectorpageNum;
                            std::vector<std::string> strVect = String::split(cmd.argments[0], '-');
                            if(strVect.size() != 2) { uart->writeLine("ERR > Argument error."); break; }
                            if(!Convert::TryToUInt16(strVect[0], 16, blockNum)) { uart->writeLine("ERR > Block number error."); break; }
                            if(!Convert::TryToUInt8(strVect[1], 16, sectorpageNum)) { uart->writeLine("ERR > Sector + Page number error."); break; }
                            uint8_t romDAT[FLASH_PAGE_SIZE];
                            if(!flash::readROM(blockNum, sectorpageNum, romDAT)) { uart->writeLine("ERR > Address error."); break; }
                            if(modeCUI && FLASH_PAGE_SIZE % 16 == 0)
                            {
                                for(uint16_t i = 0 ; i < FLASH_PAGE_SIZE ; i += 16 )
                                {
                                    for(uint8_t j = 0 ; j < 15 ; j++) { uart->write(Convert::ToString(romDAT[i + j], 16, 2) + " "); }
                                    uart->writeLine(Convert::ToString(romDAT[i + 15], 16, 2));
                                }
                            } else { for(uint8_t byteBF : romDAT) { uart->write(Convert::ToString(byteBF, 16, 2)); } uart->writeLine(""); }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::WriteROM:
                    if(cmd.argments.size() != 2) { uart->writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        uint16_t blockNum;
                        uint8_t sectorpageNum;
                        std::vector<std::string> strVect = String::split(cmd.argments[0], '-');
                        if(strVect.size() != 2) { uart->writeLine("ERR > Argument error."); break; }
                        if(!Convert::TryToUInt16(strVect[0], 16, blockNum)) { uart->writeLine("ERR > Block number error."); break; }
                        if(!Convert::TryToUInt8(strVect[1], 16, sectorpageNum)) { uart->writeLine("ERR > Sector + Page number error."); break; }
                        if(!romAddressRangeCheck(blockNum, sectorpageNum)) { uart->writeLine("ERR > Address is outside the range specified in boot mode."); break; }
                        if(cmd.argments[1].length() != 2 * FLASH_PAGE_SIZE){ uart->writeLine("ERR > Write data length mismatch"); break; }
                        uint8_t romDAT[FLASH_PAGE_SIZE];
                        for( uint i = 0 ; i < FLASH_PAGE_SIZE ; i++ )
                        {
                            if(!Convert::TryToUInt8(cmd.argments[1].substr(2 * i, 2) , 16, romDAT[i]))
                            { uart->writeLine("ERR > Write data error."); break; }
                        }
                        if(!flash::writeROM(blockNum, sectorpageNum, romDAT)) { uart->writeLine("ERR > Address error."); break; }
                        uart->writeLine("DONE > Write ROM block " + Convert::ToString(blockNum, 16, 2) + " - sector " + Convert::ToString(sectorpageNum / 0x10u, 16, 1) + " - page " + Convert::ToString(sectorpageNum % 0x10u, 16, 1) + ".");
                    }
                    break;
                case pcabCMD::cmdCode::EraseROM:
                    if(cmd.argments.size() != 1) { uart->writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        uint16_t blockNum;
                        uint8_t sectorNum;
                        std::vector<std::string> strVect = String::split(cmd.argments[0], '-');
                        if(strVect.size() != 2) { uart->writeLine("ERR > Argument error."); break; }
                        if(!Convert::TryToUInt16(strVect[0], 16, blockNum)) { uart->writeLine("ERR > Block number error."); break; }
                        if(!Convert::TryToUInt8(strVect[1], 16, sectorNum)) { uart->writeLine("ERR > Sector number error."); break; }
                        if(!romAddressRangeCheck(blockNum, sectorNum)) { uart->writeLine("ERR > Address is outside the range specified in boot mode."); break; }
                        if(!flash::eraseROM(blockNum, sectorNum)) { uart->writeLine("ERR > Address error."); break; }
                        uart->writeLine("DONE > Erase ROM block " + Convert::ToString(blockNum, 16, 2) + " - sector " + Convert::ToString(sectorNum, 16, 1) + ".");
                    }
                    break;
                case pcabCMD::cmdCode::OverwriteROM:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != (3 + 4096)) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            if(cmd.argment[2] & 0x0F != 0x00) { uart->writeSLIP_block(retCODE(0xFE)); }
                            else
                            {
                                uint16_t blockNum = (1u << 8) * cmd.argment[0] + cmd.argment[1];
                                uint8_t sectorNum = cmd.argment[2];
                                if(!flash::overwriteROMsector(blockNum, sectorNum / 0x10u, &cmd.argment[3])) { uart->writeSLIP_block(retCODE(0xFE)); }
                                uart->writeSLIP_block(retCODE(0x00));
                            }
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 2) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            uint16_t blockNum;
                            uint8_t sectorpageNum;
                            std::vector<std::string> strVect = String::split(cmd.argments[0], '-');
                            if(strVect.size() != 2) { uart->writeLine("ERR > Argument error."); break; }
                            if(!Convert::TryToUInt16(strVect[0], 16, blockNum)) { uart->writeLine("ERR > Block number error."); break; }
                            if(!Convert::TryToUInt8(strVect[1], 16, sectorpageNum)) { uart->writeLine("ERR > Sector + Page number error."); break; }
                            if(!romAddressRangeCheck(blockNum, sectorpageNum)) { uart->writeLine("ERR > Address is outside the range specified in boot mode."); break; }
                            if(cmd.argments[1].length() != 2 * FLASH_PAGE_SIZE){ uart->writeLine("ERR > Write data length mismatch"); break; }
                            uint8_t romDAT[FLASH_PAGE_SIZE];
                            for( uint i = 0 ; i < FLASH_PAGE_SIZE ; i++ )
                            {
                                if(!Convert::TryToUInt8(cmd.argments[1].substr(2 * i, 2) , 16, romDAT[i]))
                                { uart->writeLine("ERR > Write data error."); break; }
                            }
                            if(!flash::overwriteROMpage(blockNum, sectorpageNum, romDAT)) { uart->writeLine("ERR > Address error."); break; }
                            uart->writeLine("DONE > Write ROM block " + Convert::ToString(blockNum, 16, 2) + " - sector " + Convert::ToString(sectorpageNum / 0x10u, 16, 1) + " - page " + Convert::ToString(sectorpageNum % 0x10u, 16, 1) + ".");
                        }
                    }
                    break;
                case pcabCMD::cmdCode::SetSN:
                    if(cmd.argments.size() != 1) { uart->writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        if(cmd.argments[0].size() == 0) { uart->writeLine("ERR > Argument error."); break; }
                        if(cmd.argments[0].size() > 16) { uart->writeLine("ERR > Serial code is too long. Limit to 15 characters."); break; }
                        if(!romAddressRangeCheck(PICO_FLASH_SIZE_BYTES / FLASH_BLOCK_SIZE - 1, FLASH_BLOCK_SIZE / FLASH_PAGE_SIZE - 1)) { uart->writeLine("ERR > It is in an unusable state."); break; }
                        uint8_t pageBF[FLASH_PAGE_SIZE];
                        flash::readROM(PICO_FLASH_SIZE_BYTES - FLASH_PAGE_SIZE, pageBF);
                        for(uint i = 0; i < cmd.argments[0].size() ; i ++) { pageBF[FLASH_PAGE_SIZE - 0x10u + i] = cmd.argments[0][i]; }
                        for(uint i = cmd.argments[0].size(); i < 0x0Fu; i ++) { pageBF[FLASH_PAGE_SIZE - 0x10u + i] = 0; }
                        pageBF[FLASH_PAGE_SIZE - 1] = cmd.argments[0].size();
                        flash::overwriteROMpage(PICO_FLASH_SIZE_BYTES - FLASH_PAGE_SIZE, pageBF);
                        serialNum = cmd.argments[0];
                        uart->writeLine("DONE > Write serial number.");
                    }
                    break;
                case pcabCMD::cmdCode::RST:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 0) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else
                        {
                            if(!readSTATE(15u, 0u, 0u))
                            {
                                for(uint i = 0; i < NUMBER_OF_SYSTEM; i++) { dpsBF[i] = 0u; dsaBF[i] = 8u; }
                                dsaBF[NUMBER_OF_SYSTEM] = 0u;
                                stbAMP = false;
                                stbDRA = false;
                                stbLNA = false;
                                lowMODE = false;
                            }
                            writeNowSTATE();
                            uart->writeSLIP_block(retCODE(0x00));
                        }
                    }
                    else
                    {
                        if(cmd.argments.size() != 0) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            if(!readSTATE(15u, 0u, 0u))
                            {
                                for(uint i = 0; i < NUMBER_OF_SYSTEM; i++) { dpsBF[i] = 0u; dsaBF[i] = 8u; }
                                dsaBF[NUMBER_OF_SYSTEM] = 0u;
                                stbAMP = false;
                                stbDRA = false;
                                stbLNA = false;
                                lowMODE = false;
                            }
                            writeNowSTATE();
                            uart->writeLine("DONE > Reset state.");
                        }
                    }
                    break;
                case pcabCMD::cmdCode::ECHO:
                    if(cmd.argments.size() != 1) { uart->writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        bool mode;
                        if(!Convert::TryToBool(cmd.argments[0], mode)) { uart->writeLine("ERR > Argument error."); break; }
                        modeECHO = mode;
                        if(modeECHO) { uart->writeLine("DONE > WITH ECHO."); }
                        else { uart->writeLine("DONE > WITHOUT ECHO."); }
                    }
                    break;
                case pcabCMD::cmdCode::CUI:
                    if(cmd.argments.size() != 1) { uart->writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        bool mode;
                        if(!Convert::TryToBool(cmd.argments[0], mode)) { uart->writeLine("ERR > Argument error."); break; }
                        modeCUI = mode;
                        if(modeCUI) { uart->writeLine("DONE > CUI MODE."); }
                        else { uart->writeLine("DONE > GUI MODE."); }
                    }
                    break;
                case pcabCMD::cmdCode::GetMODE:
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 0) { uart->writeSLIP_block(retCODE(0xF2)); }
                        else { uart->writeSLIP_block(retCODE(bootMode)); }
                    }
                    else
                    {
                        if(cmd.argments.size() != 0) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else{ uart->writeLine("0x" + Convert::ToString(bootMode, 16, 2));}
                    }
                    break;
                case pcabCMD::cmdCode::Reboot:
                    if(cmd.argments.size() != 0) { uart->writeLine("ERR > Number of arguments does not match."); }
                    software_reset();
                    break;
                case pcabCMD::cmdCode::GetIDN:
                    if(modeBCM)
                    {
                        std::string str = FW_VENDOR + "," + FW_MODEL + "," + serialNum + "," + FW_REV;
                        std::vector<uint8_t> result(std::begin(str), std::end(str));
                        uart->writeSLIP_block(result);
                    }
                    else
                    {
                        if(cmd.argments.size() != 0) { uart->writeLine("ERR > Number of arguments does not match."); }
                        else
                        {
                            if(modeCUI)
                            {
                                uart->writeLine("Vendor   > " + FW_VENDOR);
                                uart->writeLine("Model    > " + FW_MODEL);
                                uart->writeLine("SerialNo > " + serialNum);
                                uart->writeLine("Revision > " + FW_REV);
                            } else { uart->writeLine(FW_VENDOR + "," + FW_MODEL + "," + serialNum + "," + FW_REV); }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::GetIDR:
                    if(cmd.argments.size() != 0) { uart->writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        if(modeCUI) { uart->write("ROM ID > "); }
                        uart->writeLine(Convert::ToString(romID, 16, 16));
                    }
                    break;
                case pcabCMD::cmdCode::SetBR:
                    uint32_t brBF;
                    if(modeBCM)
                    {
                        if(cmd.argment.size() != 4) { uart->writeSLIP_block(retCODE(0xF2)); break; }
                        else { uint32_t brBF = cmd.argment[0] * 0x100 * 0x100 * 0x100 + cmd.argment[1] * 0x100 * 0x100 + cmd.argment[2] * 0x100 + cmd.argment[3]; }
                        uint32_t brRET = uart->setBaudRate(brBF);
                        uart->setBaudRate(brNOW);
                        if(brRET == brBF) 
                        { 
                            uart->writeSLIP_block(retCODE(0x00));
                            brNOW = uart->setBaudRate(brBF);
                        } else { uart->writeSLIP_block(retCODE(0xEF)); }
                    }
                    else
                    {
                        if(cmd.argments.size() != 1) { uart->writeLine("ERR > Number of arguments does not match."); break; }
                        if(!Convert::TryToUInt32(cmd.argments[0], 10, brBF)) { uart->writeLine("ERR > Argument error.");  break; }
                        uint32_t brRET = uart->setBaudRate(brBF);
                        uart->setBaudRate(brNOW);
                        if(brRET == brBF) 
                        { 
                            uart->writeLine("DONE > Set BAUD RATE now.");
                            brNOW = uart->setBaudRate(brBF);
                        }
                        else
                        {
                            char ch[SNPRINTF_BUFFER_LEN];
                            int len = snprintf(ch, sizeof(ch), "[%s] is the closest possible value.", Convert::ToString(brRET, 10, 0).c_str());
                            uart->writeLine(std::string(ch, len));
                        }
                        
                    }
                    break;
                case pcabCMD::cmdCode::BINARY:
                    if(cmd.argments.size() != 0) { uart->writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        modeBCM = true;
                        uart->writeSLIP_block(retCODE(0x00));
                    }
                    break;
                case pcabCMD::cmdCode::ASCII:
                    if(cmd.argment.size() != 0) { uart->writeSLIP_block(retCODE(0xF2)); }
                    else
                    {
                        modeBCM = false;
                        uart->writeLine("DONE > ASCII Communication Mode."); 
                    }
                    break;
                case pcabCMD::cmdCode::NONE:
                    if(modeBCM) { uart->writeSLIP_block(retCODE(0xF1)); }
                    else { uart->writeLine("ERR > Command Not Found."); }
                    break;
                default:
                    //uart->writeLine("");
                    break;
            }
        }
    }
    close_core0();
    return;
}

#pragma region Private Function

void software_reset()
{
    *((volatile uint32_t*)(PPB_BASE + 0x0ED0C)) = 0x5FA0004;
}

void close_core0()
{
    delete uart;
    delete spi_dps;
    delete spi_dsa;
    delete analog;
}

void close_core1()
{
    delete sens;
}

void writeDPS()
{
    std::vector<uint8_t> stBF;
    for(uint16_t i = NUMBER_OF_SYSTEM ; i > 0 ; i-- )
    {
        stBF.push_back(dpsBF[i - 1]);
        dpsNOW[i - 1] = dpsBF[i - 1] & 0x3F;
    }
    spi_dps->spi_write_read(stBF);
}

void writeDSA()
{
    std::vector<uint8_t> stBF;
    for(uint16_t i = NUMBER_OF_SYSTEM ; i > 0 ; i-- )
    {
        stBF.push_back(dsaBF[i - 1] ^ 0x3F);
        dsaNOW[i - 1] = dsaBF[i - 1] & 0x3F;
    }
    spi_dsa->spi_write_read(stBF);
}

void writeDSAin()
{
    uint8_t ioBF = dsaBF[NUMBER_OF_SYSTEM];
    gpio_put(DSA_D0_PIN, !((ioBF >> 0) & 1));
    gpio_put(DSA_D1_PIN, !((ioBF >> 1) & 1));
    gpio_put(DSA_D2_PIN, !((ioBF >> 2) & 1));
    gpio_put(DSA_D3_PIN, !((ioBF >> 3) & 1));
    gpio_put(DSA_D4_PIN, !((ioBF >> 4) & 1));
    dsaNOW[NUMBER_OF_SYSTEM] = dsaBF[NUMBER_OF_SYSTEM];
}

void writeDSAall()
{
    writeDSAin();
    writeDSA();
}

bool romAddressRangeCheck(const uint16_t &blockNum, const uint8_t &sectorpageNum)
{
    uint32_t addr = blockNum * FLASH_BLOCK_SIZE + sectorpageNum * FLASH_PAGE_SIZE;
    if(bootMode == 0x0Fu) { return true; }
    if(bootMode == 0x02u || bootMode == 0x03u)
    {
        if( PICO_FLASH_SIZE_BYTES - FLASH_BLOCK_SIZE <= addr && addr < PICO_FLASH_SIZE_BYTES - FLASH_SECTOR_SIZE) { return true; }
    }
    if(bootMode == 0x0Au && addr == PICO_FLASH_SIZE_BYTES - 2 * FLASH_SECTOR_SIZE) { return true; }
    return false;
}

std::string readSerialNum()
{
    uint8_t romBF[FLASH_PAGE_SIZE];
    flash::readROM(PICO_FLASH_SIZE_BYTES - FLASH_PAGE_SIZE, romBF);
    // Read SERIAL NUMBER
    std::string serial = "";
    uint8_t len = romBF[FLASH_PAGE_SIZE - 1];
    if(len > 16 || len == 0) { return ""; }
    for(uint i = FLASH_PAGE_SIZE - 16; i < FLASH_PAGE_SIZE - 16 + len ; i ++)
    { if(isgraph(romBF[i])) { serial.push_back(romBF[i]); } }
    if(serial.size() == len) { return serial; }
    else { return ""; }
}

bool saveSTATE(const uint8_t &sectorNum, const uint8_t &pageNum, const uint8_t &stateNum)
{
    if(sectorNum > 0xFu || pageNum > 0xFu || stateNum > 0x3u) { return false; }
    if(!romAddressRangeCheck(PICO_FLASH_SIZE_BYTES / FLASH_BLOCK_SIZE - 1, sectorNum * 0x10u + pageNum)) { return false; }
    uint8_t pageBF[FLASH_PAGE_SIZE];
    uint8_t ioBF;
    ioBF = stbAMP;
    ioBF += 2 * stbDRA;
    ioBF += 4 * stbLNA;
    ioBF += 8 * lowMODE;
    if(!flash::readROM(PICO_FLASH_SIZE_BYTES / FLASH_BLOCK_SIZE - 1, sectorNum, pageNum, pageBF)) { return false; }
    for(uint i = 0; i < NUMBER_OF_SYSTEM ; i ++) { pageBF[stateNum * 0x40 + i] = dpsNOW[i]; }
    pageBF[stateNum * 0x40 + NUMBER_OF_SYSTEM] = ioBF;
    for(uint i = 0; i < NUMBER_OF_SYSTEM ; i ++) { pageBF[stateNum * 0x40 + NUMBER_OF_SYSTEM + 1 + i] = dsaNOW[i]; }
    pageBF[stateNum * 0x40 + NUMBER_OF_SYSTEM + 1 + NUMBER_OF_SYSTEM] = dsaNOW[NUMBER_OF_SYSTEM];
    return flash::overwriteROMpage(PICO_FLASH_SIZE_BYTES / FLASH_BLOCK_SIZE - 1, sectorNum, pageNum, pageBF);
}

bool readSTATE(const uint8_t &sectorNum, const uint8_t &pageNum, const uint8_t &stateNum)
{
    uint8_t dat[FLASH_PAGE_SIZE];
    if(!flash::readROM(PICO_FLASH_SIZE_BYTES / FLASH_BLOCK_SIZE - 1, sectorNum, pageNum, dat)) { return false; }
    uint8_t ioBF;
    for(uint i = 0; i < NUMBER_OF_SYSTEM; i++) { if((dat[stateNum * 0x40 + i] & 0xC0) != 0) { return false; } }                         // DPS(0-15) data check
    if((dat[stateNum * 0x40 + NUMBER_OF_SYSTEM] & 0xF0) != 0) { return false; }                                                         // STB & LPW data check
    for(uint i = 0; i < NUMBER_OF_SYSTEM; i++) { if((dat[stateNum * 0x40 + NUMBER_OF_SYSTEM + 1 + i] & 0xC0) != 0) { return false; } }  // DSA(0-15) data check
    if((dat[stateNum * 0x40 + NUMBER_OF_SYSTEM + 1 + NUMBER_OF_SYSTEM] & 0xE0) != 0) { return false; }                                  // DSA(IN) data check
    for(uint i = 0; i < NUMBER_OF_SYSTEM; i++) { dpsBF[i] = dat[stateNum * 0x40 + i]; }
    ioBF = dat[stateNum * 0x40 + NUMBER_OF_SYSTEM];
    for(uint i = 0; i < NUMBER_OF_SYSTEM + 1; i++) { dsaBF[i] = dat[stateNum * 0x40 + NUMBER_OF_SYSTEM + 1 + i]; }
    stbAMP = (ioBF >> 0) & 1;
    stbDRA = (ioBF >> 1) & 1;
    stbLNA = (ioBF >> 2) & 1;
    lowMODE = (ioBF >> 3) & 1;
    return true;
}

uint readBR()
{
    uint8_t dat[FLASH_PAGE_SIZE];
    if(!flash::readROM(PICO_FLASH_SIZE_BYTES / FLASH_BLOCK_SIZE - 1, 0x0F, 0x0F, dat)) { return false; }
    uint32_t ratio;
    ratio = dat[FLASH_PAGE_SIZE - 20] * 0x100 * 0x100 * 0x100 + dat[FLASH_PAGE_SIZE - 19] * 0x100 * 0x100 + dat[FLASH_PAGE_SIZE - 18] * 0x100 + dat[FLASH_PAGE_SIZE - 17];
    if(ratio == 0x00000000 || ratio == 0xFFFFFFFF) { ratio = UART_BAUD_RATE; }
    return ratio;
}

void writeNowSTATE()
{
    // Write now state.
    writeDPS();
    writeDSAall();
    gpio_put(STB_AMP_PIN, !stbAMP);
    gpio_put(STB_DRA_PIN, !stbDRA);
    gpio_put(STB_LNA_PIN, !stbLNA);
    gpio_put(LPW_MOD_PIN, !lowMODE);
}

std::vector<uint8_t> retCODE(uint8_t code)
{
    std::vector<uint8_t> res = std::vector<uint8_t>();
    res.clear();
    res.push_back(code);
    return res;
}

#pragma endregion