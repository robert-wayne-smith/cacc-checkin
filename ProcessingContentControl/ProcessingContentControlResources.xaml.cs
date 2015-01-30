using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.ComponentModel;

namespace Yaakov.Controls
{
    /// <summary>
    /// Interaction logic for ProgressingContentControl.xaml
    /// </summary>
    public partial class ProcessingContentControlResources
    {
        private static Dictionary<PolarPanel, int> currentAnimatingCircles = new Dictionary<PolarPanel, int>();
        private static HashSet<PolarPanel> panels = new HashSet<PolarPanel>();
        private static ColorAnimation animation;
        private static Brush brush;

        private static DispatcherTimer timer;

        private static DispatcherTimer Timer
        {
            get
            {
                if (timer == null)
                {
                    timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromMilliseconds(120);
                    timer.Tick += Timer_Tick;
                }

                return timer;
            }
        }

        private void PolarPanel_Loaded(object sender, RoutedEventArgs e)
        {
            PolarPanel panel = sender as PolarPanel;

            if (brush == null)
                brush = this["brush"] as Brush;

            foreach (Shape shape in panel.Children)
            {
                if (shape.Fill != null)
                    break;

                shape.Fill = brush.Clone();
            }

            ProcessingContentControl parent = panel.TemplatedParent as ProcessingContentControl;
            OnPanelVisibilityChanged(panel, panel.IsVisible);
        }

        private void PolarPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PolarPanel panel = sender as PolarPanel;
            bool visible = (bool)e.NewValue;

            OnPanelVisibilityChanged(panel, visible);

        }

        private void OnPanelVisibilityChanged(PolarPanel panel, bool visible)
        {
            if (visible)
                Animate(panel);
            else
                StopAnimating(panel);
        }



        private object ConvertProcessingToVisibility(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((bool)value == true) ?
                Visibility.Visible :
                Visibility.Collapsed;
        }

        private void Animate(PolarPanel panel)
        {
            if (currentAnimatingCircles.ContainsKey(panel))
                return;

            if (animation == null)
                animation = this["animation"] as ColorAnimation;

            panels.Add(panel);
            currentAnimatingCircles[panel] = -1;
            if (!Timer.IsEnabled)
                timer.Start();
        }

        private static void Timer_Tick(object sender, EventArgs e)
        {
            foreach (PolarPanel panel in panels)
            {
                int circleIndex = currentAnimatingCircles[panel] + 1;
                if (circleIndex >= panel.Children.Count)
                    circleIndex = 0;

                currentAnimatingCircles[panel] = circleIndex;
                Shape element = panel.Children[circleIndex] as Shape;
                SolidColorBrush brush = element.Fill as SolidColorBrush;

                brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
            }
        }

        private void StopAnimating(PolarPanel panel)
        {
            currentAnimatingCircles.Remove(panel);
            panels.Remove(panel);

            foreach (Shape shape in panel.Children)
                shape.Fill.BeginAnimation(SolidColorBrush.ColorProperty, null);

            if (currentAnimatingCircles.Count == 0)
                Timer.Stop();
        }


    }
}
