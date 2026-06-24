using Business_Layer.DTOs.UserDTO;
using Business_Layer.Services;
using FirebaseAdmin.Auth;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Google.Rpc.Context.AttributeContext.Types;
using System.IdentityModel.Tokens.Jwt;

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
            try
            {
                var token = await _auth.RegisterAsync(dto);
                return Ok(new { token });

            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });

            }
            catch (FirebaseAuthException ex)
            {
     
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
              
                return StatusCode(500, new { error = "An unexpected error occurred during registration." });
            }
        }
            

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            try
            {
                var token = await _auth.LoginAsync(dto);
                return Ok(new { token });
            } 
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });

            }
            catch (FirebaseAuthException ex)
            {
     
                return BadRequest(new { error = ex.Message });
            }
          
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { error = "An unexpected error occurred during login." });
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

        [Authorize(Policy = "CustomAuth")]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
    UpdateProfileDTO dto)
        {
            var email =
                User.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return Unauthorized();

            await _auth.UpdateProfileAsync(
                email,
                dto);

            return Ok(new
            {
                message = "Profile updated successfully"
            });
        }

        [Authorize(Policy = "CustomAuth")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO dto)
        {
            // In AuthController, change-password endpoint:
            var email = User.FindFirst(ClaimTypes.Email)?.Value
         ?? User.FindFirst("email")?.Value;
            if (email == null)
                return Unauthorized();

            try
            {
                await _auth.ChangePasswordAsync(email, dto);
                return Ok(new { message = "Password changed successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
                    message = "Password reset link has been sent to your email"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "CustomAuth")]
        [HttpGet("notification-settings")]
        public async Task<IActionResult> GetNotificationSettings()
        {
            Console.WriteLine("GET notification-settings reached");
            var userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var settings =
                await _auth.GetNotificationSettingsAsync(userId);

            return Ok(settings);
        }

        [Authorize(Policy = "CustomAuth")]
        [HttpPut("notification-settings")]
        public async Task<IActionResult> UpdateNotificationSettings(
    UpdateNotificationSettingsRequestDto dto)
        {
            var userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success =
                await _auth.UpdateNotificationSettingsAsync(
                    userId,
                    dto);

            if (!success)
                return BadRequest(new
                {
                    message = "Failed to update notification settings"
                });

            return Ok(new
            {
                message = "Notification settings updated successfully"
            });
        }
        [HttpGet("auth-test")]
        [Authorize]
        public IActionResult Test()
        {
            return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
        }

        [HttpGet("token-debug")]
        public IActionResult TokenDebug()
        {
            var header = Request.Headers["Authorization"].ToString();

            return Ok(new
            {
                authHeader = header,
                userAuthenticated = User.Identity?.IsAuthenticated,
                claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }

        [HttpGet("debug-me")]
        public IActionResult DebugMe()
        {
            var auth = Request.Headers["Authorization"].ToString();

            return Ok(new
            {
                header = auth,
                identity = User.Identity?.IsAuthenticated,
                claims = User.Claims.ToList()
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("admin-panel")]
        public IActionResult AdminPanel()
        {
            return Ok("Welcome Admin");
        }
    }
}

