using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FTP_Client.Resources.Controls
{
    public class BindablePasswordBox : Control
    {
        private bool _isPasswordChanging;
        private PasswordBox _passwordBox;
        
        static BindablePasswordBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BindablePasswordBox),
                new FrameworkPropertyMetadata(typeof(BindablePasswordBox)));
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(BindablePasswordBox),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    PasswordPropertyChanged, null, false, UpdateSourceTrigger.PropertyChanged));

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _passwordBox = GetTemplateChild("PART_PasswordBox") as PasswordBox;

            if (_passwordBox != null)
            {
                _passwordBox.PasswordChanged += OnPasswordChanged;
                UpdatePassword();
            }
        }

        private static void PasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BindablePasswordBox passwordBox)
                passwordBox.UpdatePassword();
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            _isPasswordChanging = true;
            Password = _passwordBox.Password;
            _isPasswordChanging = false;
        }

        private void UpdatePassword()
        {
            if (!_isPasswordChanging && _passwordBox != null)
            {
                _passwordBox.Password = Password;
            }
        }
    }
}