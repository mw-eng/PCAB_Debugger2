using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static PCAB_Debugger2_GUI.PCAB_TASK;

namespace PCAB_Debugger2_GUI
{
    /// <summary>
    /// winEditor.xaml の相互作用ロジック
    /// </summary>
    public partial class winEditor : Window
    {
        public class BindableBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
            {
                if (EqualityComparer<T>.Default.Equals(field, value)) { return false; }

                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
        }
        private class binaryROW : BindableBase
        {
            private string _addr;
            private string _dat00;
            private string _dat01;
            private string _dat02;
            private string _dat03;
            private string _dat04;
            private string _dat05;
            private string _dat06;
            private string _dat07;
            private string _dat08;
            private string _dat09;
            private string _dat0A;
            private string _dat0B;
            private string _dat0C;
            private string _dat0D;
            private string _dat0E;
            private string _dat0F;
            private string _datSTR;

            public string addr { get => _addr; set => SetProperty(ref _addr, value); }
            public string dat00 { get => _dat00; set => SetProperty(ref _dat00, value); }
            public string dat01 { get => _dat01; set => SetProperty(ref _dat01, value); }
            public string dat02 { get => _dat02; set => SetProperty(ref _dat02, value); }
            public string dat03 { get => _dat03; set => SetProperty(ref _dat03, value); }
            public string dat04 { get => _dat04; set => SetProperty(ref _dat04, value); }
            public string dat05 { get => _dat05; set => SetProperty(ref _dat05, value); }
            public string dat06 { get => _dat06; set => SetProperty(ref _dat06, value); }
            public string dat07 { get => _dat07; set => SetProperty(ref _dat07, value); }
            public string dat08 { get => _dat08; set => SetProperty(ref _dat08, value); }
            public string dat09 { get => _dat09; set => SetProperty(ref _dat09, value); }
            public string dat0A { get => _dat0A; set => SetProperty(ref _dat0A, value); }
            public string dat0B { get => _dat0B; set => SetProperty(ref _dat0B, value); }
            public string dat0C { get => _dat0C; set => SetProperty(ref _dat0C, value); }
            public string dat0D { get => _dat0D; set => SetProperty(ref _dat0D, value); }
            public string dat0E { get => _dat0E; set => SetProperty(ref _dat0E, value); }
            public string dat0F { get => _dat0F; set => SetProperty(ref _dat0F, value); }
            public string datSTR { get => _datSTR; set => SetProperty(ref _datSTR, value); }
            public string datLINE { get => _dat00 + _dat01 + _dat02 + _dat03 + _dat04 + _dat05 + _dat06 + _dat07 + _dat08 + _dat09 + _dat0A + _dat0B + _dat0C + _dat0D + _dat0E + _dat0F; }
            public List<byte> datLIST { get => new List<byte> { 
                Convert.ToByte(_dat00, 16),
                Convert.ToByte(_dat01, 16),
                Convert.ToByte(_dat02, 16),
                Convert.ToByte(_dat03, 16),
                Convert.ToByte(_dat04, 16),
                Convert.ToByte(_dat05, 16),
                Convert.ToByte(_dat06, 16),
                Convert.ToByte(_dat07, 16),
                Convert.ToByte(_dat08, 16),
                Convert.ToByte(_dat09, 16),
                Convert.ToByte(_dat0A, 16),
                Convert.ToByte(_dat0B, 16),
                Convert.ToByte(_dat0C, 16),
                Convert.ToByte(_dat0D, 16),
                Convert.ToByte(_dat0E, 16),
                Convert.ToByte(_dat0F, 16) }; }
             
