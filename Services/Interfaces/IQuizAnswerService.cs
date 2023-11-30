using Cursus.DTO;
using Cursus.DTO.QuizAnswer;
using Cursus.Entities;

namespace Cursus.Services.Interfaces
{
    public interface IQuizAnswerService
    {
        Task<ResultDTO<QuizAnswer>> GetQuizById(Guid id);
        Task<ResultDTO<QuizAnswer>> CreateQuiz(CreQuizAnswerReqDTO quizAnswer);
        
        

    }
}
