using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;

namespace Yaakov.Controls
{
    public class PolarPanel : Panel
    {
        /// <summary>
        /// Gets the angle (in degrees) between the radial line to the center of an element and the positive X radial line of its parent PolarPanel. 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        [AttachedPropertyBrowsableForChildren]
        [Category("Layout")]
        public static double GetAngle(DependencyObject obj)
        {
            return (double)obj.GetValue(AngleProperty);
        }

        /// <summary>
        /// Sets the angle (in degrees) between the radial line to the center of an element and the positive X radial line of its parent PolarPanel. 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetAngle(DependencyObject obj, double value)
        {
            obj.SetValue(AngleProperty, value);
        }
        
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.RegisterAttached("Angle", typeof(double), typeof(PolarPanel), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure), ValidateAngle);

        /// <summary>
        /// Gets the distance between the the center of an element and the center of its parent PolarPanel. 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        [AttachedPropertyBrowsableForChildren]
        public static double GetRadius(DependencyObject obj)
        {
            return (double)obj.GetValue(RadiusProperty);
        }

        /// <summary>
        /// Sets the distance between the the center of an element and the center of its parent PolarPanel. 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetRadius(DependencyObject obj, double value)
        {
            obj.SetValue(RadiusProperty, value);
        }

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.RegisterAttached("Radius", typeof(double), typeof(PolarPanel), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure), ValidateRadius);

        protected override Size MeasureOverride(Size availableSize)
        {
            Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            Rect bounds = new Rect();

            foreach (UIElement element in InternalChildren)
            {
                Point location = PolarToCartesian(GetAngle(element), GetRadius(element));

                element.Measure(infiniteSize);
                Rect elementBounds = CreateCenteralizeRect(location, element.DesiredSize);
                bounds.Union(elementBounds);
            }

            double width = Math.Max(Math.Abs(bounds.Left), Math.Abs(bounds.Right)),
                height = Math.Max(Math.Abs(bounds.Top), Math.Abs(bounds.Bottom));

            return new Size(width * 2, height * 2);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Point panelCenter = new Point(finalSize.Width / 2, finalSize.Height / 2);

            foreach (UIElement element in InternalChildren)
            {
                Point location = PolarToCartesian(GetAngle(element), GetRadius(element));
                location += (Vector)panelCenter;

                Rect elementBounds = CreateCenteralizeRect(location, element.DesiredSize);

                element.Arrange(elementBounds);
            }

            return finalSize;
        }

        private static Rect CreateCenteralizeRect(Point point, Size size)
        {
            Rect result = new Rect(
                point.X - size.Width / 2,
                point.Y - size.Height / 2,
                size.Width,
                size.Height);

            return result;
        }

        private static Point PolarToCartesian(double angle, double radius)
        {
            angle *= (-Math.PI / 180);
            Point result = new Point(
                radius * Math.Cos(angle),
                radius * Math.Sin(angle));

            return result;
        }

        private static bool ValidateAngle(object value)
        {
            double angle = (double)value;
            return !double.IsNaN(angle) && !double.IsInfinity(angle);
        }

        private static bool ValidateRadius(object value)
        {
            double radius = (double)value;
            return !double.IsNaN(radius) && !double.IsInfinity(radius);
        }
    }
}
