using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MVVM
{
    public interface IPropertyChangedNotifier : INotifyPropertyChanged
    {
        void OnPropertyChanged(string propertyName);
    }

    public static class PropertyChangedNotifierExtension
    {
        public static void SetProperty<T>(this IPropertyChangedNotifier notifier, ref T prop, T value, [CallerMemberName] string name = null)
        {
            prop = value;
            notifier.OnPropertyChanged(name);
        }
    }
}
