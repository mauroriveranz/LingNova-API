using LingNova.Core.Interfaces;
using LingNova.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LingNova_API.Controllers
{
    [Route ("api/[Controller]")]
    [ApiController]
    public class UserController: ControllerBase
    {
        private readonly IUsersRepository _UserRepository;
        private readonly IConfiguration _configuration;

        public UserController(IUsersRepository usersRepository, IConfiguration configuration)
        {
            _UserRepository = usersRepository;
            _configuration = configuration;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginVM loginVM)
        {
            var result = await _UserRepository.Login(loginVM);
            if (result == null)
                return Unauthorized(new { message = "Credenciales incorrectas" });

            return Ok(result);
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterVM registerVM)
        {
            var result = await _UserRepository.Register(registerVM);
            if (result == null)
                return BadRequest(new { message = "El email ya está registrado" });

            return Ok(result);
        }


        [HttpPost("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserVM updateUser)
        {
            var result = await _UserRepository.Update(updateUser);
            if (result == null)
                return BadRequest(new { message = "Error al actualizar" });

            return Ok(result);
        }

        [HttpPost("mail")]
        public async Task<IActionResult> SendMail([FromBody] EmailVM emailVM)
        {
            var result = await _UserRepository.sendEmail(emailVM);
            if (result == false)
                return BadRequest(new { message = "Error al enviar" });

            return Ok(result);
        }

        [HttpPost("forgotPassword")]
        public async Task<IActionResult> forgotPassword([FromBody] ForgotPasswordVM forgotPasswordVM)
        {
            var result = await _UserRepository.ForgotPassword(forgotPasswordVM);
            if (result == false)
                return BadRequest(new { message = "Error al enviar" });

            return Ok(result);
        }
    }
}
