using AutoMapper;
using Encryptics.WebPortal.Areas.CompanyAdmin.Models;
using Encryptics.WebPortal.Controllers;
using Encryptics.WebPortal.IdentityModel;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Areas.CompanyAdmin.Controllers
{
    public class SoftwareReleasesController : PortalServiceAwareController
    {
        private static readonly IEnumerable<string> _permissions = new[]
            {
                "CompanyAdmin/SoftwareReleases/Create", 
                "CompanyAdmin/SoftwareReleases/Edit",
                "CompanyAdmin/SoftwareReleases/SetGlobalMinimum", 
                "CompanyAdmin/SoftwareReleases/Activate"
            };

        //
        // GET: /CompanyAdmin/SoftwareReleases/
        public SoftwareReleasesController(PortalServiceSoap portalService)
            : base(portalService)
        {
        }

        public async Task<ActionResult> Index()
        {
            await SetViewPermissionsAsync(EncrypticsPrincipal.CurrentEncrypticsUser.EntityId, _permissions);

            var model = new PageableViewModel<SoftwareReleaseModel>
                {
                    CurrentPage = 1,
                    PageSize = 10
                };

            await RetrieveSoftwareReleasesAsync(models => model.DataItems = models);

            await GetPlatformsAsync(platforms =>
                {
                    ViewBag.Platforms =
                        Mapper.Map<SoftwarePlatform[], IEnumerable<SoftwareReleaseProductModel>>(platforms);
                });

            HandleMessages();

            return View(model);
        }

        ////
        //// GET: /CompanyAdmin/SoftwareReleases/Create
        public async Task<ActionResult> Create()
        {
            await GetPlatformsAsync(platforms =>
                {
                    ViewBag.Platforms = platforms.Select(x => new SelectListItem
                        {
                            Text = x.Name,
                            Value = x.Id.ToString(CultureInfo.InvariantCulture)
                        });
                });

            return PartialView("_AddNewSoftwareReleasePartial", new SoftwareReleaseModel());
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

        ////
        //// POST: /CompanyAdmin/SoftwareReleases/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SoftwareReleaseModel model)
        {
            try
            {
                var softwareRelease = Mapper.Map<SoftwareReleaseModel, SoftwareRelease>(model);

                var insertRequest = new InsertSoftwareReleaseRequest(_tokenAuth, softwareRelease);
                var response = await _portalService.InsertSoftwareReleaseAsync(insertRequest);

                if (response.TokenAuth.Status != TokenStatus.Succes ||
                    response.InsertSoftwareReleaseResult.Status != InsertSoftwareReleaseStatus.Success)
                {
                    throw new Exception("Could not create software release.");
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                Trace.TraceInformation(e.StackTrace);

                TempData["ErrorMessage"] = "Could not create software release.";
            }

            return RedirectToAction("Index");
        }

        ////
        //// GET: /CompanyAdmin/SoftwareReleases/Edit/5
        public async Task<ActionResult> Edit(int versionId)
        {
            IEnumerable<SoftwareReleaseModel> softwareReleases = new List<SoftwareReleaseModel>();

            await RetrieveSoftwareReleasesAsync(models => softwareReleases = softwareReleases.Concat(models));

            SoftwareReleaseModel model = softwareReleases.First(x => x.VersionId == versionId);

            return View(model);
        }

        ////
        //// POST: /CompanyAdmin/SoftwareReleases/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SoftwareReleaseModel model)
        {
            try
            {
                SoftwareRelease softwareRelease = Mapper.Map<SoftwareReleaseModel, SoftwareRelease>(model);
                var request = new UpdateSoftwareReleaseRequest(_tokenAuth, softwareRelease);

                var response = await _portalService.UpdateSoftwareReleaseAsync(request);

                if (response.TokenAuth.Status != TokenStatus.Succes ||
                    response.UpdateSoftwareReleaseResult.Status != UpdateSoftwareReleaseStatus.Success)
                {
                    throw new Exception("Update software release failed.");
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                Trace.TraceInformation(e.StackTrace);

                TempData["ErrorMessage"] = "Could not update software release.";
            }

            return RedirectToAction("Index");
        }

        ////
        //// POST: /CompanyAdmin/SoftwareReleases/SetGlobalMinimum
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> SetGlobalMinimum(int versionId)
        {
            var request = new UpdateSoftwareReleaseGlobalMinimumRequest(_tokenAuth, versionId);
            var response = await _portalService.UpdateSoftwareReleaseGlobalMinimumAsync(request);

            if (response.TokenAuth.Status != TokenStatus.Succes ||
                response.UpdateSoftwareReleaseGlobalMinimumResult != UpdateSoftwareReleaseGlobalMinimumStatus.Success)
            {
                switch (response.UpdateSoftwareReleaseGlobalMinimumResult)
                {
                    case UpdateSoftwareReleaseGlobalMinimumStatus.Access_Denied:
                        TempData["ErrorMessage"] = "Access Denied.";
                        break;
                    case UpdateSoftwareReleaseGlobalMinimumStatus.Failed:
                        TempData["ErrorMessage"] = "Failed.";
                        break;
                    case UpdateSoftwareReleaseGlobalMinimumStatus.Is_Global_Minimum:
                        TempData["ErrorMessage"] = "Already global minimum.";
                        break;
                    case UpdateSoftwareReleaseGlobalMinimumStatus.Is_Not_Active:
                        TempData["ErrorMessage"] = "Is not active.";
                        break;
                    case UpdateSoftwareReleaseGlobalMinimumStatus.Version_Not_Exists:
                        TempData["ErrorMessage"] = "Version does not exist.";
                        break;
                }
            }

            return RedirectToAction("Index");
        }

        ////
        //// POST: /CompanyAdmin/SoftwareReleases/Activate
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Activate(int versionId)
        {
            var request = new UpdateSoftwareReleaseActiveStatusRequest(_tokenAuth, versionId, true);
            var response = await _portalService.UpdateSoftwareReleaseActiveStatusAsync(request);

            if (response.TokenAuth.Status != TokenStatus.Succes ||
                response.UpdateSoftwareReleaseActiveStatusResult != UpdateSoftwareReleaseActiveStatusStatus.Success)
            {
                switch (response.UpdateSoftwareReleaseActiveStatusResult)
                {
                    case UpdateSoftwareReleaseActiveStatusStatus.Access_Denied:
                        TempData["ErrorMessage"] = "Access Denied.";
                        break;
                    case UpdateSoftwareReleaseActiveStatusStatus.Failed:
                        TempData["ErrorMessage"] = "Failed.";
                        break;
                    case UpdateSoftwareReleaseActiveStatusStatus.Version_Not_Exists:
                        TempData["ErrorMessage"] = "Version does not exist.";
                        break;
                }
            }

            return RedirectToAction("Index");
        }

        private async Task RetrieveSoftwareReleasesAsync(Action<IEnumerable<SoftwareReleaseModel>> setReleases)
        {
            var releasesRequest = new GetSoftwareReleasesRequest
                {
                    TokenAuth = _tokenAuth
                };

            var releasesResponse = await _portalService.GetSoftwareReleasesAsync(releasesRequest);

            if (releasesResponse.TokenAuth.Status == TokenStatus.Succes &&
                releasesResponse.GetSoftwareReleasesResult != null)
            {
                var softwareReleases = Mapper.Map<SoftwareRelease[],
                    IEnumerable<SoftwareReleaseModel>>(releasesResponse.GetSoftwareReleasesResult);

                setReleases(softwareReleases);
            }
        }
    }
}