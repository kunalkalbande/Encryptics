namespace Encryptics.WebPortal.Models
{
    public interface IUserRepository
    {
        void CreateUserProfile(string userName, string password);
    }
}