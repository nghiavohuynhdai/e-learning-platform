using Microsoft.AspNetCore.Identity;

namespace Cursus.Entities

{
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public string Status { get; set; }
        public string? Image { get; set; }
        public string Gender { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
