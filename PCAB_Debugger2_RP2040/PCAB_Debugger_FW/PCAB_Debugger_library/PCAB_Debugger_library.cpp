#include "PCAB_Debugger_library.hpp"
#include "MWComLibCPP_library.hpp"

#pragma region pcabCMD Class

pcabCMD::pcabCMD(uartSYNC uart, uint rs485de_gpio, uint32_t rs485_WaitEnable, uint32_t rs485_Waite)
: uart(uart), de_gpio(rs485de_gpio), de_mode(false), waitBeforEN(rs485_WaitEnable), waitEN(rs485_Waite)
{
    gpio_init(rs485de_gpio);
    gpio_set_dir(rs485de_gpio ,GPIO_OUT);
    rs485disable();
}

pcabCMD::pcabCMD(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, uint data_bits, uint stop_bits, uart_parity_t parity, bool cts, bool rts, std::string nlcode, uint rs485de_gpio, uint32_t rs485_WaitEnable, uint32_t rs485_Waite)
: uart(uartSYNC(uartID, tx_gpio, rx_gpio, baud_ratio, data_bits, stop_bits, parity, cts, rts, nlcode)), de_gpio(rs485de_gpio), de_mode(false), waitBeforEN(rs485_WaitEnable), waitEN(rs485_Waite)
{
    gpio_init(rs485de_gpio);
    gpio_set_dir(rs485de_gpio ,GPIO_OUT);
    rs485disable();
}

pcabCMD::pcabCMD(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode, uint rs485de_gpio, uint32_t rs485_WaitEnable, uint32_t rs485_Waite)
: pcabCMD(uartID, tx_gpio, rx_gpio, baud_ratio, 8, 1, UART_PARITY_NONE, false, false, nlcode, rs485de_gpio, rs485_WaitEnable, rs485_Waite){}

pcabCMD::pcabCMD(uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode, uint rs485de_gpio, uint32_t rs485_WaitEnable, uint32_t rs485_Waite)
: pcabCMD(uart0, tx_gpio, rx_gpio, baud_ratio, nlcode, rs485de_gpio, rs485_WaitEnable, rs485_Waite){}

pcabCMD::pcabCMD(uint tx_gpio, uint rx_gpio, uint baud_ratio, uint rs485de_gpio, uint32_t rs485_WaitEnable, uint32_t rs485_Waite)
: pcabCMD(uart0, tx_gpio, rx_gpio, baud_ratio, "\r\n", rs485de_gpio, rs485_WaitEnable, rs485_Waite){}

pcabCMD::pcabCMD() : pcabCMD(0, 1, 9600, 2, 20, 10){}

pcabCMD::~pcabCMD() { uart.~uartSYNC(); }

