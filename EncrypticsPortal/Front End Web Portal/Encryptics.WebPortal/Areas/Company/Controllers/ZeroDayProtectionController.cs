using AutoMapper;
using Encryptics.WebPortal.Areas.Company.Models;
using Encryptics.WebPortal.Controllers;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Areas.Company.Controllers
{
    public class ZeroDayProtectionController : PortalServiceAwareController
    {
        public ZeroDayProtectionController(PortalServiceSoap portalService)
            : base(portalService)
        {
        }

        [HttpGet]
        public async Task<ActionResult> ConfigureSettings(long entityId)
        {
            await InitializeViewBagAsync(entityId);

            if (!_hasZDPAccess)
            {
                TempData["ErrorMessage"] =
                    "The ZDP is not enabled for this company. Please contact your sales representative for pricing and information for this feature.";
            }

            var request = new GetCompanyGWConfigRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId
                };

            var response = await _portalService.GetCompanyGWConfigAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetCompanyGWConfigResult != null)
            {
                var model = Mapper.Map<CompanyGWConfig, ZDPSettingsModel>(response.GetCompanyGWConfigResult);

                return View(model);
            }

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateSetting(long entityId, int ftcId, string setting)
        {
            var request = new UpdateGWSettingRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    ftc_id = ftcId,
                    setting = setting
                };

            var response = await _portalService.UpdateGWSettingAsync(request);

            return
                Json(response.UpdateGWSettingResult && response.TokenAuth.Status == TokenStatus.Succes
                         ? new { success = true }
                         : new { success = false });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> PublishSettings(long entityId)
        {
            var request = new InsertGWPublicationRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId
                };

            var response = await _portalService.InsertGWPublicationAsync(request);

            if (response.TokenAuth.Status != TokenStatus.Succes || response.InsertGWPublicationResult.Status != InsertGWPublicationStatus.Success)
            {
                Trace.TraceWarning("Publishing ZDP settings failed: {0}", new object[] { Enum.GetName(typeof(InsertGWPublicationStatus), response.InsertGWPublicationResult.Status) });
                Trace.TraceInformation("Token status: {0}", TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status]);

                return Json(new
                    {
                        success = false,
                        message = "Malware rules were not published successfully."
                    });
            }

            return
                Json(new
                {
                    success = true,
                    message = "Malware rules were successfully published."
                });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableZDP(long entityId, bool? isEnabled)
        {
            var request = new UpdateCompanyGWConfigRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    is_enabled = isEnabled ?? false
                };
            var response = await _portalService.UpdateCompanyGWConfigAsync(request);

            var s = isEnabled ?? false ? "enabled" : "disabled";

            if (response.TokenAuth.Status != TokenStatus.Succes || !response.UpdateCompanyGWConfigResult)
            {
                Trace.TraceWarning("Publishing ZDP settings failed: {0}", new object[] { response.UpdateCompanyGWConfigResult });
                Trace.TraceInformation("Token status: {0}", TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status]);

                return Json(new
                {
                    success = false,
                    message = string.Format("Malware could not be {0}.", s)
                });
            }

            return
                Json(new
                {
                    success = true,
                    message = string.Format("Malware successfully {0}.", s)
                });
        }
    }
}