#define DISABLE_LOGOS

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using ModernMessageBoxLib;

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

        static App() => AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
        {
            var assName = new AssemblyName(e.Name);

            switch (assName.FullName.Replace(" ", string.Empty))
            {
                case "ModernMessageBoxLib,Version=1.3.0.0,Culture=neutral,PublicKeyToken=null":
                    return Assembly.Load(SaberColorfulStartmenu.Properties.Resources.ModernMessageBoxLib);
                default:
                    return null;
            }

        };

        protected override void OnStartup(StartupEventArgs e)
        {
            charMap_Cn = new Dictionary<string, string>();
            var lens = SaberColorfulStartmenu.Properties.Resources.SysCharMap_CN.Replace("\r\n","\n").Split(new[] {
                "\n"
            }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in lens) {
                var word = item.Split(',');
                charMap_Cn.Add(word[0],word[1]);
            }

            QModernMessageBox.MainLang = new QMetroMessageLang() {
                Abort = "中止(A)",
                Cancel = "取消(C)",
                Ignore = "忽略(I)",
                No = "否(N)",
                Ok = "确定",
                Retry = "重试(R)",
                Yes = "是(Y)"
            };
            base.OnStartup(e);
            
        }
    }
}