pcabCMD::CommandLine pcabCMD::readCMD(bool echo, bool slpi)
{
    if(slpi)
    {
        std::vector<uint8_t> datBF = readSLIP_block(echo);
        std::string serialNum;
        std::string romID = "";
        size_t cnt = 0;
        if(datBF.size() == 0){ return pcabCMD::CommandLine("", "", cmdCode::NUL, NULL, 0); }
        if(datBF[0] == 0x23)
        {
            for(size_t s = 0; s < datBF.size(); ++s)
            {
                if(datBF[s]==0xFF) { cnt = s; break; }
            }
            if(cnt == 0 || datBF.size() <= cnt + 1){ return pcabCMD::CommandLine("", "", cmdCode::NUL, NULL, 0); }
            std::string strBF = std::string(datBF.begin(), datBF.begin() + cnt);
            serialNum = strBF.substr(1);
        }else if(datBF[0] == 0x24)
        {
            if(datBF.size() <= 9) { return pcabCMD::CommandLine("", "", cmdCode::NUL, NULL, 0); }
            romID = Convert::ToString(datBF[1], 16, 2);
            romID = Convert::ToString(datBF[2], 16, 2);
            romID = Convert::ToString(datBF[3], 16, 2);
            romID = Convert::ToString(datBF[4], 16, 2);
            romID = Convert::ToString(datBF[5], 16, 2);
            romID = Convert::ToString(datBF[6], 16, 2);
            romID = Convert::ToString(datBF[7], 16, 2);
            romID = Convert::ToString(datBF[8], 16, 2);
            cnt = 8;
        } else { return pcabCMD::CommandLine("", "", cmdCode::NUL, NULL, 0); }

        std::vector<uint8_t> arg = std::vector<uint8_t>(datBF.size() - cnt - 2);
        copy(datBF.begin() + cnt + 2, datBF.end(), arg.begin());
        
        switch (datBF[cnt + 1])
        {
        case 0xB0:
        case 0xC1: return pcabCMD::CommandLine(serialNum, romID, cmdCode::WrtDSA, arg);
        case 0xC2: return pcabCMD::CommandLine(serialNum, romID, cmdCode::WrtDPS, arg);
        case 0xC3: return pcabCMD::CommandLine(serialNum, romID, cmdCode::SetSTB_AMP, arg);
        case 0xC4: return pcabCMD::CommandLine(serialNum, romID, cmdCode::SetSTB_DRA, arg);
        case 0xC5: return pcabCMD::CommandLine(serialNum, romID, cmdCode::SetSTB_LNA, arg);
        case 0xC6: return pcabCMD::CommandLine(serialNum, romID, cmdCode::SetLPM, arg);
        case 0xD0: return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetDSA, {0x0F});
        case 0xD1: return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetDSA, arg);
        case 0xD2: return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetDPS, arg);
        case 0xD3: return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetSTB_AMP, arg);
        case 0xD4: return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetSTB_DRA, arg);
        case 0xD5: return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetSTB_LNA, arg);
        case 0xD6: return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetLPM, arg);
        case 0xE1: return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetTMP_ID, arg);
        case 0xE2: return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetTMP_VAL, arg);
        case 0xE3: return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetTMP_CPU, arg);
        case 0xE4: return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetVd, arg);
        case 0xE5: return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetId, arg);
        case 0xE6: return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetVin, arg);
        case 0xE7: return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetPin, arg);
        case 0xEA: return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetMODE, arg);
        case 0xEE: return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetAD, arg);
        case 0xEF: return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetSENS, arg);
        case 0xF0: return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetIDN, arg);
        case 0xFA: return pcabCMD::CommandLine(serialNum, romID, cmdCode::RST, arg);
        case 0xFB: return pcabCMD::CommandLine(serialNum, romID, cmdCode::SaveMEM, arg);
        case 0xFC: return pcabCMD::CommandLine(serialNum, romID, cmdCode::LoadMEM, arg);
        case 0xFD: return pcabCMD::CommandLine(serialNum, romID, cmdCode::SetBR, arg);
        case 0xFE: return pcabCMD::CommandLine(serialNum, romID, cmdCode::ASCII, arg);
        case 0xAA: return pcabCMD::CommandLine(serialNum, romID, cmdCode::ReadROM, arg);
        case 0xBB: return pcabCMD::CommandLine(serialNum, romID, cmdCode::OverwriteROM, arg);
        default: return pcabCMD::CommandLine(serialNum, romID, cmdCode::NONE, NULL, 0);
        }
    }
    else
    {
        std::string strBf = readLine(echo);
        std::string serialNum = "";
        std::string romID = "";
        std::string command = "";
        std::vector<std::string> strVect = String::split(strBf, ' ');
        if(strVect.size() <= 0){return pcabCMD::CommandLine("", "", cmdCode::NUL, NULL, 0);}
        strBf = String::trim(strVect[0]);
        strVect.erase(std::cbegin(strVect));
        if(strBf.size() > 0)
        {
            if(String::strCompare(strBf.substr(0, 1), "#", true) && strVect.size() > 0)
            {
                serialNum = strBf.substr(1);
                command = strVect[0];
                strVect.erase(std::cbegin(strVect));
            }else if(String::strCompare(strBf.substr(0, 1), "$", true) && strVect.size() > 0)
            {
                romID = strBf.substr(1);
                command = strVect[0];
                strVect.erase(std::cbegin(strVect));
            }
        }
        std::vector<std::string> argments;
        for(uint i = 0 ; i < strVect.size() ; i++){ argments.push_back(strVect[i]);}

        std::string cmd = String::trim(command);
        if(cmd == "" && argments.size() == 0) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::NUL, NULL, 0); }
        std::string strArr[argments.size()];
        std::copy(argments.begin(), argments.end(), strArr);
        if (String::strCompare(cmd, "WrtDPS", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::WrtDPS, strArr, argments.size()); }
        if (String::strCompare(cmd, "GetDPS", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetDPS, strArr, argments.size()); }
        if (String::strCompare(cmd, "SetDPS", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::SetDPS, strArr, argments.size()); }
        if (String::strCompare(cmd, "WrtDSA", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::WrtDSA, strArr, argments.size()); }
        if (String::strCompare(cmd, "GetDSA", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetDSA, strArr, argments.size()); }
        if (String::strCompare(cmd, "SetDSA", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::SetDSA, strArr, argments.size()); }
        if (String::strCompare(cmd, "GetTMP.ID", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetTMP_ID, strArr, argments.size()); }
        if (String::strCompare(cmd, "GetTMP.Val", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetTMP_VAL, strArr, argments.size()); }
        if (String::strCompare(cmd, "GetTMP.CPU", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetTMP_CPU, strArr, argments.size()); }
        if (String::strCompare(cmd, "GetVd", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetVd, strArr, argments.size()); }
        if (String::strCompare(cmd, "GetId", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetId, strArr, argments.size()); }
        if (String::strCompare(cmd, "GetVin", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetVin, strArr, argments.size()); }
        if (String::strCompare(cmd, "GetPin", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetPin, strArr, argments.size()); }
        if (String::strCompare(cmd, "GetSTB.AMP", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetSTB_AMP, strArr, argments.size()); }
        if (String::strCompare(cmd, "GetSTB.DRA", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetSTB_DRA, strArr, argments.size()); }
        if (String::strCompare(cmd, "GetSTB.LNA", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetSTB_LNA, strArr, argments.size()); }
        if (String::strCompare(cmd, "SetSTB.AMP", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::SetSTB_AMP, strArr, argments.size()); }
        if (String::strCompare(cmd, "SetSTB.DRA", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::SetSTB_DRA, strArr, argments.size()); }
        if (String::strCompare(cmd, "SetSTB.LNA", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::SetSTB_LNA, strArr, argments.size()); }
        if (String::strCompare(cmd, "GetLPM", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetLPM, strArr, argments.size()); }
        if (String::strCompare(cmd, "SetLPM", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::SetLPM, strArr, argments.size()); }
        if (String::strCompare(cmd, "SetALD", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::SetALD, strArr, argments.size()); }
        if (String::strCompare(cmd, "GetALD", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetALD, strArr, argments.size()); }
        if (String::strCompare(cmd, "SMEM", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::SaveMEM, strArr, argments.size()); }
        if (String::strCompare(cmd, "LMEM", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::LoadMEM, strArr, argments.size()); }
        if (String::strCompare(cmd, "RROM", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::ReadROM, strArr, argments.size()); }
        if (String::strCompare(cmd, "WROM", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::WriteROM, strArr, argments.size()); }
        if (String::strCompare(cmd, "OROM", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::OverwriteROM, strArr, argments.size()); }
        if (String::strCompare(cmd, "EROM", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::EraseROM, strArr, argments.size()); }
        if (String::strCompare(cmd, "SetSN", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::SetSN, strArr, argments.size()); }
        if (String::strCompare(cmd, "RST", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::RST, strArr, argments.size()); }
        if (String::strCompare(cmd, "*RST", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::RST, strArr, argments.size()); }
        if (String::strCompare(cmd, "ECHO", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::ECHO, strArr, argments.size()); }
        if (String::strCompare(cmd, "CUI", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::CUI, strArr, argments.size()); }
        if (String::strCompare(cmd, "GetMODE", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetMODE, strArr, argments.size()); }
        if (String::strCompare(cmd, "ReBOOT", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::Reboot, strArr, argments.size()); }
        if (String::strCompare(cmd, "GetIDN", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetIDN, strArr, argments.size()); }
        if (String::strCompare(cmd, "*IDN?", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetIDN, strArr, argments.size()); }
        if (String::strCompare(cmd, "GetIDR", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::GetIDR, strArr, argments.size()); }
        if (String::strCompare(cmd, "SetBR", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::SetBR, strArr, argments.size()); }
        if (String::strCompare(cmd, "BCM", true)) { return pcabCMD::CommandLine(serialNum, romID, cmdCode::BINARY, strArr, argments.size()); }
        else { return pcabCMD::CommandLine(serialNum, romID, cmdCode::NONE, strArr, argments.size()); }
    }
}

