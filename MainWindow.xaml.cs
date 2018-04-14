using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
using System.IO;
using StartBgChanger.Helpers;
using Microsoft.Win32;
using __WinForm = System.Windows.Forms;

namespace StartBgChanger
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        List<string> fileList = new List<string>();
        List<Bitmap> iconList = new List<Bitmap>();
        private bool saveFlag = false;
        __WinForm.ColorDialog colorDialog = new __WinForm.ColorDialog();
        public MainWindow()
        {
            
            InitializeComponent();
            colorDialog.AllowFullOpen = true;
            colorDialog.AnyColor = true;
            colorDialog.FullOpen = true;
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
            fileList.Clear();
            appList.Items.Clear();

            fileList.AddRange(Helper.GetAllFilesByDir(App.StartMenu));
            fileList.AddRange(Helper.GetAllFilesByDir(App.CommonStartMenu));
            fileList.RemoveAll(str => !str.EndsWith(".lnk",StringComparison.CurrentCultureIgnoreCase));
            for (var i = 0; i < fileList.Count; i++)
            {
                var itemName = Path.GetFileNameWithoutExtension(fileList[i]);
                ListViewItem lvi = new ListViewItem();
                TextBlock txt = new TextBlock(){Text = itemName,FontSize = 14};
                lvi.Content = txt;
                appList.Items.Add(lvi);
            }

            //获取所有子目录内容
            //只监视.lnk文件
        }

        private void ButtonBase_OnClick_3(object sender, RoutedEventArgs e)
        {
            new AboutWindow().Show();
        }

        private void ButtonBase_OnClick_4(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", App.CommonStartMenu);
            Process.Start("explorer", App.StartMenu);
        }

        private void AppList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //保存。。
                if (saveFlag && MessageBox.Show("更改尚未保存，放弃更改？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                        MessageBoxResult.No) != MessageBoxResult.Yes) return;
            saveFlag = false;
            
        }


        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (colorSelector.SelectedItem == defineColorItem)
            {
                defineColor.Visibility = Visibility.Visible;
            }
            else
            {
                defineColor.Visibility = Visibility.Collapsed;
            }
        }

        private void DefineColor_OnClick(object sender, RoutedEventArgs e)
        {
            colorDialog.ShowDialog();
        }
    }
}
