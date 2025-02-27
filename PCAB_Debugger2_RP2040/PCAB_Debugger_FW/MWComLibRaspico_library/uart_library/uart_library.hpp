#pragma once
#include <string>
#include <vector>
#include "pico/stdlib.h"

class uartSYNC
{
    private:
    uart_inst_t *uart;
    std::string nlc;
    
    public:

    /// @brief Constructor
    /// @param uartID UART ID (uart0 or uart1)
    /// @param tx_gpio Tx gpio pin number.
    /// @param rx_gpio Rx gpio pin number.
    /// @param baud_ratio Baud ratio.
    /// @param data_bits Number of data bits.
    /// @param stop_bits Number of stop bits.
    /// @param parity Parity.
    /// @param cts CTS
    /// @param rts RTX
    /// @param nlcode New Line string.
    uartSYNC(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, uint data_bits, uint stop_bits, uart_parity_t parity, bool cts, bool rts, std::string nlcode);

    /// @brief Constructor (Data bits = 8 / Stop Bits = 1 / UART_PARITY_NONE / CTS = false / RTS = false)
    /// @param uartID UART ID (uart0 or uart1)
    /// @param tx_gpio Tx gpio pin number.
    /// @param rx_gpio Rx gpio pin number.
    /// @param baud_ratio Baud ratio.
    /// @param nlcode New Line string.
    uartSYNC(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode);

    /// @brief Constructor (UART ID = uart0 / Data bits = 8 / Stop Bits = 1 / UART_PARITY_NONE / CTS = false / RTS = false)
    /// @param tx_gpio Tx gpio pin number.
    /// @param rx_gpio Rx gpio pin number.
    /// @param baud_ratio Baud ratio.
    /// @param nlcode New Line string.
    uartSYNC(uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode);

    /// @brief Constructor (UART ID = uart0 / Data bits = 8 / Stop Bits = 1 / UART_PARITY_NONE / CTS = false / RTS = false)
    /// @param tx_gpio Tx gpio pin number.
    /// @param rx_gpio Rx gpio pin number.
    /// @param baud_ratio Baud ratio.
    uartSYNC(uint tx_gpio, uint rx_gpio, uint baud_ratio);

    /// @brief Constructor (UART ID = uart0 / Data bits = 8 / Stop Bits = 1 / UART_PARITY_NONE / CTS = false / RTS = false)
    uartSYNC();

    /// @brief Destructor
    ~uartSYNC();

    /// @brief Wait for the UART TX fifo to be drained.
    void tx_wait_blocking();

    /// @brief Set UART baud rate.
    /// @param baudrate baud rate.
    /// @return The UART is paused for around two character periods whilst the settings are changed. Data received during this time may be dropped by the UART.
    uint set_baudrate(uint baudrate);

    /// @brief Read one line of string.
    /// @param echo Return echo during communication.
    std::string readLine(bool echo);

    /// @brief Read binary
    /// @param echo Return echo during communication.
    /// @param len The number of bytes to receive.
    std::vector<uint8_t> read(bool echo, size_t len);

    /// @brief Read SLPI block. (Read and decode binary date.)
    /// @param echo Return echo during communication.
    std::vector<uint8_t> readSLIP_block(bool echo);
    
    /// @brief Send string.
    /// @param str string to send.
    void write(std::string str);

    /// @brief Send binary.
    /// @param dat Binary to send.
    void write(std::vector<uint8_t> dat);

    /// @brief Send SLPI block. (Encode and send binary date.)
    /// @param dat Binary to send.
    void writeSLIP_block(std::vector<uint8_t> dat);

    /// @brief Send string as one line. (Add new line string automatically string to send.)
    /// @param str string to send.
    void writeLine(std::string str);
};

