using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WeChat.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private ILog _logger;
        public LogController()
        {
            this._logger = LogManager.GetLogger(Startup.respository.Name, typeof(LogController));
        }
        //public string Get(string data)
        //{
        //    _logger.Info("code=" + data);
        //    return "ok";
        //}
    }
}