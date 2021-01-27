using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PePe.Base;
using PePe.Service;

namespace PePe.API.Controllers {
    public class HomeController : Controller {
        private readonly IBasicInfoProvider basicInfoProvider;

        public HomeController(IBasicInfoProvider basicInfoProvider) {
            this.basicInfoProvider = basicInfoProvider;
        }

        /// <summary>
        /// Returns basic information about the canteen such as address and opening hours.
        /// </summary>
        /// <returns></returns>
        [HttpGet("basic_info")]
        [ProducesResponseType(typeof(BasicInfo), 200)]
        public IActionResult GetBasicInfo() {
            var info = basicInfoProvider.Get();
            return Json(info);
        }
    }
}
