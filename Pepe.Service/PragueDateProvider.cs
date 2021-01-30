using System;
using System.Collections.Generic;
using System.Text;

namespace PePe.Service {
    public class PragueDateProvider : IDateProvider {
        public DateTime GetDate() {
            var utcNow = DateTime.UtcNow;
            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")).Date;
        }
    }
}
