using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserAuthManager.API.Models;
using UserAuthManager.API.Repositories;
using UserAuthManager.API.Services;

namespace UserAuthManager.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TokenService _tokenService;

        public UserController(
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager,
            TokenService tokenService)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var username = User.Identity?.Name;
            return Ok(new { Message = $"Você está autenticado como {username}" });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationUser>> GetUserById(string id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            return user is null ? NotFound() : Ok(user);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetUsers()
        {
            var users = await _userRepository.GetUsersAsync();
            return Ok(users);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userRepository.GetUserByEmailAsync(model.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized(new { Message = "Credenciais inválidas." });
            }

            var token = _tokenService.GenerateToken(user);
            return Ok(new { Token = token });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] ApplicationUser user)
        {
            if (id != user.Id)
                return BadRequest("IDs não coincidem.");

            var updated = await _userRepository.UpdateUserAsync(user);
            return updated == null ? NotFound() : NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var deleted = await _userRepository.DeleteUserAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest("O e-mail já está sendo utilizado.");

            var user = new ApplicationUser
            {
                Name = model.Name,
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }

            return Ok(new { Message = "Usuário registrado com sucesso!" });
        }
    }
}
