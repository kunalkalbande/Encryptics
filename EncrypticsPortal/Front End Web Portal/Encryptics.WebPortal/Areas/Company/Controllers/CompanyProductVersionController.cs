using AutoMapper;
using Encryptics.WebPortal.Areas.Company.Models;
using Encryptics.WebPortal.Controllers;
using Encryptics.WebPortal.IdentityModel;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Areas.Company.Controllers
{
    public class CompanyProductVersionController : PortalServiceAwareController
    {
        private static readonly IEnumerable<string> _permissions = new[]
            {
                "Company/CompanyProductVersion/SetCompanyVersion"
            };

        public CompanyProductVersionController(PortalServiceSoap portalService) : base(portalService)
        {
        }

        //
        // GET: /Company/CompanyProductVersionModel/
        public async Task<ActionResult> Index(long entityId)
        {
            await SetViewPermissionsAsync(EncrypticsPrincipal.CurrentEncrypticsUser.EntityId, _permissions);

            var model = new PageableViewModel<CompanyProductVersionModel>
                {
                    CurrentPage = 1,
                    PageSize = 10
                };

            await InitializeViewBagAsync(entityId);

            var request = new GetSoftwareReleasesRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId
                };
            var response = await _portalService.GetSoftwareReleasesAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes)
            {
                model.DataItems = Mapper.Map<SoftwareRelease[], IEnumerable<CompanyProductVersionModel>>(
                    response.GetSoftwareReleasesResult);
            }

            await GetPlatformsAsync(platforms =>
                {
                    ViewBag.Platforms =
                        Mapper.Map<SoftwarePlatform[], IEnumerable<SoftwareReleaseProductModel>>(platforms);
                });

            HandleMessages();

            return View(model);
        }

        //
        // GET: /Company/CompanyProductVersionModel/SetCompanyVersion
        public async Task<ActionResult> SetCompanyVersion(long entityId, int versionId)
        {
            var request = new UpdateEntitySoftwareReleaseRequest(_tokenAuth, entityId, versionId);
            var response = await _portalService.UpdateEntitySoftwareReleaseAsync(request);

            if (response.TokenAuth.Status != TokenStatus.Succes ||
                response.UpdateEntitySoftwareReleaseResult != UpdateEntitySoftwareReleaseStatus.Success)
            {
                switch (response.UpdateEntitySoftwareReleaseResult)
                {
                    case UpdateEntitySoftwareReleaseStatus.Access_Denied:
                        TempData["ErrorMessage"] = "Access denied.";
                        break;
                    case UpdateEntitySoftwareReleaseStatus.Failed:
                        TempData["ErrorMessage"] = "Nope.";
                        break;
                    case UpdateEntitySoftwareReleaseStatus.Is_Below_Minimum:
                        TempData["ErrorMessage"] = "Below global minimum version.";
                        break;
                    case UpdateEntitySoftwareReleaseStatus.Is_Not_Active:
                        TempData["ErrorMessage"] = "Not active.";
                        break;
                    case UpdateEntitySoftwareReleaseStatus.Version_Not_Exists:
                        TempData["ErrorMessage"] = "Version does not exist.";
                        break;
                }
            }

            return RedirectToAction("Index", new {entityId});
        }

        private async Task GetPlatformsAsync(Action<SoftwarePlatform[]> setPlatforms)
        {
            var platformsResponse =
                await _portalService.GetSoftwarePlatformsAsync(new GetSoftwarePlatformsRequest(_tokenAuth));

            if (platformsResponse.TokenAuth.Status == TokenStatus.Succes &&
                platformsResponse.GetSoftwarePlatformsResult.Any())
            {
                setPlatforms(platformsResponse.GetSoftwarePlatformsResult);
            }
        }
    }
}