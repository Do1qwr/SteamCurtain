using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

[assembly: System.Reflection.AssemblyProduct("SteamCurtain")]
[assembly: System.Reflection.AssemblyTitle("SteamCurtain")]
[assembly: System.Reflection.AssemblyCompany("๖ۣۜMoήSter√♫")]
[assembly: System.Reflection.AssemblyCopyright("© 2026 ๖ۣۜMoήSter√♫")]
[assembly: System.Reflection.AssemblyVersion("1.0.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.0.0.0")]

namespace SteamCurtain
{
    static class Native
    {
        public struct RECT { public int Left, Top, Right, Bottom; }
        public struct POINT { public int X, Y; }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
        [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int cx, int cy, uint flags);
        [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int cmd);
        [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr h, int cmd);
        [DllImport("user32.dll")] public static extern short GetAsyncKeyState(int vKey);
        [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr h);
        [DllImport("user32.dll")] public static extern bool IsWindow(IntPtr h);
        [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern int GetClassName(IntPtr h, StringBuilder s, int n);
        [DllImport("user32.dll")] public static extern bool GetCursorPos(out POINT p);
        [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
        [DllImport("user32.dll")] public static extern bool BringWindowToTop(IntPtr h);
        [DllImport("user32.dll")] public static extern bool AttachThreadInput(uint a, uint b, bool attach);
        [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
        [DllImport("winmm.dll")] public static extern uint timeBeginPeriod(uint ms);

        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;
        public const int SW_RESTORE = 9;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_ASYNCWINDOWPOS = 0x4000;
        public const int VK_LBUTTON = 0x01;
        public const int VK_RBUTTON = 0x02;
        public const int VK_MBUTTON = 0x04;
        public const int VK_ESCAPE = 0x1B;

        [StructLayout(LayoutKind.Sequential)]
        public struct THUMBNAILPROPS
        {
            public uint dwFlags;
            public RECT rcDestination;
            public RECT rcSource;
            public byte opacity;
            [MarshalAs(UnmanagedType.Bool)] public bool fVisible;
            [MarshalAs(UnmanagedType.Bool)] public bool fSourceClientAreaOnly;
        }

        public const uint DWM_TNP_RECTDESTINATION = 0x00000001;
        public const uint DWM_TNP_OPACITY = 0x00000004;
        public const uint DWM_TNP_VISIBLE = 0x00000008;

        [DllImport("dwmapi.dll")] public static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);
        [DllImport("dwmapi.dll")] public static extern int DwmUnregisterThumbnail(IntPtr thumb);
        [DllImport("dwmapi.dll")] public static extern int DwmUpdateThumbnailProperties(IntPtr thumb, ref THUMBNAILPROPS props);
        [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h, IntPtr hdc, uint flags);
        public const uint PW_RENDERFULLCONTENT = 0x00000002;

        [DllImport("gdi32.dll")] public static extern IntPtr CreateRoundRectRgn(int left, int top, int right, int bottom, int w, int h);
        [DllImport("user32.dll")] public static extern int SetWindowRgn(IntPtr h, IntPtr region, bool redraw);
        [DllImport("gdi32.dll")] public static extern bool DeleteObject(IntPtr obj);
        [DllImport("dwmapi.dll")] public static extern int DwmSetWindowAttribute(IntPtr h, int attr, ref int value, int size);
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    }

    sealed class ThumbHost : Form
    {
        protected override bool ShowWithoutActivation { get { return true; } }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x08000000;
                cp.ExStyle |= 0x00000080;
                cp.ExStyle |= 0x00000020;
                return cp;
            }
        }
    }

    sealed class SettingsForm : Form
    {
        public int CornerSize;
        public int MinSpeed;
        public int AnimMs;
        public int SwitchHideMs;
        public bool TopRight;
        public bool BottomRight;
        public bool TopLeft;
        public bool BottomLeft;
        public bool Rounded;

