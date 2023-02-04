using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MVVM
{
    internal class ViewActivator
    {
        public const string ViewModelPostfix = "Model";

        public static FrameworkElement View(string viewName)
        {
            Type[] viewTypes = GetInheritanceTypes(typeof(FrameworkElement)).Where(type => type.Name == viewName).ToArray();
            string viewModelName = viewName + ViewModelPostfix;
            Type[] viewModelTypes = GetInheritanceTypes(typeof(ViewModelBase)).Where(type => type.Name == viewModelName).ToArray();

            if (viewTypes.Length == 1 &&
                viewModelTypes.Length == 1)
            {
                FrameworkElement view = (FrameworkElement)Activator.CreateInstance(viewTypes[0]);
                view.Unloaded += ViewUnloaded;
                ViewModelBase viewModel = (ViewModelBase)Activator.CreateInstance(viewModelTypes[0]);
                view.DataContext = viewModel;
                viewModel.Shown();
                return view;
            }
            else
            {
                return null;
            }
        }

        private static void ViewUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                element.Unloaded -= ViewUnloaded;
                if (element.DataContext is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        private static Type[] GetInheritanceTypes(Type baseType, bool direct = false)
        {
            return AppDomain.CurrentDomain.GetAssemblies().
                SelectMany(assembly =>
                {
                    return assembly.GetLoadedModules().
                        SelectMany(module =>
                        {
                            return module.GetTypes().
                                Where(type => type.IsSubclassOf(baseType) || (direct && type == baseType));
                        });
                }).ToArray();
        }
    }
}
