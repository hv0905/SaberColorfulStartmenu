using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IWshRuntimeLibrary;
using SaberColorfulStartmenu.Helpers;
using File = System.IO.File;

// ReSharper disable AssignNullToNotNullAttribute

namespace SaberColorfulStartmenu.Core
{
    public class StartmenuShortcutInfo
    {
        public StartmenuXmlFile XmlFile { get; set; }
        public string XmlFileLocation { get; set; }
        public string Location { get; set; }
        public WshShortcut ShortcutInfo { get; set; }
        public string Target { get; set; }
        public const string XML_FILE_SIGN = ".visualelementsmanifest.xml";


        /// <summary>
        /// 获取现有快捷方式的信息
        /// </summary>
        /// <param name="shortcutFileName"></param>
        public StartmenuShortcutInfo(string shortcutFileName)
        {
            Location = shortcutFileName;
            ShortcutInfo = Helper.MainShell.CreateShortcut(Location);
            Target = Helper.ConvertEnviromentArgsInPath(ShortcutInfo.TargetPath);
            //__find:
            if (!File.Exists(Target)) {
                throw new FileNotFoundException(Target);
            }

            XmlFileLocation = Path.Combine(Path.GetDirectoryName(Target), Path.GetFileNameWithoutExtension(Target)) +
                              XML_FILE_SIGN;
            if (File.Exists(XmlFileLocation)) {
                XmlFile = StartmenuXmlFile.Load(XmlFileLocation);
            }
        }
    }
}