        public SettingsForm(int cornerSize, int minSpeed, int animMs, int switchHideMs,
                            bool tr, bool br, bool tl, bool bl, bool rounded,
                            string authorText, string authorUrl)
        {
            CornerSize = cornerSize;
            MinSpeed = minSpeed;
            AnimMs = animMs;
            SwitchHideMs = switchHideMs;
            TopRight = tr;
            BottomRight = br;
            TopLeft = tl;
            BottomLeft = bl;
            Rounded = rounded;

            Text = "Steam-шторка — настройки";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(380, 350);
            Font = new Font("Segoe UI", 9f);

            int y = 12;
            y = AddSlider("Размер зоны вызова, точек", 60, 320, CornerSize, y, delegate (int v) { CornerSize = v; });
            y = AddSlider("Порог скорости мыши, пикс/с", 40, 600, MinSpeed, y, delegate (int v) { MinSpeed = v; });
            y = AddSlider("Длительность анимации, мс", 100, 800, AnimMs, y, delegate (int v) { AnimMs = v; });
            y = AddSlider("Скрытие при смене окна, мс", 300, 4000, SwitchHideMs, y, delegate (int v) { SwitchHideMs = v; });

            var group = new GroupBox();
            group.Text = "Углы вызова";
            group.Left = 12;
            group.Top = y;
            group.Width = 356;
            group.Height = 106;

            var cb1 = new CheckBox { Text = "верхний правый", Left = 12, Top = 20, Width = 150, Checked = TopRight };
            var cb2 = new CheckBox { Text = "нижний правый", Left = 180, Top = 20, Width = 150, Checked = BottomRight };
            var cb3 = new CheckBox { Text = "верхний левый", Left = 12, Top = 46, Width = 150, Checked = TopLeft };
            var cb4 = new CheckBox { Text = "нижний левый", Left = 180, Top = 46, Width = 150, Checked = BottomLeft };
            var cb5 = new CheckBox { Text = "скруглять углы окна Steam", Left = 12, Top = 74, Width = 300, Checked = Rounded };

            group.Controls.Add(cb1);
            group.Controls.Add(cb2);
            group.Controls.Add(cb3);
            group.Controls.Add(cb4);
            group.Controls.Add(cb5);
            Controls.Add(group);

            cb1.CheckedChanged += delegate { TopRight = cb1.Checked; };
            cb2.CheckedChanged += delegate { BottomRight = cb2.Checked; };
            cb3.CheckedChanged += delegate { TopLeft = cb3.Checked; };
            cb4.CheckedChanged += delegate { BottomLeft = cb4.Checked; };
            cb5.CheckedChanged += delegate { Rounded = cb5.Checked; };

            var ok = new Button { Text = "Сохранить", Left = 190, Top = y + 116, Width = 90, DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Отмена", Left = 288, Top = y + 116, Width = 80, DialogResult = DialogResult.Cancel };
            Controls.Add(ok);
            Controls.Add(cancel);

            AcceptButton = ok;
            CancelButton = cancel;

            var link = new LinkLabel();
            link.Text = "автор: " + authorText;
            link.Left = 12;
            link.Top = y + 122;
            link.Width = 170;
            link.TabStop = false;
            link.LinkBehavior = LinkBehavior.HoverUnderline;
            link.LinkClicked += delegate
            {
                try { Process.Start(new ProcessStartInfo(authorUrl) { UseShellExecute = true }); }
                catch { }
            };
            Controls.Add(link);
        }

        int AddSlider(string caption, int min, int max, int value, int y, Action<int> setter)
        {
            var lb = new Label { Text = caption, Left = 12, Top = y, Width = 260, AutoSize = false };
            var val = new Label { Text = value.ToString(), Left = 300, Top = y, Width = 68, TextAlign = ContentAlignment.TopRight };
            var tb = new TrackBar
            {
                Left = 8,
                Top = y + 20,
                Width = 356,
                Height = 30,
                Minimum = min,
                Maximum = max,
                Value = Math.Min(Math.Max(value, min), max),
                TickFrequency = Math.Max(1, (max - min) / 5),
                SmallChange = Math.Max(1, (max - min) / 60),
                LargeChange = Math.Max(5, (max - min) / 10)
            };
            tb.Scroll += delegate
            {
                val.Text = tb.Value.ToString();
                setter(tb.Value);
            };
            Controls.Add(lb);
            Controls.Add(val);
            Controls.Add(tb);
            return y + 62;
        }
    }

    sealed class Curtain : ApplicationContext
    {
        const string SteamClass = "SDL_app";
        const string SteamTitle = "Steam";
        const string Version = "1.0.0";
        const string Author = "๖ۣۜMoήSter√♫";
        const string AuthorUrl = "https://s.team/p/cjvf-jfbm/WCRVJMPJ";
        const int ButtonCooldownMs = 400;
        const int AnimTickMs = 10;
        const int DwellMs = 80;
        const int CornerRadius = 18;
        const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        const int DWMWCP_ROUND = 2;
        const int TickIdleMs = 30;
        const int TickGameMs = 1000;
        const int PidRefreshMs = 15000;
        const int WorkAreaRefreshMs = 2000;

        readonly System.Windows.Forms.Timer timer;
        readonly System.Windows.Forms.Timer animTimer;
        readonly NotifyIcon tray;
        ToolStripMenuItem dockMenuItem;
        ToolStripMenuItem animateMenuItem;
        readonly string settingsPath;
        readonly string logPath;
        readonly uint ownPid;
        readonly HashSet<uint> steamPids = new HashSet<uint>();

        IntPtr steam;
        bool shown;
        bool paused;
        bool armed = true;
        bool hideOnSwitch = true;
        long edgeSince;
        long otherSince;
        long lastHide;
        long pidsAt = -100000;
        long waAt = -100000;
        int currentInterval;
        int enforceTick;
        Rectangle wa = Rectangle.Empty;
        bool docked = true;
        int posX;
        int posY;
        int setX;
        int setY;
        bool hadSet;
        bool dragging;
        bool busy;
        int prevX;
        int prevY;
        long prevMoveAt;
        int moveSpeed;
        bool btnWasDown;
        long lastBtnUp;
        bool animate = true;
        bool animating;
        int animFromX;
        int animToX;
        int animY;
        int animW;
        int animH;
        long animStart;
        Form animForm;
        IntPtr thumb;
        Thread animThread;
        IntPtr animHostHwnd;
        Bitmap animBitmap;
        int cornerSize = 120;
        int minSpeed = 120;
        int animMs = 320;
        int switchHideMs = 1200;
        bool cornerTopRight = true;
        bool cornerBottomRight = true;
        bool cornerTopLeft;
        bool cornerBottomLeft;
        bool hiding;
        bool animHide;
        string hideReason = "";
        bool rounded = true;
        bool shownBtnWasDown;
        bool escWasDown;
        readonly Control dispatcher;

        public Curtain()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SteamCurtain");
            Directory.CreateDirectory(dir);
            settingsPath = Path.Combine(dir, "settings.txt");
            logPath = Path.Combine(dir, "log.txt");
            LoadSettings();
            if (!File.Exists(settingsPath)) SaveSettings();

