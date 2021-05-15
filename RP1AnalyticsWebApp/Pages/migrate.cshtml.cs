using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using RP1AnalyticsWebApp.Models;
using RP1AnalyticsWebApp.Services;

namespace RP1AnalyticsWebApp.Pages
{
    public class migrateModel : PageModel
    {
        private readonly ICareerLogDatabaseSettings _settings;

        public migrateModel(ICareerLogDatabaseSettings settings)
        {
            _settings = settings;
        }

        public void OnGet()
        {
            var client = new MongoClient(_settings.ConnectionString);

            var database = client.GetDatabase("prod");
            var newDatabase = client.GetDatabase("test");

            var careerLogs = database.GetCollection<CareerLog>(_settings.CareerLogsCollectionName);
            var newCareerLogs = newDatabase.GetCollection<CareerLog>("CareerLogs");

            var userColl = database.GetCollection<BsonDocument>("Users");
            var newUserColl = newDatabase.GetCollection<BsonDocument>("Users");

            var careerCount1 = careerLogs.CountDocuments(FilterDefinition<CareerLog>.Empty);
            //newCareerLogs.DeleteMany(FilterDefinition<CareerLog>.Empty);
            var careerCount2 = newCareerLogs.CountDocuments(FilterDefinition<CareerLog>.Empty);
            if (careerCount2 == 0)
            {
                var cItems = careerLogs.Find(FilterDefinition<CareerLog>.Empty).ToList();
                foreach (var item in cItems)
                {
                }
                newCareerLogs.InsertMany(cItems);
            }

            var userCount1 = userColl.CountDocuments(FilterDefinition<BsonDocument>.Empty);
            //newUserColl.DeleteMany(FilterDefinition<BsonDocument>.Empty);
            var userCount2 = newUserColl.CountDocuments(FilterDefinition<BsonDocument>.Empty);
            if (userCount2 == 0)
            {
                var uItems = userColl.Find(FilterDefinition<BsonDocument>.Empty).ToList();
                foreach (var item in uItems)
                {
                }
                newUserColl.InsertMany(uItems);
            }
        }
    }
}
