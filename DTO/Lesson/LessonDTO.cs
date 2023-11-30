namespace Cursus.DTO.Lesson;

public class LessonDTO
{
    public Guid ID { get; set; }
    public string Name { get; set; }
    public int No { get; set; }
    public string Overview { get; set; }
    public string Content { get; set; }
    public string VideoFile { get; set; }
}