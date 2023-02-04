using SharedLib.Interfaces;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib.Implements
{
    public class ServiceManager : IDisposable
    {
        private static ServiceManager _serviceManager;
        public static ServiceManager GetInstance()
        {
            return _serviceManager ?? (_serviceManager = new ServiceManager());
        }

        private ServiceManager()
        {
            Register();
        }

        public ServiceCollection Services { get; } = new ServiceCollection();

        private void Register()
        {
            Type[] types = TypeFinder.FindTypes(typeof(IServiceRegister));
            types.ToList().ForEach(type =>
            {
                if (type.GetInterface(nameof(IServiceRegister)) is Type)
                {
                    IServiceRegister serviceRegister = (IServiceRegister)
                        Activator.CreateInstance(type);
                    serviceRegister.RegisterService(Services);
                }
            });
        }

        public void Initialize(object[] neededParameters, IDictionary<Type, Type> typeResolver = null)
        {
            if (_initialize)
            {
                return;
            }

            _initialize = true;
            Services.ToList().ForEach(service =>
            {
                string neededName = typeof(INeededParameter<object>).Name;
                service.Value.GetType().GetInterfaces().
                    Where(x => x.Name == neededName && x.IsGenericType).ToList().
                        ForEach(type =>
                        {
                            Type neededType = type.GenericTypeArguments[0];
                            object neededParam = null;

                            object[] parameters = neededParameters.
                                Where(param => param.GetType() == neededType || param.GetType().IsSubclassOf(neededType)).Select(x => x).ToArray();

                            if (parameters.Length == 1)
                            {
                                neededParam = parameters[0];
                            }
                            else if (parameters.Length > 1 &&
                                typeResolver != null &&
                                typeResolver.ContainsKey(neededType) &&
                                parameters.Any(x => x.GetType() == typeResolver[neededType]))
                            {
                                neededParam = parameters.First(x => x.GetType() == typeResolver[neededType]);
                            }
                            else
                            {
                                throw new AggregateException(string.Format("Not find needed parameter '{0}'", neededType.Name));
                            }

                            string methodName = nameof(INeededParameter<object>.SetParameter);
                            service.Value.GetType().GetMethods().
                                Where(method => method.Name == methodName && method.GetParameters().Any(x => x.ParameterType == neededType)).ToList().
                                    ForEach(method =>
                                    {
                                        method.Invoke(service.Value, new object[] { neededParam });
                                    });
                        });

                service.Value.Startup(Services);
            });
        }
        private bool _initialize;

        public void Dispose()
        {
            if (_initialize)
            {
                _initialize = false;
                Services.ToList().
                    ForEach(service => service.Value.Finish(Services));
                Services.Select(service => service.Value).OfType<IDisposable>().ToList().
                    ForEach(service => service.Dispose());
                Services.Clear();
            }
        }
    }
}
