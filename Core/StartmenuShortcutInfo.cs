using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace StartBgChanger.Core
{
    public class StartmenuShortcutInfo
    {
        public StartmenuXmlFile XmlFile { get; set; }
        public string XMLFileLocation { get; set; }
        public string Location { get; set; }
        public Icon MainIcon { get; set; }
        public WshShortcut ShortcutInfo { get; set; }
        public static readonly string XML_FILE_NAME= ".visualelementsmanifest.xml";

        public StartmenuShortcutInfo(string shortcutFileName)
        {
            Location = shortcutFileName;
            ShortcutInfo = Helpers.Helper.mainShell.CreateShortcut(shortcutFileName);
            XMLFileLocation = Path.Combine(Path.GetDirectoryName(shortcutFileName),
                                  Path.GetFileNameWithoutExtension(shortcutFileName)) + XML_FILE_NAME;
            if (File.Exists(XMLFileLocation))
            {
                XmlFile = StartmenuXmlFile.Load(XMLFileLocation);
            }

        }
        
    }
}
