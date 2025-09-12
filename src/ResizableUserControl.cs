using MetaFrm.Control;
using MetaFrm.RemoteDesktop.Core;
using MetaFrm.RemoteDesktop.Core.Models;
using System.ComponentModel;

namespace MetaFrm.RemoteDesktop.Control
{
    /// <summary>
    /// ResizableUserControl
    /// </summary>
    public partial class ResizableUserControl : UserControl, IAction
    {
        private readonly SharedStatus SharedStatus;

        /// <summary>
        /// Action
        /// </summary>
        public event MetaFrmEventHandler? Action;

        /// <summary>
        /// Title
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string Title { get; set; }

        private bool IsResizing = false;
        private bool IsMoving = false;
        private Point LastMousePos;
        private ResizeDirection ResizeDirection;

        private readonly int GripSize = 5;

        /// <summary>
        /// OrgLocation
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point OrgLocation { get; set; }
        /// <summary>
        /// OrgSize
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Size OrgSize { get; set; }

        private Panel ContainerPanel { get; set; }
        private Form Form { get; set; }
        private System.Windows.Forms.Control TopControl { get; set; }
        private System.Windows.Forms.Control BottomControl { get; set; }

        /// <summary>
        /// Server
        /// </summary>
        public ServerExtend Server { get; }
        private readonly RDP RDP = new() { Dock = DockStyle.None, Padding = new(0, 0, 0, 0), Margin = new(0, 0, 0, 0) };
        private readonly Point RDPInitLocation = new(0, 0);

        /// <summary>
        /// CreateID
        /// </summary>
        public int CreateID => this.RDP.CreateID;
        /// <summary>
        /// RDP_DesktopWidth
        /// </summary>
        public int RDP_DesktopWidth => this.RDP.DesktopWidth;
        /// <summary>
        /// RDP_DesktopHeight
        /// </summary>
        public int RDP_DesktopHeight => this.RDP.DesktopHeight;
        /// <summary>
        /// FullScreenStatus
        /// </summary>
        public bool RDP_FullScreenStatus => this.RDP != null && this.RDP.FullScreenStatus;

        private readonly Button[] ButtonBefore;

        private ContextMenuStrip Menu { get; set; }

        private readonly ToolStripMenuItem MenuItemGoFullScreen;
        private readonly ToolStripMenuItem MenuItemGoConnect;
        private readonly ToolStripMenuItem MenuItemGoDisconnect;
        private readonly ToolStripMenuItem MenuItemGoClose;
        private readonly ToolStripMenuItem MenuItemTabModeChange;

        private readonly ToolStripMenuItem MenuItemFullScreen;
        private readonly ToolStripMenuItem MenuItemScreen;
        private readonly ToolStripMenuItem MenuItemMultimon;
        private readonly ToolStripMenuItem MenuItemSmartSizing;
        private readonly ToolStripMenuItem MenuItemSetupDesktopSizeToDesktopSize;
        private readonly ToolStripMenuItem MenuItemSetupViewSizeToDesktopSize;
        private readonly ToolStripMenuItem MenuItemViewSizeToDesktopSize;
        private readonly ToolStripMenuItem MenuItemDesktopSizeToViewSize;
        private readonly ToolStripMenuItem MenuItemSetupViewSizeToViewSize;
        
        private readonly ToolStripMenuItem ToolStripMenuItemMovingLock;
        private readonly ToolStripMenuItem ToolStripMenuItemResizeLock;
        private readonly ToolStripMenuItem ToolStripMenuItemVisibleTitle;
        private readonly ToolStripMenuItem ToolStripMenuItemVisible_SERVER;
        private readonly ToolStripMenuItem ToolStripMenuItemVisible_USER_NAME;

        private readonly Size[] ViewSize =
            [new(320, 240)
            , new(384, 288)
            , new(640, 480)
            , new(768, 576)
            , new(800, 600)
            , new(1024, 768)
            , new(1280, 1024)
            , new(1440, 1050)
            , new(1920, 1080)
            ];
        private readonly ToolStripMenuItem MenuItemViewSize;

        private readonly Size[] DesktopSize =
            [new(640, 480)
            , new(800, 600)
            , new(1024, 768)
            , new(1280, 720)
            , new(1280, 768)
            , new(1280, 800)
            , new(1280, 1024)
            , new(1366, 768)
            , new(1440, 900)
            , new(1440, 1050)
            , new(1600, 1200)
            , new(1680, 1050)
            , new(1920, 1080)
            , new(1920, 1200)
            , new(2048, 1536)
            , new(2560, 1440)
            , new(2560, 1600)
            , new(2880, 1800)
            , new(3840, 2160)
            ];
        private readonly ToolStripMenuItem MenuItemDesktopSize;

