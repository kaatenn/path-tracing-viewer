using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace _3D_viewer;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void float_text_input_handler(object sender, TextCompositionEventArgs e)
    {
        // Check if the input is in float format using regular expression
        var regex = new Regex("^-?[0-9]*\\.?[0-9]*$");
        var isFloat = regex.IsMatch(e.Text);

        // If the input is not in float format, disable input
        if (!isFloat)
        {
            e.Handled = true;
            return;
        }

        // Convert the input to float
        var value = float.Parse((sender as TextBox)?.Text + e.Text);

        // If the input is not between 0 and 1, disable input
        if (value < 0 || value > 1)
        {
            var textBlock = (TextBlock)FindName("TrianglePointsTextBlock");
            textBlock.Foreground = new SolidColorBrush(Colors.Red);
            Task.Delay(3000).ContinueWith(t =>
                textBlock.Dispatcher.Invoke(() => textBlock.Foreground = new SolidColorBrush(Colors.Black)));

            e.Handled = true;
        }
    }

    private void button_increase_click(object sender, RoutedEventArgs e)
    {
        var x1 = float.Parse(TrianglePoint1XTextBox.Text);
        var y1 = float.Parse(TrianglePoint1YTextBox.Text);
        var z1 = float.Parse(TrianglePoint1ZTextBox.Text);
        var x2 = float.Parse(TrianglePoint2XTextBox.Text);
        var y2 = float.Parse(TrianglePoint2YTextBox.Text);
        var z2 = float.Parse(TrianglePoint2ZTextBox.Text);
        var x3 = float.Parse(TrianglePoint3XTextBox.Text);
        var y3 = float.Parse(TrianglePoint3YTextBox.Text);
        var z3 = float.Parse(TrianglePoint3ZTextBox.Text);
    }
}