using IETT.Entity.Entities;

namespace IETT.Business.Abstract
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}