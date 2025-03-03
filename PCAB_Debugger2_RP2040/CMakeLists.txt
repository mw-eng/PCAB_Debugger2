cmake_minimum_required(VERSION 3.12)

# Pull in SDK (must be before project)
include(pico_sdk_import.cmake)
include(pico_extras_import_optional.cmake)

set(ProjectName "PCAB_Debugger_RP2040")

project(${ProjectName} C CXX ASM)
add_compile_options(-Wall -g3 -O2)
set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 17)
set (CMAKE_BUILD_TYPE "Debug")
#set (PICO_DEOPTIMIZED_DEBUG "on")
#set (CMAKE_BUILD_TYPE "Release")
set (PICO_DEOPTIMIZED_DEBUG "off")

#add_compile_definitions(PICO_FLASH_SIZE_BYTES=2*1024*1024) #16Mbit (Raspberry Pi Pico)
#add_compile_definitions(PICO_FLASH_SIZE_BYTES=4*1024*1024) #32Mbit
#add_compile_definitions(PICO_FLASH_SIZE_BYTES=8*1024*1024) #64Mbit
add_compile_definitions(PICO_FLASH_SIZE_BYTES=16*1024*1024) #128Mbit (PCAB)
#add_compile_definitions(PICO_FLASH_SIZE_BYTES=32*1024*1024) #256Mbit
#add_compile_definitions(PICO_FLASH_SIZE_BYTES=64*1024*1024) #512Mbit
#add_compile_definitions(PICO_FLASH_SIZE_BYTES=128*1024*1024) #1024Mbit

if (PICO_SDK_VERSION_STRING VERSION_LESS "1.3.0")
    message(FATAL_ERROR "Raspberry Pi Pico SDK version 1.3.0 (or later) required. Your version is ${PICO_SDK_VERSION_STRING}")
endif()

# Initialize the SDK
pico_sdk_init()

add_compile_options(-Wall
        -Wno-format          # int != int32_t as far as the compiler is concerned because gcc has int32_t as long int
        -Wno-unused-function # we have some for the docs that aren't called
        )
if (CMAKE_C_COMPILER_ID STREQUAL "GNU")
    add_compile_options(-Wno-maybe-uninitialized)
endif()

# Hardware-specific examples in subdirectories:
add_subdirectory(PCAB_Debugger_FW)