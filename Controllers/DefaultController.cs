using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HSBors.Models;
using Microsoft.Extensions.Caching.Distributed;
using HSBors.Middleware;

namespace HSBors.Controllers
{
    
    public class DefaultController : ControllerBase
    {
        protected readonly IDistributedCache _distributedCache;
        protected readonly ILogger Logger;
        protected readonly HSBorsDb DbContext;

        public DefaultController(IDistributedCache distributedCache, ILogger<DefaultController> logger, HSBorsDb dbContext)
        {
            _distributedCache = distributedCache;
            Logger = logger;
            DbContext = dbContext;
        }
       
     
    }
     
}