            public binaryROW(UInt32 line, byte dat00, byte dat01, byte dat02, byte dat03, byte dat04, byte dat05, byte dat06, byte dat07, byte dat08, byte dat09, byte dat0A, byte dat0B, byte dat0C, byte dat0D, byte dat0E, byte dat0F)
            {
                _addr = (16 * line).ToString("X8");
                _dat00 = dat00.ToString("X2");
                _dat01 = dat01.ToString("X2");
                _dat02 = dat02.ToString("X2");
                _dat03 = dat03.ToString("X2");
                _dat04 = dat04.ToString("X2");
                _dat05 = dat05.ToString("X2");
                _dat06 = dat06.ToString("X2");
                _dat07 = dat07.ToString("X2");
                _dat08 = dat08.ToString("X2");
                _dat09 = dat09.ToString("X2");
                _dat0A = dat0A.ToString("X2");
                _dat0B = dat0B.ToString("X2");
                _dat0C = dat0C.ToString("X2");
                _dat0D = dat0D.ToString("X2");
                _dat0E = dat0E.ToString("X2");
                _dat0F = dat0F.ToString("X2");
                _datSTR = "";
                if (32 <= dat00 && dat00 <= 126) { datSTR += (char)dat00; } else { datSTR += "."; }
                if (32 <= dat01 && dat01 <= 126) { datSTR += (char)dat01; } else { datSTR += "."; }
                if (32 <= dat02 && dat02 <= 126) { datSTR += (char)dat02; } else { datSTR += "."; }
                if (32 <= dat03 && dat03 <= 126) { datSTR += (char)dat03; } else { datSTR += "."; }
                if (32 <= dat04 && dat04 <= 126) { datSTR += (char)dat04; } else { datSTR += "."; }
                if (32 <= dat05 && dat05 <= 126) { datSTR += (char)dat05; } else { datSTR += "."; }
                if (32 <= dat06 && dat06 <= 126) { datSTR += (char)dat06; } else { datSTR += "."; }
                if (32 <= dat07 && dat07 <= 126) { datSTR += (char)dat07; } else { datSTR += "."; }
                if (32 <= dat08 && dat08 <= 126) { datSTR += (char)dat08; } else { datSTR += "."; }
                if (32 <= dat09 && dat09 <= 126) { datSTR += (char)dat09; } else { datSTR += "."; }
                if (32 <= dat0A && dat0A <= 126) { datSTR += (char)dat0A; } else { datSTR += "."; }
                if (32 <= dat0B && dat0B <= 126) { datSTR += (char)dat0B; } else { datSTR += "."; }
                if (32 <= dat0C && dat0C <= 126) { datSTR += (char)dat0C; } else { datSTR += "."; }
                if (32 <= dat0D && dat0D <= 126) { datSTR += (char)dat0D; } else { datSTR += "."; }
                if (32 <= dat0E && dat0E <= 126) { datSTR += (char)dat0E; } else { datSTR += "."; }
                if (32 <= dat0F && dat0F <= 126) { datSTR += (char)dat0F; } else { datSTR += "."; }
            }
            public binaryROW Copy()
            {
                return (binaryROW)MemberwiseClone();
            }
        }
        private ObservableCollection<binaryROW> dataTableNOW = new ObservableCollection<binaryROW>();
        private ObservableCollection<binaryROW> dataTable = new ObservableCollection<binaryROW>();
        private PCAB_SerialInterface.PCAB_UnitInterface serialNum;
        private PCAB_TASK _serial;

        public winEditor(PCAB_TASK serial, PCAB_SerialInterface.PCAB_UnitInterface SN)
        {
            InitializeComponent();
            _serial = serial;
            serialNum = SN;
            _serial.OnTaskError += OnError;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UInt32 block = 1;
            List<byte> datBF = _serial.PCAB_ReadROM(serialNum, block, 0);
            while (datBF != null)
            {
                block *= 2u;
                datBF = _serial.PCAB_ReadROM(serialNum, block, 0);
            }
            block--;
            datBF = _serial.PCAB_ReadROM(serialNum, block, 0);
            BLOCK_COMBOBOX.Items.Clear();
            if (datBF != null)
            {
                for(UInt32 cnt = 0; cnt <= block; cnt++) { BLOCK_COMBOBOX.Items.Add(cnt.ToString("X4")); }
            }
            else
            {
                MessageBox.Show("Failed to load ROM", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            SECTOR_COMBOBOX.Items.Clear();
            for(uint i = 0; i < 0x10u; i++) { SECTOR_COMBOBOX.Items.Add((i * 0x10u).ToString("X2")); }
            BLOCK_COMBOBOX.SelectedIndex = BLOCK_COMBOBOX.Items.Count - 1;
            SECTOR_COMBOBOX.SelectedIndex = SECTOR_COMBOBOX.Items.Count - 1;
            reload();
        }

        private void BINARY_DATAGRID_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(TextBox))
            {
                int cnt = (((TextBox)e.OriginalSource).Text + e.Text).Length - ((TextBox)e.OriginalSource).SelectionLength;
                if (2 >= cnt && new Regex("[0-9a-fA-F]").IsMatch(e.Text)) { e.Handled = false; ((TextBox)e.OriginalSource).CharacterCasing = CharacterCasing.Upper; }
                else
                {
                    e.Handled = true;
                }
            }
        }

