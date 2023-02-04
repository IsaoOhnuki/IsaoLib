using SharedLib;
using SharedLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class AServiceRegister : IServiceRegister
    {
        public void RegisterService(IServiceCollection serviceCollection)
        {
            serviceCollection.RegisterService(typeof(IAService), new AService());
        }
    }

    public interface IAService : IService
    {

    }

    public class AService : IAService, INeededParameter<int>
    {
        public void Finish(IServiceCollection services)
        {
        }

        public void Startup(IServiceCollection services)
        {
        }

        public void SetParameter(int parameter)
        {
        }
    }
}
