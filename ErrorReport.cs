using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.Diagnostics;
using System.Reflection;
using System.Net.Mail;
using StartBgChanger;

namespace StartBgChanger
{
    public partial class ErrorReport : Form
    {
        public ErrorReport(Exception e)
        {
            InitializeComponent();
            textBox1.Text = $"Message:\r\n{e.Message}\r\nTargetSite:\r\n{e.TargetSite}\r\nINFO:\r\n{e}\r\n";
        }

        private void ErrorReport_Load(object sender, EventArgs e)
        {
            SystemSounds.Hand.Play();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
            Application.ExitThread();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox1.Text);
            MessageBox.Show("已复制", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }
    }
}
