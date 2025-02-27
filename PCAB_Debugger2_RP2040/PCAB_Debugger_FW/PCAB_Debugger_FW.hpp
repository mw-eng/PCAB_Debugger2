#include "MWComLibCPP_library.hpp"
#include "PCAB_Debugger_library.hpp"
#include "ds18b20_library.hpp"
#include "flash_library.hpp"
#include "spi_library.hpp"
#include "adc_library.hpp"
#include "pico/multicore.h"

#define DEBUG_RASPICO
//#define DEBUG_BOOT_MODE 0x03
//#define DEBUG_BOOT_MODE 0x0A
//#define DEBUG_BOOT_MODE 0x0F

// IO Configure
// UART PIN Configure
#define UART_TX_PIN 0
#define UART_RX_PIN 1
#define RS485_DE_PIN 2
// SPI PIN Configure
#define SPI0_TX_PIN 7
#define SPI0_RX_PIN 4
#define SPI0_LE_PIN 5
#define SPI0_CLK_PIN 6
#define SPI1_TX_PIN 15
#define SPI1_RX_PIN 12
#define SPI1_LE_PIN 13
#define SPI1_CLK_PIN 14
// Onewire PIN Configure
#define SNS_TEMP_PIN 25
// MODE PIN Configure
#define LPW_MOD_PIN 18
#define STB_DRA_PIN 19
#define STB_AMP_PIN 20
#define STB_LNA_PIN 21
#define DSA_D0_PIN 16
#define DSA_D1_PIN 17
#define DSA_D2_PIN 22
#define DSA_D3_PIN 23
#define DSA_D4_PIN 24
// Switch PIN Configure
//#define SW_1_PIN 2
//#define SW_2_PIN 3
#define SW_1_PIN 8
#define SW_2_PIN 9
#define SW_3_PIN 10
#define SW_4_PIN 11

// UART Configure
//#define UART_BAUD_RATE 9600
//#define UART_BAUD_RATE 115207
#define UART_BAUD_RATE 1000000
#define UART_STOP_BIT 1
#define UART_DATA_BITS 8
//#define UART_PARITY_ENABLE
#define RS485_DE_ENB_WAITE_TIME 1
#define RS485_DE_WAITE_TIME 0

// SPI Configure
#define SPI_MODE 3
#define SPI_CLK 1000000
#define SPI_BITS 6
#define SPI_ORDER false // 0:LSB / 1:MSB


void software_reset();
void setup();
void main_core0();
void main_core1();
void close_core0();
void close_core1();
void writeDPS();
void writeDSA();
void writeDSAin();
void writeDSAall();
bool romAddressRangeCheck(const uint16_t &blockNum, const uint8_t &sectorpageNum);
std::string readSerialNum();
bool saveSTATE(const uint8_t &sectorNum, const uint8_t &pageNum, const uint8_t &stateNum);
bool readSTATE(const uint8_t &sectorNum, const uint8_t &pageNum, const uint8_t &stateNum);
void writeNowSTATE();
uint readBR();
std::vector<uint8_t> retCODE(uint8_t code);