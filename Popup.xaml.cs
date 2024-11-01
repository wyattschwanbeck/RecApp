﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RecApp
{
    /// <summary>
    /// Interaction logic for Popup.xaml
    /// </summary>
    public partial class Popup : Window
    {
        readonly System.Drawing.Rectangle screenRectangle;
        private Point startPoint;
        private Rectangle rect;
        private Rectangle overlayShape;
        private double xRet;
        private double yRet;
        private bool setOverlay;
        public delegate void AreaSelectedEventHandler(object source, CustomEventArgs.ScreenAreaEventArgs args);
        public delegate void OverlaySelectedEventHandler(object source, CustomEventArgs.OverlayEventArgs args);
        private int ScreenIndex;

        public event AreaSelectedEventHandler AreaSelected;
        public event OverlaySelectedEventHandler OverlaySelected;
        
        public Popup( System.Windows.Forms.Screen screen, int ScreenIndex, bool overlay=false)
        {
            this.setOverlay = overlay;
            this.ScreenIndex = ScreenIndex;
            screenRectangle = screen.Bounds;
            
            InitializeComponent();
            this.CaptureSectionImage.Opacity = 50;
            //instructionTxt.SetBinding(TextBlock.TextProperty, myBinding);

        }




        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var wih = new WindowInteropHelper(this);
            IntPtr hWnd = wih.Handle;
            MoveWindow(hWnd, screenRectangle.Left, screenRectangle.Top, screenRectangle.Width, screenRectangle.Height, false);
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(canvas);
            if (!AreaSelectedAlready)
            {


                rect = new Rectangle
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = 2,

                };
                //SolidColorBrush colorBrush = new SolidColorBrush(Colors.LightGray);
                //colorBrush.Opacity = 0.50;
                //Color.FromArgb(0xff, 0x8a, 0x8a, 0x8a));
                //canvas.Background = colorBrush;
                this.RectGeometry.Rect = new Rect(new Point(startPoint.X, startPoint.X), new Point(startPoint.Y, startPoint.Y));
                Canvas.SetLeft(rect, startPoint.X);
                Canvas.SetTop(rect, startPoint.Y);
                canvas.Children.Add(rect);
            }
            else
            {


                overlayShape= new Rectangle
                {
                    Stroke = Brushes.Blue,
                    StrokeThickness = 1.5,

                };
                this.OverlayRect.Rect = new Rect(new Point(startPoint.X, startPoint.X), new Point(startPoint.Y, startPoint.Y));
                Canvas.SetLeft(overlayShape, startPoint.X);
                Canvas.SetTop(overlayShape, startPoint.Y);
                canvas.Children.Add(overlayShape);
            }
            
            
            
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released || rect == null)
                return;

            var pos = e.GetPosition(canvas);

            var x = Math.Min(pos.X, startPoint.X);
            var y = Math.Min(pos.Y, startPoint.Y);

            var w = Math.Max(pos.X, startPoint.X) - x;
            var h = Math.Max(pos.Y, startPoint.Y) - y;

            xRet = x;
            yRet = y;
            if (!AreaSelectedAlready)
            {
                rect.Width = w;
                rect.Height = h;
                this.RectGeometry.Rect = new Rect(new Point(x, y), new Point(x + w, y + h));
                
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
            }
                
            else if (overlayShape!=null)
            {
                overlayShape.Width = w;
                overlayShape.Height = h;

                    
                
                this.OverlayRect.Rect = new Rect(new Point(x,y), new Point(Math.Max(this.RectGeometry.Rect.Right, x+w) , Math.Min(this.RectGeometry.Rect.Bottom,y+h)));
                //this.OverlayRect.SetValue(LeftProperty, x);
                //this.OverlayRect.SetValue(TopProperty, y);
                //Console.WriteLine($"Width:{w}, Height:{h}");
                //this.OverlayRect.= 80;
                Canvas.SetLeft(overlayShape, x);
                Canvas.SetTop(overlayShape, y);
            }
            
        }
        private bool AreaSelectedAlready=false;
        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
            if (!AreaSelectedAlready)
            {
                OnAreaSelected();
                
                
                if (!setOverlay)
                {
                    rect = null;
                    this.RectGeometry.Rect = new Rect(new Point(0, 0), new Point(0, 0));
                }
                    
            } else
            {
                OnOverlayAreaSelected();
                overlayShape = null;
                this.OverlayRect.Rect = new Rect(new Point(0, 0), new Point(0, 0));
            }

            AreaSelectedAlready = true;

        }
        protected virtual void OnAreaSelected()
        {
            if (AreaSelected != null)
            {
                CustomEventArgs.ScreenAreaEventArgs sae = new CustomEventArgs.ScreenAreaEventArgs { ScreenNum=ScreenIndex,Height = rect.Height, Width = rect.Width, Left = xRet, Top = yRet};
                AreaSelected(this,sae);
                
            }
            if (!setOverlay)
                this.Close();
            else
            {
                this.instructionTxt.Text = "Select the area for the overlay";
            }
        }
        protected virtual void OnOverlayAreaSelected()
        {
            if (OverlaySelected != null)
            {
                CustomEventArgs.OverlayEventArgs oae = new CustomEventArgs.OverlayEventArgs{ Height = overlayShape.Height, Width = overlayShape.Width, Left = xRet, Top = yRet };
                OverlaySelected(this, oae);
            }
            //if (setOverlay)
            this.Close();
        }

        }
}
