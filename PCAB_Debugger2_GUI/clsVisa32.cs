using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PCAB_Debugger2_GUI
{
    /// <summary>VISA制御クラス</summary>
    public class VisaControl : IDisposable
    {
        /// <summary>Open時のアクセスモード列挙型</summary>
        public enum viAccessMode : int
        {
            /// <summary>デフォルト値を使用</summary>
            VI_NULL = visa32.OtherVisaDefinition.VI_NULL,
            /// <summary>排他ロックを取得(ロックを取得できない場合はセッションが閉じられエラー)</summary>
            VI_EXCLUSIVE_LOCK = visa32.OtherVisaDefinition.VI_EXCLUSIVE_LOCK,
            /// <summary>外部構成ゆーてぃりちによって指定された値(シリアルInsertセッションでのみサポート)</summary>
            VI_LOAD_CONFIG = visa32.OtherVisaDefinition.VI_LOAD_CONFIG,
        }

        #region 制御プロパティ
        /// <summary>測定器ハンドル</summary>
        private int instrID;
        /// <summary>通信結果バッファ</summary>
        private visa32.ResultCode viResult;
        /// <summary>Read1回の受信Byte数(PC側のメモリ容量等によって要調整)</summary>
        private int getByteLen;
        #endregion

        #region Property取得
        /// <summary>VisaResource</summary>
        public VisaResource Resource { get; private set; }
        /// <summary>初期化時に設定された測定器のVISAアドレス取得</summary>
        public string VisaAddress { get; private set; }
        /// <summary>接続状態確認(true:接続中/false:未接続)</summary>
        public bool IsOpen { get; private set; }
        /// <summary>クラスのデフォルトタイムアウト時間を取得・変更[mS]</summary>
        public int DefaultTimeout { get; set; }
        /// <summary>改行コード</summary>
        public string NewLine { get; set; }
        #endregion

        #region コンストラクタ
        /// <summary>コンストラクタ</summary>
        /// <param name="resource">VISA Resourceを指定</param>
        /// <param name="address">VISA Addressを指定</param>
        public VisaControl(VisaResource resource, string address) : this(resource, address, 4096, 2000, "\n") { }

        /// <summary>コンストラクタ</summary>
        /// <param name="resource">VISA Resourceを指定</param>
        /// <param name="address">VISA Addressを指定</param>
        /// <param name="bufferLength">受信時のバッファ長を指定</param>
        /// <param name="defTimeout">クラスデフォルトのタイムアウト時間をミリ秒で指定</param>
        /// <param name="NewLineCode">改行コードを指定</param>
        public VisaControl(VisaResource resource, string address, short bufferLength, short defTimeout, string NewLineCode)
            : this(resource, address, (int)bufferLength, (int)defTimeout, NewLineCode) { }

        /// <summary>コンストラクタ</summary>
        /// <param name="resource">VISA Resourceを指定</param>
        /// <param name="address">VISA Addressを指定</param>
        /// <param name="bufferLength">受信時のバッファ長を指定</param>
        /// <param name="defTimeout">クラスデフォルトのタイムアウト時間をミリ秒で指定</param>
        /// <param name="NewLineCode">改行コードを指定</param>
        public VisaControl(VisaResource resource, string address, int bufferLength, int defTimeout, string NewLineCode)
        { Resource = resource; VisaAddress = address; getByteLen = bufferLength; DefaultTimeout = defTimeout; NewLine = NewLineCode; }
        #endregion

        /// <summary>Classリソース破棄(VISA Resourceは破棄されないため注意)</summary>
        public void Dispose()
        {
            try { Close(); } catch { }
        }

        #region 接続・切断処理関連
        /// <summary>接続処理実行 (タイムアウト時間はClassのデフォルト設定を使用)</summary>
        public void Open() { Open(this.DefaultTimeout, viAccessMode.VI_NULL); }
        /// <summary>接続処理実行</summary>
        /// <param name="openTimeout">接続処理のタイムアウト時間指定[mS]</param>
        /// <param name="accessMode">Open時のアクセスモード指定</param>
        public void Open(int openTimeout, viAccessMode accessMode)
        {
            if (openTimeout <= 0) { throw new ArgumentOutOfRangeException("The timeout period cannot be less than or equal to zero."); }
            try
            {
#if DEBUG_GPIB_NOT_CONNECT
                viResult = visa32.ResultCode.VI_SUCCESS;
#else
                viResult = Resource.viOpen(VisaAddress, (int)accessMode, openTimeout, out instrID);
#endif
                if (viResult == visa32.ResultCode.VI_SUCCESS) { IsOpen = true; }
                else { throw new Exception(viResult.ToString()); }
            }
            catch (Exception e) { throw e; }
        }

        /// <summary>切断処理実行</summary>
        public void Close()
        {
            try
            {
#if DEBUG_DEBUG_GPIB_NOT_CONNECT
                if (IsOpen) { viResult = visa32.ResultCode.VI_SUCCESS; }
                else { viResult = visa32.ResultCode.VI_WARN_NULL_OBJECT; }
#else
                viResult = Resource.viClose(instrID);
#endif
                if (viResult == visa32.ResultCode.VI_SUCCESS) { IsOpen = false; }
                else { throw new Exception(viResult.ToString()); }
            }
            catch (Exception e) { throw e; }
        }

        /// <summary>コントロールを計測器に返す<br/>
        /// GPIB/USB/HiSLIPのみ対応</summary>
        public void ControlLocal() { ControlREN(false); }
        /// <summary>コントロールをリモートに変更する(計測器のLocalコントロールをロック)<br/>
        /// GPIB/USB/HiSLIPのみ対応</summary>
        public void ControlRemote() { ControlREN(true); }
        /// <summary>コントロールをリモートまたはローカルに切替(GPIB/USB/HiSLIPのみ対応)</summary>
        /// <param name="remote">true:remote / false:local</param>
        public void ControlREN(bool remote)
        {
            try
            {
                //GPIB/USB/TCPIP(HiLSIP)で接続している場合はコントロール返却(Errorはログ保存のみで無視)
                if (VisaAddress.Substring(0, 4).ToUpper() == "GPIB" || VisaAddress.Substring(0, 3).ToUpper() == "USB" || VisaAddress.Substring(0, 5).ToUpper() == "TCPIP")
                {
#if DEBUG_GPIB_NOT_CONNECT
                    viResult = visa32.ResultCode.VI_SUCCESS;
#else
                    if (remote) { viResult = Resource.viGpibControlREN(instrID, (short)visa32.OtherVisaDefinition.VI_GPIB_REN_ASSERT_LLO); }
                    else { viResult = Resource.viGpibControlREN(instrID, (short)visa32.OtherVisaDefinition.VI_GPIB_REN_DEASSERT_GTL); }
#endif
                    if (viResult != visa32.ResultCode.VI_SUCCESS) { throw new Exception(viResult.ToString()); }
                }
            }
            catch (Exception e) { throw e; }
        }

        #endregion

        #region 通信設定関連
        /// <summary>タイムアウト時間をClassのデフォルトに設定</summary>
        public void SetTimeout() { SetTimeout(this.DefaultTimeout); }
        /// <summary>タイムアウト時間の変更</summary>
        /// <param name="time">タイムアウト時間[mS]</param>
        public void SetTimeout(int time)
        {
            try
            {
#if DEBUG_GPIB_NOT_CONNECT
                viResult = visa32.ResultCode.VI_SUCCESS;
#else
                viResult = Resource.viSetAttribute(instrID, (int)visa32.Attribute.VI_ATTR_TMO_VALUE, time);
#endif
                if (viResult != visa32.ResultCode.VI_SUCCESS) { throw new Exception(viResult.ToString()); }
            }
            catch (Exception e) { throw e; }
        }
        /// <summary>設定されているタイムアウト時間[mS]取得</summary>
        /// 
        public int GetTimeout()
        {
            int time;
            try
            {
#if DEBUG_GPIB_NOT_CONNECT
                time = DefaultTimeout;
                viResult = visa32.ResultCode.VI_SUCCESS;
#else
                viResult = Resource.viGetAttribute(instrID, (int)visa32.Attribute.VI_ATTR_TMO_VALUE, out time);
#endif
                if (viResult != visa32.ResultCode.VI_SUCCESS) { throw new Exception(viResult.ToString()); }
                return time;
            }
            catch (Exception e) { throw e; }
        }
        #endregion

        #region 送信関連
        /// <summary>ASCII文字列送信</summary>
        /// <param name="Str">送信文字列</param>
        public void WriteAscii(string Str) { WriteBinary(Encoding.ASCII.GetBytes(Str)); }
        /// <summary>コマンド送信(改行コード自動追加)</summary>
        /// <param name="CMD">ASCIIコマンド</param>
        public void WriteAsciiCmdLine(string CMD) { WriteAscii(CMD + NewLine); }
        /// <summary>ByteデータをIEEEblokFormatに変換(ヘッダ情報生成・付加)して送信</summary>
        /// <param name="DAT">送信Byteデータ(自動ヘッダ情報生成および付加)</param>
        /// <param name="FixedMode">true:確定長フォーマットで送信 / false:不確定長フォーマットで送信</param>
        public void WriteByteIEEEblock(byte[] DAT, bool FixedMode) { WriteByteIEEEblock("", DAT, FixedMode); }
        /// <summary>ヘッダ文字列送信に合わせてByteデータをIEEEblokFormatに変換(ヘッダ情報生成・付加)してデータ送信</summary>
        /// <param name="Header">ヘッダ文字列(コマンド等)を指定(DATデータ前に追加)</param>
        /// <param name="DAT">送信Byteデータ(自動ヘッダ情報生成および付加)</param>
        /// <param name="FixedMode">true:確定長フォーマットで送信 / false:不確定長フォーマットで送信</param>
        public void WriteByteIEEEblock(string Header, byte[] DAT, bool FixedMode)
        {
            byte[] blockDAT;
            int count;
            string ieeeHeader;
            count = DAT.Length;
            if (!FixedMode) { ieeeHeader = "#0"; }
            else { ieeeHeader = "#" + DAT.Length.ToString().Length.ToString() + DAT.Length.ToString(); }
            blockDAT = Encoding.ASCII.GetBytes(Header);
            blockDAT = blockDAT.Concat(Encoding.ASCII.GetBytes(ieeeHeader)).ToArray();
            blockDAT = blockDAT.Concat(DAT).ToArray();
            WriteBinary(blockDAT);
        }
        /// <summary>Binaryデータ送信</summary>
        /// <param name="DAT">送信Byteデータ</param>
        public void WriteBinary(byte[] DAT)
        {
            int count;
            try
            {
#if DEBUG_GPIB_NOT_CONNECT
                viResult = visa32.ResultCode.VI_SUCCESS;
#else
                viResult = Resource.viWrite(instrID, DAT, DAT.Length, out count);
#endif
                if (viResult != visa32.ResultCode.VI_SUCCESS) { throw new Exception(viResult.ToString()); }
            }
            catch (Exception e) { throw e; }
        }
        #endregion

        #region 受信関連
        /// <summary>ASCIIデータ受信</summary>
        public string ReadAscii() { return Encoding.ASCII.GetString(ReadBinary()); }
        /// <summary>ASCIIデータ受信後、最終改行コードのみ削除</summary>
        public string ReadAsciiBlock()
        {
            string retStr;
            retStr = ReadAscii();
            if (retStr.Substring(retStr.Length - NewLine.Length, NewLine.Length) == NewLine) { return retStr.Substring(0, retStr.Length - 1); }
            else { return retStr; }
        }
        /// <summary>Binaryデータ受信後、IEEEblockフォーマットに合わせてデータのみ取得</summary>
        public byte[] ReadBinaryIEEEblock()
        {
            byte[] dat;
            int count;
            int headerCount;
            dat = ReadBinary();
            if (Encoding.ASCII.GetString(dat, 0, 1) != "#") { return dat; }
            headerCount = Convert.ToInt16(Encoding.ASCII.GetString(dat, 1, 1));
            count = Convert.ToInt32(Encoding.ASCII.GetString(dat, 2, headerCount));
            if (count == 0) { return dat.Skip(1).Take(dat.Length - 2).ToArray(); }
            else { return dat.Skip(2 + headerCount).Take(count).ToArray(); }
        }
        /// <summary>バイナリデータ受信</summary>
        public byte[] ReadBinary()
        {
            int count;
            byte[] byteBf = new byte[getByteLen];
            List<byte> retByte = new List<byte>();
            try
            {
                do
                {
#if DEBUG_GPIB_NOT_CONNECT
                    string strBF = "DEBUG_GPIB_NOT_CONNECT\n";
                    viResult = visa32.ResultCode.VI_SUCCESS;
                    count = strBF.Length;
                    byteBf = Encoding.ASCII.GetBytes(strBF);
#else
                    viResult = Resource.viRead(instrID, byteBf, (int)getByteLen, out count);
#endif
                    if (viResult < visa32.ResultCode.VI_SUCCESS) { throw new Exception(viResult.ToString()); }
                    retByte.AddRange(byteBf.Take(count));
                } while (viResult != visa32.ResultCode.VI_SUCCESS);    //SUCCESSのみ終了し、途中であればLoop
                return retByte.ToArray();
            }
            catch (Exception e) { throw e; }
        }
        #endregion
    }

    /// <summary>VISA32リソースクラス(※newはプログラム内に1個のみ)</summary>
    public class VisaResource : IDisposable
    {
        private int _defRm;
        private visa32 _vi;

        /// <summary>コンストラクタ</summary>
        public VisaResource() { _vi = new visa32("VISA32.DLL"); OpenResourceManager(); }

        /// <summary>コンストラクタ</summary>
        /// <param name="visa32dllFilePath">ロードするVISA32.DLLファイルを指定する</param>
        /// <exception cref="Exception"></exception>
        public VisaResource(string visa32dllFilePath)
        {
            if (!File.Exists(visa32dllFilePath)) { throw new Exception("DLL file Not Found."); }
            _vi = new visa32(visa32dllFilePath);
            OpenResourceManager();
        }

        /// <summary>リソース解放</summary>
        public void Dispose() { try { CloseResourceManager(); } catch { } _vi.Dispose(); }

        /// <summary>ResourceManagerを開く</summary>
        /// <exception cref="Exception"></exception>
        private void OpenResourceManager()
        {
            visa32.ResultCode err;
            try
            {
                err = _vi.viOpenDefaultRM(out _defRm);
                if (err != visa32.ResultCode.VI_SUCCESS) { throw new Exception(err.ToString()); }
            }
            catch (SystemException e) { throw e; }
        }

        /// <summary>ResourceManagerを閉じる</summary>
        /// <exception cref="Exception"></exception>
        private void CloseResourceManager()
        {
            visa32.ResultCode err;
            try
            {
                err = _vi.viClose(_defRm);
                if (err != visa32.ResultCode.VI_SUCCESS) { throw new Exception(err.ToString()); }
            }
            catch (SystemException e) { throw e; }
        }

        /// <summary>接続されている測定器VISAアドレス一覧を取得</summary>
        public List<string> GetInstrumentsAddress() { return GetInstrumentsAddress("?*"); }

        /// <summary>測定器VISAアドレス一覧を取得</summary>
        /// <param name="Expression">正規表現で測定器の絞込(例："?*"で全てのリソース、"(USB|GPIB)?*INSTR"でUSBとGPIBのみ)</param>
        public List<string> GetInstrumentsAddress(string Expression)
        {
            int findLabelSS;
            int instrCount;
            List<string> instr = new List<string>();
            StringBuilder instrumentAddr = new StringBuilder();

            visa32.ResultCode err;
            try
            {
                err = _vi.viFindRsrc(_defRm, Expression, out findLabelSS, out instrCount, instrumentAddr);
                switch (err)
                {
                    case visa32.ResultCode.VI_SUCCESS:
                        instr.Add(instrumentAddr.ToString());
                        break;
                    case visa32.ResultCode.VI_ERROR_RSRC_NFOUND:
                        return instr;
                    default:
                        throw new Exception(err.ToString());
                }
                int count = 1;
                while (count < instrCount)
                {
                    err = _vi.viFindNext(findLabelSS, instrumentAddr);
                    switch (err)
                    {
                        case visa32.ResultCode.VI_SUCCESS:
                            instr.Add(instrumentAddr.ToString());
                            count++;
                            break;
                        default:
                            throw new Exception(err.ToString());
                    }
                }
                return instr;
            }
            catch (SystemException ex) { throw ex; }
        }

        internal visa32.ResultCode viOpen(string viDesc, int mode, int timeout, out int vi)
        { return _vi.viOpen(_defRm, viDesc, mode, timeout, out vi); }

        internal visa32.ResultCode viClose(int vi)
        { return _vi.viClose(vi); }

        internal visa32.ResultCode viGetAttribute(int vi, int attrName, out int attrValue)
        { return _vi.viGetAttribute(vi, attrName, out attrValue); }

        internal visa32.ResultCode viSetAttribute(int vi, int attrName, int attrValue)
        { return _vi.viSetAttribute(vi, attrName, attrValue); }

        internal visa32.ResultCode viRead(int vi, byte[] buffer, int count, out int retCount)
        { return _vi.viRead(vi, buffer, count, out retCount); }

        internal visa32.ResultCode viWrite(int vi, byte[] buffer, int count, out int retCount)
        { return _vi.viWrite(vi, buffer, count, out retCount); }

        internal visa32.ResultCode viGpibControlREN(int vi, short mode)
        { return _vi.viGpibControlREN(vi, mode); }
    }

    /// <summary>visa32.dll動的ロードWrapper</summary>
    internal sealed class visa32 : dllDynamicLoad
    {
        #region contents
        /// <summary>属性列挙型</summary>
        public enum Attribute : int
        {
            VI_ATTR_RSRC_CLASS = -1073807359,
            VI_ATTR_RSRC_NAME = -1073807358,
            VI_ATTR_RSRC_IMPL_VERSION = 1073676291,
            VI_ATTR_RSRC_LOCK_STATE = 1073676292,
            VI_ATTR_MAX_QUEUE_LENGTH = 1073676293,
            VI_ATTR_USER_DATA = 1073676295,
            VI_ATTR_USER_DATA_32 = 1073676295,
            VI_ATTR_USER_DATA_64 = 1073676298,
            VI_ATTR_FDC_CHNL = 1073676301,
            VI_ATTR_FDC_MODE = 1073676303,
            VI_ATTR_FDC_GEN_SIGNAL_EN = 1073676305,
            VI_ATTR_FDC_USE_PAIR = 1073676307,
            VI_ATTR_SEND_END_EN = 1073676310,
            VI_ATTR_TERMCHAR = 1073676312,
            VI_ATTR_TMO_VALUE = 1073676314,
            VI_ATTR_GPIB_READDR_EN = 1073676315,
            VI_ATTR_IO_PROT = 1073676316,
            VI_ATTR_DMA_ALLOW_EN = 1073676318,
            VI_ATTR_ASRL_BAUD = 1073676321,
            VI_ATTR_ASRL_DATA_BITS = 1073676322,
            VI_ATTR_ASRL_PARITY = 1073676323,
            VI_ATTR_ASRL_STOP_BITS = 1073676324,
            VI_ATTR_ASRL_FLOW_CNTRL = 1073676325,
            VI_ATTR_RD_BUF_OPER_MODE = 1073676330,
            VI_ATTR_RD_BUF_SIZE = 1073676331,
            VI_ATTR_WR_BUF_OPER_MODE = 1073676333,
            VI_ATTR_WR_BUF_SIZE = 1073676334,
            VI_ATTR_SUPPRESS_END_EN = 1073676342,
            VI_ATTR_TERMCHAR_EN = 1073676344,
            VI_ATTR_DEST_ACCESS_PRIV = 1073676345,
            VI_ATTR_DEST_BYTE_ORDER = 1073676346,
            VI_ATTR_SRC_ACCESS_PRIV = 1073676348,
            VI_ATTR_SRC_BYTE_ORDER = 1073676349,
            VI_ATTR_SRC_INCREMENT = 1073676352,
            VI_ATTR_DEST_INCREMENT = 1073676353,
            VI_ATTR_WIN_ACCESS_PRIV = 1073676357,
            VI_ATTR_WIN_BYTE_ORDER = 1073676359,
            VI_ATTR_GPIB_ATN_STATE = 1073676375,
            VI_ATTR_GPIB_ADDR_STATE = 1073676380,
            VI_ATTR_GPIB_CIC_STATE = 1073676382,
            VI_ATTR_GPIB_NDAC_STATE = 1073676386,
            VI_ATTR_GPIB_SRQ_STATE = 1073676391,
            VI_ATTR_GPIB_SYS_CNTRL_STATE = 1073676392,
            VI_ATTR_GPIB_HS488_CBL_LEN = 1073676393,
            VI_ATTR_CMDR_LA = 1073676395,
            VI_ATTR_VXI_DEV_CLASS = 1073676396,
            VI_ATTR_MAINFRAME_LA = 1073676400,
            VI_ATTR_MANF_NAME = -1073807246,
            VI_ATTR_MODEL_NAME = -1073807241,
            VI_ATTR_VXI_VME_INTR_STATUS = 1073676427,
            VI_ATTR_VXI_TRIG_STATUS = 1073676429,
            VI_ATTR_VXI_VME_SYSFAIL_STATE = 1073676436,
            VI_ATTR_WIN_BASE_ADDR = 1073676440,
            VI_ATTR_WIN_BASE_ADDR_32 = 1073676440,
            VI_ATTR_WIN_BASE_ADDR_64 = 1073676443,
            VI_ATTR_WIN_SIZE = 1073676442,
            VI_ATTR_WIN_SIZE_32 = 1073676442,
            VI_ATTR_WIN_SIZE_64 = 1073676444,
            VI_ATTR_ASRL_AVAIL_NUM = 1073676460,
            VI_ATTR_MEM_BASE = 1073676461,
            VI_ATTR_MEM_BASE_32 = 1073676461,
            VI_ATTR_MEM_BASE_64 = 1073676496,
            VI_ATTR_ASRL_CTS_STATE = 1073676462,
            VI_ATTR_ASRL_DCD_STATE = 1073676463,
            VI_ATTR_ASRL_DSR_STATE = 1073676465,
            VI_ATTR_ASRL_DTR_STATE = 1073676466,
            VI_ATTR_ASRL_END_IN = 1073676467,
            VI_ATTR_ASRL_END_OUT = 1073676468,
            VI_ATTR_ASRL_REPLACE_CHAR = 1073676478,
            VI_ATTR_ASRL_RI_STATE = 1073676479,
            VI_ATTR_ASRL_RTS_STATE = 1073676480,
            VI_ATTR_ASRL_XON_CHAR = 1073676481,
            VI_ATTR_ASRL_XOFF_CHAR = 1073676482,
            VI_ATTR_WIN_ACCESS = 1073676483,
            VI_ATTR_RM_SESSION = 1073676484,
            VI_ATTR_VXI_LA = 1073676501,
            VI_ATTR_MANF_ID = 1073676505,
            VI_ATTR_MEM_SIZE = 1073676509,
            VI_ATTR_MEM_SIZE_32 = 1073676509,
            VI_ATTR_MEM_SIZE_64 = 1073676497,
            VI_ATTR_MEM_SPACE = 1073676510,
            VI_ATTR_MODEL_CODE = 1073676511,
            VI_ATTR_SLOT = 1073676520,
            VI_ATTR_INTF_INST_NAME = -1073807127,
            VI_ATTR_IMMEDIATE_SERV = 1073676544,
            VI_ATTR_INTF_PARENT_NUM = 1073676545,
            VI_ATTR_RSRC_SPEC_VERSION = 1073676656,
            VI_ATTR_INTF_TYPE = 1073676657,
            VI_ATTR_GPIB_PRIMARY_ADDR = 1073676658,
            VI_ATTR_GPIB_SECONDARY_ADDR = 1073676659,
            VI_ATTR_RSRC_MANF_NAME = -1073806988,
            VI_ATTR_RSRC_MANF_ID = 1073676661,
            VI_ATTR_INTF_NUM = 1073676662,
            VI_ATTR_TRIG_ID = 1073676663,
            VI_ATTR_GPIB_REN_STATE = 1073676673,
            VI_ATTR_GPIB_UNADDR_EN = 1073676676,
            VI_ATTR_DEV_STATUS_BYTE = 1073676681,
            VI_ATTR_FILE_APPEND_EN = 1073676690,
            VI_ATTR_VXI_TRIG_SUPPORT = 1073676692,
            VI_ATTR_TCPIP_ADDR = -1073806955,
            VI_ATTR_TCPIP_HOSTNAME = -1073806954,
            VI_ATTR_TCPIP_PORT = 1073676695,
            VI_ATTR_TCPIP_DEVICE_NAME = -1073806951,
            VI_ATTR_TCPIP_NODELAY = 1073676698,
            VI_ATTR_TCPIP_KEEPALIVE = 1073676699,
            VI_ATTR_4882_COMPLIANT = 1073676703,
            VI_ATTR_USB_SERIAL_NUM = -1073806944,
            VI_ATTR_USB_INTFC_NUM = 1073676705,
            VI_ATTR_USB_PROTOCOL = 1073676711,
            VI_ATTR_USB_MAX_INTR_SIZE = 1073676719,
            VI_ATTR_PXI_DEV_NUM = 1073676801,
            VI_ATTR_PXI_FUNC_NUM = 1073676802,
            VI_ATTR_PXI_BUS_NUM = 1073676805,
            VI_ATTR_PXI_CHASSIS = 1073676806,
            VI_ATTR_PXI_SLOTPATH = -1073806841,
            VI_ATTR_PXI_SLOT_LBUS_LEFT = 1073676808,
            VI_ATTR_PXI_SLOT_LBUS_RIGHT = 1073676809,
            VI_ATTR_PXI_TRIG_BUS = 1073676810,
            VI_ATTR_PXI_STAR_TRIG_BUS = 1073676811,
            VI_ATTR_PXI_STAR_TRIG_LINE = 1073676812,
            VI_ATTR_PXI_MEM_TYPE_BAR0 = 1073676817,
            VI_ATTR_PXI_MEM_TYPE_BAR1 = 1073676818,
            VI_ATTR_PXI_MEM_TYPE_BAR2 = 1073676819,
            VI_ATTR_PXI_MEM_TYPE_BAR3 = 1073676820,
            VI_ATTR_PXI_MEM_TYPE_BAR4 = 1073676821,
            VI_ATTR_PXI_MEM_TYPE_BAR5 = 1073676822,
            VI_ATTR_PXI_MEM_BASE_BAR0 = 1073676833,
            VI_ATTR_PXI_MEM_BASE_BAR1 = 1073676834,
            VI_ATTR_PXI_MEM_BASE_BAR2 = 1073676835,
            VI_ATTR_PXI_MEM_BASE_BAR3 = 1073676836,
            VI_ATTR_PXI_MEM_BASE_BAR4 = 1073676837,
            VI_ATTR_PXI_MEM_BASE_BAR5 = 1073676838,
            VI_ATTR_PXI_MEM_SIZE_BAR0 = 1073676849,
            VI_ATTR_PXI_MEM_SIZE_BAR1 = 1073676850,
            VI_ATTR_PXI_MEM_SIZE_BAR2 = 1073676851,
            VI_ATTR_PXI_MEM_SIZE_BAR3 = 1073676852,
            VI_ATTR_PXI_MEM_SIZE_BAR4 = 1073676853,
            VI_ATTR_PXI_MEM_SIZE_BAR5 = 1073676854,
            VI_ATTR_PXI_IS_EXPRESS = 1073676864,
            VI_ATTR_PXI_SLOT_LWIDTH = 1073676865,
            VI_ATTR_PXI_MAX_LWIDTH = 1073676866,
            VI_ATTR_PXI_ACTUAL_LWIDTH = 1073676867,
            VI_ATTR_PXI_DSTAR_BUS = 1073676868,
            VI_ATTR_PXI_DSTAR_SET = 1073676869,
            VI_ATTR_TCPIP_HISLIP_OVERLAP_EN = 1073677056,
            VI_ATTR_TCPIP_HISLIP_VERSION = 1073677057,
            VI_ATTR_TCPIP_HISLIP_MAX_MESSAGE_KB = 1073677058,
            VI_ATTR_JOB_ID = 1073692678,
            VI_ATTR_EVENT_TYPE = 1073692688,
            VI_ATTR_SIGP_STATUS_ID = 1073692689,
            VI_ATTR_RECV_TRIG_ID = 1073692690,
            VI_ATTR_INTR_STATUS_ID = 1073692707,
            VI_ATTR_STATUS = 1073692709,
            VI_ATTR_RET_COUNT = 1073692710,
            VI_ATTR_RET_COUNT_32 = 1073692710,
            VI_ATTR_RET_COUNT_64 = 1073692712,
            VI_ATTR_BUFFER = 1073692711,
            VI_ATTR_RECV_INTR_LEVEL = 1073692737,
            VI_ATTR_OPER_NAME = -1073790910,
            VI_ATTR_GPIB_RECV_CIC_STATE = 1073693075,
            VI_ATTR_RECV_TCPIP_ADDR = -1073790568,
            VI_ATTR_USB_RECV_INTR_SIZE = 1073693104,
            VI_ATTR_USB_RECV_INTR_DATA = -1073790543,
        }

        /// <summary>イベント種別列挙型</summary>
        public enum EventType : int
        {
            VI_EVENT_IO_COMPLETION = 1073684489,
            VI_EVENT_TRIG = -1073799158,
            VI_EVENT_SERVICE_REQ = 1073684491,
            VI_EVENT_CLEAR = 1073684493,
            VI_EVENT_EXCEPTION = -1073799154,
            VI_EVENT_GPIB_CIC = 1073684498,
            VI_EVENT_GPIB_TALK = 1073684499,
            VI_EVENT_GPIB_LISTEN = 1073684500,
            VI_EVENT_VXI_VME_SYSFAIL = 1073684509,
            VI_EVENT_VXI_VME_SYSRESET = 1073684510,
            VI_EVENT_VXI_SIGP = 1073684512,
            VI_EVENT_VXI_VME_INTR = -1073799135,
            VI_EVENT_PXI_INTR = 1073684514,
            VI_EVENT_TCPIP_CONNECT = 1073684534,
            VI_EVENT_USB_INTR = 1073684535,
            VI_ALL_ENABLED_EVENTS = 1073709055,
        }

        /// <summary>結果コード列挙型</summary>
        public enum ResultCode : int
        {
            VI_SUCCESS = 0,
            VI_SUCCESS_EVENT_EN = 1073676290,
            VI_SUCCESS_EVENT_DIS = 1073676291,
            VI_SUCCESS_QUEUE_EMPTY = 1073676292,
            VI_SUCCESS_TERM_CHAR = 1073676293,
            VI_SUCCESS_MAX_CNT = 1073676294,
            VI_SUCCESS_DEV_NPRESENT = 1073676413,
            VI_SUCCESS_TRIG_MAPPED = 1073676414,
            VI_SUCCESS_QUEUE_NEMPTY = 1073676416,
            VI_SUCCESS_NCHAIN = 1073676440,
            VI_SUCCESS_NESTED_SHARED = 1073676441,
            VI_SUCCESS_NESTED_EXCLUSIVE = 1073676442,
            VI_SUCCESS_SYNC = 1073676443,
            VI_WARN_QUEUE_OVERFLOW = 1073676300,
            VI_WARN_CONFIG_NLOADED = 1073676407,
            VI_WARN_NULL_OBJECT = 1073676418,
            VI_WARN_NSUP_ATTR_STATE = 1073676420,
            VI_WARN_UNKNOWN_STATUS = 1073676421,
            VI_WARN_NSUP_BUF = 1073676424,
            VI_WARN_EXT_FUNC_NIMPL = 1073676457,
            VI_ERROR_SYSTEM_ERROR = -1073807360,
            VI_ERROR_INV_OBJECT = -1073807346,
            VI_ERROR_RSRC_LOCKED = -1073807345,
            VI_ERROR_INV_EXPR = -1073807344,
            VI_ERROR_RSRC_NFOUND = -1073807343,
            VI_ERROR_INV_RSRC_NAME = -1073807342,
            VI_ERROR_INV_ACC_MODE = -1073807341,
            VI_ERROR_TMO = -1073807339,
            VI_ERROR_CLOSING_FAILED = -1073807338,
            VI_ERROR_INV_DEGREE = -1073807333,
            VI_ERROR_INV_JOB_ID = -1073807332,
            VI_ERROR_NSUP_ATTR = -1073807331,
            VI_ERROR_NSUP_ATTR_STATE = -1073807330,
            VI_ERROR_ATTR_READONLY = -1073807329,
            VI_ERROR_INV_LOCK_TYPE = -1073807328,
            VI_ERROR_INV_ACCESS_KEY = -1073807327,
            VI_ERROR_INV_EVENT = -1073807322,
            VI_ERROR_INV_MECH = -1073807321,
            VI_ERROR_HNDLR_NINSTALLED = -1073807320,
            VI_ERROR_INV_HNDLR_REF = -1073807319,
            VI_ERROR_INV_CONTEXT = -1073807318,
            VI_ERROR_QUEUE_OVERFLOW = -1073807315,
            VI_ERROR_NENABLED = -1073807313,
            VI_ERROR_ABORT = -1073807312,
            VI_ERROR_RAW_WR_PROT_VIOL = -1073807308,
            VI_ERROR_RAW_RD_PROT_VIOL = -1073807307,
            VI_ERROR_OUTP_PROT_VIOL = -1073807306,
            VI_ERROR_INP_PROT_VIOL = -1073807305,
            VI_ERROR_BERR = -1073807304,
            VI_ERROR_IN_PROGRESS = -1073807303,
            VI_ERROR_INV_SETUP = -1073807302,
            VI_ERROR_QUEUE_ERROR = -1073807301,
            VI_ERROR_ALLOC = -1073807300,
            VI_ERROR_INV_MASK = -1073807299,
            VI_ERROR_IO = -1073807298,
            VI_ERROR_INV_FMT = -1073807297,
            VI_ERROR_NSUP_FMT = -1073807295,
            VI_ERROR_LINE_IN_USE = -1073807294,
            VI_ERROR_NSUP_MODE = -1073807290,
            VI_ERROR_SRQ_NOCCURRED = -1073807286,
            VI_ERROR_INV_SPACE = -1073807282,
            VI_ERROR_INV_OFFSET = -1073807279,
            VI_ERROR_INV_WIDTH = -1073807278,
            VI_ERROR_NSUP_OFFSET = -1073807276,
            VI_ERROR_NSUP_VAR_WIDTH = -1073807275,
            VI_ERROR_WINDOW_NMAPPED = -1073807273,
            VI_ERROR_RESP_PENDING = -1073807271,
            VI_ERROR_NLISTENERS = -1073807265,
            VI_ERROR_NCIC = -1073807264,
            VI_ERROR_NSYS_CNTLR = -1073807263,
            VI_ERROR_NSUP_OPER = -1073807257,
            VI_ERROR_INTR_PENDING = -1073807256,
            VI_ERROR_ASRL_PARITY = -1073807254,
            VI_ERROR_ASRL_FRAMING = -1073807253,
            VI_ERROR_ASRL_OVERRUN = -1073807252,
            VI_ERROR_TRIG_NMAPPED = -1073807250,
            VI_ERROR_NSUP_ALIGN_OFFSET = -1073807248,
            VI_ERROR_USER_BUF = -1073807247,
            VI_ERROR_RSRC_BUSY = -1073807246,
            VI_ERROR_NSUP_WIDTH = -1073807242,
            VI_ERROR_INV_PARAMETER = -1073807240,
            VI_ERROR_INV_PROT = -1073807239,
            VI_ERROR_INV_SIZE = -1073807237,
            VI_ERROR_WINDOW_MAPPED = -1073807232,
            VI_ERROR_NIMPL_OPER = -1073807231,
            VI_ERROR_INV_LENGTH = -1073807229,
            VI_ERROR_INV_MODE = -1073807215,
            VI_ERROR_SESN_NLOCKED = -1073807204,
            VI_ERROR_MEM_NSHARED = -1073807203,
            VI_ERROR_LIBRARY_NFOUND = -1073807202,
            VI_ERROR_NSUP_INTR = -1073807201,
            VI_ERROR_INV_LINE = -1073807200,
            VI_ERROR_FILE_ACCESS = -1073807199,
            VI_ERROR_FILE_IO = -1073807198,
            VI_ERROR_NSUP_LINE = -1073807197,
            VI_ERROR_NSUP_MECH = -1073807196,
            VI_ERROR_INTF_NUM_NCONFIG = -1073807195,
            VI_ERROR_CONN_LOST = -1073807194,
            VI_ERROR_MACHINE_NAVAIL = -1073807193,
            VI_ERROR_NPERMISSION = -1073807192,

        }

        /// <summary>その他のVISA定義</summary>
        public enum OtherVisaDefinition : short
        {
            VI_FIND_BUFLEN = 256,

            VI_NULL = 0,
            VI_TRUE = 1,
            VI_FALSE = 0,

            VI_INTF_GPIB = 1,
            VI_INTF_VXI = 2,
            VI_INTF_GPIB_VXI = 3,
            VI_INTF_ASRL = 4,
            VI_INTF_PXI = 5,
            VI_INTF_TCPIP = 6,
            VI_INTF_USB = 7,

            VI_PROT_NORMAL = 1,
            VI_PROT_FDC = 2,
            VI_PROT_HS488 = 3,
            VI_PROT_4882_STRS = 4,
            VI_PROT_USBTMC_VENDOR = 5,

            VI_FDC_NORMAL = 1,
            VI_FDC_STREAM = 2,

            VI_LOCAL_SPACE = 0,
            VI_A16_SPACE = 1,
            VI_A24_SPACE = 2,
            VI_A32_SPACE = 3,
            VI_A64_SPACE = 4,
            VI_PXI_ALLOC_SPACE = 9,
            VI_PXI_CFG_SPACE = 10,
            VI_PXI_BAR0_SPACE = 11,
            VI_PXI_BAR1_SPACE = 12,
            VI_PXI_BAR2_SPACE = 13,
            I_PXI_BAR3_SPACE = 14,
            VI_PXI_BAR4_SPACE = 15,
            VI_PXI_BAR5_SPACE = 16,
            VI_OPAQUE_SPACE = -1,

            VI_UNKNOWN_LA = -1,
            VI_UNKNOWN_SLOT = -1,
            VI_UNKNOWN_LEVEL = -1,
            VI_UNKNOWN_CHASSIS = -1,

            VI_QUEUE = 1,
            VI_HNDLR = 2,
            VI_SUSPEND_HNDLR = 4,
            VI_ALL_MECH = -1,
            VI_ANY_HNDLR = 0,

            VI_TRIG_ALL = -2,
            VI_TRIG_SW = -1,
            VI_TRIG_TTL0 = 0,
            VI_TRIG_TTL1 = 1,
            VI_TRIG_TTL2 = 2,
            VI_TRIG_TTL3 = 3,
            VI_TRIG_TTL4 = 4,
            VI_TRIG_TTL5 = 5,
            VI_TRIG_TTL6 = 6,
            VI_TRIG_TTL7 = 7,
            VI_TRIG_ECL0 = 8,
            VI_TRIG_ECL1 = 9,
            VI_TRIG_PANEL_IN = 27,
            VI_TRIG_PANEL_OUT = 28,

            VI_TRIG_PROT_DEFAULT = 0,
            VI_TRIG_PROT_ON = 1,
            VI_TRIG_PROT_OFF = 2,
            VI_TRIG_PROT_SYNC = 5,
            VI_TRIG_PROT_RESERVE = 6,
            VI_TRIG_PROT_UNRESERVE = 7,

            VI_READ_BUF = 1,
            VI_WRITE_BUF = 2,
            VI_READ_BUF_DISCARD = 4,
            VI_WRITE_BUF_DISCARD = 8,
            VI_IO_IN_BUF = 16,
            VI_IO_OUT_BUF = 32,
            VI_IO_IN_BUF_DISCARD = 64,
            VI_IO_OUT_BUF_DISCARD = 128,

            VI_FLUSH_ON_ACCESS = 1,
            VI_FLUSH_WHEN_FULL = 2,
            VI_FLUSH_DISABLE = 3,

            VI_NMAPPED = 1,
            VI_USE_OPERS = 2,
            VI_DEREF_ADDR = 3,
            VI_DEREF_ADDR_BYTE_SWAP = 4,

            VI_TMO_IMMEDIATE = 0,
            VI_TMO_INFINITE = -1,

            VI_NO_LOCK = 0,
            VI_EXCLUSIVE_LOCK = 1,
            VI_SHARED_LOCK = 2,
            VI_LOAD_CONFIG = 4,

            VI_NO_SEC_ADDR = -1,

            VI_ASRL_PAR_NONE = 0,
            VI_ASRL_PAR_ODD = 1,
            VI_ASRL_PAR_EVEN = 2,
            VI_ASRL_PAR_MARK = 3,
            VI_ASRL_PAR_SPACE = 4,

            VI_ASRL_STOP_ONE = 10,
            VI_ASRL_STOP_ONE5 = 15,
            VI_ASRL_STOP_TWO = 20,

            VI_ASRL_FLOW_NONE = 0,
            VI_ASRL_FLOW_XON_XOFF = 1,
            VI_ASRL_FLOW_RTS_CTS = 2,
            VI_ASRL_FLOW_DTR_DSR = 4,

            VI_ASRL_END_NONE = 0,
            VI_ASRL_END_LAST_BIT = 1,
            VI_ASRL_END_TERMCHAR = 2,
            VI_ASRL_END_BREAK = 3,

            VI_STATE_ASSERTED = 1,
            VI_STATE_UNASSERTED = 0,
            VI_STATE_UNKNOWN = -1,

            VI_BIG_ENDIAN = 0,
            VI_LITTLE_ENDIAN = 1,

            VI_DATA_PRIV = 0,
            VI_DATA_NPRIV = 1,
            VI_PROG_PRIV = 2,
            VI_PROG_NPRIV = 3,
            VI_BLCK_PRIV = 4,
            VI_BLCK_NPRIV = 5,
            VI_D64_PRIV = 6,
            VI_D64_NPRIV = 7,

            VI_WIDTH_8 = 1,
            VI_WIDTH_16 = 2,
            VI_WIDTH_32 = 4,
            VI_WIDTH_64 = 8,

            VI_GPIB_REN_DEASSERT = 0,
            VI_GPIB_REN_ASSERT = 1,
            VI_GPIB_REN_DEASSERT_GTL = 2,
            VI_GPIB_REN_ASSERT_ADDRESS = 3,
            VI_GPIB_REN_ASSERT_LLO = 4,
            VI_GPIB_REN_ASSERT_ADDRESS_LLO = 5,
            VI_GPIB_REN_ADDRESS_GTL = 6,

            VI_GPIB_ATN_DEASSERT = 0,
            VI_GPIB_ATN_ASSERT = 1,
            VI_GPIB_ATN_DEASSERT_HANDSHAKE = 2,
            VI_GPIB_ATN_ASSERT_IMMEDIATE = 3,

            VI_GPIB_HS488_DISABLED = 0,
            VI_GPIB_HS488_NIMPL = -1,

            VI_GPIB_UNADDRESSED = 0,
            VI_GPIB_TALKER = 1,
            VI_GPIB_LISTENER = 2,

            VI_VXI_CMD16 = 512,
            VI_VXI_CMD16_RESP16 = 514,
            VI_VXI_RESP16 = 2,
            VI_VXI_CMD32 = 1024,
            VI_VXI_CMD32_RESP16 = 1026,
            VI_VXI_CMD32_RESP32 = 1028,
            VI_VXI_RESP32 = 4,

            VI_ASSERT_SIGNAL = -1,
            VI_ASSERT_USE_ASSIGNED = 0,
            VI_ASSERT_IRQ1 = 1,
            VI_ASSERT_IRQ2 = 2,
            VI_ASSERT_IRQ3 = 3,
            VI_ASSERT_IRQ4 = 4,
            VI_ASSERT_IRQ5 = 5,
            VI_ASSERT_IRQ6 = 6,
            VI_ASSERT_IRQ7 = 7,

            VI_UTIL_ASSERT_SYSRESET = 1,
            VI_UTIL_ASSERT_SYSFAIL = 2,
            VI_UTIL_DEASSERT_SYSFAIL = 3,

            VI_VXI_CLASS_MEMORY = 0,
            VI_VXI_CLASS_EXTENDED = 1,
            VI_VXI_CLASS_MESSAGE = 2,
            VI_VXI_CLASS_REGISTER = 3,
            VI_VXI_CLASS_OTHER = 4,
            VI_PXI_ADDR_NONE = 0,
            VI_PXI_ADDR_MEM = 1,
            VI_PXI_ADDR_IO = 2,
            VI_PXI_ADDR_CFG = 3,

            VI_TRIG_UNKNOWN = -1,
            VI_PXI_LBUS_UNKNOWN = -1,
            VI_PXI_LBUS_NONE = 0,
            VI_PXI_LBUS_STAR_TRIG_BUS_0 = 1000,
            VI_PXI_LBUS_STAR_TRIG_BUS_1 = 1001,
            VI_PXI_LBUS_STAR_TRIG_BUS_2 = 1002,
            VI_PXI_LBUS_STAR_TRIG_BUS_3 = 1003,
            VI_PXI_LBUS_STAR_TRIG_BUS_4 = 1004,
            VI_PXI_LBUS_STAR_TRIG_BUS_5 = 1005,
            VI_PXI_LBUS_STAR_TRIG_BUS_6 = 1006,
            VI_PXI_LBUS_STAR_TRIG_BUS_7 = 1007,
            VI_PXI_LBUS_STAR_TRIG_BUS_8 = 1008,
            VI_PXI_LBUS_STAR_TRIG_BUS_9 = 1009,
            VI_PXI_STAR_TRIG_CONTROLLER = 1413,

        }

        /// <summary>下位互換性マクロ</summary>
        public enum BackwardCompatibilityMacros : int
        {
            //Backward Compatibility Macros
            VI_ERROR_INV_SESSION = -1073807346,
            VI_NORMAL = 1,
            VI_FDC = 2,
            VI_HS488 = 3,
            VI_ASRL488 = 4,
            VI_ASRL_IN_BUF = 16,
            VI_ASRL_OUT_BUF = 32,
            VI_ASRL_IN_BUF_DISCARD = 64,
            VI_ASRL_OUT_BUF_DISCARD = 128
        }
        #endregion

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate int delegate_viOpenDefaultRM(out int sesn);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate int delegate_viFindRsrc(int sesn, string expr, out int vi, out int retCount, StringBuilder desc);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate int delegate_viFindNext(int vi, StringBuilder desc);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate int delegate_viOpen(int sesn, string viDesc, int mode, int timeout, out int vi);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate int delegate_viClose(int vi);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate int delegate_viGetAttribute(int vi, int attrName, out int attrValue);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate int delegate_viSetAttribute(int vi, int attrName, int attrValue);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate int delegate_viRead(int vi, byte[] buffer, int count, out int retCount);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate int delegate_viWrite(int vi, byte[] buffer, int count, out int retCount);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate int delegate_viGpibControlREN(int vi, short mode);

        private delegate_viOpenDefaultRM _viOpenDefaultRM;
        private delegate_viFindRsrc _viFindRsrc;
        private delegate_viFindNext _viFindNext;
        private delegate_viOpen _viOpen;
        private delegate_viClose _viClose;
        private delegate_viGetAttribute _viGetAttribute;
        private delegate_viSetAttribute _viSetAttribute;
        private delegate_viRead _viRead;
        private delegate_viWrite _viWrite;
        private delegate_viGpibControlREN _viGpibControlREN;

        public visa32(string dllPath) : base(dllPath)
        {
            //_viOpenDefaultRM = LoadFunction<delegate_viOpenDefaultRM>("#141");
            //_viFindRsrc = LoadFunction<delegate_viFindRsrc>("#129");
            //_viFindNext = LoadFunction<delegate_viFindNext>("#130");
            //_viOpen = LoadFunction<delegate_viOpen>("#131");
            //_viClose = LoadFunction<delegate_viClose>("#132");
            //_viGetAttribute = LoadFunction<delegate_viGetAttribute>("#133");
            //_viSetAttribute = LoadFunction<delegate_viSetAttribute>("#134");
            //_viRead = LoadFunction<delegate_viRead>("#256");
            //_viWrite = LoadFunction<delegate_viWrite>("#257");
            //_viGpibControlREN = LoadFunction<delegate_viGpibControlREN>("#208");
            _viOpenDefaultRM = LoadFunction<delegate_viOpenDefaultRM>("viOpenDefaultRM");
            _viFindRsrc = LoadFunction<delegate_viFindRsrc>("viFindRsrc");
            _viFindNext = LoadFunction<delegate_viFindNext>("viFindNext");
            _viOpen = LoadFunction<delegate_viOpen>("viOpen");
            _viClose = LoadFunction<delegate_viClose>("viClose");
            _viGetAttribute = LoadFunction<delegate_viGetAttribute>("viGetAttribute");
            _viSetAttribute = LoadFunction<delegate_viSetAttribute>("viSetAttribute");
            _viRead = LoadFunction<delegate_viRead>("viRead");
            _viWrite = LoadFunction<delegate_viWrite>("viWrite");
            _viGpibControlREN = LoadFunction<delegate_viGpibControlREN>("viGpibControlREN");
        }

        private ResultCode resultCodeTryParse(int code)
        {
            if (Enum.IsDefined(typeof(ResultCode), code)) { return (ResultCode)code; }
            else { throw new Exception("An abnormal result code was returned."); }
        }

        public ResultCode viOpenDefaultRM(out int sesn)
        { return resultCodeTryParse(_viOpenDefaultRM(out sesn)); }

        public ResultCode viFindRsrc(int sesn, string expr, out int vi, out int retCount, StringBuilder desc)
        { return resultCodeTryParse(_viFindRsrc(sesn, expr, out vi, out retCount, desc)); }

        public ResultCode viFindNext(int vi, StringBuilder desc)
        { return resultCodeTryParse(_viFindNext(vi, desc)); }

        public ResultCode viOpen(int sesn, string viDesc, int mode, int timeout, out int vi)
        { return resultCodeTryParse(_viOpen(sesn, viDesc, mode, timeout, out vi)); }

        public ResultCode viClose(int vi)
        { return resultCodeTryParse(_viClose(vi)); }

        public ResultCode viGetAttribute(int vi, int attrName, out int attrValue)
        { return resultCodeTryParse(_viGetAttribute(vi, attrName, out attrValue)); }

        public ResultCode viSetAttribute(int vi, int attrName, int attrValue)
        { return resultCodeTryParse(_viSetAttribute(vi, attrName, attrValue)); }

        public ResultCode viRead(int vi, byte[] buffer, int count, out int retCount)
        { return resultCodeTryParse(_viRead(vi, buffer, count, out retCount)); }

        public ResultCode viWrite(int vi, byte[] buffer, int count, out int retCount)
        { return resultCodeTryParse(_viWrite(vi, buffer, count, out retCount)); }

        public ResultCode viGpibControlREN(int vi, short mode)
        { return resultCodeTryParse(_viGpibControlREN(vi, mode)); }

    }

    /// <summary>DLL動的ロード用基底クラス</summary>
    internal abstract class dllDynamicLoad : IDisposable
    {
        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string dllFilePath);

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr module, string functionName);

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr module);

        private IntPtr _dllPtr;

        /// <summary>コンストラクタ</summary>
        /// <param name="dllPath">dllファイルパス</param>
        /// <exception cref="Exception"></exception>
        public dllDynamicLoad(string dllPath)
        {
            _dllPtr = LoadLibrary(dllPath);
            if (_dllPtr == IntPtr.Zero) { throw new Exception($"Failed to load {dllPath}. Error code: {Marshal.GetLastWin32Error()}"); }
        }

        /// <summary>関数ポインタ取得</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entryPoint">関数名</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected T LoadFunction<T>(string entryPoint)
        {
            // 関数ポインタ取得
            IntPtr funcPtr = GetProcAddress(_dllPtr, entryPoint);
            if (funcPtr == IntPtr.Zero) { throw new Exception($"Failed to get function pointer for {entryPoint}. Error code: {Marshal.GetLastWin32Error()}"); }
            // 関数ポインタをdelegateに変換
            return (T)(object)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(T));
        }

        public void Dispose() { FreeLibrary(_dllPtr); }
    }

    /// <summary>IEEE488.2準拠の制御用</summary>
    public static class IEEE488dot2
    {
        /// <summary>IEEE488.2準拠の計測器制御クラス</summary>
        public class Instrument : VisaControl
        {
            /// <summary>コンストラクタ</summary>
            /// <param name="resource">VISA Resourceを指定</param>
            /// <param name="address">VISA Addressを指定</param>
            public Instrument(VisaResource resource, string address)
                : base(resource, address) { }

            /// <summary>コンストラクタ</summary>
            /// <param name="resource">VISA Resourceを指定</param>
            /// <param name="address">VISA Addressを指定</param>
            /// <param name="bufferLength">受信時のバッファ長を指定</param>
            /// <param name="defTimeout">クラスデフォルトのタイムアウト時間をミリ秒で指定</param>
            /// <param name="NewLineCode">改行コードを指定</param>
            public Instrument(VisaResource resource, string address, short bufferLength, short defTimeout, string NewLineCode)
                : base(resource, address, bufferLength, defTimeout, NewLineCode) { }

            #region IEEE488 COM Command

            /// <summary>
            /// ステータスクリア(*CLS)<br/>
            /// ステータスバイト、イベントステータスレジスタ、エラーキューを含むすべてのイベントレジスタをクリア<br/>
            /// *OPC/*OPC?による完了待ち動作を取消し
            /// </summary>
            public void StatusClear()
            {
                try { WriteAsciiCmdLine("*CLS"); }
                catch (Exception e) { throw new Exception("Error>StatusClear", e); }
            }

            /// <summary>イベントステータスのイネーブルレジスタ設定(*ESE ESE.NR)</summary>
            /// <param name="ESE">設定値</param>
            public void SetEventStatusEnable(IEEE488_Register ESE)
            {
                try { WriteAsciiCmdLine("*ESE " + ESE.NR.ToString()); }
                catch (Exception e) { throw new Exception("Error>SetEventStatusEnable", e); }
            }

            /// <summary>イベントステータスのイネーブルレジスタ設定状態取得(*ESE?)</summary>
            /// <returns>設定値</returns>
            public IEEE488_Register GetEventStatusEnable()
            {
                try
                {
                    WriteAsciiCmdLine("*ESE?");
                    return new IEEE488_Register(UInt16.Parse(ReadAsciiBlock()));
                }
                catch (Exception e) { throw new Exception("Error>GetEventStatusON", e); }
            }

            /// <summary>イベントレジスタの取得(*ESR?)</summary>
            /// <returns>レジスタ値</returns>
            public IEEE488_Register GetEventStatusRegister()
            {
                try
                {
                    WriteAsciiCmdLine("*ESR?");
                    return new IEEE488_Register(UInt16.Parse(ReadAsciiBlock()));
                }
                catch (Exception e) { throw new Exception("Error>GetEventStatusON", e); }
            }

            /// <summary>識別問合せ(*IDN?)</summary>
            /// <returns>装置の識別情報</returns>
            public IEEE488_IDN GetInstrumentIdentification()
            {
                return new IEEE488_IDN(GetInstrumentIdentificationAscii());
            }

            /// <summary>識別問合せ(*IDN?)</summary>
            /// <returns>装置の識別情報</returns>
            public string GetInstrumentIdentificationAscii()
            {
                try
                {
                    WriteAsciiCmdLine("*IDN?");
                    return ReadAsciiBlock();
                }
                catch (Exception e) { throw new Exception("Error>GetInstrumentIdentification", e); }
            }

            /// <summary>装置を現在の状態に設定するのに必要な情報の取得(*LRN?)</summary>
            /// <returns>受信データ(装置に応じて要変換)</returns>
            public byte[] InstrumentLearn()
            {
                try
                {
                    WriteAsciiCmdLine("*LRN?");
                    return ReadBinary();
                }
                catch (Exception e) { throw new Exception("Error>InstrumentLearn", e); }
            }

            /// <summary>現在の動作完了時にイベントレジスタの動作完了ビットに1を設定(*OPC)</summary>
            public void SetOperationComplete()
            {
                try { WriteAsciiCmdLine("*OPC"); }
                catch (Exception e) { throw new Exception("Error>SetOperationComplete", e); }
            }

            /// <summary>動作完了後に出力バッファへ1を返す(*OPC?)</summary>
            /// <returns></returns>
            public bool GetOperationComplete()
            {
                try
                {
                    WriteAsciiCmdLine("*OPC?");
                    return Convert.ToBoolean(int.Parse(ReadAsciiBlock()));
                }
                catch (Exception e) { throw new Exception("Error>GetOperationComplete", e); }
            }

            /// <summary>装備オプションの取得(*OPT?)</summary>
            /// <returns>オプション文字列(装置に応じて要変換)</returns>
            public string GetOption()
            {
                try
                {
                    WriteAsciiCmdLine("*OPT?");
                    return ReadAsciiBlock();
                }
                catch (Exception e) { throw new Exception("Error>GetOption", e); }
            }

            /// <summary>電源導入ステータスの設定(*PSC State)</summary>
            /// <param name="State">設定ステータス(true:1/false:0)</param>
            public void SetPowerOnStatusClear(bool State)
            {
                try { WriteAsciiCmdLine("*PSC " + Convert.ToInt32(State).ToString()); }
                catch (Exception e) { throw new Exception("Error>SetPowerOnStatusClear", e); }
            }

            /// <summary>電源導入ステータスの設定取得(*PSC?)</summary>
            /// <returns>設定ステータス(true:1/false:0)</returns>
            public bool GetPowerOnStatusClear()
            {
                try
                {
                    WriteAsciiCmdLine("*PSC?");
                    return Convert.ToBoolean(int.Parse(ReadAsciiBlock()));
                }
                catch (Exception e) { throw new Exception("Error>GetPowerOnStatusClear", e); }
            }

            /// <summary>装置の状態をステートファイルからリコールする(*RCL)</summary>
            public void Recall() { Recall(""); }

            /// <summary>装置の状態をステートファイルからリコールする(*RCL arg)</summary>
            /// <param name="arg">ステートファイル指定(装置に応じて要設定)</param>
            public void Recall(string arg)
            {
                try
                {
                    if (string.IsNullOrEmpty(arg)) { WriteAsciiCmdLine("*RCL"); }
                    else { WriteAsciiCmdLine("*RCL " + arg); }
                }
                catch (Exception e) { throw new Exception("Error>Recall", e); }
            }

            /// <summary>装置を工場出荷状態時の規定値に初期化(*RST)</summary>
            /// <exception cref="Exception"></exception>
            public void Reset()
            {
                try { WriteAsciiCmdLine("*RST"); }
                catch (Exception e) { throw new Exception("Error>Reset", e); }
            }

            /// <summary>装置の状態をステートファイルに保存する</summary>
            public void Save() { Save(""); }

            /// <summary>装置の状態をステートファイルに保存する(*SAV arg)</summary>
            /// <param name="arg">ステートファイル指定(装置に応じて要設定)</param>
            public void Save(string arg)
            {
                try
                {
                    if (string.IsNullOrEmpty(arg)) { WriteAsciiCmdLine("*SAV"); }
                    else { WriteAsciiCmdLine("*SAV " + arg); }
                }
                catch (Exception e) { throw new Exception("Error>Save", e); }
            }

            /// <summary>ステータスバイトレジスタのイネーブルレジスタ設定(*SRE SRE.NR)</summary>
            /// <param name="SRE">設定値</param>
            public void SetServiceRequestEnable(IEEE488_Register SRE)
            {
                try { WriteAsciiCmdLine("*SRE " + SRE.NR.ToString()); }
                catch (Exception e) { throw new Exception("Error>SetEventStatusEnable", e); }
            }

            /// <summary>ステータスバイトレジスタのイネーブルレジスタ設定状態取得(*SRE?)</summary>
            /// <returns>設定値</returns>
            public IEEE488_Register GetServiceRequestEnable()
            {
                try
                {
                    WriteAsciiCmdLine("*SRE?");
                    return new IEEE488_Register(UInt16.Parse(ReadAsciiBlock()));
                }
                catch (Exception e) { throw new Exception("Error>GetServiceRequestEnable", e); }
            }

            /// <summary>ステータスバイトレジスタの取得(*STB?)</summary>
            /// <returns>レジスタ値</returns>
            public IEEE488_Register GetStatusByte()
            {
                try
                {
                    WriteAsciiCmdLine("*STB?");
                    return new IEEE488_Register(UInt16.Parse(ReadAsciiBlock()));
                }
                catch (Exception e) { throw new Exception("Error>GetStatusByte", e); }
            }

            /// <summary>トリガ設定(*TRG)<br/>
            /// (装置に応じて動作は異なる)</summary>
            public void Trigger()
            {
                try { WriteAsciiCmdLine("*TRG"); }
                catch (Exception e) { throw new Exception("Error>Trigger", e); }
            }

            /// <summary>装置のセルフテスト(*TST?)</summary>
            /// <returns>テスト結果(装置に応じて要変換)</returns>
            public byte[] GetTest()
            {
                try
                {
                    WriteAsciiCmdLine("*TST?");
                    return ReadBinary();
                }
                catch (Exception e) { throw new Exception("Error>GetTest", e); }
            }

            /// <summary>保留中の全処理が完了するまで待つ(*WAI)</summary>
            public void Wait()
            {
                try { WriteAsciiCmdLine("*WAI"); }
                catch (Exception e) { throw new Exception("Error>Wait", e); }
            }
            #endregion

            #region Short CMD

            /// <summary>
            /// ステータスクリア(*CLS)<br/>
            /// ステータスバイト、イベントステータスレジスタ、エラーキューを含むすべてのイベントレジスタをクリア<br/>
            /// *OPC/*OPC?による完了待ち動作を取消し
            /// </summary>
            public void CLS() { StatusClear(); }

            /// <summary>イベントステータスのイネーブルレジスタ設定(*ESE ESE.NR)</summary>
            /// <param name="ESE">設定値</param>
            public void ESE(IEEE488_Register ESE) { SetEventStatusEnable(ESE); }

            /// <summary>イベントステータスのイネーブルレジスタ設定状態取得(*ESE?)</summary>
            /// <returns>設定値</returns>
            public IEEE488_Register ESE() { return GetEventStatusEnable(); }

            /// <summary>イベントレジスタの取得(*ESR?)</summary>
            /// <returns>レジスタ値</returns>
            public IEEE488_Register ESR() { return GetEventStatusEnable(); }

            /// <summary>識別問合せ(*IDN?)</summary>
            /// <returns>装置の識別情報</returns>
            public IEEE488_IDN IDN() { return GetInstrumentIdentification(); }

            /// <summary>装置を現在の状態に設定するのに必要な情報の取得(*LRN?)</summary>
            /// <returns>受信データ(装置に応じて要変換)</returns>
            public string LRN() { return Encoding.ASCII.GetString(InstrumentLearn()); }

            /// <summary>現在の動作完了時にイベントレジスタの動作完了ビットに1を設定(*OPC)</summary>
            public void OPC() { SetOperationComplete(); }

            /// <summary>動作完了後に出力バッファへ1を返す(*OPC?)</summary>
            /// <returns></returns>
            public bool OPCQ() { return GetOperationComplete(); }

            /// <summary>装備オプションの取得(*OPT?)</summary>
            /// <returns>オプション文字列(装置に応じて要変換)</returns>
            public string OPT() { return GetOption(); }

            /// <summary>電源導入ステータスの設定(*PSC State)</summary>
            /// <param name="State">設定ステータス(true:1/false:0)</param>
            public void PSC(bool State) { SetPowerOnStatusClear(State); }

            /// <summary>電源導入ステータスの設定取得(*PSC?)</summary>
            /// <returns>設定ステータス(true:1/false:0)</returns>
            public bool PSC() { return GetPowerOnStatusClear(); }

            /// <summary>装置の状態をステートファイルからリコールする(*RCL)</summary>
            public void RCL() { Recall(); }

            /// <summary>装置の状態をステートファイルからリコールする(*RCL arg)</summary>
            /// <param name="arg">ステートファイル指定(装置に応じて要設定)</param>
            public void RCL(string arg) { Recall(arg); }

            /// <summary>装置を工場出荷状態時の規定値に初期化(*RST)</summary>
            /// <exception cref="Exception"></exception>
            public void RST() { Reset(); }

            /// <summary>装置の状態をステートファイルに保存する(*SAV)</summary>
            public void SAV() { Save(); }

            /// <summary>装置の状態をステートファイルに保存する(*SAV arg)</summary>
            /// <param name="arg">ステートファイル指定(装置に応じて要設定)</param>
            public void SAV(string arg) { Save(arg); }

            /// <summary>ステータスバイトレジスタのイネーブルレジスタ設定(*SRE SRE.NR)</summary>
            /// <param name="SRE">設定値</param>
            public void SRE(IEEE488_Register SRE) { SetServiceRequestEnable(SRE); }

            /// <summary>ステータスバイトレジスタのイネーブルレジスタ設定状態取得(*ERE?)</summary>
            /// <returns>設定値</returns>
            public IEEE488_Register ERE() { return GetServiceRequestEnable(); }

            /// <summary>ステータスバイトレジスタの取得(*STB?)</summary>
            /// <returns>レジスタ値</returns>
            public IEEE488_Register STB() { return GetStatusByte(); }

            /// <summary>トリガ設定(*TRG)<br/>
            /// (装置に応じて動作は異なる)</summary>
            public void TRG() { Trigger(); }

            /// <summary>装置のセルフテスト(*TST?)</summary>
            /// <returns>テスト結果(装置に応じて要変換)</returns>
            public string TST() { return Encoding.ASCII.GetString(GetTest()); }

            /// <summary>保留中の全処理が完了するまで待つ(*WAI)</summary>
            public void WAI() { Wait(); }

            #endregion

        }

        /// <summary>IEEE488.2 IDN(識別情報)構造体</summary>
        public struct IEEE488_IDN
        {
            /// <summary>メーカ名</summary>
            public string Vender { get; private set; }
            /// <summary>モデル番号</summary>
            public string ModelNumber { get; private set; }
            /// <summary>シリアル番号</summary>
            public string SerialNumber { get; private set; }
            /// <summary>リビジョンコード</summary>
            public string RevisionCode { get; private set; }
            /// <summary>IDNコード</summary>
            public string IDN { get; private set; }

            /// <summary>コンストラクタ</summary>
            /// <param name="strIDN">IDNコード</param>
            public IEEE488_IDN(string strIDN)
            {
                IDN = strIDN;
                string[] strArrBf = strIDN.Split(',');
                if (strArrBf.Length == 4)
                {
                    Vender = strArrBf[0];
                    ModelNumber = strArrBf[1];
                    SerialNumber = strArrBf[2];
                    RevisionCode = strArrBf[3];
                }
                else
                {
                    Vender = "";
                    ModelNumber = "";
                    SerialNumber = "";
                    RevisionCode = "";
                }
            }

            /// <summary>コンストラクタ</summary>
            /// <param name="vender">メーカ名</param>
            /// <param name="model">モデル番号</param>
            /// <param name="sn">シリアル番号</param>
            /// <param name="rev">リビジョンコード</param>
            public IEEE488_IDN(string vender, string model, string sn, string rev) : this(vender + "," + model + "," + sn + "," + rev) { }


        }

        /// <summary>2Byteレジスタクラス</summary>
        public class IEEE488_Register
        {
            /// <summary>2byte値</summary>
            public UInt16 NR { get; set; }
            /// <summary>レジストリbool配列取得</summary>
            public BitArray Register { get { return new BitArray(BitConverter.GetBytes(NR)); } }

            /// <summary>レジストリ0</summary>
            public bool Register0 { get { return Register[0]; } set { editRegister(0, value); } }
            /// <summary>レジストリ1</summary>
            public bool Register1 { get { return Register[1]; } set { editRegister(1, value); } }
            /// <summary>レジストリ2</summary>
            public bool Register2 { get { return Register[2]; } set { editRegister(2, value); } }
            /// <summary>レジストリ3</summary>
            public bool Register3 { get { return Register[3]; } set { editRegister(3, value); } }
            /// <summary>レジストリ4</summary>
            public bool Register4 { get { return Register[4]; } set { editRegister(4, value); } }
            /// <summary>レジストリ5</summary>
            public bool Register5 { get { return Register[5]; } set { editRegister(5, value); } }
            /// <summary>レジストリ6</summary>
            public bool Register6 { get { return Register[6]; } set { editRegister(6, value); } }
            /// <summary>レジストリ7</summary>
            public bool Register7 { get { return Register[7]; } set { editRegister(7, value); } }
            /// <summary>レジストリ8</summary>
            public bool Register8 { get { return Register[8]; } set { editRegister(8, value); } }
            /// <summary>レジストリ9</summary>
            public bool Register9 { get { return Register[9]; } set { editRegister(9, value); } }
            /// <summary>レジストリ10</summary>
            public bool Register10 { get { return Register[10]; } set { editRegister(10, value); } }
            /// <summary>レジストリ11</summary>
            public bool Register11 { get { return Register[11]; } set { editRegister(11, value); } }
            /// <summary>レジストリ12</summary>
            public bool Register12 { get { return Register[12]; } set { editRegister(12, value); } }
            /// <summary>レジストリ13</summary>
            public bool Register13 { get { return Register[13]; } set { editRegister(13, value); } }
            /// <summary>レジストリ14</summary>
            public bool Register14 { get { return Register[14]; } set { editRegister(14, value); } }
            /// <summary>レジストリ15</summary>
            public bool Register15 { get { return Register[15]; } set { editRegister(15, value); } }

            /// <summary>コンストラクタ</summary>
            /// <param name="nr">NR値</param>
            public IEEE488_Register(UInt16 nr) { NR = nr; }

            private void editRegister(byte num, bool val)
            {
                BitArray bitArray = Register;
                bitArray[num] = val;
                NR = 0;
                for (int i = 0; i < bitArray.Length; i++)
                {
                    if (bitArray[i])
                    {
                        NR |= (ushort)(1 << (bitArray.Length - 1 - i));
                    }
                }
            }
        }

    }
}