﻿//#define DEBUG_SHOW_DETAILS

using SaberColorfulStartmenu.Core;
using SaberColorfulStartmenu.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using ModernMessageBoxLib;
using __WinForm = System.Windows.Forms;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using File = System.IO.File;
using Size = System.Drawing.Size;

/*
 * 这里解释下win10下开始菜单重复快捷方式的判定
 * Win10下如果遇到重复的快捷方式，分为以下两种情况
 * 1）指向文件相同，快捷方式名不同              //为了保证更改能被应用，需要同时更新两个（或更多）的快捷方式文件
 *      win10：随机选取一个在开始菜单中显示
 * 2）指向文件不同，快捷方式名相同              //无需理会
 *      win10：同时显示两个快捷方式
 * 3）指向文件相同，快捷方式名相同              //同1）
 *      win10：同1）
 */

namespace SaberColorfulStartmenu
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        #region Fields

        private List<StartmenuShortcutInfo> _applistData = new List<StartmenuShortcutInfo>();
        private bool _saveFlag;
        private bool _loaded;
        private bool _sysChangeing, _scaleMode;
        private Color _currentColor = Colors.Black;
        private string _currentColorString;
        private int _currentId = -1;
        private __WinForm.ColorDialog _colorDialog;
        private OpenFileDialog _openFile;
        private BitmapSource _logo;
        //private StartmenuShortcutInfo _currentInfo;
        private string _newLogoLoc;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            _colorDialog = new __WinForm.ColorDialog {
                AllowFullOpen = true,
                AnyColor = true,
                FullOpen = true
            };
            _openFile = new OpenFileDialog {
                AddExtension = true,
                Filter = "图像文件|*.png;*.jpg;*.jpeg;*.gif",
                Title = "选择图标 尺寸>150x150",
            };
            RefreshList();
            appList.ItemsSource = _applistData;
        }

        #region Events

        private void CloseBtn_OnClick(object sender, RoutedEventArgs e) => Close();

        private void MinBtn_OnClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void Main_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            SaveCheck();
            appList.SelectedIndex = -1;
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

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _loaded = true;
            gridSetMain.Visibility = Visibility.Hidden;
            _sysChangeing = true;
            //Selector_OnSelectionChanged_1(sender, null);
            ToggleButton_OnChecked(this, null);
            _sysChangeing = false;
        }

        private void ButtonBase_OnClick_5(object sender, RoutedEventArgs e)
        {
            if (_colorDialog.ShowDialog() != __WinForm.DialogResult.OK) return;
            _saveFlag = true;
            _currentColor = _colorDialog.Color.ToMediaColor();
            _currentColorString = _currentColor.ToRgbString();
            _sysChangeing = true;
            defineColorText.Text = _currentColorString;
            _sysChangeing = false;
            UpdateRender();
        }

        private void SaveAndUpdate_RoutedEvent(object sender, RoutedEventArgs e)
        {
            if (!_loaded || _sysChangeing) return;
            _saveFlag = true;
            UpdateRender();
        }

        private void ButtonBase_OnClick_7(object sender, RoutedEventArgs e)
        {
            if (_scaleMode && QModernMessageBox.Show("警告，本操作不可逆。\n继续将清除开发者定义的可缩放图标，除非重新安装该程序，否则该图标可能不能恢复。\n继续操作？", "警告",
                    QModernMessageBox.QModernMessageBoxButtons.YesNo, ModernMessageboxIcons.Warning) != ModernMessageboxResult.Button1) return;


            if (!(_openFile.ShowDialog() ?? false)) return;
            try
            {
                var fs = File.Open(_openFile.FileName, FileMode.Open, FileAccess.Read);
                var img = new Bitmap(fs);
                var win = new ImageSnipWindow(img, new Size(150, 150));
                win.ShowDialog();
                Bitmap tmpBmp;
                switch (win.Result)
                {
                    case ImageSnipWindow.SnapWindowResult.Ok:
                        tmpBmp = win.Dst;
                        break;
                    case ImageSnipWindow.SnapWindowResult.Cancel:
                        img.Dispose();
                        fs.Close();
                        return;
                    case ImageSnipWindow.SnapWindowResult.Ignore:
                        if (img.Size.Width >= 150 && img.Size.Height >= 150)
                        {
                            //尽量使用原图
                            _newLogoLoc = _openFile.FileName;
                            _logo = img.ToBitmapSource();
                            goto done;
                        }

                        tmpBmp = img;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (tmpBmp.Size.Width < 150 || tmpBmp.Size.Height < 150)
                {
                    if (tmpBmp.Size.Width < 150 && tmpBmp.Size.Height < 150)
                    {
                        var newBmp = new Bitmap(150, 150);
                        var gps = Graphics.FromImage(newBmp);
                        gps.CompositingQuality = CompositingQuality.HighQuality;
                        gps.Clear(System.Drawing.Color.Transparent);
                        var x = (150 - tmpBmp.Size.Width) / 2;
                        var y = (150 - tmpBmp.Size.Height) / 2;
                        gps.DrawImage(tmpBmp, x, y, tmpBmp.Size.Width, tmpBmp.Size.Height);
                        tmpBmp.Dispose();
                        tmpBmp = newBmp;
                    }
                    else
                    {
                        tmpBmp = new Bitmap(tmpBmp, 150, 150);
                    }
                }

                _logo = tmpBmp.ToBitmapSource();
                _newLogoLoc = Path.GetTempFileName() + ".png";
                tmpBmp.Save(_newLogoLoc, ImageFormat.Png);
                tmpBmp.Dispose();
                done:
                img.Dispose();
                fs.Close();
            }
            catch (IOException ex)
            {
                QModernMessageBox.Error($"图片载入失败\n未知错误，无法读取此文件\n详细信息：{ex.Message}", "错误");
                return;
            }

            _saveFlag = true;
            UpdateRender();
        }

        private void ButtonBase_OnClick_6(object sender, RoutedEventArgs e)
        {
            if (Save())
                ((Storyboard)Resources["saveDoneStory"]).Begin();
        }

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
            if (!_sysChangeing) _saveFlag = true;
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
            {
                //自定义
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

            if (!_sysChangeing) UpdateRender();
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (!_loaded) return;
            if (!_sysChangeing) _saveFlag = true;
            if (!(modeCheck.IsChecked ?? false))
            {
                colorSelector.Visibility = Visibility.Collapsed;
                group_Color.Visibility = Visibility.Collapsed;
            }
            else
            {
                colorSelector.Visibility = Visibility.Visible;
                group_Color.Visibility =
                    colorSelector_17.IsChecked ?? false ? Visibility.Visible : Visibility.Collapsed;
            }

            if (!_sysChangeing) UpdateRender();
        }

        private void UndoBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentId == -1) return;
            if (QModernMessageBox.Show("将还原上一次保存的更改\n继续？", "Undo", QModernMessageBox.QModernMessageBoxButtons.YesNo, ModernMessageboxIcons.Info) !=
                ModernMessageboxResult.Button1) return;
            _saveFlag = false;
            var currentInfo = _applistData[_currentId];
            currentInfo.Undo();
            foreach (var item in _applistData)
            {
                if (item.TargetPath == currentInfo.TargetPath)
                {
                    //update
                    Helper.UpdateFile(item.FullPath);
                }
            }

            Load();
        }

        private void DefineColorText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_loaded) return;
            if (_sysChangeing) return;
            if (defineColorText.Text.Length != 7 || !defineColorText.Text.StartsWith("#")) return;
            _saveFlag = true;
            try
            {
                // ReSharper disable once PossibleNullReferenceException
                _currentColor = Helper.GetColorFromRgbString(defineColorText.Text);
                _currentColorString = defineColorText.Text;
                defineColorText.Foreground = Brushes.Black;
                defineColorText.ToolTip = null;
                UpdateRender();
            }
            catch (FormatException)
            {
                defineColorText.Foreground = Brushes.DeepPink;
                defineColorText.ToolTip = "格式错误.";
                Debug.WriteLine("ChangeColor Canceled.");
            }
        }

        private void ButtonBase_OnClick_8(object sender, RoutedEventArgs e)
        {
            var counter = 0;
            foreach (var item in _applistData)
            {
                try
                {
                    if (!File.Exists(item.ShadowFileLocation)) continue;
                    if (!File.Exists(item.XmlFileLocation) || !File.ReadAllBytes(item.ShadowFileLocation).SequenceEqual(File.ReadAllBytes(item.XmlFileLocation)))
                    {
                        item.ShadowUndo();
                        Helper.UpdateFile(item.FullPath);
                        counter++;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    QModernMessageBox.Error("无法保存设定.\n权限不足.", "错误");
                }
                catch (IOException ex)
                {
                    QModernMessageBox.Error("无法读取该文件设定.\n发生了IO异常，请稍后再试\n更多信息:" + ex.Message, "错误");
                }
            }

            QModernMessageBox.Info(this, $"修复了{counter}个图标.", "修复结果");
        }

        #endregion

        #region Functions

        private void RefreshList()
        {
#if DEBUG
            var stop = new Stopwatch();
            stop.Start();
#endif
            _applistData.Clear();
            GC.Collect();

            _currentId = -1;
            //获取所有子目录内容
            //只监视.lnk文件
            var fileList = new List<string>();
            fileList.AddRange(Helper.GetAllFilesByDir(App.StartMenu));
            fileList.AddRange(Helper.GetAllFilesByDir(App.CommonStartMenu));
            fileList.RemoveAll(str => !str.EndsWith(".lnk", StringComparison.CurrentCultureIgnoreCase));
            foreach (var item in fileList)
            {
                var target = ShortcutHelper.ResolveShortcut(item);
                //__tf:
#if DEBUG_SHOW_DETAILS
                Debug.WriteLine(target);
#endif
                if ((!target.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase)) || !File.Exists(target))
                {
#if DEBUG_SHOW_DETAILS
                    Debug.WriteLine("Torow!!!");
#endif
                    continue;
                }

                var si = new StartmenuShortcutInfo(item, target);
                si.LoadIcon();
                _applistData.Add(si);
            }

            _applistData.Sort();
#if DEBUG
            stop.Stop();
            Debug.WriteLine("Refresh list take:" + stop.Elapsed + " ms");
#endif
        }

        private void Load()
        {
            _sysChangeing = true;

            _newLogoLoc = null; //防止意外的保存
            _logo = null;
            _scaleMode = false;

            var currentInfo = _applistData[_currentId];
            if (currentInfo.XmlFile == null)
            {
                try
                {
                    currentInfo.LoadXmlInfo();
                }
                catch (UnauthorizedAccessException)
                {
                    QModernMessageBox.Show("无法读取该文件设定.\n权限不足。", "错误", QModernMessageBox.QModernMessageBoxButtons.Ok,
                        ModernMessageboxIcons.Error);
                    _sysChangeing = false;
                    appList.SelectedIndex = -1;
                    return;
                }
                catch (IOException ex)
                {
                    QModernMessageBox.Show("无法读取该文件设定.\n发生了IO异常，请稍后再试\n更多信息:" + ex.Message, "错误",
                        QModernMessageBox.QModernMessageBoxButtons.Ok, ModernMessageboxIcons.Error);
                    _sysChangeing = false;
                    appList.SelectedIndex = -1;
                    return;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("-----EXCEPTION-----");
                    Debug.WriteLine(e);
                    Debug.WriteLine("--------END--------");
                    // ReSharper disable once HeuristicUnreachableCode
                    QModernMessageBox.Show("读取配置文件时发生错误\n已重置到初始值", "错误", QModernMessageBox.QModernMessageBoxButtons.Ok,
                        ModernMessageboxIcons.Warning);
                    File.Delete(currentInfo.XmlFileLocation);
                    currentInfo.XmlFile = null;
                    Load();
                    return;
                }
            }

            if (currentInfo.XmlFile == null)
            {
                modeCheck.IsChecked = false;

                //set everything to empty

                _currentColor = Colors.Black;
                _currentColorString = "black";
                defineIconCheck.IsChecked = false;
                txtWhiteColor.IsChecked = true;
                colorSelector_1.IsChecked = true;
                grdDevDefIco.Visibility = Visibility.Collapsed;
            }
            else
            {
                //modeSelctor.SelectedIndex = 1;
                modeCheck.IsChecked = true;
                largeAppNameCheck.IsChecked = currentInfo.XmlFile.ShowTitleOnLargeIcon;
                try
                {
                    //Color
                    switch (currentInfo.XmlFile.ColorStr)
                    {
                        case "black":
                            colorSelector_1.IsChecked = true;
                            break;
                        case "silver":
                            colorSelector_2.IsChecked = true;
                            break;
                        case "gray":
                            colorSelector_3.IsChecked = true;
                            break;
                        case "white":
                            colorSelector_4.IsChecked = true;
                            break;
                        case "maroon":
                            colorSelector_5.IsChecked = true;
                            break;
                        case "red":
                            colorSelector_6.IsChecked = true;
                            break;
                        case "purple":
                            colorSelector_7.IsChecked = true;
                            break;
                        case "fuchsia":
                            colorSelector_8.IsChecked = true;
                            break;
                        case "green":
                            colorSelector_9.IsChecked = true;
                            break;
                        case "lime":
                            colorSelector_10.IsChecked = true;
                            break;
                        case "olive":
                            colorSelector_11.IsChecked = true;
                            break;
                        case "yellow":
                            colorSelector_12.IsChecked = true;
                            break;
                        case "navy":
                            colorSelector_13.IsChecked = true;
                            break;
                        case "blue":
                            colorSelector_14.IsChecked = true;
                            break;
                        case "teal":
                            colorSelector_15.IsChecked = true;
                            break;
                        case "aqua":
                            colorSelector_16.IsChecked = true;
                            break;
                        default:
                            try
                            {
                                _currentColor = Helper.GetColorFromRgbString(currentInfo.XmlFile.ColorStr);
                                _currentColorString = currentInfo.XmlFile.ColorStr;
                                colorSelector_17.IsChecked = true;
                                defineColorText.Text = currentInfo.XmlFile.ColorStr;
                            }
                            catch (FormatException)
                            {
                                currentInfo.XmlFile.ColorStr = "black";
                                _currentColor = Colors.Black;
                                colorSelector_1.IsChecked = true;
                            }

                            break;
                    }

                    //Logo
                    if (string.IsNullOrEmpty(currentInfo.XmlFile.SmallLogoLoc) &&
                        !string.IsNullOrEmpty(currentInfo.XmlFile.LargeLogoLoc))
                    {
                        //replace smallLogo with largeLogo
                        currentInfo.XmlFile.SmallLogoLoc = currentInfo.XmlFile.LargeLogoLoc;
                    }

                    if (!string.IsNullOrEmpty(currentInfo.XmlFile.SmallLogoLoc))
                    {
                        void CheckLargeLoc()
                        {
                            if (string.IsNullOrEmpty(currentInfo.XmlFile.LargeLogoLoc))
                            {
                                currentInfo.XmlFile.LargeLogoLoc = currentInfo.XmlFile.SmallLogoLoc;
                            }
                        }

                        if (File.Exists(currentInfo.XmlFile.GetFullPath(currentInfo.XmlFile.SmallLogoLoc)))
                        {
                            //直接获取
                            Debug.WriteLine(
                                $"Load small icon successfully with file location{currentInfo.XmlFile.SmallLogoLoc}");
                            _logo = new BitmapImage(
                                new Uri(currentInfo.XmlFile.GetFullPath(currentInfo.XmlFile
                                    .SmallLogoLoc))); //Load the logo
                            //_scaleMode = false;
                            defineIconCheck.IsChecked = true;
                            grdDevDefIco.Visibility = Visibility.Collapsed;
                            CheckLargeLoc();
                            goto __hasLogo;
                        }
                        else if (Directory.Exists(
                                     Path.GetDirectoryName(
                                         currentInfo.XmlFile.GetFullPath(currentInfo.XmlFile.SmallLogoLoc))) &&
                                 // ReSharper disable once AssignNullToNotNullAttribute
                                 File.Exists(Path.Combine(Path.GetDirectoryName(currentInfo.TargetPath),
                                     "Resources.pri")))
                        {
                            //scale模式
                            _scaleMode = true;
                            defineIconCheck.IsChecked = true;
                            //btnChangeLogo.Visibility = Visibility.Collapsed;
                            grdDevDefIco.Visibility = Visibility.Visible;
                            CheckLargeLoc();
                            goto __hasLogo;
                        }
                        else
                        {
                            //异常，清除
                            currentInfo.XmlFile.SmallLogoLoc = currentInfo.XmlFile.LargeLogoLoc = null;
                        }
                    }

                    //_scaleMode = false;
                    defineIconCheck.IsChecked = false;
                    grdDevDefIco.Visibility = Visibility.Collapsed;
                    __hasLogo:
                    //txtColorSelector.SelectedIndex = (int)currentInfo.XmlFile.TxtForeground;
                    if (currentInfo.XmlFile.TxtForeground == StartmenuXmlFile.TextCol.light)
                        txtWhiteColor.IsChecked = true;
                    else txtBlackColor.IsChecked = true;
                }
#if DEBUG
                catch (Exception e)
                {
                    Debug.WriteLine("-----EXCEPTION-----");
                    Debug.WriteLine(e);
                    Debug.WriteLine("--------END--------");
#else
                catch {
#endif
                    // ReSharper disable once HeuristicUnreachableCode
                    QModernMessageBox.Show("读取配置文件时发生错误\n已重置到初始值", "错误", QModernMessageBox.QModernMessageBoxButtons.Ok,
                        ModernMessageboxIcons.Warning);
                    File.Delete(currentInfo.XmlFileLocation);
                    currentInfo.XmlFile = null;
                    Load();
                    return;
                }
            }

            undoBtn.IsEnabled = currentInfo.BakFileExist;
            UpdateRender();
            _sysChangeing = false;
        }

        private void UpdateRender()
        {
            Debug.WriteLine(DateTime.Now + " An Render update queue.");
            defineColorPreview.Fill = new SolidColorBrush(_currentColor);
            previewColor.Color = _currentColor;
            preview_LargeText.Foreground = (txtWhiteColor.IsChecked ?? false) ? Brushes.White : Brushes.Black;
            preview_LargeText.Visibility =
                largeAppNameCheck.IsChecked ?? false ? Visibility.Visible : Visibility.Hidden;
            if (_logo == null || !(defineIconCheck.IsChecked ?? false))
            {
                preview_SmallImg.Source = preview_LargeImg.Source = _applistData[_currentId].Logo;
                preview_SmallImg.Stretch = preview_LargeImg.Stretch = Stretch.None;
            }
            else
            {
                preview_SmallImg.Source = preview_LargeImg.Source = _logo;
                preview_SmallImg.Stretch =
                    (_logo.PixelWidth > 70 || _logo.PixelHeight > 70) ? Stretch.Fill : Stretch.None;
                preview_LargeImg.Stretch =
                    (_logo.PixelWidth > 150 || _logo.PixelHeight > 150) ? Stretch.Fill : Stretch.None;
            }

            preview_LargeText.Text = Path.GetFileNameWithoutExtension(_applistData[_currentId].FullPath);
        }

        private bool SaveCheck()
        {
            if (!_saveFlag || _currentId == -1) return true;
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (QModernMessageBox.Show("更改尚未保存，是否保存？", "提示", QModernMessageBox.QModernMessageBoxButtons.YesNoCancel,
                ModernMessageboxIcons.Info))
            {
                case ModernMessageboxResult.Button1:
                    return Save();
                case ModernMessageboxResult.Button2:
                    _saveFlag = false;
                    return true;
                default:
                    return false;
            }
        }

        private bool Save()
        {
#if DEBUG
            var stop = new Stopwatch();
            stop.Start();
#endif
            if (_currentId == -1) return true;
            var currentInfo = _applistData[_currentId];
            //todo save
            if (_saveFlag)
            {
                if (!(modeCheck.IsChecked ?? false))
                {
                    if (currentInfo.XmlFile != null)
                    {
                        if (QModernMessageBox.Show("将删除所有自定义效果文件\n继续?", "⚠警告",
                                QModernMessageBox.QModernMessageBoxButtons.YesNo, ModernMessageboxIcons.Warning) !=
                            ModernMessageboxResult.Button1) return false;
                        currentInfo.XmlFile = null;
                        File.Delete(currentInfo.XmlFileLocation);
                        if (File.Exists(currentInfo.ShadowFileLocation))
                            File.Delete(currentInfo.ShadowFileLocation);

                        _logo = null;
                    }
                }
                else
                {
                    if (currentInfo.XmlFile == null)
                    {
                        currentInfo.XmlFile = new StartmenuXmlFile(currentInfo.XmlFileLocation);
                    }

                    currentInfo.XmlFile.ColorStr = _currentColorString;
                    currentInfo.XmlFile.TxtForeground = (txtWhiteColor.IsChecked ?? false)
                        ? StartmenuXmlFile.TextCol.light
                        : StartmenuXmlFile.TextCol.dark;
                    currentInfo.XmlFile.ShowTitleOnLargeIcon = largeAppNameCheck.IsChecked ?? false;
                    //保存图片
                    var sha1 = SHA1.Create();
                    // ReSharper disable once AssignNullToNotNullAttribute
                    var pathName = currentInfo.LogoDirLocation;
                    if (!Directory.Exists(pathName))
                        Directory.CreateDirectory(pathName);

                    //文件名：[DIR]\__StartmenuIcons__\[SHA1].png
                    if (defineIconCheck.IsChecked ?? false)
                    {
                        if (!string.IsNullOrEmpty(_newLogoLoc))
                        {
                            //计算SHA1
                            var fs = File.Open(_newLogoLoc, FileMode.Open, FileAccess.Read);
                            var hash = BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", string.Empty).ToLower();
                            fs.Close();
                            var fileName = Path.Combine(pathName, hash) + Path.GetExtension(_newLogoLoc);
                            var fileNameWithoutDir = Path.Combine(Properties.Resources.IconDirName, hash) +
                                                     Path.GetExtension(_newLogoLoc).ToLower();
                            if (!File.Exists(fileName)) //拷贝图像到目录
                                File.Copy(_newLogoLoc, fileName);
                            currentInfo.XmlFile.SmallLogoLoc = currentInfo.XmlFile.LargeLogoLoc = fileNameWithoutDir;
                            _newLogoLoc = null;
                        }
                        else if (string.IsNullOrEmpty(currentInfo.XmlFile.SmallLogoLoc))
                        {
                            QModernMessageBox.Info("需要指定作为图标的图片。", "提示");
                            return false;
                        }
                    }
                    else
                    {
                        currentInfo.XmlFile.LargeLogoLoc = currentInfo.XmlFile.SmallLogoLoc = string.Empty;
                        if (_newLogoLoc == null)
                            _logo = null;
                    }

                    try
                    {
                        currentInfo.Backup();
                        currentInfo.XmlFile.Save();
                        undoBtn.IsEnabled = currentInfo.BakFileExist;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        QModernMessageBox.Error("无法保存设定.\n权限不足.", "错误");
                        return false;
                    }
                    catch (IOException ex)
                    {
                        QModernMessageBox.Error("无法读取该文件设定.\n发生了IO异常，请稍后再试\n更多信息:" + ex.Message, "错误");
                        _sysChangeing = false;
                        appList.SelectedIndex = -1;
                        return false;
                    }
                }

                _saveFlag = false;
            }

            currentInfo.ShadowBackup(); 

            foreach (var item in _applistData)
            {
                if (item.TargetPath == currentInfo.TargetPath)
                {
                    //update
                    Helper.UpdateFile(item.FullPath);
                }
            }

#if DEBUG
            stop.Stop();
            Debug.WriteLine("save takes:" + stop.Elapsed + " ms");
#endif
            return true;
        }

        #endregion
    }
}