            ownPid = (uint)Process.GetCurrentProcess().Id;
            try { Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal; }
            catch { }

            Native.timeBeginPeriod(1);
            RefreshSteamPids();

            tray = new NotifyIcon();
            tray.Icon = LoadAppIcon();
            tray.Text = "Steam-шторка " + Version;
            tray.ContextMenuStrip = BuildMenu();
            tray.Visible = true;

            timer = new System.Windows.Forms.Timer();
            timer.Interval = TickIdleMs;
            currentInterval = TickIdleMs;
            timer.Tick += OnTick;
            timer.Start();

            animTimer = new System.Windows.Forms.Timer();
            animTimer.Interval = AnimTickMs;
            animTimer.Tick += OnAnimTick;

            dispatcher = new Control();
            dispatcher.CreateControl();
            IntPtr sink = dispatcher.Handle;

            Log("старт v" + Version);
        }

        static void OpenAuthorProfile()
        {
            try
            {
                Process.Start(new ProcessStartInfo(AuthorUrl) { UseShellExecute = true });
            }
            catch { }
        }

        static Icon LoadAppIcon()
        {
            try
            {
                Icon exeIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (exeIcon != null) return exeIcon;
            }
            catch { }
            return SystemIcons.Application;
        }

        ContextMenuStrip BuildMenu()
        {
            var menu = new ContextMenuStrip();
            var head = new ToolStripMenuItem("Steam-шторка " + Version);
            head.Enabled = false;
            menu.Items.Add(head);

            var author = new ToolStripMenuItem("автор: " + Author + " — открыть профиль");
            author.Click += delegate { OpenAuthorProfile(); };
            menu.Items.Add(author);

            menu.Items.Add(new ToolStripSeparator());

            var toggle = new ToolStripMenuItem("Показать / скрыть сейчас");
            toggle.Click += delegate { if (shown) HideDrawer("меню"); else ShowDrawer(); };
            menu.Items.Add(toggle);

            var settings = new ToolStripMenuItem("Настройки…");
            settings.Click += delegate { ShowSettings(); };
            menu.Items.Add(settings);

            var onSwitch = new ToolStripMenuItem("Скрывать при переключении окон");
            onSwitch.Checked = hideOnSwitch;
            onSwitch.Click += delegate
            {
                hideOnSwitch = !hideOnSwitch;
                onSwitch.Checked = hideOnSwitch;
                SaveSettings();
            };
            menu.Items.Add(onSwitch);

            var dockItem = new ToolStripMenuItem("Прикреплять к правому краю");
            dockItem.Checked = docked;
            dockMenuItem = dockItem;
            dockItem.Click += delegate
            {
                docked = !docked;
                dockItem.Checked = docked;
                if (!docked) RememberPosition(false);
                SaveSettings();
                Log(docked ? "режим: правый край" : "режим: свободная позиция");
            };
            menu.Items.Add(dockItem);

            var remember = new ToolStripMenuItem("Запомнить текущее положение");
            remember.Click += delegate { RememberPosition(true); };
            menu.Items.Add(remember);

            var animItem = new ToolStripMenuItem("Плавное появление");
            animItem.Checked = animate;
            animateMenuItem = animItem;
            animItem.Click += delegate
            {
                animate = !animate;
                animItem.Checked = animate;
                SaveSettings();
                Log(animate ? "анимация включена" : "анимация выключена");
            };
            menu.Items.Add(animItem);

            var pause = new ToolStripMenuItem("Пауза");
            pause.Click += delegate
            {
                paused = !paused;
                pause.Checked = paused;
                tray.Text = paused ? "Steam-шторка " + Version + " (пауза)" : "Steam-шторка " + Version;
            };
            menu.Items.Add(pause);

            menu.Items.Add(new ToolStripSeparator());
            var auto = new ToolStripMenuItem("Запускать с Windows");
            auto.Checked = IsAutostart();
            auto.Click += delegate
            {
                bool want = !IsAutostart();
                SetAutostart(want);
                auto.Checked = want;
            };
            menu.Items.Add(auto);

            var quit = new ToolStripMenuItem("Выход");
            quit.Click += delegate
            {
                timer.Stop();
                StopAnimation();
                ClearRoundedCorners();
                tray.Visible = false;
                tray.Dispose();
                ExitThread();
            };
            menu.Items.Add(quit);
            return menu;
        }

        void ShowSettings()
        {
            using (var form = new SettingsForm(cornerSize, minSpeed, animMs, switchHideMs,
                                               cornerTopRight, cornerBottomRight, cornerTopLeft, cornerBottomLeft, rounded,
                                               Author, AuthorUrl))
            {
                if (form.ShowDialog() != DialogResult.OK) return;

                cornerSize = form.CornerSize;
                minSpeed = form.MinSpeed;
                animMs = form.AnimMs;
                switchHideMs = form.SwitchHideMs;
                cornerTopRight = form.TopRight;
                cornerBottomRight = form.BottomRight;
                cornerTopLeft = form.TopLeft;
                cornerBottomLeft = form.BottomLeft;

                bool wasRounded = rounded;
                rounded = form.Rounded;
                if (!rounded && wasRounded) ClearRoundedCorners();

                SaveSettings();
                Log("настройки обновлены: зона=" + cornerSize + " скорость=" + minSpeed +
                    " анимация=" + animMs + " скрытие=" + switchHideMs);
            }
        }

