using PCAB_Debugger2_GUI.Properties;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static PCAB_Debugger2_GUI.IEEE488dot2;

namespace PCAB_Debugger2_GUI
{
    /// <summary>
    /// AutoMeasure.xaml の相互作用ロジック
    /// </summary>
    public partial class AutoMeasure : UserControl
    {
        private VisaResource sesn;
        public delegate void StartButtonClickEventHandler(object sender, RoutedEventArgs e, string dirPath);
        public event StartButtonClickEventHandler ButtonClickEvent;
        public VisaResource setResourceManager
        {
            get { return sesn; }
            set
            {
                if (value == null)
                {
                    VISA_CONFIG_GRID.IsEnabled = false;
                    INSTER_CONFIG_GRID.IsEnabled = false;
                    SAVE_CONFIG_GRID.IsEnabled = false;
                }
                else
                {
                    VISA_CONFIG_GRID.IsEnabled = true;
                    INSTER_CONFIG_GRID.IsEnabled = true;
                    SAVE_CONFIG_GRID.IsEnabled = true;
                }
                sesn = value;
            }
        }
        public string SerialNumber { get; private set; }

        public string VISA_Address { get { return VNALOOP_VISAADDR_TEXTBOX.Text; } }
        public uint VISA_Timeout { get { return uint.Parse(VNALOOP_TIMEOUT_TEXTBOX.Text); } }
        public string FileNameHeader { get { return VNALOOP_FILEHEADER_TEXTBOX.Text; } }
        public uint WaiteTime { get { return uint.Parse(VNALOOP_WAITTIME_TEXTBOX.Text); } }
        public bool? DPS_Enable { get { return DPS_VnaLoopEnable.IsChecked; } }
        public bool? DSA_Enable { get { return DSA_VnaLoopEnable.IsChecked; } }
        public bool? GetScreen_Enable { get { return VNALOOP_SCRE_CHECKBOX.IsChecked; } }
        public bool? GetTrace_Enable { get { return VNALOOP_TRA_CHECKBOX.IsChecked; } }
        public bool? SingleTrigger { get { return VNALOOP_SING_CHECKBOX.IsChecked; } }
        public int Channel
        {
            get
            {
                if (VNALOOP_CH_ALL.IsChecked == true) { return -1; }
                return int.Parse(VNALOOP_CHANNEL_COMBOBOX.Text);
            }
        }
        public uint DPS_Step { get { return (uint)Math.Pow(2, (double)VNALOOP_DPSstep_COMBOBOX.SelectedIndex); } }
        public uint DSA_Step { get { return (uint)Math.Pow(2, (double)VNALOOP_DSAstep_COMBOBOX.SelectedIndex); } }
        public List<uint> DPS
        {
            get
            {
                List<uint> dps = new List<uint>();
                dps.Clear();
                foreach (object objBF in DPS_VNALOOP_GRID.Children)
                {
                    if (typeof(CheckBox) == objBF.GetType())
                    {
                        if (((CheckBox)objBF).IsChecked == true)
                        {
                            dps.Add(uint.Parse(((CheckBox)objBF).Content.ToString().Substring(3)));
                        }
                    }
                }
                return dps;
            }
        }
        public List<uint> DSA
        {
            get
            {
                List<uint> dps = new List<uint>();
                dps.Clear();
                foreach (object objBF in DSA_VNALOOP_GRID.Children)
                {
                    if (typeof(CheckBox) == objBF.GetType())
                    {
                        if (((CheckBox)objBF).IsChecked == true)
                        {
                            dps.Add(uint.Parse(((CheckBox)objBF).Content.ToString().Substring(3)));
                        }
                    }
                }
                return dps;
            }
        }

        public AutoMeasure() : this("SN") { }
        public AutoMeasure(string SN)
        {
            InitializeComponent();
            SerialNumber = SN;
            VNALOOP_DPSstep_COMBOBOX.IsEnabled = false;
            VNALOOP_DSAstep_COMBOBOX.IsEnabled = false;
            DPS_VNALOOP_GRID.IsEnabled = false;
            DSA_VNALOOP_GRID.IsEnabled = false;
            VNALOOP_CONF_GRID.IsEnabled = false;
            VNALOOP_VISAADDR_TEXTBOX.Text = Settings.Default.visaAddr;
            VNALOOP_TIMEOUT_TEXTBOX.Text = Settings.Default.visaTO.ToString("0");
            VNALOOP_FILEHEADER_TEXTBOX.Text = Settings.Default.fnHeader;
        }

