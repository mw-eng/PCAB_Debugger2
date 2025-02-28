using System.Windows.Controls;
using static PCAB_Debugger2_GUI.Ports;

namespace PCAB_Debugger2_GUI
{
    /// <summary>
    /// ControlTab.xaml の相互作用ロジック
    /// </summary>
    public partial class ControlTab : UserControl
    {
        public string SerialNumber { get; private set; }
        public UnitControl CONFIG { get; private set; }
        public AutoMeasure AUTO { get; private set; }
        public ControlTab() : this("SN", 0) { }
        public ControlTab(string _serialNumber, ROTATE _rotate)
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
