using System.Web.Mvc;

namespace Encryptics.WebPortal.Models
{
    public class VersionInfoModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            return new VersionInfo(result.AttemptedValue);
        }
    }
}