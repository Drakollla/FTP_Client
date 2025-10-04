using System.Windows;
using System.Windows.Controls;

namespace FTP_Client.Resources.Controls
{
    public class SidePanelControl : ContentControl
    {
        static SidePanelControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SidePanelControl),
                new FrameworkPropertyMetadata(typeof(SidePanelControl)));
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(SidePanelControl), new PropertyMetadata(null));
      
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(SidePanelControl), new PropertyMetadata(false));

        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }
    }
}