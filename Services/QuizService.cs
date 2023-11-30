using AutoMapper;
using Cursus.Constants;
using Cursus.DTO;
using Cursus.DTO.Quiz;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using Cursus.Services.Interfaces;

namespace Cursus.Services
{
    public class QuizService : IQuizService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IUserService _userService;
        private readonly ISectionService _sectionService;
        private readonly IMapper _mapper;
        private readonly IInstructorService _instructorService;

        public QuizService(IQuizRepository quizRepository, IUserService userService,
            ISectionService sectionService, IInstructorService instructorService, IMapper mapper)
        {
            _quizRepository = quizRepository;
            _userService = userService;
            _sectionService = sectionService;
            _instructorService = instructorService;
            _mapper = mapper;
        }

        public async Task<ResultDTO<Quiz>> GetQuizById(Guid id)
        {
            try
            {
                var user = await _userService.GetCurrentUser();
                var userId = Guid.Parse(user.Id);
                //check userid
                var quiz = await _quizRepository.GetByIdAsync(id);
                if (quiz != null)
                {
                    return ResultDTO<Quiz>.Success(quiz);
                }

                return ResultDTO<Quiz>.Fail($"Quiz with ID {id} not found.");
            }
            catch (Exception ex)
            {
                return ResultDTO<Quiz>.Fail($"An Fail occurred: {ex.Message}");
            }
        }

        public async Task<ResultDTO<Quiz>> CreateQuiz(Guid sectionId, CreateQuizReq create)
        {
            try
            {
                if (string.IsNullOrEmpty(create.Name))
                {
                    return ResultDTO<Quiz>.Fail("Name cannot be empty.");
                }

                if (create.TimeTaken < 5 && create.TimeTaken > 120)
                {
                    return ResultDTO<Quiz>.Fail("Time must be between 5 and 120.");
                }

                var instructor = await _instructorService.GetCurrentInstructor();
                if (instructor is null)
                    return ResultDTO<Quiz>.Fail(new[] { "Fail to create quiz" });

                var section =
                    await _sectionService.GetAsync(sectionId, instructor.ID);
                if (section is null)
                    return ResultDTO<Quiz>.Fail(new[] { "Section not found" }, 404);

                var quizNo = await _sectionService.CalculateNewSectionItemNo(sectionId);
                if (quizNo == -1) throw new Exception("Fail to create quiz");
                try
                {
                    var quiz = new Quiz
                    {
                        ID = Guid.NewGuid(),
                        No = quizNo,
                        CourseID = section.CourseID,
                        Name = create.Name,
                        TimeTaken = create.TimeTaken,
                        Status = "Active",
                        IntructorID = instructor.ID,
                        SectionID = sectionId,
                        CreateDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow,
                        Questions = create.Questions.Select(questionCreate =>
                        {
                            if (string.IsNullOrWhiteSpace(questionCreate.QuestionName))
                            {
                                throw new ArgumentException("QuestionName cannot be empty.");
                            }

                            var hasCorrectOption = false;
                            var isMutiFalseCount = 0;
                            // check option
                            var options = questionCreate.Options.Select(optionCreate =>
                            {
                                if (string.IsNullOrWhiteSpace(optionCreate.OptionText))
                                {
                                    throw new ArgumentException("OptionText cannot be empty.");
                                }

                                if (optionCreate.Iscorrect)
                                {
                                    hasCorrectOption = true;
                                    isMutiFalseCount++;
                                }


                                return new Option
                                {
                                    Option_ID = Guid.NewGuid(),
                                    OptionText = optionCreate.OptionText,
                                    Iscorrect = optionCreate.Iscorrect
                                };
                            }).ToList();
                            // end check option

                            if (!hasCorrectOption)
                            {
                                throw new ArgumentException(
                                    $"At least one option must be marked as correct (Iscorrect=true) at the question {questionCreate.QuestionName}.");
                            }

                            if (!questionCreate.IsMuti && isMutiFalseCount > 1)
                            {
                                throw new ArgumentException(
                                    $"When IsMuti is false, at one option must be marked as correct at the question {questionCreate.QuestionName}.");
                            }

                            return new Question
                            {
                                QuestionID = Guid.NewGuid(),
                                IsMuti = questionCreate.IsMuti,
                                QuestionName = questionCreate.QuestionName,
                                Options = options
                            };
                        }).ToList()
                    };

                    if (!quiz.Questions.Any())
                    {
                        throw new ArgumentException("At least one question must be provided.");
                    }


                    await _quizRepository.CreateAsync(quiz);
                    return ResultDTO<Quiz>.Success(quiz);
                }
                catch (ArgumentException ex)
                {
                    return ResultDTO<Quiz>.Fail(ex.Message);
                }
            }
            catch (ArgumentException ex)
            {
                return ResultDTO<Quiz>.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                return ResultDTO<Quiz>.Fail($"An error occurred while creating the quiz: {ex.Message}");
            }
        }


