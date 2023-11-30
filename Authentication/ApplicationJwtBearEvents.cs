using Cursus.Constants;
using Cursus.Data;
using Cursus.DTO;
using Cursus.Entities;
using Cursus.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace Cursus.Authentication;

public class ApplicationJwtBearEvents : JwtBearerEvents
{
    public ApplicationJwtBearEvents()
    {
        OnChallenge = _onChallenge;
        OnTokenValidated = _onTokenValidated;
        OnForbidden = _onForbidden;
    }

    private readonly Func<JwtBearerChallengeContext, Task> _onChallenge =
        async delegate(JwtBearerChallengeContext context)
        {
            var ex = context.AuthenticateFailure;

            context.Response.OnStarting(async () =>
            {
                var message = "";
                if (ex is not null)
                {
                    message = "Invalid token";
                    if (ex.GetType() == typeof(SecurityTokenExpiredException))
                        message = "Expired token";
                }

                await context.Response.WriteAsJsonAsync(ResultDTO<string>.Fail(message, context.Response.StatusCode));
            });
        };

    private readonly Func<TokenValidatedContext, Task> _onTokenValidated = async delegate(TokenValidatedContext context)
    {
        var userIdClaim = context.Principal.Claims.FirstOrDefault(claim => claim.Type == "Id");
        if (userIdClaim is not null)
        {
            var _redisService = context.HttpContext.RequestServices.GetService<IRedisService>();
            var key = CacheKeyPatterns.User + userIdClaim.Value;
            var isCached = false;

            var user = await _redisService.GetDataAsync<User>(key);

            if (user is null)
            {
                var _dbContext = context.HttpContext.RequestServices.GetService<MyDbContext>();
                try
                {
                    user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Id == userIdClaim.Value);
                    if (user is not null)
                    {
                        await _redisService.SetDataAsync(key, user, TimeSpan.FromDays(1));
                    }
                    else
                    {
                        context.Fail("Invalid token");
                        return;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (user.Status != Enum.GetName(UserStatus.Disable))
            {
                context.Success();
                return;
            }
        }

        context.Fail("Invalid token");
    };

    private readonly Func<ForbiddenContext, Task> _onForbidden = async delegate(ForbiddenContext context)
    {
        context.Response.OnStarting(async Task() =>
        {
            await context.Response.WriteAsJsonAsync(ResultDTO<string>.Fail(
                    "You are not allow to access this resource", context.Response.StatusCode
                )
            );
        });
    };
}