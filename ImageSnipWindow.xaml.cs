using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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
using Rectangle = System.Drawing.Rectangle;
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

        public SnapWindowResult Result { get; private set; } = SnapWindowResult.Unknown;

        public Bitmap Source { get; }

        public Bitmap Dst { get; private set; }

        public ImageSnipWindow(Bitmap src)
        {
            Source = src;
            InitializeComponent();
            imgDst.Source = Source.ToBitmapSource();
            Scale(0);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            // X Y Width = Height
            var canvX = Canvas.GetLeft(imgDst);
            var canvY = Canvas.GetTop(imgDst);

            var imgX = (leftMask.ActualWidth - canvX) / imgScale.ScaleX;
            var imgY = (0 - canvY) / imgScale.ScaleY;

            var width = centerMask.ActualWidth / imgScale.ScaleX;
            var rect = new Rectangle((int)(imgX + 0.5), (int)(imgY + 0.5),(int)(width + 0.5),(int)(width + 0.5));
            Debug.WriteLine($"imgX:{imgX}  imgY:{imgY} width:{width} format:{Source.PixelFormat}");
            Dst = Source.Clone(rect, Source.PixelFormat);
            Result = SnapWindowResult.Ok;
            Close();
        }

        private void GridImg_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => _mouseStartPoint = null;

        private void ButtonBase_OnClick_1(object sender, RoutedEventArgs e)
        {
            Result = SnapWindowResult.Cancel;
            Close();
        }

        private void GridImg_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (_mouseStartPoint.HasValue) {
                var pos = e.GetPosition(gridImg);

                var newX = Canvas.GetLeft(imgDst) + pos.X - _mouseStartPoint.Value.X;
                var newY = Canvas.GetTop(imgDst) + pos.Y - _mouseStartPoint.Value.Y;
                Offset(newX, newY);
                _mouseStartPoint = pos;
            }
            else {
                _mouseStartPoint = e.GetPosition(gridImg);
            }
        }

        private void GridImg_OnMouseWheel(object sender, MouseWheelEventArgs e) => Scale(e.Delta);


        public void Scale(int ratio)
        {
            if (ratio > 0) {
                //放大
                imgScale.ScaleX += 0.1;
            }
            else {
                //缩小
                if (ratio < 0)
                    imgScale.ScaleX -= 0.1;

                var size = GetRealSize();
                if (size.Width < centerMask.ActualWidth) {
                    imgScale.ScaleX = canvasImg.ActualWidth / Source.Size.Width;
                }
                else if (size.Height < canvasImg.ActualHeight) {
                    imgScale.ScaleX = canvasImg.ActualHeight / Source.Size.Height;
                }

                Offset(Canvas.GetLeft(imgDst), Canvas.GetTop(imgDst));
            }
        }

        public void Offset(double x, double y)
        {
            var siz = GetRealSize();
            var newX = x;
            var newY = y;

            //左
            if (newX > leftMask.ActualWidth) {
                newX = leftMask.ActualWidth;
            }

            //上
            if (newY > 0) {
                newY = 0;
            }

            //右
            if ((newX + siz.Width) < leftMask.ActualWidth + centerMask.ActualWidth) {
                newX = leftMask.ActualWidth + centerMask.ActualWidth - siz.Width;
            }

            //下
            if (newY + siz.Height < centerMask.ActualHeight) {
                newY = centerMask.ActualHeight - siz.Height;
            }

            Canvas.SetLeft(imgDst, newX);
            Canvas.SetTop(imgDst, newY);
        }


        private Size GetRealSize() => new Size((int)(Source.Size.Width * imgScale.ScaleX),
            (int)(Source.Size.Height * imgScale.ScaleY));

        private void ButtonBase_OnClick_2(object sender, RoutedEventArgs e) => Scale(1);

        private void ImageSnipWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_loaded) return;

            if (Width < Height * 1.2) {
                Width = Height * 1.2;
            }

            Scale(0);
        }

        private void ImageSnipWindow_OnLoaded(object sender, RoutedEventArgs e) => _loaded = true;

        private void GridImg_OnMouseLeave(object sender, MouseEventArgs e) => _mouseStartPoint = null;

        private void ButtonBase_OnClick_3(object sender, RoutedEventArgs e) => Scale(-1);

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();


        private void ButtonBase_OnClick_4(object sender, RoutedEventArgs e)
        {
            Result = SnapWindowResult.Ignore;
            Close();
        }


        public enum SnapWindowResult
        {
            Unknown,
            Ok,
            Cancel,
            Ignore
        }
    }
}
