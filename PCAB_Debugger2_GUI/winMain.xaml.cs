using PCAB_Debugger2_GUI.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static PCAB_Debugger2_GUI.PCAB;
using static PCAB_Debugger2_GUI.PCAB_SerialInterface;
using static PCAB_Debugger2_GUI.PCAB_TASK;
using static PCAB_Debugger2_GUI.Ports;
using static PCAB_Debugger2_GUI.ShowSerialPortName;
using static PCAB_Debugger2_GUI.Unit;
using static PCAB_Debugger2_GUI.UnitControl;


namespace PCAB_Debugger2_GUI
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class winMain : Window
    {
        SerialPortTable[] ports;
        public List<PCAB> _ioList = new List<PCAB>();
        private winMonitor monitor;
        private VisaResource visa32Resource;

        public winMain()
        {
            InitializeComponent();
            if (Settings.Default.winMainTop >= SystemParameters.VirtualScreenTop &&
                (Settings.Default.winMainTop + Settings.Default.winMainHeight) <
                SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight)
            {
                this.Top = Settings.Default.winMainTop;
            }
            if (Settings.Default.winMainLeft >= SystemParameters.VirtualScreenLeft &&
                (Settings.Default.winMainLeft + Settings.Default.winMainWidth) <
                SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth)
            {
                this.Left = Settings.Default.winMainLeft;
            }
            if (Settings.Default.winMainWidth > 0 &&
                Settings.Default.winMainWidth < SystemParameters.WorkArea.Width)
            {
                this.Width = Settings.Default.winMainWidth;
            }
            if (Settings.Default.winMainHeight > 0 &&
                Settings.Default.winMainHeight < SystemParameters.WorkArea.Height)
            {
                this.Height = Settings.Default.winMainHeight;
            }
            if (Settings.Default.winMainMaximized)
            { Loaded += (o, e) => WindowState = WindowState.Maximized; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title += " Ver," + System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductVersion;
            BOARD_GRID.IsEnabled = false;
            CONFIG_EXPANDER.IsExpanded = true;
#if DEBUG_RESET || DEBUG
            this.Title += "_DEBUG MODE";
            BOARD_GRID.IsEnabled = true;
            //CONFIG_EXPANDER.IsExpanded = false;
#endif
#if DEBUG_RESET
            Settings.Default.Reset();
#endif
            try
            {
                visa32Resource = new VisaResource();
            }
            catch
            {
                visa32Resource.Dispose();
                visa32Resource = null;
            }
            SERIAL_PORTS_COMBOBOX_RELOAD(sender, e);
            SERIAL_PORTS_COMBOBOX.SelectedIndex = Settings.Default.spBaudRate;
            if (SERIAL_PORTS_COMBOBOX.Items.Count > 0) { SERIAL_PORTS_COMBOBOX.SelectedIndex = 0; }
            if (ports != null)
            {
                for (int i = 0; i < ports.Length; i++)
                {
                    if (Settings.Default.spCaption == ports[i].Caption)
                    { SERIAL_PORTS_COMBOBOX.SelectedIndex = i; }
                }
            }
            if (SERIAL_PORTS_COMBOBOX.SelectedIndex >= 0) { CONNECT_BUTTON.IsEnabled = true; }
            else { CONNECT_BUTTON.IsEnabled = false; }
            WAITE_TIME_TEXTBOX.Text = Settings.Default.mli.ToString("0");
            SERIAL_NUMBERS_TEXTBOX.Text = Settings.Default.sn;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_ioList.Count > 0)
            {
                foreach (PCAB _io in _ioList)
                {
                    if (_io?.isOpen == true)
                    {
                        if (MessageBox.Show("Communication with PCAB\nDo you want to disconnect and exit?", "Worning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                        {
                            OnError(null, null);
                        }
                        else { e.Cancel = true; return; }
                    }
                }

            }
            Settings.Default.spCaption = SERIAL_PORTS_COMBOBOX.Text;
            Settings.Default.spBaudRate = SERIAL_PORTS_COMBOBOX.SelectedIndex;
            Settings.Default.mli = long.Parse(WAITE_TIME_TEXTBOX.Text);
            Settings.Default.sn = SERIAL_NUMBERS_TEXTBOX.Text;
            Settings.Default.winMainTop = this.Top;
            Settings.Default.winMainLeft = this.Left;
            Settings.Default.winMainHeight = this.Height;
            Settings.Default.winMainWidth = this.Width;
            Settings.Default.winMainMaximized = this.WindowState == WindowState.Maximized;
            this.WindowState = WindowState.Normal;
            Settings.Default.Save();
        }

        #region Serial EVENT

        private void SERIAL_PORTS_COMBOBOX_RELOAD(object sender, EventArgs e)
        {
            ports = GetDeviceNames();
            if (sender is ComboBox cb)
            {
                cb.Items.Clear();
                if (ports != null)
                {
                    foreach (SerialPortTable port in ports)
                    {
                        cb.Items.Add(port.Caption);
                    }
                }
            }
            else
            {
                SERIAL_PORTS_COMBOBOX.Items.Clear();
                if (ports != null)
                {
                    foreach (SerialPortTable port in ports)
                    {
                        SERIAL_PORTS_COMBOBOX.Items.Add(port.Caption);
                    }
                }
            }
        }

        private void SERIAL_PORTS_COMBOBOX_DropDownOpened(object sender, EventArgs e)
        {
            SERIAL_PORTS_COMBOBOX_RELOAD(sender, e);
        }

        private void SERIAL_PORTS_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {
            if (SERIAL_PORTS_COMBOBOX.SelectedIndex >= 0) { CONNECT_BUTTON.IsEnabled = true; }
            else { CONNECT_BUTTON.IsEnabled = false; }
        }

        private void CONNECT_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            if (_ioList.Count > 0)
            {
                OnError(null, null);
            }
            else
            {
                string[] snArr = SERIAL_NUMBERS_TEXTBOX.Text.Replace(" ", "").Split(',');
                List<SN_POSI> sn = new List<SN_POSI>();
                foreach (string strBf in snArr)
                {
                    string[] snBF = strBf.Split('@');
                    if (snBF.Length == 1) { sn.Add(new SN_POSI(snBF[0], ROTATE.ZERO)); }
                    else if (snBF.Length == 2)
                    {
                        if (string.Compare(snBF[1], "Z", true) == 0) { sn.Add(new SN_POSI(snBF[0], ROTATE.ZERO)); }
                        if (string.Compare(snBF[1], "R", true) == 0) { sn.Add(new SN_POSI(snBF[0], ROTATE.RIGHT_TURN)); }
                        if (string.Compare(snBF[1], "L", true) == 0) { sn.Add(new SN_POSI(snBF[0], ROTATE.LEFT_TURN)); }
                        if (string.Compare(snBF[1], "H", true) == 0) { sn.Add(new SN_POSI(snBF[0], ROTATE.HALF_TURN)); }
                        if (string.Compare(snBF[1], "ZM", true) == 0) { sn.Add(new SN_POSI(snBF[0], ROTATE.MIRROR_ZERO)); }
                        if (string.Compare(snBF[1], "RM", true) == 0) { sn.Add(new SN_POSI(snBF[0], ROTATE.MIRROR_RIGHT_TURN)); }
                        if (string.Compare(snBF[1], "LM", true) == 0) { sn.Add(new SN_POSI(snBF[0], ROTATE.MIRROR_LEFT_TURN)); }
                        if (string.Compare(snBF[1], "HM", true) == 0) { sn.Add(new SN_POSI(snBF[0], ROTATE.MIRROR_HALF_TURN)); }
                        if (string.Compare(snBF[1], "M", true) == 0) { sn.Add(new SN_POSI(snBF[0], ROTATE.MATRIX)); }
                    }
                }
#if !DEBUG_RESET
                sn = sn.Distinct().ToList();
#endif
                SerialPortTable[] pt = GetDeviceNames();
                foreach (SerialPortTable port in pt)
                {
                    if (port.Caption == SERIAL_PORTS_COMBOBOX.Text)
                    {
                        _ioList.Add(new PCAB(port.Name, UInt32.Parse(BAUD_RATE_COMBOBOX.Text.Trim().Replace(",", ""))));
                    }
                }
                try
                {
                    foreach (PCAB _io in _ioList)
                    {
                        _io.OnError += OnError;
                        _io.Open(sn, uint.Parse(WAITE_TIME_TEXTBOX.Text));
                    }
                }
                catch
                {
                    foreach (PCAB _io in _ioList)
                    {
                        _io.Close();
                    }
                    SERIAL_PORTS_COMBOBOX_RELOAD(null, null);
                    SERIAL_PORTS_COMBOBOX_DropDownClosed(null, null);
                    return;
                }
                for (int i = _ioList.Count; i > 0; i--)
                {
                    if (_ioList[i - 1]?.isOpen != true) { _ioList.RemoveAt(i - 1); }
                }
                if (_ioList.Count > 0)
                {
                    monitor = new winMonitor();
                    monitor.MONITOR_GRID.Children.Clear();
                    monitor.MONITOR_GRID.RowDefinitions.Clear();
                    monitor.MONITOR_GRID.ColumnDefinitions.Clear();
                    for (int i = 0; i < _ioList.Count; i++)
                    {
                        monitor.MONITOR_GRID.RowDefinitions.Add(new RowDefinition());
                        for (int j = 0; j < _ioList[i].PCAB_Monitors.Count; j++)
                        {
                            if (monitor.MONITOR_GRID.ColumnDefinitions.Count < j + 1)
                            {
                                monitor.MONITOR_GRID.ColumnDefinitions.Add(new ColumnDefinition());
                            }
                            _ioList[i].PCAB_Monitors[j].SetValue(Grid.RowProperty, i);
                            _ioList[i].PCAB_Monitors[j].SetValue(Grid.ColumnProperty, j);
                            monitor.MONITOR_GRID.Children.Add(_ioList[i].PCAB_Monitors[j]);
                        }
                    }
                    if (monitor.MONITOR_GRID.Children.Count > 1) { monitor.MONITOR_GRID.ShowGridLines = true; }
                    BOARD_GRID.Children.Clear();
                    if (_ioList.Count == 1 && _ioList[0].PCAB_Boards.Count == 1)
                    {
                        BOARD_GRID.Children.Add(_ioList[0].PCAB_Boards[0]);
                        ((ControlTab)BOARD_GRID.Children[0]).AUTO.setResourceManager = visa32Resource;
                        ((ControlTab)BOARD_GRID.Children[0]).AUTO.ButtonClickEvent += AUTO_ButtonClickEvent;
                        ((ControlTab)BOARD_GRID.Children[0]).CONFIG.ButtonClickEvent += CONFIG_ButtonClickEvent;
                        ((ControlTab)BOARD_GRID.Children[0]).CONFIG.CONFIG_SETTINGS.CheckboxClickEvent += CONFIG_CONFIG_SETTINGS_CheckboxClickEventHandler;
                        if (!readConfig(((ControlTab)BOARD_GRID.Children[0]).CONFIG.CONFIG_SETTINGS))
                        {
                            MessageBox.Show("Config read error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        BOARD_GRID.Children.Add(new TabControl());
                        ((TabControl)BOARD_GRID.Children[0]).FontSize = 24;
                        ((TabControl)BOARD_GRID.Children[0]).Margin = new Thickness(5);
                        foreach (PCAB _io in _ioList)
                        {
                            for (int i = 0; i < _io.PCAB_Boards.Count; i++)
                            {
                                TabItem _tabitem = new TabItem();
                                _tabitem.Header = "S/N, " + _io.PCAB_Boards[i].SerialNumber;
                                _tabitem.Content = _io.PCAB_Boards[i];
                                ((ControlTab)_tabitem.Content).AUTO.setResourceManager = visa32Resource;
                                ((ControlTab)_tabitem.Content).AUTO.ButtonClickEvent += AUTO_ButtonClickEvent;
                                ((ControlTab)_tabitem.Content).CONFIG.ButtonClickEvent += CONFIG_ButtonClickEvent;
                                ((ControlTab)_tabitem.Content).CONFIG.CONFIG_SETTINGS.CheckboxClickEvent += CONFIG_CONFIG_SETTINGS_CheckboxClickEventHandler;
                                ((TabControl)BOARD_GRID.Children[0]).Items.Add(_tabitem);
                                if (!readConfig(((ControlTab)_tabitem.Content).CONFIG.CONFIG_SETTINGS))
                                {
                                    MessageBox.Show("Config read error.\nS/N, " + _io.PCAB_Boards[i].SerialNumber, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                    }
                    monitor.Show();
                    CONFIG_EXPANDER.IsExpanded = false;
                    CONFIG_GRID.IsEnabled = false;
                    BOARD_GRID.IsEnabled = true;
                    CONNECT_BUTTON_CONTENT.Text = "Disconnect";
                    if (!(BOARD_GRID.Children[0] is TabControl) && _ioList[0].PCAB_Boards[0].SerialNumber == "*" &&
                        (_ioList[0].serial.PCAB_GetMode(new PCAB_UnitInterface(_ioList[0].PCAB_Boards[0].SerialNumber)) == 0x0F ||
                        _ioList[0].serial.PCAB_GetMode(new PCAB_UnitInterface(_ioList[0].PCAB_Boards[0].SerialNumber)) == 0x0A))
                    {
                        if (MessageBox.Show("Do you want to launch a binary editor?", "Binary editor", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            this.Hide();
                            Window win = new winEditor(_ioList[0].serial, new PCAB_UnitInterface(_ioList[0].PCAB_Boards[0].SerialNumber));
                            win.ShowDialog();
                            this.Show();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No valid PCAB found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return;
        }

        #endregion

        #region Other EVENT

        private void DEC_TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 0-9のみ
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void DEC_TextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // 貼付け場合
            if (e.Command == ApplicationCommands.Paste)
            {
                string strTXT = Clipboard.GetText();
                for (int cnt = 0; cnt < strTXT.Length; cnt++)
                {
                    if (!new Regex("[0-9]|[ ]").IsMatch(strTXT[cnt].ToString()))
                    {
                        // 処理済み
                        e.Handled = true;
                        break;
                    }
                }
            }
        }

        private void DEC_TextBox_PreviewLostKeyboardForcus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                uint uintVal = Convert.ToUInt32(((TextBox)sender).Text);
                if (0 <= uintVal && uintVal <= 65535) { return; }
                MessageBox.Show("Enter in the range 0 to 65535");
                e.Handled = true;
            }
            catch
            {
                MessageBox.Show("Enter in the range 0 to 65535");
                e.Handled = true;
            }
        }

        private void SN_TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 0-9およびa-f/A-Fのみ
            e.Handled = !new Regex("[0-9|a-z|A-Z|,| |*|@]").IsMatch(e.Text);
        }

        private void SN_TextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // 貼付け場合
            if (e.Command == ApplicationCommands.Paste)
            {
                string strTXT = Clipboard.GetText();
                for (int cnt = 0; cnt < strTXT.Length; cnt++)
                {
                    if (!new Regex("[0-9|a-z|A-Z|,|@]|[ ]").IsMatch(strTXT[cnt].ToString()))
                    {
                        // 処理済み
                        e.Handled = true;
                        break;
                    }
                }
            }
        }

        private void SN_TextBox_PreviewLostKeyboardForcus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                string strBF = ((TextBox)sender).Text.Replace(" ", "");
                if (0 <= strBF.IndexOf("*") && strBF.Length != 1)
                {
                    MessageBox.Show("Multiple \"*\" specifications cannot be specified.");
                    e.Handled = true;
                    return;
                }
                string[] arrBF = strBF.Split(',');
                if (arrBF.Length > 0)
                {
                    foreach (string str in arrBF)
                    {
                        string[] snBF = str.Split('@');
                        if (snBF.Length == 1)
                        {
                            if (snBF[0].Length <= 0 || 15 < snBF[0].Length) { throw new Exception(); }
                        }
                        else if (snBF.Length == 2)
                        {
                            if (snBF[0].Length < 0 || 15 < snBF[0].Length) { throw new Exception(); }
                            if (string.Compare(snBF[1], "Z", true) != 0 &&
                                string.Compare(snBF[1], "R", true) != 0 &&
                                string.Compare(snBF[1], "L", true) != 0 &&
                                string.Compare(snBF[1], "H", true) != 0 &&
                                string.Compare(snBF[1], "ZM", true) != 0 &&
                                string.Compare(snBF[1], "RM", true) != 0 &&
                                string.Compare(snBF[1], "LM", true) != 0 &&
                                string.Compare(snBF[1], "HM", true) != 0 &&
                                string.Compare(snBF[1], "M", true) != 0)
                            {
                                throw new Exception();
                            }
                        }
                    }
                }
                else { throw new Exception(); }
                return;
            }
            catch
            {
                MessageBox.Show("Enter the serial number between 1 and 15 characters, without spaces, separated by commas.");
                e.Handled = true;
            }
        }

        #endregion

        #region Sub Function EVENT
        private void AUTO_ButtonClickEvent(object sender, RoutedEventArgs e, string dirPath)
        {
            string strSN = ((AutoMeasure)sender).SerialNumber;
            PCAB _io = null;
            bool flg = false;
            foreach (PCAB io in _ioList)
            {
                foreach (PCAB_UnitInterface unit in io.serial.UNITs)
                {
                    if (unit.SerialNumberASCII == strSN) { _io = io; flg = true; break; }
                }
                if (flg) { break; }
            }
            bool detFLG = false;
            UnitControl conf = new UnitControl();
            foreach (ControlTab board in _io.PCAB_Boards)
            {
                if (board.SerialNumber == strSN)
                { conf = board.CONFIG; detFLG = true; break; }
            }
            if (!detFLG) { return; }
            readConfig(conf.CONFIG_SETTINGS);
            winLoop win = new winLoop((AutoMeasure)sender, _io.serial, new PCAB_UnitInterface(strSN), dirPath);
            win.ShowDialog();
            CONFIG_ButtonClickEvent(conf, null, ButtonCategory.WRITE);
        }
        private void CONFIG_ButtonClickEvent(object sender, RoutedEventArgs e, ButtonCategory category)
        {
            string strSN = ((UnitControl)sender).SerialNumber;
            PCAB _io = null;
            bool flg = false;
            foreach (PCAB io in _ioList)
            {
                foreach (PCAB_UnitInterface unit in io.serial.UNITs)
                {
                    if (unit.SerialNumberASCII == strSN) { _io = io; flg = true; break; }
                }
                if (flg) { break; }
            }
            uint sectorPage = 0xE0;
            uint stateNum = 0;
            string strBF = ((UnitControl)sender).MemoeryTargetAddress;
            string[] strARR = strBF.Split('-');
            try
            {
                switch (category)
                {
                    case ButtonCategory.WRITEDSA:
                    case ButtonCategory.WRITEDPS:
                    case ButtonCategory.WRITE:
                        int dsaIN = -1;
                        List<uint> dsa = new List<uint>();
                        List<uint> dps = new List<uint>();
                        if (category == ButtonCategory.WRITE || category == ButtonCategory.WRITEDSA)
                        {
                            for (uint i = 0; i < 15; i++)
                            {
                                int d = ((UnitControl)sender).CONFIG_SETTINGS.GetDSA(i + 1);
                                if (0 <= d && d < 64) { dsa.Add((uint)d); }
                            }
                            dsaIN = ((UnitControl)sender).CONFIG_SETTINGS.GetDSA(0);
                            if (dsa.Count != 15 || dsaIN < 0 || dsaIN > 31)
                            {
                                MessageBox.Show("ATT config error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                        if (category == ButtonCategory.WRITE || category == ButtonCategory.WRITEDPS)
                        {
                            for (uint i = 0; i < 15; i++)
                            {
                                int d = ((UnitControl)sender).CONFIG_SETTINGS.GetDPS(i + 1);
                                if (0 <= d && d < 64) { dps.Add((uint)d); }
                            }
                            if (dps.Count != 15)
                            {
                                MessageBox.Show("Phase config error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                        if (category == ButtonCategory.WRITE || category == ButtonCategory.WRITEDSA)
                        {
                            if (!_io.serial.PCAB_WriteDSAin(new PCAB_UnitInterface(strSN), (uint)dsaIN))
                            {
                                MessageBox.Show("J1 in ATT config write error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            if (!_io.serial.PCAB_WriteDSA(new PCAB_UnitInterface(strSN), dsa))
                            {
                                MessageBox.Show("ATT config write error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                        if (category == ButtonCategory.WRITE || category == ButtonCategory.WRITEDPS)
                        {
                            if (!_io.serial.PCAB_WriteDPS(new PCAB_UnitInterface(strSN), dps))
                            {
                                MessageBox.Show("Phase config write error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                        if (category == ButtonCategory.WRITEDSA)
                        {
                            MessageBox.Show("ATT config write done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if (category == ButtonCategory.WRITEDPS)
                        {
                            MessageBox.Show("Phase config write done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if (category == ButtonCategory.WRITE)
                        {
                            MessageBox.Show("Config write done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        break;
                    case ButtonCategory.READ:
                        if (readConfig(((UnitControl)sender).CONFIG_SETTINGS))
                        {
                            MessageBox.Show("Config read done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Config read error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        break;
                    case ButtonCategory.LOADMEM:
                        if (MessageBox.Show("Load configuration from memory."
                            , "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) { return; }
                        if (strARR.Length == 1)
                        {
                            if (!uint.TryParse(strARR[0].Trim(), out stateNum))
                            { MessageBox.Show("The setting number is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                        }
                        else if (strARR.Length == 2)
                        {
                            if (!uint.TryParse(strARR[0].Trim().StartsWith("0x") ? strARR[0].Trim().Substring(2) : strARR[0], System.Globalization.NumberStyles.HexNumber, null, out sectorPage))
                            { MessageBox.Show("The setting number is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                            if (!uint.TryParse(strARR[1].Trim(), out stateNum))
                            { MessageBox.Show("The setting number is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                        }
                        if (_io.serial.PCAB_LoadState(new PCAB_UnitInterface(strSN), (byte)sectorPage, stateNum) == false)
                        { MessageBox.Show("Load memory error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                        if (readConfig(((UnitControl)sender).CONFIG_SETTINGS))
                        { MessageBox.Show("Load memory done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information); }
                        else
                        { MessageBox.Show("Config read error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    case ButtonCategory.SAVEMEM:
                        if (MessageBox.Show("Save configuration from memory."
                            , "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) { return; }
                        if (strARR.Length == 1)
                        {
                            if (!uint.TryParse(strARR[0].Trim(), out stateNum))
                            { MessageBox.Show("The setting number is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                        }
                        else if (strARR.Length == 2)
                        {
                            if (!uint.TryParse(strARR[0].Trim().StartsWith("0x") ? strARR[0].Trim().Substring(2) : strARR[0], System.Globalization.NumberStyles.HexNumber, null, out sectorPage))
                            { MessageBox.Show("The setting number is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                            if (!uint.TryParse(strARR[1].Trim(), out stateNum))
                            { MessageBox.Show("The setting number is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                        }
                        if (_io.serial.PCAB_SaveState(new PCAB_UnitInterface(strSN), (byte)sectorPage, stateNum) == false)
                        { MessageBox.Show("Save memory error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                        if (readConfig(((UnitControl)sender).CONFIG_SETTINGS))
                        { MessageBox.Show("Save memory done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information); }
                        else
                        { MessageBox.Show("Config read error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                        break;
                    case ButtonCategory.RESET:
                        if (MessageBox.Show("Restore default settins."
                            , "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) { return; }
                        if (_io.serial.PCAB_PRESET(new PCAB_UnitInterface(strSN)))
                        {
                            MessageBox.Show("Preset done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            if (!readConfig(((UnitControl)sender).CONFIG_SETTINGS))
                            {
                                MessageBox.Show("Config read error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Reset error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CONFIG_CONFIG_SETTINGS_CheckboxClickEventHandler(object sender, RoutedEventArgs e, CheckBoxCategory category, bool? isChecked)
        {
            string strSN = ((Unit)sender).SerialNumber;
            PCAB _io = null;
            bool flg = false;
            foreach (PCAB io in _ioList)
            {
                foreach (PCAB_UnitInterface unit in io.serial.UNITs)
                {
                    if (unit.SerialNumberASCII == strSN) { _io = io; flg = true; break; }
                }
                if (flg) { break; }
            }
            bool ch;
            bool? result;
            if (isChecked == false) { ch = false; }
            else { ch = true; }
            try
            {
                switch (category)
                {
                    case CheckBoxCategory.StandbyAMP:
                        result = _io.serial.PCAB_SetSTB_AMP(new PCAB_UnitInterface(strSN), ch);
                        if (result != true) { ((Unit)sender).CHECKBOX_Indeterminate("STBAMP", null); }
                        break;
                    case CheckBoxCategory.StandbyDRA:
                        result = _io.serial.PCAB_SetSTB_DRA(new PCAB_UnitInterface(strSN), ch);
                        if (result != true) { ((Unit)sender).CHECKBOX_Indeterminate("STBDRA", null); }
                        break;
                    case CheckBoxCategory.StandbyLNA:
                        result = _io.serial.PCAB_SetSTB_LNA(new PCAB_UnitInterface(strSN), ch);
                        if (result != true) { ((Unit)sender).CHECKBOX_Indeterminate("STBLNA", null); }
                        break;
                    case CheckBoxCategory.LowPowerMode:
                        result = _io.serial.PCAB_SetLowPowerMode(new PCAB_UnitInterface(strSN), ch);
                        if (result != true) { ((Unit)sender).CHECKBOX_Indeterminate("LPM", null); }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnError(object sender, PCABEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                monitor?.WindowClose();
                foreach (PCAB _io in _ioList)
                {
                    _io.Close();
                }
                BOARD_GRID.Children.Clear();
                BOARD_GRID.Children.Add(new ControlTab());
                CONFIG_GRID.IsEnabled = true;
                BOARD_GRID.IsEnabled = false;
                CONNECT_BUTTON_CONTENT.Text = "Connect";
                CONFIG_EXPANDER.IsExpanded = true;
                _ioList = new List<PCAB>();
            }));
        }
        #endregion

        private bool readConfig(Unit conf)
        {
            try
            {
                foreach (PCAB _io in _ioList)
                {
                    foreach (PCAB_UnitInterface unit in _io.serial.UNITs)
                    {
                        if (unit.SerialNumberASCII == conf.SerialNumber)
                        {
                            List<uint> dsa = _io.serial.PCAB_GetDSA(new PCAB_UnitInterface(conf.SerialNumber));
                            int dsaIN = _io.serial.PCAB_GetDSAin(new PCAB_UnitInterface(conf.SerialNumber));
                            List<uint> dps = _io.serial.PCAB_GetDPS(new PCAB_UnitInterface(conf.SerialNumber));
                            if (dsa.Count != dps.Count || dsa.Count != 15) { return false; }
                            bool? result;
                            result = _io.serial.PCAB_GetSTB_AMP(new PCAB_UnitInterface(conf.SerialNumber));
                            switch (result)
                            {
                                case true:
                                    conf.CHECKBOX_Checked("STBAMP", null);
                                    break;
                                case false:
                                    conf.CHECKBOX_Unchecked("STBAMP", null);
                                    break;
                                default:
                                    conf.CHECKBOX_Indeterminate("STBAMP", null);
                                    break;
                            }
                            result = _io.serial.PCAB_GetSTB_DRA(new PCAB_UnitInterface(conf.SerialNumber));
                            switch (result)
                            {
                                case true:
                                    conf.CHECKBOX_Checked("STBDRA", null);
                                    break;
                                case false:
                                    conf.CHECKBOX_Unchecked("STBDRA", null);
                                    break;
                                default:
                                    conf.CHECKBOX_Indeterminate("STBDRA", null);
                                    break;
                            }
                            result = _io.serial.PCAB_GetLowPowerMode(new PCAB_UnitInterface(conf.SerialNumber));
                            switch (result)
                            {
                                case true:
                                    conf.CHECKBOX_Checked("LPM", null);
                                    break;
                                case false:
                                    conf.CHECKBOX_Unchecked("LPM", null);
                                    break;
                                default:
                                    conf.CHECKBOX_Indeterminate("LPM", null);
                                    break;
                            }
                            result = _io.serial.PCAB_GetSTB_LNA(new PCAB_UnitInterface(conf.SerialNumber));
                            switch (result)
                            {
                                case true:
                                    conf.CHECKBOX_Checked("STBLNA", null);
                                    break;
                                case false:
                                    conf.CHECKBOX_Unchecked("STBLNA", null);
                                    break;
                                default:
                                    conf.CHECKBOX_Indeterminate("STBLNA", null);
                                    break;
                            }
                            conf.ALL_DPS = false;
                            conf.ALL_DSA = false;
                            conf.SetDSA(0, (uint)dsaIN);
                            for (int i = 0; i < dsa.Count; i++)
                            {
                                conf.SetDSA((uint)(i + 1), dsa[i]);
                                conf.SetDPS((uint)(i + 1), dps[i]);
                            }
                            break;
                        }
                    }
                }
                return true;
            }
            catch { return false; }
        }

    }
}
