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
        public string XmlFileLocation { get;}
        public string Location { get; }
        public WshShortcut ShortcutInfo { get; }
        public string Target { get;  }
        public const string XML_FILE_SIGN = ".visualelementsmanifest.xml";

        public string LogoDirLocation => Path.Combine(Path.GetDirectoryName(XmlFileLocation),
            Properties.Resources.IconDirName);


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
