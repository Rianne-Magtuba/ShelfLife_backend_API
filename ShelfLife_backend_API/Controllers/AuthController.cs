using Business_Layer.DTOs.UserDTO;
using Business_Layer.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Google.Rpc.Context.AttributeContext.Types;

namespace ShelfLife_backend_API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            var token = await _auth.RegisterAsync(dto);
            return Ok(new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            try
            {
                var token = await _auth.LoginAsync(dto);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "CustomAuth")]
        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return Unauthorized();

            var user = await _auth.GetProfileByEmailAsync(email);

            return Ok(user);
        }



        [HttpGet("firebase-profile")]
        public async Task<IActionResult> FirebaseProfile([FromServices] FirebaseAuthService firebaseAuth)
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader))
                return Unauthorized(new { message = "Missing Authorization header" });

            var token = authHeader.Replace("Bearer ", "");

            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await firebaseAuth.VerifyToken(token);
            }
            catch
            {
                return Unauthorized(new { message = "Invalid Firebase token" });
            }

            return Ok(new
            {
                email = payload.Email,
                username = payload.Name,
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO dto)
        {
            try
            {
                await _auth.SendPasswordResetAsync(dto.Email);

                return Ok(new
                {
                    message = "Password reset link has been generated"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