        public async Task<ResultDTO<Quiz>> UpdateQuiz(Guid id, UpdateQuizReq quiz)
        {
            try
            {
                if (string.IsNullOrEmpty(quiz.Name))
                {
                    return ResultDTO<Quiz>.Fail("Name cannot be empty.");
                }

                if (quiz.TimeTaken < 5 || quiz.TimeTaken > 120)
                {
                    return ResultDTO<Quiz>.Fail("Time must be between 5 and 120.");
                }

                var instructor = await _instructorService.GetCurrentInstructor();
                if (instructor is null)
                    return ResultDTO<Quiz>.Fail(new[] { "Fail to update quiz" });

                var currentQuiz = await _quizRepository.GetByIdAsync(id);

                if (currentQuiz is null || currentQuiz.IntructorID != instructor.ID)
                    return ResultDTO<Quiz>.Fail(new[] { "Quiz not found" }, 404);

                if (!quiz.Questions.Any())
                {
                    return ResultDTO<Quiz>.Fail("At least one question must be provided.");
                }

                // begin create new and update
                foreach (var question in quiz.Questions)
                {
                    if (string.IsNullOrWhiteSpace(question.QuestionName))
                    {
                        throw new ArgumentException("QuestionName cannot be empty.");
                    }

                    question.IsMuti = question.IsMuti;
                    var hasCorrectOption = false;
                    var isMutiFalseCount = 0;

                    foreach (var option in question.Options)
                    {
                        if (string.IsNullOrWhiteSpace(option.OptionText))
                        {
                            throw new ArgumentException(
                                $"OptionText cannot be empty at question {question.QuestionName}");
                        }

                        if (option.Iscorrect)
                        {
                            hasCorrectOption = true;
                            isMutiFalseCount++;
                        }
                    }

                    if (!hasCorrectOption) // if hasn't option true
                    {
                        throw new ArgumentException(
                            $"At least one option must be marked as correct (Iscorrect=true) at the question {question.QuestionName}.");
                    }

                    if (!question.IsMuti && isMutiFalseCount > 1)
                    {
                        throw new ArgumentException(
                            $"When IsMuti is false, at one option must be marked as correct at the question {question.QuestionName}.");
                    }

                    var existingQuestion =
                        currentQuiz.Questions.FirstOrDefault(q => q.QuestionID == question.QuestionID);

                    if (existingQuestion != null)
                    {
                        // Update existing question
                        existingQuestion.QuestionName = question.QuestionName;
                        existingQuestion.IsMuti = question.IsMuti;

                        // Update existing options or add new options
                        foreach (var option in question.Options)
                        {
                            var existingOption =
                                existingQuestion.Options.FirstOrDefault(o => o.Option_ID == option.Option_ID);

                            if (existingOption != null)
                            {
                                // Update existing option
                                existingOption.OptionText = option.OptionText;
                                existingOption.Iscorrect = option.Iscorrect;
                            }
                            else
                            {
                                throw new ArgumentException($"not found option: {option.OptionText}");
                                //// Add new option
                                //existingQuestion.Options.Add(new Option
                                //{
                                //    Option_ID = Guid.NewGuid(), // Generate a new ID for the new option
                                //    OptionText = option.OptionText,
                                //    Iscorrect = option.Iscorrect
                                //});
                            }
                        }

                        // Additional conditions within the loop for existing questions
                        if (existingQuestion.Options.Count > 6)
                        {
                            throw new ArgumentException(
                                $"A question cannot have more than 5 options. Question: {existingQuestion.QuestionName}");
                        }
                    }
                    else
                    {
                        //throw new ArgumentException($"not found question: {quiz.Name}");
                        // Add new question
                        currentQuiz.Questions.Add(new Question
                        {
                            QuestionID = Guid.NewGuid(), // Generate a new ID for the new question
                            QuestionName = question.QuestionName,
                            IsMuti = question.IsMuti,
                            Options = question.Options.Select(o => new Option
                            {
                                Option_ID = Guid.NewGuid(), // Generate a new ID for each new option
                                OptionText = o.OptionText,
                                Iscorrect = o.Iscorrect
                            }).ToList()
                        });
                    }
                }
                //end create new and update

                currentQuiz.Name = quiz.Name;
                currentQuiz.TimeTaken = quiz.TimeTaken;
                currentQuiz.LastModifiedDate = DateTime.UtcNow;

                _quizRepository.Update(q => q.ID == currentQuiz.ID, currentQuiz);

                return ResultDTO<Quiz>.Success(currentQuiz);
            }
            catch (Exception ex)
            {
                return ResultDTO<Quiz>.Fail($"An error occurred while updating the quiz: {ex.Message}");
            }
        }


