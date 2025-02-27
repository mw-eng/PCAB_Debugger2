#include "adc_library.hpp"

#define ADC0 26
#define ADC1 27
#define ADC2 28
#define ADC3 29
#define ADC_TEMP_SENSOR 4


adc::adc(bool adc0, bool adc1, bool adc2, bool adc3, float vref) : adc0(adc0), adc1(adc1), adc2(adc2), vref(vref)
{
    adc_init();
    if(adc0) { adc_gpio_init(ADC0); }
    if(adc1) { adc_gpio_init(ADC1); }
    if(adc2) { adc_gpio_init(ADC2); }
    if(adc3) { adc_gpio_init(ADC3); }
    adc_set_temp_sensor_enabled(true);
}

adc::adc() : adc(true, true, true, true, 3.3f) {}

uint16_t adc::readADC0()
{
    if(adc0)
    {
        adc_select_input(0);
        return adc_read();
    }
    else { return 0xff; }
}

uint16_t adc::readADC1()
{
    if(adc1)
    {
        adc_select_input(1);
        return adc_read();
    }
    else { return 0xff; }
}

uint16_t adc::readADC2()
{
    if(adc2)
    {
        adc_select_input(2);
        return adc_read();
    }
    else { return 0xff; }
}

uint16_t adc::readADC3()
{
    if(adc3)
    {
        adc_select_input(3);
        return adc_read();
    }
    else { return 0xff; }
}

uint16_t adc::readADC4()
{
    adc_select_input(ADC_TEMP_SENSOR);
    return adc_read();
}

float adc::readVoltageADC0() { return readADC0() * vref / (1 << 12); }
float adc::readVoltageADC1() { return readADC1() * vref / (1 << 12); }
float adc::readVoltageADC2() { return readADC2() * vref / (1 << 12); }
float adc::readVoltageADC3() { return readADC3() * vref / (1 << 12); }
float adc::readVoltageADC4() { return readADC4() * vref / (1 << 12); }
float adc::readTempCPU() { return 27.0f - ( readVoltageADC4() - 0.706f ) / 0.001721f; }
