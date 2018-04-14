using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StartBgChanger.Helpers;

namespace StartBgChanger
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            RefreshList();
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) => Close();

        private void ButtonBase_OnClick_1(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void ButtonBase_OnClick_2(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }


        private void RefreshList()
        {
            appList.Items.Clear();
            List<string> fileList = new List<string>();
            fileList.AddRange(Helper.GetAllFilesByDir(App.StartMenu));
            fileList.AddRange(Helper.GetAllFilesByDir(App.CommonStartMenu));
            fileList.RemoveAll(str => !str.EndsWith(".lnk",StringComparison.CurrentCultureIgnoreCase));
            foreach (var item in fileList)
            {
                appList.Items.Add(item);
            }
            //获取所有子目录内容
            //只监视.lnk文件
        }
    }
}
