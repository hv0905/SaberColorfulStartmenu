using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IWshRuntimeLibrary;
using StartBgChanger.Helpers;
using File = System.IO.File;
// ReSharper disable AssignNullToNotNullAttribute

namespace StartBgChanger.Core
{
    public class StartmenuShortcutInfo
    {
        public StartmenuXmlFile XmlFile { get; set; }
        public string XMLFileLocation { get; set; }
        public string Location { get; set; }
        public WshShortcut ShortcutInfo { get; set; }
        public string Target { get; set; }
        public static readonly string XML_FILE_NAME= ".visualelementsmanifest.xml";

        public StartmenuShortcutInfo(string shortcutFileName)
        {
            Location = shortcutFileName;
            ShortcutInfo = Helper.MainShell.CreateShortcut(shortcutFileName);
            Target = Helper.GetPathWithPathWithEnvimentArgs(ShortcutInfo.TargetPath);
            __find:
            if (!File.Exists(Target))
            {
                if (Target.ToLower().Contains("program files (x86)"))
                {
                    Target = Target.ToLower().Replace("program files (x86)", "program files");
                    goto __find;
                }
                else
                {
                    throw new FileNotFoundException(Target);
                }
            }
            XMLFileLocation = Path.Combine(Path.GetDirectoryName(Target),Path.GetFileNameWithoutExtension(Target)) + XML_FILE_NAME;
            if (File.Exists(XMLFileLocation))
            {
                XmlFile = StartmenuXmlFile.Load(XMLFileLocation);
            }
        }
        
    }
}
