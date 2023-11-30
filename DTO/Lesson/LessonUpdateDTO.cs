namespace Cursus.DTO.Lesson;

public class LessonUpdateDTO
{
    public string Name { get; set; }
    public string Overview { get; set; }
    public string Content { get; set; }
    public string VideoUrl { get; set; }
    public int LearningTime { get; set; }

    public ResultDTO Validate()
    {
        var errorMessages = new List<string>();
        if (string.IsNullOrEmpty(Name))
            errorMessages.Add("Name is required");

        return errorMessages.Count == 0 ? ResultDTO.Success() : ResultDTO.Fail(errorMessages, 400);
    }
}