        void OnTick(object sender, EventArgs e)
        {
            if (busy) return;
            busy = true;
            try { Tick(); }
            catch (Exception ex) { Log("ошибка: " + ex.GetType().Name + " " + ex.Message); }
            finally { busy = false; }
        }

        void Tick()
        {
            if (hiding) return;
            long now = Environment.TickCount;

            if (now - pidsAt > PidRefreshMs) RefreshSteamPids();

            if (!shown)
            {
                otherSince = 0;
                Native.POINT cp;
                if (!Native.GetCursorPos(out cp)) return;

                int dx = cp.X - prevX;
                int dy = cp.Y - prevY;
                if (dx != 0 || dy != 0)
                {
                    long dt = now - prevMoveAt;
                    if (prevMoveAt != 0 && dt > 0 && dt < 200)
                        moveSpeed = (int)(Math.Sqrt((double)dx * dx + (double)dy * dy) * 1000.0 / dt);
                    prevMoveAt = now;
                    prevX = cp.X;
                    prevY = cp.Y;
                }
                else if (now - prevMoveAt > 150) moveSpeed = 0;

                bool btn = (Native.GetAsyncKeyState(Native.VK_LBUTTON) & 0x8000) != 0
                        || (Native.GetAsyncKeyState(Native.VK_RBUTTON) & 0x8000) != 0
                        || (Native.GetAsyncKeyState(Native.VK_MBUTTON) & 0x8000) != 0;
                if (btnWasDown && !btn) lastBtnUp = now;
                btnWasDown = btn;

                Rectangle w = WorkArea();
                int corner = Math.Min(cornerSize, Math.Min(w.Width / 2, w.Height / 2));
                bool nearRight = cp.X >= w.Right - corner;
                bool nearLeft = cp.X <= w.Left + corner;
                bool topCorner = cp.Y >= w.Top && cp.Y < w.Top + corner;
                bool bottomCorner = cp.Y >= w.Bottom - corner && cp.Y < w.Bottom;

                bool inZone = (nearRight && ((topCorner && cornerTopRight) || (bottomCorner && cornerBottomRight)))
                           || (nearLeft && ((topCorner && cornerTopLeft) || (bottomCorner && cornerBottomLeft)));

                if (!inZone)
                {
                    edgeSince = 0;
                    armed = true;
                    return;
                }

                if (btn || now - lastBtnUp < ButtonCooldownMs || moveSpeed < minSpeed)
                {
                    edgeSince = 0;
                    return;
                }

                if (!armed) return;
                if (now - lastHide < 350) return;

                if (IsGameForeground())
                {
                    SetInterval(TickGameMs);
                    edgeSince = 0;
                    return;
                }
                SetInterval(TickIdleMs);

                if (edgeSince == 0) edgeSince = now;
                else if (now - edgeSince >= DwellMs)
                {
                    edgeSince = 0;
                    ShowDrawer();
                }
                return;
            }

            IntPtr fg = Native.GetForegroundWindow();
            uint fgPid;
            Native.GetWindowThreadProcessId(fg, out fgPid);
            string cls = ClassOf(fg);

            bool desktop = cls == "Progman" || cls == "WorkerW" || cls == "SHELLDLL_DefView";
            bool mine = fgPid == ownPid || steamPids.Contains(fgPid);

            if (paused) return;

            Native.POINT clickPt;
            bool btnNow = (Native.GetAsyncKeyState(Native.VK_LBUTTON) & 0x8000) != 0;
            if (btnNow && !shownBtnWasDown && Native.GetCursorPos(out clickPt))
            {
                Native.RECT wr;
                if (Native.GetWindowRect(steam, out wr))
                {
                    bool inside = clickPt.X >= wr.Left && clickPt.X < wr.Right
                               && clickPt.Y >= wr.Top && clickPt.Y < wr.Bottom;
                    if (!inside)
                    {
                        shownBtnWasDown = false;
                        HideDrawer("клик вне");
                        return;
                    }
                }
            }
            shownBtnWasDown = btnNow;

            bool escNow = (Native.GetAsyncKeyState(Native.VK_ESCAPE) & 0x8000) != 0;
            if (escNow && !escWasDown)
            {
                escWasDown = escNow;
                HideDrawer("Esc");
                return;
            }
            escWasDown = escNow;

            bool lmb = btnNow;
            if (lmb) dragging = true;
            else if (dragging) { dragging = false; AdoptUserPosition(); }

            if (!desktop && !mine && IsFullscreen(fg))
            {
                SetInterval(TickGameMs);
                HideDrawer("игра");
                return;
            }
            SetInterval(TickIdleMs);

            if (desktop) { HideDrawer("рабочий стол"); return; }
            if (hideOnSwitch && !mine)
            {
                if (otherSince == 0) otherSince = now;
                else if (now - otherSince >= switchHideMs) HideDrawer("другое окно");
            }
            else otherSince = 0;

            if (!dragging && !animating && ++enforceTick >= 4)
            {
                enforceTick = 0;
                EnforcePosition();
            }
        }

        bool IsGameForeground()
        {
            IntPtr fg = Native.GetForegroundWindow();
            uint pid;
            Native.GetWindowThreadProcessId(fg, out pid);
            string cls = ClassOf(fg);
            if (cls == "Progman" || cls == "WorkerW" || cls == "SHELLDLL_DefView") return false;
            if (pid == ownPid || steamPids.Contains(pid)) return false;
            return IsFullscreen(fg);
        }
        Rectangle WorkArea()
        {
            if (Environment.TickCount - waAt > WorkAreaRefreshMs)
            {
                wa = Screen.PrimaryScreen.WorkingArea;
                waAt = Environment.TickCount;
            }
            return wa;
        }

