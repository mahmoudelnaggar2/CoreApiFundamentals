﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [ApiController]
    public class OperationController : ControllerBase
    {
        private readonly IConfiguration _config;

        public OperationController(IConfiguration config)
        {
            this._config = config;
        }

        [HttpOptions("reloadconfiguration")]
        public IActionResult Reload()
        {
            try
            {
                var root = (IConfigurationRoot)_config;
                
                root.Reload();

                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}