        private void BINARY_DATAGRID_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(TextBox))
            {
                if (((TextBox)e.OriginalSource).Text.Length == 0) { ((TextBox)e.OriginalSource).Text = "00"; }
                else if (((TextBox)e.OriginalSource).Text.Length == 1) { ((TextBox)e.OriginalSource).Text = "0" + ((TextBox)e.OriginalSource).Text; }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _serial.OnTaskError -= OnError;
        }

        private void OnError(object sender, PCABEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.Close();
            }));
        }

        private void RELOAD_Click(object sender, RoutedEventArgs e)
        {
            if (reload()) { MessageBox.Show("Rom reload completed.", "Information", MessageBoxButton.OK, MessageBoxImage.Information); }
            else { MessageBox.Show("Rom reload failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void WRITE_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to overwrite the ROM data at Address:" + BLOCK_COMBOBOX.Text + "-" + SECTOR_COMBOBOX.Text + "?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) { return; }

            string strBF = BLOCK_COMBOBOX.Text.Trim();
            UInt32 uiBF;
            if (!UInt32.TryParse(strBF.StartsWith("0x") ? strBF.Trim().Substring(2) : strBF, System.Globalization.NumberStyles.HexNumber, null, out uiBF))
            { MessageBox.Show("Failed write rom.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            UInt32 block = uiBF;
            strBF = SECTOR_COMBOBOX.Text.Trim();
            if (!uint.TryParse(strBF.StartsWith("0x") ? strBF.Trim().Substring(2) : strBF, System.Globalization.NumberStyles.HexNumber, null, out uiBF))
            { MessageBox.Show("Failed write rom.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            byte sector = (byte)((uiBF & 0xF0) >> 4);
            List<byte> datBF = new List<byte>();
            for (int i = 0; i < dataTable.Count; i += 0x10)
            {
                for (int j = 0; j < 0x10u; j++)
                {
                    datBF.AddRange(dataTable[i + j].datLIST);
                }
            }
            if (_serial.PCAB_OverWriteROM(serialNum, block, sector, datBF) == true)
            { MessageBox.Show("Success write rom.", "Information", MessageBoxButton.OK, MessageBoxImage.Information); }
            else { MessageBox.Show("Failed write rom.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void RESET_Click(object sender, RoutedEventArgs e)
        {
            dataTable.Clear();
            foreach (binaryROW row in dataTableNOW) { dataTable.Add(row.Copy()); }
            BINARY_DATAGRID.Items.Refresh();
        }

        private void BLOCK_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {

        }

        private void SECTOR_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {

        }

        private bool reload()
        {
            dataTable.Clear();
            dataTableNOW.Clear();
            string strBF = BLOCK_COMBOBOX.Text.Trim();
            UInt32 uiBF;
            if (!UInt32.TryParse(strBF.StartsWith("0x") ? strBF.Trim().Substring(2) : strBF, System.Globalization.NumberStyles.HexNumber, null, out uiBF)) { return false; }
            UInt32 block = uiBF;
            strBF = SECTOR_COMBOBOX.Text.Trim();
            if (!uint.TryParse(strBF.StartsWith("0x") ? strBF.Trim().Substring(2) : strBF, System.Globalization.NumberStyles.HexNumber, null, out uiBF)) { return false; }
            byte sector = (byte)((uiBF & 0xF0) >> 4);
            List<byte> datBF = _serial.PCAB_ReadROM(serialNum, block, sector);
            if(datBF == null) { return false; }
            for(int cnt = 0; cnt < datBF.Count; cnt += 16)
            {
                if(datBF.Count < cnt + 16) { break; }
                dataTable.Add(new binaryROW((UInt32)(block * 16u * 16u * 16u + sector * 16u * 16u + cnt / 16u),
                    datBF[(int)cnt + 0],
                    datBF[cnt + 1],
                    datBF[cnt + 2],
                    datBF[cnt + 3],
                    datBF[cnt + 4],
                    datBF[cnt + 5],
                    datBF[cnt + 6],
                    datBF[cnt + 7],
                    datBF[cnt + 8],
                    datBF[cnt + 9],
                    datBF[cnt + 10],
                    datBF[cnt + 11],
                    datBF[cnt + 12],
                    datBF[cnt + 13],
                    datBF[cnt + 14],
                    datBF[cnt + 15]
                    ));
            }
            foreach (binaryROW row in dataTable) { dataTableNOW.Add(row.Copy()); }
            BINARY_DATAGRID.ItemsSource = dataTable;
            foreach (DataGridColumn col in BINARY_DATAGRID.Columns) { col.MinWidth = col.ActualWidth; }
            return true;
        }

    }
}
