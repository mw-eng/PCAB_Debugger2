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
    /// cntUnit.xaml の相互作用ロジック
    /// </summary>
    public partial class Unit : UserControl
    {
        public enum CheckBoxCategory
        {
            StandbyAMP,
            StandbyDRA,
            LowPowerMode,
            StandbyLNA,
            NULL
        }

        public delegate void CheckboxClickEventHandler(object sender, RoutedEventArgs e, CheckBoxCategory category, bool? isChecked);
        public event CheckboxClickEventHandler CheckboxClickEvent;
        public bool? StandbyAMP
        {
            get { return CheckBox_STBAMP.IsChecked; }
            set { CHECKBOX_Changed("STBAMP", null, value); }
        }
        public bool? StandbyDRA
        {
            get { return CheckBox_STBDRA.IsChecked; }
            set { CHECKBOX_Changed("STBDRA", null, value); }
        }
        public bool? LowPowerMode
        {
            get { return CheckBox_SETLPM.IsChecked; }
            set { CHECKBOX_Changed("LPM", null, value); }
        }
        public bool? StandbyLNA
        {
            get { return CONFIG_PORTS.StandbyLNA; }
            set { CONFIG_PORTS.StandbyLNA = value; }
        }

        public void SetDSA(uint number, uint value) { CONFIG_PORTS.SetDSA(number, value); }
        public int GetDSA(uint number) { return CONFIG_PORTS.GetDSA(number); }
        public void SetDPS(uint number, uint value) { CONFIG_PORTS.SetDPS(number, value); }
        public int GetDPS(uint number) { return CONFIG_PORTS.GetDPS(number); }
        public string SerialNumber { get; private set; }

        public Ports CONFIG_PORTS { get; private set; }

        public bool? ALL_DPS { get { return CONFIG_PORTS.CheckBox_ALL_DPS.IsChecked; } set { CONFIG_PORTS.CheckBox_ALL_DPS.IsChecked = value; } }
        public bool? ALL_DSA { get { return CONFIG_PORTS.CheckBox_ALL_DSA.IsChecked; } set { CONFIG_PORTS.CheckBox_ALL_DSA.IsChecked = value; } }

        public Unit() : this("SN", ROTATE.ZERO) { }
        public Unit(string serialNumber, ROTATE _turn)
        {
            InitializeComponent();
            CONFIG_PORTS = new Ports(serialNumber, _turn);
            Viewbox_Ports.Child = CONFIG_PORTS;
            CONFIG_PORTS.STBLNA_CheckboxClickEvent += STBLNA_CheckboxClickEvent;
            SerialNumber = serialNumber;
            CONFIG_PORTS.TURN = _turn;
            Combobox_VIEW.SelectedIndex = (int)CONFIG_PORTS.TURN;
        }

        public void CHECKBOX_Checked(object sender, RoutedEventArgs e)
        {
            CHECKBOX_Changed(sender, e, true);
        }

        public void CHECKBOX_Unchecked(object sender, RoutedEventArgs e)
        {
            CHECKBOX_Changed(sender, e, false);
        }

        public void CHECKBOX_Indeterminate(object sender, RoutedEventArgs e)
        {
            CHECKBOX_Changed(sender, e, null);
        }

        private void CHECKBOX_Changed(object sender, RoutedEventArgs e, bool? isChecked)
        {
            if (typeof(CheckBox) == sender.GetType())
            {
                CheckBoxCategory cat = CheckBoxCategory.NULL;
                switch (((CheckBox)sender).Name)
                {
                    case "CheckBox_STBAMP": cat = CheckBoxCategory.StandbyAMP; break;
                    case "CheckBox_STBDRA": cat = CheckBoxCategory.StandbyDRA; break;
                    case "CheckBox_STBLNA": cat = CheckBoxCategory.StandbyLNA; break;
                    case "CheckBox_SETLPM": cat = CheckBoxCategory.LowPowerMode; break;
                }
                CheckboxClickEvent?.Invoke(this, e, cat, null);
            }
            else
            {
                switch ((string)sender)
                {
                    case "STBAMP": CheckBox_STBAMP.IsChecked = isChecked; break;
                    case "STBDRA": CheckBox_STBDRA.IsChecked = isChecked; break;
                    case "STBLNA": StandbyLNA = isChecked; break;
                    case "LPM": CheckBox_SETLPM.IsChecked = isChecked; break;
                    default: break;
                }
            }
        }

        private void VIEW_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {
            if (Combobox_VIEW.SelectedIndex != (int)CONFIG_PORTS.TURN) { CONFIG_PORTS.TURN = (ROTATE)Combobox_VIEW.SelectedIndex; }
        }

        private void STBLNA_CheckboxClickEvent(object sender, RoutedEventArgs e, bool? isChecked)
        {
            CheckboxClickEvent?.Invoke(this, e, CheckBoxCategory.StandbyLNA, isChecked);
        }

    }
}
