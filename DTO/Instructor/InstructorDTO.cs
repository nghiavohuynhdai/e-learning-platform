using Cursus.DTO.User;

namespace Cursus.DTO.Instructor;

public class InstructorDTO : UserDTO
{
    public string Bio { get; set; }
    public string Career { get; set; }
}