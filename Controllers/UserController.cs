using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WeatherApp.DTO;
using WeatherApp.Model;
using WeatherApp.Services;

namespace WeatherApp.Controllers
{
    [EnableCors("CorsPolicy")] // Allow cross Platform resource haring
    [Route("api/[controller]")] // endpoint routing
    [ApiController]

    //Inject the needed services to the controller
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;
        private readonly SignInManager<User> _signInManager;
        public UserController(UserManager<User> userManager, SignInManager<User> signInManager, IUserService userService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userService = userService;
        }


        //Post:/api/User/Register 
        //User Regiter Endpoint
        [EnableCors("CorsPolicy")]
        [HttpPost]
        [Route("Register")]
        [AllowAnonymous] //Specifies that the Endpoint does not need Authorization
        public async Task<IActionResult> RegisterAsync([FromBody]UserRegister model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Call the userservice method 
                    var result = await _userService.RegisterUserAsync(model);
                    if (result.IsSuccess)
                        return Ok(result);

                    return BadRequest(result);
                }
                return BadRequest("Some Properties are not valid");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {Message = "Error while Registering User" , isSuccess = false , Errors = ex.Message });
            }
        }

        //Post:api/User/Login
        [EnableCors("CorsPolicy")]
        [HttpPost]
        [Route("Login")]
        [AllowAnonymous] //Specifies that the Endpoint does not need Authorization
        public async Task<IActionResult> LoginAsync(UserLoginDto model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //Call the LoginUser method
                    var result = await _userService.LoginUserAsync(model);

                    if (result.IsSuccess)
                    {
                        return Ok(result);
                    }

                    return BadRequest(result);
                }
                return BadRequest("Some Properties are not valid");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ex.Message, isSuccess = false });
            }

        }
    }
}