using AxMSTSCLib;
using MetaFrm.RemoteDesktop.Core;
using MSTSCLib;
using System.ComponentModel;
using ColorDepth = MetaFrm.RemoteDesktop.Core.ColorDepth;

namespace MetaFrm.RemoteDesktop.Control
{
    /// <summary>
    /// RDP
    /// </summary>
    public class RDP : AxMsTscAxNotSafeForScripting
    {
        private static int _CreateID_Index = 0;
        private static int CreateID_Index
        {
            get
            {
                return ++_CreateID_Index;
            }
        }

        private readonly int createID = 0;
        /// <summary>
        /// CreateID
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CreateID => this.createID;

        /// <summary>
        /// FullScreenWhenConnectd
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool FullScreenWhenConnectd { get; set; } = false;

        /// <summary>
        /// FullScreenStatus
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool FullScreenStatus { get; set; } = false;

        /// <summary>
        /// RDP
        /// </summary>
        public RDP()
        {
            this.DoubleBuffered = true;
            this.Dock = DockStyle.Fill;
            this.Enabled = true;

            this.createID = CreateID_Index;

            this.Name = string.Format("axMsTscAxNotSafeForScripting{0}", this.createID);
        }

        /// <summary>
        /// Server
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Server
        {
            get
            {
                return base.Server;
            }
            set
            {
                string[] tmps;
                int port = 3389;

                tmps = value.Split(":".ToCharArray());

                base.Server = tmps[0];

                if (tmps.Length > 1)
                    port = int.Parse(tmps[1]);

                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings8)
                {
                    msRdpClientAdvancedSettings8.EnableCredSspSupport = true;
                    msRdpClientAdvancedSettings8.RDPPort = port;
                }
                else if (this.AdvancedSettings is IMsRdpClientAdvancedSettings msRdpClientAdvancedSettings)
                    msRdpClientAdvancedSettings.RDPPort = port;
            }
        }
        /// <summary>
        /// Port
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Port
        {
            get
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings8)
                {
                    return msRdpClientAdvancedSettings8.RDPPort;
                }
                else if (this.AdvancedSettings is IMsRdpClientAdvancedSettings msRdpClientAdvancedSettings)
                    return msRdpClientAdvancedSettings.RDPPort;

