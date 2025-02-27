#pragma once
#include <string>
#include <cstring>
#include <vector>
#include <iostream>
#include <algorithm>

class Convert
{
    public:
    static std::string ToString(const bool &val, const bool &formatString);
    static std::string ToString(const bool &val);
    static std::string ToString(const uint64_t &val, const uint &BaseNumber, const uint &digit);
    static std::string ToString(const uint64_t &val, const uint &BaseNumber);
    static std::string ToString(const uint64_t &val);

    static bool TryToBool(const std::string &str, bool &out);
    static bool TryToInt(const std::string &str, int &out);
    static bool TryToDouble(const std::string &str, double &out);
    static bool TryToFloat(const std::string &str, float &out);
    static bool TryToUInt(const std::string &str, const uint8_t &BaseNumber, uint &out);
    static bool TryToUInt2(const std::string &str, const uint8_t &BaseNumber, uint8_t &out);
    static bool TryToUInt4(const std::string &str, const uint8_t &BaseNumber, uint8_t &out);
    static bool TryToUInt8(const std::string &str, const uint8_t &BaseNumber, uint8_t &out);
    static bool TryToUInt16(const std::string &str, const uint8_t &BaseNumber, uint16_t &out);
    static bool TryToUInt32(const std::string &str, const uint8_t &BaseNumber, uint32_t &out);
    static bool TryToUInt64(const std::string &str, const uint8_t &BaseNumber, uint64_t &out);
};

class String
{
    public:
    
    /// @brief Trimming the specified std::string from the beginning(Left).
    /// @param str std::string to be trimmed.
    /// @param targ Specified std::string.
    /// @return std::string after trimming.
    static std::string ltrim(const std::string &str, const std::string &targ);

    /// @brief Trimming the specified std::string from the end(Right).
    /// @param str std::string to be trimmed.
    /// @param targ Specified std::string.
    /// @return std::string after trimming.
    static std::string rtrim(const std::string &str, const std::string &targ);

    /// @brief Trimming the beginning(Left) and end(Right) of the specified std::string.
    /// @param str std::string to be trimmed.
    /// @param targ Specified std::string.
    /// @return std::string after trimming.
    static std::string trim(const std::string &str, const std::string &targ);

    /// @brief Trimming the character (Space, CR, LF, Tab, 0x0c, 0x0b) from the beginning(Left).
    /// @param str std::string to be trimmed.
    /// @return std::string after trimming
    static std::string ltrim(const std::string &str);

    /// @brief Trimming the character (Space, CR, LF, Tab, 0x0c, 0x0b) from the end(Right).
    /// @param str std::string to be trimmed.
    /// @return std::string after trimming.
    static std::string rtrim(const std::string &str);

    /// @brief Trimming the beginning(Left) and end(Right) of the character (Space, CR, LF, Tab, 0x0c, 0x0b).
    /// @param str std::string to be trimmed.
    /// @return std::string after trimming.
    static std::string trim(const std::string &str);

    /// @brief Shifts bit to the left by the specified bits.
    /// @param bit Traget bits.
    /// @param shift Number of shifts.
    /// @return Result bit.
    static uint8_t uint8_bitShiftLeft(const uint8_t &bit, const uint8_t &shift);

    /// @brief Compares two (A and B) std::strings.
    /// @param a Target std::string A.
    /// @param b Target std::string B.
    /// @param ignoreCase Case sensitivity. (true:Compare case insensitivitely, false:Compare case sensitivively)
    /// @return Compare result.
    static bool strCompare(const std::string &a, const std::string &b, const bool &ignoreCase);

    /// @brief Compares two (A and B) std::strings.
    /// @param a Target std::string A.
    /// @param b Target std::string B.
    /// @return Compare result.
    static bool strCompare(const std::string &a, const std::string &b);


    /// @brief Convert string to uint
    /// @param str convert string
    /// @return uint
    static int conv_uint(const std::string &str);

    /// @brief String split.
    /// @param str Target string.
    /// @param delim Split charactor
    /// @return vector
    static std::vector<std::string> split(const std::string &str, const char &delim);

};

class Math
{
    public:
    static uint64_t POW64(const uint64_t &x, const uint64_t &y, bool &out);
    static uint32_t POW32(const uint64_t &x, const uint64_t &y, bool &out);
    static uint16_t POW16(const uint64_t &x, const uint64_t &y, bool &out);
    static uint8_t POW8(const uint64_t &x, const uint64_t &y, bool &out);
    static uint POW(const uint64_t &x, const uint64_t &y, bool &out);
};
