using System;
using System.Collections.Generic;
using System.Text;

namespace PePe.Service {
    public class PragueDateProvider : IDateProvider {
        public TimeZoneInfo PragueTimeZoneInfo { get; }
        public PragueDateProvider() {
            try {
                PragueTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            } catch (TimeZoneNotFoundException) {
                PragueTimeZoneInfo = CreatePragueTimeZone();
            }
        }
        public DateTime GetDate() {
            var utcNow = DateTime.UtcNow;
            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, PragueTimeZoneInfo).Date;
        }

        private TimeZoneInfo CreatePragueTimeZone() {
            string displayName = "(UTC+01:00) Central European Standard Time";
            string standardName = "Central European Standard Time";
            string daylightName = "Central European Summer Time";
            TimeSpan offset = new TimeSpan(1, 0, 0);

            var startTransition = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 3, 5, DayOfWeek.Sunday);
            var endTransition = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 10, 5, DayOfWeek.Sunday);
            TimeSpan delta = new TimeSpan(1, 0, 0);
            TimeZoneInfo.AdjustmentRule adjustment;
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1996, 1, 1), DateTime.MaxValue.Date, delta, startTransition, endTransition);

            TimeZoneInfo.AdjustmentRule[] adjustments = { adjustment };

            return TimeZoneInfo.CreateCustomTimeZone(standardName, offset, displayName, standardName, daylightName, adjustments);
        }
    }
}
