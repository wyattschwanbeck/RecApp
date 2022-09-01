using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecApp
{

    public class BoundingBox
    {
        enum SelectCircle
        {
            TopLeftCorner,
            TopRightCorner,
            BottomLeftCorner,
            BottomRightCorner,
            TopMiddle,
            BottomMiddle,
            RightMiddle,
            LeftMiddle,
            None
        }
        public Point startPoint;
        public Point endPoint;
        public Rectangle boundingBox;
        private Pen selectionPen = new Pen(Color.FromArgb(255, 72, 145, 220));
        private Brush selectionBrush = new SolidBrush(Color.FromArgb(255, 72, 145, 220));
        private SelectCircle selectedResizeCircle;
        Rectangle BottomRightCorner;
        Rectangle BottomLeftCorner;
        Rectangle BottomMiddle;
        Rectangle RightMiddle;
        Rectangle LeftMiddle;
        Rectangle TopLeftCorner;
        Rectangle TopRightCorner;
        Rectangle TopMiddle;
        public string label { get; set; }
        public BoundingBox(Point point1, Point point2, string label)
        {
            this.label = label;
            if (point1.X < point2.X)
            {
                this.startPoint = point1;
                this.endPoint = point2;
            }
            else
            {
                this.startPoint = point2;
                this.endPoint = point1;
            }
            boundingBox = new Rectangle(startPoint, new Size(Math.Abs(endPoint.X - startPoint.X), Math.Abs(endPoint.Y - startPoint.Y)));
            selectedResizeCircle = SelectCircle.BottomRightCorner;
        }
        private Point clickOffset;
        public void setSelectedCircles(Point point)
        {
            if (BottomRightCorner.Contains(point))
                selectedResizeCircle = SelectCircle.BottomRightCorner;
            else if (BottomLeftCorner.Contains(point))
                selectedResizeCircle = SelectCircle.BottomLeftCorner;
            else if (BottomMiddle.Contains(point))
                selectedResizeCircle = SelectCircle.BottomMiddle;
            else if (RightMiddle.Contains(point))
                selectedResizeCircle = SelectCircle.RightMiddle;
            else if (LeftMiddle.Contains(point))
                selectedResizeCircle = SelectCircle.LeftMiddle;
            else if (TopLeftCorner.Contains(point))
                selectedResizeCircle = SelectCircle.TopLeftCorner;
            else if (TopRightCorner.Contains(point))
                selectedResizeCircle = SelectCircle.TopRightCorner;
            else if (TopMiddle.Contains(point))
                selectedResizeCircle = SelectCircle.TopMiddle;
            else
            {
                clickOffset = new Point(point.X - boundingBox.X, point.Y - boundingBox.Y);
                selectedResizeCircle = SelectCircle.None;
            }

        }
        public void UpdateEndPoint(Point tempEndPoint)
        {
            if (selectedResizeCircle == SelectCircle.BottomRightCorner)
            {
                boundingBox.Size = new Size(
                    Math.Abs(startPoint.X - Math.Max(tempEndPoint.X, startPoint.X + 10)),
                    Math.Abs(startPoint.Y - Math.Max(tempEndPoint.Y, startPoint.Y + 10)));
                endPoint = new Point(tempEndPoint.X, tempEndPoint.Y);
            }
            else if (selectedResizeCircle == SelectCircle.BottomLeftCorner)
            {
                boundingBox.Location = new Point(
                    Math.Min(tempEndPoint.X, endPoint.X),
                    startPoint.Y);

                startPoint = boundingBox.Location;

                endPoint = new Point(endPoint.X, Math.Max(boundingBox.Location.Y, tempEndPoint.Y));

                boundingBox.Size = new Size(Math.Max(10, endPoint.X - boundingBox.Location.X), Math.Max(10, endPoint.Y - boundingBox.Location.Y));
            }
            else if (selectedResizeCircle == SelectCircle.TopLeftCorner)
            {
                boundingBox.Location = new Point(
                    Math.Min(tempEndPoint.X, endPoint.X),
                    Math.Min(tempEndPoint.Y, endPoint.Y));

                startPoint = boundingBox.Location;

                boundingBox.Size = new Size(Math.Max(10, endPoint.X - boundingBox.Location.X), Math.Max(10, endPoint.Y - boundingBox.Location.Y));
            }
            else if (selectedResizeCircle == SelectCircle.TopRightCorner)
            {
                startPoint = new Point(startPoint.X, tempEndPoint.Y);
                endPoint = new Point(tempEndPoint.X, endPoint.Y);
                boundingBox.Location = startPoint;
                boundingBox.Size = new Size(
                     Math.Abs(startPoint.X - Math.Max(tempEndPoint.X, startPoint.X + 10)),
                     Math.Abs(startPoint.Y - Math.Max(tempEndPoint.Y, endPoint.Y + 10)));

            }
            else if (selectedResizeCircle == SelectCircle.None)
            {

                boundingBox.Location = new Point(tempEndPoint.X - clickOffset.X, tempEndPoint.Y - clickOffset.Y);
                startPoint = boundingBox.Location;
                endPoint = new Point(startPoint.X + boundingBox.Width, startPoint.Y + boundingBox.Height);
            }
            BottomRightCorner = new Rectangle(boundingBox.X + boundingBox.Width - 10, boundingBox.Y + boundingBox.Height - 10, 10, 10);
            BottomLeftCorner = new Rectangle(boundingBox.X, boundingBox.Y + boundingBox.Height - 10, 10, 10);
            //BottomMiddle = new Rectangle(boundingBox.X + (boundingBox.Width / 2) - 5, boundingBox.Y + boundingBox.Height - 10, 10, 10);
            //RightMiddle = new Rectangle(boundingBox.X + boundingBox.Width - 10, boundingBox.Y + (boundingBox.Height / 2) - 5, 10, 10);
            //LeftMiddle = new Rectangle(boundingBox.X, boundingBox.Y + (boundingBox.Height/2) - 5, 10, 10);
            TopLeftCorner = new Rectangle(boundingBox.X, boundingBox.Y, 10, 10);
            TopRightCorner = new Rectangle(boundingBox.X + boundingBox.Width - 10, boundingBox.Y, 10, 10);
            //TopMiddle = new Rectangle(boundingBox.X + (boundingBox.Width / 2) - 5, boundingBox.Y, 10, 10);
        }

        public void DrawSelectCircles(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (selectedResizeCircle == SelectCircle.BottomRightCorner)
                e.Graphics.FillEllipse(selectionBrush, BottomRightCorner);
            else
            {
                e.Graphics.DrawEllipse(selectionPen, BottomRightCorner);
            }
            if (selectedResizeCircle == SelectCircle.BottomLeftCorner)
                e.Graphics.FillEllipse(selectionBrush, BottomLeftCorner);
            else
            {
                e.Graphics.DrawEllipse(selectionPen, BottomLeftCorner);
            }

            //if (selectedResizeCircle == SelectCircle.BottomMiddle)
            //    e.Graphics.FillEllipse(selectionBrush, BottomMiddle);
            //else
            //{
            //    e.Graphics.DrawEllipse(selectionPen, BottomMiddle);
            //}
            //if (selectedResizeCircle == SelectCircle.RightMiddle)
            //    e.Graphics.FillEllipse(selectionBrush, RightMiddle);
            //else
            //{
            //    e.Graphics.DrawEllipse(selectionPen, RightMiddle);
            //}
            //if (selectedResizeCircle == SelectCircle.LeftMiddle)
            //    e.Graphics.FillEllipse(selectionBrush, LeftMiddle);
            //else
            //{
            //    e.Graphics.DrawEllipse(selectionPen, LeftMiddle);
            //}
            if (selectedResizeCircle == SelectCircle.TopLeftCorner)
                e.Graphics.FillEllipse(selectionBrush, TopLeftCorner);
            else
            {
                e.Graphics.DrawEllipse(selectionPen, TopLeftCorner);
            }
            if (selectedResizeCircle == SelectCircle.TopRightCorner)
                e.Graphics.FillEllipse(selectionBrush, TopRightCorner);
            else
            {
                e.Graphics.DrawEllipse(selectionPen, TopRightCorner);
            }
            //if (selectedResizeCircle == SelectCircle.TopMiddle)
            //{
            //    e.Graphics.FillEllipse(selectionBrush, TopMiddle);
            //}
            //else
            //{
            //    e.Graphics.DrawEllipse(selectionPen, TopMiddle);
            //}






        }

        public void DrawBoundingBox(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            //e.Graphics.
            e.Graphics.DrawString(this.label, SystemFonts.DefaultFont, selectionBrush, new PointF(boundingBox.Location.X, boundingBox.Location.Y - 12));
            e.Graphics.DrawRectangle(selectionPen, boundingBox);
        }

    }
}