        /// <summary>
        /// ResizableUserControl
        /// </summary>
        /// <param name="server"></param>
        /// <param name="sharedStatus"></param>
        /// <param name="buttonBefore"></param>
        /// <param name="form"></param>
        /// <param name="containerPanel"></param>
        /// <param name="topControl"></param>
        /// <param name="bottomControl"></param>
        public ResizableUserControl(ServerExtend server, SharedStatus sharedStatus, Button[] buttonBefore, Form form, Panel containerPanel, Panel topControl, Panel bottomControl)
        {
            this.Server = server;
            this.SharedStatus = sharedStatus;
            this.ButtonBefore = buttonBefore;
            this.Form = form;
            this.ContainerPanel = containerPanel;
            this.TopControl = topControl;
            this.BottomControl = bottomControl;

            this.DoubleBuffered = true;
            this.BackColor = Color.WhiteSmoke;
            this.MinimumSize = new Size(50, 50);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.BorderStyle = BorderStyle.FixedSingle;

            this.Padding = new Padding(0, 0, 0, (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0));

            this.Controls.Add(this.RDP);

            this.RDP.BringToFront();
            this.RDP.Location = this.RDPInitLocation;

            this.Title = $"{this.RDP.CreateID}:{server.SERVER_NAME}";


            this.Menu = new();

            this.MenuItemGoFullScreen = new("전체화면 전환");
            this.MenuItemGoConnect = new("연결");
            this.MenuItemGoDisconnect = new("연결 종료");
            this.MenuItemGoClose = new("닫기");
            this.MenuItemTabModeChange = new("탭모드 <-> 창모드");

            this.MenuItemFullScreen = new("전체화면");
            this.MenuItemScreen = new("전체화면 모니터");
            this.MenuItemMultimon = new("다중모니터");
            this.MenuItemScreen.DropDownItems.Add(this.MenuItemMultimon);

            this.MenuItemSmartSizing = new("스마트사이징");
            this.MenuItemSetupDesktopSizeToDesktopSize = new("설정해상도->원격해상도");
            this.MenuItemSetupViewSizeToDesktopSize = new("설정뷰크기->원격해상도");
            this.MenuItemViewSizeToDesktopSize = new("뷰크기->원격해상도");
            this.MenuItemDesktopSizeToViewSize = new("원격해상도->뷰크기");
            this.MenuItemSetupViewSizeToViewSize = new("설정뷰크기->뷰크기");

            this.MenuItemViewSize = new("뷰 크기");
            if (this.ContainerPanel.Width >= 100 && this.ContainerPanel.Height >= 100)
            {
                // 2 X 2
                Size size = new(50, 50);
                this.MenuItemViewSize.DropDownItems.Add(new ToolStripMenuItem($"{size.Width}x{size.Height} (2x2)", null
                    , (s, e) =>
                    {
                        if (s is ToolStripMenuItem menuItem && menuItem.Tag != null && menuItem.Tag is Size size)
                        {
                            var controls = this.ContainerPanel.Controls.OfType<ResizableUserControl>().ToList();

                            foreach (var item in controls)
                            {
                                item.Width = size.Width;
                                item.Height = size.Height + (this.SharedStatus.IsTab ? 0 : (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0));
                            }
                            this.Action?.Invoke(this, new() { Action = "LayoutControls", Value = controls.Count > 4 ? this.SharedStatus.ContainerPanelRightMargin : 0 });
                            this.Form.Refresh();
                        }
                    })
                { Tag = size });

                // 3 X 3
                this.MenuItemViewSize.DropDownItems.Add(new ToolStripMenuItem($"{size.Width}x{size.Height} (3x3)", null
                    , (s, e) =>
                    {
                        if (s is ToolStripMenuItem menuItem && menuItem.Tag != null && menuItem.Tag is Size size)
                        {
                            var controls = this.ContainerPanel.Controls.OfType<ResizableUserControl>().ToList();

                            foreach (var item in controls)
                            {
                                item.Width = size.Width;
                                item.Height = size.Height + (this.SharedStatus.IsTab ? 0 : (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0));
                            }
                            this.Action?.Invoke(this, new() { Action = "LayoutControls", Value = controls.Count > 9 ? this.SharedStatus.ContainerPanelRightMargin : 0 });
                            this.Form.Refresh();
                        }
                    })
                { Tag = size });

                // 4 X 4
                this.MenuItemViewSize.DropDownItems.Add(new ToolStripMenuItem($"{size.Width}x{size.Height} (4X4)", null
                    , (s, e) =>
                    {
                        if (s is ToolStripMenuItem menuItem && menuItem.Tag != null && menuItem.Tag is Size size)
                        {
                            var controls = this.ContainerPanel.Controls.OfType<ResizableUserControl>().ToList();

                            foreach (var item in controls)
                            {
                                item.Width = size.Width;
                                item.Height = size.Height + (this.SharedStatus.IsTab ? 0 : (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0));
                            }
                            this.Action?.Invoke(this, new() { Action = "LayoutControls", Value = controls.Count > 16 ? this.SharedStatus.ContainerPanelRightMargin : 0 });
                            this.Form.Refresh();
                        }
                    })
                { Tag = size });

                this.MenuItemViewSize.DropDownItems.Add(new ToolStripSeparator());
            }
            foreach (var item in this.ViewSize)
                this.MenuItemViewSize.DropDownItems.Add(new ToolStripMenuItem($"{item.Width}x{item.Height}", null
                    , (s, e) =>
                    {
                        if (s is ToolStripMenuItem menuItem && menuItem.Tag != null && menuItem.Tag is Size size)
                        {
                            this.Width = size.Width;
                            this.Height = size.Height + (this.SharedStatus.IsTab ? 0 : (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0));
                            this.Refresh();
                        }
                    })
                { Tag = item });


            this.MenuItemDesktopSize = new("원격해상도 크기");
            if (this.ContainerPanel.Width >= 100 && this.ContainerPanel.Height >= 100)
            {
                // 2 X 2
                Size size = new(50, 50);
                this.MenuItemDesktopSize.DropDownItems.Add(new ToolStripMenuItem($"{size.Width}x{size.Height} (2x2)", null
                    , (s, e) =>
                    {
                        if (s is ToolStripMenuItem menuItem && menuItem.Tag != null && menuItem.Tag is Size size)
                        {
                            var controls = this.ContainerPanel.Controls.OfType<ResizableUserControl>().ToList();

                            foreach (var item in controls)
                            {
                                if (!item.RDP.IsConnected)
                                {
                                    item.RDP.DesktopWidth = size.Width;
                                    item.RDP.DesktopHeight = size.Height;
                                }
                            }
                            this.Form.Refresh();
                        }
                    })
                { Tag = size });

                // 3 X 3
                this.MenuItemDesktopSize.DropDownItems.Add(new ToolStripMenuItem($"{size.Width}x{size.Height} (3x3)", null
                    , (s, e) =>
                    {
                        if (s is ToolStripMenuItem menuItem && menuItem.Tag != null && menuItem.Tag is Size size)
                        {
                            var controls = this.ContainerPanel.Controls.OfType<ResizableUserControl>().ToList();

                            foreach (var item in controls)
                            {
                                if (!item.RDP.IsConnected)
                                {
                                    item.RDP.DesktopWidth = size.Width;
                                    item.RDP.DesktopHeight = size.Height;
                                }
                            }
                            this.Form.Refresh();
                        }
                    })
                { Tag = size });

                // 4 X 4
                this.MenuItemDesktopSize.DropDownItems.Add(new ToolStripMenuItem($"{size.Width}x{size.Height} (4X4)", null
                    , (s, e) =>
                    {
                        if (s is ToolStripMenuItem menuItem && menuItem.Tag != null && menuItem.Tag is Size size)
                        {
                            var controls = this.ContainerPanel.Controls.OfType<ResizableUserControl>().ToList();

                            foreach (var item in controls)
                            {
                                if (!item.RDP.IsConnected)
                                {
                                    item.RDP.DesktopWidth = size.Width;
                                    item.RDP.DesktopHeight = size.Height;
                                }
                            }
                            this.Form.Refresh();
                        }
                    })
                { Tag = size });

                this.MenuItemDesktopSize.DropDownItems.Add(new ToolStripSeparator());
            }
            foreach (var item in this.DesktopSize)
                this.MenuItemDesktopSize.DropDownItems.Add(new ToolStripMenuItem($"{item.Width}x{item.Height}", null
                    , (s, e) => {
                        if (s is ToolStripMenuItem menuItem && menuItem.Tag != null && menuItem.Tag is Size size)
                        {
                            if (!this.RDP.IsConnected)
                            {
                                this.RDP.DesktopWidth = size.Width;
                                this.RDP.DesktopHeight = size.Height;
                                this.Refresh();
                            }
                        }
                    }
                    ) { Tag = item });

            this.ToolStripMenuItemMovingLock = new("이동 잠금");
            this.ToolStripMenuItemResizeLock = new("크기 잠금");

            this.ToolStripMenuItemVisibleTitle = new("타이틀 보이기") { Checked = true };
            this.ToolStripMenuItemVisible_SERVER = new("서버 정보 보이기") { Checked = true };
            this.ToolStripMenuItemVisible_USER_NAME = new("사용자 정보 보이기") { Checked = true };

            this.Menu.Items.Add(this.MenuItemGoFullScreen);
            this.Menu.Items.Add(this.MenuItemGoConnect);
            this.Menu.Items.Add(this.MenuItemGoDisconnect);
            this.Menu.Items.Add(this.MenuItemGoClose);
            this.Menu.Items.Add(this.MenuItemTabModeChange);
            this.Menu.Items.Add(new ToolStripSeparator());
            this.Menu.Items.Add(this.MenuItemFullScreen);
            this.Menu.Items.Add(this.MenuItemScreen);
            this.Menu.Items.Add(this.MenuItemSmartSizing);
            this.Menu.Items.Add(new ToolStripSeparator());
            this.Menu.Items.Add(this.MenuItemSetupDesktopSizeToDesktopSize);
            this.Menu.Items.Add(this.MenuItemSetupViewSizeToDesktopSize);
            this.Menu.Items.Add(this.MenuItemViewSizeToDesktopSize);
            this.Menu.Items.Add(this.MenuItemDesktopSize);
            this.Menu.Items.Add(new ToolStripSeparator());
            this.Menu.Items.Add(this.MenuItemDesktopSizeToViewSize);
            this.Menu.Items.Add(this.MenuItemSetupViewSizeToViewSize);
            this.Menu.Items.Add(this.MenuItemViewSize);

            this.Menu.Items.Add(new ToolStripSeparator());
            this.Menu.Items.Add(this.ToolStripMenuItemMovingLock);
            this.Menu.Items.Add(this.ToolStripMenuItemResizeLock);
            this.Menu.Items.Add(new ToolStripSeparator());
            this.Menu.Items.Add(this.ToolStripMenuItemVisibleTitle);
            this.Menu.Items.Add(this.ToolStripMenuItemVisible_SERVER);
            this.Menu.Items.Add(this.ToolStripMenuItemVisible_USER_NAME);

            this.DockChanged += ResizableUserControl_DockChanged;

            this.RDP.OnRequestGoFullScreen += OnEnterFullScreenMode;//this.ContainerHandledFullScreen = true 일때 호출 됨
            this.RDP.OnEnterFullScreenMode += OnEnterFullScreenMode;
            this.RDP.OnRequestLeaveFullScreen += OnLeaveFullScreenMode;//this.ContainerHandledFullScreen = true 일때 호출 됨
            this.RDP.OnLeaveFullScreenMode += OnLeaveFullScreenMode;

            this.RDP.OnFatalError += (s, e) => { };
            this.RDP.OnLogonError += (s, e) => { };
            this.RDP.OnWarning += (s, e) => { };

            this.RDP.OnConnected += OnConnected;
            this.RDP.OnDisconnected += OnDisconnected;

            this.RDP.GotFocus += RDP_GotFocus;

            this.InitMenuEvent();
        }
        private void InitMenuEvent()
        {
            this.MenuItemGoFullScreen.Click += this.MenuItemGoFullScreen_Click;//전체화면 전환
            this.MenuItemGoConnect.Click += (s, e) => this.Connect();//연결
            this.MenuItemGoDisconnect.Click += (s, e) =>
            {
                DialogResult dialogResult = MessageBox.Show(this.Form, $"'{this.Server.SERVER_NAME}'을 연결 종료 하시겠습니까?", "연결 종료", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.No) return;

                this.Disconnect();
            };//연결 종료
            this.MenuItemGoClose.Click += this.MenuItemGoClose_Click;//닫기
            this.MenuItemTabModeChange.Click += (s, e) => this.Action?.Invoke(this, new() { Action = "TabModeChange" });//탭모드 <-> 창모드

            this.MenuItemFullScreen.Click += this.MenuItemFullScreen_Click;//전체화면
            this.MenuItemMultimon.Click += this.MenuItemMultimon_Click;//다중모니터
            this.MenuItemSmartSizing.Click += this.MenuItemSmartSizing_Click;//스마트사이징
            this.MenuItemSetupDesktopSizeToDesktopSize.Click += this.MenuItemSetupDesktopSizeToDesktopSize_Click;//설정해상도->원격해상도
            this.MenuItemSetupViewSizeToDesktopSize.Click += this.MenuItemSetupViewSizeToDesktopSize_Click;//설정뷰크기->원격해상도
            this.MenuItemViewSizeToDesktopSize.Click += this.MenuItemViewSizeToDesktopSize_Click;//뷰크기->원격해상도
            this.MenuItemDesktopSizeToViewSize.Click += this.MenuItemDesktopSizeToViewSize_Click;//원격해상도->뷰크기
            this.MenuItemSetupViewSizeToViewSize.Click += this.MenuItemSetupViewSizeToViewSize_Click;//설정뷰크기->뷰크기

            this.MenuItemFullScreen.Checked = this.Server.IsFullScreen;
            this.MenuItemMultimon.Checked = this.Server.IsUseMultimon;
            this.MenuItemSmartSizing.Checked = this.Server.IsSmartSizing;

            this.ToolStripMenuItemMovingLock.Click += (s, e) =>
            {
                this.SharedStatus.IsMovingLock = !this.SharedStatus.IsMovingLock;
                if (s != null && s is ToolStripMenuItem tool)
                    tool.Checked = this.SharedStatus.IsMovingLock;
            };

            this.ToolStripMenuItemResizeLock.Click += (s, e) =>
            {
                this.SharedStatus.IsResizeLock = !this.SharedStatus.IsResizeLock;
                if (s != null && s is ToolStripMenuItem tool)
                    tool.Checked = this.SharedStatus.IsResizeLock;
            };

            this.ToolStripMenuItemVisibleTitle.Click += (s, e) =>
            {
                this.SharedStatus.IsTitleVisible = !this.SharedStatus.IsTitleVisible;

                if (s != null && s is ToolStripMenuItem tool)
                {
                    tool.Checked = this.SharedStatus.IsTitleVisible;

                    foreach (var ctrl in this.ContainerPanel.Controls.OfType<ResizableUserControl>().OrderBy(x => x.CreateID))
                        ctrl.SetTitleHeight();

                    this.Form.Refresh();
                }
            };
            this.ToolStripMenuItemVisible_SERVER.Click += (s, e) =>
            {
                this.SharedStatus.Visible_SERVER = !this.SharedStatus.Visible_SERVER;
                if (s != null && s is ToolStripMenuItem tool)
                {
                    tool.Checked = this.SharedStatus.Visible_SERVER;
                    this.Form.Refresh();
                }
            };
            this.ToolStripMenuItemVisible_USER_NAME.Click += (s, e) =>
            {
                this.SharedStatus.Visible_USER_NAME = !this.SharedStatus.Visible_USER_NAME;
                if (s != null && s is ToolStripMenuItem tool)
                {
                    tool.Checked = this.SharedStatus.Visible_USER_NAME;
                    this.Form.Refresh();
                }
            };
        }

