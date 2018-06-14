using SaberColorfulStartmenu.Core;
using SaberColorfulStartmenu.Helpers;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Animation;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using __WinForm = System.Windows.Forms;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using File = System.IO.File;
using Image = System.Windows.Controls.Image;

namespace SaberColorfulStartmenu
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        #region Fields

        private List<string> _fileList = new List<string>();
        private List<Bitmap> _iconList = new List<Bitmap>();
        private bool _saveFlag, _loaded, _sysChangeing;
        private Color _currentColor = Colors.Black;
        private string _currentColorString;
        private int _currentId = -1;
        private __WinForm.ColorDialog _colorDialog;
        private OpenFileDialog _openFile;
        //private Bitmap _largeIcon, _smallIcon;
        private Bitmap _icon;
        private StartmenuShortcutInfo _currectInfo;
        //private string _newLargeIconLoc, _newSmallIconLoc;
        private string _newIconLoc;

        #endregion

        #region Events

        public MainWindow()
        {
            _colorDialog = new __WinForm.ColorDialog();
            _openFile = new OpenFileDialog();

            InitializeComponent();
            _colorDialog.AllowFullOpen = true;
            _colorDialog.AnyColor = true;
            _colorDialog.FullOpen = true;
            _openFile.AddExtension = true;
            _openFile.Filter = "图像文件|*.png;*.jpg;*.jpeg;*.gif";
            RefreshList();
        }

        private void CloseBtn_OnClick(object sender, RoutedEventArgs e) => Close();

        private void MinBtn_OnClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void Main_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            SaveCheck();
            appList.Items.Clear();
            RefreshList();

        }

        private void About_OnClick(object sender, RoutedEventArgs e) => new AboutWindow().Show();

        private void ButtonBase_OnClick_4(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", App.CommonStartMenu);
            Process.Start("explorer", App.StartMenu);
        }

        private void AppList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (appList.SelectedIndex == _currentId) return;
            if (appList.SelectedIndex == -1)
            {
                gridSetMain.Visibility = Visibility.Collapsed;
                return;
            }
            gridSetMain.Visibility = Visibility.Visible;
            //保存。。
            if (!SaveCheck())
            {
                appList.SelectedIndex = _currentId;
                return;
            }
            var first = _currentId == -1;
            _currentId = appList.SelectedIndex;
            if (first)
            {
                ChangeStory_OnCompleted(null, null);
            }
            else
            {
                var csb = (Storyboard)Resources["ChangeStory_1"];
                csb.Begin();
            }
            //Load();
        }

        private void Selector_OnSelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (!_loaded) return;
            if (!_sysChangeing) _saveFlag = true;
            if (modeSelctor.SelectedIndex == 0)
            {
                colorSelector.Visibility = Visibility.Collapsed;
                group_Color.Visibility = Visibility.Collapsed;
            }
            else
            {
                colorSelector.Visibility = Visibility.Visible;
                // ReSharper disable once PossibleUnintendedReferenceComparison
                //                if (colorSelector.SelectedItem == defineColorItem)
                //                {
                //                    group_Color.Visibility = Visibility.Visible;
                //                }
                group_Color.Visibility = colorSelector_17.IsChecked ?? false ? Visibility.Visible : Visibility.Collapsed;
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
            if (_colorDialog.ShowDialog() != __WinForm.DialogResult.OK) return;
            _saveFlag = true;
            _currentColor = _colorDialog.Color.ToMediaColor();
            _currentColorString = _currentColor.ToRgbString();
            defineColorText.Text = _currentColorString;
            UpdatePreview();
        }

        private void DefineColorText_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (_sysChangeing) return;
            if (defineColorText.Text.Length != 7 || !defineColorText.Text.StartsWith("#")) return;
            _saveFlag = true;
            try
            {
                // ReSharper disable once PossibleNullReferenceException
                _currentColor = Helper.GetColorFromRgbString(defineColorText.Text);
                _currentColorString = defineColorText.Text;
                UpdatePreview();
            }
            catch (FormatException)
            {
                Debug.WriteLine("ChangeColor Canceled.");
                // ignored
            }
        }

        private void TxtColorSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loaded || _sysChangeing) return;
            _saveFlag = true;
            UpdatePreview();
        }

        private void LargeAppNameCheck_OnChecked(object sender, RoutedEventArgs e)
        {
            if (!_loaded || _sysChangeing) return;
            _saveFlag = true;
            UpdatePreview();
        }

        private void ButtonBase_OnClick_7(object sender, RoutedEventArgs e)
        {
            _openFile.Title = "选择150x150大图标 建议比例：1：1";
            if (!(_openFile.ShowDialog() ?? false)) return;
            try
            {
                var fs = File.Open(_openFile.FileName, FileMode.Open, FileAccess.Read);
                _saveFlag = true;
                _newIconLoc = _openFile.FileName;
                _icon = new Bitmap(fs);
                UpdatePreview();
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"图片载入失败\n未知错误，无法读取此文件\n详细信息：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void ButtonBase_OnClick_6(object sender, RoutedEventArgs e)
        {
            if (Save())
                ((Storyboard)Resources["saveDoneStory"]).Begin();
        }

        //        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        //        {
        //            if (!_loaded) return;
        //            if (!_sysChangeing) _saveFlag = true;
        //            // ReSharper disable once PossibleUnintendedReferenceComparison
        //            group_Color.Visibility = colorSelector.SelectedItem == defineColorItem ? Visibility.Visible : Visibility.Collapsed;
        //            switch (colorSelector.SelectedIndex)
        //            {
        //                case 0://固定色区
        //                    _currentColorString = "black";
        //                    _currentColor = Colors.Black;
        //                    break;
        //                case 1:
        //                    _currentColorString = "silver";
        //                    _currentColor = Colors.Silver;
        //                    break;
        //                case 2:
        //                    _currentColorString = "gray";
        //                    _currentColor = Colors.Gray;
        //                    break;
        //                case 3:
        //                    _currentColorString = "white";
        //                    _currentColor = Colors.White;
        //                    break;
        //                case 4:
        //                    _currentColorString = "maroon";
        //                    _currentColor = Colors.Maroon;
        //                    break;
        //                case 5:
        //                    _currentColorString = "red";
        //                    _currentColor = Colors.Red;
        //                    break;
        //                case 6:
        //                    _currentColorString = "purple";
        //                    _currentColor = Colors.Purple;
        //                    break;
        //                case 7:
        //                    _currentColorString = "fuchsia";
        //                    _currentColor = Colors.Fuchsia;
        //                    break;
        //                case 8:
        //                    _currentColorString = "green";
        //                    _currentColor = Colors.Green;
        //                    break;
        //                case 9:
        //                    _currentColorString = "lime";
        //                    _currentColor = Colors.Lime;
        //                    break;
        //                case 10:
        //                    _currentColorString = "olive";
        //                    _currentColor = Colors.Olive;
        //                    break;
        //                case 11:
        //                    _currentColorString = "yellow";
        //                    _currentColor = Colors.Yellow;
        //                    break;
        //                case 12:
        //                    _currentColorString = "navy";
        //                    _currentColor = Colors.Navy;
        //                    break;
        //                case 13:
        //                    _currentColorString = "blue";
        //                    _currentColor = Colors.Blue;
        //                    break;
        //                case 14:
        //                    _currentColorString = "teal";
        //                    _currentColor = Colors.Teal;
        //                    break;
        //                case 15:
        //                    _currentColorString = "aqua";
        //                    _currentColor = Colors.Aqua;
        //                    break;
        //                case 16://自定义
        //                    try
        //                    {
        //                        if (!_sysChangeing)
        //                        {
        //                            _currentColor = Helper.GetColorFromRgbString(defineColorText.Text);
        //                            _currentColorString = defineColorText.Text;
        //                        }
        //                    }
        //                    catch (FormatException)
        //                    {
        //                        defineColorText.Text = "#000000";
        //                        _currentColorString = "black";
        //                        _currentColor = Colors.Black;
        //                    }
        //
        //                    break;
        //            }
        //            if (!_sysChangeing)
        //                UpdatePreview();
        //        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e) =>
            e.Cancel = !SaveCheck();

        private void ChangeStory_OnCompleted(object sender, EventArgs e)
        {
            Load();
            var sb = (Storyboard)Resources["ChangeStory_2"];
            sb.Begin();
        }

        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
        }

        private void ColorSelector_OnChecked(object sender, RoutedEventArgs e)
        {
            // ReSharper disable PossibleUnintendedReferenceComparison
            if (!_loaded) return;
            group_Color.Visibility = colorSelector_17.IsChecked ?? false ? Visibility.Visible : Visibility.Collapsed;
            if (sender == colorSelector_1)
            {
                _currentColorString = "black";
                _currentColor = Colors.Black;
            }
            else if (sender == colorSelector_2)
            {
                _currentColorString = "silver";
                _currentColor = Colors.Silver;
            }
            else if (sender == colorSelector_3)
            {
                _currentColorString = "gray";
                _currentColor = Colors.Gray;
            }
            else if (sender == colorSelector_4)
            {
                _currentColorString = "white";
                _currentColor = Colors.White;
            }
            else if (sender == colorSelector_5)
            {
                _currentColorString = "maroon";
                _currentColor = Colors.Maroon;
            }
            else if (sender == colorSelector_6)
            {
                _currentColorString = "red";
                _currentColor = Colors.Red;
            }
            else if (sender == colorSelector_7)
            {
                _currentColorString = "purple";
                _currentColor = Colors.Purple;
            }
            else if (sender == colorSelector_8)
            {
                _currentColorString = "fuchsia";
                _currentColor = Colors.Fuchsia;
            }
            else if (sender == colorSelector_9)
            {
                _currentColorString = "green";
                _currentColor = Colors.Green;
            }
            else if (sender == colorSelector_10)
            {
                _currentColorString = "lime";
                _currentColor = Colors.Lime;
            }
            else if (sender == colorSelector_11)
            {
                _currentColorString = "olive";
                _currentColor = Colors.Olive;
            }
            else if (sender == colorSelector_12)
            {
                _currentColorString = "yellow";
                _currentColor = Colors.Yellow;
            }
            else if (sender == colorSelector_13)
            {
                _currentColorString = "navy";
                _currentColor = Colors.Navy;
            }
            else if (sender == colorSelector_14)
            {
                _currentColorString = "blue";
                _currentColor = Colors.Blue;
            }
            else if (sender == colorSelector_15)
            {
                _currentColorString = "teal";
                _currentColor = Colors.Teal;
            }
            else if (sender == colorSelector_16)
            {
                _currentColorString = "aqua";
                _currentColor = Colors.Aqua;
            }
            else if (!_sysChangeing && sender == colorSelector_17)
            {//自定义
                try
                {
                    _currentColor = Helper.GetColorFromRgbString(defineColorText.Text);
                    _currentColorString = defineColorText.Text;
                }
                catch (FormatException)
                {
                    defineColorText.Text = "#000000";
                    _currentColorString = "black";
                    _currentColor = Colors.Black;
                }
            }
            if (!_sysChangeing) UpdatePreview();
            // ReSharper restore PossibleUnintendedReferenceComparison
        }

        #endregion

        #region Functions

        private void RefreshList()
        {
            _fileList.Clear();
            appList.Items.Clear();
            //_iconList.Clear();
            //GC.Collect();

            _fileList.AddRange(Helper.GetAllFilesByDir(App.StartMenu));
            _fileList.AddRange(Helper.GetAllFilesByDir(App.CommonStartMenu));
            _fileList.RemoveAll(str => !str.EndsWith(".lnk", StringComparison.CurrentCultureIgnoreCase));
            for (var i = 0; i < _fileList.Count; i++)
            {
                WshShortcut shortcut = Helper.MainShell.CreateShortcut(_fileList[i]);
                var target = Helper.ConvertEnviromentArgsInPath(shortcut.TargetPath);
                __tf:
                Debug.WriteLine(target);
                if ((!target.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase)) || !File.Exists(target))
                {
                    if (target.ToLower().Contains("program files (x86)"))
                    {
                        //Reason
                        //实测有部分应用（这包括Microsoft Office） 的快捷方式在使用任何一种Wshshell（这包括C# 的WshShortcut和C++的shlobj.h）获取TargetPath时
                        //Program Files 都有几率变为 Program Files (x86) 暂时不了解原因，网上也没有相关的错误报告
                        //msdn居然对IWshShell一个文档都没有= = 
                        //这种临时的解决方式，只能算是一种下下策了吧 =。=
                        //如果有知道解决方案的可以当issue汇报
                        //阿里嘎多

                        target = target.ToLower().Replace("program files (x86)", "program files");
                        goto __tf;
                    }
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
                    iconPath = target;
                }
                else
                {
                    var tmp = shortcut.IconLocation.Split(',');

                    iconId = int.Parse(tmp[tmp.Length - 1]);
                    iconPath = Helper.ConvertEnviromentArgsInPath(shortcut.IconLocation.Replace($",{iconId}", string.Empty));
                }

                if (iconPath.EndsWith(".exe") || iconPath.EndsWith(".dll"))
                {
                    try
                    {
                        var icons = Helper.GetLargeIconsFromExeFile(iconPath);
                        _iconList.Add(icons[iconId].ToBitmap());

                        foreach (var item in icons)
                        {
                            Helper.DestroyIcon(item.Handle);
                            item.Dispose();
                        }
                    }
                    catch
                    {
                        _iconList.Add(Properties.Resources.unknown);
                    }

                }
                else
                {//ico
                    try
                    {
                        var ico = new Icon(iconPath);
                        _iconList.Add(ico.ToBitmap());
                        ico.Dispose();
                    }
                    catch
                    {
                        _iconList.Add(Properties.Resources.unknown);
                    }
                }
                Debug.WriteLine($"icon id:{iconId};icon path:{iconPath}");
                var itemName = Path.GetFileNameWithoutExtension(_fileList[i]);
                var lvi = new ListViewItem();
                var sp = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Height = 25
                };
                sp.Children.Add(new Image()
                {
                    Source = _iconList[i].GetBitmapSourceFromBitmap()
                });
                sp.Children.Add(new TextBlock() { Text = itemName, FontSize = 14 });
                lvi.Content = sp;
                appList.Items.Add(lvi);
            }

            //获取所有子目录内容
            //只监视.lnk文件
        }

        private void Load()
        {
            _sysChangeing = true;

            //todo: 载入
            _currectInfo = new StartmenuShortcutInfo(_fileList[_currentId]);
            if (_currectInfo.XmlFile == null)
            {
                modeSelctor.SelectedIndex = 0;
                //set everything to empty
                _currentColor = Colors.Black;
                _currentColorString = "black";
                _icon = null;
                defineIconCheck.IsChecked = false;
                txtColorSelector.SelectedIndex = 0;
                //colorSelector.SelectedIndex = 0;
                colorSelector_1.IsChecked = true;

            }
            else
            {
                try
                {
                    modeSelctor.SelectedIndex = 1;
                    //                    switch (_nowInfo.XmlFile.ColorStr)
                    //                    {
                    //                        case "black":
                    //                            _currentColor = Colors.Black;
                    //                            colorSelector.SelectedIndex = 0;
                    //                            break;
                    //                        case "silver":
                    //                            _currentColor = Colors.Silver;
                    //                            colorSelector.SelectedIndex = 1;
                    //                            break;
                    //                        case "gray":
                    //                            _currentColor = Colors.Gray;
                    //                            colorSelector.SelectedIndex = 2;
                    //                            break;
                    //                        case "white":
                    //                            _currentColor = Colors.White;
                    //                            colorSelector.SelectedIndex = 3;
                    //                            break;
                    //                        case "maroon":
                    //                            _currentColor = Colors.Maroon;
                    //                            colorSelector.SelectedIndex = 4;
                    //                            break;
                    //                        case "red":
                    //                            _currentColor = Colors.Red;
                    //                            colorSelector.SelectedIndex = 5;
                    //                            break;
                    //                        case "purple":
                    //                            _currentColor = Colors.Purple;
                    //                            colorSelector.SelectedIndex = 6;
                    //                            break;
                    //                        case "fuchsia":
                    //                            _currentColor = Colors.Fuchsia;
                    //                            colorSelector.SelectedIndex = 7;
                    //                            break;
                    //                        case "green":
                    //                            _currentColor = Colors.Green;
                    //                            colorSelector.SelectedIndex = 8;
                    //                            break;
                    //                        case "lime":
                    //                            _currentColor = Colors.Lime;
                    //                            colorSelector.SelectedIndex = 9;
                    //                            break;
                    //                        case "olive":
                    //                            _currentColor = Colors.Olive;
                    //                            colorSelector.SelectedIndex = 10;
                    //                            break;
                    //                        case "yellow":
                    //                            _currentColor = Colors.Yellow;
                    //                            colorSelector.SelectedIndex = 11;
                    //                            break;
                    //                        case "navy":
                    //                            _currentColor = Colors.Navy;
                    //                            colorSelector.SelectedIndex = 12;
                    //                            break;
                    //                        case "blue":
                    //                            _currentColor = Colors.Blue;
                    //                            colorSelector.SelectedIndex = 13;
                    //                            break;
                    //                        case "teal":
                    //                            _currentColor = Colors.Teal;
                    //                            colorSelector.SelectedIndex = 14;
                    //                            break;
                    //                        case "aqua":
                    //                            _currentColor = Colors.Aqua;
                    //                            colorSelector.SelectedIndex = 15;
                    //                            break;
                    //                        default:
                    //                            _currentColor = Helper.GetColorFromRgbString(_nowInfo.XmlFile.ColorStr);
                    //                            colorSelector.SelectedIndex = 16;
                    //                            defineColorText.Text = _nowInfo.XmlFile.ColorStr;
                    //                            break;
                    //                    }

                    switch (_currectInfo.XmlFile.ColorStr)
                    {
                        case "black":
                            //                            _currentColor = Colors.Black;
                            colorSelector_1.IsChecked = true;
                            break;
                        case "silver":
                            //                            _currentColor = Colors.Silver;
                            colorSelector_2.IsChecked = true;
                            break;
                        case "gray":
                            //                            _currentColor = Colors.Gray;
                            colorSelector_3.IsChecked = true;
                            break;
                        case "white":
                            //                            _currentColor = Colors.White;
                            colorSelector_4.IsChecked = true;
                            break;
                        case "maroon":
                            //                            _currentColor = Colors.Maroon;
                            colorSelector_5.IsChecked = true;
                            break;
                        case "red":
                            //                            _currentColor = Colors.Red;
                            colorSelector_6.IsChecked = true;
                            break;
                        case "purple":
                            //                            _currentColor = Colors.Purple;
                            colorSelector_7.IsChecked = true;
                            break;
                        case "fuchsia":
                            //                            _currentColor = Colors.Fuchsia;
                            colorSelector_8.IsChecked = true;
                            break;
                        case "green":
                            //                            _currentColor = Colors.Green;
                            colorSelector_9.IsChecked = true;
                            break;
                        case "lime":
                            //                            _currentColor = Colors.Lime;
                            colorSelector_10.IsChecked = true;
                            break;
                        case "olive":
                            //                            _currentColor = Colors.Olive;
                            colorSelector_11.IsChecked = true;
                            break;
                        case "yellow":
                            //                            _currentColor = Colors.Yellow;
                            colorSelector_12.IsChecked = true;
                            break;
                        case "navy":
                            //                            _currentColor = Colors.Navy;
                            colorSelector_13.IsChecked = true;
                            break;
                        case "blue":
                            //                            _currentColor = Colors.Blue;
                            colorSelector_14.IsChecked = true;
                            break;
                        case "teal":
                            //                            _currentColor = Colors.Teal;
                            colorSelector_15.IsChecked = true;
                            break;
                        case "aqua":
                            //                            _currentColor = Colors.Aqua;
                            colorSelector_16.IsChecked = true;
                            break;
                        default:
                            try
                            {
                                _currentColor = Helper.GetColorFromRgbString(_currectInfo.XmlFile.ColorStr);
                                _currentColorString = _currectInfo.XmlFile.ColorStr;
                                colorSelector_17.IsChecked = true;
                                defineColorText.Text = _currectInfo.XmlFile.ColorStr;
                            }
                            catch (FormatException)
                            {
                                _currectInfo.XmlFile.ColorStr = "black";
                                _currentColor = Colors.Black;
                                colorSelector_1.IsChecked = true;
                            }
                            break;
                    }

                    //                    if (!string.IsNullOrEmpty(_nowInfo.XmlFile.LargeIconLoc))
                    //                    {
                    //                        if (File.Exists(_nowInfo.XmlFile.LargeIconLoc))
                    //                        {
                    //                            Debug.WriteLine(
                    //                                $"Load large icon successfully with file location{_nowInfo.XmlFile.LargeIconLoc}");
                    //                            _icon = new Bitmap(_nowInfo.XmlFile.LargeIconLoc);
                    //                        }
                    //                        else if (Directory.Exists(Path.GetDirectoryName(_nowInfo.XmlFile.LargeIconLoc)))
                    //                        {
                    //                            var files = Directory.GetFiles(Path.GetDirectoryName(_nowInfo.XmlFile.LargeIconLoc));
                    //                            // ReSharper disable once AssignNullToNotNullAttribute
                    //                            var regex = new Regex(Path.Combine(Path.GetDirectoryName(_nowInfo.XmlFile.LargeIconLoc),
                    //                                                      Path.GetFileNameWithoutExtension(_nowInfo.XmlFile
                    //                                                          .LargeIconLoc)).RegexFree() + "\\.scale-\\d+\\" +
                    //                                                  Path.GetExtension(_nowInfo.XmlFile.LargeIconLoc));
                    //                            var lastFileName = files.LastOrDefault(a => regex.IsMatch(a));
                    //                            if (!string.IsNullOrEmpty(lastFileName))
                    //                            {
                    //                                Debug.WriteLine($"Load large icon successfully with file location{lastFileName}");
                    //                                _largeIcon = new Bitmap(lastFileName);
                    //                            }
                    //                        }
                    //                    }
                    var tryLarge = false;
                    var tryDir = true;
                    geticonLoc:
                    if (!string.IsNullOrEmpty(_currectInfo.XmlFile.SmallIconLoc))
                    {
                        if (File.Exists(_currectInfo.XmlFile.SmallIconLoc))
                        {
                            Debug.WriteLine(
                                $"Load small icon successfully with file location{_currectInfo.XmlFile.SmallIconLoc}");
                            _icon = new Bitmap(_currectInfo.XmlFile.SmallIconLoc);
                            defineIconCheck.IsChecked = true;
                        }
                        else if (Directory.Exists(Path.GetDirectoryName(_currectInfo.XmlFile.SmallIconLoc)) && tryDir)
                        {
                            // ReSharper disable once AssignNullToNotNullAttribute
                            var files = Directory.GetFiles(Path.GetDirectoryName(_currectInfo.XmlFile.SmallIconLoc));
                            // ReSharper disable once AssignNullToNotNullAttribute
                            var regex = new Regex(Path.Combine(Path.GetDirectoryName(_currectInfo.XmlFile.SmallIconLoc),
                                                      Path.GetFileNameWithoutExtension(_currectInfo.XmlFile
                                                          .SmallIconLoc)).RegexFree() + "\\.scale-\\d+\\" +
                                                  Path.GetExtension(_currectInfo.XmlFile.SmallIconLoc));
                            var lastFileName = files.LastOrDefault(a => regex.IsMatch(a));
                            if (!string.IsNullOrEmpty(lastFileName))
                            {
                                Debug.WriteLine($"Load small icon successfully with file location{lastFileName}");
                                _icon = new Bitmap(lastFileName);
                                defineIconCheck.IsChecked = true;
                            }
                            tryDir = false;
                            goto geticonLoc;
                        }
                        else
                        {
                            if (!tryLarge && !string.IsNullOrEmpty(_currectInfo.XmlFile.LargeIconLoc))
                            {
                                tryLarge = true;
                                _currectInfo.XmlFile.SmallIconLoc = _currectInfo.XmlFile.LargeIconLoc;
                                goto geticonLoc;
                            }
                            _currectInfo.XmlFile.LargeIconLoc = _currectInfo.XmlFile.SmallIconLoc = string.Empty;
                            _icon = null;
                            defineIconCheck.IsChecked = false;

                        }
                    }
                    else
                    {
                        if (!tryLarge && !string.IsNullOrEmpty(_currectInfo.XmlFile.LargeIconLoc))
                        {
                            tryLarge = true;
                            _currectInfo.XmlFile.SmallIconLoc = _currectInfo.XmlFile.LargeIconLoc;
                            goto geticonLoc;
                        }
                        _icon = null;
                        defineIconCheck.IsChecked = false;
                    }

                    txtColorSelector.SelectedIndex = (int)_currectInfo.XmlFile.TxtForeground;
                }
                catch
                {
#if DEBUG
                    throw;
#endif
#pragma warning disable 162
                    // ReSharper disable once HeuristicUnreachableCode
                    MessageBox.Show("读取配置文件时发生错误\n已重置到初始值", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    File.Delete(_currectInfo.XmlFileLocation);
                    Load();
                    return;
#pragma warning restore 162
                }
            }
            UpdatePreview();
            _sysChangeing = false;
        }

        private void UpdatePreview()
        {
            defineColorPreview.Fill = new SolidColorBrush(_currentColor);
            previewColor.Color = _currentColor;
            preview_LargeText.Foreground = (txtColorSelector.SelectedIndex == 0) ? Brushes.White : Brushes.Black;
            preview_LargeText.Visibility = largeAppNameCheck.IsChecked ?? false ? Visibility.Visible : Visibility.Hidden;

            if (_icon == null || !(defineIconCheck.IsChecked ?? false))
            {
                preview_SmallImg.Source = preview_LargeImg.Source = _iconList[_currentId].GetBitmapSourceFromBitmap();
                preview_SmallImg.Stretch = preview_LargeImg.Stretch = Stretch.None;
            }
            else
            {
                preview_SmallImg.Source = preview_LargeImg.Source = _icon.GetBitmapSourceFromBitmap();
                preview_SmallImg.Stretch = (_icon.Size.Width > 70 || _icon.Size.Height > 70) ? Stretch.Uniform : Stretch.None;
                preview_LargeImg.Stretch = (_icon.Size.Width > 150 || _icon.Size.Height > 150) ? Stretch.Uniform : Stretch.None;
            }

            preview_LargeText.Text = Path.GetFileNameWithoutExtension(_fileList[_currentId]);
        }

        private bool SaveCheck()
        {
            if (!_saveFlag) return true;
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (MessageBox.Show("更改尚未保存，是否保存？", "警告", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning,
                MessageBoxResult.Cancel))
            {
                case MessageBoxResult.Yes:
                    return Save();
                case MessageBoxResult.No:
                    _saveFlag = false;
                    return true;
                default:
                    return false;
            }
        }

        private bool Save()
        {
            //todo save
            if (modeSelctor.SelectedIndex == 0)
            {
                if (_currectInfo.XmlFile != null)
                {
                    if (MessageBox.Show("将删除所有自定义效果文件\n继续?", "⚠警告", MessageBoxButton.YesNo, MessageBoxImage.Warning) !=
                        MessageBoxResult.Yes) return false;
                    _currectInfo.XmlFile = null;
                    File.Delete(_currectInfo.XmlFileLocation);
                }
            }
            else
            {
                if (_currectInfo.XmlFile == null)
                {
                    _currectInfo.XmlFile = new StartmenuXmlFile(_currectInfo.XmlFileLocation);
                }
                _currectInfo.XmlFile.ColorStr = _currentColorString;
                _currectInfo.XmlFile.TxtForeground = (StartmenuXmlFile.TextCol)txtColorSelector.SelectedIndex;
                _currectInfo.XmlFile.ShowTitleOnLargeIcon = largeAppNameCheck.IsChecked ?? false;
                //保存图片
                var sha1 = SHA1.Create();
                // ReSharper disable once AssignNullToNotNullAttribute
                var pathName = Path.Combine(Path.GetDirectoryName(_currectInfo.XmlFileLocation),
                    "__StartmenuIcons__");
                if (!Directory.Exists(pathName))
                    Directory.CreateDirectory(pathName);

                //                                if (!string.IsNullOrEmpty(_newLargeIconLoc) && (defineLargeIconCheck.IsChecked ?? false))
                //                                {
                //                                    //计算SHA1
                //                                    var fs = File.Open(_newLargeIconLoc, FileMode.Open, FileAccess.Read);
                //                                    var fileName = Path.Combine(pathName,
                //                                                       BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", string.Empty)) + Path.GetExtension(_newLargeIconLoc);
                //                                    fs.Close();
                //                                    if (!File.Exists(fileName)) //拷贝图像到目录
                //                                        File.Copy(_newLargeIconLoc, fileName);
                //                                    _nowInfo.XmlFile.LargeIconLoc = fileName;
                //                                }
                //文件名：[DIR]\__StartmenuIcons__\[SHA1].png
                if (!string.IsNullOrEmpty(_newIconLoc) && (defineIconCheck.IsChecked ?? false))
                {
                    //计算SHA1
                    var fs = File.Open(_newIconLoc, FileMode.Open, FileAccess.Read);
                    var fileName = Path.Combine(pathName,
                                       BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", string.Empty)) + Path.GetExtension(_newIconLoc);
                    fs.Close();
                    if (!File.Exists(fileName)) //拷贝图像到目录
                        File.Copy(_newIconLoc, fileName);
                    _currectInfo.XmlFile.SmallIconLoc = _currectInfo.XmlFile.LargeIconLoc = fileName;
                }
                _currectInfo.XmlFile.Save();
            }
            //Update file and let the explorer reload the link
            Helper.UpdateFile(_currectInfo.Location);

            _saveFlag = false;
            return true;
        }

        #endregion
    }
}
