using AutoMapper;
using Encryptics.WebPortal.Areas.Company.Models;
using Encryptics.WebPortal.Controllers;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Mvc;
using PortalServiceCompanyDomain = Encryptics.WebPortal.PortalService.CompanyDomain;

namespace Encryptics.WebPortal.Areas.Company.Controllers
{
    public class CompanyDomainController : PortalServiceAwareController
    {
        private readonly IEnumerable<string> _domainManagementPermissions = new[] { "Company/CompanyDomain/AddDomain", "Company/CompanyDomain/RemoveDomain" };

        public CompanyDomainController(PortalServiceSoap portalService)
            : base(portalService)
        {

        }

        //
        // GET: /Company/Domain/
        public async Task<ActionResult> Index(long entityId)
        {
            await InitializeViewBagAsync(entityId);

            var response = await _portalService.GetCompanyDomainsAsync(new GetCompanyDomainsRequest(_tokenAuth, entityId));

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetCompanyDomainsResult != null)
            {
                var model = Mapper.Map<PortalServiceCompanyDomain[], IEnumerable<CompanyDomainModel>>(
                            response.GetCompanyDomainsResult);

                HandleMessages();

                await SetViewPermissionsAsync(entityId, _domainManagementPermissions);

                return View(model);
            }

            Trace.TraceError("Token error: {0}",
                             (object)TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status]);

            TempData["ErrorMessage"] = "Could not retrieve entity domain assignments.";

            return RedirectToAction("Dashboard", "CompanyHome");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> AddDomain(long entityId, string domainName)
        {
            if (!string.IsNullOrEmpty(domainName))
            {
                var response = await _portalService.InsertCompanyDomainAsync(new InsertCompanyDomainRequest(_tokenAuth, entityId, domainName));

                if (response.InsertCompanyDomainResult.Status == InsertCompanyDomainStatus.Not_Available)
                {
                    Trace.TraceWarning(string.Format("Domain {0} already in use.", domainName));
                    TempData["ErrorMessage"] = "Domain could not be added because it is already in use.";
                }
                else if (!(response.InsertCompanyDomainResult.Status == InsertCompanyDomainStatus.Success && response.TokenAuth.Status == TokenStatus.Succes))
                {
                    Trace.TraceWarning("InsertCompanyDomainResult: {0}", new object[] { response.InsertCompanyDomainResult.Status });
                    Trace.TraceWarning("Token Status: {0}", new[] { _tokenAuth.Status });

                    TempData["ErrorMessage"] = "Domain could not be added for unknown reasons.";
                }
            }

            return RedirectToAction("Index", new { entityId });
        }

        [HttpGet]
        public async Task<ActionResult> RemoveDomain(long entityId, long[] domainIds)
        {
            foreach (var domId in domainIds)
            {
                var response = await _portalService.RemoveCompanyDomainAsync(new RemoveCompanyDomainRequest(_tokenAuth, entityId, domId));
                if (response.RemoveCompanyDomainResult == RemoveCompanyDomainResult.Success) continue;
                TempData["ErrorMessage"] = "Domain could not be removed.";
                break;
            }

            return RedirectToAction("Index", new { entityId });
        }
    }
}
