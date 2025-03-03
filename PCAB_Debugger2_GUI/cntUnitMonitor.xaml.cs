using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PCAB_Debugger2_GUI
{
    /// <summary>
    /// UnitMonitor.xaml の相互作用ロジック
    /// </summary>
    public partial class UnitMonitor : UserControl
    {
        private const float maxTMP = 55.0f;
        private const float minTMP = 20.0f;
        private const float maxVin = 40.0f;
        private const float minVin = 26.0f;
        private const float maxPin = 1.1f;
        private const float minPin = 0.9f;
        private const float maxVd = 26.0f;
        private const float minVd = 10.0f;
        private const float maxId = 4.0f;
        private const float minId = 0.2f;
        private SolidColorBrush maxColor = new SolidColorBrush(Color.FromScRgb(100, 255, 0, 0));
        private SolidColorBrush minColor = new SolidColorBrush(Color.FromScRgb(100, 0, 255, 255));
        private SolidColorBrush normColor = null;
        private SolidColorBrush errColor = new SolidColorBrush(Color.FromScRgb(100, 255, 255, 0));

        #region Property
        public string SerialNumber { get; private set; }
        public string TEMPcpu
        {
            get { return SNS_CPU_TEMP_LABEL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    SNS_CPU_TEMP_LABEL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { SNS_CPU_TEMP_LABEL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { SNS_CPU_TEMP_LABEL.Background = minColor; }
                        else { SNS_CPU_TEMP_LABEL.Background = normColor; }
                    }
                    catch { SNS_CPU_TEMP_LABEL.Background = errColor; }
                }));
            }
        }
        public string SNSvin
        {
            get { return SNS_VIN_LABEL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    SNS_VIN_LABEL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxVin) { SNS_VIN_LABEL.Background = maxColor; }
                        else if (float.Parse(value) < minVin) { SNS_VIN_LABEL.Background = minColor; }
                        else { SNS_VIN_LABEL.Background = normColor; }
                    }
                    catch { SNS_VIN_LABEL.Background = errColor; }
                }));
            }
        }
        public string SNSpin
        {
            get { return SNS_PIN_LABEL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    SNS_PIN_LABEL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxPin) { SNS_PIN_LABEL.Background = maxColor; }
                        else if (float.Parse(value) < minPin) { SNS_PIN_LABEL.Background = minColor; }
                        else { SNS_PIN_LABEL.Background = normColor; }
                    }
                    catch { SNS_PIN_LABEL.Background = errColor; }
                }));
            }
        }
        public string SNSvd
        {
            get { return SNS_VD_LABEL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    SNS_VD_LABEL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxVd) { SNS_VD_LABEL.Background = maxColor; }
                        else if (float.Parse(value) < minVd) { SNS_VD_LABEL.Background = minColor; }
                        else { SNS_VD_LABEL.Background = normColor; }
                    }
                    catch { SNS_VD_LABEL.Background = errColor; }
                }));
            }
        }
        public string SNSid
        {
            get { return SNS_ID_LABEL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    SNS_ID_LABEL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxId) { SNS_ID_LABEL.Background = maxColor; }
                        else if (float.Parse(value) < minId) { SNS_ID_LABEL.Background = minColor; }
                        else { SNS_ID_LABEL.Background = normColor; }
                    }
                    catch { SNS_ID_LABEL.Background = errColor; }
                }));
            }
        }
        public string TEMPavg
        {
            get { return TMP_AVG_LABEL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    TMP_AVG_LABEL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { TMP_AVG_LABEL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { TMP_AVG_LABEL.Background = minColor; }
                        else { TMP_AVG_LABEL.Background = normColor; }
                    }
                    catch { TMP_AVG_LABEL.Background = errColor; }
                }));
            }
        }
        public string TEMPmax
        {
            get { return TMP_MAX_LABEL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    TMP_MAX_LABEL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { TMP_MAX_LABEL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { TMP_MAX_LABEL.Background = minColor; }
                        else { TMP_MAX_LABEL.Background = normColor; }
                    }
                    catch { TMP_MAX_LABEL.Background = errColor; }
                }));
            }
        }
        public string TEMPmin
        {
            get { return TMP_MIN_LABEL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    TMP_MIN_LABEL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { TMP_MIN_LABEL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { TMP_MIN_LABEL.Background = minColor; }
                        else { TMP_MIN_LABEL.Background = normColor; }
                    }
                    catch { TMP_MIN_LABEL.Background = errColor; }
                }));
            }
        }
        public string TEMP01ID
        {
            get { return TEMP01CODE.Content.ToString(); }
            set { Dispatcher.BeginInvoke(new Action(() => { TEMP01CODE.Content = value; })); }
        }
        public string TEMP02ID
        {
            get { return TEMP02CODE.Content.ToString(); }
            set { Dispatcher.BeginInvoke(new Action(() => { TEMP02CODE.Content = value; })); }
        }
        public string TEMP03ID
        {
            get { return TEMP03CODE.Content.ToString(); }
            set { Dispatcher.BeginInvoke(new Action(() => { TEMP03CODE.Content = value; })); }
        }
        public string TEMP04ID
        {
            get { return TEMP04CODE.Content.ToString(); }
            set { Dispatcher.BeginInvoke(new Action(() => { TEMP04CODE.Content = value; })); }
        }
        public string TEMP05ID
        {
            get { return TEMP05CODE.Content.ToString(); }
            set { Dispatcher.BeginInvoke(new Action(() => { TEMP05CODE.Content = value; })); }
        }
        public string TEMP06ID
        {
            get { return TEMP06CODE.Content.ToString(); }
            set { Dispatcher.BeginInvoke(new Action(() => { TEMP06CODE.Content = value; })); }
        }
        public string TEMP07ID
        {
            get { return TEMP07CODE.Content.ToString(); }
            set { Dispatcher.BeginInvoke(new Action(() => { TEMP07CODE.Content = value; })); }
        }
        public string TEMP08ID
        {
            get { return TEMP08CODE.Content.ToString(); }
            set { Dispatcher.BeginInvoke(new Action(() => { TEMP08CODE.Content = value; })); }
        }
        public string TEMP09ID
        {
            get { return TEMP09CODE.Content.ToString(); }
            set { Dispatcher.BeginInvoke(new Action(() => { TEMP09CODE.Content = value; })); }
        }
        public string TEMP10ID
        {
            get { return TEMP10CODE.Content.ToString(); }
            set { Dispatcher.BeginInvoke(new Action(() => { TEMP10CODE.Content = value; })); }
        }
        public string TEMP11ID
        {
            get { return TEMP11CODE.Content.ToString(); }
            set { Dispatcher.BeginInvoke(new Action(() => { TEMP11CODE.Content = value; })); }
        }
        public string TEMP12ID
        {
            get { return TEMP12CODE.Content.ToString(); }
            set { Dispatcher.BeginInvoke(new Action(() => { TEMP12CODE.Content = value; })); }
        }
        public string TEMP13ID
        {
            get { return TEMP13CODE.Content.ToString(); }
            set { Dispatcher.BeginInvoke(new Action(() => { TEMP13CODE.Content = value; })); }
        }
        public string TEMP14ID
        {
            get { return TEMP14CODE.Content.ToString(); }
            set { Dispatcher.BeginInvoke(new Action(() => { TEMP14CODE.Content = value; })); }
        }
        public string TEMP15ID
        {
            get { return TEMP15CODE.Content.ToString(); }
            set { Dispatcher.BeginInvoke(new Action(() => { TEMP15CODE.Content = value; })); }
        }
        public string TEMP01VALUE
        {
            get { return TEMP01VAL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    TEMP01VAL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { TEMP01VAL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { TEMP01VAL.Background = minColor; }
                        else { TEMP01VAL.Background = normColor; }
                    }
                    catch { TEMP01VAL.Background = errColor; }
                }));
            }
        }
        public string TEMP02VALUE
        {
            get { return TEMP02VAL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    TEMP02VAL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { TEMP02VAL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { TEMP02VAL.Background = minColor; }
                        else { TEMP02VAL.Background = normColor; }
                    }
                    catch { TEMP02VAL.Background = errColor; }
                }));
            }
        }
        public string TEMP03VALUE
        {
            get { return TEMP03VAL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    TEMP03VAL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { TEMP03VAL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { TEMP03VAL.Background = minColor; }
                        else { TEMP03VAL.Background = normColor; }
                    }
                    catch { TEMP03VAL.Background = errColor; }
                }));
            }
        }
        public string TEMP04VALUE
        {
            get { return TEMP04VAL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    TEMP04VAL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { TEMP04VAL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { TEMP04VAL.Background = minColor; }
                        else { TEMP04VAL.Background = normColor; }
                    }
                    catch { TEMP04VAL.Background = errColor; }
                }));
            }
        }
        public string TEMP05VALUE
        {
            get { return TEMP05VAL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    TEMP05VAL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { TEMP05VAL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { TEMP05VAL.Background = minColor; }
                        else { TEMP05VAL.Background = normColor; }
                    }
                    catch { TEMP05VAL.Background = errColor; }
                }));
            }
        }
        public string TEMP06VALUE
        {
            get { return TEMP06VAL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    TEMP06VAL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { TEMP06VAL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { TEMP06VAL.Background = minColor; }
                        else { TEMP06VAL.Background = normColor; }
                    }
                    catch { TEMP06VAL.Background = errColor; }
                }));
            }
        }
        public string TEMP07VALUE
        {
            get { return TEMP07VAL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    TEMP07VAL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { TEMP07VAL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { TEMP07VAL.Background = minColor; }
                        else { TEMP07VAL.Background = normColor; }
                    }
                    catch { TEMP07VAL.Background = errColor; }
                }));
            }
        }
        public string TEMP08VALUE
        {
            get { return TEMP08VAL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    TEMP08VAL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { TEMP08VAL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { TEMP08VAL.Background = minColor; }
                        else { TEMP08VAL.Background = normColor; }
                    }
                    catch { TEMP08VAL.Background = errColor; }
                }));
            }
        }
        public string TEMP09VALUE
        {
            get { return TEMP09VAL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    TEMP09VAL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { TEMP09VAL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { TEMP09VAL.Background = minColor; }
                        else { TEMP09VAL.Background = normColor; }
                    }
                    catch { TEMP09VAL.Background = errColor; }
                }));
            }
        }
        public string TEMP10VALUE
        {
            get { return TEMP10VAL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    TEMP10VAL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { TEMP10VAL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { TEMP10VAL.Background = minColor; }
                        else { TEMP10VAL.Background = normColor; }
                    }
                    catch { TEMP10VAL.Background = errColor; }
                }));
            }
        }
        public string TEMP11VALUE
        {
            get { return TEMP11VAL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    TEMP11VAL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { TEMP11VAL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { TEMP11VAL.Background = minColor; }
                        else { TEMP11VAL.Background = normColor; }
                    }
                    catch { TEMP11VAL.Background = errColor; }
                }));
            }
        }
        public string TEMP12VALUE
        {
            get { return TEMP12VAL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    TEMP12VAL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { TEMP12VAL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { TEMP12VAL.Background = minColor; }
                        else { TEMP12VAL.Background = normColor; }
                    }
                    catch { TEMP12VAL.Background = errColor; }
                }));
            }
        }
        public string TEMP13VALUE
        {
            get { return TEMP13VAL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    TEMP13VAL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { TEMP13VAL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { TEMP13VAL.Background = minColor; }
                        else { TEMP13VAL.Background = normColor; }
                    }
                    catch { TEMP13VAL.Background = errColor; }
                }));
            }
        }
        public string TEMP14VALUE
        {
            get { return TEMP14VAL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    TEMP14VAL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { TEMP14VAL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { TEMP14VAL.Background = minColor; }
                        else { TEMP14VAL.Background = normColor; }
                    }
                    catch { TEMP14VAL.Background = errColor; }
                }));
            }
        }
        public string TEMP15VALUE
        {
            get { return TEMP15VAL.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    TEMP15VAL.Content = value;
                    try
                    {
                        if (float.Parse(value) > maxTMP) { TEMP15VAL.Background = maxColor; }
                        else if (float.Parse(value) < minTMP) { TEMP15VAL.Background = minColor; }
                        else { TEMP15VAL.Background = normColor; }
                    }
                    catch { TEMP15VAL.Background = errColor; }
                }));
            }
        }
        public bool TEMPviewIDs
        {
            get { if (TEMP01CODE.Visibility == Visibility.Visible) { return true; } else { return false; } }
            set
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    if (value)
                    {
                        TEMP01CODE.Visibility = Visibility.Visible;
                        TEMP02CODE.Visibility = Visibility.Visible;
                        TEMP03CODE.Visibility = Visibility.Visible;
                        TEMP04CODE.Visibility = Visibility.Visible;
                        TEMP05CODE.Visibility = Visibility.Visible;
                        TEMP06CODE.Visibility = Visibility.Visible;
                        TEMP07CODE.Visibility = Visibility.Visible;
                        TEMP08CODE.Visibility = Visibility.Visible;
                        TEMP09CODE.Visibility = Visibility.Visible;
                        TEMP10CODE.Visibility = Visibility.Visible;
                        TEMP11CODE.Visibility = Visibility.Visible;
                        TEMP12CODE.Visibility = Visibility.Visible;
                        TEMP13CODE.Visibility = Visibility.Visible;
                        TEMP14CODE.Visibility = Visibility.Visible;
                        TEMP15CODE.Visibility = Visibility.Visible;
                        TEMP01VAL.Parent.SetValue(Grid.RowProperty, 1);
                        TEMP02VAL.Parent.SetValue(Grid.RowProperty, 1);
                        TEMP03VAL.Parent.SetValue(Grid.RowProperty, 1);
                        TEMP04VAL.Parent.SetValue(Grid.RowProperty, 1);
                        TEMP05VAL.Parent.SetValue(Grid.RowProperty, 1);
                        TEMP06VAL.Parent.SetValue(Grid.RowProperty, 1);
                        TEMP07VAL.Parent.SetValue(Grid.RowProperty, 1);
                        TEMP08VAL.Parent.SetValue(Grid.RowProperty, 1);
                        TEMP09VAL.Parent.SetValue(Grid.RowProperty, 1);
                        TEMP10VAL.Parent.SetValue(Grid.RowProperty, 1);
                        TEMP11VAL.Parent.SetValue(Grid.RowProperty, 1);
                        TEMP12VAL.Parent.SetValue(Grid.RowProperty, 1);
                        TEMP13VAL.Parent.SetValue(Grid.RowProperty, 1);
                        TEMP14VAL.Parent.SetValue(Grid.RowProperty, 1);
                        TEMP15VAL.Parent.SetValue(Grid.RowProperty, 1);
                    }
                    else
                    {
                        TEMP01CODE.Visibility = Visibility.Hidden;
                        TEMP02CODE.Visibility = Visibility.Hidden;
                        TEMP03CODE.Visibility = Visibility.Hidden;
                        TEMP04CODE.Visibility = Visibility.Hidden;
                        TEMP05CODE.Visibility = Visibility.Hidden;
                        TEMP06CODE.Visibility = Visibility.Hidden;
                        TEMP07CODE.Visibility = Visibility.Hidden;
                        TEMP08CODE.Visibility = Visibility.Hidden;
                        TEMP09CODE.Visibility = Visibility.Hidden;
                        TEMP10CODE.Visibility = Visibility.Hidden;
                        TEMP11CODE.Visibility = Visibility.Hidden;
                        TEMP12CODE.Visibility = Visibility.Hidden;
                        TEMP13CODE.Visibility = Visibility.Hidden;
                        TEMP14CODE.Visibility = Visibility.Hidden;
                        TEMP15CODE.Visibility = Visibility.Hidden;
                        TEMP01VAL.Parent.SetValue(Grid.RowProperty, 0);
                        TEMP02VAL.Parent.SetValue(Grid.RowProperty, 0);
                        TEMP03VAL.Parent.SetValue(Grid.RowProperty, 0);
                        TEMP04VAL.Parent.SetValue(Grid.RowProperty, 0);
                        TEMP05VAL.Parent.SetValue(Grid.RowProperty, 0);
                        TEMP06VAL.Parent.SetValue(Grid.RowProperty, 0);
                        TEMP07VAL.Parent.SetValue(Grid.RowProperty, 0);
                        TEMP08VAL.Parent.SetValue(Grid.RowProperty, 0);
                        TEMP09VAL.Parent.SetValue(Grid.RowProperty, 0);
                        TEMP10VAL.Parent.SetValue(Grid.RowProperty, 0);
                        TEMP11VAL.Parent.SetValue(Grid.RowProperty, 0);
                        TEMP12VAL.Parent.SetValue(Grid.RowProperty, 0);
                        TEMP13VAL.Parent.SetValue(Grid.RowProperty, 0);
                        TEMP14VAL.Parent.SetValue(Grid.RowProperty, 0);
                        TEMP15VAL.Parent.SetValue(Grid.RowProperty, 0);
                    }
                }));
            }
        }
        public string TITLE
        {
            get { return LABEL_TITLE.Content.ToString(); }
            set
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    LABEL_TITLE.Content = value;
                    if (string.IsNullOrWhiteSpace(value)) { GRID_TITLE.Height = new GridLength(0, GridUnitType.Star); }
                    else { GRID_TITLE.Height = new GridLength(1, GridUnitType.Star); }
                }));
            }
        }
        public string GetTempID(uint id)
        {
            switch (id)
            {
                case 1:
                    return TEMP01ID;
                case 2:
                    return TEMP02ID;
                case 3:
                    return TEMP03ID;
                case 4:
                    return TEMP04ID;
                case 5:
                    return TEMP05ID;
                case 6:
                    return TEMP06ID;
                case 7:
                    return TEMP07ID;
                case 8:
                    return TEMP08ID;
                case 9:
                    return TEMP09ID;
                case 10:
                    return TEMP10ID;
                case 11:
                    return TEMP11ID;
                case 12:
                    return TEMP12ID;
                case 13:
                    return TEMP13ID;
                case 14:
                    return TEMP14ID;
                case 15:
                    return TEMP15ID;
                default:
                    throw new ArgumentException("A non-existent TEMP number was specified.", "GetTempID[" + id + "]");
            }
        }
        public void SetTempID(uint id, string tempID)
        {
            switch (id)
            {
                case 1:
                    TEMP01ID = tempID;
                    break;
                case 2:
                    TEMP02ID = tempID;
                    break;
                case 3:
                    TEMP03ID = tempID;
                    break;
                case 4:
                    TEMP04ID = tempID;
                    break;
                case 5:
                    TEMP05ID = tempID;
                    break;
                case 6:
                    TEMP06ID = tempID;
                    break;
                case 7:
                    TEMP07ID = tempID;
                    break;
                case 8:
                    TEMP08ID = tempID;
                    break;
                case 9:
                    TEMP09ID = tempID;
                    break;
                case 10:
                    TEMP10ID = tempID;
                    break;
                case 11:
                    TEMP11ID = tempID;
                    break;
                case 12:
                    TEMP12ID = tempID;
                    break;
                case 13:
                    TEMP13ID = tempID;
                    break;
                case 14:
                    TEMP14ID = tempID;
                    break;
                case 15:
                    TEMP15ID = tempID;
                    break;
                default:
                    throw new ArgumentException("A non-existent TEMP number was specified.", "SetTempID[" + id + "]");
            }
        }
        public string GetTempValue(uint id)
        {
            switch (id)
            {
                case 1:
                    return TEMP01VALUE;
                case 2:
                    return TEMP02VALUE;
                case 3:
                    return TEMP03VALUE;
                case 4:
                    return TEMP04VALUE;
                case 5:
                    return TEMP05VALUE;
                case 6:
                    return TEMP06VALUE;
                case 7:
                    return TEMP07VALUE;
                case 8:
                    return TEMP08VALUE;
                case 9:
                    return TEMP09VALUE;
                case 10:
                    return TEMP10VALUE;
                case 11:
                    return TEMP11VALUE;
                case 12:
                    return TEMP12VALUE;
                case 13:
                    return TEMP13VALUE;
                case 14:
                    return TEMP14VALUE;
                case 15:
                    return TEMP15VALUE;
                default:
                    throw new ArgumentException("A non-existent TEMP number was specified.", "GetTempValue[" + id + "]");
            }
        }
        public void SetTempValue(uint id, string tempValue)
        {
            switch (id)
            {
                case 1:
                    TEMP01VALUE = tempValue;
                    break;
                case 2:
                    TEMP02VALUE = tempValue;
                    break;
                case 3:
                    TEMP03VALUE = tempValue;
                    break;
                case 4:
                    TEMP04VALUE = tempValue;
                    break;
                case 5:
                    TEMP05VALUE = tempValue;
                    break;
                case 6:
                    TEMP06VALUE = tempValue;
                    break;
                case 7:
                    TEMP07VALUE = tempValue;
                    break;
                case 8:
                    TEMP08VALUE = tempValue;
                    break;
                case 9:
                    TEMP09VALUE = tempValue;
                    break;
                case 10:
                    TEMP10VALUE = tempValue;
                    break;
                case 11:
                    TEMP11VALUE = tempValue;
                    break;
                case 12:
                    TEMP12VALUE = tempValue;
                    break;
                case 13:
                    TEMP13VALUE = tempValue;
                    break;
                case 14:
                    TEMP14VALUE = tempValue;
                    break;
                case 15:
                    TEMP15VALUE = tempValue;
                    break;
                default:
                    throw new ArgumentException("A non-existent TEMP number was specified.", "SetTempValue[" + id + "]");
            }
        }
        public double TEMPviewIDratio
        {
            get
            {
                if (TEMP01CODE.Visibility == Visibility.Visible)
                { return ((Grid)TEMP_GRID.Children[0]).RowDefinitions[1].Height.Value; }
                else { return 0; }
            }
            set
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (Grid grid in TEMP_GRID.Children)
                    {
                        grid.RowDefinitions[1].Height = new GridLength(value, GridUnitType.Star);
                    }
                }));
            }
        }

        #endregion
        public UnitMonitor() : this("SN") { }
        public UnitMonitor(string SN)
        {
            InitializeComponent();
            TEMPcpu = "---";
            SNSvin = "---";
            SNSpin = "---";
            SNSvd = "---";
            SNSid = "---";
            TEMPavg = "---";
            TEMPmax = "---";
            TEMPmin = "---";
            TITLE = "";
            SerialNumber = SN;
            for (uint i = 1; i < 16; i++)
            {
                SetTempID(i, "ND");
                SetTempValue(i, "---");
            }
            TEMPviewIDs = false;
            TEMPviewIDratio = 2;
        }
    }
}
