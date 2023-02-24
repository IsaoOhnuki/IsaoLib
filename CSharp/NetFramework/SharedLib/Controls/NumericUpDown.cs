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
            new PropertyMetadata(default(decimal), ValueChanged));

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NumericUpDown)d).UpdateState(true);
        }

        public decimal Value
        {
            get => decimal.TryParse(TextBox?.Text, out decimal value) ? value : 0;
            set => TextBox.SetValue(TextBox.TextProperty, value.ToString($"F{DecimalPlace}"));
        }

        private void UpdateState(bool useTransition)
        {
            if (decimal.TryParse(TextBox?.Text, out _))
            {
                VisualStateManager.GoToState(this, "Positive", useTransition);
            }
            else
            {
                VisualStateManager.GoToState(this, "Negative", useTransition);
            }
        }

        public static readonly DependencyProperty MaxValueProperty =
        DependencyProperty.Register(
            nameof(MaxValue),
            typeof(decimal),
            typeof(NumericUpDown),
            new PropertyMetadata((decimal)0));

        public decimal MaxValue
        {
            get => (decimal)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public static readonly DependencyProperty MinValueProperty =
        DependencyProperty.Register(
            nameof(MinValue),
            typeof(decimal),
            typeof(NumericUpDown),
            new PropertyMetadata(decimal.MinValue));

        public decimal MinValue
        {
            get => (decimal)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        public static readonly DependencyProperty DecimalPlaceProperty =
        DependencyProperty.Register(
            nameof(DecimalPlace),
            typeof(int),
            typeof(NumericUpDown),
            new PropertyMetadata(
                0,
                (s, e) =>
                {
                    if (s is NumericUpDown ctrl)
                    {
                        if (e.OldValue is int oldVal)
                        {
                            ctrl.StepValue = 1;
                        }
                        if (e.NewValue is int newVal)
                        {
                            decimal val = 1;
                            for (int i = 0; i < newVal; i++)
                            {
                                val /= 10;
                            }
                            ctrl.StepValue = val;
                        }
                    }
                },
                (s, e) => e is int value && value >= 0 ? value : 0));

        public int DecimalPlace
        {
            get => (int)GetValue(DecimalPlaceProperty);
            set => SetValue(DecimalPlaceProperty, value);
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
            UpdateState(true);
        }

        private decimal StepValue { get; set; } = 1;

        private void UpButtonClick(object sender, RoutedEventArgs e)
        {
            decimal val = StepValue;
            if (MaxValue == decimal.MaxValue ||
                Value <= MaxValue - val)
            {
                Value += val;
            }
        }

        private void DownButtonClick(object sender, RoutedEventArgs e)
        {
            decimal val = StepValue;
            if (MinValue == decimal.MinValue ||
                Value >= MinValue + val)
            {
                Value -= val;
            }
        }
    }
}
