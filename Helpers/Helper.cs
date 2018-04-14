using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using IWshRuntimeLibrary;

namespace StartBgChanger.Helpers
{
    public static class Helper
    {

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        [DllImport("shell32.dll")]
        private static extern int ExtractIconEx(string lpszFile, int niconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);

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


        public static string GetShortcutTarget(string shortcutPath)
        {
            if (System.IO.File.Exists(shortcutPath))
            {
                WshShell shell = new WshShell();
                IWshShortcut wshShortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                return wshShortcut.TargetPath;

            }
            return "";
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


    }
}
