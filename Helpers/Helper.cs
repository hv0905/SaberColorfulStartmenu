using System;
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
using IWshRuntimeLibrary;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using File = System.IO.File;

namespace SaberColorfulStartmenu.Helpers
{
    public static class Helper
    {
        public static readonly WshShell MainShell;

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
        /// 从exe dll中导出图标
        /// </summary>
        [DllImport("shell32.dll")]
        private static extern int ExtractIconEx(string lpszFile, int niconIndex, IntPtr[] phiconLarge,
            IntPtr[] phiconSmall, int nIcons);

        static Helper()
        {
            MainShell = new WshShell();
        }

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
        public static BitmapSource GetBitmapSourceFromBitmap(this Bitmap target)
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

        public static Icon[] GetLargeIconsFromExeFile(string exeFile)
        {
            if (string.IsNullOrEmpty(exeFile))
                throw new ArgumentException("Value cannot be null or empty.", nameof(exeFile));
            //第一步：获取程序中的图标数  
            var iconCount = ExtractIconEx(exeFile, -1, null, null, 0);
            //第二步：创建存放大/小图标的空间  
            var largeIcons = new IntPtr[iconCount];
            var smallIcons = new IntPtr[iconCount];
            //第三步：抽取所有的大小图标保存到largeIcons和smallIcons中  
            ExtractIconEx(exeFile, 0, largeIcons, smallIcons, iconCount);
            var result = largeIcons.Select(Icon.FromHandle).ToArray();
            foreach (var item in smallIcons)
            {
                DestroyIcon(item);
            }

            return result;
        }

        public static Color ToMediaColor(this System.Drawing.Color color) =>
            Color.FromArgb(color.A, color.R, color.G, color.B);


        public static string ConvertEnviromentArgsInPath(string path)
        {
            if (!path.Contains("%")) return path;
            var ps = Process.Start(new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"echo {path}\"",
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }); //使用cmd的ECHO命令进行转换
            //简单粗暴的解决方案=.=
            // ReSharper disable once PossibleNullReferenceException
            return ps.StandardOutput.ReadLine()?.Trim();
        }

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
        public static Color GetColorFromRgbString(string rgbString)
            => (Color) ColorConverter.ConvertFromString($"#FF{rgbString.Substring(1)}");


        /// <summary>
        /// 修改字符串，使其不会与Regex关键字冲突
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RegexFree(this string str)
        {
            var sb = new StringBuilder(str);
            sb.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)").Replace("[", "\\[").Replace("]", "\\]")
                .Replace(".", "\\.").Replace("*", "\\*")
                .Replace("|", "\\|").Replace("{", "\\{").Replace("}", "\\}").Replace("?", "\\?").Replace("+", "\\+");
            return sb.ToString();
        }
    }
}