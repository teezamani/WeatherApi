using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;
using WeatherApp.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using WeatherApp.Model;

namespace WeatherApp.Controllers
{
    [EnableCors("CorsPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationSettings _appSettings;
        public WeatherController(IConfiguration configuration, IOptions<ApplicationSettings> appSettings)
        {
            _configuration = configuration;
            _appSettings = appSettings.Value;

        }

        [EnableCors("CorsPolicy")]
        [HttpGet("{city}")]
      //  [Route("{city}")]
        [Authorize]
        public async Task<IActionResult> WeatherforCity(string city)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    //Get the key from appsettings
                    var weatherKey = _appSettings.OpenWeatherApiKey;
                 //  var weatherKey = "d96bdd95f1dffd44ca50f67d1beedd23";

                    // Make web requests from our.NET APP Code using HttpClient. To do so it needs to know which url to make the request to.
                    //we point the BaseAddress property to the Open Weather

                    client.BaseAddress = new Uri("http://api.openweathermap.org");

                    //the response should return metric units (e.g. celsius for temperatures) and EnsureSuccessStatusCode
                    var response = await client.GetAsync($"/data/2.5/weather?q={city}&appid={weatherKey}");
                    response.EnsureSuccessStatusCode();

                    //Get the response from OpenWeather and deserialize it into a C# object
                    var stringResult = await response.Content.ReadAsStringAsync();
                    var rawWeather = JsonConvert.DeserializeObject<OpenWeatherResponse>(stringResult);

                    //  Return a trimmed version of the weather data
                    return Ok(new
                    {
                        Temp = (rawWeather.Main.Temp + "*C").ToString(),
                        Weather = string.Join(",", rawWeather.Weather.Select(x => x.Main)),
                        Weather_Summary = string.Join(",", rawWeather.Weather.Select(x => x.Description)),
                        City = rawWeather.Name
                    });
                }
                //Handle any errors raised making the call to OpenWeather
                catch (HttpRequestException httpRequestException)
                {
                    return BadRequest($"Error getting weather from OpenWeather: {httpRequestException.Message}");
                }
            }
        }
    }
}