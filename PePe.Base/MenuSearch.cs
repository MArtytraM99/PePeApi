using System;
using System.Collections.Generic;
using System.Text;

namespace PePe.Base {
    public class MenuSearch {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public MealType? MealTypeFilter { get; set; }
    }
}
