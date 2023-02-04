using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLib.Interfaces
{
    public interface IServiceCollection : IDictionary<Type, IService>
    {
        void RegisterService(Type serviceType, IService service);
        T GetService<T>() where T : IService;
    }
}
