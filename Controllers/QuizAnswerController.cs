using Cursus.DTO.QuizAnswer;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using Cursus.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cursus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizAnswerController : ControllerBase
    {
        private readonly IQuizAnswerService _quizAnswer;

        public QuizAnswerController(IQuizAnswerService quizAnswer)
        {
            _quizAnswer = quizAnswer;
        }
        [HttpGet("get-detail-by-id/{id}")]
        public async Task<ActionResult<QuizAnswer>> GetQuizById(Guid id)
        {
            var result = await _quizAnswer.GetQuizById(id);

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

        [HttpPost("add-quiz-answer")]
        public async Task<ActionResult<QuizAnswer>> CreateQuiz([FromBody] CreQuizAnswerReqDTO quiz)
        {
            var result = await _quizAnswer.CreateQuiz(quiz);

            if (result._isSuccess)
            {
                return Created($"/api/quiz/{result._data.Id}", result._data);
            }

            return StatusCode(result._statusCode, result._message);
        }

    }
}
