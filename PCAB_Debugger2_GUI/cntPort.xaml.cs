using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PCAB_Debugger2_GUI
{
    /// <summary>
    /// Port.xaml の相互作用ロジック
    /// </summary>
    public partial class Port : UserControl
    {
        private byte _portNum;
        private Color _bgColor;
        public byte PortNumber
        {
            get { return _portNum; }
            set { Label_PortName.Content = "Port " + (value + 1).ToString(); _portNum = value; }
        }
        public uint DSA
        {
            get { return (uint)ComboBox_DSA.SelectedIndex; }
            set { if (value < ComboBox_DSA.Items.Count) { ComboBox_DSA.SelectedIndex = (int)value; } }
        }
        public uint DPS
        {
            get { return (uint)ComboBox_DPS.SelectedIndex; }
            set { if (value < ComboBox_DPS.Items.Count) { ComboBox_DPS.SelectedIndex = (int)value; } }
        }
        public Brush BackgroundBrush { get; set; }

        public Port()
        {
            InitializeComponent();
            PortNumber = 0;
            DSA = 0;
            DPS = 0;
            this.DataContext = this;
        }
    }
}
