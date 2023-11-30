using Cursus.Entities;
using System.Linq.Expressions;

namespace Cursus.Repositories.Interfaces
{
    public interface IQuizAnswerRepository
    {
        List<QuizAnswer> GetAll();
        QuizAnswer GetAsync(Expression<Func<QuizAnswer, bool>> filter);
        void Create(QuizAnswer quiz);
        void Update(Expression<Func<QuizAnswer, bool>> filter, QuizAnswer quiz);
        void Remove(Expression<Func<QuizAnswer, bool>> filter);

    }
}
