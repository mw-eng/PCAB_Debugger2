#include "uart_library.hpp"
#include "MWComLibCPP_library.hpp"

uartSYNC::uartSYNC(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, uint data_bits, uint stop_bits, uart_parity_t parity, bool cts, bool rts, std::string nlcode)
: uart(uartID), nlc(nlcode)
{
    uart_init(uart, baud_ratio);
    gpio_set_function(tx_gpio, GPIO_FUNC_UART);
    gpio_set_function(rx_gpio, GPIO_FUNC_UART);
    uart_set_hw_flow(uart, cts, rts);
    uart_set_format(uart, data_bits, stop_bits, parity);
}

uartSYNC::uartSYNC(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode)
: uartSYNC(uartID, tx_gpio, rx_gpio, baud_ratio, 8, 1, UART_PARITY_NONE, false, false, nlcode){}

uartSYNC::uartSYNC(uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode)
: uartSYNC(uart0, tx_gpio, rx_gpio, baud_ratio, nlcode){}

uartSYNC::uartSYNC(uint tx_gpio, uint rx_gpio, uint baud_ratio)
: uartSYNC(uart0, tx_gpio, rx_gpio, baud_ratio, "\r\n"){}

uartSYNC::uartSYNC() : uartSYNC(0, 1, 9600){}

uartSYNC::~uartSYNC() { uart_deinit(uart); }

/// @brief Serial Line Internet Protocol Encode
/// @param dat Original Data
/// @return SLIP Data
std::vector<uint8_t> EncodeSLIP(const std::vector<uint8_t> &dat)
{
    std::vector<uint8_t> slip;
    slip.clear();
    for(uint8_t d : dat)
    {
        switch (d)
        {
        case 0xC0:
            slip.push_back(0xDB);
            slip.push_back(0xDC);
            break;
        case 0xDB:
            slip.push_back(0xDB);
            slip.push_back(0xDD);
            break;
        default:
            slip.push_back(d);
            break;
        }
    }
    slip.push_back(0xC0);
    return slip;
}

 /// @brief Serial Line Internet Protocol Decode
 /// @param dat SLIP Data
 /// @return Original Data
std::vector<uint8_t> DecodeSLIP(const std::vector<uint8_t> &dat)
{
    std::vector<uint8_t> slip;
    slip.clear();

    for(size_t i = 0; i < dat.size(); ++i)
    {
        switch (dat[i])
        {
        case 0xC0:
            return slip;
        case 0xDB:
            if(dat[i + 1] == 0xDC) { slip.push_back(0xC0); }
            else if(dat[i + 1] == 0xDD) { slip.push_back(0xDB); }
            ++i;
            break;
        default:
            slip.push_back(dat[i]);
            break;
        }
    }
    return slip;
}


void uartSYNC::tx_wait_blocking() { uart_tx_wait_blocking(uart); }

uint uartSYNC::set_baudrate(uint baudrate)
{
    tx_wait_blocking();
    uart_set_irq_enables(uart, false, false);
    uint result = uart_set_baudrate(uart, baudrate);
    uart_set_irq_enables(uart, true, true);
    return result;
}

std::string uartSYNC::readLine(bool echo)
{
    char chBF;
    std::string strBf = "";
    do
    {
        chBF = uart_getc(uart);
        if(echo){char chRET[] = {chBF, '\0'}; uart_puts(uart, chRET);}
        strBf += chBF;
        if(strBf.find_last_not_of(nlc) == std::string::npos){return "";}
    } while (strBf.find_last_not_of(nlc) == strBf.length() - 1 );
    return String::rtrim(strBf, nlc);
}

std::vector<uint8_t> uartSYNC::readSLIP_block(bool echo)
{
    uint8_t byteBF[1];
    std::vector<uint8_t> dat;
    dat.clear();
    do{
        uart_read_blocking(uart, byteBF,sizeof(byteBF));
        if(echo){uart_write_blocking(uart, byteBF, sizeof(byteBF));}
        dat.push_back(byteBF[0]);
        if(dat.size() == SIZE_MAX - 1){return std::vector<uint8_t>();}
    }while(byteBF[0] != 0xC0);
    return DecodeSLIP(dat);
}

 std::vector<uint8_t> uartSYNC::read(bool echo, size_t len)
{
    uint8_t byteBF[len];
    uart_read_blocking(uart, byteBF, sizeof(byteBF));
    std::vector<uint8_t> _dst(byteBF, byteBF + len);
    if(echo) { uart_write_blocking(uart, byteBF, len); }
    return _dst;
}

void uartSYNC::write(std::string str) { uart_puts(uart, str.c_str()); }

void uartSYNC::write(std::vector<uint8_t> dat) { uart_write_blocking(uart, dat.data(), dat.size()); }

void uartSYNC::writeSLIP_block(std::vector<uint8_t> dat){ write(EncodeSLIP(dat)); }

void uartSYNC::writeLine(std::string str) { write(str + nlc); }

