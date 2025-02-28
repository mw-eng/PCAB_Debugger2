using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PCAB_Debugger2_GUI
{
    public class clsAsyncButton : Button
    {
        public clsAsyncButton() { this.Click += Button_Click; }
        volatile bool isProcessing;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (isProcessing)
            {
                e.Handled = true;
                return;
            }

            isProcessing = true;

            this.Dispatcher.BeginInvoke(new Action(() => {
                isProcessing = false;

            }), DispatcherPriority.ApplicationIdle);
        }

    }
}
