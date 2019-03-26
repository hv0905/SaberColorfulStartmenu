using System;
using System.Diagnostics;
using System.Windows.Forms;
using SaberColorfulStartmenu.Properties;

namespace SaberColorfulStartmenu
{
    public partial class AboutWindow : Form
    {

        public AboutWindow() => InitializeComponent();

        private void button1_Click(object sender, EventArgs e) => Close();

        private void AboutWindow_Load(object sender, EventArgs e) => textBox1.Text = Resources.About.Replace("\n", "\r\n");

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Process.Start("explorer", "https://edgeneko.github.io/2019/01/31/SaberColorfulStartmenu/");

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Process.Start("explorer", "https://github.com/hv0905/SaberColorfulStartmenu/");

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Process.Start("explorer", "https://edgeneko.github.io/2019/01/31/SaberColorfulStartmenu/#%E6%94%AF%E6%8C%81-Support");

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => MessageBox.Show("This release have not any preview functions.\n Enjoy~","BETA Notice");
    }
}
