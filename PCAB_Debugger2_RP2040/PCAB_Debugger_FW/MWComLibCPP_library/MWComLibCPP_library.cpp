#include "MWComLibCPP_library.hpp"

uint64_t pow64(const uint8_t &x, const uint &y)
{
    bool ret;
    return Math::POW64((uint64_t)x, (uint64_t)y, ret);
}
void eraseChar(std::string &str, const char &ch)
{
    std::string strBf = "";
    for(char c: str)
    {
        if(c != ch){strBf += c;}
    }
    str = strBf;
}
void eraseChar(std::string &str, const std::string &chars) { for(char c: chars) { eraseChar(str, c); } }
void eraseCharNum(std::string &str) { eraseChar(str, ", \n\r\t\f\v"); }
bool checkNUM(std::string &str)
{
    std::string strBf = str;
    bool flg = false;
    eraseCharNum(strBf);
    if(strBf == "") { return false; }
    if(strBf[0] == '.'){ flg = true; }
    for(uint i = 1; i < strBf.length() ; i++ )
    {
        if(!('0' <= strBf[i] && strBf[i] <= '9'))
        {
            if(strBf[i] == '.' && !flg) { flg = true; }
            else { return false; }
        }
    }
    if(strBf[0] == '-' || strBf[0] == '+' || strBf[0] == '.' || ('0' <= strBf[0] && strBf[0] <= '9')) { return true;}
    return false;
}

std::string Convert::ToString(const bool &val, const bool &formatString)
{
    if(formatString && val){ return "true"; }
    if(formatString && !val){ return "false"; }
    if(!formatString && val){ return "1"; }
    return "0";
}
std::string Convert::ToString(const bool &val) { return ToString(val, false); }
std::string Convert::ToString(const uint64_t &val, const uint &BaseNumber, const uint &digit)
{
    uint64_t ulngBF = val;
    std::string strBf = "";
    std::string str = "";
    while(ulngBF)
    {
        if(ulngBF % BaseNumber < 10) { strBf.push_back('0' + ulngBF % BaseNumber); }
        else { strBf.push_back('0' + ulngBF % BaseNumber + 7); }
        ulngBF /= BaseNumber;
    }
    if(strBf.length() < digit)
    {
        for(uint i = 0 ; i < digit - strBf.length() ; i++ ) { str.push_back('0'); }
    }
    for(int i = strBf.length() ; 0 < i ; i-- ){ str.push_back(strBf[i - 1]); }
    return str;
}
std::string Convert::ToString(const uint64_t &val, const uint &BaseNumber){ return ToString(val, BaseNumber, 0);}
std::string Convert::ToString(const uint64_t &val){ return ToString(val, 10, 0);}

bool Convert::TryToBool(const std::string &str, bool &out)
{
    if(String::strCompare(String::trim(str), "true", true)){ out = true; return true; }
    if(String::strCompare(String::trim(str), "false", true)){ out = false; return true; }
    if(String::strCompare(String::trim(str), "1", true)){ out = true; return true; }
    if(String::strCompare(String::trim(str), "0", true)){ out = false; return true; }
    return false;
}
bool Convert::TryToInt(const std::string &str, int &out)
{
    out = 0;
    std::string strBf = str;
    eraseCharNum(strBf);
    if(strBf == "") { return false; }
    if(strBf.length() > 11) { return false; }
    int sign = 1;
    for(uint i = 1; i < strBf.length() ; i++ )
    {
        if(!('0' <= strBf[i] && strBf[i] <= '9')) { return false; }
    }
    if(strBf[0] == '-') { sign = -1; eraseChar(strBf, '-'); }
    else if(strBf[0] == '+') { sign = 1; eraseChar(strBf, '+'); }
    else if(!('0' <= strBf[0] && strBf[0] <= '9')) { return false; }
    //digit check
    int64_t in64 = sign * std::stoll(strBf);
    if(INT32_MIN <= in64 && in64 <= INT32_MAX ) { out = in64; return true; }
    else { return false; }
}
bool Convert::TryToDouble(const std::string &str, double &out)
{
    std::string strBf = str;
    eraseCharNum(strBf);
    if(!checkNUM(strBf)) { return false; }
    if(strBf[0] == '+') { eraseChar(strBf, '+'); }
    out = std::stod(strBf);
    return true;
}
bool Convert::TryToFloat(const std::string &str, float &out)
{
    std::string strBf = str;
    eraseCharNum(strBf);
    if(!checkNUM(strBf)) { return false; }
    if(strBf[0] == '+') { eraseChar(strBf, '+'); }
    out = std::stof(strBf);
    return true;
}

