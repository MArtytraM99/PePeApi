using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using PePe.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PePe.DAO {
    public class MongoMenuDao : IMenuDao {

        private readonly MongoClient client;
        private readonly IMongoDatabase database;
        private readonly IMongoCollection<Menu> collection;
        private readonly ILogger<MongoMenuDao> logger;

        public FilterDefinitionBuilder<Menu> Filter { get; set; } = Builders<Menu>.Filter;
        public UpdateDefinitionBuilder<Menu> Update { get; set; } = Builders<Menu>.Update;
        public MongoMenuDao(string connectionString, string databaseName, string collectionName, ILogger<MongoMenuDao> logger) {
            client = new MongoClient(connectionString);

            database = client.GetDatabase(databaseName);

            collection = database.GetCollection<Menu>(collectionName);
            this.logger = logger;
        }

        public Menu GetMenuByDate(DateTime dateTime) {
            var eqFilter = Filter.Eq(m => m.Date, dateTime.Date);
            return collection.Find(eqFilter).SingleOrDefault();
        }

        public IEnumerable<Menu> GetAll() {
            return collection.Find(Builders<Menu>.Filter.Empty).ToEnumerable();
        }

        public Menu Save(Menu menu) {
            if (menu == null || menu.Meals == null || menu.Meals.Count() == 0) {
                logger.LogInformation("Menu is null or empty => not saving and returning null");
                return null;
            }

            var eqFilter = Filter.Eq(m => m.Date, menu.Date);
            var update = Update.Set(m => m.Date, menu.Date).Set(m => m.Meals, menu.Meals);
            var options = new UpdateOptions { IsUpsert = true };

            var result = collection.UpdateOne(eqFilter, update, options);
            logger.LogInformation($"Mongo menu update result -- matched: {result.MatchedCount}, upsertedId: {result.UpsertedId}, modified: {result.ModifiedCount}");
            
            return collection.Find(eqFilter).SingleOrDefault();
        }

        public IEnumerable<Menu> Find(MenuSearch menuSearch) {
            var fromDateFilter = Filter.Empty;
            if(menuSearch.FromDate.HasValue)
                fromDateFilter = Filter.Gte(m => m.Date, menuSearch.FromDate?.Date);

            var toDateFilter = Filter.Empty;
            if(menuSearch.ToDate.HasValue)
                toDateFilter = Filter.Lte(m => m.Date, menuSearch.ToDate?.Date);

            var filter = Filter.And(fromDateFilter, toDateFilter);
            var foundMenus = collection.Find(filter);

            if (!menuSearch.MealTypeFilter.HasValue) {
                return foundMenus.ToEnumerable();
            }

            return foundMenus.Project(m => new Menu {
                Id = m.Id,
                Date = m.Date,
                Meals = m.Meals.Where(meal => meal.MealType == menuSearch.MealTypeFilter.Value)
            }).ToEnumerable();
        }
    }
}
