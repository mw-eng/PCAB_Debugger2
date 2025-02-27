#pragma once
#include <vector>
#include "hardware/spi.h"

class spi
{
    private:
    spi_inst_t *spiID;
    uint16_t gpioLE;

    public:
    
    /// @brief Constructor
    /// @param spiID SPI instance specifier, either spi0 or spi1.
    /// @param spiCLK Baudrate required in Hz.
    /// @param clk_gpio CLK gpio pin number.
    /// @param tx_gpio Tx gpio pin number.
    /// @param rx_gpio Rx gpio pin number.
    /// @param le_gpio LE gpio pin number.
    /// @param data_bits Number of data bits per transfer. Valid values 4 to 16. ( If out of range set to 8. )
    /// @param cpol CPOL Bit.
    /// @param cpha CPHA Bit.
    /// @param order true:MSB / false:LSB
    spi(spi_inst_t *spiID, uint spiCLK, uint clk_gpio, uint tx_gpio, uint rx_gpio, uint le_gpio, uint data_bits, bool cpol, bool cpha, bool order);

    /// @brief 
    /// @param spiID SPI instance specifier, either spi0 or spi1.
    /// @param spiCLK Baudrate required in Hz.
    /// @param clk_gpio CLK gpio pin number.
    /// @param tx_gpio Tx gpio pin number.
    /// @param rx_gpio Rx gpio pin number.
    /// @param le_gpio LE gpio pin number.
    /// @param data_bits Number of data bits per transfer. Valid values 4 to 16. ( If out of range set to 8. )
    /// @param mode mode0 = CPOL0,CPHA0 / mode1 = CPOL0,CPHA1 / mode3 = CPOL1,CPHA0 / mode4 = CPOL1,CPHA1 / other = mode0
    /// @param order true:MSB / false:LSB
    spi(spi_inst_t *spiID, uint spiCLK, uint clk_gpio, uint tx_gpio, uint rx_gpio, uint le_gpio, uint data_bits, uint8_t mode, bool order);

    /// @brief Constructor (spi0, 8MHz, SPI0_CLK_PIN, SPI0_TX_PIN, SPI0_RX_PIN, SPI0_LE_PIN, 8bit, mode0, MSB)
    spi();

    /// @brief Set LE enable.
    /// @param mode enable or disable. (true:LOW / false:HIGH)
    void setLE(bool mode);

    /// @brief Set LE enable. (LE PIN is LOW)
    void setLE_enable();

    /// @brief Set LE enable. (LE PIN is HIGH)
    void setLE_disable();

    /// @brief SPI write and read. (LE no set.)
    /// @param wdat Write data.
    /// @return Read data.
    std::vector<uint8_t> write_read(std::vector<uint8_t> wdat);

    /// @brief SPI write and read. (LE no set.)
    /// @param wdat Write data.
    /// @return Read data.
    std::vector<uint16_t> write_read(std::vector<uint16_t> wdat);

    /// @brief SPI write and read.
    /// @param wdat Write data.
    /// @return Read data.
    std::vector<uint8_t> spi_write_read(std::vector<uint8_t> wdat);

    /// @brief SPI write and read.
    /// @param wdat Write data.
    /// @return Read data.
    std::vector<uint16_t> spi_write_read(std::vector<uint16_t> wdat);

};
