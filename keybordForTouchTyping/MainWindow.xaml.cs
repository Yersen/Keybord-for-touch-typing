using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace keybordForTouchTyping
{
    class KeyboardButton
    {
        public string Value { get; private set; }
        public string ShiftValue { get; private set; }
        public UIElement GridElement { get; private set; }
        public TextBlock TextElement { get; private set; }
        /// Инициализирует новый экземпляр класса KeyboardButton.
        /// </summary>
        /// <param name = "regularValue"> Текст на кнопке, когда клавиша Shift не нажата (цифра или строчная буква) </param>
        /// <param name = "shiftValue"> Текст на кнопке при нажатии клавиши Shift (заглавная буква или специальный символ на цифровой кнопке) </param>
        /// <param name = "row"> Строка сетки окна, в которой расположена кнопка </param>
        /// <param name = "column"> Столбец сетки окна, в котором расположена кнопка </param>
        /// <param name = "columnSpan"> Сколько столбцов сетки окна занимает кнопка </param>
        /// <param name = "backgroundColor"> Цвет фона кнопки клавиатуры </param>
        public KeyboardButton(string regularValue, string shiftValue, int row, int column, int columnSpan, Color backgroundColor)
        {
            Value = regularValue;
            ShiftValue = shiftValue;
            Border border = new Border
            {
                Margin = new Thickness(2.0),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(1.5),
                Background = new SolidColorBrush(backgroundColor),
                CornerRadius = new CornerRadius(7.0)
            };

            TextBlock textBlock = new TextBlock
            {
                Text = regularValue,
                FontSize = 24.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, -3, 0, 0)
            };

            border.Child = textBlock;
            Grid.SetRow(border, row);
            Grid.SetColumn(border, column);
            Grid.SetColumnSpan(border, columnSpan);

            TextElement = textBlock;
            GridElement = border;
        }

        /// <резюме>
        /// Обновляет текст на кнопке клавиатуры при нажатии Shift или Caps Lock.
        /// </summary>
        /// <param name = "shiftIsOn"> Истина, если клавиша Shift нажата, иначе false </param>
        /// <param name = "capsIsOn"> Истина, если клавиша Caps Lock включена, иначе ложь </param>

        public virtual void RefreshText(bool shiftIsOn, bool capsIsOn) { }
    }

    /// <резюме>
    /// Клавиша (кнопка), представляющая управляющую клавишу (не печатаемый символ).
    /// </summary>
    class ControlKey : KeyboardButton
    {
        public ControlKey(string value, int row, int column, int columnSpan) : base(value, value, row, column, columnSpan, Colors.LightGray)
        {
            TextElement.FontSize = 16.0;
        }
    }

    /// <резюме>
    /// Ключ (кнопка), представляющий одну букву.
    /// </summary>
    /// 
    class LetterKey : KeyboardButton
    {
        public LetterKey(string value, int row, int column, Color background) : base(value.ToLower(), value.ToUpper(), row, column, 2, background)
        {

        }
        public override void RefreshText(bool shiftIsOn, bool capsIsOn)
        {
            if (shiftIsOn ^ capsIsOn)
            {
                TextElement.Text = ShiftValue;
            }
            else
            {
                TextElement.Text = Value;
            }
        }
    }

    /// <резюме>
    /// Клавиша (кнопка), представляющая отдельный специальный символ (но не цифровую клавишу).
    /// </summary>
    /// <summary>
    /// A key (button) that represents a single special character (but not a digit key). 
    /// </summary>
    class SpecialCharKey : KeyboardButton
    {
        public SpecialCharKey(string regularValue, string shiftValue, int row, int column, int columnSpan, Color backgroundColor)
            : base(regularValue, shiftValue, row, column, columnSpan, backgroundColor)
        { }

        public override void RefreshText(bool shiftIsOn, bool capsIsOn)
        {
            if (shiftIsOn)
                TextElement.Text = ShiftValue;
            else
                TextElement.Text = Value;
        }
    }


    /// <summary>
    /// Кнопка представляющая цифру
    /// </summary>
    class DigitKey : KeyboardButton
    {
        public DigitKey(string regularValue, string shiftValue, int row, int column, Color background) : base(regularValue, shiftValue, row, column, 2, background)
        {

        }

        public override void RefreshText(bool shiftIsOn, bool capsIsOn)
        {
            if (shiftIsOn)
            {
                TextElement.Text = ShiftValue;
            }
            else
            {
                TextElement.Text = Value;
            }
        }
    }

    /// <резюме>
    /// Клавиша (кнопка), представляющая клавишу пробела.
    /// </summary>
    class SpaceKey : KeyboardButton
    {
        public SpaceKey(int row, int col, int colSpan, Color back) : base("Space", "Space", row, col, colSpan, back)
        {
            TextElement.FontSize = 16.0;
        }
    }


    /// <резюме>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int GENERATED_TEXT_LENGTH = 68;
        const int N_PRINTABLE_CHARS = 47;
        Random rand;// для генерации случайных символов
        DateTime startTime;// для сохранении времени, когда пользователь нажимает кнопку Start
        int correctlyTypedTextLength;// количество набранных символов до первого несовпадения с сгенерированной строкой
        int fails;// количество неправильно набранных символов
        Dictionary<Key, KeyboardButton> allKeyboardButtons;// сохранить кнопки, отображаемые на графической клавиатуре, в главном окне
        public MainWindow()
        {
            InitializeComponent();
            rand = new Random(DateTime.Now.Millisecond);
            allKeyboardButtons = new Dictionary<Key, KeyboardButton>();

            // Create all keys for the keyboard:
            allKeyboardButtons[Key.Oem3] = new SpecialCharKey("`", "~", 0, 0, 2, Colors.Pink);
            allKeyboardButtons[Key.D1] = new DigitKey("1", "!", 0, 2, Colors.Pink);
            allKeyboardButtons[Key.D2] = new DigitKey("2", "@", 0, 4, Colors.Pink);
            allKeyboardButtons[Key.D3] = new DigitKey("3", "#", 0, 6, Colors.Yellow);
            allKeyboardButtons[Key.D4] = new DigitKey("4", "$", 0, 8, Colors.LawnGreen);
            allKeyboardButtons[Key.D5] = new DigitKey("5", "%", 0, 10, Colors.DeepSkyBlue);
            allKeyboardButtons[Key.D6] = new DigitKey("6", "^", 0, 12, Colors.DeepSkyBlue);
            allKeyboardButtons[Key.D7] = new DigitKey("7", "&", 0, 14, Colors.MediumVioletRed);
            allKeyboardButtons[Key.D8] = new DigitKey("8", "*", 0, 16, Colors.MediumVioletRed);
            allKeyboardButtons[Key.D9] = new DigitKey("9", "(", 0, 18, Colors.Pink);
            allKeyboardButtons[Key.D0] = new DigitKey("0", ")", 0, 20, Colors.Yellow);
            allKeyboardButtons[Key.OemMinus] = new SpecialCharKey("-", "_", 0, 22, 2, Colors.LawnGreen);
            allKeyboardButtons[Key.OemPlus] = new SpecialCharKey("=", "+", 0, 24, 2, Colors.LawnGreen);
            allKeyboardButtons[Key.Back] = new ControlKey("Backspace", 0, 26, 4);
            allKeyboardButtons[Key.Tab] = new ControlKey("Tab", 1, 0, 3);
            allKeyboardButtons[Key.Q] = new LetterKey("Q", 1, 3, Colors.Pink);
            allKeyboardButtons[Key.W] = new LetterKey("W", 1, 5, Colors.Yellow);
            allKeyboardButtons[Key.E] = new LetterKey("E", 1, 7, Colors.LawnGreen);
            allKeyboardButtons[Key.R] = new LetterKey("R", 1, 9, Colors.DeepSkyBlue);
            allKeyboardButtons[Key.T] = new LetterKey("T", 1, 11, Colors.DeepSkyBlue);
            allKeyboardButtons[Key.Y] = new LetterKey("Y", 1, 13, Colors.MediumVioletRed);
            allKeyboardButtons[Key.U] = new LetterKey("U", 1, 15, Colors.MediumVioletRed);
            allKeyboardButtons[Key.I] = new LetterKey("I", 1, 17, Colors.Pink);
            allKeyboardButtons[Key.O] = new LetterKey("O", 1, 19, Colors.Yellow);
            allKeyboardButtons[Key.P] = new LetterKey("p", 1, 21, Colors.LawnGreen);
            allKeyboardButtons[Key.OemOpenBrackets] = new SpecialCharKey("[", "{", 1, 23, 2, Colors.LawnGreen);
            allKeyboardButtons[Key.OemCloseBrackets] = new SpecialCharKey("]", "}", 1, 25, 2, Colors.LawnGreen);
            allKeyboardButtons[Key.Oem5] = new SpecialCharKey("\\", "|", 1, 27, 3, Colors.LawnGreen);
            allKeyboardButtons[Key.CapsLock] = new ControlKey("Caps Lock", 2, 0, 4);
            allKeyboardButtons[Key.A] = new LetterKey("A", 2, 4, Colors.Pink);
            allKeyboardButtons[Key.S] = new LetterKey("S", 2, 6, Colors.Yellow);
            allKeyboardButtons[Key.D] = new LetterKey("D", 2, 8, Colors.LawnGreen);
            allKeyboardButtons[Key.F] = new LetterKey("F", 2, 10, Colors.DeepSkyBlue);
            allKeyboardButtons[Key.G] = new LetterKey("G", 2, 12, Colors.DeepSkyBlue);
            allKeyboardButtons[Key.H] = new LetterKey("H", 2, 14, Colors.MediumVioletRed);
            allKeyboardButtons[Key.J] = new LetterKey("J", 2, 16, Colors.MediumVioletRed);
            allKeyboardButtons[Key.K] = new LetterKey("K", 2, 18, Colors.Pink);
            allKeyboardButtons[Key.L] = new LetterKey("L", 2, 20, Colors.Yellow);
            allKeyboardButtons[Key.OemSemicolon] = new SpecialCharKey(";", ":", 2, 22, 2, Colors.LawnGreen);
            allKeyboardButtons[Key.OemQuotes] = new SpecialCharKey("'", "\"", 2, 24, 2, Colors.LawnGreen);
            allKeyboardButtons[Key.Enter] = new ControlKey("Enter", 2, 26, 4);
            allKeyboardButtons[Key.LeftShift] = new ControlKey("Shift", 3, 0, 5);
            allKeyboardButtons[Key.Z] = new LetterKey("Z", 3, 5, Colors.Pink);
            allKeyboardButtons[Key.X] = new LetterKey("X", 3, 7, Colors.Yellow);
            allKeyboardButtons[Key.C] = new LetterKey("C", 3, 9, Colors.LawnGreen);
            allKeyboardButtons[Key.V] = new LetterKey("V", 3, 11, Colors.DeepSkyBlue);
            allKeyboardButtons[Key.B] = new LetterKey("B", 3, 13, Colors.DeepSkyBlue);
            allKeyboardButtons[Key.N] = new LetterKey("N", 3, 15, Colors.MediumVioletRed);
            allKeyboardButtons[Key.M] = new LetterKey("M", 3, 17, Colors.MediumVioletRed);
            allKeyboardButtons[Key.OemComma] = new SpecialCharKey(",", "<", 3, 19, 2, Colors.Pink);
            allKeyboardButtons[Key.OemPeriod] = new SpecialCharKey(".", ">", 3, 21, 2, Colors.Yellow);
            allKeyboardButtons[Key.OemQuestion] = new SpecialCharKey("/", "?", 3, 23, 2, Colors.LawnGreen);
            allKeyboardButtons[Key.RightShift] = new ControlKey("Shift", 3, 25, 5);
            allKeyboardButtons[Key.LeftCtrl] = new ControlKey("Ctrl", 4, 0, 3);
            allKeyboardButtons[Key.LWin] = new ControlKey("Win", 4, 3, 3);
            allKeyboardButtons[Key.LeftAlt] = new ControlKey("Alt", 4, 6, 3);
            allKeyboardButtons[Key.Space] = new SpaceKey(4, 9, 12, Colors.Orange);
            allKeyboardButtons[Key.RightAlt] = new ControlKey("Alt", 4, 21, 3);
            allKeyboardButtons[Key.RWin] = new ControlKey("Win", 4, 24, 3);
            allKeyboardButtons[Key.RightCtrl] = new ControlKey("Ctrl", 4, 27, 3);

            // Place all KeyboardButtons to the grid on the main window:
            foreach (KeyboardButton keyboardButton in allKeyboardButtons.Values)
                keyboardGrid.Children.Add(keyboardButton.GridElement);
        }

        private void sliderDifficulty_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tbDifficulty.Text = Math.Round(sliderDifficulty.Value).ToString();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStop.IsEnabled = true;
            btnStart.IsEnabled = false;
            cbCaseSensitive.IsEnabled = false;
            cbIncludeDigits.IsEnabled = false;
            cbIncludeSpecialChars.IsEnabled = false;
            sliderDifficulty.IsEnabled = false;
            tbTypedText.Text = "";
            startTime = DateTime.Now;
            correctlyTypedTextLength = 0;
            fails = 0;
            tbFails.Text = "0";
            tbSpeed.Text = "0";

            int difficulty;
            try
            {
                difficulty = int.Parse(tbDifficulty.Text);

                if (difficulty > N_PRINTABLE_CHARS)
                {
                    difficulty = N_PRINTABLE_CHARS;
                }
                else if (difficulty < 2)
                {
                    difficulty = 2;
                }
            }
            catch (FormatException re)
            {
                difficulty = 12;//значение по умолчанию
                tbDifficulty.Text = "12";
            }
            tbDifficulty.Text = difficulty.ToString();
            tbGeneratedText.Text = GenerateText(difficulty);
            tbTypedText.Focus();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        public void Stop()
        {
            btnStop.IsEnabled = false;
            btnStart.IsEnabled = true;
            cbCaseSensitive.IsEnabled = true;
            cbIncludeDigits.IsEnabled = true;
            cbIncludeSpecialChars.IsEnabled = true;
            sliderDifficulty.IsEnabled = true;
        }

        private void mainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!allKeyboardButtons.ContainsKey(e.Key))
            {
                return;
            }

            allKeyboardButtons[e.Key].GridElement.Effect = new DropShadowEffect();

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.CapsLock)
            {
                RefreshKeyboard();
                return;
            }

            if (btnStart.IsEnabled)  // если кнопка Старт включена, упражнение еще не началось
                return;

            if (e.Key == Key.Back)
            {
                if (tbTypedText.Text.Length > 0)
                {
                    tbTypedText.Text = tbTypedText.Text.Remove(tbTypedText.Text.Length - 1);
                    if (correctlyTypedTextLength >= tbTypedText.Text.Length)
                    {
                        correctlyTypedTextLength = tbTypedText.Text.Length;
                        tbTypedText.Foreground = new SolidColorBrush(Colors.Black);  // поскольку весь набранный текст правильный
                    }
                    tbTypedText.Select(0, correctlyTypedTextLength);  // выделение зеленого цвета выделяет правильно набранный текст
                }
                return;
            }

            if(allKeyboardButtons[e.Key] is ControlKey)
            {
                return;
            }

            e.Handled = true;
            if (e.Key == Key.Space)
                tbTypedText.AppendText(" ");
            else if (allKeyboardButtons[e.Key] is LetterKey || allKeyboardButtons[e.Key] is SpecialCharKey)
                tbTypedText.AppendText(allKeyboardButtons[e.Key].TextElement.Text);

            // Если следующий символ после последовательности ранее проверенных символов правильный, он расширит зеленую подсветку до этого символа:
            if (tbGeneratedText.Text[correctlyTypedTextLength] == tbTypedText.Text[correctlyTypedTextLength])
                correctlyTypedTextLength++;

            // Если последний введенный символ неверен, это увеличит количество сбоев пользователя
            if (tbTypedText.Text.Length <= tbGeneratedText.Text.Length)
                if (tbGeneratedText.Text[tbTypedText.Text.Length - 1] != tbTypedText.Text[tbTypedText.Text.Length - 1])
                    fails++;

            if (correctlyTypedTextLength == tbTypedText.Text.Length)
                tbTypedText.Foreground = new SolidColorBrush(Colors.Black);  // нет ошибок в набранном тексте
            else
                tbTypedText.Foreground = new SolidColorBrush(Colors.Red);  // есть хотя бы одна ошибка

            tbFails.Text = fails.ToString();
            tbSpeed.Text = Math.Round(correctlyTypedTextLength / (DateTime.Now - startTime).TotalMinutes).ToString();
            tbTypedText.Select(0, correctlyTypedTextLength);  // выделение зеленого цвета выделяет правильно набранный текст

            if (correctlyTypedTextLength == tbGeneratedText.Text.Length)
            {
                Stop();
                MessageBox.Show("Congrats! You have successfully typed the whole text", "Finished", MessageBoxButton.OK);
                RefreshKeyboard();
            }
        }
        private void mainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (!allKeyboardButtons.ContainsKey(e.Key))
                return;

            allKeyboardButtons[e.Key].GridElement.Effect = null;

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                RefreshKeyboard();

            e.Handled = true;
        }
        private void RefreshKeyboard()
        {
            bool shiftIsOn = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool capsIsOn = Keyboard.IsKeyToggled(Key.CapsLock);
            foreach(KeyboardButton keyboardButton in allKeyboardButtons.Values)
            {
                keyboardButton.RefreshText(shiftIsOn, capsIsOn);
                keyboardButton.GridElement.Effect = null;
            }
        }

        private string GenerateText(int difficulty)
        {
            List<KeyboardButton> charsForTextGeneration = allKeyboardButtons.Values.OfType<LetterKey>().ToList<KeyboardButton>();
            if (cbIncludeDigits.IsChecked == true)
                charsForTextGeneration.AddRange(allKeyboardButtons.Values.OfType<DigitKey>().ToList<KeyboardButton>());
            if (cbIncludeSpecialChars.IsChecked == true)
                charsForTextGeneration.AddRange(allKeyboardButtons.Values.OfType<SpecialCharKey>().ToList<KeyboardButton>());

            List<char> randomChars = new List<char>();  // список случайно выбранных символов для генерации текстовой строки в соответствии с уровнем сложности
            int randomIndex;

            // Добавляем в список необходимое количество случайных символов:
            for (int i = 0; i < difficulty; ++i)
            {
                randomIndex = rand.Next(charsForTextGeneration.Count);
                randomChars.Add(charsForTextGeneration.ElementAt(randomIndex).Value[0]);
                if (cbCaseSensitive.IsChecked == true)
                    randomChars.Add(charsForTextGeneration.ElementAt(randomIndex).ShiftValue[0]);
            }

            // Добавляем в список некоторое количество пробелов относительно длины списка:
            for (int i = 5; i <= difficulty; i += 5)
            {
                randomChars.Add(' ');
                if (cbCaseSensitive.IsChecked == true)
                    randomChars.Add(' ');
            }

            // Генерируем случайную строку из ранее случайно выбранных символов:
            string result = "";
            for (int i = 0; i < GENERATED_TEXT_LENGTH; ++i)
            {
                result += randomChars[rand.Next(randomChars.Count)];
            }
            return result;
        }


    }

    

}

