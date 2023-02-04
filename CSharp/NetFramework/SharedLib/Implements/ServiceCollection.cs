using SharedLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib.Implements
{
    public class ServiceCollection : Dictionary<Type, IService>, IServiceCollection
    {
        public T GetService<T>() where T : IService
        {
            return (T)this[typeof(T)];
        }

        public void RegisterService(Type serviceType, IService service)
        {
            Add(serviceType, service);
        }
    }
}
