using System;
using System.Collections.Generic;
using System.Text;

namespace PePe.Service {
    public class MonthConvertor : IMonthConvertor {
        private Dictionary<string, int> monthMap;

        public MonthConvertor() {
            monthMap = new Dictionary<string, int> {
                { "ledna", 1 }, { "leden", 1 },
                { "února", 2 }, { "únor", 2 },
                { "března", 3 }, { "březen", 3 },
                { "dubna", 4 }, { "duben", 4 },
                { "května", 5 }, { "květen", 5 },
                { "června", 6 }, { "červen", 6 },
                { "července", 7 }, { "červenec", 7 },
                { "srpna", 8 }, { "srpen", 8 },
                { "září", 9 },
                { "října", 10 }, { "říjen", 10 },
                { "listopadu", 11 }, { "listopad", 12 },
                { "prosince", 12 }, { "prosinec", 12 }
            };
        }
        public int convertMonth(string monthName) {
            return monthMap[monthName.ToLower()];
        }
    }
}
