using Microsoft.AspNetCore.Mvc;
using TDEV_811.Services;
using TDEV_811.DTO;

namespace TDEV_811.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUserAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(UserDTO userDTO)
        {
            await _userService.AddUserAsync(userDTO);
            return CreatedAtAction(nameof(GetById), new { id = userDTO.Id }, userDTO);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UserDTO userDTO)
        {
            try
            {
                await _userService.UpdateUserAsync(id, userDTO);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
