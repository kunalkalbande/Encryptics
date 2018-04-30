using AutoMapper;
using Encryptics.WebPortal.ActionResults;
using Encryptics.WebPortal.Areas.UserAccount.Models;
using Encryptics.WebPortal.Controllers;
using Encryptics.WebPortal.Filters;
using Encryptics.WebPortal.Helpers;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.SessionState;
using DeviceStatus = Encryptics.WebPortal.PortalService.DeviceStatus;
using MyResources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Areas.UserAccount.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly), AuthorizeAction/*Authorize*/]
    public class DeviceController : PortalServiceAwareController
    {
        public DeviceController(PortalServiceSoap portalService)
            : base(portalService)
        {
        }

        public string MobileDeviceModel
        {
            get { return Request.Browser.MobileDeviceModel.ToLower(); }
        }

        public int DeviceListPageSize
        {
            get { return MobileDeviceModel == "unknown" || MobileDeviceModel == "ipad" ? 4 : 1; }
        }

        [HttpGet]
        public JsonpResult AjaxDeactivateSession()
        {
            return GetJsonpAntiforgeryToken();
        }

        // POST: /UserAccount/Device/AjaxDeactivateSession
        [HttpPost, ValidateHttpAntiForgeryToken]
        public async Task<ActionResult> AjaxDeactivateSession(long deviceId, long? entityId, long? userId)
        {
            Trace.TraceInformation(string.Format("Disabling Session on Device: {0}", deviceId));
            Trace.TraceInformation(string.Format("EntityId: {0}", entityId ?? _encrypticsUser.EntityId));
            Trace.TraceInformation(string.Format("UserId: {0}", userId ?? _encrypticsUser.UserId));

            var response =
                await _portalService.ExpireTokenSessionAsync(new ExpireTokenSessionRequest
                    {
                        TokenAuth = _tokenAuth,
                        entity_id = entityId ?? _encrypticsUser.EntityId,
                        token_id = deviceId,
                        user_id = userId ?? _encrypticsUser.UserId
                    });

            Trace.TraceInformation(string.Format("Response: {0}", response.ExpireTokenSessionResult));
            Trace.TraceInformation(string.Format("Token Status: {0}",
                                                 TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status]));

            if (response.ExpireTokenSessionResult && response.TokenAuth.Status == TokenStatus.Succes)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, errors = new[] { MyResources.CouldNotDeactivateDeviceSessionErrorMessage } });
        }

        [HttpGet]
        public JsonpResult AjaxDeactivateDevice()
        {
            return GetJsonpAntiforgeryToken();
        }

        // POST: /UserAccount/Device/AjaxDeactivateDevice
        [HttpPost, ValidateHttpAntiForgeryToken]
        public async Task<ActionResult> AjaxDeactivateDevice(long deviceId, long? entityId)
        {
            Trace.TraceInformation(string.Format("Deactivating Device: {0}", deviceId));
            Trace.TraceInformation(string.Format("EntityId: {0}", entityId ?? _encrypticsUser.EntityId));

            var response =
                await _portalService.UpdateDeviceStatusAsync(new UpdateDeviceStatusRequest
                    {
                        TokenAuth = _tokenAuth,
                        entity_id = entityId ?? _encrypticsUser.EntityId,
                        device_id = deviceId,
                        new_status = DeviceStatus.Suspended
                    });

            Trace.TraceInformation(string.Format("Response: {0}", response.UpdateDeviceStatusResult));
            Trace.TraceInformation(string.Format("Token Status: {0}",
                                                 TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status]));

            return response.UpdateDeviceStatusResult
                       ? Json(new { success = true })
                       : Json(new { success = false, errors = new[] { MyResources.CouldNotDeactivateDeviceErrorMessage } });
        }

        [HttpGet]
        public JsonpResult AjaxActivateDevice()
        {
            return GetJsonpAntiforgeryToken();
        }

        // POST: /UserAccount/Device/AjaxActivateDevice
        [HttpPost, ValidateHttpAntiForgeryToken]
        public async Task<ActionResult> AjaxActivateDevice(long deviceId, long? entityId)
        {
            Trace.TraceInformation(string.Format("Activating Device: {0}", deviceId));
            Trace.TraceInformation(string.Format("EntityId: {0}", entityId ?? _encrypticsUser.EntityId));

            var response =
                await _portalService.UpdateDeviceStatusAsync(new UpdateDeviceStatusRequest
                    {
                        TokenAuth = _tokenAuth,
                        entity_id = entityId ?? _encrypticsUser.EntityId,
                        device_id = deviceId,
                        new_status = DeviceStatus.Active
                    });

            Trace.TraceInformation(string.Format("Response: {0}", response.UpdateDeviceStatusResult));
            Trace.TraceInformation(string.Format("Token Status: {0}", TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status]));

            return response.UpdateDeviceStatusResult
                       ? Json(new { success = true })
                       : Json(new { success = false, errors = new[] { MyResources.CouldNotActivateDeviceErrorMessage } });
        }

        [HttpGet]
        public JsonpResult AjaxRemoveDevice()
        {
            return GetJsonpAntiforgeryToken();
        }

        // POST: /UserAccount/Device/AjaxRemoveDevice
        [HttpPost, ValidateHttpAntiForgeryToken]
        public async Task<ActionResult> AjaxRemoveDevice(long deviceId, int? page, long? entityId, long? userId, string viewPath = "_DevicePartial")
        {
            // Must retrieve device list before removing this device to get accurate listing to calculate next device to render upon successful return
            var devices = await GetDevicesAsync(entityId, userId);

            Trace.TraceInformation(string.Format("Removing Device: {0}", deviceId));
            Trace.TraceInformation(string.Format("EntityId: {0}", entityId ?? _encrypticsUser.EntityId));
            Trace.TraceInformation(string.Format("UserId: {0}", userId ?? _encrypticsUser.UserId));

            // Remove the device requested
            var response =
                 _portalService.RemoveUserDevice(new RemoveUserDeviceRequest
                    {
                        TokenAuth = _tokenAuth,
                        entity_id = entityId ?? _encrypticsUser.EntityId,
                        user_id = userId ?? _encrypticsUser.UserId,
                        device_id = deviceId
                    });

            Trace.TraceInformation(string.Format("Response: {0}", response.RemoveUserDeviceResult));
            Trace.TraceInformation(string.Format("Token Status: {0}", TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status]));

            // If successful render the HTML of the next device in the list following the one we removed.
            if (response.RemoveUserDeviceResult && response.TokenAuth.Status == TokenStatus.Succes)
            {
                var deviceModels = (devices as IList<DeviceModel> ?? devices.ToList()).ToList();

                var skipCount = (++page ?? 1) * DeviceListPageSize; // How many do we need to skip?
                var adjustedDeviceCount = deviceModels.Count - 1; // subtract 1 to account for the one we removed.
                var nextDevice = deviceModels.Skip(skipCount).FirstOrDefault(); //  this should be the next device.
                
                // Set up the ViewDataDictionary so we have all the data filled in when the partial view is renederd.
                ViewData["EntityId"] = entityId ?? _encrypticsUser.EntityId;
                ViewData["UserId"] = userId ?? _encrypticsUser.UserId;

                // Render the partial view that represents the next device to fill in after removal of requested device.
                var partialViewHtml = nextDevice != null ?
                    ViewRenderer.RenderViewToString(viewPath, ControllerContext, nextDevice, true) :
                    string.Empty;

                Debug.Print("Returning HTML: {0}", partialViewHtml);

                // return JSON containting all data needed
                return Json(
                    new
                    {
                        success = true,
                        nextDeviceHtml = partialViewHtml,
                        removeNextLink = (skipCount == adjustedDeviceCount)
                    });
            }

            // Removal failed so return with an error.
            return Json(new { success = false, errors = new[] { MyResources.CouldNotRemoveDeviceErrorMessage } });
        }

        // GET: /UserAccount/Device/AjaxDevices
        [HttpGet]
        public async Task<ActionResult> AjaxDevices(int? page, long? entityId, long? userId, string viewPath = "_DevicesPartial")
        {
            try
            {
                var devices = await GetDevicesAsync(entityId, userId);

                var pageableData = new PageableViewModel<DeviceModel>
                {
                    PageSize = DeviceListPageSize,
                    CurrentPage = page ?? 1,
                    DataItems = devices
                };

                ViewData["CurrentPage"] = pageableData.CurrentPage;
                ViewData["EntityId"] = entityId ?? _encrypticsUser.EntityId;
                ViewData["UserId"] = userId ?? _encrypticsUser.UserId;

                var partialViewHtml =
                    ViewRenderer.RenderViewToString(viewPath, ControllerContext, pageableData, true);
                Debug.Print("Returning HTML: {0}", partialViewHtml);

                var antiForgeryToken = this.GetAntiForgeryToken();
                Debug.Print("antiForgeryToken = {0}", antiForgeryToken);

                return Json(new { success = true, token = antiForgeryToken, html = partialViewHtml },
                            JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format("Error: {0}", e.Message));
                return Json(new
                {
                    success = false,
                    errors = new[] { MyResources.CouldNotRetrieveDevicesErrorMessage }
                },
                            JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /UserAccount/Device/ViewDevices
        [HttpGet]
        public ActionResult ViewDevices(int? page)
        {
            var devices = GetDevices(null, null);

            var pageableData = new PageableViewModel<DeviceModel>
            {
                PageSize = DeviceListPageSize,
                CurrentPage = page ?? 1,
                DataItems = devices,
            };

            ViewData["EntityId"] = _encrypticsUser.EntityId;
            ViewData["UserID"] = _encrypticsUser.UserId;

            return PartialView("_UserDevicesPartial", pageableData);
        }

        private IEnumerable<DeviceModel> GetDevices(long? entityId, long? userId)
        {
            try
            {
                var getDeviceListRequest = BuildGetDeviceListRequest(entityId, userId);

                var response = _portalService.GetDeviceList(getDeviceListRequest);

                IEnumerable<DeviceModel> devices = Mapper.Map<Device[], IEnumerable<DeviceModel>>(response.GetDeviceListResult)
                    .OrderByDescending(x => x.HasActiveSession).ThenByDescending(x => x.DateDeployed);

                return devices;
            }
            catch
            {
                return new List<DeviceModel>();
            }
        }

        private async Task<IEnumerable<DeviceModel>> GetDevicesAsync(long? entityId, long? userId)
        {
            var getDeviceListRequest = BuildGetDeviceListRequest(entityId, userId);

            var response = await _portalService.GetDeviceListAsync(getDeviceListRequest);

            IEnumerable<DeviceModel> devices = Mapper.Map<Device[], IEnumerable<DeviceModel>>(response.GetDeviceListResult)
                .OrderByDescending(x => x.HasActiveSession).ThenByDescending(x => x.DateDeployed);

            return devices;
        }

        private GetDeviceListRequest BuildGetDeviceListRequest(long? entityId, long? userId)
        {
            return new GetDeviceListRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId ?? _encrypticsUser.EntityId,
                    user_id = userId ?? _encrypticsUser.UserId
                };
        }
    }
}