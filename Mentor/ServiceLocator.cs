using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mentor.MVVM
{
    public class ServiceLocator
    {
        public static ServiceLocator Instance { get; } = new ServiceLocator();

        private IContainer Container { get; set; }

        private ContainerBuilder _containerBuilder;

        public bool Built { get; private set; }

        public void Build()
        {
            if(Built == false)
            {
                _containerBuilder.Build();
                Built = true;
            }
        }

        public ServiceLocator()
        {
            _containerBuilder = new ContainerBuilder();
        }

        public void RegisterViewModels()
        {
            _containerBuilder.RegisterAssemblyTypes(GetType().Assembly)
                .Where(type => type.Name.EndsWith("ViewModel"))
                .AsSelf()
                .InstancePerDependency(); 
        }
    }
}
