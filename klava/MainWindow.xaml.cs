using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Klavagonochka
{
    public partial class MainWindow : Window
    {
        private string textToType = "";
        private string typedText = "";
        private Random random = new Random();
        private int currentIndex = 0;
        private DateTime startTime;
        private int fails = 0;
        private DateTime lastSpeedUpdate = DateTime.Now;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void GenerateRandomText()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
            int length = (int)DifficultySlider.Value;
            textToType = new string(Enumerable.Range(0, length).Select(x => chars[random.Next(chars.Length)]).ToArray());
            UpdateTextToTypeDisplay();
        }

        private void UpdateTextToTypeDisplay()
        {
            TextToType.Inlines.Clear();

            for (int i = 0; i < textToType.Length; i++)
            {
                var run = new Run(textToType[i].ToString());

                if (i == currentIndex)
                {
                    run.Background = Brushes.Yellow;
                    run.FontWeight = FontWeights.Bold;
                }
                else if (i < currentIndex)
                {
                    run.Foreground = Brushes.Gray;
                }
                else
                {
                    run.Foreground = Brushes.Black;
                }

                TextToType.Inlines.Add(run);
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            typedText = "";
            currentIndex = 0;
            fails = 0;
            InputTextBox.Clear();
            GenerateRandomText();
            ResultText.Text = "";
            SpeedText.Text = "0 chars/min";
            FailsText.Text = "0";
            startTime = DateTime.Now;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            CheckResult();
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsValidKey(e.Key))
            {
                char keyChar = GetCharFromKey(e.Key);

                if (currentIndex < textToType.Length && keyChar != '\0')
                {
                    if (keyChar == textToType[currentIndex])
                    {
                        typedText += keyChar;
                        currentIndex++;
                        HighlightKey(keyChar, Brushes.Green);
                    }
                    else
                    {
                        fails++;
                        HighlightKey(keyChar, Brushes.Red);
                    }
                }

                UpdateTextToTypeDisplay();
                UpdateSpeed();
                CheckResult();
            }
            else if (e.Key != Key.Back && e.Key != Key.Space)
            {
                e.Handled = true;
            }
        }

        private bool IsValidKey(Key key)
        {
            return (key >= Key.A && key <= Key.Z) || (key >= Key.D0 && key <= Key.D9) ||
                   (key >= Key.NumPad0 && key <= Key.NumPad9) || key == Key.Space || key == Key.Back;
        }

        private char GetCharFromKey(Key key)
        {
            if (key >= Key.A && key <= Key.Z)
            {
                bool isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                char ch = (char)('a' + (key - Key.A));
                return isShiftPressed ? char.ToUpper(ch) : ch;
            }
            else if (key >= Key.D0 && key <= Key.D9)
            {
                return (char)('0' + (key - Key.D0));
            }
            else if (key >= Key.NumPad0 && key <= Key.NumPad9)
            {
                return (char)('0' + (key - Key.NumPad0));
            }

            switch (key)
            {
                case Key.Space: return ' ';
                case Key.OemComma: return ',';
                case Key.OemPeriod: return '.';
                case Key.OemMinus: return '-';
                case Key.OemPlus: return '+';
                default: return '\0';
            }
        }

        private void HighlightKey(char keyChar, Brush color)
        {
            string keyName = "Key" + keyChar.ToString().ToUpper();
            Button keyButton = FindName(keyName) as Button;

            if (keyButton != null)
            {
                Brush originalColor = keyButton.Background;
                keyButton.Background = color;

                Dispatcher.InvokeAsync(() =>
                {
                    System.Threading.Tasks.Task.Delay(300).ContinueWith(t =>
                    {
                        Dispatcher.Invoke(() => keyButton.Background = originalColor);
                    });
                });
            }
        }

        private void UpdateSpeed()
        {
            TimeSpan elapsedTime = DateTime.Now - startTime;
            double speed = typedText.Length / elapsedTime.TotalMinutes;
            SpeedText.Text = $"{speed:F2} chars/min";
            FailsText.Text = fails.ToString();
        }

        private void CheckResult()
        {
            if (currentIndex == textToType.Length)
            {
                TimeSpan timeTaken = DateTime.Now - startTime;
                double speed = textToType.Length / timeTaken.TotalMinutes;
                ResultText.Text = $"Харош! твоя скорость: {speed:F2} символов в минуту. Ошибок: {fails}";
                ResultText.Foreground = Brushes.Green;

                MessageBox.Show($"Поздравляшки! твоя скорость: {speed:F2} символов в минуту, ошибок: {fails}",
                                "да ты крут!",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }

        private void Key_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                string keyContent = button.Content.ToString();
                char keyChar = keyContent.Length == 1 ? keyContent[0] : '\0';
                if (currentIndex < textToType.Length && keyChar != '\0')
                {
                    if (keyChar == textToType[currentIndex])
                    {
                        typedText += keyChar;
                        currentIndex++;
                        HighlightKey(keyChar, Brushes.Green);
                    }
                    else
                    {
                        fails++;
                        HighlightKey(keyChar, Brushes.Red);
                    }
                }

                UpdateTextToTypeDisplay();
                CheckResult();
            }
        }
    }
}