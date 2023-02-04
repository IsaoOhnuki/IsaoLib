using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SharedLib.Implements
{
    public static class TypeFinder
    {
        public static Type[] FindTypes(Type baseType, bool direct = false)
        {
            return AppDomain.CurrentDomain.GetAssemblies().
                SelectMany(assembly =>
                {
                    return assembly.GetLoadedModules().
                        SelectMany(module =>
                        {
                            return module.GetTypes().
                                Where(type => type.GetInterface(baseType.Name) != null || type.IsSubclassOf(baseType) || (direct && type == baseType));
                        });
                }).Distinct().ToArray();
        }
    }
}
