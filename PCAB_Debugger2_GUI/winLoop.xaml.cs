using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static PCAB_Debugger2_GUI.agPNA835x;
using static PCAB_Debugger2_GUI.PCAB_TASK;

namespace PCAB_Debugger2_GUI
{
    /// <summary>
    /// winLoop.xaml の相互作用ロジック
    /// </summary>
    public partial class winLoop : Window
    {
        private bool runTASK = true;
        private string dirPath;
        private string fileHeader;
        private bool singTRIG = false;
        private bool saveSCR = false;
        private bool saveTRA = false;
        private List<uint> dps = new List<uint>();
        private List<uint> dsa = new List<uint>();
        private List<uint> channels = new List<uint>();
        private List<uint> sheets = new List<uint>();
        private List<loopCONF> loops = new List<loopCONF>();
        private List<SweepMode> trig = new List<SweepMode>();
        private PCAB_SerialInterface.PCAB_UnitInterface serialNum;
        private PCAB_TASK _serial;
        private agPNA835x instr;
        private AutoMeasure owner;

        public winLoop(AutoMeasure WinOwner, PCAB_TASK serial, PCAB_SerialInterface.PCAB_UnitInterface SN, string DirPATH)
        {
            InitializeComponent();
            _serial = serial;
            serialNum = SN;
            dirPath = DirPATH;
            owner = WinOwner;
            _serial.OnTaskError += OnError;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool fileFLG = false;
            uint stepDPS = 0;
            uint stepDSA = 0;
            int waitTIME = -1;
            //Get Configuration
            fileHeader = owner.FileNameHeader;
            dps.Clear();
            dsa.Clear();
            waitTIME = (int)owner.WaiteTime;
            if (owner.DPS_Enable == true)
            {
                dps = owner.DPS;
                if (dps.Count > 0) { stepDPS = owner.DPS_Step; }
            }
            if (owner.DSA_Enable == true)
            {
                dsa = owner.DSA;
                if (dsa.Count > 0) { stepDSA = owner.DSA_Step; }
            }
            loops.Clear();
            loopCONF loopCONFBF = new loopCONF();
            if (stepDPS > 0 && stepDSA > 0)
            {
                for (int cntDPS = 0; cntDPS < 64; cntDPS += (int)stepDPS)
                {
                    for (int cntDSA = 0; cntDSA < 64; cntDSA += (int)stepDSA)
                    {
                        loopCONFBF.dps = cntDPS;
                        loopCONFBF.dsa = cntDSA;
                        loops.Add(loopCONFBF);
                    }
                }
            }
            else if (stepDPS > 0)
            {
                for (int cntDPS = 0; cntDPS < 64; cntDPS += (int)stepDPS)
                {
                    loopCONFBF.dps = cntDPS;
                    loopCONFBF.dsa = -1;
                    loops.Add(loopCONFBF);
                }
            }
            else if (stepDSA > 0)
            {
                for (int cntDSA = 0; cntDSA < 64; cntDSA += (int)stepDSA)
                {
                    loopCONFBF.dps = -1;
                    loopCONFBF.dsa = cntDSA;
                    loops.Add(loopCONFBF);
                }
            }
            else
            {
                loopCONFBF.dps = -1;
                loopCONFBF.dsa = -1;
                loops.Add(loopCONFBF);
            }
            if (owner.GetScreen_Enable == true ||
                owner.GetTrace_Enable == true)
            {
                if (owner.GetScreen_Enable == true) { saveSCR = true; }
                if (owner.GetTrace_Enable == true) { saveTRA = true; }
                instr = new agPNA835x(new IEEE488dot2.Instrument(new VisaResource(), owner.VISA_Address));
                //Get instrument configure
                try
                {
                    instr.Open();
                    instr.Instrument.SetTimeout((int)owner.VISA_Timeout);
                    IEEE488dot2.IEEE488_IDN idn = instr.Instrument.IDN();
                    channels.Clear();
                    if (owner.Channel == -1)
                    {
                        foreach (uint i in instr.getChannelCatalog())
                        {
                            channels.Add(i);
                        }
                    }
                    else
                    {
                        channels.Add((uint)owner.Channel);
                    }
                    trig.Clear();
                    if (owner.SingleTrigger == true)
                    {
                        singTRIG = true;
                        foreach (uint ch in channels)
                        {
                            trig.Add(instr.getTriggerMode(ch));
                        }
                    }
                    sheets.Clear();
                    foreach (uint i in instr.getSheetsCatalog())
                    {
                        sheets.Add(i);
                    }
                    instr.Close();
                }
                catch
                {
                    MessageBox.Show("GPIB Connection Error.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    ExitErrTASK();
                    return;
                }
                //check file path
                if (owner.GetScreen_Enable == true)
                {
                    foreach (uint sh in sheets)
                    {
                        foreach (loopCONF cnf in loops)
                        {
                            string filePath = dirPath + "\\" + fileHeader;
                            if (cnf.dps >= 0) { filePath += "_DPS" + cnf.dps.ToString("00"); }
                            if (cnf.dsa >= 0) { filePath += "_DSA" + cnf.dsa.ToString("00"); }
                            filePath += "_Sheet" + sh.ToString() + ".png";
                            if (System.IO.File.Exists(filePath)) { fileFLG = true; break; }
                        }
                        if (fileFLG) { break; }
                    }
                }
                if (owner.GetTrace_Enable == true && fileFLG == false)
                {
                    foreach (uint sh in sheets)
                    {
                        foreach (loopCONF cnf in loops)
                        {
                            string filePath = dirPath + "\\" + fileHeader;
                            if (cnf.dps >= 0) { filePath += "_DPS" + cnf.dps.ToString("00"); }
                            if (cnf.dsa >= 0) { filePath += "_DSA" + cnf.dsa.ToString("00"); }
                            filePath += "_Sheet" + sh.ToString() + ".csv";
                            if (System.IO.File.Exists(filePath)) { fileFLG = true; break; }
                        }
                        if (fileFLG) { break; }
                    }
                }
                if (fileFLG)
                {
                    if (MessageBox.Show("The file exists in the specified folder.\nDo you want to overwrite?",
                        "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel) { runTASK = false; }
                }

            }
            if (owner.GetScreen_Enable == true || owner.GetTrace_Enable == true) { try { instr.Open(); } catch { ExitErrTASK(); }            }
            Task task = Task.Factory.StartNew(() => { LOOP_Task(waitTIME); });
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            runTASK = false;
        }

        private void LOOP_Task(int waiteTime)
        {
            int loopCNT = 0;
            if (waiteTime < 0) { ExitErrTASK(); return; }
            try
            {
                OnUpdateDAT(loopCNT);
                uint[] dpsNOW = _serial.PCAB_GetDPS(serialNum).ToArray();
                uint[] dsaNOW = _serial.PCAB_GetDSA(serialNum).ToArray();
                if (runTASK)
                {
                    foreach (loopCONF cnf in loops)
                    {
                        string filePath = dirPath + "\\" + fileHeader;
                        if (!runTASK) { ExitCancelTASK(); return; }
                        if (cnf.dps >= 0)
                        {
                            foreach (int p in dps)
                            {
                                dpsNOW[p - 1] = (uint)cnf.dps;
                            }
                            if (!_serial.PCAB_WriteDPS(serialNum, dpsNOW.ToList())) { ExitErrTASK(); return; }
                            filePath += "_DPS" + cnf.dps.ToString("00");
                        }
                        if (!runTASK) { ExitCancelTASK(); return; }
                        if (cnf.dsa >= 0)
                        {
                            foreach (int p in dsa)
                            {
                                dsaNOW[p - 1] = (uint)cnf.dsa;
                            }
                            if (!_serial.PCAB_WriteDSA(serialNum, dsaNOW.ToList())) { ExitErrTASK(); return; }
                            filePath += "_DSA" + cnf.dsa.ToString("00");
                        }
                        if (!runTASK) { ExitCancelTASK(); return; }
                        Thread.Sleep(waiteTime);
                        if (!runTASK) { ExitCancelTASK(); return; }
                        //Single Trigger Set
                        if (singTRIG)
                        {
                            foreach (uint ch in channels)
                            {
                                instr.trigSingle(ch);
                            }
                        }
                        Thread.Sleep(100);
                        if (!runTASK)
                        {
                            //Trigger ReSET
                            if (singTRIG)
                            {
                                for (int i = 0; i < channels.Count; i++)
                                {
                                    instr.setTriggerMode(channels[i], trig[i]);
                                }
                            }
                            ExitCancelTASK();
                            return;
                        }

                        if (!string.IsNullOrWhiteSpace(dirPath))
                        {
                            //Save Screen
                            if (saveSCR)
                            {
                                foreach (uint sh in sheets)
                                {
                                    instr.selectSheet(sh);
                                    if (!instr.GetScreen(filePath + "_Sheet" + sh.ToString() + ".png", out _))
                                    {
                                        if (singTRIG)
                                        {
                                            for (int i = 0; i < channels.Count; i++)
                                            {
                                                instr.setTriggerMode(channels[i], trig[i]);
                                            }
                                        }
                                        ExitErrTASK();
                                        return;
                                    }
                                }
                            }

                            if (!runTASK)
                            {
                                //Trigger ReSET
                                if (singTRIG)
                                {
                                    for (int i = 0; i < channels.Count; i++)
                                    {
                                        instr.setTriggerMode(channels[i], trig[i]);
                                    }
                                }
                                ExitCancelTASK();
                                return;
                            }

                            //Save Trace
                            if (saveTRA)
                            {
                                try
                                {
                                    foreach (uint sh in sheets)
                                    {
                                        //Select Sheet
                                        instr.selectSheet(sh);
                                        //Get Trace DAT
                                        List<ChartDAT> dat = new List<ChartDAT>();
                                        foreach (uint win in instr.getWindowCatalog(sh))
                                        {
                                            List<TraceDAT> trace = new List<TraceDAT>();
                                            foreach (uint tra in instr.getTraceCatalog(win))
                                            {
                                                instr.selectTrace(win, tra);
                                                uint ch = instr.getSelectChannel();
                                                uint num = instr.getSelectMeasurementNumber();
                                                //string x = pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":X:AXIS:UNIT?");
                                                string x = "X";
                                                string y = instr.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":PAR?");
                                                y += "_" + instr.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":FORM?");
                                                //y += "_" + pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":X:AXIS:UNIT?");
                                                string mem = instr.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":MATH:FUNC?");
                                                if (mem.ToUpper() != "NORM")
                                                {
                                                    y += "@" + mem + "[MEM]";
                                                }
                                                string[] valx = instr.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":X?").Trim().Split(',');
                                                //string[] valy = pna.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":Y?").Trim().Split(',');
                                                string[] valy = instr.getASCII("CALC" + ch.ToString() + ":MEAS" + num.ToString() + ":DATA:FDAT?").Trim().Split(',');
                                                trace.Add(new TraceDAT("CH" + ch.ToString(), x, y, valx, valy));
                                            }
                                            dat.Add(new ChartDAT("Win" + win.ToString(), trace.ToArray()));
                                        }
                                        //Write CSV Data
                                        using (StreamWriter sw = new StreamWriter(filePath + "_Sheet" + sh.ToString() + ".csv", false, Encoding.UTF8))
                                        {
                                            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);

                                            sw.WriteLine("\"PCAB Debugger Ver," + fvi.ProductVersion + "\"");
                                            sw.WriteLine(fvi.LegalCopyright);
                                            sw.WriteLine();
                                            string strBF1 = "";
                                            string strBF2 = "";
                                            string strBF3 = "";
                                            int cnt = 0;
                                            foreach (ChartDAT chart in dat)
                                            {
                                                strBF1 += chart.WindowNumber + ",,";
                                                for (int i = 0; i < chart.Trace.Length - 1; i++)
                                                {
                                                    strBF1 += "," + ",";
                                                }
                                                foreach (TraceDAT trace in chart.Trace)
                                                {
                                                    strBF2 += trace.ChannelNumber + ",,";
                                                    strBF3 += trace.AxisX + "," + trace.AxisY + ",";
                                                    if (cnt < trace.ValueX.Length) { cnt = trace.ValueX.Length; }
                                                    if (cnt < trace.ValueY.Length) { cnt = trace.ValueY.Length; }
                                                }
                                            }
                                            sw.WriteLine(strBF1.Trim(','));
                                            sw.WriteLine(strBF2.Trim(','));
                                            sw.WriteLine(strBF3.Trim(','));

                                            for (int i = 0; i < cnt; i++)
                                            {
                                                strBF1 = "";
                                                foreach (ChartDAT chart in dat)
                                                {
                                                    foreach (TraceDAT trace in chart.Trace)
                                                    {
                                                        if (trace.ValueX.Length > i) { strBF1 += trace.ValueX[i]; }
                                                        strBF1 += ",";
                                                        if (trace.ValueY.Length > i) { strBF1 += trace.ValueY[i]; }
                                                        strBF1 += ",";
                                                    }
                                                }
                                                sw.WriteLine(strBF1.Trim(','));
                                            }
                                            sw.Close();
                                        }
                                    }
                                }
                                catch
                                {
                                    if (singTRIG)
                                    {
                                        for (int i = 0; i < channels.Count; i++)
                                        {
                                            instr.setTriggerMode(channels[i], trig[i]);
                                        }
                                    }
                                    ExitErrTASK();
                                    return;
                                }
                            }

                            if (!runTASK)
                            {
                                //Trigger ReSET
                                if (singTRIG)
                                {
                                    for (int i = 0; i < channels.Count; i++)
                                    {
                                        instr.setTriggerMode(channels[i], trig[i]);
                                    }
                                }
                                ExitCancelTASK();
                                return;
                            }
                        }

                        //Trigger ReSET
                        if (singTRIG)
                        {
                            for (int i = 0; i < channels.Count; i++)
                            {
                                instr.setTriggerMode(channels[i], trig[i]);
                            }
                        }
                        if (!runTASK) { ExitCancelTASK(); return; }
                        OnUpdateDAT(loopCNT);
                        loopCNT++;
                    }
                    ExitNormTASK();
                }
                else { ExitCancelTASK(); return; }
            }
            catch { ExitErrTASK(); }
        }

        private void OnUpdateDAT(int cnt)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (loops[cnt].dps < 0) { dps_state.Content = "LOCK"; }
                else { dps_state.Content = (5.625 * loops[cnt].dps).ToString("0.000").PadLeft(7, ' ') + "deg (" + loops[cnt].dps.ToString("00") + ")"; }
                if (loops[cnt].dsa < 0) { dsa_state.Content = "LOCK"; }
                else { dsa_state.Content = (0.25 * loops[cnt].dsa).ToString("0.00").PadLeft(6, ' ') + " dB (" + loops[cnt].dsa.ToString("00") + ")"; }
                Progress.Value = (int)((double)cnt / (loops.Count - 1) * 100.0);
            }));
        }

        private void ExitNormTASK()
        {
            instr?.Close();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageBox.Show("Loop function is done.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }));
        }

        private void ExitErrTASK()
        {
            instr?.Close();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageBox.Show("Loop function.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
                this.Close();
            }));
        }

        private void ExitCancelTASK()
        {
            instr?.Close();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageBox.Show("Loop function is cancel.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }));
        }

        private void Progress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageLabel.Content = "Sweep... " + Progress.Value.ToString() + "%";
            }));
        }

        private void OnError(object sender, PCABEventArgs e)
        {
            instr?.Close();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.DialogResult = false;
                this.Close();
            }));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _serial.OnTaskError -= OnError;
        }

        #region Structure
        private struct ChartDAT
        {
            public string WindowNumber { get; set; }
            public TraceDAT[] Trace { get; set; }

            public ChartDAT(string winNum, TraceDAT[] trace) { WindowNumber = winNum; Trace = trace; }
        }

        private struct TraceDAT
        {
            public string ChannelNumber { get; set; }
            public string AxisX { get; set; }
            public string AxisY { get; set; }
            public string[] ValueX { get; set; }
            public string[] ValueY { get; set; }
            public TraceDAT(string ch, string x, string y, string[] val_x, string[] val_y)
            {
                ChannelNumber = ch; AxisX = x; AxisY = y; ValueX = val_x; ValueY = val_y;
            }
        }

        private struct loopCONF
        {
            public int dps { get; set; }
            public int dsa { get; set; }
        }
        #endregion

    }
}