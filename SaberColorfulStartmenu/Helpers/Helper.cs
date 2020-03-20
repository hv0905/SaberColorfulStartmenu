﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using File = System.IO.File;

namespace SaberColorfulStartmenu.Helpers
{
    public static class Helper
    {
        /// <summary>
        /// 通过句柄销毁HBitmap
        /// </summary>
        /// <param name="hObject">句柄</param>
        /// <returns>是否成功删除</returns>
        [DllImport("gdi32", EntryPoint = "DeleteObject")]
        public static extern bool DeleteHBitmap(IntPtr hObject);

        /// <summary>
        /// 通过句柄销毁一个Icon图标
        /// </summary>
        /// <param name="icon">句柄</param>
        /// <returns>是否成功删除</returns>
        [DllImport("user32.dll")]
        public static extern bool DestroyIcon(IntPtr icon);

        /// <summary>
        /// 从dll exe中导出图标
        /// </summary>
        /// <param name="hInst">IntPtr.Zero</param>
        /// <param name="pszExeFileName">exe、dll文件</param>
        /// <param name="nIconIndex">图标序号</param>
        /// <returns>若图标不存在返回IntPtr.Zero</returns>
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr ExtractIcon(IntPtr hInst, string pszExeFileName, int nIconIndex);

        /// <summary>
        /// 遍历一个目录，获取内部所有子文件夹的所有文件
        /// </summary>
        /// <param name="dirPath">目录str</param>
        /// <returns></returns>
        public static string[] GetAllFilesByDir(string dirPath)
        {
            var dirs = new List<string> {dirPath};
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < dirs.Count; i++)
            {
                try
                {
                    dirs.AddRange(Directory.GetDirectories(dirs[i]));
                }
                catch
                {
                    // ignored
                }
            }

            var files = new List<string>();
            foreach (var itemDir in dirs)
            {
                try
                {
                    files.AddRange(Directory.GetFiles(itemDir));
                }
                catch
                {
                    // ignored
                }
            }

            return files.ToArray();
        }

        /// <summary>
        /// 转换为BitmapSource
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static BitmapSource ToBitmapSource(this Bitmap target)
        {
            var hbitmap = target.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteHBitmap(hbitmap);
            }
        }

        /// <summary>
        /// 获取所有图标
        /// </summary>
        /// <param name="exeFile"></param>
        /// <returns></returns>
        public static Icon[] GetLargeIconsFromExeFile(string exeFile)
        {
            if (string.IsNullOrEmpty(exeFile))
                throw new ArgumentException("Value cannot be null or empty.", nameof(exeFile));
            var iconCount = ExtractIcon(IntPtr.Zero, exeFile, -1).ToInt32();
            var icons = new IntPtr[iconCount];
            for (var i = 0; i < iconCount; i++)
            {
                icons[i] = ExtractIcon(IntPtr.Zero, exeFile, i);
            }

            return icons.Select(Icon.FromHandle).ToArray();
        }

        /// <summary>
        /// 根据图标索引获取图标
        /// 本方法效率较高
        /// </summary>
        /// <param name="exeFile"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Icon GetLargeIconsFromExeFile(string exeFile, int id)
        {
            if (string.IsNullOrEmpty(exeFile))
                throw new ArgumentException("Value cannot be null or empty.", nameof(exeFile));
            var hwnd = ExtractIcon(IntPtr.Zero, exeFile, id);
            if (hwnd != IntPtr.Zero)
            {
                return Icon.FromHandle(hwnd);
            }
            else return null;
        }

        public static Color ToMediaColor(this System.Drawing.Color color) =>
            Color.FromArgb(color.A, color.R, color.G, color.B);

        /// <summary>
        /// 强制更新文件
        /// </summary>
        public static void UpdateFile(string path)
        {
            if (File.Exists(path))
            {
                File.SetLastWriteTime(path, DateTime.Now);
            }
        }

        public static string ToRgbString(this Color argbColor) =>
            "#" + argbColor.ToString().Substring(3);

        // ReSharper disable once PossibleNullReferenceException
        public static Color GetColorFromRgbString(string rgbString) =>
            (Color)ColorConverter.ConvertFromString($"#FF{rgbString.Substring(1)}");


        /// <summary>
        /// 修改字符串，使其不会与Regex关键字冲突
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RegexFree(this string str)
        {
            var sb = new StringBuilder(str);
            sb.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)").Replace("[", "\\[").Replace("]", "\\]")
                .Replace(".", "\\.").Replace("*", "\\*").Replace("|", "\\|").Replace("{", "\\{").Replace("}", "\\}")
                .Replace("?", "\\?").Replace("+", "\\+");
            return sb.ToString();
        }
    }
}
