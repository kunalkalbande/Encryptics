using AutoMapper;
using Encryptics.WebPortal.ActionResults;
using Encryptics.WebPortal.Areas.Company.Models;
using Encryptics.WebPortal.Controllers;
using Encryptics.WebPortal.Helpers;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ServiceCompanyDLPConfig = Encryptics.WebPortal.PortalService.CompanyDLPConfig;
using ServiceDLPRule = Encryptics.WebPortal.PortalService.DLPRule;
using ServiceDLPRuleType = Encryptics.WebPortal.PortalService.DLPRuleType;

namespace Encryptics.WebPortal.Areas.Company.Controllers
{
    public class PolicyBasedProtectionController : PortalServiceAwareController
    {
        public PolicyBasedProtectionController(PortalServiceSoap portalService)
            : base(portalService)
        {
        }

        [HttpGet]
        public async Task<ActionResult> ConfigurePolicies(long entityId)
        {
            await InitializeViewBagAsync(entityId);

            if (!_hasZDPAccess)
            {
                TempData["ErrorMessage"] =
                    "The PBP is not enabled for this company. Please contact your sales representative for pricing and information for this feature.";
            }

            var request = new GetCompanyDLPConfigRequest
                {
                    entity_id = entityId,
                    TokenAuth = _tokenAuth
                };

            var response = await _portalService.GetCompanyDLPConfigAsync(request);

            var model =
                Mapper.Map<ServiceCompanyDLPConfig, CompanyDLPSettingsModel>(response.GetCompanyDLPConfigResult);

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> AssignRules(long entityId)
        {
            await InitializeViewBagAsync(entityId);

            if (!_hasZDPAccess)
            {
                TempData["ErrorMessage"] =
                    "The PBP is not enabled for this company. Please contact your sales representative for pricing and information for this feature.";
            }

            var model = await GetVisibilityRules(entityId);

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateRuleAssignments(long entityId, IEnumerable<string> rules)
        {
            await InitializeViewBagAsync(entityId);

            var request = new UpdateDLPRulesVisibilityRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    rule_list = rules.Aggregate((current, next) => current + ", " + next)
                };

            var response = await _portalService.UpdateDLPRulesVisibilityAsync(request);

            if (response.UpdateDLPRulesVisibilityResult == UpdateDLPRulesVisibilityStatus.Success &&
                response.TokenAuth.Status == TokenStatus.Succes)
            {
                ViewData["SuccessMessage"] = "Rules updated successfully.";
            }
            else
            {
                ViewData["ErrorMessage"] = "Updating rules failed. ";
            }

            var model = await GetVisibilityRules(entityId);

            return View("AssignRules", model);
        }

        [HttpGet]
        public JsonpResult UpdateSettings()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateSettings(long entityId, CompanyDLPSettingsModel model)
        {
            var dlpSettings = Mapper.Map<CompanyDLPSettingsModel, ServiceCompanyDLPConfig>(model);

            var request = new UpdateCompanyDLPConfigRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    new_settings = dlpSettings
                };

            var response = await _portalService.UpdateCompanyDLPConfigAsync(request);

            if (response.UpdateCompanyDLPConfigResult && response.TokenAuth.Status == TokenStatus.Succes)
            {
                return new JsonResult
                    {
                        Data =
                            new
                                {
                                    @status = "success",
                                    @message = "PBP settings successfully updated."
                                }
                    };
            }

            return new JsonResult
                {
                    Data = new { @status = "error", @message = "Could not update PBP settings." }
                };
        }

        [HttpGet]
        public async Task<ActionResult> GetRuleTypes(int entityId, int policyId)
        {
            ViewBag.CompanyId = entityId;

            var request = new GetDLPRuleTypesRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    policy_id = policyId
                };

            var response = await _portalService.GetDLPRuleTypesAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes)
            {
                var ruleTypes = Mapper.Map<ServiceDLPRuleType[], IEnumerable<DLPRuleTypeModel>>(response.GetDLPRuleTypesResult).OrderBy(t => t.Id);

                return PartialView("_PBPRuleTypes", ruleTypes);
            }

