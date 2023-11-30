using Cursus.DTO;
using Cursus.DTO.Quiz;
using Cursus.Entities;

namespace Cursus.Services.Interfaces
{
    public interface IQuizService
    {
        Task<ResultDTO<Quiz>> GetQuizById(Guid id);
        Task<ResultDTO<Quiz>> CreateQuiz(Guid sectionId, CreateQuizReq create);
        Task<ResultDTO<Quiz>> UpdateQuiz(Guid id, UpdateQuizReq quiz);
        Task<ResultDTO<Quiz>> DeleteQuiz(Guid id);
        Task<ResultDTO<Quiz>> UpdateStatus(Guid id);
        Task<ResultDTO<Quiz>> DeleteQuestion(Guid id, Guid questionId);
        Task<ResultDTO<Quiz>> CreateQuestion(Guid id, CreateQuestionReq quiz);

    }
}
