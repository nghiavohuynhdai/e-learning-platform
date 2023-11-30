using Cursus.Constants;
using Cursus.DTO;
using Cursus.DTO.QuizAnswer;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using Cursus.Services.Interfaces;
using Cursus.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cursus.Services
{
    public class QuizAnswerService : IQuizAnswerService
    {
        private readonly IQuizAnswerRepository _quizAnsRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IQuizRepository _quizRepository;

        public QuizAnswerService(IQuizAnswerRepository quizRepo, IUnitOfWork unitOfWork, IUserService userService, IQuizRepository quizRepository)
        {
            _quizAnsRepo = quizRepo;
            _unitOfWork = unitOfWork;
            _userService = userService;
            _quizRepository = quizRepository;
        }
        public async Task<ResultDTO<QuizAnswer>> CreateQuiz(CreQuizAnswerReqDTO quizAnswer)
        {
            try
            {
                //check null
                if (quizAnswer == null)
                {
                    return ResultDTO<QuizAnswer>.Fail("All field can not be null");
                }
                //check UserID
                var exitUser = await _userService.GetCurrentUser();
                if (exitUser == null)
                {
                    return ResultDTO<QuizAnswer>.Fail("user not avalidable");
                }
                var userid = Guid.Parse(exitUser.Id);

                var exitPayedUser = await _unitOfWork.OrderRepository
                    .GetAsync(x => x.UserID.Equals(userid) && x.Status.Equals("Completed"));

                if (exitPayedUser is null)
                {
                    return ResultDTO<QuizAnswer>.Fail("User does not pay for this course");
                }
                // check quizID
                var exitQuiz = await _quizRepository.GetByIdAsync(quizAnswer.QuizID);
                var exitQuizId = exitQuiz.ID;

                if (exitQuizId == null)
                {
                    return ResultDTO<QuizAnswer>.Fail("Invalid QuizID");
                }
                //check QuestionAnsID

                var exitQuestionAnsID = (await _quizRepository.GetByIdAsync(exitQuizId)).Questions.Select(x => x.QuestionID).ToList();

                //check score 

                var count = exitQuestionAnsID.Count();
                double scorePerQuestion = 100 / count;
                double totalScore = 0;
                List<Guid> TrueOption = new List<Guid>();
                var lisTrueOption = 0;

                //create quiz answer
                var flag = true;
                try
                {
                    var quiz = new QuizAnswer
                    {
                        Id = new Guid(),
                        QuizId = exitQuizId,
                        UserID = userid,
                        QuestionAnswer = (await Task.WhenAll(quizAnswer.QuestionAnswer.Select(async questionCreate =>
                        {
                            if (questionCreate == null)
                            {
                                throw new ArgumentException("Question cannot be empty.");
                            }
                            var checkQuesID = exitQuestionAnsID.Any(x => x.Equals(questionCreate.QuestionID));

                            if (checkQuesID == false)
                            {
                                flag = false;
                            }



                            var option = await Task.WhenAll(questionCreate.OptionAnswers.Select(async optionCreate =>
                            {
                                if (optionCreate == null)
                                {
                                    throw new ArgumentException("Option cannot be empty.");
                                }

                                var exitOptionAnsID = (await _quizRepository.GetByIdAsync(exitQuizId))
                                        .Questions.FirstOrDefault(x => x.QuestionID == questionCreate.QuestionID)
                                        .Options.Select(x => x.Option_ID).ToList();

                                var checkOptionAnsID = exitOptionAnsID.Any(x => x.Equals(optionCreate.OptionID));
                                if (checkOptionAnsID == false)
                                {
                                    flag = false;
                                }
                                //checkscore


                                var isCorrect = (await _quizRepository.GetByIdAsync(exitQuizId))
                                    .Questions.FirstOrDefault(x => x.QuestionID == questionCreate.QuestionID)
                                    .Options.FirstOrDefault(x => x.Option_ID == optionCreate.OptionID).Iscorrect;


                                var listQuiz = await _quizRepository.GetManyAsync(x => x.ID == exitQuizId);
                                var listQuestion = listQuiz.FirstOrDefault().Questions.FirstOrDefault(x => x.QuestionID == questionCreate.QuestionID);
                                lisTrueOption = listQuestion.Options.FindAll(x => x.Iscorrect).Count();


                                if (isCorrect)
                                {

                                    if (listQuestion.IsMuti == true)
                                    {
                                        if (listQuestion.Options.Count() <= 2)
                                        {
                                            totalScore += scorePerQuestion;
                                        }
                                        else
                                        {
                                            TrueOption.Add(optionCreate.OptionID);
                                        }
                                    }
                                    else
                                    {
                                        totalScore += scorePerQuestion;
                                    }

                                }

                                return new OptionAnswer
                                {
                                    OptionID = optionCreate.OptionID,
                                };

                            }).ToList());

                            //check duplicated OptionIDs
                            if (TrueOption.Count != 0)
                            {
                                for (int i = 0; i < TrueOption.Count() - 1; i++)
                                {
                                    var check = TrueOption[i].Equals(TrueOption[i + 1]);
                                    if (check)
                                    {
                                        throw new ArgumentException("OptionIDs cannot be duplicated.");
                                    }
                                }
                            }

                            if (TrueOption.Count() < lisTrueOption)
                            {
                                var failOption = 0;
                                totalScore = totalScore + failOption;
                                TrueOption.Clear();
                            }
                            else
                            {
                                //case : 3 option True
                                totalScore += scorePerQuestion;
                                TrueOption.Clear();
                            }
                            return new QuestionAnswer
                            {
                                QuestionID = questionCreate.QuestionID,
                                OptionAnswers = option.ToList<OptionAnswer>()
                            };

                        }))).ToList<QuestionAnswer>()

                    };
                    //count score


                    //check Multi option => Score
                    quiz.Score = totalScore;

                    if (flag == false)
                    {
                        return ResultDTO<QuizAnswer>.Fail("Invalid QuestionAnswerID or OptionAnswerID");
                    }
                    _quizAnsRepo.Create(quiz);

                    return ResultDTO<QuizAnswer>.Success(quiz);
                }
                catch (Exception ex)
                {
                    return ResultDTO<QuizAnswer>.Fail($"service fail, error: {ex.Message}");
                }

            }
            catch (Exception ex)
            {
                return ResultDTO<QuizAnswer>.Fail(ex.Message);
            }

        }



        public async Task<ResultDTO<QuizAnswer>> GetQuizById(Guid id)
        {
            try
            {
                var searchQuiz = _quizAnsRepo.GetAsync(quiz => quiz.Id == id);
                if (searchQuiz != null)
                {
                    return ResultDTO<QuizAnswer>.Success(searchQuiz);
                }
                return ResultDTO<QuizAnswer>.Fail("QuizAnswer is null");
            }
            catch (Exception ex)
            {
                return ResultDTO<QuizAnswer>.Fail("service fail");
            }
        }


    }
}
