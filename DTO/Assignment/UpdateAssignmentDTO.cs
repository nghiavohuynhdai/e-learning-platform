namespace Cursus.DTO.Assignment;

public class UpdateAssignmentDTO
{
    public string? Title { get; set; }

    public string? Description { get; set; }
    
    public int TimeTaken { get; set; }

    public ResultDTO Validate()
    {
        var errorMessages = new List<string>();

        if (string.IsNullOrEmpty(Title) || Title?.Length >= 100)
            errorMessages.Add("Title is required and must has less or equal to 100 characters");

        if (string.IsNullOrEmpty(Description))
            errorMessages.Add("Description is required");
        
        if (TimeTaken is < 1 or > 120)
            errorMessages.Add("TimeTaken must be between 1 and 120 minutes");

        return errorMessages.Count == 0 ? ResultDTO.Success() : ResultDTO.Fail(errorMessages, 400);
    }
}