#pragma once
#include "hardware/adc.h"

class adc
{
    private:
    bool adc0;
    bool adc1;
    bool adc2;
    bool adc3;
    float vref;

    public:
    
    /// @brief Constructor
    /// @param adc0 Use adc0.
    /// @param adc1 Use adc1.
    /// @param adc2 Use adc2.
    /// @param adc3 Use adc2.
    /// @param vref Vref value.
    adc(bool adc0, bool adc1, bool adc2, bool adc3, float vref);

    /// @brief Constructor ( ADCs all used and vref = 3.3v )
    adc();

    /// @brief Read adc0 value.
    /// @return ADC0 value.
    uint16_t readADC0();

    /// @brief Read adc01 value.
    /// @return ADC1 value.
    uint16_t readADC1();

    /// @brief Read adc2 value.
    /// @return ADC2 value.
    uint16_t readADC2();

    /// @brief Read adc3(Vref) value.
    /// @return ADC3(Vref) value.
    uint16_t readADC3();

    /// @brief Read adc4(CPU Tempreature) value.
    /// @return ADC0 value.
    uint16_t readADC4();

    /// @brief Read adc0 voltage value.
    /// @return ADC0 voltage value.
    float readVoltageADC0();

    /// @brief Read adc1 voltage value.
    /// @return ADC1 voltage value.
    float readVoltageADC1();

    /// @brief Read adc2 voltage value.
    /// @return ADC2 voltage value.
    float readVoltageADC2();

    /// @brief Read adc3(Vref) voltage value.
    /// @return ADC3(Vref) voltage value.
    float readVoltageADC3();

    /// @brief Read adc4(CPU Tempreature) voltage value.
    /// @return ADC4(CPU Tempreature) voltage value.
    float readVoltageADC4();

    /// @brief Read Vref(adc3) voltage value.
    /// @return Vref(adc3) voltage value.
    float readVsys();

    /// @brief Read CPU Tempreature(adc4) value.
    /// @return CPU Tempreature(adc4) value.
    float readTempCPU();

};