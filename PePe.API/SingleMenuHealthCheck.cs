using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PePe.Base;
using PePe.DAO;
using PePe.Service;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PePe.API
{
    public class SingleMenuHealthCheck : IHealthCheck
    {
        private readonly IMongoCollection<Menu> collection;
        private readonly IDateProvider dateProvider;

        public SingleMenuHealthCheck(string connectionString, string databaseName, string collectionName, IDateProvider dateProvider)
        {
            MongoClient client = new MongoClient(connectionString);

            IMongoDatabase database = client.GetDatabase(databaseName);

            collection = database.GetCollection<Menu>(collectionName);
            this.dateProvider = dateProvider;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var todaysDate = dateProvider.GetDate();

            var eqFilter = Builders<Menu>.Filter.Eq(m => m.Date, todaysDate.Date);
            var count = await collection.Find(eqFilter).CountDocumentsAsync(cancellationToken);

            if (count == 1)
            {
                return HealthCheckResult.Healthy("One menu was found for today");
            } else if (count == 0)
            {
                return HealthCheckResult.Degraded("No menu was found for today");
            } else
            {
                return HealthCheckResult.Unhealthy($"More than one menu was found today. Count: {count}");
            }
        }
    }
}
