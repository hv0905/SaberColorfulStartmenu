using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SaberColorfulStartmenu.Core
{
    public class StartmenuXmlFile
    {
        public string FileName { get; private set; }
        private XmlDocument _doc;
        //private FileStream _xmlFile;
        private XmlElement _visualElements;
        public string ColorStr { get; set; }

        public string LargeLogoLoc { get; set; }

        public string SmallLogoLoc { get; set; }

        public bool ShowTitleOnLargeIcon { get; set; }
        public TextCol TxtForeground { get; set; }


        public static StartmenuXmlFile Load(string fileName)
        {
            var sxf = new StartmenuXmlFile {
                FileName = fileName,
                _doc = new XmlDocument()
            };
            //sxf._xmlFile = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite);
            sxf._doc.Load(sxf.FileName);
            var root = sxf._doc.DocumentElement;
            // ReSharper disable once PossibleNullReferenceException
            sxf._visualElements = (XmlElement)(root.GetElementsByTagName("VisualElements")[0]);

            sxf.ColorStr = sxf._visualElements.Attributes["BackgroundColor"].Value;
            sxf.ShowTitleOnLargeIcon = sxf._visualElements.Attributes["ShowNameOnSquare150x150Logo"].Value == "on";
            sxf.TxtForeground = sxf._visualElements.Attributes["ForegroundText"].Value == "light"
                ? TextCol.light
                : TextCol.dark;
            if (sxf._visualElements.HasAttribute("Square150x150Logo")) {
                sxf.LargeLogoLoc = sxf._visualElements.Attributes["Square150x150Logo"].Value;
            }

            if (sxf._visualElements.HasAttribute("Square70x70Logo")) {
                sxf.SmallLogoLoc = sxf._visualElements.Attributes["Square70x70Logo"].Value;
            }

            return sxf;
        }


        public static StartmenuXmlFile LoadOrNew(string fileName) =>
            File.Exists(fileName) ? Load(fileName) : new StartmenuXmlFile(fileName);

        /// <summary>
        /// Create a new StartmenuXmlFile
        /// Warning:If the loc has an exist file,it will be cover when <see cref="Save"/>
        /// </summary>
        /// <param name="fileName"></param>
        public StartmenuXmlFile(string fileName)
        {
            FileName = fileName;
            //_xmlFile = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite);
            _doc = new XmlDocument();
            _doc.AppendChild(_doc.CreateXmlDeclaration("1.0", "utf-8", null));

            var root = _doc.CreateElement("Application");
            root.Attributes.Append(_doc.CreateAttribute("xmlns:xsi"));
            root.Attributes["xmlns:xsi"].Value = "http://www.w3.org/2001/XMLSchema-instance";
            _visualElements = _doc.CreateElement("VisualElements");
            _visualElements.Attributes.Append(_doc.CreateAttribute("BackgroundColor"));
            _visualElements.Attributes.Append(_doc.CreateAttribute("ForegroundText"));
            _visualElements.Attributes.Append(_doc.CreateAttribute("ShowNameOnSquare150x150Logo"));
            root.AppendChild(_visualElements);
            _doc.AppendChild(root);


            _doc.AppendChild(_doc.CreateComment(string.Format("本文件由{0}创建。不要修改或删除本文件否则可能导致不正确的行为。如需修改开始菜单自定义效果，请使用{0}。",
                Properties.Resources.AppName)));
            _doc.Save(FileName);
            //_xmlFile.Flush();
        }

        private StartmenuXmlFile() { }

        /// <summary>
        /// 将更改保存到<see cref="FileName"/>中
        /// </summary>
        public void Save()
        {
            Backup();

            _visualElements.Attributes["BackgroundColor"].Value = ColorStr;
            _visualElements.Attributes["ShowNameOnSquare150x150Logo"].Value = ShowTitleOnLargeIcon ? "on" : "off";
            _visualElements.Attributes["ForegroundText"].Value = TxtForeground.ToString();
            //可选项
            if (_visualElements.HasAttribute("Square150x150Logo")) {
                if (string.IsNullOrEmpty(LargeLogoLoc)) {
                    _visualElements.RemoveAttribute("Square150x150Logo");
                }
                else {
                    _visualElements.Attributes["Square150x150Logo"].Value = LargeLogoLoc;
                }
            }
            else {
                if (!string.IsNullOrEmpty(LargeLogoLoc)) {
                    _visualElements.Attributes.Append(_doc.CreateAttribute("Square150x150Logo"));
                    _visualElements.Attributes["Square150x150Logo"].Value = LargeLogoLoc;
                }
            }

            if (_visualElements.HasAttribute("Square70x70Logo")) {
                if (string.IsNullOrEmpty(SmallLogoLoc)) {
                    _visualElements.RemoveAttribute("Square70x70Logo");
                }
                else {
                    _visualElements.Attributes["Square70x70Logo"].Value = SmallLogoLoc;
                }
            }
            else {
                if (!string.IsNullOrEmpty(SmallLogoLoc)) {
                    _visualElements.Attributes.Append(_doc.CreateAttribute("Square70x70Logo"));
                    _visualElements.Attributes["Square70x70Logo"].Value = SmallLogoLoc;
                }
            }

            _doc.Save(FileName);
            //_xmlFile.Flush();
        }

        public string GetFullPath(string loc) => loc[2] != ':' //不是磁盘
            // ReSharper disable once AssignNullToNotNullAttribute
            ? Path.Combine(Path.GetDirectoryName(FileName), loc)
            : loc;

        /// <summary>
        /// 备份xml文件
        /// </summary>
        public void Backup()
        {
            if (File.Exists(FileName)) {
                File.Copy(FileName, FileName + ".bak", true);
            }
        }


        /// <summary>
        ///指定文字颜色 
        /// </summary>
        public enum TextCol
        {
            // ReSharper disable once InconsistentNaming
            light = 0,
            // ReSharper disable once InconsistentNaming
            dark = 1,
        }
    }
}
