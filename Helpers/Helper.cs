using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using IWshRuntimeLibrary;

namespace StartBgChanger.Helpers
{
    public static class Helper
    {

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


        private static string GetShortcutTarget(string shortcutPath)
        {
            if (System.IO.File.Exists(shortcutPath))
            {
                WshShell shell = new WshShell();
                IWshShortcut wshShortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                return wshShortcut.TargetPath;
            }
            return "";
        }


    }
}
