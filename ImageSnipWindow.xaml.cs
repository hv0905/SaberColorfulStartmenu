using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SaberColorfulStartmenu.Helpers;
using Point = System.Windows.Point;
using Size = System.Drawing.Size;

namespace SaberColorfulStartmenu
{
    /// <summary>
    /// ImageSnipWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ImageSnipWindow : Window
    {
        private bool _loaded;
        private Point? _mouseStartPoint;
        private Bitmap _currentBitMap;

        public ImageSnipWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            return;
           Close();
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void GridImg_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mouseStartPoint = null;
        }

        private void ButtonBase_OnClick_1(object sender, RoutedEventArgs e)
        {
            string str = @"E:\themes\e8a5909b36a77b3894fe45ec3421e6cc34e31057.jpg";
            _currentBitMap = new Bitmap(str);
            imgDst.Source = _currentBitMap.ToBitmapSource();
        }

        private void GridImg_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) {
                if (_mouseStartPoint.HasValue) {
                    var tmp = e.GetPosition(gridImg);
                    var newX = Canvas.GetLeft(imgDst) + tmp.X - _mouseStartPoint.Value.X;
                    if (newX > leftMask.ActualWidth) {
                        newX = leftMask.ActualWidth;
                    }
                    var newY = Canvas.GetTop(imgDst) + tmp.Y - _mouseStartPoint.Value.Y;
                    if (newY > 0) {
                        newY = 0;
                    }
                    Canvas.SetLeft(imgDst,newX);
                    Canvas.SetTop(imgDst, newY);
                    _mouseStartPoint = tmp;
                }
                else {
                    _mouseStartPoint = e.GetPosition(gridImg);
                }
            }
        }

        private void GridImg_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) {
                //放大
                imgScale.ScaleX += 0.1;


            }
            else if(e.Delta < 0) {
                //缩小
                imgScale.ScaleX -= 0.1;
                var size = GetActSize();
                if (size.Width < canvasImg.ActualWidth) {
                    imgScale.ScaleX = canvasImg.ActualWidth / _currentBitMap.Size.Width;
                }else if (size.Height < canvasImg.ActualHeight) {
                    imgScale.ScaleX = canvasImg.ActualHeight / _currentBitMap.Size.Height;
                }
            }
        }

        private Size GetActSize()
        {
            return new Size((int)(_currentBitMap.Size.Width * imgScale.ScaleX),(int)(_currentBitMap.Size.Height *imgScale.ScaleY));
        }

        private void ButtonBase_OnClick_2(object sender, RoutedEventArgs e)
        {
        }

        private void ImageSnipWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_loaded) return;
            var size = GetActSize();
            if (size.Width < canvasImg.ActualWidth) {
                imgScale.ScaleX = canvasImg.ActualWidth / _currentBitMap.Size.Width;
            }
            else if (size.Height < canvasImg.ActualHeight) {
                imgScale.ScaleX = canvasImg.ActualHeight / _currentBitMap.Size.Height;
            }
        }

        private void ImageSnipWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _loaded = true;
        }
    }
}
