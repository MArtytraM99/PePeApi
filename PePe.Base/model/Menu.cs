using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace PePe.Base {
    public class Menu {
        public ObjectId Id { get; set; }
        public DateTime Date { get; set; }
        public IEnumerable<Meal> Meals { get; set; }
    }
}
