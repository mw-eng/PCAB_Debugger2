using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static PCAB_Debugger2_GUI.CommandControl;
using static PCAB_Debugger2_GUI.Ports;

namespace PCAB_Debugger2_GUI
{
    /// <summary>
    /// UnitControl.xaml の相互作用ロジック
    /// </summary>
    public partial class UnitControl : UserControl
    {
        public enum ButtonCategory
        {
            LOADMEM,
            SAVEMEM,
            RESET,
            WRITEDSA,
            WRITEDPS,
            WRITE,
            READ,
            NULL
        }
        public delegate void ButtonClickEventHandler(object sender, RoutedEventArgs e, ButtonCategory category);
        public event ButtonClickEventHandler ButtonClickEvent;
        public string SerialNumber { get; private set; }
        public Unit CONFIG_SETTINGS { get; private set; }

        public string MemoeryTargetAddress { get { return SAVEADDRESS_COMBOBOX.Text; } }

        public UnitControl() : this("SN", 0) { }
        public UnitControl(string _serialNumber, ROTATE _rotate)
        {
            InitializeComponent();
            SerialNumber = _serialNumber;
            CONFIG_SETTINGS = new Unit(_serialNumber, _rotate);
            Grid_UNIT.Children.Clear();
            Grid_UNIT.Children.Add(CONFIG_SETTINGS);
            CommandControl cmd = new CommandControl();
            cmd.CommandEvent += CommandEvent;
            this.DataContext = cmd;

        }

        private void SAVEADDRESS_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {
            if (SAVEADDRESS_COMBOBOX.SelectedIndex < 0) { SAVEADDRESS_COMBOBOX.SelectedIndex = 0; }
        }

        private void LOADMEM_Click(object sender, RoutedEventArgs e)
        {
            ButtonClickEvent?.Invoke(this, e, ButtonCategory.LOADMEM);
        }

        private void SAVEMEM_Click(object sender, RoutedEventArgs e)
        {
            ButtonClickEvent?.Invoke(this, e, ButtonCategory.SAVEMEM);
        }

        private void RESET_Click(object sender, RoutedEventArgs e)
        {
            ButtonClickEvent?.Invoke(this, e, ButtonCategory.RESET);
        }

        private void WRITE_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                Button btn = (Button)sender;
                switch (btn.Name)
                {
                    case "WRITEDSA":
                        ButtonClickEvent?.Invoke(this, e, ButtonCategory.WRITEDSA);
                        break;
                    case "WRITEDPS":
                        ButtonClickEvent?.Invoke(this, e, ButtonCategory.WRITEDPS);
                        break;
                    case "WRITE":
                        ButtonClickEvent?.Invoke(this, e, ButtonCategory.WRITE);
                        break;
                }
            }
        }

        private void READ_Click(object sender, RoutedEventArgs e)
        {
            ButtonClickEvent?.Invoke(this, e, ButtonCategory.READ);
        }

        private void CommandEvent(object sender, CommandCode code)
        {
            switch (code)
            {
                case CommandCode.SetATT:
                    ButtonClickEvent?.Invoke(this, null, ButtonCategory.WRITEDSA);
                    break;
                case CommandCode.SetPhase:
                    ButtonClickEvent?.Invoke(this, null, ButtonCategory.WRITEDPS);
                    break;
                case CommandCode.SetConfig:
                    ButtonClickEvent?.Invoke(this, null, ButtonCategory.WRITE);
                    break;
                case CommandCode.ReadConfig:
                    ButtonClickEvent?.Invoke(this, null, ButtonCategory.READ);
                    break;
            }
        }
    }

    public class CommandControl
    {
        public delegate void CommandEventHandler(object sender, CommandCode code);
        public event CommandEventHandler CommandEvent;
        public class Command : ICommand
        {
            public bool CanExecute(object parameter) { return true; }
            public event EventHandler CanExecuteChanged;
            public event CommandEventHandler CommandEvent;
            private CommandCode _code;
            public Command(CommandCode code) { _code = code; }
            public void Execute(object parameter)
            {
                CommandEvent?.Invoke(this, _code);
            }

        }

        public Command SetATTCommand { get; private set; }
        public Command SetPhaseCommand { get; private set; }
        public Command SetConfigCommand { get; private set; }
        public Command ReadConfigCommand { get; private set; }
        public CommandControl()
        {
            this.SetATTCommand = new Command(CommandCode.SetATT);
            this.SetATTCommand.CommandEvent += cmdEvent;
            this.SetPhaseCommand = new Command(CommandCode.SetPhase);
            this.SetPhaseCommand.CommandEvent += cmdEvent;
            this.SetConfigCommand = new Command(CommandCode.SetConfig);
            this.SetConfigCommand.CommandEvent += cmdEvent;
            this.ReadConfigCommand = new Command(CommandCode.ReadConfig);
            this.ReadConfigCommand.CommandEvent += cmdEvent;
        }
        private void cmdEvent(object sender, CommandCode code)
        {
            CommandEvent?.Invoke(this, code);
        }
        public enum CommandCode
        {
            SetATT,
            SetPhase,
            SetConfig,
            ReadConfig
        }
    }
}
