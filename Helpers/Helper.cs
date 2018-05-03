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
using IWshRuntimeLibrary;
using Color = System.Windows.Media.Color;
using File = System.IO.File;

namespace StartBgChanger.Helpers
{
    public static class Helper
    {
        public static readonly WshShell MainShell;
        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        [DllImport("shell32.dll")]
        private static extern int ExtractIconEx(string lpszFile, int niconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);

        static Helper()
        {
            MainShell = new WshShell();
        }

        public static string[] GetAllFilesByDir(string dirPath)
        {
            var dirs = new List<string> { dirPath };
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

        public static BitmapSource GetBitmapSourceFromBitmap(this Bitmap target)
        {
            IntPtr hbitmap = target.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hbitmap);
            }
        }

        public static Icon[] GetLargeIconsFromExeFile(string exeFile)
        {
            //第一步：获取程序中的图标数  
            var iconCount = ExtractIconEx(exeFile, -1, null, null, 0);
            //第二步：创建存放大/小图标的空间  
            var largeIcons = new IntPtr[iconCount];
            var smallIcons = new IntPtr[iconCount];
            //第三步：抽取所有的大小图标保存到largeIcons和smallIcons中  
            ExtractIconEx(exeFile, 0, largeIcons, smallIcons, iconCount);

            return largeIcons.Select(Icon.FromHandle).ToArray();
        }

        public static Color ToMediaColor(this System.Drawing.Color color) => 
            Color.FromArgb(color.A, color.R, color.G, color.B);



        public static string GetPathWithPathWithEnvimentArgs(string path)
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
            });
            // ReSharper disable once PossibleNullReferenceException
            return ps.StandardOutput.ReadLine().Trim();
        }

        public static void UpdateFile(string path)
        {
            if (File.Exists(path))
            {
                File.SetLastWriteTime(path,DateTime.Now);
            }
        }
    }
}