void pcabCMD::writeSLIP_block(std::vector<uint8_t> dat)
{
    if(!de_mode){sleep_ms(waitBeforEN); rs485enable(); sleep_ms(waitEN);}
    uart.writeSLIP_block(dat);
    uart.tx_wait_blocking();
    sleep_ms(waitEN);
    rs485disable();
}

void pcabCMD::write(std::string str)
{
    if(!de_mode){sleep_ms(waitBeforEN); rs485enable(); sleep_ms(waitEN);}
    uart.write(str);
    uart.tx_wait_blocking();
    sleep_ms(waitEN);
    rs485disable();
}

void pcabCMD::writeLine(std::string str)
{
    if(!de_mode){sleep_ms(waitBeforEN); rs485enable(); sleep_ms(waitEN);}
    uart.writeLine(str);
    uart.tx_wait_blocking();
    sleep_ms(waitEN);
    rs485disable();
}

void pcabCMD::rs485enable()
{
    gpio_put(de_gpio, true);
    de_mode = true;
}

void pcabCMD::rs485disable()
{
    gpio_put(de_gpio, false);
    de_mode = false;
}

bool pcabCMD::getRS485mode(){ return de_mode; }

uint pcabCMD::setBaudRate(uint baudrate){ return uart.set_baudrate(baudrate); }

#pragma endregion pcabCMD Class
