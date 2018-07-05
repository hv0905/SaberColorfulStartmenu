#define DISABLE_LOGOS


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SaberColorfulStartmenu
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static readonly string CommonStartMenu =
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
        public static readonly string StartMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
        public static Dictionary<string, string> charMap_Cn;

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) =>
            new ErrorReport(e.Exception).ShowDialog();

        protected override void OnStartup(StartupEventArgs e)
        {
            charMap_Cn = new Dictionary<string, string>();
            var lens = SaberColorfulStartmenu.Properties.Resources.SysCharMap_CN.Split(new[] {
                "\r\n"
            }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in lens) {
                var word = item.Split(',');
                charMap_Cn.Add(word[0],word[1]);
            }
            base.OnStartup(e);
        }
    }
}
