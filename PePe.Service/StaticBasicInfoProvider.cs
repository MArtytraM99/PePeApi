using PePe.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace PePe.Service {
    public class StaticBasicInfoProvider : IBasicInfoProvider {
        public BasicInfo Get() {
            return new BasicInfo {
                Address = new Address {
                    FirstLine = "Jarníkova 1903/35",
                    SecondLine = "",
                    PostalCode = "140 00",
                    City = "Praha 4 Chodov"
                },
                OpeningHours = "8:00 - 14:00",
                OpeningDays = "Pondělí - Pátek"
            };
        }
    }
}
