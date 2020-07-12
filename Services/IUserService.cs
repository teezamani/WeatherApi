using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.DTO;
using WeatherApp.Model;

namespace WeatherApp.Services
{
    //Interface for User Authentication - Blueprint for the Class to follow
    public interface IUserService
    {

        Task<UserManagerResponse> RegisterUserAsync(UserRegister model);
        Task<UserManagerResponse> LoginUserAsync(UserLoginDto model);
    }

    //Class for User Authentication
    public class UserService : IUserService
    {
        //Injecting all required Services
        private UserManager<User> _userManager;
        private readonly ApplicationSettings _appSettings;

        public UserService(UserManager<User> userManager, IOptions<ApplicationSettings> appSettings)
        {
            _userManager = userManager;
            _appSettings = appSettings.Value;
        }

        //The register controller calls thos method beforeexecutin the register endpoint
        public async Task<UserManagerResponse> RegisterUserAsync(UserRegister model)
        {
            try
            {
                // Check if body of request is not empty 
                if (model == null)
                    return new UserManagerResponse
                    {
                        Message = "Null or Empty",
                    };

                //Check if the email is not used 
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                    return new UserManagerResponse
                    {
                        Message = "User With this email address already exists",
                    };

                //Map the Dto To the Domain Model
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    CreationDate = DateTime.Now
                };

                //Hash the password in the database
                var results = await _userManager.CreateAsync(user, model.Password);

                if (results.Succeeded)
                {
                    return new UserManagerResponse
                    {
                        Message = "User Created successfully",
                        IsSuccess = true,
                    };
                }
                return new UserManagerResponse
                {

                    Message = "User did not create",
                    IsSuccess = false,
                };
            }
            catch (Exception)
            {

                return new UserManagerResponse
                {
                    Message = "User did not create",
                    IsSuccess = false,
                };
            }
        }

        //The login controller calls this method before executing the endpoint
        public async Task<UserManagerResponse> LoginUserAsync(UserLoginDto model)
        {
            try
            {
                //Get the user by email entered
                var user = await _userManager.FindByEmailAsync(model.Email);

                //If no user with the email
                if (user == null)
                    return new UserManagerResponse
                    {
                        Message = "We couldn’t find a user with that email address ",
                        IsSuccess = false
                    };
                //Get the User password
                var result = await _userManager.CheckPasswordAsync(user, model.Password);

                //If password does not match
                if (!result)
                    return new UserManagerResponse
                    {
                        Message = "Invalid Password",
                        IsSuccess = false,
                    };

                //Method to generate JSON WEB TOKEN
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                         {

                        new Claim("UserID" , user.Id),
                        new Claim("Email", user.Email),
                             // new Claim("Role", user.RoleName)
                         }),
                    Expires = DateTime.UtcNow.AddHours(4),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JWT_Secret)), SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);

                //If Authentication was successful token is returned
                return new UserManagerResponse
                {
                    Message = token,
                    IsSuccess = true,
                    ExpireDate = tokenDescriptor.Expires
                };
            }
            //If error
            catch (Exception)
            {
                return new UserManagerResponse
                {
                    Message = "Error while authenticating user",
                    IsSuccess = false
                };
            }
        }

    }
}