        private void VNALOOP_START_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            string dirPath = "";
            if (VNALOOP_SCRE_CHECKBOX.IsChecked == true ||
                VNALOOP_TRA_CHECKBOX.IsChecked == true)
            {
                using (System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog())
                {
                    fbd.Description = "Please Select Folder";
                    fbd.RootFolder = Environment.SpecialFolder.Desktop;
                    fbd.ShowNewFolderButton = true;
                    if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        return;
                    }
                    dirPath = fbd.SelectedPath;
                }
            }
            ButtonClickEvent?.Invoke(this, e, dirPath);
        }

        private void VNALOOP_VISA_CONNECT_CHECK_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            string strBF = VNALOOP_VISAADDR_TEXTBOX.Text;
            try
            {
                IEEE488dot2.Instrument instr;
                instr = new Instrument(new VisaResource(), strBF);
                instr.Open();
                instr.SetTimeout(int.Parse(VNALOOP_TIMEOUT_TEXTBOX.Text));
                IEEE488_IDN idn = instr.IDN();
                instr.Close();
                instr.Dispose();
                MessageBox.Show("Vender\t\t: " + idn.Vender +
                              "\nModel Number\t: " + idn.ModelNumber +
                              "\nRevision Code\t: " + idn.RevisionCode +
                              "\nSerial Number\t: " + idn.SerialNumber, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VNALOOP_CH_Click(object sender, RoutedEventArgs e)
        {
            if (((RadioButton)sender).Name == "VNALOOP_CH_ALL")
            {
                VNALOOP_CHANNEL_COMBOBOX.IsEnabled = false;
                VNALOOP_CH_SEL.IsChecked = false;
            }
            else
            {
                VNALOOP_CHANNEL_COMBOBOX.IsEnabled = true;
                VNALOOP_CH_ALL.IsChecked = false;
                VNALOOP_CHANNEL_COMBOBOX.Items.Clear();
                try
                {
                    agPNA835x pna = new agPNA835x(new Instrument(new VisaResource(), VNALOOP_VISAADDR_TEXTBOX.Text));
                    foreach (uint i in pna.getChannelCatalog())
                    {
                        VNALOOP_CHANNEL_COMBOBOX.Items.Add(i.ToString());
                    }
                    pna.Instrument.Dispose();
                    pna = null;
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    VNALOOP_CHANNEL_COMBOBOX.IsEnabled = false;
                    VNALOOP_CH_SEL.IsChecked = false;
                    VNALOOP_CH_ALL.IsChecked = true;
                }
            }
        }

        private void DPS_VnaLoopEnable_Checked(object sender, RoutedEventArgs e)
        {
            VNALOOP_DPSstep_COMBOBOX.IsEnabled = true;
            DPS_VNALOOP_GRID.IsEnabled = true;
            foreach (object objBF in DPS_VNALOOP_GRID.Children)
            {
                if (typeof(CheckBox) == objBF.GetType())
                {
                    if (((CheckBox)objBF).IsChecked == true)
                    {
                        VNALOOP_CONF_GRID.IsEnabled = true;
                    }
                }
            }
        }

        private void DPS_VnaLoopEnable_Unchecked(object sender, RoutedEventArgs e)
        {
            VNALOOP_DPSstep_COMBOBOX.IsEnabled = false;
            DPS_VNALOOP_GRID.IsEnabled = false;
            if (VNALOOP_SCRE_CHECKBOX.IsChecked != true &&
                VNALOOP_TRA_CHECKBOX.IsChecked != true)
            {
                if (DSA_VnaLoopEnable.IsChecked != true)
                {
                    VNALOOP_CONF_GRID.IsEnabled = false;
                }
                else
                {
                    foreach (object objBF in DSA_VNALOOP_GRID.Children)
                    {
                        if (typeof(CheckBox) == objBF.GetType())
                        {
                            if (((CheckBox)objBF).IsChecked == true)
                            {
                                return;
                            }
                        }
                    }
                    VNALOOP_CONF_GRID.IsEnabled = false;
                }
            }
        }

        private void DSA_VnaLoopEnable_Checked(object sender, RoutedEventArgs e)
        {
            VNALOOP_DSAstep_COMBOBOX.IsEnabled = true;
            DSA_VNALOOP_GRID.IsEnabled = true;
            foreach (object objBF in DSA_VNALOOP_GRID.Children)
            {
                if (typeof(CheckBox) == objBF.GetType())
                {
                    if (((CheckBox)objBF).IsChecked == true)
                    {
                        VNALOOP_CONF_GRID.IsEnabled = true;
                    }
                }
            }
        }

        private void DSA_VnaLoopEnable_Unchecked(object sender, RoutedEventArgs e)
        {
            VNALOOP_DSAstep_COMBOBOX.IsEnabled = false;
            DSA_VNALOOP_GRID.IsEnabled = false;
            if (VNALOOP_SCRE_CHECKBOX.IsChecked != true &&
                VNALOOP_TRA_CHECKBOX.IsChecked != true)
            {
                if (DPS_VnaLoopEnable.IsChecked != true)
                {
                    VNALOOP_CONF_GRID.IsEnabled = false;
                }
                else
                {
                    foreach (object objBF in DPS_VNALOOP_GRID.Children)
                    {
                        if (typeof(CheckBox) == objBF.GetType())
                        {
                            if (((CheckBox)objBF).IsChecked == true)
                            {
                                return;
                            }
                        }
                    }
                    VNALOOP_CONF_GRID.IsEnabled = false;
                }
            }
        }

        private void DPSn_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            VNALOOP_CONF_GRID.IsEnabled = true;
        }

        private void DPSn_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (object objBF in DPS_VNALOOP_GRID.Children)
            {
                if (typeof(CheckBox) == objBF.GetType())
                {
                    if (((CheckBox)objBF).IsChecked == true)
                    {
                        return;
                    }
                }
            }
            if (VNALOOP_SCRE_CHECKBOX.IsChecked != true &&
                VNALOOP_TRA_CHECKBOX.IsChecked != true)
            {
                if (DSA_VnaLoopEnable.IsChecked != true)
                {
                    VNALOOP_CONF_GRID.IsEnabled = false;
                }
                else
                {
                    foreach (object objBF in DSA_VNALOOP_GRID.Children)
                    {
                        if (typeof(CheckBox) == objBF.GetType())
                        {
                            if (((CheckBox)objBF).IsChecked == true)
                            {
                                return;
                            }
                        }
                    }
                    VNALOOP_CONF_GRID.IsEnabled = false;
                }
            }
        }

        private void DSAn_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            VNALOOP_CONF_GRID.IsEnabled = true;
        }

        private void DSAn_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (object objBF in DSA_VNALOOP_GRID.Children)
            {
                if (typeof(CheckBox) == objBF.GetType())
                {
                    if (((CheckBox)objBF).IsChecked == true)
                    {
                        return;
                    }
                }
            }
            if (VNALOOP_SCRE_CHECKBOX.IsChecked != true &&
                VNALOOP_TRA_CHECKBOX.IsChecked != true)
            {
                if (DPS_VnaLoopEnable.IsChecked != true)
                {
                    VNALOOP_CONF_GRID.IsEnabled = false;
                }
                else
                {
                    foreach (object objBF in DPS_VNALOOP_GRID.Children)
                    {
                        if (typeof(CheckBox) == objBF.GetType())
                        {
                            if (((CheckBox)objBF).IsChecked == true)
                            {
                                return;
                            }
                        }
                    }
                    VNALOOP_CONF_GRID.IsEnabled = false;
                }
            }
        }

        private void VNALOOP_SaveTarget_CHECKBOX_Checked(object sender, RoutedEventArgs e)
        {
            VNALOOP_CONF_GRID.IsEnabled = true;
        }

        private void VNALOOP_SaveTarget_CHECKBOX_Unchecked(object sender, RoutedEventArgs e)
        {
            if (VNALOOP_SCRE_CHECKBOX.IsChecked != true &&
                VNALOOP_TRA_CHECKBOX.IsChecked != true)
            {
                if (DPS_VnaLoopEnable.IsChecked == true)
                {
                    foreach (object objBF in DPS_VNALOOP_GRID.Children)
                    {
                        if (typeof(CheckBox) == objBF.GetType())
                        {
                            if (((CheckBox)objBF).IsChecked == true)
                            {
                                return;
                            }
                        }
                    }
                }
                if (DSA_VnaLoopEnable.IsChecked == true)
                {
                    foreach (object objBF in DSA_VNALOOP_GRID.Children)
                    {
                        if (typeof(CheckBox) == objBF.GetType())
                        {
                            if (((CheckBox)objBF).IsChecked == true)
                            {
                                return;
                            }
                        }
                    }
                }
                VNALOOP_CONF_GRID.IsEnabled = false;
            }
        }

        private void DPS_CHECK_ALL_Click(object sender, RoutedEventArgs e)
        {
            foreach (object objBF in DPS_VNALOOP_GRID.Children)
            {
                if (typeof(CheckBox) == objBF.GetType()) { ((CheckBox)objBF).IsChecked = true; }
            }
        }

        private void DPS_UNCHECK_ALL_Click(object sender, RoutedEventArgs e)
        {
            foreach (object objBF in DPS_VNALOOP_GRID.Children)
            {
                if (typeof(CheckBox) == objBF.GetType()) { ((CheckBox)objBF).IsChecked = false; }
            }
        }

        private void DSA_CHECK_ALL_Click(object sender, RoutedEventArgs e)
        {
            foreach (object objBF in DSA_VNALOOP_GRID.Children)
            {
                if (typeof(CheckBox) == objBF.GetType()) { ((CheckBox)objBF).IsChecked = true; }
            }
        }

        private void DSA_UNCHECK_ALL_Click(object sender, RoutedEventArgs e)
        {
            foreach (object objBF in DSA_VNALOOP_GRID.Children)
            {
                if (typeof(CheckBox) == objBF.GetType()) { ((CheckBox)objBF).IsChecked = false; }
            }
        }

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
    }
}
