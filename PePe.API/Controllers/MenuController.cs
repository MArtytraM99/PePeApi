using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PePe.Base;
using PePe.Service;

namespace PePe.API.Controllers {
    [Route("menu")]
    public class MenuController : Controller {
        private readonly IWebScraper webScraper;

        public MenuController(IWebScraper webScraper) {
            this.webScraper = webScraper;
        }
        /// <summary>
        /// Fetches today's menu from web. If the menu has not been published yet meals will be empty.
        /// </summary>
        /// <returns>Today's menu</returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(Menu), 200)]
        public IActionResult GetMenu() {
            return Json(webScraper.ScrapeForMenu());
        }
    }
}
