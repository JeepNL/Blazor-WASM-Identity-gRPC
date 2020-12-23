using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace BlazorTemplate.Server.Controllers
{
    //[ApiController]
    //[Route("api/AlexaSkill")]
    public class AlexaSkillController : ControllerBase
    {
        //private Microsoft.Extensions.Configuration.IConfiguration _config;
        //private string _amazonLink;



        //public AlexaSkillController(Microsoft.Extensions.Configuration.IConfiguration config)
        //{
        //    _config = config;
        //    var alexaVendor = _config["Alexa:BlazorNews:VendorId"];
        //    _amazonLink = "https://layla.amazon.com/spa/skill/account-linking-status.html?vendorId=" + alexaVendor;
        //}

        [HttpPost("api/AlexaSkill/Request")]
        public IActionResult HandleResponse([FromBody] SkillRequest input)

        {

            var requestType = input.GetRequestType();
            SkillResponse response = null;

            var name = "";
            var jwtEncodedString = input.Session.User.AccessToken;
            if (jwtEncodedString is null)
            {
                response = ResponseBuilder.TellWithLinkAccountCard("You are not currently linked to this skill. Please go into your Alexa app and sign in.");
                response.Response.ShouldEndSession = true;

                return new OkObjectResult(response);
            }


            var token = new JwtSecurityToken(jwtEncodedString: jwtEncodedString);
            var claims = token.Claims;
            name = claims.First(c => c.Type == "name").Value;


            if (requestType == typeof(LaunchRequest))
            {
                response = ResponseBuilder.Tell($"Welcome to Blazor News {name}!");
                response.Response.ShouldEndSession = false;
            }

            // return information from an intent
            else if (requestType == typeof(IntentRequest))
            {
                // do some intent-based stuff
                var intentRequest = input.Request as IntentRequest;
                if (intentRequest.Intent.Name.Equals("news"))
                {
                    // get the pull requests
                    var news = GetNews();

                    if (news == 0)
                        response = ResponseBuilder.Tell("We have no blazor news at this time.");
                    else
                        response = ResponseBuilder.Tell("There are " + news.ToString() + " blazor news articles.");

                    response.Response.ShouldEndSession = false;
                }
                else
                {
                    response = ResponseBuilder.Ask("I don't understand. Can you please try again?", null);
                    response.Response.ShouldEndSession = false;

                }
            }
            else if (requestType == typeof(SessionEndedRequest))
            {
                response = ResponseBuilder.Tell("See you next time!");
                response.Response.ShouldEndSession = true;
            }

            return new OkObjectResult(response);
        }

        [HttpGet("RequestSimple/{clientId}")]
        public IActionResult GetClientRequestParameters([FromRoute] string clientId)
        {
            return new OkObjectResult(clientId);
        }

        private static int GetNews()
        {
            return 3;
        }
    }
}
