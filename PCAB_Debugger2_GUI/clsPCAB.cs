using System;
using System.Collections.Generic;
using System.Windows;
using static PCAB_Debugger2_GUI.PCAB_TASK;
using static PCAB_Debugger2_GUI.Ports;

namespace PCAB_Debugger2_GUI
{
    public class PCAB
    {
        public struct SN_POSI : IEquatable<SN_POSI>,  IEqualityComparer<SN_POSI>
        {
            public string SerialNumber { get; set; }
            public ROTATE RotateCODE { get; set; }
            public SN_POSI(string _serialNumber, ROTATE _rotatecode) { SerialNumber = _serialNumber; RotateCODE = _rotatecode; }

            public bool Equals(SN_POSI x, SN_POSI y)
            {
                if (Object.ReferenceEquals(x, y)) return true;
                return x.SerialNumber == y.SerialNumber;
            }

            public int GetHashCode(SN_POSI obj)
            {
                return this.GetHashCode();
            }

            bool IEquatable<SN_POSI>.Equals(SN_POSI other)
            {
                return this.SerialNumber == other.SerialNumber;
            }
        }
        private PCAB_TASK _task;
        public bool? isOpen { get { return _task?.isOpen; } }
        public List<ControlTab> PCAB_Boards {  get; private set; } = new List<ControlTab>();
        public List<UnitMonitor> PCAB_Monitors { get; private set; } = new List<UnitMonitor>();
        public event EventHandler<PCABEventArgs> OnError;
        public PCAB_TASK serial { get { return _task; } }
        public PCAB(string SerialPortName, UInt32 BaudRate)
        {
            _task = new PCAB_TASK(SerialPortName, BaudRate);
            _task.OnUpdateDAT += OnUpdateDAT;
            _task.OnTaskError += PCAB_TASK_OnError;
        }

        ~PCAB() { }

        public void Open(List<SN_POSI> SerialNumber_Posi, uint MonitorIntervalTime)
        {
            bool result = false;
            List<string> serialNumber = new List<string>();
            foreach(SN_POSI snp in SerialNumber_Posi) { serialNumber.Add(snp.SerialNumber); }
            List<bool> ret = _task.PCAB_AutoTaskStart(MonitorIntervalTime, serialNumber);
            foreach (bool re in ret) { if (re) { result = true; break; } }
            if (result && ret.Count == SerialNumber_Posi.Count)
            {
                int cnt = 0;
                for (int snCNT = 0; snCNT < SerialNumber_Posi.Count; snCNT++)
                {
                    if (ret[snCNT])
                    {
                        PCAB_Boards.Add(new ControlTab(_task.UNITs[cnt].SerialNumberASCII, SerialNumber_Posi[snCNT].RotateCODE));
                        PCAB_Monitors.Add(new UnitMonitor(_task.UNITs[cnt].SerialNumberASCII));
                        PCAB_Monitors[cnt].TITLE = "S/N, " + _task.UNITs[cnt].SerialNumberASCII;
                        cnt++;
                    }
                }
            }
        }

        public void Close()
        {
            _task?.PCAB_AutoTaskStop(true);
            PCAB_Boards.Clear(); PCAB_Monitors.Clear();
        }

        private void PCAB_TASK_OnError(object sender, PCABEventArgs e)
        {
            _task.PCAB_AutoTaskStop(false);
            _task = null;
            MessageBox.Show(e.Message.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            OnError?.Invoke(this, e);
        }

        private void OnUpdateDAT(object sender, PCABEventArgs e)
        {
            if (e.ReceiveDAT.Count == PCAB_Monitors.Count)
            {
                for (int cnt = 0; cnt < PCAB_Monitors.Count; cnt++)
                {
                    PCAB_Monitors[cnt].TITLE = "S/N, " + e.ReceiveDAT[cnt].SerialNumberASCII;
                    PCAB_Monitors[cnt].TEMPcpu = e.ReceiveDAT[cnt].SensorValuesNOW.Analog.CPU_Temprature.ToString("0.00");
                    PCAB_Monitors[cnt].SNSvin = e.ReceiveDAT[cnt].SensorValuesNOW.Analog.Vin.ToString("0.00");
                    PCAB_Monitors[cnt].SNSpin = e.ReceiveDAT[cnt].SensorValuesNOW.Analog.Pin.ToString("0.00");
                    PCAB_Monitors[cnt].SNSvd = e.ReceiveDAT[cnt].SensorValuesNOW.Analog.Vd.ToString("0.00");
                    PCAB_Monitors[cnt].SNSid = e.ReceiveDAT[cnt].SensorValuesNOW.Analog.Id.ToString("0.00");
                    if (e.ReceiveDAT[cnt].SensorValuesNOW.Temprature.Values?.Length == 15)
                    {
                        float avg = 0;
                        for (uint i = 0; i < e.ReceiveDAT[cnt].SensorValuesNOW.Temprature.Values.Length; i++)
                        {
                            avg += e.ReceiveDAT[cnt].SensorValuesNOW.Temprature.Values[i];
                            PCAB_Monitors[cnt].SetTempValue(i + 1, e.ReceiveDAT[cnt].SensorValuesNOW.Temprature.Values[i].ToString("0.00"));
                        }
                        PCAB_Monitors[cnt].TEMPavg = (avg / 15.0f).ToString("0.00");
                    }
                    if(e.ReceiveDAT[cnt].SensorValuesNOW.ID.IDs?.Length == 15)
                    {
                        for (uint i = 0; i < e.ReceiveDAT[cnt].SensorValuesNOW.ID.IDs.Length; i++)
                        {
                            PCAB_Monitors[cnt].SetTempID(i + 1, "0x" + e.ReceiveDAT[cnt].SensorValuesNOW.ID.IDs[i].ToString("X16"));
                        }
                        PCAB_Monitors[cnt].TEMPviewIDs = true;
                    }
                }
            }
        }
    }
}
