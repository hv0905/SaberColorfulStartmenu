using SaberColorfulStartmenu.Core;
using SaberColorfulStartmenu.Helpers;

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        #region 字段

        private List<string> _fileList = new List<string>();
        private List<Bitmap> _iconList = new List<Bitmap>();
        private bool _saveFlag, _loaded, _sysChangeing;
        private Color _nowColor = Colors.Black;
        private string _nowColorString;
        private int _nowWorkingId = -1;
        private __WinForm.ColorDialog _colorDialog;
        private OpenFileDialog _openFile;
        private Bitmap _largeIcon, _smallIcon;
        private StartmenuShortcutInfo _nowInfo;
        private string _newLargeIconLoc, _newSmallIconLoc;

        #endregion

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

        #region 事件

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) => Close();

        private void ButtonBase_OnClick_1(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void ButtonBase_OnClick_2(object sender, RoutedEventArgs e)
        {
            SaveCheck();
            appList.Items.Clear();
            RefreshList();
        }

        private void ButtonBase_OnClick_8(object sender, RoutedEventArgs e)
        {
            _openFile.Title = "选择70x70小图标 建议比例：1：1";
            if (!(_openFile.ShowDialog() ?? false)) return;
            var fs = File.Open(_openFile.FileName, FileMode.Open, FileAccess.Read);
            _saveFlag = true;
            _newSmallIconLoc = _openFile.FileName;
            _smallIcon = new Bitmap(fs);
            UpdatePreview();
            fs.Close();
        }

        private void ButtonBase_OnClick_3(object sender, RoutedEventArgs e) => new AboutWindow().Show();

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
                _nowColorString = _nowColor.ToRgbString();
                defineColorText.Text = _nowColorString;
                UpdatePreview();
            }
        }

        private void DefineColorText_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (_sysChangeing) return;
            if (defineColorText.Text.Length != 7 || !defineColorText.Text.StartsWith("#")) return;
            _saveFlag = true;
            try
            {
                // ReSharper disable once PossibleNullReferenceException
                _nowColor = Helper.GetColorFromRgbString(defineColorText.Text);
                _nowColorString = defineColorText.Text;
                UpdatePreview();
            }
            catch (FormatException)
            {
                Debug.WriteLine("ChangeColor Stoped.");
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
                _newLargeIconLoc = _openFile.FileName;
                _largeIcon = new Bitmap(fs);
                UpdatePreview();
                fs.Close();
            }
            finally { }

        }

        private void ButtonBase_OnClick_6(object sender, RoutedEventArgs e)
        {
            if (Save())
                ((Storyboard)Resources["saveDoneStory"]).Begin();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loaded) return;
            if (!_sysChangeing) _saveFlag = true;
            // ReSharper disable once PossibleUnintendedReferenceComparison
            group_Color.Visibility = colorSelector.SelectedItem == defineColorItem ? Visibility.Visible : Visibility.Collapsed;
            switch (colorSelector.SelectedIndex)
            {
                case 0://固定色区
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
                case 16://自定义
                    try
                    {
                        _nowColor = Helper.GetColorFromRgbString(defineColorText.Text);
                        _nowColorString = defineColorText.Text;
                    }
                    catch (FormatException)
                    {
                        defineColorText.Text = "#000000";
                        _nowColorString = "black";
                        _nowColor = Colors.Black;
                    }

                    break;
            }
            if (!_sysChangeing)
                UpdatePreview();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e) =>
            e.Cancel = !SaveCheck();
        #endregion

        #region 方法

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
            _nowInfo = new StartmenuShortcutInfo(_fileList[_nowWorkingId]);
            if (_nowInfo.XmlFile == null)
            {
                modeSelctor.SelectedIndex = 0;
                //set everything to empty
                _nowColor = Colors.Black;
                _nowColorString = "black";
                _largeIcon = _smallIcon = null;
                defineLargeIconCheck.IsChecked = defineSmallIconCheck.IsChecked = false;
                txtColorSelector.SelectedIndex = 0;
                colorSelector.SelectedIndex = 0;

            }
            else
            {
                try
                {
                    modeSelctor.SelectedIndex = 1;
                    switch (_nowInfo.XmlFile.ColorStr)
                    {
                        case "black":
                            _nowColor = Colors.Black;
                            colorSelector.SelectedIndex = 0;
                            break;
                        case "silver":
                            _nowColor = Colors.Silver;
                            colorSelector.SelectedIndex = 1;
                            break;
                        case "gray":
                            _nowColor = Colors.Gray;
                            colorSelector.SelectedIndex = 2;
                            break;
                        case "white":
                            _nowColor = Colors.White;
                            colorSelector.SelectedIndex = 3;
                            break;
                        case "maroon":
                            _nowColor = Colors.Maroon;
                            colorSelector.SelectedIndex = 4;
                            break;
                        case "red":
                            _nowColor = Colors.Red;
                            colorSelector.SelectedIndex = 5;
                            break;
                        case "purple":
                            _nowColor = Colors.Purple;
                            colorSelector.SelectedIndex = 6;
                            break;
                        case "fuchsia":
                            _nowColor = Colors.Fuchsia;
                            colorSelector.SelectedIndex = 7;
                            break;
                        case "green":
                            _nowColor = Colors.Green;
                            colorSelector.SelectedIndex = 8;
                            break;
                        case "lime":
                            _nowColor = Colors.Lime;
                            colorSelector.SelectedIndex = 9;
                            break;
                        case "olive":
                            _nowColor = Colors.Olive;
                            colorSelector.SelectedIndex = 10;
                            break;
                        case "yellow":
                            _nowColor = Colors.Yellow;
                            colorSelector.SelectedIndex = 11;
                            break;
                        case "navy":
                            _nowColor = Colors.Navy;
                            colorSelector.SelectedIndex = 12;
                            break;
                        case "blue":
                            _nowColor = Colors.Blue;
                            colorSelector.SelectedIndex = 13;
                            break;
                        case "teal":
                            _nowColor = Colors.Teal;
                            colorSelector.SelectedIndex = 14;
                            break;
                        case "aqua":
                            _nowColor = Colors.Aqua;
                            colorSelector.SelectedIndex = 15;
                            break;
                        default:
                            _nowColor = Helper.GetColorFromRgbString(_nowInfo.XmlFile.ColorStr);
                            colorSelector.SelectedIndex = 16;
                            defineColorText.Text = _nowInfo.XmlFile.ColorStr;
                            break;
                    }

                    if (!string.IsNullOrEmpty(_nowInfo.XmlFile.LargeIconLoc))
                    {
                        if (File.Exists(_nowInfo.XmlFile.LargeIconLoc))
                        {
                            Debug.WriteLine(
                                $"Load large icon successfully with file location{_nowInfo.XmlFile.LargeIconLoc}");
                            _largeIcon = new Bitmap(_nowInfo.XmlFile.LargeIconLoc);
                        }
                        else if (Directory.Exists(Path.GetDirectoryName(_nowInfo.XmlFile.LargeIconLoc)))
                        {
                            var files = Directory.GetFiles(Path.GetDirectoryName(_nowInfo.XmlFile.LargeIconLoc));
                            // ReSharper disable once AssignNullToNotNullAttribute
                            var regex = new Regex(Path.Combine(Path.GetDirectoryName(_nowInfo.XmlFile.LargeIconLoc),
                                                      Path.GetFileNameWithoutExtension(_nowInfo.XmlFile
                                                          .LargeIconLoc)).RegexFree() + "\\.scale-\\d+\\" +
                                                  Path.GetExtension(_nowInfo.XmlFile.LargeIconLoc));
                            var lastFileName = files.LastOrDefault(a => regex.IsMatch(a));
                            if (!string.IsNullOrEmpty(lastFileName))
                            {
                                Debug.WriteLine($"Load large icon successfully with file location{lastFileName}");
                                _largeIcon = new Bitmap(lastFileName);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(_nowInfo.XmlFile.SmallIconLoc))
                    {
                        if (File.Exists(_nowInfo.XmlFile.SmallIconLoc))
                        {
                            Debug.WriteLine(
                                $"Load small icon successfully with file location{_nowInfo.XmlFile.SmallIconLoc}");
                            _largeIcon = new Bitmap(_nowInfo.XmlFile.SmallIconLoc);
                        }
                        else if (Directory.Exists(Path.GetDirectoryName(_nowInfo.XmlFile.SmallIconLoc)))
                        {
                            // ReSharper disable once AssignNullToNotNullAttribute
                            var files = Directory.GetFiles(Path.GetDirectoryName(_nowInfo.XmlFile.SmallIconLoc));
                            // ReSharper disable once AssignNullToNotNullAttribute
                            var regex = new Regex(Path.Combine(Path.GetDirectoryName(_nowInfo.XmlFile.SmallIconLoc),
                                                      Path.GetFileNameWithoutExtension(_nowInfo.XmlFile
                                                          .SmallIconLoc)).RegexFree() + "\\.scale-\\d+\\" +
                                                  Path.GetExtension(_nowInfo.XmlFile.SmallIconLoc));
                            var lastFileName = files.LastOrDefault(a => regex.IsMatch(a));
                            if (!string.IsNullOrEmpty(lastFileName))
                            {
                                Debug.WriteLine($"Load small icon successfully with file location{lastFileName}");
                                _largeIcon = new Bitmap(lastFileName);
                            }
                        }
                    }

                    txtColorSelector.SelectedIndex = (int)_nowInfo.XmlFile.TxtForeground;
                }
                catch
                {
                    MessageBox.Show("读取配置文件时发生错误\n已重置到初始值", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    File.Delete(_nowInfo.XmlFileLocation);
                    _nowInfo.XmlFile = null;
                    Load();
                    return;
                }
            }
            UpdatePreview();
            _sysChangeing = false;
        }

        private void UpdatePreview()
        {
            defineColorPreview.Fill = new SolidColorBrush(_nowColor);
            previewColor.Color = _nowColor;
            preview_LargeText.Foreground = (txtColorSelector.SelectedIndex == 0) ? Brushes.White : Brushes.Black;
            preview_LargeText.Visibility = largeAppNameCheck.IsChecked ?? false ? Visibility.Visible : Visibility.Hidden;
            if (_largeIcon == null || !(defineLargeIconCheck.IsChecked ?? false))
            {
                preview_LargeImg.Source = _iconList[_nowWorkingId].GetBitmapSourceFromBitmap();
                preview_LargeImg.Stretch = Stretch.None;
            }
            else
            {
                preview_LargeImg.Source = _largeIcon.GetBitmapSourceFromBitmap();
                preview_LargeImg.Stretch = (_largeIcon.Size.Width > 150 || _largeIcon.Size.Height > 150) ? Stretch.Uniform : Stretch.None;
            }

            if (_smallIcon == null || !(defineSmallIconCheck.IsChecked ?? false))
            {
                preview_SmallImg.Source = _iconList[_nowWorkingId].GetBitmapSourceFromBitmap();
                preview_SmallImg.Stretch = Stretch.None;
            }
            else
            {
                preview_SmallImg.Source = _smallIcon.GetBitmapSourceFromBitmap();
                preview_SmallImg.Stretch = (_smallIcon.Size.Width > 70 || _smallIcon.Size.Height > 70) ? Stretch.Uniform : Stretch.None;
            }

            preview_LargeText.Text = Path.GetFileNameWithoutExtension(_fileList[_nowWorkingId]);
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
                if (_nowInfo.XmlFile != null)
                {
                    if (MessageBox.Show("将删除所有自定义效果文件\n继续?", "⚠警告", MessageBoxButton.YesNo, MessageBoxImage.Warning) !=
                        MessageBoxResult.Yes) return false;
                    _nowInfo.XmlFile = null;
                    File.Delete(_nowInfo.XmlFileLocation);
                }
            }
            else
            {
                if (_nowInfo.XmlFile == null)
                {
                    _nowInfo.XmlFile = new StartmenuXmlFile(_nowInfo.XmlFileLocation);
                }
                _nowInfo.XmlFile.ColorStr = _nowColorString;
                _nowInfo.XmlFile.TxtForeground = (StartmenuXmlFile.TextCol)txtColorSelector.SelectedIndex;
                _nowInfo.XmlFile.ShowTitleOnLargeIcon = largeAppNameCheck.IsChecked ?? false;
                //保存图片
                var sha1 = SHA1.Create();
                // ReSharper disable once AssignNullToNotNullAttribute
                var pathName = Path.Combine(Path.GetDirectoryName(_nowInfo.XmlFileLocation),
                    "__StartmenuIcons__");
                if (!Directory.Exists(pathName))
                    Directory.CreateDirectory(pathName);
                //文件名：[DIR]\__StartmenuIcons__\[SHA1].png
                if (!string.IsNullOrEmpty(_newLargeIconLoc) && (defineLargeIconCheck.IsChecked ?? false))
                {
                    //计算SHA1
                    var fs = File.Open(_newLargeIconLoc, FileMode.Open, FileAccess.Read);
                    var fileName = Path.Combine(pathName,
                                       BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", string.Empty)) + Path.GetExtension(_newLargeIconLoc);
                    fs.Close();
                    if (!File.Exists(fileName)) //拷贝图像到目录
                        File.Copy(_newLargeIconLoc, fileName);
                    _nowInfo.XmlFile.LargeIconLoc = fileName;
                }

                if (!string.IsNullOrEmpty(_newSmallIconLoc) && (defineSmallIconCheck.IsChecked ?? false))
                {
                    //计算SHA1
                    var fs = File.Open(_newSmallIconLoc, FileMode.Open, FileAccess.Read);
                    var fileName = Path.Combine(pathName,
                                       BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", string.Empty)) + Path.GetExtension(_newSmallIconLoc);
                    fs.Close();
                    if (!File.Exists(fileName)) //拷贝图像到目录
                        File.Copy(_newSmallIconLoc, fileName);
                    _nowInfo.XmlFile.SmallIconLoc = fileName;
                }
                _nowInfo.XmlFile.Save();
            }
            Helper.UpdateFile(_nowInfo.Location);
            _saveFlag = false;
            return true;
        }

        #endregion


    }
}