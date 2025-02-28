using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace PCAB_Debugger2_GUI
{
    /// <summary>
    /// Port.xaml の相互作用ロジック
    /// </summary>
    public partial class Port : UserControl, INotifyPropertyChanged
    {
        private byte _portNum;

        /// <summary>ポート番号(0-15)</summary>
        public byte PortNumber
        {
            get { return _portNum; }
            set { Label_PortName.Content = "Port " + (value + 1).ToString(); _portNum = value; }
        }
        private bool _dsaIsEnabled = true;
        /// <summary>DSA設定変更有効状態</summary>
        public bool DSA_IsEnabled
        {
            get { return _dsaIsEnabled; }
            set
            {
                if (_dsaIsEnabled != value)
                {
                    _dsaIsEnabled = value;
                    OnPropertyChanged(nameof(DSA_IsEnabled));
                }
            }
        }
        private bool _dpsIsEnabled = true;
        /// <summary>DPS設定変更有効状態</summary>
        public bool DPS_IsEnabled
        {
            get { return _dpsIsEnabled; }
            set
            {
                if (_dpsIsEnabled != value)
                {
                    _dpsIsEnabled = value;
                    OnPropertyChanged(nameof(DPS_IsEnabled));
                }
            }
        }

        /// <summary>DSA設定値</summary>
        public uint DSA
        {
            get { return (uint)ComboBox_DSA.SelectedIndex; }
            set
            {
                if (value < ComboBox_DSA.Items.Count) { ComboBox_DSA.SelectedIndex = (int)value; }
                else { throw new ArgumentException("A value out of range was specified.\nVALUE > " + value.ToString()); }
            }
        }
        /// <summary>DPS設定値</summary>
        public uint DPS
        {
            get { return (uint)ComboBox_DPS.SelectedIndex; }
            set
            {
                if (value < ComboBox_DPS.Items.Count) { ComboBox_DPS.SelectedIndex = (int)value; }
                else { throw new ArgumentException("A value out of range was specified.\nVALUE > " + value.ToString()); }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public Port()
        {
            InitializeComponent();
            PortNumber = 0;
            DSA = 0;
            DPS = 0;
            DataContext = this;
        }
    }
}
