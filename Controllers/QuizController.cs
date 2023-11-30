using System.Threading.Tasks;
using Cursus.Constants;
using Cursus.DTO;
using Cursus.DTO.Quiz;
using Cursus.Entities;
using Cursus.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cursus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly IRedisService _cacheService;

        public QuizController(IQuizService quizService, IRedisService cacheService)
        {
            _quizService = quizService;
            _cacheService = cacheService;
        }

        [HttpGet("get-detail-by-id/{id}")]
        public async Task<ActionResult<Quiz>> GetQuizById(Guid id)
        {
            var result = await _quizService.GetQuizById(id);

            if (result._isSuccess)
            {
                return Ok(result._data);
            }

            if (result._statusCode == 404)
            {
                return NotFound(result._message);
            }

            return StatusCode(result._statusCode, result._message);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<Quiz>> CreateQuiz(Guid sectionId, [FromBody] CreateQuizReq quiz)
        {
            var result = await _quizService.CreateQuiz(sectionId, quiz);
            if (result._isSuccess)
            {
                await _cacheService.RemoveDataAsync(CacheKeyPatterns.CourseDetail + result._data.CourseID);
            }

            return StatusCode(result._statusCode, result);
        }

        [HttpPut("update-quiz-and-create-question")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<Quiz>> UpdateQuiz(Guid QuizId, [FromBody] UpdateQuizReq quiz)
        {
            var result = await _quizService.UpdateQuiz(QuizId, quiz);

            if (result._isSuccess)
            {
                await _cacheService.RemoveDataAsync(CacheKeyPatterns.CourseDetail + result._data.CourseID);
                return Ok(result._data);
            }

            if (result._statusCode == 404)
            {
                return NotFound(result._message);
            }

            return StatusCode(result._statusCode, result._message);
        }

        [HttpPut("create-question")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<Quiz>> CreateQuestion(Guid QuizId, [FromBody] CreateQuestionReq quiz)
        {
            var result = await _quizService.CreateQuestion(QuizId, quiz);

            if (result._isSuccess)
            {
                await _cacheService.RemoveDataAsync(CacheKeyPatterns.CourseDetail + result._data.CourseID);
                return Ok(result._data);
            }

            if (result._statusCode == 404)
            {
                return NotFound(result._message);
            }

            return StatusCode(result._statusCode, result._message);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<bool>> DeleteQuiz(Guid id)
        {
            var result = await _quizService.DeleteQuiz(id);

            if (result._isSuccess)
            {
                await _cacheService.RemoveDataAsync(CacheKeyPatterns.CourseDetail + result._data.CourseID);
                return Ok($"Delete Quiz ID:{id}.");
            }

            if (result._statusCode == 404)
            {
                return NotFound(result._message);
            }

            return StatusCode(result._statusCode, $"Delete Quiz ID:{id} success.");
        }

        [HttpDelete("delete-question/{quizId}/{questionId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<ResultDTO<bool>>> DeleteQuestion(Guid quizId, Guid questionId)
        {
            var result = await _quizService.DeleteQuestion(quizId, questionId);

            if (result._isSuccess)
            {
                await _cacheService.RemoveDataAsync(CacheKeyPatterns.CourseDetail + result._data.CourseID);
                return Ok(result);
            }

            return StatusCode(result._statusCode, result._message);
        }

        [HttpPut("toggle-status/{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var result = await _quizService.UpdateStatus(id);

            if (result._isSuccess)
            {
                await _cacheService.RemoveDataAsync(CacheKeyPatterns.CourseDetail + result._data.CourseID);
                return Ok(result._data);
            }

            return StatusCode(result._statusCode, result._message);
        }
    }
}