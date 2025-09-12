using System.Runtime.InteropServices;

namespace MetaFrm.RemoteDesktop.Control
{
    /// <summary>
    /// Dpi
    /// </summary>
    public static partial class Dpi
    {
        [LibraryImport("Shcore.dll")]
        private static partial int SetProcessDpiAwareness(int value);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetProcessDPIAware();

        /// <summary>
        /// EnableDpiAwareness
        /// </summary>
        public static void EnableDpiAwareness()
        {
            try
            {
                // Windows 8.1 이상
                int v = SetProcessDpiAwareness((int)ProcessDpiAwareness.Process_Per_Monitor_DPI_Aware);
            }
            catch
            {
                // Windows 7 fallback
                SetProcessDPIAware();
            }
        }


        // Win32 API
        [LibraryImport("Shcore.dll")]
        private static partial int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

        [LibraryImport("User32.dll")]
        private static partial IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT(int x, int y)
        {
            public int X = x;
            public int Y = y;
        }

        const int MDT_EFFECTIVE_DPI = 0;// scaling 반영된 최종 DPI
        const uint MONITOR_DEFAULTTONEAREST = 2;// 모니터 없으면 가장 가까운 모니터 반환

        /// <summary>
        /// GetTotalScreenBounds
        /// </summary>
        /// <returns></returns>
        public static Rectangle GetTotalScreenBounds()
        {
            Rectangle totalBounds = Rectangle.Empty;
            foreach (var screen in Screen.AllScreens)
            {
                // 모니터 안쪽 좌표 하나 잡기
                var pt = new POINT(screen.Bounds.Left + 1, screen.Bounds.Top + 1);

                // 모니터 핸들 얻기
                IntPtr hMonitor = MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);

                // DPI 얻기
                GetDpiForMonitor(hMonitor, MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY);

                // Scaling factor (96 DPI = 100%)
                float scale = dpiX / 96.0f;

                // 실제 픽셀 해상도 (DPI scaling 반영)
                int realWidth = (int)(screen.Bounds.Width * scale);
                int realHeight = (int)(screen.Bounds.Height * scale);


                totalBounds = Rectangle.Union(totalBounds, new Rectangle(screen.Bounds.X, screen.Bounds.Y, realWidth, realHeight));
            }

            return totalBounds;
        }
    }
}