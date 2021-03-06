﻿using MongoDB.Bson;
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
            return collection.Find(Builders<Menu>.Filter.Empty).ToEnumerable();
        }

        public Menu Save(Menu menu) {
            var eqFilter = Filter.Eq(m => m.Date, menu.Date);

            using (var session = client.StartSession()) {
                
                var savedInstance = collection.Find(eqFilter).SingleOrDefault();

                if (savedInstance == null) {
                    collection.InsertOne(menu);
                }

                // if new meals are added or just changed
                if(savedInstance.Meals.Count() < menu.Meals.Count()) {
                    collection.DeleteOne(eqFilter); // delete old one

                    collection.InsertOne(menu); // save new one
                }

                session.CommitTransaction();
            }
            
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
