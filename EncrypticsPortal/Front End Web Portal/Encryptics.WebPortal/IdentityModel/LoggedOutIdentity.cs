using System.Security.Principal;

namespace Encryptics.WebPortal.IdentityModel
{
    public class LoggedOutIdentity : IIdentity
    {
        public string Name { get; private set; }
        public string AuthenticationType { get { return string.Empty; } }
        public bool IsAuthenticated { get { return false; } }

        public LoggedOutIdentity(string name)
        {
            Name = name;
        }
    }
}