        private bool IsGoFullScreenSaveFormWindowStateLocation = false;
        /// <summary>
        /// 전체화면 전환
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemGoFullScreen_Click(object? sender, EventArgs e)
        {
            if (!this.Server.IsUseMultimon)
                this.SaveGoFullScreenFormWindowStateLocation();

            this.RDP.GoFullScreen();
        }
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemGoClose_Click(object? sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(this.Form, $"'{this.Server.SERVER_NAME}'을 닫으시겠습니까?", "닫기", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.No) return;

            this.Disconnect();
            this.RDP.Dispose();

            this.Action?.Invoke(this, new() { Action = "Close" });
        }
        /// <summary>
        /// 전체화면
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemFullScreen_Click(object? sender, EventArgs e)
        {
            this.Server.IsFullScreen = !this.Server.IsFullScreen;

            this.MenuItemFullScreen.Checked = this.Server.IsFullScreen;

            if (!this.Server.IsFullScreen)
            {
                this.Server.IsUseMultimon = this.Server.IsFullScreen;
                this.MenuItemMultimon.Checked = this.Server.IsFullScreen;
            }

            this.Refresh();
        }
        /// <summary>
        /// 다중모니터
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemMultimon_Click(object? sender, EventArgs e)
        {
            this.Server.IsUseMultimon = !this.Server.IsUseMultimon;
            this.RDP.UseMultimon = this.Server.IsUseMultimon;

            if (sender is ToolStripMenuItem item)
                item.Checked = this.Server.IsUseMultimon;

            if (this.Server.IsUseMultimon)
            {
                this.Server.IsFullScreen = this.Server.IsUseMultimon;
                this.MenuItemFullScreen.Checked = this.Server.IsUseMultimon;
            }

            this.Server.ScreenBounds = null;

            this.Form.Refresh();
        }
        /// <summary>
        /// 스마트사이징
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemSmartSizing_Click(object? sender, EventArgs e)
        {
            this.RDP.SmartSizing = !this.RDP.SmartSizing;
            this.Server.IsSmartSizing = this.RDP.SmartSizing;

            this.MenuItemSmartSizing.Checked = this.Server.IsSmartSizing;

            this.Refresh();
        }
        /// <summary>
        /// 설정해상도->원격해상도
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemSetupDesktopSizeToDesktopSize_Click(object? sender, EventArgs e)
        {
            this.RDP.DesktopWidth = this.Server.DESKTOP_WIDTH;
            this.RDP.DesktopHeight = this.Server.DESKTOP_HEIGHT;

            this.Refresh();
        }
        /// <summary>
        /// 설정뷰크기->원격해상도
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemSetupViewSizeToDesktopSize_Click(object? sender, EventArgs e)
        {
            this.RDP.DesktopWidth = this.Server.CONTROL_WIDTH;
            this.RDP.DesktopHeight = this.Server.CONTROL_HEIGHT;

            this.Refresh();
        }
        /// <summary>
        /// 뷰크기->원격해상도
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemViewSizeToDesktopSize_Click(object? sender, EventArgs e)
        {
            this.RDP.DesktopWidth = this.Width;
            this.RDP.DesktopHeight = this.Height - (this.SharedStatus.IsTab ? 0 : (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0));

            this.Refresh();
        }
        /// <summary>
        /// 원격해상도->뷰크기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemDesktopSizeToViewSize_Click(object? sender, EventArgs e)
        {
            this.Width = this.RDP.DesktopWidth;
            this.Height = this.RDP.DesktopHeight + (this.SharedStatus.IsTab ? 0 : (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0));

            this.Refresh();
        }
        /// <summary>
        /// 설정뷰크기->뷰크기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemSetupViewSizeToViewSize_Click(object? sender, EventArgs e)
        {
            this.Width = this.Server.CONTROL_WIDTH;
            this.Height = this.Server.CONTROL_HEIGHT + (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0);

            this.Refresh();
        }

