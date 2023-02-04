using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace MVVM
{
    public abstract class ConverterBase<TSource, TTarget> : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => Convert((TSource)value, parameter, culture);

        public abstract TTarget Convert(TSource value, object parameter, CultureInfo culture);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => ConvertBack((TTarget)value, parameter, culture);

        public abstract TSource ConvertBack(TTarget value, object parameter, CultureInfo culture);

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    public class EnumTypeToVisibilityConverter : ConverterBase<object, Visibility>
    {
        public EnumTypeToVisibilityConverter()
        {
        }

        public EnumTypeToVisibilityConverter(Type enumType)
        {
            EnumType = enumType;
        }

        [ConstructorArgument("enumType")]
        public Type EnumType { get; set; }

        public override Visibility Convert(object value, object parameter, CultureInfo culture)
        {
            return Enum.Parse(EnumType, value.ToString()) == Enum.Parse(EnumType, parameter.ToString()) ?
                Visibility.Visible : Visibility.Collapsed;
        }

        public override object ConvertBack(Visibility value, object parameter, CultureInfo culture)
        {
            return value == Visibility.Visible ?
                Enum.Parse(EnumType, parameter.ToString()) :
                    throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : ConverterBase<bool, Visibility>
    {
        public override Visibility Convert(bool value, object parameter, CultureInfo culture)
        {
            return value ? Visibility.Visible : Visibility.Collapsed;
        }

        public override bool ConvertBack(Visibility value, object parameter, CultureInfo culture)
        {
            return value == Visibility.Visible;
        }
    }

    public class NegativeBoolToVisibilityConverter : ConverterBase<bool, Visibility>
    {
        public override Visibility Convert(bool value, object parameter, CultureInfo culture)
        {
            return !value ? Visibility.Visible : Visibility.Collapsed;
        }

        public override bool ConvertBack(Visibility value, object parameter, CultureInfo culture)
        {
            return value != Visibility.Visible;
        }
    }
}
