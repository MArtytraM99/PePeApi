using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using PePe.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace PePe.DAO {
    public class MongoMenuDao : IMenuDao {

        private readonly MongoClient client;
        private readonly IMongoDatabase database;
        private readonly IMongoCollection<Menu> collection;

        public FilterDefinitionBuilder<Menu> Filter { get; set; } = Builders<Menu>.Filter;
        public MongoMenuDao(string connectionString, string databaseName, string collectionName) {
            client = new MongoClient(connectionString);

            database = client.GetDatabase(databaseName);

            collection = database.GetCollection<Menu>(collectionName);
        }

        public Menu GetMenuByDate(DateTime dateTime) {
            var eqFilter = Filter.Eq(m => m.Date, dateTime.Date);
            return collection.Find(eqFilter).SingleOrDefault();
        }

        public IEnumerable<Menu> GetAll() {
            return collection.Find(Builders<Menu>.Filter.Empty).ToList();
        }

        public Menu Save(Menu menu) {
            collection.InsertOne(menu);
            return menu;
        }
    }
}
