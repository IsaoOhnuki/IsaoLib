using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SharedLib.Controls
{
    public class NumericUpDown : Control
    {
        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown),
                new FrameworkPropertyMetadata(typeof(NumericUpDown)));
        }

        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(decimal),
            typeof(NumericUpDown),
            new FrameworkPropertyMetadata(
                default(decimal),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (s, e) =>
                {
                    if (s is NumericUpDown ctrl &&
                        e.NewValue is decimal value)
                    {
                        ctrl.TextBox.SetValue(TextBox.TextProperty, value.ToString($"F{ctrl.DecimalPlace}"));
                    }
                }));

        public decimal Value
        {
            get => decimal.TryParse(TextBox?.Text, out decimal value) ? value : 0;
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty MaxValueProperty =
        DependencyProperty.Register(
            nameof(MaxValue),
            typeof(decimal?),
            typeof(NumericUpDown),
            new PropertyMetadata((decimal?)null));

        public decimal? MaxValue
        {
            get => (decimal?)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public static readonly DependencyProperty MinValueProperty =
        DependencyProperty.Register(
            nameof(MinValue),
            typeof(decimal?),
            typeof(NumericUpDown),
            new PropertyMetadata((decimal?)null));

        public decimal? MinValue
        {
            get => (decimal?)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        public static readonly DependencyProperty DecimalPlaceProperty =
        DependencyProperty.Register(
            nameof(DecimalPlace),
            typeof(int),
            typeof(NumericUpDown),
            new PropertyMetadata(0));

        public int DecimalPlace
        {
            get => (int)GetValue(DecimalPlaceProperty);
            set => SetValue(DecimalPlaceProperty, value);
        }

        private decimal StepValue()
        {
            decimal val = 1;
            for (int i = 0; i < DecimalPlace; i++)
            {
                val /= 10;
            }
            return val;
        }

        public TextBox TextBox
        {
            get => _textBox;
            private set
            {
                if (_textBox != null)
                {
                    _textBox.TextChanged -= TextBoxTextChanged;
                }
                _textBox = value;
                if (_textBox != null)
                {
                    _textBox.Text = Value.ToString($"F{DecimalPlace}");
                    _textBox.TextChanged += TextBoxTextChanged;
                }
            }
        }

        private TextBox _textBox;

        public RepeatButton UpButton
        {
            get => _upButton;
            private set
            {
                if (_upButton != null)
                {
                    _upButton.Click -= UpButtonClick;
                }
                _upButton = value;
                if (_upButton != null)
                {
                    _upButton.Click += UpButtonClick;
                }
            }
        }

        private RepeatButton _upButton;
        public RepeatButton DownButton
        {
            get => _downButton;
            private set
            {
                if (_downButton != null)
                {
                    _downButton.Click -= DownButtonClick;
                }
                _downButton = value;
                if (_downButton != null)
                {
                    _downButton.Click += DownButtonClick;
                }
            }
        }
        private RepeatButton _downButton;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            TextBox = GetTemplateChild("PART_TextBox") as TextBox;
            UpButton = GetTemplateChild("PART_UpButton") as RepeatButton;
            DownButton = GetTemplateChild("PART_DownButton") as RepeatButton;
        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox &&
                decimal.TryParse(textBox.Text, out decimal value))
            {
                Value = value;
            }
        }

        private void UpButtonClick(object sender, RoutedEventArgs e)
        {
            decimal value = Value;
            decimal step = StepValue();
            if (!MaxValue.HasValue ||
                value <= MaxValue.Value - step)
            {
                value += step;
            }
            if (!MinValue.HasValue ||
                value < MinValue)
            {
                value = MinValue ?? 0;
            }
            Value = value;
        }

        private void DownButtonClick(object sender, RoutedEventArgs e)
        {
            decimal value = Value;
            decimal step = StepValue();
            if (!MinValue.HasValue ||
                value >= MinValue + step)
            {
                value -= step;
            }
            if (!MaxValue.HasValue ||
                value > MaxValue)
            {
                value = MaxValue ?? 0;
            }
            Value = value;
        }
    }
}
