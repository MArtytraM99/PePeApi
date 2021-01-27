using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pepe.Base;
using Pepe.Service;

namespace PePe.API.Controllers {
    [Route("menu")]
    public class MenuController : Controller {
        private readonly IWebScraper webScraper;

        public MenuController(IWebScraper webScraper) {
            this.webScraper = webScraper;
        }
        [HttpGet("")]
        [ProducesResponseType(typeof(Menu), 200)]
        public IActionResult GetMenu() {
            /*var testMenu = new Menu { Date = DateTime.Now.Date, Meals=new List<Meal> {
                new Meal{MealType = MealType.Soup, Name = "Fazolová s klobásou", Info = "", Price = 30},
                new Meal{MealType = MealType.Main, Name = "Halušky s brynzou polévané slaninou", Info = "", Price = 98},
                new Meal{MealType = MealType.Main, Name = "Vepřový gulášek s feferonkou, houskové knedlíky", Info = "", Price = 98},
            } };*/

            return Json(webScraper.ScrapeForMenu());
        }
    }
}