        void RefreshSteamPids()
        {
            pidsAt = Environment.TickCount;
            steamPids.Clear();
            foreach (string name in new string[] { "steam", "steamwebhelper", "steamservice", "gameoverlayui" })
            {
                Process[] found;
                try { found = Process.GetProcessesByName(name); }
                catch { continue; }
                foreach (Process pr in found)
                {
                    try { steamPids.Add((uint)pr.Id); }
                    catch { }
                    finally { pr.Dispose(); }
                }
            }
            if (steam != IntPtr.Zero && !Native.IsWindow(steam)) { steam = IntPtr.Zero; }
        }

        void SetInterval(int ms)
        {
            if (currentInterval == ms) return;
            currentInterval = ms;
            timer.Interval = ms;
        }

        static string ClassOf(IntPtr h)
        {
            if (h == IntPtr.Zero) return "";
            var sb = new StringBuilder(128);
            Native.GetClassName(h, sb, sb.Capacity);
            return sb.ToString();
        }

        static bool IsFullscreen(IntPtr h)
        {
            Native.RECT r;
            if (h == IntPtr.Zero || !Native.GetWindowRect(h, out r)) return false;
            Rectangle b = Screen.PrimaryScreen.Bounds;
            return r.Left <= b.Left && r.Top <= b.Top && r.Right >= b.Right && r.Bottom >= b.Bottom;
        }

        void ApplyRoundedCorners(int width, int height)
        {
            if (!rounded || steam == IntPtr.Zero || width <= 0 || height <= 0) return;

            try
            {
                IntPtr rgn = Native.CreateRoundRectRgn(0, 0, width + 1, height + 1,
                    CornerRadius * 2, CornerRadius * 2);
                if (rgn != IntPtr.Zero && Native.SetWindowRgn(steam, rgn, true) == 0)
                    Native.DeleteObject(rgn);
            }
            catch { }

            try
            {
                int pref = DWMWCP_ROUND;
                Native.DwmSetWindowAttribute(steam, DWMWA_WINDOW_CORNER_PREFERENCE, ref pref, sizeof(int));
            }
            catch { }
        }

        void ClearRoundedCorners()
        {
            if (steam == IntPtr.Zero) return;
            try { Native.SetWindowRgn(steam, IntPtr.Zero, true); }
            catch { }
        }

        void ShowDrawer()
        {
            if (steam == IntPtr.Zero || !Native.IsWindow(steam))
                steam = Native.FindWindow(SteamClass, SteamTitle);
            if (steam == IntPtr.Zero || !Native.IsWindow(steam)) { Log("окно Steam не найдено"); return; }

            if (Native.IsIconic(steam)) Native.ShowWindowAsync(steam, Native.SW_RESTORE);

            Native.RECT r;
            if (!Native.GetWindowRect(steam, out r)) return;

            Rectangle w = WorkArea();
            int width = r.Right - r.Left;
            int height = r.Bottom - r.Top;
            int x, y;
            if (docked)
            {
                x = w.Right - Math.Min(width, w.Width);
                y = Math.Max(w.Top, Math.Min(r.Top, w.Bottom - height));
            }
            else
            {
                x = Math.Min(Math.Max(posX, w.Left), Math.Max(w.Left, w.Right - width));
                y = Math.Min(Math.Max(posY, w.Top), Math.Max(w.Top, w.Bottom - height));
                posX = x;
                posY = y;
            }

            if (animate)
            {
                ShowDrawerAnimated(x, y, width, height, w.Right);
                return;
            }

            ApplyRoundedCorners(width, height);

            Native.SetWindowPos(steam, Native.HWND_TOPMOST, x, y, width, height,
                Native.SWP_NOACTIVATE | Native.SWP_ASYNCWINDOWPOS);
            Native.ShowWindowAsync(steam, Native.SW_SHOW);
            Native.SetWindowPos(steam, Native.HWND_NOTOPMOST, x, y, width, height,
                Native.SWP_NOACTIVATE | Native.SWP_ASYNCWINDOWPOS);

            setX = x;
            setY = y;
            hadSet = true;
            dragging = false;

            Activate(steam);

            shown = true;
            armed = false;
            otherSince = 0;
            enforceTick = 0;
            Log("показана " + width + "x" + height + " @ " + x + "," + y);
        }

        void ShowDrawerAnimated(int x, int y, int width, int height, int screenRight)
        {
            animFromX = screenRight;
            animToX = x;
            animY = y;
            animW = width;
            animH = height;
            animStart = Environment.TickCount;

            setX = x;
            setY = y;
            hadSet = true;
            dragging = false;

            shown = true;
            armed = false;
            otherSince = 0;
            enforceTick = 0;

            Native.SetWindowPos(steam, Native.HWND_TOPMOST, animFromX, y, width, height,
                Native.SWP_NOACTIVATE | Native.SWP_ASYNCWINDOWPOS);
            Native.ShowWindowAsync(steam, Native.SW_SHOW);
            Activate(steam);

            ApplyRoundedCorners(width, height);

            animating = true;
            animHostHwnd = steam;
            animThread = new Thread(AnimLoop);
            animThread.IsBackground = true;
            animThread.Start();
            Log("показана (слайд окна) " + width + "x" + height + " @ " + x + "," + y);
        }

