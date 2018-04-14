using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StartBgChanger
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static readonly string CommonStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
        public static readonly string StartMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }
}
