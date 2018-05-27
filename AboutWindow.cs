using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SaberColorfulStartmenu.Properties;
using Application = System.Windows.Application;

namespace SaberColorfulStartmenu
{
    public partial class AboutWindow : Form
    {
        private static bool functionEnabled = false;
        public AboutWindow()
        {
            InitializeComponent();
            linkLabel5.Enabled = !functionEnabled;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AboutWindow_Load(object sender, EventArgs e)
        {
            textBox1.Text = Resources.About;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer", "http://hv0905.github.io/saber_startmenu_diyer/");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer", "http://hv0905.github.io/saber_startmenu_diyer/help.html");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer", "https://github.com/hv0905/SaberColorfulStartmenu/");
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer", "http://hv0905.github.io/donate.html");

        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (MessageBox.Show("这些功能尚未发布\n可能还未完善，启用后可能会发生很多错误\n继续？", "⚠警告", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                var mw = ((MainWindow) Application.Current.MainWindow);
                mw.defineLargeIconCheck.Opacity = mw.defineSmallIconCheck.Opacity = mw.defineLargeIconButton.Opacity = mw.defineSmallIconButton.Opacity= 1d;
                functionEnabled = true;
                linkLabel5.Enabled = false;
            }

        }
    }
}
