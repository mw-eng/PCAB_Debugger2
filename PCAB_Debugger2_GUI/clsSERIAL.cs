using System.Collections.Generic;
using System.IO.Ports;
using System.Management;

namespace PCAB_Debugger2_GUI
{
    public class ShowSerialPortName
    {
        public struct SerialPortTable
        {
            public string Name { get; }
            public string Caption { get; }
            public SerialPortTable(string name, string caption) { Name = name; Caption = caption; }
        }

        public static SerialPortTable[] GetDeviceNames()
        {
            var deviceNameList = new System.Collections.ArrayList();
            var check = new System.Text.RegularExpressions.Regex("(COM[1-9][0-9]?[0-9]?)");
            ManagementClass mcPnPEntity = new ManagementClass("Win32_PnPEntity");
            ManagementObjectCollection manageObjCol = mcPnPEntity.GetInstances();
            //全てのPnPデバイスを探索しシリアル通信が行われるデバイスを随時追加
            foreach (ManagementObject manageObj in manageObjCol)
            {
                //Nameプロパティを取得
                var namePropertyValue = manageObj.GetPropertyValue("Name");
                if (namePropertyValue == null) { continue; }

                //Nameプロパティ文字列の一部が"(COM1)～(COM999)"と一致するときリストに追加"
                string name = namePropertyValue.ToString();
                if (check.IsMatch(name)) { deviceNameList.Add(name); }
            }

            //戻り値作成
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length > 0 && deviceNameList.Count > 0)
            {
                List<SerialPortTable> res = new List<SerialPortTable>();
                res.Clear();
                foreach (string port in ports)
                {
                    SerialPort serial = new SerialPort(port);
                    try
                    {
                        serial.Open();
                        serial.Close();
                        foreach (string name in deviceNameList)
                        {
                            if (name.IndexOf("(" + port + ")") >= 0)
                            {
                                res.Add(new SerialPortTable(port, name));
                                break;
                            }
                        }
                    }
                    catch { }
                }
                return res.ToArray();
            }
            else
            {
                return null;
            }
        }
    }
}