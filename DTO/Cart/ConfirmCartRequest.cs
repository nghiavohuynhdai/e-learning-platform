namespace Cursus.DTO.Cart
{
    public class ConfirmCartRequest
    {
        public string UserId { get; set; }
        public List<string> CourseIds { get; set; }
    }
}
