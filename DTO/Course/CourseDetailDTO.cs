namespace Cursus.DTO.Course;

public class CourseDetailDTO
{
    public Guid ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    public string Outcome { get; set; }
    public string VideoIntroduction { get; set; }
    public List<CourseSectionDTO> Sections { get; set; }
}

public class CourseSectionDTO
{
    public Guid ID { get; set; }
    public string Name { get; set; }
    public int No { get; set; }
    public string Description { get; set; }
    public List<CourseLessonDTO> Lessons { get; set; }
    public List<CourseAssignmentDTO> Assignments { get; set; }
    public List<CourseQuizDTO> Quizzes { get; set; }
}

public class CourseLessonDTO
{
    public Guid ID { get; set; }
    public string Name { get; set; }
    public int No { get; set; }
    public string Overview { get; set; }
    public int LearningTime { get; set; }
    public string Content { get; set; }
    public string VideoUrl { get; set; }
}

public class CourseAssignmentDTO
{
    public Guid ID { get; set; }
    public int No { get; set; }
    public int TimeTaken { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}

public class CourseQuizDTO
{
    public Guid ID { get; set; }

    public int No { get; set; }

    public string Name { get; set; }

    public int TimeTaken { get; set; }
}