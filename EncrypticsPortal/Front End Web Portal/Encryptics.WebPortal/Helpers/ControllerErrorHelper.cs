using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Helpers
{
    public static class ControllerErrorHelper
    {
        public static IEnumerable<string> ErrorsFromModelState(this Controller controller)
        {
            return controller.ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
        }
    }
}