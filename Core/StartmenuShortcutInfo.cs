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
        public const string XML_FILE_SIGN= ".visualelementsmanifest.xml";

        public StartmenuShortcutInfo(string shortcutFileName)
        {
            Location = shortcutFileName;
            ShortcutInfo = Helper.MainShell.CreateShortcut(shortcutFileName);
            Target = Helper.ConvertEnviromentArgsInPath(ShortcutInfo.TargetPath);
            __find:
            if (!File.Exists(Target))
            {
                if (Target.ToLower().Contains("program files (x86)"))
                {
                    //Reason
                    //实测有部分应用（这包括Microsoft Office） 的快捷方式在使用任何一种Wshshell（这包括C# 的WshShortcut和C++的shlobj.h）时
                    //Program Files 都有几率变为 Program Files (x86) 暂时不了解原因，网上也没有相关的错误报告
                    //这种临时的解决方式，只能算是一种下下策了吧 =。=
                    //如果有知道解决方案的可以当issue汇报
                    //阿里嘎多
                    Target = Target.ToLower().Replace("program files (x86)", "program files");
                    goto __find;
                }
                else
                {
                    throw new FileNotFoundException(Target);
                }
            }
            XmlFileLocation = Path.Combine(Path.GetDirectoryName(Target),Path.GetFileNameWithoutExtension(Target)) + XML_FILE_SIGN;
            if (File.Exists(XmlFileLocation))
            {
                XmlFile = StartmenuXmlFile.Load(XmlFileLocation);
            }
        }

        public string GetExistIconFile()
        {
            throw new NotImplementedException();
        }
        
        
    }
}
