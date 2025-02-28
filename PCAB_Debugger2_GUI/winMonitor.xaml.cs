using PCAB_Debugger2_GUI.Properties;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace PCAB_Debugger2_GUI
{
    /// <summary>
    /// winMonitor.xaml の相互作用ロジック
    /// </summary>
    public partial class winMonitor : Window
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        private static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);
        private const int SC_CLOSE = 0xf060;
        private const int MF_BYCOMMAND = 0x0000;

        public bool closeFLG = false;
        public winMonitor()
        {
            InitializeComponent();
            if (Settings.Default.winMonitorTop >= SystemParameters.VirtualScreenTop &&
                (Settings.Default.winMonitorTop + Settings.Default.winMonitorHeight) <
                SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight)
            {
                this.Top = Settings.Default.winMonitorTop;
            }
            if (Settings.Default.winMonitorLeft >= SystemParameters.VirtualScreenLeft &&
                (Settings.Default.winMonitorLeft + Settings.Default.winMonitorWidth) <
                SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth)
            {
                this.Left = Settings.Default.winMonitorLeft;
            }
            if (Settings.Default.winMonitorWidth > 0 &&
                Settings.Default.winMonitorWidth < SystemParameters.WorkArea.Width)
            {
                this.Width = Settings.Default.winMonitorWidth;
            }
            if (Settings.Default.winMonitorHeight > 0 &&
                Settings.Default.winMonitorHeight < SystemParameters.WorkArea.Height)
            {
                this.Height = Settings.Default.winMonitorHeight;
            }
            if (Settings.Default.winMonitorMaximized)
            { Loaded += (o, e) => this.WindowState = WindowState.Maximized; }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper((Window)sender).Handle;
            IntPtr hMenu = GetSystemMenu(hwnd, false);
            RemoveMenu(hMenu, SC_CLOSE, MF_BYCOMMAND);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!closeFLG) { e.Cancel = true; }
        }

        public void WindowClose()
        {
            Settings.Default.winMonitorTop = this.Top;
            Settings.Default.winMonitorLeft = this.Left;
            Settings.Default.winMonitorHeight = this.Height;
            Settings.Default.winMonitorWidth = this.Width;
            Settings.Default.winMonitorMaximized = this.WindowState == WindowState.Maximized;
            this.WindowState = WindowState.Normal;
            closeFLG = true;
            Close();
        }
    }
}
