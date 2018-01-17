// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IoC.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Encryptics.Cryptography;
using Encryptics.Cryptography.Fips;
using Encryptics.WebPortal.Areas.Company.Models;
using Encryptics.WebPortal.Areas.Company.Models.Reports.Malware;
using Encryptics.WebPortal.Areas.Company.Models.Reports.PBP;
using Encryptics.WebPortal.PortalService;
using StructureMap;
using System.Web.Mvc;

namespace Encryptics.WebPortal.DependencyResolution
{
    public static class IoC
    {
        public static IContainer Initialize()
        {
            ObjectFactory.Initialize(register =>
                {
                    register.PullConfigurationFromAppConfig = true;

                    register.Scan(scan =>
                        {
                            scan.TheCallingAssembly();
                            scan.ExcludeType<ReportDefinitionModel>();
                            scan.AddAllTypesOf<ReportDefinitionModel>();
                            scan.ExcludeType<PBPReportDefinition>();
                            scan.ExcludeType<GWReportDefinition>();
                            scan.WithDefaultConventions();
                        });
                    register.For<IControllerActivator>().Use<StructureMapControllerActivator>();
                    register.SelectConstructor(() => new PortalServiceSoapClient());
                    register.For<IHasherCryptoProvider>().Use<Sha256CryptoProvider>();
                    register.For<ReportDefinitionModel>().Singleton();
                });
            return ObjectFactory.Container;
        }
    }
}