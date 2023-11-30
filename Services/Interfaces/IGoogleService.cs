using Google.Apis.Auth;

namespace Cursus.Services.Interfaces
{
    public interface IGoogleService
    {
        Task<GoogleJsonWebSignature.Payload> VerifyGoogleTokenAsync(string idToken);
    }
}
