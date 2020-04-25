using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Negotiate.Controllers
{   
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class InfoController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public IEnumerable<Object> Get()
        {
            return User.Identities.Select(identity => new { identity.AuthenticationType, identity.Name });
        }
    }
}
