using System.Collections.Generic;
using System.Linq;

namespace PCAB_Debugger2_GUI
{
    public class SLIP
    {
        public static List<byte> EncodeSLIP(List<byte> data)
        {
            List<byte> result = new List<byte>();
            result.Clear();
            foreach (byte d in data)
            {
                switch (d)
                {
                    case 0xC0:
                        result.Add(0xDB);
                        result.Add(0XDC);
                        break;
                    case 0xDB:
                        result.Add(0xDB);
                        result.Add(0xDD);
                        break;
                    default:
                        result.Add(d);
                        break;
                }
            }
            result.Add(0xC0);
            return result;
        }

        public static List<byte> DecodeSLIP(List<byte> data)
        {
            List<byte> result = new List<byte>();
            result.Clear();
            for (int cnt = 0; cnt < data.Count(); cnt++)
            {
                switch (data[cnt])
                {
                    case 0xC0:
                        return result;
                    case 0xDB:
                        if (data[cnt + 1] == 0xDC) { result.Add(0xC0); }
                        else if (data[cnt + 1] == 0xDD) { result.Add(0xDB); }
                        cnt++;
                        break;
                    default:
                        result.Add(data[cnt]);
                        break;
                }
            }
            return result;
        }
    }
}
