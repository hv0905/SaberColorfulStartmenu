
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
        private string _largeIconLoc;
        private string _smallIconLoc;
        public string ColorStr { get; set; }

        public string LargeIconLoc
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            get
            {
                if (_largeIconLoc != null)
                    return _largeIconLoc[2] != ':'
                        ? Path.Combine(Path.GetDirectoryName(FileName), _largeIconLoc)
                        : _largeIconLoc;
                else return null;
            }
            set  => _largeIconLoc = value;
        }

        public string SmallIconLoc
        {
            get
            {
                if (_smallIconLoc != null)
                    return _smallIconLoc[2] != ':'
                        // ReSharper disable once AssignNullToNotNullAttribute
                        ? Path.Combine(Path.GetDirectoryName(FileName), _smallIconLoc)
                        : _smallIconLoc;
                else return null;
            }

            set => _smallIconLoc = value;
        }

        public bool ShowTitleOnLargeIcon { get; set; }
        public TextCol TxtForeground { get; set; }


        public static StartmenuXmlFile Load(string fileName)
        {

            var sxf = new StartmenuXmlFile();
            sxf.FileName = fileName;
            //sxf._xmlFile = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite);
            sxf._doc = new XmlDocument();
            sxf._doc.Load(sxf.FileName);
            var root = sxf._doc.DocumentElement;
            // ReSharper disable once PossibleNullReferenceException
            sxf._visualElements = (XmlElement)(root.GetElementsByTagName("VisualElements")[0]);

            sxf.ColorStr = sxf._visualElements.Attributes["BackgroundColor"].Value;
            sxf.ShowTitleOnLargeIcon = sxf._visualElements.Attributes["ShowNameOnSquare150x150Logo"].Value == "on";
            sxf.TxtForeground = sxf._visualElements.Attributes["ForegroundText"].Value == "light" ? TextCol.light : TextCol.dark;
            if (sxf._visualElements.HasAttribute("Square150x150Logo"))
            {
                sxf.LargeIconLoc = sxf._visualElements.Attributes["Square150x150Logo"].Value;
            }
            if (sxf._visualElements.HasAttribute("Square70x70Logo"))
            {
                sxf.SmallIconLoc = sxf._visualElements.Attributes["Square70x70Logo"].Value;
            }
            return sxf;
        }


        public static StartmenuXmlFile LoadOrNew(string fileName)
        {
            if (File.Exists(fileName))
            {
                return Load(fileName);
            }
            else
            {
                return new StartmenuXmlFile(fileName);
            }
        }

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


            _doc.AppendChild(_doc.CreateComment(
                $"本文件由{Properties.Resources.AppName}创建。不要修改或删除本文件否则可能导致开始菜单自定义效果丢失。如需修改开始菜单自定义效果，请使用{Properties.Resources.AppName}。"));
            _doc.Save(FileName);
            //_xmlFile.Flush();
        }

        private StartmenuXmlFile() { }

        /// <summary>
        /// 将更改保存到<see cref="FileName"/>中
        /// </summary>
        public void Save()
        {
            _visualElements.Attributes["BackgroundColor"].Value = ColorStr;
            _visualElements.Attributes["ShowNameOnSquare150x150Logo"].Value = ShowTitleOnLargeIcon ? "on" : "off";
            _visualElements.Attributes["ForegroundText"].Value = TxtForeground.ToString();
            //可选项
            if (_visualElements.HasAttribute("Square150x150Logo"))
            {
                if (string.IsNullOrEmpty(LargeIconLoc))
                {
                    _visualElements.RemoveAttribute("Square150x150Logo");
                }
                else
                {
                    _visualElements.Attributes["Square150x150Logo"].Value = _largeIconLoc;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(LargeIconLoc))
                {
                    _visualElements.Attributes.Append(_doc.CreateAttribute("Square150x150Logo"));
                    _visualElements.Attributes["Square150x150Logo"].Value = _largeIconLoc;
                }
            }
            if (_visualElements.HasAttribute("Square70x70Logo"))
            {
                if (string.IsNullOrEmpty(SmallIconLoc))
                {
                    _visualElements.RemoveAttribute("Square70x70Logo");
                }
                else
                {
                    _visualElements.Attributes["Square70x70Logo"].Value = _smallIconLoc;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(SmallIconLoc))
                {
                    _visualElements.Attributes.Append(_doc.CreateAttribute("Square70x70Logo"));
                    _visualElements.Attributes["Square70x70Logo"].Value = _smallIconLoc;
                }
            }

            _doc.Save(FileName);
            //_xmlFile.Flush();
        }

        /// <summary>
        ///指定文字颜色 
        /// </summary>
        public enum TextCol : int
        {
            // ReSharper disable once InconsistentNaming
            light = 0,
            // ReSharper disable once InconsistentNaming
            dark = 1,
        }

    }
}
