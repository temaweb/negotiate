using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Negotiate.Controllers
{   
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class InfoController : ControllerBase
    {
        public class Auth
        {
            public String Name
            {
                get; 
                set;
            }

            public String AuthType
            {
                get;
                set;
            }
        }

        private readonly ILogger<InfoController> _logger;

        public InfoController(ILogger<InfoController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public IEnumerable<Auth> Get()
        {
            return User.Identities.Select(i => new Auth{ AuthType = i.AuthenticationType, Name = i.Name });
        }
    }
}
