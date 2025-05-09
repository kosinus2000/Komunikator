using System.Windows;
using System.Windows.Controls;

namespace Komunikator
{
    public partial class RoundedPasswordBox : UserControl
    {
        public RoundedPasswordBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(nameof(Password), typeof(string), typeof(RoundedPasswordBox),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(RoundedPasswordBox),
                new PropertyMetadata(new CornerRadius(0)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public event RoutedEventHandler PasswordChanged;

        private void InnerPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = InnerPasswordBox.Password;
            PasswordChanged?.Invoke(this, e);
        }
    }
}