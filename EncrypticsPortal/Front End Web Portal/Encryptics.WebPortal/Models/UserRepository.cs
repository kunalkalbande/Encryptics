using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
//using System.Linq;
using System.Web.Security;
using WebMatrix.WebData;

namespace Encryptics.WebPortal.Models
{
    public class UserRepository : IUserRepository
    {
        private SqlConnectionStringBuilder _builder;

        public SqlConnectionStringBuilder Builder
        {
            get
            {
                if (_builder == null)
                {
                    var connectionStringSettings =
                        ConfigurationManager.ConnectionStrings["DefaultConnection"];
                    _builder = new SqlConnectionStringBuilder(connectionStringSettings.ConnectionString);
                }
               
                return _builder;
            }
        }

        public void CreateUserProfile(string userName, string password)
        {
            Database.SetInitializer<UsersContext>(null);

            using (var context = new UsersContext())
            {
                CreateLocalDatabaseIfNeeded(context);

                // check if user exists with this username
                WebSecurity.UserExists(userName);
                //IQueryable<UserProfile> existingUsers = from u in context.UserProfiles
                //                                        where u.UserName == userName
                //                                        select u;

                // if any exist exit
                //if (existingUsers.Any()) return;


                // otherwise create the new user
                WebSecurity.CreateUserAndAccount(userName, password);
                //context.UserProfiles.Add(new UserProfile
                //    {
                //        UserName = userName
                //    });

                //context.SaveChanges();
            }
        }

        private void CreateLocalDatabaseIfNeeded(DbContext context)
        {
            if (!Builder.DataSource.ToLower().Contains("localdb") || context.Database.Exists()) return;
            // Create the SimpleMembership database without Entity Framework migration schema
            ((IObjectContextAdapter) context).ObjectContext.CreateDatabase();
            Roles.CreateRole("User");
            Roles.CreateRole("Admin");
            Roles.CreateRole("SuperAdmin");
        }
    }
}