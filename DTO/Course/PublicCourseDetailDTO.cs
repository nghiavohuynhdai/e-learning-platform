namespace Cursus.DTO.Course
{
    public class PublicCourseDetailDTO
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Outcome { get; set; }
        public string VideoIntroduction { get; set; }
        public List<PublicCourseSectionDTO> Sections { get; set; }
    }

    public class PublicCourseSectionDTO
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string No { get; set; }
        public string Description { get; set; }
        public List<PublicCourseLessonDTO> Lessons { get; set; }
        public List<PublicCourseAssignmentDTO> Assignments { get; set; }
        public List<PublicCourseQuizDTO> Quizzes { get; set; }
    }

    public class PublicCourseLessonDTO
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public int LearningTime { get; set; }
        public int No { get; set; }
        public string Overview { get; set; }
    }

    public class PublicCourseAssignmentDTO
    {
        public Guid ID { get; set; }
        public int No { get; set; }
        public int LearningTime { get; set; }
        public string Title { get; set; }
    }

    public class PublicCourseQuizDTO
    {
        public Guid ID { get; set; }

        public int No { get; set; }

        public string Name { get; set; }

        public int TimeTaken { get; set; }
    }
}