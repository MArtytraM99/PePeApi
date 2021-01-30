using PePe.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PePe.API.Model {
    public class PePeMenu {
        public DateTime Date { get; set; }
        public IEnumerable<Meal> Meals { get; set; }
    }
}
