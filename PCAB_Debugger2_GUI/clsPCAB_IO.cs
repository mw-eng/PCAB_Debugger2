using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static PCAB_Debugger2_GUI.PCAB_SerialInterface;

namespace PCAB_Debugger2_GUI
{
    public class PCAB_TASK
    {
        private PCAB_SerialInterface serialInterface;
        public bool? isOpen { get { return serialInterface?.isOpen; } }
        private bool? _task;    //true:run / false:stop / null:Interrupt
        private bool _state;
        public event EventHandler<PCABEventArgs> OnUpdateDAT;
        public event EventHandler<PCABEventArgs> OnTaskError;
        public List<PCAB_UnitInterface> UNITs { get { return serialInterface?.pcabUNITs; } }
        private Task _loopTask;

        public PCAB_TASK(string PortName, UInt32 BaudRate) { serialInterface = new PCAB_SerialInterface(PortName, BaudRate); }
        public PCAB_TASK(SerialPort serialPort) { serialInterface = new PCAB_SerialInterface(serialPort); }

        public void Close()
        {
            if (_task != false) { PCAB_AutoTaskStop(true); }
            try { serialInterface?.Close(); } catch { }
            serialInterface = null;
        }

        public List<bool> PCAB_AutoTaskStart(UInt32 waiteTime, List<string> serialNum)
        {
            if (serialInterface.isOpen) { serialInterface.Close(); }
            List<bool> ret = serialInterface.Open(serialNum);
            bool result = false;
            foreach(bool re in ret) { if (re) { result = true; break; } }
            if (result)
            {
                _task = true;
                _state = false;
                _loopTask = Task.Factory.StartNew(() => { PCAB_Task(waiteTime); });
            }
            return ret;
        }

        private void PCAB_Task(UInt32 waiteTime)
        {
            try
            {
                if(_task == true)
                {
                    while (_task == null) { Thread.Sleep(10); }
                    _state = true;
                    try
                    {
                        foreach (PCAB_UnitInterface unit in serialInterface.pcabUNITs)
                        {
                            serialInterface.DiscardInBuffer();
                            TempratureID ids = serialInterface.GetTempID(unit);
                            unit.SensorValuesNOW = new SensorValues(ids.IDs.ToList());
                        }
                    }
                    catch { }
                    _state = false;
                }
                do
                {
                    Thread.Sleep((int)waiteTime);
                    if (_task == true)
                    {
                        while (_task == null) { Thread.Sleep(10); }
                        _state = true;
                        bool updateFLG = false;
                        foreach (PCAB_UnitInterface unit in serialInterface.pcabUNITs)
                        {
                            serialInterface.DiscardInBuffer();
                            try { SensorValues values = serialInterface.GetSensorValue(unit); 
                            if (
                                values.Analog.Vd != unit.SensorValuesNOW.Analog.Vd ||
                                values.Analog.Id != unit.SensorValuesNOW.Analog.Id ||
                                values.Analog.Vin != unit.SensorValuesNOW.Analog.Vin ||
                                values.Analog.Pin != unit.SensorValuesNOW.Analog.Pin ||
                                values.Analog.CPU_Temprature != unit.SensorValuesNOW.Analog.CPU_Temprature ||
                                values.Temprature.Values != unit.SensorValuesNOW.Temprature.Values)
                            { updateFLG = true; unit.SensorValuesNOW = new SensorValues(values.Analog,values.Temprature, unit.SensorValuesNOW.ID); }
                            }
                            catch (Exception e) {OnTaskError?.Invoke(this, new PCABEventArgs(null, e.Message));  }
                        }
                        if (updateFLG)
                        {
                            OnUpdateDAT?.Invoke(this, new PCABEventArgs(serialInterface.pcabUNITs, null));
                        }
                        _state = false;
                    }
                } while (_task != false);
                serialInterface?.Close();
            }
            catch (Exception e)
            {
                foreach (PCAB_UnitInterface unit in serialInterface.pcabUNITs)
                {
                    unit.SensorValuesNOW.Clear();
                }
                OnTaskError?.Invoke(this, new PCABEventArgs(null, e.Message));
            }
        }

        public void PCAB_AutoTaskStop(bool wait)
        {
            _task = false;
            if (wait)
            {
                _loopTask?.ConfigureAwait(false);
                _loopTask?.Wait();
            }
        }

        public void PCAB_TaskPause()
        {
            if (_task != null)
            {
                _task = null;
                while (_state) { Thread.Sleep(10); }
            }
        }

        public void PCAB_TaskRestart()
        {
            if (_task == null)
            {
                _task = true;
            }
        }


