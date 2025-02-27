#include "spi_library.hpp"
#include "pico/stdlib.h"

#define SPI0_TX_PIN 7
#define SPI0_RX_PIN 4
#define SPI0_LE_PIN 5
#define SPI0_CLK_PIN 6

spi::spi(spi_inst_t *spiID, uint spiCLK, uint clk_gpio, uint tx_gpio, uint rx_gpio, uint le_gpio, uint data_bits, bool cpol, bool cpha, bool order) : spiID(spiID), gpioLE(le_gpio)
{
    spi_init(spiID, spiCLK);
    gpio_set_function(clk_gpio, GPIO_FUNC_SPI);
    gpio_set_function(tx_gpio, GPIO_FUNC_SPI);
    gpio_set_function(rx_gpio, GPIO_FUNC_SPI);
    gpio_set_function(gpioLE, GPIO_FUNC_SPI);
    gpio_init(gpioLE);
    gpio_set_dir(gpioLE , GPIO_OUT);
    setLE_disable();
    spi_cpol_t cpolt;
    spi_cpha_t cphat;
    spi_order_t ordert;
    uint bits;
    if( 4 <= data_bits && data_bits <= 16 ){ bits = data_bits; }
    else { bits = 8; }
    if(cpol) { cpolt = SPI_CPOL_1; }
    else { cpolt = SPI_CPOL_0; }
    if(cpha) { cphat = SPI_CPHA_1; }
    else { cphat = SPI_CPHA_0; }
    if(order) { ordert = SPI_MSB_FIRST; }
    else { ordert = SPI_LSB_FIRST; }
    spi_set_format(spiID, bits, cpolt, cphat, ordert);
}


spi::spi(spi_inst_t *spiID, uint spiCLK, uint clk_gpio, uint tx_gpio, uint rx_gpio, uint le_gpio, uint data_bits, uint8_t mode, bool order) : spiID(spiID), gpioLE(le_gpio)
{
    switch (mode)
    {
    case 1:
        spi(spiID, spiCLK, clk_gpio, tx_gpio, rx_gpio, le_gpio, data_bits, false, true, order);
        break;
    case 2:
        spi(spiID, spiCLK, clk_gpio, tx_gpio, rx_gpio, le_gpio, data_bits, true, false, order);
        break;
    case 3:
        spi(spiID, spiCLK, clk_gpio, tx_gpio, rx_gpio, le_gpio, data_bits, true, true, order);
        break;
    default:
        spi(spiID, spiCLK, clk_gpio, tx_gpio, rx_gpio, le_gpio, data_bits, false, false, order);
        break;
    }
}

spi::spi() : spi(spi0, 8000000, SPI0_CLK_PIN, SPI0_TX_PIN, SPI0_RX_PIN, SPI0_LE_PIN, 8, 0, true) {}


void spi::setLE(bool mode)
{
    if(mode) { setLE_enable(); }
    else { setLE_disable(); }
}

void spi::setLE_enable() { gpio_put(gpioLE, 0); }

void spi::setLE_disable(){ gpio_put(gpioLE, 1); }

std::vector<uint8_t> spi::write_read(std::vector<uint8_t> wdat)
{
    if(wdat.size() <= 0) { return std::vector<uint8_t>(); }
    uint8_t writeDAT[wdat.size()];
    uint8_t buffer[wdat.size()];
    std::copy(wdat.begin(), wdat.end(), writeDAT);
    spi_write_read_blocking(spiID, writeDAT, buffer, wdat.size());
    return std::vector<uint8_t>(buffer, buffer + wdat.size());
}

std::vector<uint16_t> spi::write_read(std::vector<uint16_t> wdat)
{
    if(wdat.size() <= 0) { return std::vector<uint16_t>(); }
    uint16_t writeDAT[wdat.size()];
    uint16_t buffer[wdat.size()];
    std::copy(wdat.begin(), wdat.end(), writeDAT);
    spi_write16_read16_blocking(spiID, writeDAT, buffer, wdat.size());
    return std::vector<uint16_t>(buffer, buffer + wdat.size());
}

std::vector<uint8_t> spi::spi_write_read(std::vector<uint8_t> wdat)
{
    if(wdat.size() <= 0) { return std::vector<uint8_t>(); }
    std::vector<uint8_t> buffer;
    setLE_enable();
    buffer = write_read(wdat);
    setLE_disable();
    return buffer;
}

std::vector<uint16_t> spi::spi_write_read(std::vector<uint16_t> wdat)
{
    if(wdat.size() <= 0) { return std::vector<uint16_t>(); }
    std::vector<uint16_t> buffer;
    setLE_enable();
    buffer = write_read(wdat);
    setLE_disable();
    return buffer;
}