        private void RDP_GotFocus(object? sender, EventArgs e)
        {
            if (this.Tag != null && this.Tag is Button button && (this.ButtonBefore[0] == null || !this.ButtonBefore[0].Equals(button)))
            {
                this.ButtonBefore[0] = button;

                this.SetActiveControl(this);
            }
        }

        private void ResizableUserControl_DockChanged(object? sender, EventArgs e)
        {
            if (this.Dock == DockStyle.Fill)
            {
                this.BorderStyle = BorderStyle.None; // ← 테두리 표시
                this.Padding = new Padding(0, 0, 0, 0);
                this.SetStyle(ControlStyles.ResizeRedraw, false);
            }
            else
            {
                if (this.RDP.IsConnected)
                    this.BorderStyle = BorderStyle.None; // ← 테두리 표시
                else
                    this.BorderStyle = BorderStyle.FixedSingle; // ← 테두리 표시

                this.Padding = new Padding(0, 0, 0, (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0));
                this.SetStyle(ControlStyles.ResizeRedraw, true);
            }
        }

        /// <summary>
        /// SetTitleHeight
        /// </summary>
        public void SetTitleHeight()
        {
            this.Padding = new Padding(0, 0, 0, this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0);
        }

        /// <summary>
        /// SetActiveControl
        /// </summary>
        /// <param name="resizableUserControl"></param>
        public void SetActiveControl(ResizableUserControl resizableUserControl)
        {
            var controls = this.ContainerPanel.Controls.OfType<ResizableUserControl>().ToList();

            controls.ForEach(x =>
            {
                if (x.Equals(resizableUserControl))
                {
                    x.BackColor = Color.White;

                    if (x.Tag != null && x.Tag is Button button)
                        button.FlatAppearance.BorderSize = 1;
                }
                else
                {
                    x.BackColor = Color.WhiteSmoke;

                    if (x.Tag != null && x.Tag is Button button)
                        button.FlatAppearance.BorderSize = 0;
                }
            });
        }

        /// <summary>
        /// RegLocationSize
        /// </summary>
        public void RegLocationSize()
        {
            if (!this.RDP.FullScreenStatus)
            {
                this.OrgLocation = this.Location;
                this.OrgSize = this.Size;
            }
        }
        /// <summary>
        /// SetLocationSize
        /// </summary>
        public void SetLocationSize()
        {
            this.Location = this.OrgLocation;
            this.Size = this.OrgSize;
        }

        /// <summary>
        /// OnPaint
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.Dock == DockStyle.Fill) return;

            Rectangle titleRect = new(0, this.Height - (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0), this.Width, (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0));
            Rectangle titleRect1 = new(0, this.Height - (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0) + 3, this.Width, (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0) - 6);

            e.Graphics.FillRectangle(Brushes.DarkSlateBlue, titleRect);