        bool StartThumbnail(int y, int width, int height, int screenRight)
        {
            try
            {
                Native.SetWindowPos(steam, Native.HWND_TOPMOST, screenRight, y, width, height,
                    Native.SWP_NOACTIVATE | Native.SWP_ASYNCWINDOWPOS);
                Native.ShowWindowAsync(steam, Native.SW_SHOW);
                Activate(steam);

                var host = new ThumbHost();
                host.FormBorderStyle = FormBorderStyle.None;
                host.ShowInTaskbar = false;
                host.TopMost = true;
                host.BackColor = Color.Black;
                host.StartPosition = FormStartPosition.Manual;
                host.Bounds = new Rectangle(screenRight, y, width, height);
                host.Show();

                Bitmap shot = CaptureWindow(steam, width, height);
                if (shot != null)
                {
                    host.BackgroundImage = shot;
                    host.BackgroundImageLayout = ImageLayout.None;
                    animForm = host;
                    animBitmap = shot;
                    thumb = IntPtr.Zero;
                    return true;
                }

                IntPtr t;
                if (Native.DwmRegisterThumbnail(host.Handle, steam, out t) != 0 || t == IntPtr.Zero)
                {
                    host.Close();
                    host.Dispose();
                    return false;
                }

                var p = new Native.THUMBNAILPROPS();
                p.dwFlags = Native.DWM_TNP_RECTDESTINATION | Native.DWM_TNP_VISIBLE;
                p.rcDestination = new Native.RECT { Left = 0, Top = 0, Right = width, Bottom = height };
                p.fVisible = true;
                if (Native.DwmUpdateThumbnailProperties(t, ref p) != 0)
                {
                    Native.DwmUnregisterThumbnail(t);
                    host.Close();
                    host.Dispose();
                    return false;
                }

                animForm = host;
                thumb = t;
                return true;
            }
            catch
            {
                if (animForm != null)
                {
                    try { animForm.Close(); animForm.Dispose(); } catch { }
                    animForm = null;
                }
                return false;
            }
        }

        Bitmap CaptureWindow(IntPtr h, int width, int height)
        {
            if (h == IntPtr.Zero || width <= 0 || height <= 0) return null;
            Bitmap bmp = null;
            try
            {
                bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    IntPtr hdc = g.GetHdc();
                    bool ok;
                    try { ok = Native.PrintWindow(h, hdc, Native.PW_RENDERFULLCONTENT); }
                    finally { g.ReleaseHdc(hdc); }
                    if (!ok) { bmp.Dispose(); return null; }
                }
                if (!LooksVisible(bmp)) { bmp.Dispose(); return null; }
                return bmp;
            }
            catch
            {
                if (bmp != null) bmp.Dispose();
                return null;
            }
        }

        static bool LooksVisible(Bitmap bmp)
        {
            try
            {
                int w = bmp.Width, h = bmp.Height;
                Color a = bmp.GetPixel(w / 4, h / 4);
                Color b = bmp.GetPixel(w / 2, h / 2);
                Color c = bmp.GetPixel(w * 3 / 4, h * 3 / 4);
                bool allBlack = (a.R + a.G + a.B + b.R + b.G + b.B + c.R + c.G + c.B) == 0;
                return !allBlack;
            }
            catch { return true; }
        }

        void AnimLoop()
        {
            try
            {
                while (animating)
                {
                    long now = Environment.TickCount;
                    double t = (now - animStart) / (double)animMs;
                    if (t < 0) t = 0;
                    if (t > 1) t = 1;

                    double eased = animHide
                        ? t * t
                        : 1.0 - Math.Pow(1.0 - t, 5.0);
                    int x = animFromX + (int)Math.Round((animToX - animFromX) * eased);

                    IntPtr h = animHostHwnd;
                    if (h != IntPtr.Zero)
                        Native.SetWindowPos(h, IntPtr.Zero, x, animY, animW, animH,
                            Native.SWP_NOZORDER | Native.SWP_NOACTIVATE | Native.SWP_ASYNCWINDOWPOS);

                    if (t >= 1) break;
                    Thread.Sleep(6);
                }
            }
            catch { }

            animating = false;
            try { dispatcher.BeginInvoke(new Action(FinishAnimation)); }
            catch { }
        }

        void OnAnimTick(object sender, EventArgs e)
        {
            if (!animating) { animTimer.Stop(); return; }
            if (steam == IntPtr.Zero || !Native.IsWindow(steam)) { StopAnimation(); return; }

            long now = Environment.TickCount;
            double t = (now - animStart) / (double)animMs;
            if (t < 0) t = 0;
            if (t > 1) t = 1;

            double eased = 1.0 - Math.Pow(1.0 - t, 5.0);
            int x = animFromX + (int)Math.Round((animToX - animFromX) * eased);

            if (animForm != null)
            {
                try { animForm.Location = new Point(x, animY); } catch { }
                if (t >= 1) FinishAnimation();
                return;
            }

            Native.SetWindowPos(steam, IntPtr.Zero, x, animY, animW, animH,
                Native.SWP_NOZORDER | Native.SWP_NOACTIVATE | Native.SWP_ASYNCWINDOWPOS);

            if (t >= 1)
            {
                animating = false;
                animTimer.Stop();
                Native.SetWindowPos(steam, Native.HWND_NOTOPMOST, animToX, animY, animW, animH,
                    Native.SWP_NOACTIVATE | Native.SWP_ASYNCWINDOWPOS);
            }
        }

