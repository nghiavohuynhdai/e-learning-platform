namespace Cursus.DTO.Authorization
{
    public class LoginResponseDTO
    {
        public string AccessToken { get; set; }
        public DateTime Expire { get; set; }
    }
}
