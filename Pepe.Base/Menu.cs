using System;
using System.Collections.Generic;
using System.Text;

namespace PePe.Base {
    public class Menu {
        public DateTime Date { get; set; }
        public IEnumerable<Meal> Meals { get; set; }
    }
}
