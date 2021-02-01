using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PePe.API.Model;
using PePe.Base;
using PePe.Manager;
using PePe.Service;

namespace PePe.API.Controllers {
    [Route("menu")]
    public class MenuController : Controller {
        private readonly IMenuManager menuManager;
        private readonly IMapper mapper;

        public MenuController(IMenuManager menuManager, IMapper mapper) {
            this.menuManager = menuManager;
            this.mapper = mapper;
        }

        /// <summary>
        /// Fetches today's menu from web. If the menu has not been published yet meals will be empty.
        /// </summary>
        /// <returns>Today's menu</returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(PePeMenu), 200)]
        public IActionResult GetTodaysMenu() {
            var todaysMenu = menuManager.GetTodaysMenu();
            var mapped = mapper.Map<Menu, PePeMenu>(todaysMenu);
            return Json(mapped);
        }

        /// <summary>
        /// Searches for menus which match based on <paramref name="menuSearch"/>.
        /// </summary>
        /// <param name="menuSearch">search object</param>
        /// <returns>List of matched menus</returns>
        [HttpPost("find")]
        [ProducesResponseType(typeof(IEnumerable<PePeMenu>), 200)]
        public IActionResult Find([FromBody] MenuSearch menuSearch) {
            if (menuSearch == null)
                return BadRequest("Ill-formed search object.");

            var foundMenus = mapper.Map<IEnumerable<Menu>, IEnumerable<PePeMenu>>(menuManager.Find(menuSearch));
            return Json(foundMenus);
        }
    }
}