        public bool PCAB_PRESET(PCAB_UnitInterface unit)
        {
            try
            {
                PCAB_TaskPause();
                bool result = serialInterface.LoadFactoryDefault(unit);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex){ throw ex; }
        }
        public bool PCAB_WriteDSAin(PCAB_UnitInterface unit, uint config)
        {
            PCAB_TaskPause();
            try
            {
                bool result = serialInterface.WriteDSAin(unit, config);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool PCAB_WriteDSA(PCAB_UnitInterface unit, List<uint> configs)
        {
            PCAB_TaskPause();
            try
            {
                bool result = serialInterface.WriteDSA(unit, configs);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool PCAB_WriteDPS(PCAB_UnitInterface unit, List<uint> configs)
        {
            PCAB_TaskPause();
            try
            {
                bool result = serialInterface.WriteDPS(unit, configs);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool PCAB_SetSTB_AMP(PCAB_UnitInterface unit, bool mode)
        {
            PCAB_TaskPause();
            try
            {
                bool result = serialInterface.SetSTB_AMP(unit, mode);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool PCAB_SetSTB_DRA(PCAB_UnitInterface unit, bool mode)
        {
            PCAB_TaskPause();
            try
            {
                bool result = serialInterface.SetSTB_DRA(unit, mode);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool PCAB_SetSTB_LNA(PCAB_UnitInterface unit, bool mode)
        {
            PCAB_TaskPause();
            try
            {
                bool result = serialInterface.SetSTB_LNA(unit, mode);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool PCAB_SetLowPowerMode(PCAB_UnitInterface unit, bool mode)
        {
            PCAB_TaskPause();
            try
            {
                bool result = serialInterface.SetLowPowerMode(unit, mode);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public int PCAB_GetDSAin(PCAB_UnitInterface unit)
        {
            PCAB_TaskPause();
            try
            {
                int result = serialInterface.GetDSAin(unit);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public List<uint> PCAB_GetDSA(PCAB_UnitInterface unit)
        {
            PCAB_TaskPause();
            try
            {
                List<uint> result = serialInterface.GetDSA(unit);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public List<uint> PCAB_GetDPS(PCAB_UnitInterface unit)
        {
            PCAB_TaskPause();
            try
            {
                List<uint> result = serialInterface.GetDPS(unit);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool? PCAB_GetSTB_AMP(PCAB_UnitInterface unit)
        {
            PCAB_TaskPause();
            try
            {
                bool result = serialInterface.GetSTB_AMP(unit);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool? PCAB_GetSTB_DRA(PCAB_UnitInterface unit)
        {
            PCAB_TaskPause();
            try
            {
                bool result = serialInterface.GetSTB_DRA(unit);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool? PCAB_GetSTB_LNA(PCAB_UnitInterface unit)
        {
            PCAB_TaskPause();
            try
            {
                bool result = serialInterface.GetSTB_LNA(unit);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool? PCAB_GetLowPowerMode(PCAB_UnitInterface unit)
        {
            PCAB_TaskPause();
            try
            {
                bool result = serialInterface.GetLowPowerMode(unit);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public TempratureID PCAB_GetTempID(PCAB_UnitInterface unit)
        {
            PCAB_TaskPause();
            try
            {
                TempratureID result = serialInterface.GetTempID(unit);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public TempratureValue PCAB_GetTempValue(PCAB_UnitInterface unit)
        {
            PCAB_TaskPause();
            try
            {
                TempratureValue result = serialInterface.GetTempValue(unit);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public byte PCAB_GetMode(PCAB_UnitInterface unit)
        {
            PCAB_TaskPause();
            try
            {
                byte result = serialInterface.GetMode(unit);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public AnalogValues PCAB_GetAnalogValue(PCAB_UnitInterface unit)
        {
            PCAB_TaskPause();
            try
            {
                AnalogValues result = serialInterface.GetAnalogValue(unit);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public SensorValues PCAB_GetSensorValue(PCAB_UnitInterface unit)
        {
            PCAB_TaskPause();
            try
            {
                SensorValues result = serialInterface.GetSensorValue(unit);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public string PCAB_GetIDN(PCAB_UnitInterface unit)
        {
            PCAB_TaskPause();
            try
            {
                string result = serialInterface.GetIDN(unit);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool PCAB_LoadFactoryDefault(PCAB_UnitInterface unit)
        { return PCAB_PRESET(unit); }
        public bool? PCAB_SaveState(PCAB_UnitInterface unit, uint confNum)
        { return PCAB_SaveState(unit, 14, 0, confNum); }
        public bool? PCAB_SaveState(PCAB_UnitInterface unit, byte sectorNum, byte pageNum, uint confNum)
        {
            PCAB_TaskPause();
            try
            {
                bool result = serialInterface.SaveState(unit, sectorNum, pageNum, confNum);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool? PCAB_SaveState(PCAB_UnitInterface unit, byte sectorPageNum, uint confNum)
        {
            PCAB_TaskPause();
            try
            {
                bool result = serialInterface.SaveState(unit, sectorPageNum, confNum);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool? PCAB_LoadState(PCAB_UnitInterface unit, uint confNum)
        { return PCAB_LoadState(unit, 14, 0, confNum); }
        public bool? PCAB_LoadState(PCAB_UnitInterface unit, byte sectorNum, byte pageNum, uint confNum)
        {
            PCAB_TaskPause();
            try
            {
                bool result = serialInterface.LoadState(unit, sectorNum, pageNum, confNum);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool? PCAB_LoadState(PCAB_UnitInterface unit, byte sectorPageNum, uint confNum)
        {
            PCAB_TaskPause();
            try
            {
                bool result = serialInterface.LoadState(unit, sectorPageNum, confNum);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }

        public List<byte> PCAB_ReadROM(PCAB_UnitInterface unit, UInt32 blockNum, byte sectorNum)
        {
            PCAB_TaskPause();
            try
            {
                List<byte> result = serialInterface.ReadROM(unit, blockNum, sectorNum);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool? PCAB_OverWriteROM(PCAB_UnitInterface unit, UInt32 blockNum, byte sectorNum, List<byte> dat)
        {
            PCAB_TaskPause();
            try
            {
                bool result = serialInterface.OverWriteROM(unit, blockNum, sectorNum, dat);
                PCAB_TaskRestart();
                return result;
            }
            catch (Exception ex) { throw ex; }
        }

        public class PCABEventArgs : EventArgs
        {
            private string msg;
            private List<PCAB_UnitInterface> dat;

            public PCABEventArgs(List<PCAB_UnitInterface> ReceiveDAT, string Message) { dat = ReceiveDAT; msg = Message; }

            public string Message { get { return msg; } }

            public List<PCAB_UnitInterface> ReceiveDAT { get { return dat; } }

        }
    }

    public class PCAB_SerialInterface
    {
        private const string REVISION_CHECK_STRING = "2.0.";
        private const int SLEEP_TIME_LOOP = 5;
        private const int SLEEP_TIME = 50;
        private SerialPort _serialPort;
        public bool isOpen { get; private set; } = false;
        public List<PCAB_UnitInterface> pcabUNITs { get; private set; } = new List<PCAB_UnitInterface>();
        public string PortName { get { return _serialPort.PortName; } }
        private List<byte> serialBF = new List<byte>();

        /// <summary>Constructor</summary>
        /// <param name="PortName">Serial Port Name</param>
        public PCAB_SerialInterface(string PortName, UInt32 BaudRate_Ex)
            : this(PortName, (int)BaudRate_Ex, 8, Parity.None, StopBits.One, 4096, 5000, 5000) {  }
        public PCAB_SerialInterface(string PortName,
            int baudRate, int dataBits, Parity parity, StopBits stopbit, int readBufferSize, int writeTimeOut, int readTimeOut)
        {
            _serialPort = new SerialPort(PortName);
            _serialPort.BaudRate = baudRate;
            _serialPort.DataBits = dataBits;
            _serialPort.Parity = parity;
            _serialPort.StopBits = stopbit;
            _serialPort.Handshake = Handshake.None;
            _serialPort.DtrEnable = true;
            _serialPort.Encoding = Encoding.ASCII;
            _serialPort.NewLine = "\r\n";
            _serialPort.ReadBufferSize = readBufferSize;
            _serialPort.WriteTimeout = writeTimeOut;
            _serialPort.ReadTimeout = readTimeOut;
        }

        public PCAB_SerialInterface(SerialPort serialPort) { _serialPort = serialPort; }

        ~PCAB_SerialInterface() { this.Close(); _serialPort = null; }

        public void Close()
        {
            if (_serialPort?.IsOpen == true)
            {
                try
                {
                    foreach (PCAB_UnitInterface unit in pcabUNITs)
                    {
                        try
                        {
                            WriteSLIP(unit.GetCommandCode(new List<byte> { 0xFE }));
                        }
                        catch { }
                        Thread.Sleep(SLEEP_TIME_LOOP);
                        DiscardInBuffer();
                    }
                    Thread.Sleep(SLEEP_TIME);
                    DiscardInBuffer();
                    _serialPort.Close();
                }
                catch { }
            }
            isOpen = false;
            pcabUNITs.Clear();
        }

        /// <summary>Open Serial Port</summary>
        /// <param name="SerialNumbers">Serial Number List</param>
        /// <returns></returns>
        public List<bool> Open(List<String> SerialNumbers)
        {
            List<bool> results = new List<bool>();
            if (SerialNumbers.Count <= 0)
            {
                return results;
            }
            if (_serialPort?.IsOpen == true) { return results; }
            try { _serialPort.Open(); }
            catch (UnauthorizedAccessException e) { MessageBox.Show("Serial port open Error.\nAlready used.\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw e; }
            catch (Exception e) { MessageBox.Show("Serial port open Error.\n{" + e.ToString() + "}\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw e; }
            try
            {
                DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
                foreach (string s in SerialNumbers)
                {
                    List<byte> bf = new List<byte>();
                    bf.AddRange(Encoding.ASCII.GetBytes("#"));
                    bf.AddRange(Encoding.ASCII.GetBytes(s));
                    bf.Add(0xFF);
                    bf.Add(0xFE);
                    WriteSLIP(bf);
                    WriteSLIP(bf);
                    WriteSLIP(bf);
                    Thread.Sleep(SLEEP_TIME_LOOP);
                    _serialPort.WriteLine("");
                    _serialPort.WriteLine("");
                    _serialPort.WriteLine("");
                    _serialPort.WriteLine("#" + s + " CUI 0");
                    _serialPort.WriteLine("#" + s + " CUI 0");
                    DiscardInBuffer();
                }
                _serialPort.DiscardOutBuffer();
                Thread.Sleep(SLEEP_TIME);
                DiscardInBuffer();
                int rTObf = _serialPort.ReadTimeout;
                _serialPort.ReadTimeout = 100;
                foreach (string s in SerialNumbers)
                {
                    try
                    {
                        _serialPort.WriteLine("#" + s + " GetIDN");
                        string[] arrBf = _serialPort.ReadLine().Split(',');
                        if (arrBf.Length == 4)
                        {
                            if (arrBf[0] == "Orient Microwave Corp." && arrBf[1] == "LX00-0004-00" && arrBf[3].Substring(0, REVISION_CHECK_STRING.Length) == REVISION_CHECK_STRING && (arrBf[2] == s || "*" == s))
                            {
                                pcabUNITs.Add(new PCAB_UnitInterface(s));
                                results.Add(true);
                            }
                            else { results.Add(false); }
                        }
                        else { results.Add(false); }
                    }
                    catch
                    {
                        results.Add(false);
                        _serialPort.WriteLine("#" + s + " CUI 1");
                        Thread.Sleep(SLEEP_TIME_LOOP);
                        DiscardInBuffer();
                    }
                }
                _serialPort.ReadTimeout = rTObf;
                if (pcabUNITs.Count <= 0)
                {
                    _serialPort.Close();
                    return results;
                }

                foreach (PCAB_UnitInterface unit in pcabUNITs)
                {
                    _serialPort.WriteLine("#" + unit.SerialNumberASCII + " BCM");
                }
                _serialPort.Write(new byte[] { 0xC0 }, 0, 1);
                Thread.Sleep(SLEEP_TIME);
                DiscardInBuffer();
            }
            catch (Exception)
            {
                return results;
            }
            isOpen = true;
            return results;
        }

        public void DiscardInBuffer()
        {
            _serialPort.DiscardInBuffer();
            serialBF.Clear();
        }
        public void WriteSLIP(List<byte> dat)
        {
            try
            {
                List<byte> tx = SLIP.EncodeSLIP(dat);
                _serialPort.Write(tx.ToArray(), 0, tx.Count);
            }
            catch (Exception e) { throw e; }
        }
        public List<byte> ReadSLIP() { return ReadSLIP(_serialPort.ReadBufferSize); }
        public List<byte> ReadSLIP(int bfLEN)
        {
            List<byte> ret = new List<byte>();
            if (serialBF.Count != 0)
            {
                int cnt = serialBF.IndexOf(0xC0);
                if (cnt != -1)
                {
                    ret = new List<byte>(serialBF.Skip(0).Take(cnt + 1).ToArray());
                    serialBF = new List<byte>(serialBF.Skip(cnt + 1).Take(serialBF.Count - cnt - 1).ToArray());
                }
                else
                {
                    ret = new List<byte>(serialBF.Skip(0).Take(serialBF.Count).ToArray());
                    serialBF.Clear();
                }
            }

            byte[] dat = new byte[bfLEN];
            DateTime startTime = DateTime.Now;
            int num;
            try
            {
                do
                {
                    TimeSpan span = DateTime.Now - startTime;
                    if (span.TotalMilliseconds > _serialPort.ReadTimeout) { throw new Exception("ReadSLPI TimeOut"); }
                    int count = _serialPort.Read(dat, 0, bfLEN);
                    ret.AddRange(new List<byte>(dat.Skip(0).Take(count).ToArray()));
                    num = ret.IndexOf(0xC0);
                    if (num != -1)
                    {
                        if (ret.Count != num + 1)
                        {
                            serialBF = new List<byte>(ret.Skip(num).Take(ret.Count - num - 1).ToArray());
                            ret = new List<byte>(ret.Skip(0).Take(num + 1).ToArray());
                        }
                    }
                } while (num == -1);
                return SLIP.DecodeSLIP(ret);
            }
            catch (Exception ex) { throw ex; }
        }
        public List<byte> WriteReadSLIP(List<byte> dat)
        {
            WriteSLIP(dat);
            return ReadSLIP();
        }

        public bool WriteDSAin(uint unitNum, uint config) { try { return WriteDSAin(pcabUNITs[(int)unitNum], config); } catch (Exception ex) { throw ex; } }
        public bool WriteDSAin(PCAB_UnitInterface unit, uint config)
        { return WriteDSAin(unit, (byte)config); }
        public bool WriteDSAin(PCAB_UnitInterface unit, byte config)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xB0, config }));
                if (ret[0] == 0x00) { return true; }
                return false;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool WriteDSA(uint unitNum, List<uint> config) { try { return WriteDSA(pcabUNITs[(int)unitNum], config); } catch (Exception ex) { throw ex; } }
        public bool WriteDSA(PCAB_UnitInterface unit, List<uint> configs)
        { return WriteDSA(unit, configs.ConvertAll(x => (byte)x)); }
        public bool WriteDSA(PCAB_UnitInterface unit, List<byte> configs)
        {
            if (configs.Count != 15) { return false; }
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC1 }.Concat(configs).ToList()));
                if (ret[0] == 0x00) { return true; }
                return false;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool WriteDPS(uint unitNum, List<uint> config)
        { try { return WriteDPS(pcabUNITs[(int)unitNum], config); } catch (Exception ex) { throw ex; } }
        public bool WriteDPS(PCAB_UnitInterface unit, List<uint> configs)
        { return WriteDPS(unit, configs.ConvertAll(x => (byte)x)); }
        public bool WriteDPS(PCAB_UnitInterface unit, List<byte> configs)
        {
            try
            {
                if (configs?.Count != 15) { return false; }
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC2 }.Concat(configs).ToList()));
                if (ret[0] == 0x00) { return true; }
                return false;
            }
            catch (Exception ex) { throw ex; }
        }

        public bool SetSTB_AMP(uint unitNum, bool config) { try { return SetSTB_AMP(pcabUNITs[(int)unitNum], config); } catch (Exception ex) { throw ex; } }
        public bool SetSTB_AMP(PCAB_UnitInterface unit, bool mode)
        {
            try
            {
                List<byte> ret;
                if (mode) { ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC3, 0x01 })); }
                else { ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC3, 0x00 })); }
                if (ret[0] == 0x00) { return true; }
                return false;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool SetSTB_DRA(uint unitNum, bool config) { try { return SetSTB_DRA(pcabUNITs[(int)unitNum], config); } catch (Exception ex) { throw ex; } }
        public bool SetSTB_DRA(PCAB_UnitInterface unit, bool mode)
        {
            try
            {
                List<byte> ret;
                if (mode) { ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC4, 0x01 })); }
                else { ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC4, 0x00 })); }
                if (ret[0] == 0x00) { return true; }
                return false;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool SetSTB_LNA(uint unitNum, bool config) { try { return SetSTB_LNA(pcabUNITs[(int)unitNum], config); } catch (Exception ex) { throw ex; } }
        public bool SetSTB_LNA(PCAB_UnitInterface unit, bool mode)
        {
            try
            {
                List<byte> ret;
                if (mode) { ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC5, 0x01 })); }
                else { ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC5, 0x00 })); }
                if (ret[0] == 0x00) { return true; }
                return false;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool SetLowPowerMode(PCAB_UnitInterface unit, bool mode)
        {
            try
            {
                List<byte> ret;
                if (mode) { ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC6, 0x01 })); }
                else { ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xC6, 0x00 })); }
                if (ret[0] == 0x00) { return true; }
                return false;
            }
            catch (Exception ex) { throw ex; }
        }
        public bool SetLowPowerMode(uint unitNum, bool config) { try { return SetLowPowerMode(pcabUNITs[(int)unitNum], config); } catch (Exception ex) { throw ex; } }

        public int GetDSAin(uint unitNum) { try { return GetDSAin(pcabUNITs[(int)unitNum]); } catch (Exception ex) { throw ex; } }
        public int GetDSAin(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xD0 }));
                if (ret[0] > 0xF0) { return -1; }
                return (int)ret[0];
            }
            catch (Exception ex) { throw ex; }
        }
        public List<uint> GetDSA(uint unitNum) { try { return GetDSA(pcabUNITs[(int)unitNum]); } catch (Exception ex) { throw ex; } }
        public List<uint> GetDSA(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xD1 }));
                if (ret.Count == 15) { return ret.ConvertAll(x => (uint)x); }
                else { throw new Exception("GetDSA Error"); }
            }
            catch (Exception ex) { throw ex; }
        }
        public List<uint> GetDPS(uint unitNum) { try { return GetDPS(pcabUNITs[(int)unitNum]); } catch (Exception ex) { throw ex; } }
        public List<uint> GetDPS(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xD2 }));
                if (ret.Count == 15) { return ret.ConvertAll(x => (uint)x); }
                else { throw new Exception("GetDSA Error"); }
            }
            catch (Exception ex) { throw ex; }
        }
        public bool GetSTB_AMP(uint unitNum) { try { return GetSTB_AMP(pcabUNITs[(int)unitNum]); } catch (Exception ex) { throw ex; } }
        public bool GetSTB_AMP(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xD3 }));
                if (ret[0] == 0x00) { return false; }
                else if (ret[0] == 0x01) { return true; }
                else { throw new Exception("GetSTB_AMP Error"); }
            }
            catch (Exception ex) { throw ex; }
        }
        public bool GetSTB_DRA(uint unitNum) { try { return GetSTB_DRA(pcabUNITs[(int)unitNum]); } catch (Exception ex) { throw ex; } }
        public bool GetSTB_DRA(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xD4 }));
                if (ret[0] == 0x00) { return false; }
                else if (ret[0] == 0x01) { return true; }
                else { throw new Exception("GetSTB_DRA Error"); }
            }
            catch (Exception ex) { throw ex; }
        }
        public bool GetSTB_LNA(uint unitNum) { try { return GetSTB_LNA(pcabUNITs[(int)unitNum]); } catch (Exception ex) { throw ex; } }
        public bool GetSTB_LNA(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xD5 }));
                if (ret[0] == 0x00) { return false; }
                else if (ret[0] == 0x01) { return true; }
                else { throw new Exception("GetSTB_LNA Error"); }
            }
            catch (Exception ex) { throw ex; }
        }
        public bool GetLowPowerMode(uint unitNum) { try { return GetLowPowerMode(pcabUNITs[(int)unitNum]); } catch (Exception ex) { throw ex; } }
        public bool GetLowPowerMode(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xD6 }));
                if (ret[0] == 0x00) { return false; }
                else if (ret[0] == 0x01) { return true; }
                else { throw new Exception("GetLowPowerMode Error"); }
            }
            catch (Exception ex) { throw ex; }
        }
        public TempratureID GetTempID(uint unitNum) { try { return GetTempID(pcabUNITs[(int)unitNum]); } catch (Exception ex) { throw ex; } }
        public TempratureID GetTempID(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xE1 }));
                if (ret.Count % 8 == 0)
                {
                    List<UInt64> result = new List<UInt64>();
                    for (int i = 0; i < ret.Count; i += 8)
                    {
                        result.Add(
                            (1ul << 56) * (UInt64)ret[i] +
                            (1ul << 48) * (UInt64)ret[i + 1] +
                            (1ul << 40) * (UInt64)ret[i + 2] +
                            (1ul << 32) * (UInt64)ret[i + 3] +
                            (1ul << 24) * (UInt64)ret[i + 4] +
                            (1ul << 16) * (UInt64)ret[i + 5] +
                            (1ul << 8) * (UInt64)ret[i + 6] +
                            (UInt64)ret[i + 7]);
                    }
                    return new TempratureID(result);
                }
                else { throw new Exception("GetTempID Error"); }
            }
            catch (Exception ex) { throw ex; }
        }
        public TempratureValue GetTempValue(uint unitNum) { try { return GetTempValue(pcabUNITs[(int)unitNum]); } catch (Exception ex) { throw ex; } }
        public TempratureValue GetTempValue(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xE2 }));
                return new TempratureValue(ret);
            }
            catch (Exception ex) { throw ex; }
        }
        public byte GetMode(uint unitNum) { try { return GetMode(pcabUNITs[(int)unitNum]); } catch (Exception ex) { throw ex; } }
        public byte GetMode(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xEA }));
                if (ret[0] > 0x0F) { throw new Exception("GetMode Error"); }
                else { return ret[0]; }
            }
            catch (Exception ex) { throw ex; }
        }
        public AnalogValues GetAnalogValue(uint unitNum) { try { return GetAnalogValue(pcabUNITs[(int)unitNum]); } catch (Exception ex) { throw ex; } }
        public AnalogValues GetAnalogValue(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xEE }));
                return new AnalogValues(ret);
            }
            catch (Exception ex) { throw ex; }
        }
        public SensorValues GetSensorValue(uint unitNum) { try { return GetSensorValue(pcabUNITs[(int)unitNum]); } catch (Exception ex) { throw ex; } }
        public SensorValues GetSensorValue(PCAB_UnitInterface unit)
        {
            try
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xEF }));
                sw.Stop();
                if (ret.Count == 10 + 2 * 15 || ret.Count == 10 || ret.Count == 2 * 15) { return new SensorValues(ret); }
                else { throw new Exception("GetSensorValue Error.\nCount > " + ret?.Count + "\nDAT > " + BitConverter.ToString(ret?.ToArray())); }
            }
            catch (Exception ex) { throw ex; }
        }

        public string GetIDN(uint unitNum) { try { return GetIDN(pcabUNITs[(int)unitNum]); } catch (Exception ex) { throw ex; } }
        public string GetIDN(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xF0 }));
                if (ret[0] == 0xF2) { return null; }
                else { return Encoding.ASCII.GetString(ret.ToArray()); }
            }
            catch (Exception ex) { throw ex; }
        }
        public bool LoadFactoryDefault(uint unitNum) { try { return LoadFactoryDefault(pcabUNITs[(int)unitNum]); } catch (Exception ex) { throw ex; } }
        public bool LoadFactoryDefault(PCAB_UnitInterface unit)
        {
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xFA }));
                if (ret[0] == 0x00) { return true; }
                else { return false; }
            }
            catch (Exception ex) { throw ex; }
        }

        public bool SaveState(PCAB_UnitInterface unit, uint confNum)
        {
            try
            {
                if (confNum > 3) { return false; }
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xFB, (byte)confNum }));
                if (ret[0] == 0x00) { return true; }
                else { return false; }
            }
            catch (Exception ex) { throw ex; }
        }
        public bool SaveState(PCAB_UnitInterface unit, byte sectorNum, byte pageNum, uint confNum)
        {
            if (sectorNum > 0x0F) { return false; }
            if (pageNum > 0x0F) { return false; }
            uint spNum = (1u << 4) * sectorNum + pageNum;
            return SaveState(unit, (byte)spNum, confNum);
        }
        public bool SaveState(PCAB_UnitInterface unit, byte sectorPageNum, uint confNum)
        {
            if (confNum > 3) { return false; }
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xFB, sectorPageNum, (byte)confNum }));
                if (ret[0] == 0x00) { return true; }
                else { return false; }
            }
            catch (Exception ex) { throw ex; }
        }
        public bool LoadState(PCAB_UnitInterface unit, uint confNum)
        {
            try
            {
                if (confNum > 3) { return false; }
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xFC, (byte)confNum }));
                if (ret[0] == 0x00) { return true; }
                else { return false; }
            }
            catch (Exception ex) { throw ex; }
        }
        public bool LoadState(PCAB_UnitInterface unit, byte sectorNum, byte pageNum, uint confNum)
        {
            if (sectorNum > 0x0F) { return false; }
            if (pageNum > 0x0F) { return false; }
            uint spNum = (1u << 4) * sectorNum + pageNum;
            return LoadState(unit, (byte)spNum, confNum);
        }
        public bool LoadState(PCAB_UnitInterface unit, byte sectorPageNum, uint confNum)
        {
            if (confNum > 3) { return false; }
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xFC, sectorPageNum, (byte)confNum }));
                if (ret[0] == 0x00) { return true; }
                else { return false; }
            }
            catch (Exception ex) { throw ex; }
        }

        public List<byte> ReadROM(PCAB_UnitInterface unit, UInt32 blockNum, byte sectorNum)
        {
            if (sectorNum > 0x0F) { return null; }
            byte block1 = (byte)((blockNum & 0xFF00) >> 8);
            byte block2 = (byte)((blockNum & 0x00FF));
            uint spNum = (1u << 4) * sectorNum;
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xAA, block1, block2, (byte)spNum }));
                if (ret.Count != 4096) { return null; }
                else { return ret; }
            }
            catch (Exception ex) { throw ex; }
        }
        public bool OverWriteROM(PCAB_UnitInterface unit, UInt32 blockNum, byte sectorNum, List<byte> dat)
        {
            if (sectorNum > 0x0F) { return false; }
            if (dat.Count != 4096) { return false; }
            byte block1 = (byte)((blockNum & 0xFF00) >> 8);
            byte block2 = (byte)((blockNum & 0x00FF));
            uint spNum = (1u << 4) * sectorNum;
            try
            {
                List<byte> ret = WriteReadSLIP(unit.GetCommandCode(new List<byte> { 0xBB, block1, block2, (byte)spNum }.Concat(dat).ToList()));
                if (ret[0] == 0x00) { return true; }
                else { return false; }
            }
            catch (Exception ex) { throw ex; }
        }

        public class PCAB_UnitInterface
        {
            private string _sn;
            public string SerialNumberASCII { get { return _sn; } }
            public byte[] SerialNumberBinary { get { return Encoding.ASCII.GetBytes(_sn); } }
            public SensorValues SensorValuesNOW { get; set; }

            public PCAB_UnitInterface(string SerialNumber)
            {
                _sn = SerialNumber;
                SensorValuesNOW = new SensorValues();
            }

            public List<byte> GetCommandCode(List<byte> cmd)
            {
                List<byte> ret = new List<byte>();
                try
                {
                    ret.AddRange(Encoding.ASCII.GetBytes("#"));
                    ret.AddRange(Encoding.ASCII.GetBytes(_sn));
                    ret.Add(0xFF);
                    ret.AddRange(cmd);
                    return ret;
                }catch (Exception ex) { throw ex; }
            }

        }

        public struct SensorValues
        {
            public AnalogValues Analog { get; private set; }
            public TempratureValue Temprature { get; private set; }
            public TempratureID ID { get; private set; }
            public SensorValues(List<byte> dat)
            {
                ID = new TempratureID();
                if (dat.Count == 10 + 2 * 15)
                {
                    Analog = new AnalogValues(dat.Skip(0).Take(10).ToList());
                    Temprature = new TempratureValue(dat.Skip(10).Take(2 * 15).ToList());
                }
                else if (dat.Count == 10)
                {
                    Analog = new AnalogValues(dat);
                    Temprature = new TempratureValue();
                }
                else if (dat.Count == 2 * 15)
                {
                    Analog = new AnalogValues();
                    Temprature = new TempratureValue(dat);
                }
                else
                {
                    Analog = new AnalogValues();
                    Temprature = new TempratureValue();
                }
            }
            public SensorValues(List<UInt64> id) : this(new List<byte>(), id) { }
            public SensorValues(List<byte> dat, List<UInt64> id) : this(dat)
            { ID = new TempratureID(id); }
            public SensorValues(AnalogValues _analog, TempratureValue _tempVal, TempratureID _id)
            { Analog = _analog; Temprature = _tempVal; ID = _id; }
            public void Clear() { Analog.Clear(); Temprature.Clear(); ID.Clear(); }
        }

        public struct AnalogValues
        {
            public float CPU_Temprature { get; private set; }
            public float Vd { get; private set; }
            public float Id { get; private set; }
            public float Vin { get; private set; }
            public float Pin { get; private set; }
            public AnalogValues(List<byte> dat)
            {
                if (dat.Count == 10)
                {
                    Vd = ((1u << 8) * (UInt16)dat[0] + (UInt16)dat[1]) * 3.3f / (1 << 12);
                    Id = ((1u << 8) * (UInt16)dat[2] + (UInt16)dat[3]) * 3.3f / (1 << 12);
                    Vin = ((1u << 8) * (UInt16)dat[4] + (UInt16)dat[5]) * 3.3f / (1 << 12);
                    Pin = ((1u << 8) * (UInt16)dat[6] + (UInt16)dat[7]) * 3.3f / (1 << 12);
                    CPU_Temprature = ((1u << 8) * (UInt16)dat[8] + (UInt16)dat[9]) * 3.3f / (1 << 12);
                    Vd = Vd * 10.091f;
                    Id = (Id - 0.08f) / 0.737f;
                    Vin = Vin * 15.0f;
                    CPU_Temprature = 27.0f - (CPU_Temprature - 0.706f) / 0.001721f;
                }
                else { CPU_Temprature = float.NaN; Vd = float.NaN; Id = float.NaN; Vin = float.NaN; Pin = float.NaN; }
            }
            public void Clear() { CPU_Temprature = float.NaN; Vd = float.NaN; Id = float.NaN; Vin = float.NaN; Pin = float.NaN; }
        }

        public struct TempratureValue
        {
            public float[] Values { get; private set; }
            public TempratureValue(List<byte> dat)
            {
                if (dat.Count == 2 * 15)
                {
                    Values = new float[15];
                    for (int i = 0; i < 15; i++)
                    {
                        Values[i] = (short)((1u << 8) * (UInt16)dat[2 * i] + (UInt16)dat[2 * i + 1]) / 16.0f;
                    }
                }
                else { Values = null; }
            }
            public void Clear() { Values = null; }
        }

        public struct TempratureID
        {
            public UInt64[] IDs { get; private set; }
            public TempratureID(List<UInt64> dat)
            {
                if (dat.Count == 15)
                {
                    IDs = new UInt64[15];
                    for (int i = 0; i < 15; i++)
                    {
                        IDs[i] = dat[i];
                    }
                }
                else { IDs = null; }
            }
            public void Clear() { IDs = null; }
        }
    }
}
