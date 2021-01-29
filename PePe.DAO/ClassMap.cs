using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using PePe.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace PePe.DAO {
    public class ClassMap {
        private static bool isRegistered = false;
        private static readonly object _lock = new object();
        public void RegisterMaps() {
            lock (_lock) {
                if (isRegistered)
                    return;
                isRegistered = true;
            }

            RegisterMealMap();
            RegisterMenuMap();
        }

        private void RegisterMealMap() {
            BsonClassMap.RegisterClassMap<Meal>(cm => {
                cm.MapMember(m => m.Name).SetElementName("name");
                cm.MapMember(m => m.Info).SetElementName("info");
                cm.MapMember(m => m.Price).SetElementName("price");
                cm.MapMember(m => m.MealType).SetElementName("mealType").SetSerializer(new EnumSerializer<MealType>(BsonType.String));
            });
        }

        private void RegisterMenuMap() {
            BsonClassMap.RegisterClassMap<Menu>(cm => {
                cm.MapMember(m => m.Date).SetElementName("date").SetSerializer(DateTimeSerializer.DateOnlyInstance);
                cm.MapMember(m => m.Meals).SetElementName("meals");
            });
        }
    }
}