        public async Task<ResultDTO<Quiz>> DeleteQuiz(Guid id)
        {
            try
            {
                var existingQuiz = await _quizRepository.GetByIdAsync(id);
                if (existingQuiz != null)
                {
                    var num = await _sectionService.UpdateSectionItemNoAfterDeleteItem(existingQuiz.No,
                        existingQuiz.SectionID);
                    if (num == 0) throw new Exception("Fail to delete quiz");
                    await _quizRepository.RemoveAsync(q => q.ID == id, existingQuiz);
                    return ResultDTO<Quiz>.Success(existingQuiz);
                }

                return ResultDTO<Quiz>.Fail($"Quiz with ID {id} not found.");
            }
            catch (Exception ex)
            {
                return ResultDTO<Quiz>.Fail($"An Fail occurred while deleting the quiz: {ex.Message}");
            }
        }

        public async Task<ResultDTO<Quiz>> UpdateStatus(Guid id)
        {
            try
            {
                var instructor = await _instructorService.GetCurrentInstructor();
                if (instructor is null)
                    return ResultDTO<Quiz>.Fail(new[] { "Fail to update quiz" });

                var currentQuiz = await _quizRepository.GetByIdAsync(id);

                if (currentQuiz is null || currentQuiz.IntructorID != instructor.ID)
                    return ResultDTO<Quiz>.Fail(new[] { "Quiz not found" }, 404);

                // Validate the newStatus, assuming you have specific allowed values in the enum
                if (currentQuiz.Status == Enum.GetName(QuizStatus.InActive))
                {
                    currentQuiz.Status = Enum.GetName(QuizStatus.Active);
                }
                else
                {
                    currentQuiz.Status = Enum.GetName(QuizStatus.InActive);
                }

                _quizRepository.Update(q => q.ID == currentQuiz.ID, currentQuiz);

                return ResultDTO<Quiz>.Success(currentQuiz);
            }
            catch (Exception ex)
            {
                return ResultDTO<Quiz>.Fail($"An error occurred while updating the status of the quiz: {ex.Message}");
            }
        }

        public async Task<ResultDTO<Quiz>> DeleteQuestion(Guid id, Guid questionId)
        {
            try
            {
                var instructor = await _instructorService.GetCurrentInstructor();
                if (instructor is null)
                    return ResultDTO<Quiz>.Fail("User not found");

                //var instructorId = Guid.Parse(instructor.ID);

                var quiz = await _quizRepository.GetByIdAsync(id);
                ;
                var question = quiz.Questions.FirstOrDefault(q => q.QuestionID == questionId);
                if (question == null)
                {
                    return ResultDTO<Quiz>.Fail(
                        $"Quiz with ID {id} not found or question with ID {questionId} not in the quiz.");
                }

                if (quiz.IntructorID != instructor.ID)
                {
                    return ResultDTO<Quiz>.Fail("You don't have permission to delete this question.");
                }

                var result = await _quizRepository.DeleteQuestionAsync(id, questionId);

                if (result)
                {
                    return ResultDTO<Quiz>.Success(quiz, "Question deleted successfully");
                }

                return ResultDTO<Quiz>.Fail($"Question with ID {questionId} not found in quiz with ID {id}.");
            }
            catch (Exception ex)
            {
                return ResultDTO<Quiz>.Fail($"An error occurred while deleting the question: {ex.Message}");
            }
        }

        public async Task<ResultDTO<Quiz>> CreateQuestion(Guid id, CreateQuestionReq quiz)
        {
            try
            {
                var instructor = await _instructorService.GetCurrentInstructor();
                if (instructor is null)
                    return ResultDTO<Quiz>.Fail(new[] { "You don't have permission to add questions to this quiz." });

                var currentQuiz = await _quizRepository.GetByIdAsync(id);

                if (currentQuiz is null || currentQuiz.IntructorID != instructor.ID)
                    return ResultDTO<Quiz>.Fail(new[] { "Quiz not found" }, 404);


                if (string.IsNullOrWhiteSpace(quiz.QuestionName))
                    return ResultDTO<Quiz>.Fail(new[] { "QuestionName cannot be empty." });

                if (quiz.Options == null || !quiz.Options.Any())
                    return ResultDTO<Quiz>.Fail(new[] { "At least one option must be provided." });

                if (!quiz.Options.Any(o => o.Iscorrect))
                    return ResultDTO<Quiz>.Fail(new[]
                        { "At least one option must be marked as correct (Iscorrect=true)." });

                // Thêm câu hỏi vào quiz

                currentQuiz.Questions.Add(new Question
                {
                    QuestionID = Guid.NewGuid(),
                    QuestionName = quiz.QuestionName,
                    IsMuti = quiz.IsMuti,
                    Options = quiz.Options.Select(o => new Option
                    {
                        Option_ID = Guid.NewGuid(),
                        OptionText = o.OptionText,
                        Iscorrect = o.Iscorrect
                    }).ToList()
                });

                _quizRepository.Update(q => q.ID == currentQuiz.ID, currentQuiz);

                return ResultDTO<Quiz>.Success(currentQuiz);
            }
            catch (Exception ex)
            {
                return ResultDTO<Quiz>.Fail($"An error occurred while creating the question: {ex.Message}");
            }
        }
    }
}