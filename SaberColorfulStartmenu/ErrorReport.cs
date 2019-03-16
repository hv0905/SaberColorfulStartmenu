using System;
using System.Media;
using System.Windows.Forms;


namespace SaberColorfulStartmenu
{
    public partial class ErrorReport : Form
    {
        public ErrorReport(Exception e)
        {
            InitializeComponent();
            // ReSharper disable once LocalizableElement
            textBox1.Text = $"Message:\r\n{e.Message}\r\nTargetSite:\r\n{e.TargetSite}\r\nINFO:\r\n{e}\r\n";
        }

        private void ErrorReport_Load(object sender, EventArgs e) => SystemSounds.Hand.Play();

        private void button6_Click(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown(-1);
            Environment.Exit(-1);
        }

        private void button5_Click(object sender, EventArgs e) => Close();

        private void button4_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox1.Text);
            MessageBox.Show("已复制", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }
    }
}
