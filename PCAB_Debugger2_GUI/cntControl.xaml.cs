using System;
using System.Collections.Generic;
using System.Linq;
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
using static PCAB_Debugger2_GUI.Ports;

namespace PCAB_Debugger2_GUI
{
    /// <summary>
    /// Control.xaml の相互作用ロジック
    /// </summary>
    public partial class Control : UserControl
    {
        public string SerialNumber { get; private set; }
        public UnitControl CONFIG { get; private set; }
        public AutoMeasure AUTO { get; private set; }
        public Control() : this("SN", 0) { }
        public Control(string _serialNumber, ROTATE _rotate)
        {
            InitializeComponent();
            TAB_CONTROL.Items.Clear();
            CONFIG = new UnitControl(_serialNumber, _rotate);
            AUTO = new AutoMeasure(_serialNumber);
            SerialNumber = _serialNumber;
            TabItem control = new TabItem();
            TabItem auto = new TabItem();
            control.Header = "CONTROL";
            auto.Header = "ATUO";
            control.Name = "CONFIG";
            auto.Name = "CONFIG";
            control.Content = CONFIG;
            auto.Content = AUTO;
            TAB_CONTROL.Items.Add(control);
            TAB_CONTROL.Items.Add(auto);
        }
    }
}
