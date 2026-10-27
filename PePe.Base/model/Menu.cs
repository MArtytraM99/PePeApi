using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace PePe.Base {
    public class Menu {
        public ObjectId Id { get; set; }
        public DateTime Date { get; set; }
        public IEnumerable<Meal> Meals { get; set; }

        /// <summary>
        /// Day of week as written in the source heading (null if unknown). Used only for reconciling a mistyped date, never persisted.
        /// </summary>
        [BsonIgnore]
        public DayOfWeek? HeadingDayOfWeek { get; set; }
    }
}
