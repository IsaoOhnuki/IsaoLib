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

    public abstract class MultiValueConverterBase<TSource, TTarget> : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            => Convert(values.Select(x => (TSource)x).ToArray(), parameter, culture);

        public abstract TTarget Convert(TSource[] values, object parameter, CultureInfo culture);

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => ConvertBack((TTarget)value, parameter, culture).Select(x => (object)x).ToArray();

        public abstract TSource[] ConvertBack(TTarget value, object parameter, CultureInfo culture);

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    public abstract class EnumConverterBase<TTarget> : ConverterBase<object, TTarget>
    {
        public EnumConverterBase()
        {
            EnumType = typeof(object);
        }

        public EnumConverterBase(Type enumType)
        {
            EnumType = enumType;
        }

        [ConstructorArgument("enumType")]
        public Type EnumType { get; set; }

        public bool ConvertBase(object value, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return false;
            }

            if (parameter != null &&
                parameter.GetType().IsArray &&
                parameter.GetType().GetElementType() == EnumType &&
                parameter is Array array &&
                Enumerable.Range(0, array.Length).Select(x => array.GetValue(x)).ToArray() is object[] enumValues)
            {
                object enumValue = Enum.Parse(EnumType, value.ToString());
                return enumValues.Any(x => x.Equals(enumValue));
            }
            else
            {
                return Enum.Parse(EnumType, value.ToString()).Equals(Enum.Parse(EnumType, parameter.ToString()));
            }
        }

        public override object ConvertBack(TTarget value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumToBooleanConverter : EnumConverterBase<bool>
    {
        public override bool Convert(object value, object parameter, CultureInfo culture)
        {
            return ConvertBase(value, parameter, culture);
        }
    }

    public class EnumToVisibilityConverter : EnumConverterBase<Visibility>
    {
        public override Visibility Convert(object value, object parameter, CultureInfo culture)
        {
            return ConvertBase(value, parameter, culture) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class BooleanToVisibilityConverter : ConverterBase<bool, Visibility>
    {
        public override Visibility Convert(bool value, object parameter, CultureInfo culture)
        {
            bool param = true;
            if (parameter is bool boolParam ||
                (parameter != null &&
                bool.TryParse(parameter.ToString(), out boolParam)))
            {
                param = boolParam;
            }
            return value == param ? Visibility.Visible : Visibility.Collapsed;
        }

        public override bool ConvertBack(Visibility value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MultiBooleanToVisibilityConverter : MultiValueConverterBase<bool, Visibility>
    {
        public override Visibility Convert(bool[] values, object parameter, CultureInfo culture)
        {
            if (parameter is bool[] array)
            {
                if (array.Length == values.Length)
                {
                    return values.Select((v, i) => (v, i)).All(x => x.v == array[x.i]) ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            else
            {
                return values.All(x => x) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public override bool[] ConvertBack(Visibility value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
