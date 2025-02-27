set(ProjectName "PCAB_Debugger_FW")

add_subdirectory(MWComLibCPP_library)
add_subdirectory(PCAB_Debugger_library)
add_subdirectory(MWComLibRaspico_library/flash_library)
add_subdirectory(MWComLibRaspico_library/uart_library)
add_subdirectory(MWComLibRaspico_library/spi_library)
add_subdirectory(MWComLibRaspico_library/adc_library)
add_subdirectory(MWComLibRaspico_library/ds18b20_library)
add_subdirectory(MWComLibRaspico_library/onewire_library)

add_executable(${ProjectName}
    ${ProjectName}.cpp
)

# Add pico_stdlib library which aggregates commonly used features
target_link_libraries(${ProjectName}
    pico_stdlib
    pico_multicore
    hardware_flash
    hardware_sync
    hardware_gpio
    hardware_spi
    hardware_pio
    hardware_adc
    MWComLibCPP_library
    flash_library
    uart_library
    spi_library
    adc_library
    ds18b20_library
    onewire_library
    PCAB_Debugger_library
)

pico_add_extra_outputs(${ProjectName})