bool Convert::TryToUInt2(const std::string &str, const uint8_t &BaseNumber, uint8_t &out)
{
    uint64_t bf;
    if(TryToUInt64(str, BaseNumber, bf) && bf <= 0x3u)
    {
        out = bf;
        return true;
    }
    else { return false; }
}
bool Convert::TryToUInt4(const std::string &str, const uint8_t &BaseNumber, uint8_t &out)
{
    uint64_t bf;
    if(TryToUInt64(str, BaseNumber, bf) && bf <= 0xFu)
    {
        out = bf;
        return true;
    }
    else { return false; }
}
bool Convert::TryToUInt8(const std::string &str, const uint8_t &BaseNumber, uint8_t &out)
{
    uint64_t bf;
    if(TryToUInt64(str, BaseNumber, bf) && bf <= UINT8_MAX)
    {
        out = bf;
        return true;
    }
    else { return false; }
}
bool Convert::TryToUInt16(const std::string &str, const uint8_t &BaseNumber, uint16_t &out)
{
    uint64_t bf;
    if(TryToUInt64(str, BaseNumber, bf) && bf <= UINT16_MAX)
    {
        out = bf;
        return true;
    }
    else { return false; }
}
bool Convert::TryToUInt32(const std::string &str, const uint8_t &BaseNumber, uint32_t &out)
{
    uint64_t bf;
    if(TryToUInt64(str, BaseNumber, bf) && bf <= UINT32_MAX)
    {
        out = bf;
        return true;
    }
    else { return false; }
}
bool Convert::TryToUInt64(const std::string &str, const uint8_t &BaseNumber, uint64_t &out)
{
    out = 0;
    if(BaseNumber > 36){ return false; }
    for(uint i = 0 ; i < str.length() ; i++ )
    {
        uint8_t uiBF = str[i];
        if(48 <= uiBF && uiBF <= 57)
        {
            if(uiBF - 48 > BaseNumber) { out = 0; return false; }
            out += (uiBF - 48) * pow64(BaseNumber, str.length() - i - 1);
        }
        else if(65 <= uiBF && uiBF <= 90)
        {
            if(uiBF - 55 > BaseNumber) { out = 0; return false; }
            out += (uiBF - 55) * pow64(BaseNumber, str.length() - i - 1);
        }
        else if(97 <= uiBF && uiBF <= 122) {
            if(uiBF - 87 > BaseNumber) { out = 0; return false; }
            out += (uiBF - 87) * pow64(BaseNumber, str.length() - i - 1);    
        }
        else if(uiBF != 44) { return false; }
    }
    return true;
}

std::string String::ltrim(const std::string &str, const std::string &targ)
{
    size_t start = str.find_first_not_of(targ);
    return (start == std::string::npos) ? "" : str.substr(start);
}

std::string String::rtrim(const std::string  &str, const std::string &targ)
{
    size_t end = str.find_last_not_of(targ);
    return (end == std::string::npos) ? "" : str.substr(0, end + 1);
}
std::string String::trim(const std::string  &str, const std::string &targ) { return rtrim(ltrim(str, targ), targ); }
std::string String::ltrim(const std::string &str) { return ltrim(str, " \n\r\t\f\v"); }
std::string String::rtrim(const std::string &str) { return rtrim(str, " \n\r\t\f\v"); }
std::string String::trim(const std::string &str) { return trim(str, " \n\r\t\f\v"); }
uint8_t String::uint8_bitShiftLeft(const uint8_t &bit, const uint8_t &shift)
{
    unsigned char chBF = bit;
    return (uint8_t)(chBF << shift);
}
bool String::strCompare(const std::string &a, const std::string &b, const bool &ignoreCase)
{
    if(a.length() <= 0 && b.length() <= 0){return true;}
    if (ignoreCase)
    {
        return strcasecmp(a.c_str(), b.c_str()) == 0 ? true : false;
    }
    else{ return a == b ? true : false; }
}
bool String::strCompare(const std::string &a, const std::string &b) { return strCompare(a, b, false); }
int String::conv_uint(const std::string &str)
{
    if(str.length() <= 0){return -1;}
    if (std::all_of(str.cbegin(), str.cend(), isdigit)) { return stoi(str); }
    return -1;
}
std::vector<std::string> String::split(const std::string &str, const char &delim) {
    std::vector<std::string> elems;
    std::string item;
    for (char ch: str) {
        if (ch == delim) {
            if (!item.empty())
                elems.push_back(item);
            item.clear();
        }
        else {
            item += ch;
        }
    }
    if (!item.empty())
        elems.push_back(item);
    return elems;
}

uint64_t Math::POW64(const uint64_t &x, const uint64_t &y, bool &out)
{
    uint64_t ret = 1;
    for(uint64_t i = 0 ; i < y ; i++ )
    {
        if(ret < UINT64_MAX / x) { ret *= x; }
        else { out = false; ret = UINT64_MAX; }
    }
    out = true;
    return ret;
}
uint32_t Math::POW32(const uint64_t &x, const uint64_t &y, bool &out)
{uint32_t ret = Math::POW64(x, y, out); if(ret > UINT32_MAX) { out = false; return UINT32_MAX; } else { return ret; } }
uint16_t Math::POW16(const uint64_t &x, const uint64_t &y, bool &out)
{uint32_t ret = Math::POW64(x, y, out); if(ret > UINT16_MAX) { out = false; return UINT16_MAX; } else { return ret; } }
uint8_t Math::POW8(const uint64_t &x, const uint64_t &y, bool &out)
{uint32_t ret = Math::POW64(x, y, out); if(ret > UINT8_MAX) { out = false; return UINT8_MAX; } else { return ret; } }
uint Math::POW(const uint64_t &x, const uint64_t &y, bool &out)
{ return Math::POW32(x, y, out); }
