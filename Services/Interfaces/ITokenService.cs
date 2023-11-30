using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Cursus.Repositories
{
    public interface ITokenService
    {
        JwtSecurityToken GetToken(Claim userClaim, IEnumerable<string> userRoles, int expireTime);
    }
}
