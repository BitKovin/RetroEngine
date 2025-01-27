using DarkUI.Collections;
using DarkUI.Config;
using DarkUI.Controls;
using DarkUI.Docking;
using DarkUI.Forms;
using DarkUI.Renderers;
using DarkUI.Win32;
using Microsoft.VisualBasic.Logging;
using System;
using System.Runtime.InteropServices;

namespace LoggerHost
{
    public partial class Form1 : DarkForm
    {

        TcpReceiver TcpReceiver;

        List<string> log = new List<string>();

        bool autoScroll = true;

        public Form1()
        {
            InitializeComponent();


            TcpReceiver = new TcpReceiver(2004);
            TcpReceiver.OnMessageReceived += TcpReceiver_OnMessageReceived;
            TcpReceiver.Start();

            Application.AddMessageFilter(new ControlScrollFilter());

            UseImmersiveDarkMode(this.Handle, true);

            darkCheckBox1.Checked = autoScroll;

        }


        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Environment.Exit(0);

        }

        private void TcpReceiver_OnMessageReceived(string message)
        {

            log.Add(message);

            darkListView1.Items.Add(new DarkListItem(message));

            if (autoScroll)
                darkListView1.VScrollTo(int.MaxValue);
        }

        private void darkListView1_Click(object sender, EventArgs e)
        {

        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {
            if (IsWindows10OrGreater(17763))
            {
                var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = enabled ? 1 : 0;
                return DwmSetWindowAttribute(handle, (int)attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }

            return false;
        }

        private static bool IsWindows10OrGreater(int build = -1)
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }

        private void darkCheckBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            autoScroll = darkCheckBox1.Checked;
        }

        private void darkListView1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                var selected = darkListView1.SelectedIndices;
                if (selected.Count > 0)
                {
                    Clipboard.SetText(darkListView1.Items[selected[0]].Text);
                }
            }
            catch (Exception ex) { }

        }
    }
}
