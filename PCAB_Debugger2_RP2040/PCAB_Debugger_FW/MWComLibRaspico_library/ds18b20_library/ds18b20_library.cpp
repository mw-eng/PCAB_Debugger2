#include "ds18b20_library.hpp"

// Function commands for d218b20 1-Wire temperature sensor 
// https://www.analog.com/en/products/ds18b20.html
//
#define DS18B20_CONVERT_T           0x44
#define DS18B20_WRITE_SCRATCHPAD    0x4e
#define DS18B20_READ_SCRATCHPAD     0xbe
#define DS18B20_COPY_SCRATCHPAD     0x48
#define DS18B20_RECALL_EE           0xb8
#define DS18B20_READ_POWER_SUPPLY   0xb4

// Generic ROM commands for 1-Wire devices
// https://www.analog.com/en/technical-articles/guide-to-1wire-communication.html
//
#define OW_READ_ROM         0x33
#define OW_MATCH_ROM        0x55
#define OW_SKIP_ROM         0xCC
#define OW_ALARM_SEARCH     0xEC
#define OW_SEARCH_ROM       0xF0


ds18b20::ds18b20(PIO pioID, uint gpioNumber, uint8_t senser_count_max)
{
    //pull up check
    gpio_init(gpioNumber);
    gpio_set_dir(gpioNumber ,GPIO_IN);
    if(!gpio_get(gpioNumber)){ SENS_TMP.clear(); return; }
    
    SENS_TMP.clear();
    if (pio_can_add_program (pioID, &onewire_program)) {
        uint offset = pio_add_program (pioID, &onewire_program);
        // claim a state machine and initialise a driver instance
        if (ow_init (&ow, pioID, offset, gpioNumber)) {
            uint64_t romcode[senser_count_max];
            // find and display 64-bit device addresses
            int num_devs = ow_romsearch (&ow, romcode, senser_count_max, OW_SEARCH_ROM);
            for(uint8_t i = 0; i < num_devs; i++)
            {
                SENS_TMP.push_back(romcode[i]);
            }
        }
    }
}

ds18b20::ds18b20(PIO pioID, uint gpioNumber) : ds18b20(pioID, gpioNumber, UINT8_MAX) {}

ds18b20::ds18b20(uint gpioNumber) : ds18b20(pio0, gpioNumber) {}

ds18b20::ds18b20() : ds18b20(0) {}

ds18b20::~ds18b20(){}

std::vector<uint16_t> ds18b20::readSENS()
{
    if(SENS_TMP.size() <= 0) { return std::vector<uint16_t>(); }
    std::vector<uint16_t> temp;
    temp.clear();
    ow_reset (&ow);
    ow_send (&ow, OW_SKIP_ROM);
    ow_send (&ow, DS18B20_CONVERT_T);
    while (ow_read(&ow) == 0);
    for (uint64_t &x:SENS_TMP) 
    {
        ow_reset (&ow);
        ow_send (&ow, OW_MATCH_ROM);
        for (int b = 0; b < 64; b += 8) {
            ow_send (&ow, x >> b);
        }
        ow_send (&ow, DS18B20_READ_SCRATCHPAD);
        uint16_t tempBf = 0;
        tempBf = ow_read (&ow) | (ow_read (&ow) << 8);
        temp.push_back(tempBf);
    }
    return temp;
}

uint16_t ds18b20::readSENS(uint sensNUM)
{
    if(SENS_TMP.size() <= sensNUM) { return 0; }
    ow_reset (&ow);
    ow_send (&ow, OW_SKIP_ROM);
    ow_send (&ow, DS18B20_CONVERT_T);
    while (ow_read(&ow) == 0);
    ow_reset (&ow);
    ow_send (&ow, OW_MATCH_ROM);
    for (int b = 0; b < 64; b += 8) {
        ow_send (&ow, SENS_TMP[sensNUM] >> b);
    }
    ow_send (&ow, DS18B20_READ_SCRATCHPAD);
    uint16_t tempBf = 0;
    tempBf = ow_read (&ow) | (ow_read (&ow) << 8);
    return tempBf;
}

std::vector<float> ds18b20::readTEMP()
{
    std::vector<float> temp;
    temp.clear();
    for (uint16_t &x:readSENS()) { temp.push_back(x / 16.0f); }
    return temp;
}

float ds18b20::readTEMP(uint sensNUM) { return readSENS(sensNUM) / 16.0f; }

std::vector<float> ds18b20::convertTEMP(std::vector<uint16_t> sensDAT)
{
    std::vector<float> temp;
    temp.clear();
    for (uint16_t &x:sensDAT) { temp.push_back(x / 16.0f); }
    return temp;
}

float ds18b20::convertTEMP(uint16_t sensDAT) { return sensDAT / 16.0f; }

int16_t ds18b20::getNumberOfSenser() { return SENS_TMP.size(); }

std::vector<uint64_t> ds18b20::getSENS_ROMCODE() { return SENS_TMP; }

uint64_t ds18b20::getSENS_ROMCODE(uint sensNUM)
{
    if(sensNUM < SENS_TMP.size()) { return SENS_TMP[sensNUM]; }
    else{ return 0; }
}
