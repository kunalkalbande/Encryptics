using StructureMap;
using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Encryptics.WebPortal.DependencyResolution
{
    public class StructureMapControllerActivator : IControllerActivator
    {
        private readonly IContainer _container;

        public StructureMapControllerActivator(IContainer container)
        {
            _container = container;
        }

        public IController Create(RequestContext requestContext, Type controllerType)
        {
            var x = _container.TryGetInstance(controllerType);

            return _container.GetInstance(controllerType) as IController;
        }
    }
}