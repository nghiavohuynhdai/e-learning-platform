namespace Cursus.Entities
{
    public class Instructor : BaseEntity
    {
        public Guid UserID { get; set; }
        public string Bio { get; set; }
        public string Career { get; set; }
    }
}