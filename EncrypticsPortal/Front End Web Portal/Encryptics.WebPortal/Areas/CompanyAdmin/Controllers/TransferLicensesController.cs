using Encryptics.WebPortal.Areas.CompanyAdmin.Models;
using Encryptics.WebPortal.Controllers;
using Encryptics.WebPortal.PortalService;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Areas.CompanyAdmin.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class TransferLicensesController : PortalServiceAwareController
    {

        public TransferLicensesController(PortalServiceSoap portalService)
            : base(portalService)
        {
        }

        //
        // GET: /CompanyAdmin/TransferLicenses/
        public ActionResult Index()
        {
            ViewData["SuccessMessage"] = TempData["SuccessMessage"];
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(TransferLicenseModel model)
        {
            if (ModelState.IsValid)
            {
                var request = new InsertLicenseTransferRequest
                {
                    TokenAuth = _tokenAuth,
                    amount = model.TransferAmount,
                    from_entity_id = model.FromEntityId,
                    from_pool = (LicensePoolType)model.FromLicensePool,
                    to_pool = (LicensePoolType)model.ToLicensePool,
                    to_entity_id = model.ToEntityId
                };

                var response = await _portalService.InsertLicenseTransferAsync(request);

                if (response.InsertLicenseTransferResult.Status == InsertLicenseTransferStatus.Success)
                {
                    ModelState.Clear();
                    TempData["SuccessMessage"] = "Licenses transferred successfully.";
                    
                    return RedirectToAction(string.Empty);
                }
                
                ViewData["ErrorMessage"] = "Licenses NOT transferred successfully.";
            }

            return View();
        }
    }
}
