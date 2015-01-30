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

namespace Yaakov.Controls
{
    /// <summary>
    /// Represents a content control which displays process view while content is processing.
    /// </summary>
    public class ProcessingContentControl : ContentControl
    {
        public bool IsContentProcessing
        {
            get { return (bool)GetValue(IsContentProcessingProperty); }
            set { SetValue(IsContentProcessingProperty, value); }
        }

        public static readonly DependencyProperty IsContentProcessingProperty =
            DependencyProperty.Register("IsContentProcessing", typeof(bool), typeof(ProcessingContentControl), new UIPropertyMetadata(false));


        static ProcessingContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProcessingContentControl), new FrameworkPropertyMetadata(typeof(ProcessingContentControl)));
        }
    }
}
