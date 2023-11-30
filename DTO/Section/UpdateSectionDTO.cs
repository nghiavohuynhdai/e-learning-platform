namespace Cursus.DTO.Section
{
    public class UpdateSectionDTO
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        public ResultDTO Validate()
        {
            var errorMessages = new List<string>();
            if (string.IsNullOrEmpty(Name))
            {
                errorMessages.Add("Name is required");
            }

            return errorMessages.Count == 0 ? ResultDTO.Success() : ResultDTO.Fail(errorMessages, 400);
        }
    }
}