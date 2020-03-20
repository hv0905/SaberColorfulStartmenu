﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using SaberColorfulStartmenu.Helpers;
using File = System.IO.File;

// ReSharper disable AssignNullToNotNullAttribute

namespace SaberColorfulStartmenu.Core
{
    public class StartmenuShortcutInfo : IComparable<StartmenuShortcutInfo>
    {
        public static readonly BitmapSource Unknown =
            new BitmapImage(new Uri("WpfImages/unknown.png", UriKind.Relative));
        public const string XML_FILE_SIGN = ".visualelementsmanifest.xml";

        public string AppName { get; set; }
        public BitmapSource Logo { get; set; }
        public StartmenuXmlFile XmlFile { get; set; }
        public string XmlFileLocation { get; }
        public string FullPath { get; }
        public string TargetPath { get; }

        public string LogoDirLocation =>
            Path.Combine(Path.GetDirectoryName(XmlFileLocation), Properties.Resources.IconDirName);
        public string BakFileLocation => XmlFileLocation + ".bak";

        public string ShadowFileLocation => XmlFileLocation + ".shadow";
        public bool XmlDefined => File.Exists(XmlFileLocation);
        public bool BakFileExist => File.Exists(BakFileLocation);

        // ReSharper disable once UnusedMember.Global
        public string Ui_ToolTipTxt => $"{AppName}\n目标：{TargetPath}\n自定义：{(XmlDefined ? "是" : "否")}";

        /// <summary>
        /// 获取现有快捷方式的信息
        /// </summary>
        /// <param name="shortcutFileName"></param>
        // ReSharper disable once UnusedMember.Global
        public StartmenuShortcutInfo(string shortcutFileName)
        {
            FullPath = shortcutFileName;
            TargetPath = ShortcutHelper.ResolveShortcut(FullPath);
            //__find:
            if (!File.Exists(TargetPath)) {
                throw new FileNotFoundException(TargetPath);
            }

            XmlFileLocation =
                Path.Combine(Path.GetDirectoryName(TargetPath), Path.GetFileNameWithoutExtension(TargetPath)) +
                XML_FILE_SIGN;

            AppName = Path.GetFileNameWithoutExtension(shortcutFileName);
            // ReSharper disable once AssignNullToNotNullAttribute
            if (App.charMap_Cn.ContainsKey(AppName))
                AppName = App.charMap_Cn[AppName];
        }


        /// <summary>
        /// 获取现有快捷方式的信息，提供TargetPath
        /// </summary>
        /// <param name="shortcutFileName"></param>
        /// <param name="targetPath"></param>
        public StartmenuShortcutInfo(string shortcutFileName, string targetPath)
        {
            FullPath = shortcutFileName;
            TargetPath = targetPath;
            //__find:
            if (!File.Exists(TargetPath)) {
                throw new FileNotFoundException(TargetPath);
            }

            XmlFileLocation =
                Path.Combine(Path.GetDirectoryName(TargetPath), Path.GetFileNameWithoutExtension(TargetPath)) +
                XML_FILE_SIGN;

            AppName = Path.GetFileNameWithoutExtension(shortcutFileName);
            // ReSharper disable once AssignNullToNotNullAttribute
            if (App.charMap_Cn.ContainsKey(AppName))
                AppName = App.charMap_Cn[AppName];
        }

        /// <summary>
        /// 载入Icon
        /// </summary>
        public void LoadIcon()
        {
            var iconPath = ShortcutHelper.GetShortcutIconPath(FullPath, out var iconId);
            if (string.IsNullOrWhiteSpace(iconPath)) {
                //targetPath对应icon
                iconPath = TargetPath;
            }

            BitmapSource logo;
            if (iconPath.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase) ||
                iconPath.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase)) {
                try {
                    var tmp = Helper.GetLargeIconsFromExeFile(iconPath, iconId);
                    if (tmp != null) {
                        logo = tmp.ToBitmap().ToBitmapSource();
                        Helper.DestroyIcon(tmp.Handle);
                        tmp.Dispose();
                    }
                    else {
                        logo = Unknown;
                    }
                }
                catch {
                    //catch(NotImplementedException) {
                    logo = Unknown;
                }
            }
            else {
                //ico
                try {
                    var ico = new Icon(iconPath);
                    logo = ico.ToBitmap().ToBitmapSource();
                    ico.Dispose();
                }
                catch {
                    //catch (NotImplementedException) {
                    logo = Unknown;
                }
            }

            Logo = logo;
        }

        public void LoadXmlInfo()
        {
            if (XmlDefined) {
                XmlFile = StartmenuXmlFile.Load(XmlFileLocation);
            }
        }

        /// <summary>
        /// 备份xml文件
        /// </summary>
        public void Backup()
        {
            if (XmlDefined) {
                File.Copy(XmlFileLocation, BakFileLocation, true);
            }
        }

        /// <summary>
        /// 用bak还原xml
        /// </summary>
        public bool Undo()
        {
            if (!File.Exists(BakFileLocation)) return false;
            XmlFile = null;
            File.Copy(BakFileLocation, XmlFileLocation, true);
            File.Copy(BakFileLocation, ShadowFileLocation, true);
            File.Delete(BakFileLocation);
            return true;
        }


        public int CompareTo(StartmenuShortcutInfo other) =>
            string.Compare(AppName, other.AppName, StringComparison.CurrentCulture);
    }
}
