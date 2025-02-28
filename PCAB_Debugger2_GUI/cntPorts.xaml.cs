using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PCAB_Debugger2_GUI
{
    /// <summary>
    /// Ports.xaml の相互作用ロジック
    /// </summary>
    public partial class Ports : UserControl
    {
        /// <summary>配列表示順指定列挙型</summary>
        public enum ROTATE
        {
            ZERO = 0,
            RIGHT_TURN = 1,
            LEFT_TURN = 2,
            HALF_TURN = 3,
            MIRROR_ZERO = 4,
            MIRROR_RIGHT_TURN = 5,
            MIRROR_LEFT_TURN = 6,
            MIRROR_HALF_TURN = 7,
            MATRIX = 8
        }
        public delegate void CheckboxClickEventHandler(object sender, RoutedEventArgs e, bool? isChecked);
        /// <summary>STBLNAチェックボックスクリックイベント</summary>
        public event CheckboxClickEventHandler STBLNA_CheckboxClickEvent;
        /// <summary>StandbyLNAチェックボックス状態プロパティ</summary>
        public bool? StandbyLNA
        {
            get { return CheckBox_STBLNA.IsChecked; }
            set
            {
                switch (value)
                {
                    case true:
                        CheckBox_STBLNA_Checked("STBLNA", null);
                        break;
                    case false:
                        CheckBox_STBLNA_Unchecked("STBLNA", null);
                        break;
                    default:
                        CheckBox_STBLNA_Indeterminate("STBLNA", null);
                        break;
                }
            }
        }
        public void SetDSA(List<uint> value)
        {
            if (value.Count == 15)
            {
                for (int i = 0; i < value.Count; i++)
                {
                    SetDSA((uint)(i + 1), value[i]);
                }
            }
        }
        public void SetDSA(uint number, uint value)
        {
            try
            {
                switch (number)
                {
                    case 0:
                        if (ComboBox_DSA16.Items.Count <= value || value < 0)
                        { throw new ArgumentException("A value out of range was specified.\nVALUE > " + value.ToString(), "SetDSA[" + number + "]"); }
                        ComboBox_DSA16.SelectedIndex = (int)value;
                        break;
                    case 1: Port01.DSA = value; break;
                    case 2: Port02.DSA = value; break;
                    case 3: Port03.DSA = value; break;
                    case 4: Port04.DSA = value; break;
                    case 5: Port05.DSA = value; break;
                    case 6: Port06.DSA = value; break;
                    case 7: Port07.DSA = value; break;
                    case 8: Port08.DSA = value; break;
                    case 9: Port09.DSA = value; break;
                    case 10: Port10.DSA = value; break;
                    case 11: Port11.DSA = value; break;
                    case 12: Port12.DSA = value; break;
                    case 13: Port13.DSA = value; break;
                    case 14: Port14.DSA = value; break;
                    case 15: Port15.DSA = value; break;
                    default:
                        throw new ArgumentException("A non-existent DSA number was specified.", "SetDSA[" + number + "]");
                }
                if (CheckBox_ALL_DSA.IsChecked == true) { CheckBox_ALL_DSA.IsChecked = false; }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int GetDSA(uint number)
        {
            switch (number)
            {
                case 0: return ComboBox_DSA16.SelectedIndex;
                case 1: return (int)Port01.DSA;
                case 2: return (int)Port02.DSA;
                case 3: return (int)Port03.DSA;
                case 4: return (int)Port04.DSA;
                case 5: return (int)Port05.DSA;
                case 6: return (int)Port06.DSA;
                case 7: return (int)Port07.DSA;
                case 8: return (int)Port08.DSA;
                case 9: return (int)Port09.DSA;
                case 10: return (int)Port10.DSA;
                case 11: return (int)Port11.DSA;
                case 12: return (int)Port12.DSA;
                case 13: return (int)Port13.DSA;
                case 14: return (int)Port14.DSA;
                case 15: return (int)Port15.DSA;
                default: throw new ArgumentException("A non-existent DSA number was specified.", "GetDSA[" + number + "]");
            }
        }
        public List<uint> GetDSA()
        {
            List<uint> dsa = new List<uint>();
            for (uint i = 0; i < 15; i++) { dsa.Add((uint)GetDSA(i + 1)); }
            return dsa;
        }
        public void SetDPS(List<uint> value)
        {
            if (value.Count == 15)
            {
                for (int i = 0; i < value.Count; i++)
                {
                    SetDPS((uint)(i + 1), value[i]);
                }
            }
        }
        public void SetDPS(uint number, uint value)
        {
            try
            {
                switch (number)
                {
                    case 1: Port01.DPS = value; break;
                    case 2: Port02.DPS = value; break;
                    case 3: Port03.DPS = value; break;
                    case 4: Port04.DPS = value; break;
                    case 5: Port05.DPS = value; break;
                    case 6: Port06.DPS = value; break;
                    case 7: Port07.DPS = value; break;
                    case 8: Port08.DPS = value; break;
                    case 9: Port09.DPS = value; break;
                    case 10: Port10.DPS = value; break;
                    case 11: Port11.DPS = value; break;
                    case 12: Port12.DPS = value; break;
                    case 13: Port13.DPS = value; break;
                    case 14: Port14.DPS = value; break;
                    case 15: Port15.DPS = value; break;
                    default:
                        throw new ArgumentException("A non-existent DPS number was specified.", "SetDPS[" + number + "]");
                }
                if (CheckBox_ALL_DPS.IsChecked == true) { CheckBox_ALL_DPS.IsChecked = false; }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int GetDPS(uint number)
        {
            switch (number)
            {
                case 1: return (int)Port01.DPS;
                case 2: return (int)Port02.DPS;
                case 3: return (int)Port03.DPS;
                case 4: return (int)Port04.DPS;
                case 5: return (int)Port05.DPS;
                case 6: return (int)Port06.DPS;
                case 7: return (int)Port07.DPS;
                case 8: return (int)Port08.DPS;
                case 9: return (int)Port09.DPS;
                case 10: return (int)Port10.DSA;
                case 11: return (int)Port11.DSA;
                case 12: return (int)Port12.DSA;
                case 13: return (int)Port13.DSA;
                case 14: return (int)Port14.DSA;
                case 15: return (int)Port15.DSA;
                default: throw new ArgumentException("A non-existent DPS number was specified.", "GetDPS[" + number + "]");
            }
        }
        public List<uint> GetDPS()
        {
            List<uint> dsa = new List<uint>();
            for (uint i = 0; i < 15; i++) { dsa.Add((uint)GetDPS(i + 1)); }
            return dsa;
        }
        /// <summary>初期化時指定シリアル番号取得</summary>
        public string SerialNumber { get; private set; }
        /// <summary>配列表示順プロパティ</summary>
        public ROTATE TURN
        {
            get { return _angle; }
            set
            {
                _angle = value;
                switch (_angle)
                {
                    case ROTATE.RIGHT_TURN:
                        Port01.SetValue(Grid.RowProperty, 3);
                        Port02.SetValue(Grid.RowProperty, 3);
                        Port03.SetValue(Grid.RowProperty, 3);
                        Port04.SetValue(Grid.RowProperty, 3);
                        Port05.SetValue(Grid.RowProperty, 2);
                        Port06.SetValue(Grid.RowProperty, 2);
                        Port07.SetValue(Grid.RowProperty, 2);
                        Port08.SetValue(Grid.RowProperty, 2);
                        Port09.SetValue(Grid.RowProperty, 1);
                        Port10.SetValue(Grid.RowProperty, 1);
                        Port11.SetValue(Grid.RowProperty, 1);
                        Port12.SetValue(Grid.RowProperty, 1);
                        Port13.SetValue(Grid.RowProperty, 0);
                        Port14.SetValue(Grid.RowProperty, 0);
                        Port15.SetValue(Grid.RowProperty, 0);
                        Port16.SetValue(Grid.RowProperty, 0);
                        Port01.SetValue(Grid.ColumnProperty, 0);
                        Port02.SetValue(Grid.ColumnProperty, 1);
                        Port03.SetValue(Grid.ColumnProperty, 2);
                        Port04.SetValue(Grid.ColumnProperty, 3);
                        Port05.SetValue(Grid.ColumnProperty, 3);
                        Port06.SetValue(Grid.ColumnProperty, 2);
                        Port07.SetValue(Grid.ColumnProperty, 1);
                        Port08.SetValue(Grid.ColumnProperty, 0);
                        Port09.SetValue(Grid.ColumnProperty, 0);
                        Port10.SetValue(Grid.ColumnProperty, 1);
                        Port11.SetValue(Grid.ColumnProperty, 2);
                        Port12.SetValue(Grid.ColumnProperty, 3);
                        Port13.SetValue(Grid.ColumnProperty, 3);
                        Port14.SetValue(Grid.ColumnProperty, 2);
                        Port15.SetValue(Grid.ColumnProperty, 1);
                        Port16.SetValue(Grid.ColumnProperty, 0);
                        break;
                    case ROTATE.LEFT_TURN:
                        Port01.SetValue(Grid.RowProperty, 0);
                        Port02.SetValue(Grid.RowProperty, 0);
                        Port03.SetValue(Grid.RowProperty, 0);
                        Port04.SetValue(Grid.RowProperty, 0);
                        Port05.SetValue(Grid.RowProperty, 1);
                        Port06.SetValue(Grid.RowProperty, 1);
                        Port07.SetValue(Grid.RowProperty, 1);
                        Port08.SetValue(Grid.RowProperty, 1);
                        Port09.SetValue(Grid.RowProperty, 2);
                        Port10.SetValue(Grid.RowProperty, 2);
                        Port11.SetValue(Grid.RowProperty, 2);
                        Port12.SetValue(Grid.RowProperty, 2);
                        Port13.SetValue(Grid.RowProperty, 3);
                        Port14.SetValue(Grid.RowProperty, 3);
                        Port15.SetValue(Grid.RowProperty, 3);
                        Port16.SetValue(Grid.RowProperty, 3);
                        Port01.SetValue(Grid.ColumnProperty, 3);
                        Port02.SetValue(Grid.ColumnProperty, 2);
                        Port03.SetValue(Grid.ColumnProperty, 1);
                        Port04.SetValue(Grid.ColumnProperty, 0);
                        Port05.SetValue(Grid.ColumnProperty, 0);
                        Port06.SetValue(Grid.ColumnProperty, 1);
                        Port07.SetValue(Grid.ColumnProperty, 2);
                        Port08.SetValue(Grid.ColumnProperty, 3);
                        Port09.SetValue(Grid.ColumnProperty, 3);
                        Port10.SetValue(Grid.ColumnProperty, 2);
                        Port11.SetValue(Grid.ColumnProperty, 1);
                        Port12.SetValue(Grid.ColumnProperty, 0);
                        Port13.SetValue(Grid.ColumnProperty, 0);
                        Port14.SetValue(Grid.ColumnProperty, 1);
                        Port15.SetValue(Grid.ColumnProperty, 2);
                        Port16.SetValue(Grid.ColumnProperty, 3);
                        break;
                    case ROTATE.HALF_TURN:
                        Port01.SetValue(Grid.RowProperty, 0);
                        Port02.SetValue(Grid.RowProperty, 1);
                        Port03.SetValue(Grid.RowProperty, 2);
                        Port04.SetValue(Grid.RowProperty, 3);
                        Port05.SetValue(Grid.RowProperty, 3);
                        Port06.SetValue(Grid.RowProperty, 2);
                        Port07.SetValue(Grid.RowProperty, 1);
                        Port08.SetValue(Grid.RowProperty, 0);
                        Port09.SetValue(Grid.RowProperty, 0);
                        Port10.SetValue(Grid.RowProperty, 1);
                        Port11.SetValue(Grid.RowProperty, 2);
                        Port12.SetValue(Grid.RowProperty, 3);
                        Port13.SetValue(Grid.RowProperty, 3);
                        Port14.SetValue(Grid.RowProperty, 2);
                        Port15.SetValue(Grid.RowProperty, 1);
                        Port16.SetValue(Grid.RowProperty, 0);
                        Port01.SetValue(Grid.ColumnProperty, 0);
                        Port02.SetValue(Grid.ColumnProperty, 0);
                        Port03.SetValue(Grid.ColumnProperty, 0);
                        Port04.SetValue(Grid.ColumnProperty, 0);
                        Port05.SetValue(Grid.ColumnProperty, 1);
                        Port06.SetValue(Grid.ColumnProperty, 1);
                        Port07.SetValue(Grid.ColumnProperty, 1);
                        Port08.SetValue(Grid.ColumnProperty, 1);
                        Port09.SetValue(Grid.ColumnProperty, 2);
                        Port10.SetValue(Grid.ColumnProperty, 2);
                        Port11.SetValue(Grid.ColumnProperty, 2);
                        Port12.SetValue(Grid.ColumnProperty, 2);
                        Port13.SetValue(Grid.ColumnProperty, 3);
                        Port14.SetValue(Grid.ColumnProperty, 3);
                        Port15.SetValue(Grid.ColumnProperty, 3);
                        Port16.SetValue(Grid.ColumnProperty, 3);
                        break;
                    case ROTATE.MIRROR_ZERO:
                        Port01.SetValue(Grid.RowProperty, 3);
                        Port02.SetValue(Grid.RowProperty, 2);
                        Port03.SetValue(Grid.RowProperty, 1);
                        Port04.SetValue(Grid.RowProperty, 0);
                        Port05.SetValue(Grid.RowProperty, 0);
                        Port06.SetValue(Grid.RowProperty, 1);
                        Port07.SetValue(Grid.RowProperty, 2);
                        Port08.SetValue(Grid.RowProperty, 3);
                        Port09.SetValue(Grid.RowProperty, 3);
                        Port10.SetValue(Grid.RowProperty, 2);
                        Port11.SetValue(Grid.RowProperty, 1);
                        Port12.SetValue(Grid.RowProperty, 0);
                        Port13.SetValue(Grid.RowProperty, 0);
                        Port14.SetValue(Grid.RowProperty, 1);
                        Port15.SetValue(Grid.RowProperty, 2);
                        Port16.SetValue(Grid.RowProperty, 3);
                        Port01.SetValue(Grid.ColumnProperty, 0);
                        Port02.SetValue(Grid.ColumnProperty, 0);
                        Port03.SetValue(Grid.ColumnProperty, 0);
                        Port04.SetValue(Grid.ColumnProperty, 0);
                        Port05.SetValue(Grid.ColumnProperty, 1);
                        Port06.SetValue(Grid.ColumnProperty, 1);
                        Port07.SetValue(Grid.ColumnProperty, 1);
                        Port08.SetValue(Grid.ColumnProperty, 1);
                        Port09.SetValue(Grid.ColumnProperty, 2);
                        Port10.SetValue(Grid.ColumnProperty, 2);
                        Port11.SetValue(Grid.ColumnProperty, 2);
                        Port12.SetValue(Grid.ColumnProperty, 2);
                        Port13.SetValue(Grid.ColumnProperty, 3);
                        Port14.SetValue(Grid.ColumnProperty, 3);
                        Port15.SetValue(Grid.ColumnProperty, 3);
                        Port16.SetValue(Grid.ColumnProperty, 3);
                        break;
                    case ROTATE.MIRROR_RIGHT_TURN:
                        Port01.SetValue(Grid.RowProperty, 3);
                        Port02.SetValue(Grid.RowProperty, 3);
                        Port03.SetValue(Grid.RowProperty, 3);
                        Port04.SetValue(Grid.RowProperty, 3);
                        Port05.SetValue(Grid.RowProperty, 2);
                        Port06.SetValue(Grid.RowProperty, 2);
                        Port07.SetValue(Grid.RowProperty, 2);
                        Port08.SetValue(Grid.RowProperty, 2);
                        Port09.SetValue(Grid.RowProperty, 1);
                        Port10.SetValue(Grid.RowProperty, 1);
                        Port11.SetValue(Grid.RowProperty, 1);
                        Port12.SetValue(Grid.RowProperty, 1);
                        Port13.SetValue(Grid.RowProperty, 0);
                        Port14.SetValue(Grid.RowProperty, 0);
                        Port15.SetValue(Grid.RowProperty, 0);
                        Port16.SetValue(Grid.RowProperty, 0);
                        Port01.SetValue(Grid.ColumnProperty, 3);
                        Port02.SetValue(Grid.ColumnProperty, 2);
                        Port03.SetValue(Grid.ColumnProperty, 1);
                        Port04.SetValue(Grid.ColumnProperty, 0);
                        Port05.SetValue(Grid.ColumnProperty, 0);
                        Port06.SetValue(Grid.ColumnProperty, 1);
                        Port07.SetValue(Grid.ColumnProperty, 2);
                        Port08.SetValue(Grid.ColumnProperty, 3);
                        Port09.SetValue(Grid.ColumnProperty, 3);
                        Port10.SetValue(Grid.ColumnProperty, 2);
                        Port11.SetValue(Grid.ColumnProperty, 1);
                        Port12.SetValue(Grid.ColumnProperty, 0);
                        Port13.SetValue(Grid.ColumnProperty, 0);
                        Port14.SetValue(Grid.ColumnProperty, 1);
                        Port15.SetValue(Grid.ColumnProperty, 2);
                        Port16.SetValue(Grid.ColumnProperty, 3);
                        break;
                    case ROTATE.MIRROR_LEFT_TURN:
                        Port01.SetValue(Grid.RowProperty, 0);
                        Port02.SetValue(Grid.RowProperty, 0);
                        Port03.SetValue(Grid.RowProperty, 0);
                        Port04.SetValue(Grid.RowProperty, 0);
                        Port05.SetValue(Grid.RowProperty, 1);
                        Port06.SetValue(Grid.RowProperty, 1);
                        Port07.SetValue(Grid.RowProperty, 1);
                        Port08.SetValue(Grid.RowProperty, 1);
                        Port09.SetValue(Grid.RowProperty, 2);
                        Port10.SetValue(Grid.RowProperty, 2);
                        Port11.SetValue(Grid.RowProperty, 2);
                        Port12.SetValue(Grid.RowProperty, 2);
                        Port13.SetValue(Grid.RowProperty, 3);
                        Port14.SetValue(Grid.RowProperty, 3);
                        Port15.SetValue(Grid.RowProperty, 3);
                        Port16.SetValue(Grid.RowProperty, 3);
                        Port01.SetValue(Grid.ColumnProperty, 0);
                        Port02.SetValue(Grid.ColumnProperty, 1);
                        Port03.SetValue(Grid.ColumnProperty, 2);
                        Port04.SetValue(Grid.ColumnProperty, 3);
                        Port05.SetValue(Grid.ColumnProperty, 3);
                        Port06.SetValue(Grid.ColumnProperty, 2);
                        Port07.SetValue(Grid.ColumnProperty, 1);
                        Port08.SetValue(Grid.ColumnProperty, 0);
                        Port09.SetValue(Grid.ColumnProperty, 0);
                        Port10.SetValue(Grid.ColumnProperty, 1);
                        Port11.SetValue(Grid.ColumnProperty, 2);
                        Port12.SetValue(Grid.ColumnProperty, 3);
                        Port13.SetValue(Grid.ColumnProperty, 3);
                        Port14.SetValue(Grid.ColumnProperty, 2);
                        Port15.SetValue(Grid.ColumnProperty, 1);
                        Port16.SetValue(Grid.ColumnProperty, 0);
                        break;
                    case ROTATE.MIRROR_HALF_TURN:
                        Port01.SetValue(Grid.RowProperty, 0);
                        Port02.SetValue(Grid.RowProperty, 1);
                        Port03.SetValue(Grid.RowProperty, 2);
                        Port04.SetValue(Grid.RowProperty, 3);
                        Port05.SetValue(Grid.RowProperty, 3);
                        Port06.SetValue(Grid.RowProperty, 2);
                        Port07.SetValue(Grid.RowProperty, 1);
                        Port08.SetValue(Grid.RowProperty, 0);
                        Port09.SetValue(Grid.RowProperty, 0);
                        Port10.SetValue(Grid.RowProperty, 1);
                        Port11.SetValue(Grid.RowProperty, 2);
                        Port12.SetValue(Grid.RowProperty, 3);
                        Port13.SetValue(Grid.RowProperty, 3);
                        Port14.SetValue(Grid.RowProperty, 2);
                        Port15.SetValue(Grid.RowProperty, 1);
                        Port16.SetValue(Grid.RowProperty, 0);
                        Port01.SetValue(Grid.ColumnProperty, 3);
                        Port02.SetValue(Grid.ColumnProperty, 3);
                        Port03.SetValue(Grid.ColumnProperty, 3);
                        Port04.SetValue(Grid.ColumnProperty, 3);
                        Port05.SetValue(Grid.ColumnProperty, 2);
                        Port06.SetValue(Grid.ColumnProperty, 2);
                        Port07.SetValue(Grid.ColumnProperty, 2);
                        Port08.SetValue(Grid.ColumnProperty, 2);
                        Port09.SetValue(Grid.ColumnProperty, 1);
                        Port10.SetValue(Grid.ColumnProperty, 1);
                        Port11.SetValue(Grid.ColumnProperty, 1);
                        Port12.SetValue(Grid.ColumnProperty, 1);
                        Port13.SetValue(Grid.ColumnProperty, 0);
                        Port14.SetValue(Grid.ColumnProperty, 0);
                        Port15.SetValue(Grid.ColumnProperty, 0);
                        Port16.SetValue(Grid.ColumnProperty, 0);
                        break;
                    case ROTATE.MATRIX:
                        Port01.SetValue(Grid.RowProperty, 0);
                        Port02.SetValue(Grid.RowProperty, 0);
                        Port03.SetValue(Grid.RowProperty, 0);
                        Port04.SetValue(Grid.RowProperty, 0);
                        Port05.SetValue(Grid.RowProperty, 1);
                        Port06.SetValue(Grid.RowProperty, 1);
                        Port07.SetValue(Grid.RowProperty, 1);
                        Port08.SetValue(Grid.RowProperty, 1);
                        Port09.SetValue(Grid.RowProperty, 2);
                        Port10.SetValue(Grid.RowProperty, 2);
                        Port11.SetValue(Grid.RowProperty, 2);
                        Port12.SetValue(Grid.RowProperty, 2);
                        Port13.SetValue(Grid.RowProperty, 3);
                        Port14.SetValue(Grid.RowProperty, 3);
                        Port15.SetValue(Grid.RowProperty, 3);
                        Port16.SetValue(Grid.RowProperty, 3);
                        Port01.SetValue(Grid.ColumnProperty, 0);
                        Port02.SetValue(Grid.ColumnProperty, 1);
                        Port03.SetValue(Grid.ColumnProperty, 2);
                        Port04.SetValue(Grid.ColumnProperty, 3);
                        Port05.SetValue(Grid.ColumnProperty, 0);
                        Port06.SetValue(Grid.ColumnProperty, 1);
                        Port07.SetValue(Grid.ColumnProperty, 2);
                        Port08.SetValue(Grid.ColumnProperty, 3);
                        Port09.SetValue(Grid.ColumnProperty, 0);
                        Port10.SetValue(Grid.ColumnProperty, 1);
                        Port11.SetValue(Grid.ColumnProperty, 2);
                        Port12.SetValue(Grid.ColumnProperty, 3);
                        Port13.SetValue(Grid.ColumnProperty, 0);
                        Port14.SetValue(Grid.ColumnProperty, 1);
                        Port15.SetValue(Grid.ColumnProperty, 2);
                        Port16.SetValue(Grid.ColumnProperty, 3);
                        break;
                    default:
                        Port01.SetValue(Grid.RowProperty, 3);
                        Port02.SetValue(Grid.RowProperty, 2);
                        Port03.SetValue(Grid.RowProperty, 1);
                        Port04.SetValue(Grid.RowProperty, 0);
                        Port05.SetValue(Grid.RowProperty, 0);
                        Port06.SetValue(Grid.RowProperty, 1);
                        Port07.SetValue(Grid.RowProperty, 2);
                        Port08.SetValue(Grid.RowProperty, 3);
                        Port09.SetValue(Grid.RowProperty, 3);
                        Port10.SetValue(Grid.RowProperty, 2);
                        Port11.SetValue(Grid.RowProperty, 1);
                        Port12.SetValue(Grid.RowProperty, 0);
                        Port13.SetValue(Grid.RowProperty, 0);
                        Port14.SetValue(Grid.RowProperty, 1);
                        Port15.SetValue(Grid.RowProperty, 2);
                        Port16.SetValue(Grid.RowProperty, 3);
                        Port01.SetValue(Grid.ColumnProperty, 3);
                        Port02.SetValue(Grid.ColumnProperty, 3);
                        Port03.SetValue(Grid.ColumnProperty, 3);
                        Port04.SetValue(Grid.ColumnProperty, 3);
                        Port05.SetValue(Grid.ColumnProperty, 2);
                        Port06.SetValue(Grid.ColumnProperty, 2);
                        Port07.SetValue(Grid.ColumnProperty, 2);
                        Port08.SetValue(Grid.ColumnProperty, 2);
                        Port09.SetValue(Grid.ColumnProperty, 1);
                        Port10.SetValue(Grid.ColumnProperty, 1);
                        Port11.SetValue(Grid.ColumnProperty, 1);
                        Port12.SetValue(Grid.ColumnProperty, 1);
                        Port13.SetValue(Grid.ColumnProperty, 0);
                        Port14.SetValue(Grid.ColumnProperty, 0);
                        Port15.SetValue(Grid.ColumnProperty, 0);
                        Port16.SetValue(Grid.ColumnProperty, 0);
                        break;
                }
            }
        }

        private ROTATE _angle;

        /// <summary>コンストラクタ</summary>
        public Ports() : this("SN", ROTATE.MATRIX) { }

        /// <summary>コンストラクタ</summary>
        /// <param name="serialNumber">シリアル番号</param>
        /// <param name="_turn">配列表示順指定</param>
        public Ports(string serialNumber, ROTATE _turn)
        {
            InitializeComponent();
            SerialNumber = serialNumber;
            TURN = _turn;
        }

        private void CheckBox_ALL_DSA_ChengeChecked(object sender, RoutedEventArgs e)
        {
            if (CheckBox_ALL_DSA.IsChecked == true)
            {
                ComboBox_ALL_DSA.IsEnabled = true;
                foreach (object gridProt in Grid_Ports.Children)
                {
                    if (gridProt is Port)
                    {
                        ((Port)gridProt).DSA_IsEnabled = false;
                        ((Port)gridProt).DSA = (uint)ComboBox_ALL_DSA.SelectedIndex;
                    }
                }
            }
            else
            {
                ComboBox_ALL_DSA.IsEnabled = false;
                foreach (object gridProt in Grid_Ports.Children)
                {
                    if (gridProt is Port) { ((Port)gridProt).DSA_IsEnabled = true; }
                }
            }
        }

        private void CheckBox_ALL_DPS_ChengeChecked(object sender, RoutedEventArgs e)
        {
            if (CheckBox_ALL_DPS.IsChecked == true)
            {
                ComboBox_ALL_DPS.IsEnabled = true;
                foreach (object gridProt in Grid_Ports.Children)
                {
                    if (gridProt is Port)
                    {
                        ((Port)gridProt).DPS_IsEnabled = false;
                        ((Port)gridProt).DPS = (uint)ComboBox_ALL_DPS.SelectedIndex;
                    }
                }
            }
            else
            {
                ComboBox_ALL_DPS.IsEnabled = false;
                foreach (object gridProt in Grid_Ports.Children)
                {
                    if (gridProt is Port) { ((Port)gridProt).DPS_IsEnabled = true; }
                }
            }
        }

        private void ComboBox_ALL_DSA_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                foreach (object gridProt in Grid_Ports.Children)
                {
                    if (gridProt is Port)
                    {
                        ((Port)gridProt).DSA = (uint)ComboBox_ALL_DSA.SelectedIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.HResult != -2147467261) { throw; }
            }
        }

        private void ComboBox_ALL_DPS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                foreach (object gridProt in Grid_Ports.Children)
                {
                    if (gridProt is Port)
                    {
                        ((Port)gridProt).DPS = (uint)ComboBox_ALL_DPS.SelectedIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.HResult != -2147467261) { throw; }
            }
        }

        private void CheckBox_STBLNA_Checked(object sender, RoutedEventArgs e)
        {
            if (typeof(CheckBox) == sender.GetType())
            {
                STBLNA_CheckboxClickEvent?.Invoke(this, e, true);
            }
            else
            {
                CheckBox_STBLNA.IsChecked = true;
            }
        }

        private void CheckBox_STBLNA_Unchecked(object sender, RoutedEventArgs e)
        {
            if (typeof(CheckBox) == sender.GetType())
            {
                STBLNA_CheckboxClickEvent?.Invoke(this, e, false);
            }
            else
            {
                CheckBox_STBLNA.IsChecked = false;
            }
        }

        private void CheckBox_STBLNA_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (typeof(CheckBox) == sender.GetType())
            {
                STBLNA_CheckboxClickEvent?.Invoke(this, e, null);
            }
            else
            {
                CheckBox_STBLNA.IsChecked = null;
            }
        }
    }
}
