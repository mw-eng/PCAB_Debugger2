using System.Collections.Generic;
using System.IO;

namespace PCAB_Debugger2_GUI
{
    /// <summary>AgilentPNA835x Wrapper Library</summary>
    public class agPNA835x
    {
        private IEEE488dot2.Instrument instr;

        #region ENUM
        /// <summary>Sweep Mode enum</summary>
        public enum SweepMode
        {
            /// <summary>Hold</summary>
            HOLD = 1,
            /// <summary>Continuous</summary>
            CONT = 2,
            /// <summary>Groups(SENS:SWE:GRO:COUN)</summary>
            GRO = 3,
            /// <summary>Single</summary>
            SING = 4
        }
        #endregion

        /// <summary>コンストラクタ</summary>
        public agPNA835x(IEEE488dot2.Instrument Instrument) { instr = Instrument; }

        public IEEE488dot2.Instrument Instrument { get { return instr; } }

        /// <summary>VISA32 Open</summary>
        public void Open() { instr.Open(); }

        /// <summary>VISA32 Close</summary>
        public void Close() { instr.Close(); }

        /// <summary>Get Trigger Mode</summary>
        /// <param name="ch">Channel</param>
        /// <returns>Trigger Mode</returns>
        public SweepMode getTriggerMode(uint ch)
        {
            string strBF = getASCII("SENS" + ch.ToString() + ":SWE:MODE?");
            switch (strBF.ToUpper())
            {
                case "HOLD":
                    return SweepMode.HOLD;
                case "CONT":
                    return SweepMode.CONT;
                case "GRO":
                    return SweepMode.GRO;
                case "SING":
                    return SweepMode.SING;
                default:
                    return SweepMode.CONT;
            }
        }

        /// <summary>Set Trigger Mode</summary>
        /// <param name="ch">Channel</param>
        /// <param name="trig">Trigger Mode</param>
        public void setTriggerMode(uint ch, SweepMode trig)
        {
            setSCPIcommand("SENS" + ch.ToString() + ":SWE:MODE " + trig.ToString());
            string strBF = getASCII("*OPC?");
        }

        /// <summary>Set Single Trigger</summary>
        /// <param name="ch"></param>
        public void trigSingle(uint ch) { setTriggerMode(ch, SweepMode.SING); }
        /// <summary>Set HOLD Trigger</summary>
        /// <param name="ch"></param>
        public void trigHold(uint ch) { setTriggerMode(ch, SweepMode.HOLD); }
        /// <summary>Set Trigger Continuous</summary>
        /// <param name="ch"></param>
        public void trigContinuous(uint ch) { setTriggerMode(ch, SweepMode.CONT); }

        /// <summary>Select Sheet</summary>
        /// <param name="sheetID">Sheet ID</param>
        public void selectSheet(uint sheetID)
        {
            uint win = getWindowCatalog(sheetID)[0];
            uint tra = getTraceCatalog()[0];
            selectTrace(win, tra);
        }

        /// <summary>Get Trace Catalog</summary>
        /// <returns>Trace Catalog</returns>
        public uint[] getTraceCatalog() { return getTraceCatalog(0); }

        /// <summary>Get Trace Catalog</summary>
        /// <param name="WindowID">Window ID</param>
        /// <returns>Trace Catalog</returns>
        public uint[] getTraceCatalog(uint WindowID)
        {
            List<uint> trace = new List<uint>();
            string[] arrBF;
            if (WindowID <= 0) { arrBF = getASCII("DISP:WIND:CAT?").Split(','); }
            else { arrBF = getASCII("DISP:WIND" + WindowID.ToString() + ":CAT?").Split(','); }
            foreach (string strBf in arrBF) { trace.Add(uint.Parse(strBf)); }
            return trace.ToArray();
        }

        /// <summary>Get Window Catalog</summary>
        /// <returns>Window List</returns>
        public uint[] getWindowCatalog() { return getWindowCatalog(0); }

        /// <summary>Get Window Catalog</summary>
        /// <param name="sheetID">Sheet ID</param>
        /// <returns>Window List</returns>
        public uint[] getWindowCatalog(uint sheetID)
        {
            List<uint> win = new List<uint>();
            string[] arrBF;
            if (sheetID <= 0) { arrBF = getASCII("DISP:SHE:CAT?").Split(','); }
            else { arrBF = getASCII("DISP:SHE" + sheetID.ToString() + ":CAT?").Split(','); }
            foreach (string strBf in arrBF) { win.Add(uint.Parse(strBf)); }
            return win.ToArray();
        }

        /// <summary>Get Sheet Catalog</summary>
        /// <returns>sheet list</returns>
        public uint[] getSheetsCatalog()
        {
            List<uint> sh = new List<uint>();
            string[] arrBF = getASCII("SYST:SHE:CAT?").Split(',');
            foreach (string strBf in arrBF) { sh.Add(uint.Parse(strBf)); }
            return sh.ToArray();
        }

        /// <summary>Get Channel Catalog</summary>
        /// <returns>channel catalog</returns>
        public uint[] getChannelCatalog()
        {
            List<uint> ch = new List<uint>();
            string[] arrBF = getASCII("SYST:CHAN:CAT?").Split(',');
            foreach (string strBf in arrBF) { ch.Add(uint.Parse(strBf)); }
            return ch.ToArray();
        }

        /// <summary>Get Port Catalog</summary>
        /// <returns>Port Catalog</returns>
        public uint[] getPortCatalog()
        {
            List<uint> port = new List<uint>();
            string[] arrBF = getASCII("SOUR:CAT?").Split(',');
            foreach (string strBf in arrBF) { port.Add(uint.Parse(strBf.Replace("Port", ""))); }
            return port.ToArray();
        }

        public void selectTrace(uint winNum, uint traceNum)
        {
            setSCPIcommand("DISP:WIND" + winNum + ":TRAC" + traceNum + ":SEL");
        }

        public uint getSelectChannel()
        {
            return uint.Parse(getASCII("SYST:ACT:CHAN?"));
        }

        public uint getSelectMeasurementNumber()
        {
            return uint.Parse(getASCII("SYST:ACT:MEAS:NUMB?"));
        }

        public bool GetScreen(string filePATH, out string ErrorMessage)
        {
            string strFORM = getASCII("HCOP:SDUM:DATA:FORM?");
            setSCPIcommand("HCOP:SDUM:DATA:FORM PNG");
            setSCPIcommand("DISP:CCL");    //Display Error clear
            setSCPIcommand("*CLS");         //status register clear
            setSCPIcommand("HCOP:SDUM:DATA?");
            byte[] byIMG = instr.ReadBinaryIEEEblock();
            string strBF = getASCII("SYST:ERR?"); //Error check
            string[] strArr = strBF.Split(',');
            if (int.Parse(strArr[0]) != 0)
            {
                ErrorMessage = strArr[1];
                return false;
            }
            else
            {
                using (FileStream fs = new FileStream(filePATH, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byIMG, 0, byIMG.Length);
                }
                ErrorMessage = "SUCCESS";
                return true;
            }
        }

        public void setSCPIcommand(string cmd)
        {
            instr.WriteAsciiCmdLine(cmd);
        }

        public string getASCII() { return instr.ReadAsciiBlock().Trim('\n').Trim('\"'); }

        public string getASCII(string cmd) { setSCPIcommand(cmd); return getASCII(); }

    }
}