            TextRenderer.DrawText(e.Graphics
                , $"{this.RDP.CreateID}) 서버:{(this.SharedStatus.Visible_SERVER ? this.Server.SERVER : new('*', this.Server.SERVER.Length))
                } 사용자:{(this.SharedStatus.Visible_USER_NAME ? this.Server.USER_NAME : new('*', this.Server.USER_NAME.Length))
                } 서버이름:{this.Server.SERVER_NAME}"
                , this.Font, titleRect1, Color.White, TextFormatFlags.Top | TextFormatFlags.Left);
            TextRenderer.DrawText(e.Graphics
                , $"    원격해상도:{this.RDP.DesktopWidth}x{this.RDP.DesktopHeight}(설정해상도:{this.Server.DESKTOP_WIDTH}x{this.Server.DESKTOP_HEIGHT
                })  뷰크기:{this.Width}x{this.Height - (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0)}(설정뷰크기:{this.Server.CONTROL_WIDTH}x{this.Server.CONTROL_HEIGHT
                })  {(this.Server.IsFullScreen ? "전체화면 " : "")
                }{(this.Server.IsUseMultimon ? "다중모니터 " : (this.Server.ScreenBounds != null && Screen.AllScreens.Length > 1)
                                                                ? $"{Screen.AllScreens.SingleOrDefault(x => this.Server.ScreenBounds.Equals(x.Bounds))?.DeviceName.Replace(".", "").Replace("\\", "")} " : "")
                }{(this.Server.IsSmartSizing ? "스마트사이징 " : "")
                }{(this.Server.IS_REDIRECT_CLIPBOARD ? "클립보드 " : "")
                }{(this.Server.IS_REDIRECT_DRIVES ? "드라이브 " : "")
                }{(this.Server.IS_REDIRECT_PORTS ? "포트 " : "")
                } {(this.Server.IS_CONNECT_TO_ADMINISTRATOR_SERVER ? "관리자권한 " : "")
                }"
                , this.Font, titleRect1, Color.White, TextFormatFlags.Bottom | TextFormatFlags.Left);

            // 투명 그립
            using Brush b = new SolidBrush(Color.FromArgb(50, 0, 0, 0));
            e.Graphics.FillRectangle(b, new Rectangle(this.Width - this.GripSize, this.Height - this.GripSize, this.GripSize, this.GripSize));
        }

        /// <summary>
        /// OnMouseDown
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Rectangle titleRect = new(0, 0, this.Width - this.GripSize, this.Height - this.GripSize);

            this.ResizeDirection = this.GetResizeDirection(e.Location);

            //if (e.Button == MouseButtons.Right && titleRect.Contains(e.Location))
            if (e.Button == MouseButtons.Right)
            {
                this.ContextMenuShow(this, e.Location);
                return;
            }

            if (this.Dock == DockStyle.Fill) return;

            if (titleRect.Contains(e.Location) && e.Button == MouseButtons.Left)
            {
                this.IsMoving = true;
                this.LastMousePos = e.Location;
                this.BringToFront();
                this.Capture = true;
            }
            else if (this.ResizeDirection != ResizeDirection.None)
            {
                this.IsResizing = true;
                this.LastMousePos = e.Location;
                this.Capture = true;
                this.BringToFront();
            }
        }
        /// <summary>
        /// ContextMenuShow
        /// </summary>
        /// <param name="control"></param>
        /// <param name="position"></param>
        public void ContextMenuShow(System.Windows.Forms.Control control, Point position)
        {
            if (this.RDP.IsConnected)
            {
                this.MenuItemGoFullScreen.Enabled = true;
                this.MenuItemGoConnect.Enabled = false;
                this.MenuItemGoDisconnect.Enabled = true;

                this.MenuItemFullScreen.Enabled = false;
                this.MenuItemMultimon.Enabled = false;
                this.MenuItemSetupDesktopSizeToDesktopSize.Enabled = false;
                this.MenuItemSetupViewSizeToDesktopSize.Enabled = false;
                this.MenuItemViewSizeToDesktopSize.Enabled = false;
                this.MenuItemDesktopSize.Enabled = false;
            }
            else
            {
                this.MenuItemGoFullScreen.Enabled = false;
                this.MenuItemGoConnect.Enabled = true;
                this.MenuItemGoDisconnect.Enabled = false;

                this.MenuItemFullScreen.Enabled = true;
                this.MenuItemMultimon.Enabled = true;
                this.MenuItemSetupDesktopSizeToDesktopSize.Enabled = true;
                this.MenuItemSetupViewSizeToDesktopSize.Enabled = true;
                this.MenuItemViewSizeToDesktopSize.Enabled = true;
                this.MenuItemDesktopSize.Enabled = true;
            }

            this.MenuItemDesktopSizeToViewSize.Enabled = !this.SharedStatus.IsTab;
            this.MenuItemSetupViewSizeToViewSize.Enabled = !this.SharedStatus.IsTab;
            this.MenuItemViewSize.Enabled = !this.SharedStatus.IsTab;

            if (this.RDP_FullScreenStatus)//전체 화면 상태이면 또 전체 화면을 못 누르게 막음
            {
                this.MenuItemGoFullScreen.Enabled = false;
                this.MenuItemGoDisconnect.Enabled = false;
                this.MenuItemGoClose.Enabled = false;
                this.MenuItemTabModeChange.Enabled = false;
                this.MenuItemScreen.Enabled = false;
                this.MenuItemSmartSizing.Enabled = false;
                this.MenuItemDesktopSizeToViewSize.Enabled = false;
                this.MenuItemSetupViewSizeToViewSize.Enabled = false;
                this.MenuItemViewSize.Enabled = false;

                this.ToolStripMenuItemMovingLock.Enabled = false;
                this.ToolStripMenuItemResizeLock.Enabled = false;

                this.ToolStripMenuItemVisibleTitle.Enabled = false;
                this.ToolStripMenuItemVisible_SERVER.Enabled = false;
                this.ToolStripMenuItemVisible_USER_NAME.Enabled = false;
            }
            else
            {
                this.MenuItemGoClose.Enabled = true;
                this.MenuItemTabModeChange.Enabled = true;
                this.MenuItemScreen.Enabled = true;
                this.MenuItemSmartSizing.Enabled = true;
                this.ToolStripMenuItemMovingLock.Enabled = true;
                this.ToolStripMenuItemResizeLock.Enabled = true;

                this.ToolStripMenuItemVisibleTitle.Enabled = true;
                this.ToolStripMenuItemVisible_SERVER.Enabled = true;
                this.ToolStripMenuItemVisible_USER_NAME.Enabled = true;
            }

            this.MenuItemScreen.DropDownItems.Clear();
            this.MenuItemScreen.DropDownItems.Add(this.MenuItemMultimon);


            foreach (var ctrl in this.ContainerPanel.Controls.OfType<ResizableUserControl>().OrderBy(x => x.CreateID))
            {
                if (!ctrl.Equals(this) && ctrl.Server.IsFullScreen)
                {
                    this.MenuItemMultimon.Enabled = false;
                    break;
                }
            }
            foreach (var ctrl in this.ContainerPanel.Controls.OfType<ResizableUserControl>().OrderBy(x => x.CreateID))
            {
                if (!ctrl.Equals(this) && ctrl.Server.IsUseMultimon)
                {
                    this.MenuItemFullScreen.Enabled = false;
                    break;
                }
            }

            if (Screen.AllScreens.Length > 1)
            {
                string deviceName;
                ToolStripMenuItem toolStripMenu;
                foreach (var screen in Screen.AllScreens)
                {
                    deviceName = screen.DeviceName.Replace(".", "").Replace("\\", "");

                    toolStripMenu = new ToolStripMenuItem($"{deviceName}: {screen.Bounds.Width}x{screen.Bounds.Height} ({screen.Bounds.X},{screen.Bounds.Y})", null
                        , (s, e) =>
                        {
                            if (s is ToolStripMenuItem menuItem && menuItem.Tag != null && menuItem.Tag is Screen sc)
                            {
                                if (menuItem.Checked)
                                {
                                    menuItem.Checked = false;
                                    this.Server.ScreenBounds = null;
                                }
                                else
                                {
                                    menuItem.Checked = true;
                                    this.Server.IsUseMultimon = false;
                                    this.MenuItemMultimon.Checked = false;
                                    this.Server.ScreenBounds = sc.Bounds;

                                    //foreach (var ctrl in this.ContainerPanel.Controls.OfType<ResizableUserControl>().OrderBy(x => x.CreateID))
                                    //{
                                    //    if (!ctrl.Equals(this) && !ctrl.Server.IsConnected && ctrl.Server.ScreenBounds != null
                                    //     && sc.Bounds.Contains((Rectangle)ctrl.Server.ScreenBounds))//연결 안되어 있고 동일한 디스플레이가 선택 되어 있으면 선택 해제(다중도 해제)
                                    //    {
                                    //        ctrl.Server.ScreenBounds = null;
                                    //        ctrl.Server.IsUseMultimon = false;
                                    //    }
                                    //}
                                }

                                this.Form.Refresh();
                            }
                        })
                    {
                        Tag = screen,
                    };

                    this.MenuItemScreen.DropDownItems.Add(toolStripMenu);

                    //foreach (var ctrl in this.ContainerPanel.Controls.OfType<ResizableUserControl>().OrderBy(x => x.CreateID))
                    //{
                    //    if (
                    //        (!ctrl.Equals(this) && ctrl.Server.IsConnected && ctrl.Server.ScreenBounds != null && screen.Bounds.Contains((Rectangle)ctrl.Server.ScreenBounds))//다른 연결에서 설정한 모니터는 선택 불가
                    //        || (ctrl.Server.IsConnected && ctrl.Server.IsUseMultimon)//연결 되어있고 다중모니터 사용중이면 비활성화
                    //        )
                    //    {
                    //        toolStripMenu.Enabled = false;
                    //        break;
                    //    }
                    //}
                }

                bool menuItemChecked = false;
                foreach(var item in this.MenuItemScreen.DropDownItems)
                {
                    if (item is ToolStripMenuItem menuItem && menuItem.Tag != null && menuItem.Tag is Screen screen)
                    {
                        menuItem.Checked = this.Server.IsUseMultimon == false && this.Server.ScreenBounds.Equals(screen.Bounds);
                        if (!menuItemChecked && menuItem.Checked)
                        {
                            menuItemChecked = true;
                            break;
                        }
                    }
                }
                if (!menuItemChecked)
                    this.Server.ScreenBounds = null;
            }

            this.MenuItemSetupDesktopSizeToDesktopSize.Checked = this.RDP.DesktopWidth == this.Server.DESKTOP_WIDTH && this.RDP.DesktopHeight == this.Server.DESKTOP_HEIGHT;
            this.MenuItemSetupViewSizeToDesktopSize.Checked = this.RDP.DesktopWidth == this.Server.CONTROL_WIDTH && this.RDP.DesktopHeight == this.Server.CONTROL_HEIGHT;
            this.MenuItemViewSizeToDesktopSize.Checked = this.RDP.DesktopWidth == this.Width && this.RDP.DesktopHeight == this.Height - (this.SharedStatus.IsTab ? 0 : (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0));
            this.MenuItemDesktopSizeToViewSize.Checked = this.Width == this.RDP.DesktopWidth && this.Height == this.RDP.DesktopHeight + (this.SharedStatus.IsTab ? 0 : (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0));
            this.MenuItemSetupViewSizeToViewSize.Checked = this.Width == this.Server.CONTROL_WIDTH && this.Height == this.Server.CONTROL_HEIGHT + (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0);


            if (this.ContainerPanel.Width >= 640 && this.ContainerPanel.Height >= 480)
            {
                int cnt = this.ContainerPanel.Controls.OfType<ResizableUserControl>().ToList().Count;

                // 2 X 2
                Size size = new((this.ContainerPanel.Size.Width - (cnt > 4 ? this.SharedStatus.ContainerPanelRightMargin : 0) - (this.SharedStatus.ResizableUserControlPadding * 3)) / 2
                    , (this.ContainerPanel.Size.Height - (cnt > 4 ? this.SharedStatus.ContainerPanelRightMargin : 0) - ((this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0) * 2) - (this.SharedStatus.ResizableUserControlPadding * 3)) / 2);
                var menuItem = this.MenuItemViewSize.DropDownItems.OfType<ToolStripMenuItem>().FirstOrDefault(x => x.Tag != null && x.Tag is Size size1 && x.Text != null && x.Text.Contains("(2x2)"));
                if (menuItem != null)
                {
                    menuItem.Text = $"{size.Width}x{size.Height} (2x2)";
                    menuItem.Tag = size;

                    menuItem.Enabled = (cnt <= 4);
                }

                // 3 X 3
                size = new((this.ContainerPanel.Size.Width - (cnt > 9 ? this.SharedStatus.ContainerPanelRightMargin : 0) - (this.SharedStatus.ResizableUserControlPadding * 4)) / 3
                    , (this.ContainerPanel.Size.Height - (cnt > 9 ? this.SharedStatus.ContainerPanelRightMargin : 0) - ((this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0) * 3) - (this.SharedStatus.ResizableUserControlPadding * 4)) / 3);
                menuItem = this.MenuItemViewSize.DropDownItems.OfType<ToolStripMenuItem>().FirstOrDefault(x => x.Tag != null && x.Tag is Size size1 && x.Text != null && x.Text.Contains("(3x3)"));
                if (menuItem != null)
                {
                    menuItem.Text = $"{size.Width}x{size.Height} (3x3)";
                    menuItem.Tag = size;

                    menuItem.Enabled = (cnt <= 9);
                }
                // 4 X 4
                size = new((this.ContainerPanel.Size.Width - (cnt > 16 ? this.SharedStatus.ContainerPanelRightMargin : 0) - (this.SharedStatus.ResizableUserControlPadding * 5)) / 4
                    , (this.ContainerPanel.Size.Height - (cnt > 16 ? this.SharedStatus.ContainerPanelRightMargin : 0) - ((this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0) * 2) - (this.SharedStatus.ResizableUserControlPadding * 5)) / 4);
                menuItem = this.MenuItemViewSize.DropDownItems.OfType<ToolStripMenuItem>().FirstOrDefault(x => x.Tag != null && x.Tag is Size size1 && x.Text != null && x.Text.Contains("(4X4)"));
                if (menuItem != null)
                {
                    menuItem.Text = $"{size.Width}x{size.Height} (4X4)";
                    menuItem.Tag = size;

                    menuItem.Enabled = (cnt <= 16);
                }



                // 2 X 2
                size = new((this.ContainerPanel.Size.Width - (cnt > 4 ? this.SharedStatus.ContainerPanelRightMargin : 0) - (this.SharedStatus.ResizableUserControlPadding * 3)) / 2
                    , (this.ContainerPanel.Size.Height - (cnt > 4 ? this.SharedStatus.ContainerPanelRightMargin : 0) - ((this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0) * 2) - (this.SharedStatus.ResizableUserControlPadding * 3)) / 2);
                menuItem = this.MenuItemDesktopSize.DropDownItems.OfType<ToolStripMenuItem>().FirstOrDefault(x => x.Tag != null && x.Tag is Size size1 && x.Text != null && x.Text.Contains("(2x2)"));
                if (menuItem != null)
                {
                    menuItem.Text = $"{size.Width}x{size.Height} (2x2)";
                    menuItem.Tag = size;

                    menuItem.Enabled = (cnt <= 4);
                }
                // 3 X 3
                size = new((this.ContainerPanel.Size.Width - (cnt > 9 ? this.SharedStatus.ContainerPanelRightMargin : 0) - (this.SharedStatus.ResizableUserControlPadding * 4)) / 3
                    , (this.ContainerPanel.Size.Height - (cnt > 9 ? this.SharedStatus.ContainerPanelRightMargin : 0) - ((this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0) * 3) - (this.SharedStatus.ResizableUserControlPadding * 4)) / 3);
                menuItem = this.MenuItemDesktopSize.DropDownItems.OfType<ToolStripMenuItem>().FirstOrDefault(x => x.Tag != null && x.Tag is Size size1 && x.Text != null && x.Text.Contains("(3x3)"));
                if (menuItem != null)
                {
                    menuItem.Text = $"{size.Width}x{size.Height} (3x3)";
                    menuItem.Tag = size;

                    menuItem.Enabled = (cnt <= 9);
                }
                // 4 X 4
                size = new((this.ContainerPanel.Size.Width - (cnt > 16 ? this.SharedStatus.ContainerPanelRightMargin : 0) - (this.SharedStatus.ResizableUserControlPadding * 5)) / 4
                    , (this.ContainerPanel.Size.Height - (cnt > 16 ? this.SharedStatus.ContainerPanelRightMargin : 0) - ((this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0) * 2) - (this.SharedStatus.ResizableUserControlPadding * 5)) / 4);
                menuItem = this.MenuItemDesktopSize.DropDownItems.OfType<ToolStripMenuItem>().FirstOrDefault(x => x.Tag != null && x.Tag is Size size1 && x.Text != null && x.Text.Contains("(4X4)"));
                if (menuItem != null)
                {
                    menuItem.Text = $"{size.Width}x{size.Height} (4X4)";
                    menuItem.Tag = size;

                    menuItem.Enabled = (cnt <= 16);
                }
            }
            else
            {
                this.MenuItemViewSize.Enabled = false;
                this.MenuItemDesktopSize.Enabled = false;
            }

            foreach (var item in this.MenuItemViewSize.DropDownItems)
            {
                if (item is ToolStripMenuItem menuItem && menuItem.Tag != null && menuItem.Tag is Size size)
                    menuItem.Checked = this.Width == size.Width && this.Height == size.Height + (this.SharedStatus.IsTab ? 0 : (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0));
            }
            foreach (var item in this.MenuItemDesktopSize.DropDownItems)
            {
                if (item is ToolStripMenuItem menuItem && menuItem.Tag != null && menuItem.Tag is Size size)
                    menuItem.Checked = this.RDP.DesktopWidth == size.Width && this.RDP.DesktopHeight == size.Height;
            }

            this.ToolStripMenuItemMovingLock.Checked = this.SharedStatus.IsMovingLock;
            this.ToolStripMenuItemResizeLock.Checked = this.SharedStatus.IsResizeLock;
            this.ToolStripMenuItemVisibleTitle.Checked = this.SharedStatus.IsTitleVisible;
            this.ToolStripMenuItemVisible_SERVER.Checked = this.SharedStatus.Visible_SERVER;
            this.ToolStripMenuItemVisible_USER_NAME.Checked = this.SharedStatus.Visible_USER_NAME;

            this.Menu.Show(control, position);
        }
        /// <summary>
        /// OnMouseMove
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!this.IsResizing && !this.IsMoving)
            {
                if (this.Dock != DockStyle.Fill && !this.SharedStatus.IsResizeLock)
                {
                    this.ResizeDirection = this.GetResizeDirection(e.Location);

                    this.Cursor = this.ResizeDirection switch
                    {
                        ResizeDirection.BottomRight => Cursors.SizeNWSE,
                        ResizeDirection.Right => Cursors.SizeWE,
                        ResizeDirection.Bottom => Cursors.SizeNS,
                        _ => new Rectangle(this.GripSize, 0, this.Width - this.GripSize - this.GripSize, (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0)).Contains(e.Location) ? Cursors.Default : Cursors.Default,
                    };
                }
                else
                {
                    this.Cursor = Cursors.Default;
                    return;
                }
            }

            if (this.IsResizing && !this.SharedStatus.IsResizeLock)
            {
                if (this.RDP.IsConnected && this.RDP.FullScreenStatus) return;

                int dx = e.X - this.LastMousePos.X;
                int dy = e.Y - this.LastMousePos.Y;

                Rectangle r = this.Bounds;
                switch (this.ResizeDirection)
                {
                    case ResizeDirection.Right: r.Width += dx; break;
                    case ResizeDirection.Bottom: r.Height += dy; break;
                    case ResizeDirection.BottomRight: r.Width += dx; r.Height += dy; break;
                }

                if (r.Width < this.MinimumSize.Width) r.Width = this.MinimumSize.Width;
                if (r.Height < this.MinimumSize.Height) r.Height = this.MinimumSize.Height;

                this.Bounds = r;
                this.LastMousePos = e.Location;
                this.Invalidate();
            }
            else if (this.IsMoving && !this.SharedStatus.IsMovingLock)
            {
                if (this.RDP.IsConnected && this.RDP.FullScreenStatus) return;

                int dx = e.X - this.LastMousePos.X;
                int dy = e.Y - this.LastMousePos.Y;
                this.Left += dx;
                this.Top += dy;
            }
        }
        /// <summary>
        /// OnMouseUp
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (this.Dock == DockStyle.Fill) return;

            this.IsResizing = false;
            this.IsMoving = false;
            this.ResizeDirection = ResizeDirection.None;
            this.Capture = false;

            this.RegLocationSize();
        }
        private ResizeDirection GetResizeDirection(Point mouse)
        {
            bool right = mouse.X >= this.Width - this.GripSize;
            bool bottom = mouse.Y >= this.Height - this.GripSize;

            if (right && bottom) return ResizeDirection.BottomRight;
            if (right) return ResizeDirection.Right;
            if (bottom) return ResizeDirection.Bottom;
            return ResizeDirection.None;
        }

        private async void OnEnterFullScreenMode(object? sender, EventArgs e)
        {
            this.RDP.FullScreenStatus = true;

            if (this.Server.IsUseMultimon)
            {
                Rectangle rectangle;

                rectangle = Dpi.GetTotalScreenBounds();

                this.SharedStatus.FormOrgFormWindowState = (int)this.Form.WindowState;

                this.Form.WindowState = FormWindowState.Maximized;
                this.Form.FormBorderStyle = FormBorderStyle.None;
                this.Form.WindowState = FormWindowState.Normal;

                this.SharedStatus.FormOrgLocation = this.Form.Location;
                this.SharedStatus.FormOrgSize = this.Form.Size;

                this.Form.Location = new Point(rectangle.X, rectangle.Y);
                this.Form.Size = new Size(rectangle.Width, rectangle.Height + (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0));

#if !DEBUG
                this.Form.TopMost = true;
#endif

                this.RegLocationSize();

                this.TopControl.Visible = false;
                this.BottomControl.Visible = false;

                this.ContainerPanel.AutoScroll = false;

                this.Location = new Point(0, 0);
                this.Size = new Size(this.RDP.DesktopWidth, this.RDP.DesktopHeight + (this.SharedStatus.IsTitleVisible ? this.SharedStatus.TitleHeightConst : 0));
            }
            else
            {
                //전체 화면 전환하기 전에 윈도우 상태를 저장했으면
                if (this.Server.ScreenBounds != null && this.IsGoFullScreenSaveFormWindowStateLocation)
                {
                    await Task.Delay(700); // 0.7초 딜레이

                    this.Form.Location = this.SharedStatus.FormOrgLocation;
                    this.Form.WindowState = (FormWindowState)this.SharedStatus.FormOrgFormWindowState;
                }
            }

            this.IsGoFullScreenSaveFormWindowStateLocation = false;

            this.SharedStatus.IsTitleVisible = true;

            foreach (var ctrl in this.ContainerPanel.Controls.OfType<ResizableUserControl>().OrderBy(x => x.CreateID))
                ctrl.SetTitleHeight();

            this.BringToFront();

            this.Refresh();
        }
        private void OnLeaveFullScreenMode(object? sender, EventArgs e)
        {
            this.RDP.FullScreenStatus = false;

            if (this.Server.IsUseMultimon)
            {
                this.Form.WindowState = FormWindowState.Normal;
                this.Form.Location = SharedStatus.FormOrgLocation;
                this.Form.Size = SharedStatus.FormOrgSize;

                this.Form.FormBorderStyle = FormBorderStyle.Sizable;
                this.Form.WindowState = (FormWindowState)SharedStatus.FormOrgFormWindowState;

                this.Form.TopMost = false;

                if (this.SharedStatus.IsTab)
                    this.TopControl.Visible = true;

                this.BottomControl.Visible = true;

                this.ContainerPanel.AutoScroll = true;

                this.SetLocationSize();
            }

            this.Refresh();
        }

        private void OnConnected(object? sender, EventArgs e)
        {
            this.Server.ConnectedStartTime = DateTime.Now;
            this.Server.Status = "Connected";
            this.Server.IsConnected = true;
            this.Server.DoAction(this.Server, new() { Action = "Status", Value = this.Server.Status });

            this.RDP.ConnectionBarText = $"{this.Server.SERVER} : {this.Server.ConnectedStartTime:MM-dd HH:mm:ss}";
        }
        private void OnDisconnected(object sender, AxMSTSCLib.IMsTscAxEvents_OnDisconnectedEvent e)
        {
            this.RDP.Dock = DockStyle.None;
            this.RDP.Size = new Size(0, 0);
            this.RDP.Location = this.RDPInitLocation;
            this.BorderStyle = BorderStyle.FixedSingle;

            this.Server.ConnectedStartTime = null;
            this.Server.Status = "Disconnected";
            this.Server.IsConnected = false;
            this.Server.DoAction(this.Server, new() { Action = "Status", Value = this.Server.Status });
        }

        /// <summary>
        /// InitRDP
        /// </summary>
        public void InitRDP()
        {
            this.RDP.Size = new(0, 0);

            if (this.Server.DesktopWidth != 0 && this.Server.DesktopHeight != 0)
            {
                this.RDP.DesktopWidth = this.Server.DesktopWidth;
                this.RDP.DesktopHeight = this.Server.DesktopHeight;
            }
            else
            {
                this.RDP.DesktopWidth = this.Server.DESKTOP_WIDTH;
                this.RDP.DesktopHeight = this.Server.DESKTOP_HEIGHT;
            }

            this.RDP.SmartSizing = this.Server.IsSmartSizing;
            this.RDP.FullScreenWhenConnectd = this.Server.IsFullScreen;
            if (this.RDP.FullScreenWhenConnectd) this.RDP.UseMultimon = this.Server.IsUseMultimon;
        }

        /// <summary>
        /// Connect
        /// </summary>
        public void Connect()
        {
            Rectangle rectangle;
            Size? size = null;

            if (this.RDP.IsConnected) return;

            this.RDP.Server = this.Server.SERVER;
            this.RDP.UserName = this.Server.USER_NAME;
            this.RDP.ClearTextPassword = this.Server.USER_ACCESS_NUMBER;

            this.RDP.ColorDepth = this.Server.COLOR_DEPTH;
            this.RDP.SmartSizing = this.Server.IsSmartSizing;
            this.RDP.FullScreenWhenConnectd = this.Server.IsFullScreen;
            if (this.RDP.FullScreenWhenConnectd) this.RDP.UseMultimon = this.Server.IsUseMultimon;

            this.RDP.RedirectClipboard = this.Server.IS_REDIRECT_CLIPBOARD;
            this.RDP.RedirectDevices = this.Server.IS_REDIRECT_DEVICES;
            this.RDP.RedirectDrives = this.Server.IS_REDIRECT_DRIVES;

            this.RDP.RedirectPorts = this.Server.IS_REDIRECT_PORTS;
            this.RDP.RedirectPOSDevices = this.Server.IS_REDIRECT_POS_DEVICES;
            this.RDP.RedirectPrinters = this.Server.IS_REDIRECT_PRINTERS;

            this.RDP.RedirectSmartCards = this.Server.IS_REDIRECT_SMART_CARDS;
            this.RDP.ConnectToAdministerServer = this.Server.IS_CONNECT_TO_ADMINISTRATOR_SERVER;
            this.RDP.KeyboardHookMode = this.Server.IS_KEYBOARD_HOOK_MODE;

            this.RDP.HotKeyCtrlAltDel = true;
            this.RDP.HotKeyFullScreen = true;
            this.RDP.EnableSuperPan = true;

            this.RDP.ConnectionBarShowMinimizeButton = false;

            this.RDP.ConnectingText = $"Connecting : {this.Server.SERVER_NAME}";
            this.RDP.DisconnectedText = $"Disconnected : {this.Server.SERVER_NAME}";


            if (this.Server.IsFullScreen)
            {
                if (this.Server.IsUseMultimon)
                {
                    rectangle = Dpi.GetTotalScreenBounds();

                    this.RDP.DesktopWidth = rectangle.Width;
                    this.RDP.DesktopHeight = rectangle.Height;
                }
                else
                {
                    if (this.Server.ScreenBounds == null)
                        size = Screen.FromControl(this.Form).Bounds.Size;
                    else
                    {
                        foreach (var screen in Screen.AllScreens)
                        {
                            if (this.Server.ScreenBounds.Equals(screen.Bounds))
                            { 
                                size = screen.Bounds.Size;
                                break;
                            }
                        }
                    }

                    if (size == null)
                        size = Screen.FromControl(this.Form).Bounds.Size;

                    this.RDP.DesktopWidth = ((Size)size).Width;
                    this.RDP.DesktopHeight = ((Size)size).Height;

                    this.SaveGoFullScreenFormWindowStateLocation();
                }

                this.RDP.GoFullScreen();
            }

            this.RDP.Dock = DockStyle.Fill;
            this.BorderStyle = BorderStyle.None;

            this.Server.IsConnected = true;
            this.Server.DoAction(this.Server, new() { Action = "Status", Value = "Connecting" });

            this.RDP.Connect();
        }
        /// <summary>
        /// Disconnect
        /// </summary>
        public void Disconnect()
        {
            if (this.RDP.IsConnected)
                this.RDP.Disconnect();
        }

        private void SaveGoFullScreenFormWindowStateLocation()
        {
            if (!this.Server.IsUseMultimon && this.Server.ScreenBounds != null)
            {
                this.IsGoFullScreenSaveFormWindowStateLocation = true;
                this.SharedStatus.FormOrgFormWindowState = (int)this.Form.WindowState;
                this.Form.WindowState = FormWindowState.Normal;
                this.SharedStatus.FormOrgLocation = new(this.Form.Location.X, this.Form.Location.Y);

                this.Form.Location = new(((Rectangle)this.Server.ScreenBounds).X, ((Rectangle)this.Server.ScreenBounds).Y);
            }
        }
    }
}