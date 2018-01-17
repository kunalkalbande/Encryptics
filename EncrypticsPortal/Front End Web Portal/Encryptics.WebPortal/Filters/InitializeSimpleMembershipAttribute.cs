using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Threading;
using System.Web.Mvc;
using System.Web.Security;
using Encryptics.WebPortal.Models;
using WebMatrix.WebData;

namespace Encryptics.WebPortal.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InitializeSimpleMembershipAttribute : ActionFilterAttribute
    {
        private static SimpleMembershipInitializer _initializer;
        private static object _initializerLock = new object();
        private static bool _isInitialized;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Ensure ASP.NET Simple Membership is initialized only once per app start
            LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);
        }

        internal class SimpleMembershipInitializer
        {
            public SimpleMembershipInitializer()
            {
                Database.SetInitializer<UsersContext>(null);

                try
                {
                    using (var context = new UsersContext())
                    {
                        var connectionStringSettings =
                            ConfigurationManager.ConnectionStrings["DefaultConnection"];
                        var builder = new SqlConnectionStringBuilder(connectionStringSettings.ConnectionString);

                        if (builder.DataSource.ToLower().Contains("localdb") && !context.Database.Exists())
                        {
                            // Create the SimpleMembership database without Entity Framework migration schema
                            ((IObjectContextAdapter) context).ObjectContext.CreateDatabase();
                            Roles.CreateRole("User");
                            Roles.CreateRole("Admin");
                            Roles.CreateRole("SuperAdmin");
                        }
                    }

                    WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName",
                                                             true);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        "The ASP.NET Simple Membership database could not be initialized. For more information, please see http://go.microsoft.com/fwlink/?LinkId=256588",
                        ex);
                }
            }
        }
    }
}