            return PartialView("_PBPRuleTypes");
        }

        [HttpGet]
        public async Task<ActionResult> GetRules(int entityId, DLPPolicyId policyId, int ruleTypeId)
        {
            ViewBag.CompanyId = entityId;
            ViewBag.PolicyId = policyId;
            ViewBag.RuleTypeId = ruleTypeId;

            var request = new GetDLPRulesRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    policy_id = (int)policyId,
                    rule_type_id = ruleTypeId,
                    only_enabled = false
                };

            var response = await _portalService.GetDLPRulesAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes)
            {
                var rules = Mapper.Map<ServiceDLPRule[], IEnumerable<DLPRuleModel>>(response.GetDLPRulesResult).OrderBy(r => r.EntityRuleId);

                return PartialView("_PBPRules", rules);
            }

            return PartialView("_PBPRules");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> EnableRule(long entityId, DLPRuleModel rule)
        {
            var request = new UpdateDLPRuleStatusRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    rule_id = rule.RuleId,
                    entity_rule_id = rule.EntityRuleId,
                    is_enabled = rule.Enabled
                };

            var response = await _portalService.UpdateDLPRuleStatusAsync(request);

            if (response.UpdateDLPRuleStatusResult == UpdateDLPRuleStatusStatus.Success)
            {
                return new JsonResult
                    {
                        Data = new { @status = "success", @message = "PBP rule successfully updated." }
                    };
            }

            return new JsonResult
                {
                    Data = new { @status = "error", @message = "Could not update rule." }
                };
        }

        [HttpGet]
        public JsonpResult Publish()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> Publish(long entityId)
        {
            var request = new InsertDLPPublicationRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId
                };

            var response = await _portalService.InsertDLPPublicationAsync(request);

            if (response.InsertDLPPublicationResult.Status == InsertDLPPublicationStatus.Success)
            {
                return new JsonResult
                    {
                        Data = new { @status = "success", @message = "PBP rules were successfully published." }
                    };
            }

            return new JsonResult
                {
                    Data = new { @status = "error", @message = "PBP rules cannot be published." }
                };
        }

        [HttpGet]
        public ActionResult GetNewUserRuleDetails(long entityId, int ruleTypeId)
        {
            ViewBag.CompanyId = entityId;

            var ruleId = GetRuleId(ruleTypeId);

            var dlpRule = new DLPRuleModel
                {
                    RuleId = ruleId,
                    RuleTypeId = ruleTypeId,
                    Enabled = true,
                    DefaultDRM = DRMType.NoDRM,
                    Severity = SeverityLevel.Level1,
                    PolicyId = DLPPolicyId.UserPolicies,
                    Formats = ruleTypeId == 31 ? @"Enter a domain such as mailhost.com for example." :
                        @"An exact match can be achieved with any number of white space characters between words: I.E. Myocardial infarction",
                    BWList = new string[] { },
                    RegexTerm = ruleTypeId == 31 ? @"^([a-zA-Z0-9][-a-zA-Z0-9]*[a-zA-Z0-9]\.)+([a-zA-Z0-9]{3,5})$" : string.Empty
                };

            return PartialView("_PBPNewRuleDetails", dlpRule);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateRule(int entityId, DLPRuleModel model)
        {
            if (ModelState.IsValid)
            {
                DLPRule rule = Mapper.Map<DLPRuleModel, ServiceDLPRule>(model);

                var request = new UpdateDLPRuleDetailsRequest
                    {
                        TokenAuth = _tokenAuth,
                        entity_id = entityId,
                        rule = rule
                    };

                var response = await _portalService.UpdateDLPRuleDetailsAsync(request);

                if (response.TokenAuth.Status == TokenStatus.Succes && response.UpdateDLPRuleDetailsResult == UpdateDLPRuleDetailsStatus.Success)
                {
                    return new JsonResult
                    {
                        Data =
                            new
                            {
                                @status = "success",
                                @message = "PBP rule successfully updated.",
                                @enabled = rule.IsUsedByCompany,
                                @description = rule.Description
                            }
                    };
                }
            }

            return new JsonResult
                {
                    Data = new { @status = "error", @message = "Could not update rule." }
                };
        }

        [HttpGet]
        public async Task<ActionResult> GetRuleDetails(long entityId, long ruleId, long? entityRuleId)
        {
            ViewBag.CompanyId = entityId;
            var request = new GetDLPRuleRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    rule_id = ruleId,
                    entity_rule_id = entityRuleId
                };

            var response = await _portalService.GetDLPRuleAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetDLPRuleResult != null)
            {
                var model = Mapper.Map<ServiceDLPRule, DLPRuleModel>(response.GetDLPRuleResult);

                return PartialView("_PBPRuleDetails", model);
            }

            return PartialView("_PBPRuleDetails");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> RemoveUserRule(long entityId, long entityRuleId)
        {
            var request = new RemoveDLPRuleRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    entity_rule_id = entityRuleId
                };

            var response = await _portalService.RemoveDLPRuleAsync(request);

            if (!(response.TokenAuth.Status == TokenStatus.Succes && response.RemoveDLPRuleResult == RemoveDLPRuleResult.Success))
            {
                return new JsonResult
                    {
                        Data = new { @status = "error", @message = "Could not remove PBP rule." }
                    };
            }

            return new JsonResult
            {
                Data = new { @status = "success", @message = "PBP rule successfully updated." }
            };
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateNewUserRule(long entityId, DLPRuleModel newRule)
        {
            var newUserRule = Mapper.Map<DLPRuleModel, ServiceDLPRule>(newRule);

            var request = new InsertDLPRuleRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    new_rule = newUserRule
                };

            var response = await _portalService.InsertDLPRuleAsync(request);

            if (!(response.TokenAuth.Status == TokenStatus.Succes && response.InsertDLPRuleResult.Status == InsertDLPRuleStatus.Success))
            {
                return new JsonResult
                    {
                        Data = new { @status = "error", @message = "Could not create new user PBP rule." }
                    };
            }

            ModelState.Clear();

            newRule.EntityRuleId = response.InsertDLPRuleResult.Id;

            string partialViewHtml =
                    ViewRenderer.RenderViewToString("_PBPEnableRule", ControllerContext,
                                                            newRule, true);

            return new JsonResult
                {
                    Data =
                        new
                            {
                                @status = "success",
                                @message = "PBP rule successfully updated.",
                                @enabled = newRule.Enabled,
                                @description = newRule.Description,
                                @newRule = partialViewHtml
                            }
                };
        }

        private async Task<DLPRulesVisibilityModel> GetVisibilityRules(long entityId)
        {
            DLPRulesVisibilityModel model;
            var response = await GetRulesVisibility(entityId);

            if (response.TokenAuth.Status != TokenStatus.Succes)
            {
                ViewData["ErrorMessage"] += "Could not retrieve rules.";

                model = new DLPRulesVisibilityModel();
            }
            else
            {
                model = BuildModel(response.GetDLPRulesVisibilityResult);
            }

            return model;
        }

        private async Task<GetDLPRulesVisibilityResponse> GetRulesVisibility(long entityId)
        {
            var request = new GetDLPRulesVisibilityRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId
                };

            var response = await _portalService.GetDLPRulesVisibilityAsync(request);

            return response;
        }

        private static DLPRulesVisibilityModel BuildModel(DLPRulesVisibility result)
        {
            return new DLPRulesVisibilityModel
                {
                    Policies = result.Policies.Select(p => new PolicyVisibilityModel
                        {
                            Id = p.Id,
                            Description = p.Description,
                            RuleTypes =
                                result.RuleTypes.Where(t => t.PolicyId == p.Id)
                                      .Select(t => new RuleTypeVisibilityModel
                                          {
                                              Id = p.Id,
                                              Description = t.Description,
                                              Rules =
                                                  result.Rules.Where(r => r.RuleTypeId == t.Id)
                                                        .Select(r => new RulesVisibilityModel
                                                            {
                                                                Id = r.Id,
                                                                Description = r.Description,
                                                                IsVisible = r.IsVisible
                                                            }).OrderBy(r => r.Id)
                                          }).OrderBy(t => t.Id)
                        }).OrderBy(p => p.Id)
                };
        }

        private static int GetRuleId(int ruleTypeId)
        {
            switch (ruleTypeId)
            {
                //case 28:
                //    return 211;
                case 30:
                    return 217;
                case 31:
                    return 218;
                default:
                    return 211;
            }
        }
    }
}