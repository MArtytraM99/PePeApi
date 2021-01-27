using System;
using System.Collections.Generic;
using System.Text;

namespace Pepe.Base {
    public class Menu {
        public DateTime Date { get; set; }
        public IEnumerable<Meal> Meals { get; set; }
    }
}