                return 3389;
            }
        }
        /// <summary>
        /// UserName
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string UserName
        {
            set
            {
                string[] tmps;

                tmps = value.Split("\\".ToCharArray());

                //도메인이 포함 되어 있으면
                if (tmps.Length > 1)
                {
                    this.Domain = tmps[0];
                    base.UserName = tmps[1];
                }
                else
                {
                    this.Domain = "";
                    base.UserName = value;
                }
            }
        }
        /// <summary>
        /// ClearTextPassword
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ClearTextPassword
        {
            set
            {
                if(this.GetOcx() is IMsTscNonScriptable msTscNonScriptable)
                    msTscNonScriptable.ClearTextPassword = value;
            }
        }
        /// <summary>
        /// ResetPassword
        /// </summary>
        private void ResetPassword()
        {
            if (this.GetOcx() is IMsTscNonScriptable msTscNonScriptable)
                msTscNonScriptable.ResetPassword();
        }

        /// <summary>
        /// 클립보드
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RedirectClipboard
        {
            get
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    return msRdpClientAdvancedSettings.RedirectClipboard;
                else
                    return ((IMsRdpClientAdvancedSettings5)this.AdvancedSettings).RedirectClipboard;
            }
            set
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    msRdpClientAdvancedSettings.RedirectClipboard = value;
                else
                    ((IMsRdpClientAdvancedSettings5)this.AdvancedSettings).RedirectClipboard = value;
            }
        }
        /// <summary>
        /// RedirectDevices
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RedirectDevices
        {
            get
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    return msRdpClientAdvancedSettings.RedirectDevices;
                else
                    return ((IMsRdpClientAdvancedSettings5)this.AdvancedSettings).RedirectDevices;
            }
            set
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    msRdpClientAdvancedSettings.RedirectDevices = value;
                else
                    ((IMsRdpClientAdvancedSettings5)this.AdvancedSettings).RedirectDevices = value;
            }
        }
        /// <summary>
        /// 디스크
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RedirectDrives
        {
            get
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    return msRdpClientAdvancedSettings.RedirectDrives;
                else
                    return ((IMsRdpClientAdvancedSettings)this.AdvancedSettings).RedirectDrives;
            }
            set
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    msRdpClientAdvancedSettings.RedirectDrives = value;
                else
                    ((IMsRdpClientAdvancedSettings)this.AdvancedSettings).RedirectDrives = value;
            }
        }
        /// <summary>
        /// RedirectPorts
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RedirectPorts
        {
            get
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    return msRdpClientAdvancedSettings.RedirectPorts;
                else
                    return ((IMsRdpClientAdvancedSettings)this.AdvancedSettings).RedirectPorts;
            }
            set
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    msRdpClientAdvancedSettings.RedirectPorts = value;
                else
                    ((IMsRdpClientAdvancedSettings)this.AdvancedSettings).RedirectPorts = value;
            }
        }
        /// <summary>
        /// Windows Server 2008은 Microsoft POS for .NET 1.1을 사용하는 Windows Embedded for Point of Service 장치의 리디렉션도 지원합니다. Windows Embedded for Point of Service 장치에는 다기능 POS 워크스테이션, 네트워크 부팅 가능한 “씬 클라이언트” POS 터미널, 고객 관리 정보 키오스크, 자체 계산 시스템 등이 포함됩니다. 그러나 Windows Embedded for POS 장치 리디렉션은 터미널 서버가 Windows Server 2008 32비트 버전을 실행하는 경우에만 지원됩니다.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RedirectPOSDevices
        {
            get
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    return msRdpClientAdvancedSettings.RedirectPOSDevices;
                else
                    return ((IMsRdpClientAdvancedSettings5)this.AdvancedSettings).RedirectPOSDevices;
            }
            set
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    msRdpClientAdvancedSettings.RedirectPOSDevices = value;
                else
                    ((IMsRdpClientAdvancedSettings5)this.AdvancedSettings).RedirectPOSDevices = value;
            }
        }
        /// <summary>
        /// 프린트
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RedirectPrinters
        {
            get
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    return msRdpClientAdvancedSettings.RedirectPrinters;
                else
                    return ((IMsRdpClientAdvancedSettings)this.AdvancedSettings).RedirectPrinters;
            }
            set
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    msRdpClientAdvancedSettings.RedirectPrinters = value;
                else
                    ((IMsRdpClientAdvancedSettings)this.AdvancedSettings).RedirectPrinters = value;
            }
        }
        /// <summary>
        /// RedirectSmartCards
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RedirectSmartCards
        {
            get
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    return msRdpClientAdvancedSettings.RedirectSmartCards;
                else
                    return ((IMsRdpClientAdvancedSettings)this.AdvancedSettings).RedirectSmartCards;
            }
            set
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    msRdpClientAdvancedSettings.RedirectSmartCards = value;
                else
                    ((IMsRdpClientAdvancedSettings)this.AdvancedSettings).RedirectSmartCards = value;
            }
        }

        /// <summary>
        /// GoFullScreen
        /// </summary>
        public void GoFullScreen()
        {
            this.SecuredSettings.FullScreen = 1;
        }
        /// <summary>
        /// LeaveFullScreen
        /// </summary>
        public void LeaveFullScreen()
        {
            this.SecuredSettings.FullScreen = 0;
        }


        /// <summary>
        /// 현재 컨트롤의 크기에 화면이 출력됨(스크롤이 생기지 않음, 창의 크기가 변경되면 출력화면이 자동으로 스케일됨
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SmartSizing
        {
            get
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    return msRdpClientAdvancedSettings.SmartSizing;
                else
                    return ((IMsRdpClientAdvancedSettings)this.AdvancedSettings).SmartSizing;
            }
            set
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    msRdpClientAdvancedSettings.SmartSizing = value;
                else
                    ((IMsRdpClientAdvancedSettings)this.AdvancedSettings).SmartSizing = value;
            }
        }
        /// <summary>
        /// 컨테이너를 포함한 전체화면
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ContainerHandledFullScreen
        {
            get
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    return msRdpClientAdvancedSettings.ContainerHandledFullScreen == 1;
                else
                    return ((IMsRdpClientAdvancedSettings)this.AdvancedSettings).ContainerHandledFullScreen == 1;
            }
            set
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    msRdpClientAdvancedSettings.ContainerHandledFullScreen = value ? 1 : 0;
                else
                    ((IMsRdpClientAdvancedSettings)this.AdvancedSettings).ContainerHandledFullScreen = value ? 1 : 0;
            }
        }
        /// <summary>
        /// ColorDepth
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ColorDepth ColorDepth
        {
            get
            {
                if (this.GetOcx() is IMsRdpClient msRdpClient)
                    return (ColorDepth)msRdpClient.ColorDepth;

                return ColorDepth.HighColor_16bit;
            }
            set
            {
                if (this.GetOcx() is IMsRdpClient msRdpClient)
                    msRdpClient.ColorDepth = (int)value;
            }
        }
        /// <summary>
        /// UseMultimon
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UseMultimon
        {
            get
            {
                if (this.GetOcx() is IMsRdpClientNonScriptable7 msRdpClient)
                    return msRdpClient.UseMultimon;
                else
                    return (this.GetOcx() as IMsRdpClientNonScriptable5)?.UseMultimon ?? false;
            }
            set
            {
                if (this.GetOcx() is IMsRdpClientNonScriptable7 msRdpClient)
                    msRdpClient.UseMultimon = value;
                else if (this.GetOcx() is IMsRdpClientNonScriptable5 msRdpClient1)
                    msRdpClient1.UseMultimon = value;

                if (!this.IsConnected)
                    this.ContainerHandledFullScreen = value;
            }
        }

        /// <summary>
        /// DisplayConnectionBar
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DisplayConnectionBar
        {
            get
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    return msRdpClientAdvancedSettings.DisplayConnectionBar;
                else
                    return ((IMsRdpClientAdvancedSettings)this.AdvancedSettings).DisplayConnectionBar;
            }
            set
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    msRdpClientAdvancedSettings.DisplayConnectionBar = value;
                else
                    ((IMsRdpClientAdvancedSettings)this.AdvancedSettings).DisplayConnectionBar = value;
            }
        }
        /// <summary>
        /// ConnectionBarShowPinButton
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ConnectionBarShowPinButton
        {
            get
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    return msRdpClientAdvancedSettings.ConnectionBarShowPinButton;
                else
                    return ((IMsRdpClientAdvancedSettings5)this.AdvancedSettings).ConnectionBarShowPinButton;
            }
            set
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    msRdpClientAdvancedSettings.ConnectionBarShowPinButton = value;
                else
                    ((IMsRdpClientAdvancedSettings5)this.AdvancedSettings).ConnectionBarShowPinButton = value;
            }
        }
        /// <summary>
        /// ConnectionBarShowMinimizeButton
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ConnectionBarShowMinimizeButton
        {
            get
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    return msRdpClientAdvancedSettings.ConnectionBarShowMinimizeButton;
                else
                    return ((IMsRdpClientAdvancedSettings3)this.AdvancedSettings).ConnectionBarShowMinimizeButton;
            }
            set
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    msRdpClientAdvancedSettings.ConnectionBarShowMinimizeButton = value;
                else
                    ((IMsRdpClientAdvancedSettings3)this.AdvancedSettings).ConnectionBarShowMinimizeButton = value;
            }
        }
        /// <summary>
        /// ConnectionBarShowRestoreButton
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ConnectionBarShowRestoreButton
        {
            get
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    return msRdpClientAdvancedSettings.ConnectionBarShowRestoreButton;
                else
                    return ((IMsRdpClientAdvancedSettings3)this.AdvancedSettings).ConnectionBarShowRestoreButton;
            }
            set
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    msRdpClientAdvancedSettings.ConnectionBarShowRestoreButton = value;
                else
                    ((IMsRdpClientAdvancedSettings3)this.AdvancedSettings).ConnectionBarShowRestoreButton = value;
            }
        }

        /// <summary>
        /// ConnectionBarText
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ConnectionBarText
        {
            get
            {
                if (this.GetOcx() is IMsRdpClientNonScriptable7 msRdpClientNonScriptable)
                    return msRdpClientNonScriptable.ConnectionBarText;
                else
                    return (this.GetOcx() as IMsRdpClientNonScriptable3)?.ConnectionBarText ?? $"{this.Server}";
            }
            set
            {
                if (this.GetOcx() is IMsRdpClientNonScriptable7 msRdpClientNonScriptable7)
                    msRdpClientNonScriptable7.ConnectionBarText = value;
                else if (this.GetOcx() is IMsRdpClientNonScriptable3 msRdpClientNonScriptable)
                    msRdpClientNonScriptable.ConnectionBarText = value;
            }
        }

        /// <summary>
        /// KeyboardHookMode
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool KeyboardHookMode
        {
            get
            {
                if (this.GetOcx() is IMsRdpClient10 msRdpClient)
                    return msRdpClient.SecuredSettings3.KeyboardHookMode == 2;
                else
                    return false;
            }
            set
            {
                if (this.GetOcx() is IMsRdpClient10 msRdpClient)
                    msRdpClient.SecuredSettings3.KeyboardHookMode = value ? 2 : 0;
            }
        }

        /// <summary>
        /// ConnectToAdministerServer 
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ConnectToAdministerServer
        {
            get
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    return msRdpClientAdvancedSettings.ConnectToAdministerServer;
                else
                    return ((IMsRdpClientAdvancedSettings6)this.AdvancedSettings).ConnectToAdministerServer;
            }
            set
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    msRdpClientAdvancedSettings.ConnectToAdministerServer = value;
                else
                    ((IMsRdpClientAdvancedSettings6)this.AdvancedSettings).ConnectToAdministerServer = value;
            }
        }
        /// <summary>
        /// HotKeyCtrlAltDel 
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HotKeyCtrlAltDel
        {
            get
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    return msRdpClientAdvancedSettings.HotKeyCtrlAltDel == 1;
                else
                    return ((IMsRdpClientAdvancedSettings)this.AdvancedSettings).HotKeyCtrlAltDel == 1;
            }
            set
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    msRdpClientAdvancedSettings.HotKeyCtrlAltDel = (int)(value ? VirtualKey.VK_END : VirtualKey.VK_INSERT);
                else
                    ((IMsRdpClientAdvancedSettings)this.AdvancedSettings).HotKeyCtrlAltDel = value ? 1 : 0;
            }
        }
        /// <summary>
        /// HotKeyFullScreen 
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HotKeyFullScreen
        {
            get
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    return msRdpClientAdvancedSettings.HotKeyFullScreen == 1;
                else
                    return ((IMsRdpClientAdvancedSettings)this.AdvancedSettings).HotKeyFullScreen == 1;
            }
            set
            {
                if (value)
                {
                    if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                        msRdpClientAdvancedSettings.HotKeyFullScreen = (int)VirtualKey.VK_RETURN;
                    else
                        ((IMsRdpClientAdvancedSettings)this.AdvancedSettings).HotKeyFullScreen = (int)VirtualKey.VK_RETURN;
                }
            }
        }
        /// <summary>
        /// EnableSuperPan 
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool EnableSuperPan
        {
            get
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    return msRdpClientAdvancedSettings.EnableSuperPan;
                else
                    return ((IMsRdpClientAdvancedSettings7)this.AdvancedSettings).EnableSuperPan;
            }
            set
            {
                if (this.AdvancedSettings is IMsRdpClientAdvancedSettings8 msRdpClientAdvancedSettings)
                    msRdpClientAdvancedSettings.EnableSuperPan = value;
                else
                    ((IMsRdpClientAdvancedSettings7)this.AdvancedSettings).EnableSuperPan = value;
            }
        }

        /// <summary>
        /// IsConnected
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsConnected
        {
            get
            {
                return this.Connected == 1;
            }
        }
    }
}