        void FinishAnimation()
        {
            animating = false;
            animTimer.Stop();

            if (thumb != IntPtr.Zero)
            {
                Native.DwmUnregisterThumbnail(thumb);
                thumb = IntPtr.Zero;
            }

            Form host = animForm;
            animForm = null;
            animHostHwnd = IntPtr.Zero;

            if (animBitmap != null)
            {
                try { host.BackgroundImage = null; } catch { }
                try { animBitmap.Dispose(); } catch { }
                animBitmap = null;
            }

            if (animHide)
            {
                animHide = false;
                hiding = false;
                shown = false;
                armed = false;
                otherSince = 0;
                lastHide = Environment.TickCount;
                dragging = false;

                if (steam != IntPtr.Zero && Native.IsWindow(steam))
                {
                    ClearRoundedCorners();
                    Native.SetWindowPos(steam, IntPtr.Zero, animToX, animY, animW, animH,
                        Native.SWP_NOZORDER | Native.SWP_NOACTIVATE | Native.SWP_ASYNCWINDOWPOS);
                    Native.ShowWindowAsync(steam, Native.SW_HIDE);
                }

                if (host != null)
                {
                    try { host.Hide(); host.Close(); host.Dispose(); }
                    catch { }
                }

                Log("скрыта (" + hideReason + ")");
                return;
            }

            Native.SetWindowPos(steam, Native.HWND_TOPMOST, animToX, animY, animW, animH,
                Native.SWP_NOACTIVATE | Native.SWP_ASYNCWINDOWPOS);
            Native.SetWindowPos(steam, Native.HWND_NOTOPMOST, animToX, animY, animW, animH,
                Native.SWP_NOACTIVATE | Native.SWP_ASYNCWINDOWPOS);

            if (host != null)
            {
                try { host.Hide(); host.Close(); host.Dispose(); }
                catch { }
            }
        }

        void StopAnimation()
        {
            if (!animating) return;
            animating = false;
            animTimer.Stop();
            animHostHwnd = IntPtr.Zero;
            animHide = false;
            hiding = false;

            if (thumb != IntPtr.Zero)
            {
                Native.DwmUnregisterThumbnail(thumb);
                thumb = IntPtr.Zero;
            }

            Form host = animForm;
            animForm = null;

            if (animBitmap != null)
            {
                if (host != null) { try { host.BackgroundImage = null; } catch { } }
                try { animBitmap.Dispose(); } catch { }
                animBitmap = null;
            }

            if (host != null)
            {
                try { host.Hide(); host.Close(); host.Dispose(); }
                catch { }
            }
        }

        void RememberPosition(bool log)
        {
            if (steam == IntPtr.Zero || !Native.IsWindow(steam))
                steam = Native.FindWindow(SteamClass, SteamTitle);
            if (steam == IntPtr.Zero || !Native.IsWindow(steam)) return;

            Native.RECT r;
            if (!Native.GetWindowRect(steam, out r)) return;

            posX = r.Left;
            posY = r.Top;
            setX = r.Left;
            setY = r.Top;
            hadSet = true;
            SaveSettings();
            if (log) Log("позиция запомнена: " + posX + "," + posY);
        }

        void AdoptUserPosition()
        {
            if (steam == IntPtr.Zero || !Native.IsWindow(steam)) return;
            Native.RECT r;
            if (!Native.GetWindowRect(steam, out r)) return;
            if (hadSet && Math.Abs(r.Left - setX) <= 2 && Math.Abs(r.Top - setY) <= 2) return;

            Rectangle w = WorkArea();
            if (r.Left > w.Right - 80 || r.Top > w.Bottom - 80 || r.Right < w.Left + 80) return;

            posX = r.Left;
            posY = r.Top;
            setX = r.Left;
            setY = r.Top;
            hadSet = true;
            bool wasDocked = docked;
            docked = false;
            if (dockMenuItem != null) dockMenuItem.Checked = false;
            SaveSettings();
            if (wasDocked) Log("шторка передвинута вручную, режим края выключен");
            else Log("позиция обновлена: " + posX + "," + posY);
        }

        void EnforcePosition()
        {
            if (steam == IntPtr.Zero || !Native.IsWindow(steam)) return;
            Native.RECT r;
            if (!Native.GetWindowRect(steam, out r)) return;

            Rectangle w = WorkArea();
            int width = r.Right - r.Left;
            int height = r.Bottom - r.Top;
            int x, y;
            if (docked)
            {
                x = w.Right - Math.Min(width, w.Width);
                y = Math.Max(w.Top, Math.Min(r.Top, w.Bottom - height));
            }
            else
            {
                x = Math.Min(Math.Max(posX, w.Left), Math.Max(w.Left, w.Right - width));
                y = Math.Min(Math.Max(posY, w.Top), Math.Max(w.Top, w.Bottom - height));
            }

            if (Math.Abs(r.Left - x) <= 2 && Math.Abs(r.Top - y) <= 2)
            {
                setX = r.Left;
                setY = r.Top;
                hadSet = true;
                return;
            }

            Native.SetWindowPos(steam, IntPtr.Zero, x, y, width, height,
                Native.SWP_NOZORDER | Native.SWP_NOACTIVATE | Native.SWP_ASYNCWINDOWPOS);
            setX = x;
            setY = y;
            hadSet = true;
        }

        static void Activate(IntPtr h)
        {
            IntPtr fg = Native.GetForegroundWindow();
            uint fgPid;
            uint fgThread = Native.GetWindowThreadProcessId(fg, out fgPid);
            uint cur = Native.GetCurrentThreadId();
            bool attached = fgThread != cur && Native.AttachThreadInput(cur, fgThread, true);
            Native.BringWindowToTop(h);
            Native.SetForegroundWindow(h);
            if (attached) Native.AttachThreadInput(cur, fgThread, false);
        }

