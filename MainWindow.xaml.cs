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
//using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Windows.Media;
using IWshRuntimeLibrary;
using StartBgChanger.Helpers;
using Microsoft.Win32;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using File = System.IO.File;
using __WinForm = System.Windows.Forms;

namespace StartBgChanger
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private List<string> _fileList = new List<string>();
        private List<Bitmap> _iconList = new List<Bitmap>();
        private bool _saveFlag;
        private bool _loaded;
        private Color _nowColor = Colors.Black;
        private string _nowColorString;
        private int _nowWorkingId;
        private bool _sysChangeing;
        private __WinForm.ColorDialog _colorDialog;
        private OpenFileDialog _openFile;


        public MainWindow()
        {
            _colorDialog = new __WinForm.ColorDialog();
            _openFile = new OpenFileDialog();

            InitializeComponent();
            _colorDialog.AllowFullOpen = true;
            _colorDialog.AnyColor = true;
            _colorDialog.FullOpen = true;
            _openFile.AddExtension = true;
            _openFile.Filter = "图像文件|*.png;*.jpg;*.jpeg;*.bmp";
            RefreshList();
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) => Close();

        private void ButtonBase_OnClick_1(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void ButtonBase_OnClick_2(object sender, RoutedEventArgs e)
        {
            SaveCheck();
            RefreshList();
        }

        private bool SaveCheck()
        {
            if (_saveFlag)
            {
                if(MessageBox.Show("更改尚未保存，放弃更改？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning,MessageBoxResult.No) != MessageBoxResult.Yes)
                {
                    return false;
                }

                _saveFlag = false;
                return true;
            }

            return true;
        }


        private void RefreshList()
        {
            _fileList.Clear();
            appList.Items.Clear();

            _fileList.AddRange(Helper.GetAllFilesByDir(App.StartMenu));
            _fileList.AddRange(Helper.GetAllFilesByDir(App.CommonStartMenu));
            _fileList.RemoveAll(str => !str.EndsWith(".lnk", StringComparison.CurrentCultureIgnoreCase));
            for (var i = 0; i < _fileList.Count; i++)
            {
                WshShortcut shortcut = Helper.mainShell.CreateShortcut(_fileList[i]);
                Debug.WriteLine(shortcut.TargetPath);
                if (!shortcut.TargetPath.EndsWith(".exe",StringComparison.CurrentCultureIgnoreCase))
                {
                    Debug.WriteLine("Torow!!!");
                    _fileList.RemoveAt(i);
                    i--;
                    continue;
                }
                string iconPath;
                int iconId;
                if (shortcut.IconLocation.Trim().StartsWith(","))
                {//targetpath对应icon
                    iconId = int.Parse(shortcut.IconLocation.Substring(1));
                    iconPath = shortcut.TargetPath;
                }
                else
                {
                    string[] tmp = shortcut.IconLocation.Split(',');
                    
                    iconId = int.Parse(tmp[tmp.Length - 1]);
                    iconPath = shortcut.IconLocation.Replace($",{iconId}",string.Empty);
                }
                Debug.WriteLine($"icon id:{iconId};icon path:{iconPath}");
                var itemName = Path.GetFileNameWithoutExtension(_fileList[i]);
                var lvi = new ListViewItem();
                var txt = new TextBlock() { Text = itemName, FontSize = 14 };
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
            if (appList.SelectedIndex == _nowWorkingId) return;
            if (appList.SelectedIndex == -1)
            {
                gridSetMain.Visibility = Visibility.Collapsed;
                return;
            }
            gridSetMain.Visibility = Visibility.Visible;
            //保存。。
            if (!SaveCheck())
            {
                appList.SelectedIndex = _nowWorkingId;
                return;
            }
            _nowWorkingId = appList.SelectedIndex;
            Load();
        }

        private void Load()
        {
            _sysChangeing = true;

            //todo: 载入
            

            _sysChangeing = false;
        }


        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loaded) return;
            _saveFlag = true;
            // ReSharper disable once PossibleUnintendedReferenceComparison
            group_Color.Visibility = colorSelector.SelectedItem == defineColorItem ? Visibility.Visible : Visibility.Collapsed;
            switch (colorSelector.SelectedIndex)
            {
                case 0:
                    _nowColorString = "black";
                    _nowColor = Colors.Black;
                    break;
                case 1:
                    _nowColorString = "silver";
                    _nowColor = Colors.Silver;
                    break;
                case 2:
                    _nowColorString = "gray";
                    _nowColor = Colors.Gray;
                    break;
                case 3:
                    _nowColorString = "white";
                    _nowColor = Colors.White;
                    break;
                case 4:
                    _nowColorString = "maroon";
                    _nowColor = Colors.Maroon;
                    break;
                case 5:
                    _nowColorString = "red";
                    _nowColor = Colors.Red;
                    break;
                case 6:
                    _nowColorString = "purple";
                    _nowColor = Colors.Purple;
                    break;
                case 7:
                    _nowColorString = "fuchsia";
                    _nowColor = Colors.Fuchsia;
                    break;
                case 8:
                    _nowColorString = "green";
                    _nowColor = Colors.Green;
                    break;
                case 9:
                    _nowColorString = "lime";
                    _nowColor = Colors.Lime;
                    break;
                case 10:
                    _nowColorString = "olive";
                    _nowColor = Colors.Olive;
                    break;
                case 11:
                    _nowColorString = "yellow";
                    _nowColor = Colors.Yellow;
                    break;
                case 12:
                    _nowColorString = "navy";
                    _nowColor = Colors.Navy;
                    break;
                case 13:
                    _nowColorString = "blue";
                    _nowColor = Colors.Blue;
                    break;
                case 14:
                    _nowColorString = "teal";
                    _nowColor = Colors.Teal;
                    break;
                case 15:
                    _nowColorString = "aqua";
                    _nowColor = Colors.Aqua;
                    break;
                default:
                    _nowColor = Colors.Black;
                    _nowColorString = "black";
                    break;
            }
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            previewColor.Color = _nowColor;
            preview_LargeText.Foreground = (txtColorSelector.SelectedIndex == 0) ? Brushes.White : Brushes.Black;
            preview_LargeText.Visibility = largeAppNameCheck.IsChecked.Value ? Visibility.Visible : Visibility.Hidden;
        }

        private void Selector_OnSelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (!_loaded) return;
            if(!_sysChangeing) _saveFlag = true;
            if (modeSelctor.SelectedIndex == 0)
            {
                colorSelector.Visibility = Visibility.Collapsed;
                group_Color.Visibility = Visibility.Collapsed;
            }
            else
            {
                colorSelector.Visibility = Visibility.Visible;
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (colorSelector.SelectedItem == defineColorItem)
                {
                    group_Color.Visibility = Visibility.Visible;
                }
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _loaded = true;
            gridSetMain.Visibility = Visibility.Hidden;
            _sysChangeing = true;
            Selector_OnSelectionChanged_1(sender, null);
            _sysChangeing = false;
        }

        private void ButtonBase_OnClick_5(object sender, RoutedEventArgs e)
        {
            if (_colorDialog.ShowDialog() == __WinForm.DialogResult.OK)
            {
                _saveFlag = true;
                _nowColor = _colorDialog.Color.ToMediaColor();
                defineColorPreview.Fill = new SolidColorBrush(_nowColor);
                _nowColorString = "#" + _nowColor.ToString().Substring(3);
                defineColorText.Text = _nowColorString;
                UpdatePreview();
            }
        }

        private void DefineColorText_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (defineColorText.Text.Length != 7 || !defineColorText.Text.StartsWith("#")) return;
            _saveFlag = true;
            try
            {
                // ReSharper disable once PossibleNullReferenceException
                _nowColor = (Color)ColorConverter.ConvertFromString($"#FF{defineColorText.Text.Substring(1)}");
                defineColorPreview.Fill = new SolidColorBrush(_nowColor);
                UpdatePreview();
            }
            catch
            {
                // ignored
            }
        }

        private void TxtColorSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loaded) return;
            _saveFlag = true;
            UpdatePreview();
        }

        private void LargeAppNameCheck_OnChecked(object sender, RoutedEventArgs e)
        {
            if (!_loaded) return;
            _saveFlag = true;
            UpdatePreview();
        }

        private void ButtonBase_OnClick_7(object sender, RoutedEventArgs e)
        {
            _openFile.Title = "选择150x150大图标 建议比例：1：1";

            if (_openFile.ShowDialog() ?? false)
            {

            }
        }

        private void ButtonBase_OnClick_6(object sender, RoutedEventArgs e)
        {
            //todo save

        }
    }
}