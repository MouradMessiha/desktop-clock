using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

namespace DesktopClock
{
    public partial class frmMain : Form
    {
        private Thread mobjDisplayTimeThread;
        private bool mblnFormClosed = false;
        private IntPtr mobjdeskDC;

        [DllImport("user32.dll", EntryPoint = "GetDC")]
        public static extern IntPtr GetDC(IntPtr ptr);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr hDC);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RedrawWindow(IntPtr hWnd, ref RECT lprcUpdate, IntPtr hrgnUpdate, RedrawWindowFlags flags);

        public enum RedrawWindowFlags
        {
            RDW_INVALIDATE = 0x0001,
            RDW_NOERASE = 0x0020,
            RDW_ERASE = 0x0004,
            RDW_UPDATENOW = 0x0100
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
        }

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            mobjdeskDC = IntPtr.Zero;
            IntPtr hProgMan = FindWindow("ProgMan", null);
            if (!hProgMan.Equals(IntPtr.Zero))
            {
                IntPtr hShellDefView = FindWindowEx(hProgMan, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (!hShellDefView.Equals(IntPtr.Zero))
                    mobjdeskDC = FindWindowEx(hShellDefView, IntPtr.Zero, "SysListView32", null);
            }

            mobjDisplayTimeThread = new Thread(StartDisplayTimeThread);
            mobjDisplayTimeThread.Start();
            Hide();
        }

        private void StartDisplayTimeThread()
        {
            Font objFont;
            IntPtr hDC;
            Graphics objGraphics;

            objFont = new Font("Arial Black", 24, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            RECT rc;
            rc.left = 0;
            rc.top = 0;
            rc.right = 250;
            rc.bottom = 50;

            while (!mblnFormClosed)
            {
                hDC = GetDC(mobjdeskDC);
                objGraphics = Graphics.FromHdc(hDC);
                RedrawWindow(mobjdeskDC, ref rc, IntPtr.Zero, RedrawWindowFlags.RDW_INVALIDATE | RedrawWindowFlags.RDW_ERASE | RedrawWindowFlags.RDW_UPDATENOW);
                objGraphics.DrawString(string.Format("{0:h:mm:ss tt}", DateTime.Now), objFont, Brushes.Red, 0, 0);
                ReleaseDC(mobjdeskDC, hDC);
                Thread.Sleep(500);
            }
            
            hDC = GetDC(mobjdeskDC);

            RedrawWindow(mobjdeskDC, ref rc, IntPtr.Zero, RedrawWindowFlags.RDW_INVALIDATE | RedrawWindowFlags.RDW_ERASE | RedrawWindowFlags.RDW_UPDATENOW);
            ReleaseDC(mobjdeskDC, hDC);
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            mblnFormClosed = true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void objNotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Right:
                    Show();
                    this.WindowState = FormWindowState.Normal;
                    break;
            }
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }
    }
}
