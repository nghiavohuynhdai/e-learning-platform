namespace Cursus.DTO.Course
{
    public class UpdateCourseDTO
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string Image { get; set; }
        public string Outcome { get; set; }
        public string VideoIntroduction { get; set; }
        public List<Guid> CatalogIDs { get; set; }
    }
}
