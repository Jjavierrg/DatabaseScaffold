namespace DatabaseScaffold.Controls
{
    using DatabaseScaffold.Interfaces;
    using System.Windows;
    using System.Windows.Controls;

    public class MotorViewer : UserControl
    {
        public static readonly DependencyProperty MotorProperty = DependencyProperty.Register("Motor", typeof(IMotor), typeof(MotorViewer), new PropertyMetadata(null));

        public IMotor Motor
        {
            get { return (IMotor)GetValue(MotorProperty); }
            set => SetValue(MotorProperty, value);
        }
    }
}