        void HideDrawer(string why)
        {
            if (!shown || hiding) return;

            Native.RECT r;
            if (animate && steam != IntPtr.Zero && Native.IsWindow(steam) && Native.GetWindowRect(steam, out r))
            {
                Rectangle w = WorkArea();
                hiding = true;
                animHide = true;
                hideReason = why;
                animFromX = r.Left;
                animToX = w.Right;
                animY = r.Top;
                animW = Math.Max(1, r.Right - r.Left);
                animH = Math.Max(1, r.Bottom - r.Top);
                animStart = Environment.TickCount;
                animating = true;
                animHostHwnd = steam;
                animThread = new Thread(AnimLoop);
                animThread.IsBackground = true;
                animThread.Start();
                return;
            }

            HideNow(why);
        }

        void HideNow(string why)
        {
            if (!shown) return;
            StopAnimation();
            hiding = false;
            animHide = false;
            shown = false;
            armed = false;
            lastHide = Environment.TickCount;
            otherSince = 0;

            if (steam != IntPtr.Zero && Native.IsWindow(steam))
            {
                ClearRoundedCorners();
                Native.ShowWindowAsync(steam, Native.SW_HIDE);
                Native.RECT r;
                if (Native.GetWindowRect(steam, out r))
                    Native.SetWindowPos(steam, IntPtr.Zero, WorkArea().Right, r.Top,
                        r.Right - r.Left, r.Bottom - r.Top,
                        Native.SWP_NOZORDER | Native.SWP_NOACTIVATE | Native.SWP_ASYNCWINDOWPOS);
            }
            dragging = false;
            Log("скрыта (" + why + ")");
        }

        static bool IsAutostart()
        {
            using (Microsoft.Win32.RegistryKey k = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                return k != null && k.GetValue("SteamCurtain") != null;
        }

        static void SetAutostart(bool on)
        {
            using (Microsoft.Win32.RegistryKey k = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (k == null) return;
                if (on) k.SetValue("SteamCurtain", "\"" + Application.ExecutablePath + "\"");
                else if (k.GetValue("SteamCurtain") != null) k.DeleteValue("SteamCurtain");
            }
        }

        void LoadSettings()
        {
            try
            {
                foreach (string line in File.ReadAllLines(settingsPath))
                {
                    if (line.StartsWith("hideOnSwitch=")) hideOnSwitch = line.Substring(13) == "1";
                    else if (line.StartsWith("docked=")) docked = line.Substring(7) == "1";
                    else if (line.StartsWith("posX=")) int.TryParse(line.Substring(5), out posX);
                    else if (line.StartsWith("posY=")) int.TryParse(line.Substring(5), out posY);
                    else if (line.StartsWith("animate=")) animate = line.Substring(8) == "1";
                    else if (line.StartsWith("cornerSize=")) int.TryParse(line.Substring(11), out cornerSize);
                    else if (line.StartsWith("minSpeed=")) int.TryParse(line.Substring(9), out minSpeed);
                    else if (line.StartsWith("animMs=")) int.TryParse(line.Substring(7), out animMs);
                    else if (line.StartsWith("switchHideMs=")) int.TryParse(line.Substring(13), out switchHideMs);
                    else if (line.StartsWith("cornerTR=")) cornerTopRight = line.Substring(9) == "1";
                    else if (line.StartsWith("cornerBR=")) cornerBottomRight = line.Substring(9) == "1";
                    else if (line.StartsWith("cornerTL=")) cornerTopLeft = line.Substring(9) == "1";
                    else if (line.StartsWith("cornerBL=")) cornerBottomLeft = line.Substring(9) == "1";
                    else if (line.StartsWith("rounded=")) rounded = line.Substring(8) == "1";
                }
            }
            catch { }
        }

        void SaveSettings()
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("hideOnSwitch=" + (hideOnSwitch ? "1" : "0"));
                sb.AppendLine("docked=" + (docked ? "1" : "0"));
                sb.AppendLine("posX=" + posX);
                sb.AppendLine("posY=" + posY);
                sb.AppendLine("animate=" + (animate ? "1" : "0"));
                sb.AppendLine("cornerSize=" + cornerSize);
                sb.AppendLine("minSpeed=" + minSpeed);
                sb.AppendLine("animMs=" + animMs);
                sb.AppendLine("switchHideMs=" + switchHideMs);
                sb.AppendLine("cornerTR=" + (cornerTopRight ? "1" : "0"));
                sb.AppendLine("cornerBR=" + (cornerBottomRight ? "1" : "0"));
                sb.AppendLine("cornerTL=" + (cornerTopLeft ? "1" : "0"));
                sb.AppendLine("cornerBL=" + (cornerBottomLeft ? "1" : "0"));
                sb.AppendLine("rounded=" + (rounded ? "1" : "0"));
                File.WriteAllText(settingsPath, sb.ToString());
            }
            catch { }
        }

        void Log(string msg)
        {
            try
            {
                var fi = new FileInfo(logPath);
                if (fi.Exists && fi.Length > 200000) fi.Delete();
                File.AppendAllText(logPath, DateTime.Now.ToString("HH:mm:ss ") + msg + Environment.NewLine);
            }
            catch { }
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool created;
            using (var mutex = new System.Threading.Mutex(true, "SteamCurtainSingleInstance", out created))
            {
                if (!created) return;
                Application.EnableVisualStyles();
                Application.Run(new Curtain());
            }
        }
    }
}




