using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using RP1AnalyticsWebApp.Models;

namespace RP1AnalyticsWebApp.Pages
{
    public class migrateModel : PageModel
    {
        private readonly ICareerLogDatabaseSettings _settings;
        private readonly MongoClient _client;
        private readonly MongoClient _migrateTgtClient;

        public List<CareerLog> EmptyCareers { get; set; }

        public migrateModel(ICareerLogDatabaseSettings settings)
        {
            _settings = settings;
            _client = new MongoClient(_settings.ConnectionString);
            _migrateTgtClient = new MongoClient(_settings.ConnectionString2);
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostFindEmptyCareersAsync()
        {
            EmptyCareers = FindEmptyCareers(_settings.DatabaseName);
            return Page();
        }

        public async Task<IActionResult> OnPostPruneEmptyCareersAsync()
        {
            PruneEmptyCareers(_settings.DatabaseName);
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateSchemaAsync()
        {
            UpdateSchemaInDB(_settings.DatabaseName);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCopyDBAsync()
        {
            CopyDB(_settings.DatabaseName, _settings.DatabaseName2);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostFillVersionsAsync()
        {
            FillVersions(_settings.DatabaseName);
            return RedirectToPage();
        }

        private List<CareerLog> FindEmptyCareers(string dbName)
        {
            var database = _client.GetDatabase(dbName);
            var careerLogs = database.GetCollection<CareerLog>(_settings.CareerLogsCollectionName);

            var cItems = careerLogs.AsQueryable()
                .Where(c => c.CareerLogEntries == null && (c.CareerLogMeta == null || c.CareerLogMeta.CreationDate < DateTime.UtcNow.AddDays(-90)))
                .ToList();
            return cItems;
        }

        private void PruneEmptyCareers(string dbName)
        {
            var database = _client.GetDatabase(dbName);
            var careerLogs = database.GetCollection<CareerLog>(_settings.CareerLogsCollectionName);

            List<CareerLog> careers = FindEmptyCareers(dbName);
            foreach (CareerLog c in careers)
            {
                Console.WriteLine($"Deleting career: {c.Id} | {c.UserLogin} | {c.Name}");
                careerLogs.DeleteOne(c2 => c2.Id == c.Id);
            }
        }

        private void UpdateSchemaInDB(string dbName)
        {
            var database = _client.GetDatabase(dbName);
            var careerLogs = database.GetCollection<CareerLog>(_settings.CareerLogsCollectionName);

            var cItems = careerLogs.Find(FilterDefinition<CareerLog>.Empty).ToList();
            foreach (var item in cItems)
            {
                var updateDefs = new List<UpdateDefinition<CareerLog>>();
                if (item.CareerLogMeta == null)
                {
                    updateDefs.Add(Builders<CareerLog>.Update.Set(nameof(CareerLog.CareerLogMeta), (CareerLogMeta)null));
                }
                if (item.LaunchEventEntries == null)
                {
                    updateDefs.Add(Builders<CareerLog>.Update.Set(nameof(CareerLog.LaunchEventEntries), (LaunchEvent)null));
                }
                if (item.TechEventEntries == null)
                {
                    updateDefs.Add(Builders<CareerLog>.Update.Set(nameof(CareerLog.TechEventEntries), (TechResearchEvent)null));
                }

                if (updateDefs.Count > 0)
                {
                    var updateDef = Builders<CareerLog>.Update.Combine(updateDefs);
                    var opts = new FindOneAndUpdateOptions<CareerLog> { ReturnDocument = ReturnDocument.After };
                    careerLogs.FindOneAndUpdate<CareerLog>(entry => entry.Id == item.Id, updateDef, opts);
                }
            }
        }

        private void FillVersions(string dbName)
        {
            var database = _client.GetDatabase(dbName);
            var careerLogs = database.GetCollection<CareerLog>(_settings.CareerLogsCollectionName);

            var cItems = careerLogs.Find(FilterDefinition<CareerLog>.Empty).ToList();
            foreach (var item in cItems)
            {
                if (item.CareerLogMeta == null || string.IsNullOrWhiteSpace(item.CareerLogMeta.VersionTag)) continue;

                var meta = item.CareerLogMeta;
                string v = meta.VersionTag;
                var version = new Version(v);
                int sortVer = version.Major * 1000000 + version.Minor * 1000 + version.Build;

                meta.VersionSort = sortVer;

                var updateDef = Builders<CareerLog>.Update
                    .Set(nameof(CareerLog.CareerLogMeta), meta);
                var opts = new FindOneAndUpdateOptions<CareerLog> { ReturnDocument = ReturnDocument.After };
                careerLogs.FindOneAndUpdate<CareerLog>(entry => entry.Id == item.Id, updateDef, opts);
            }
        }

        public void CopyDB(string srcDBName, string destDBName)
        {
            var database = _client.GetDatabase(srcDBName);
            var newDatabase = _migrateTgtClient.GetDatabase(destDBName);

            var careerLogs = database.GetCollection<CareerLog>(_settings.CareerLogsCollectionName);
            var newCareerLogs = newDatabase.GetCollection<CareerLog>("CareerLogs");

            var userColl = database.GetCollection<BsonDocument>("Users");
            var newUserColl = newDatabase.GetCollection<BsonDocument>("Users");

            var roleColl = database.GetCollection<BsonDocument>("Roles");
            var newRoleColl = newDatabase.GetCollection<BsonDocument>("Roles");

            var careerCount1 = careerLogs.CountDocuments(FilterDefinition<CareerLog>.Empty);
            var careerCount2 = newCareerLogs.CountDocuments(FilterDefinition<CareerLog>.Empty);
            if (careerCount2 > 0)
                newCareerLogs.DeleteMany(FilterDefinition<CareerLog>.Empty);
            careerCount2 = newCareerLogs.CountDocuments(FilterDefinition<CareerLog>.Empty);
            Console.WriteLine($"CareerLogs: src {careerCount1}; dest {careerCount2}");
            if (careerCount2 == 0)
            {
                Console.WriteLine($"Copying CareerLogs...");
                var cItems = careerLogs.Find(FilterDefinition<CareerLog>.Empty).ToList();

                int i = 0;
                foreach (CareerLog[] batch in cItems.Chunk(25))
                {
                    Console.WriteLine($"Copying batch {++i}");
                    newCareerLogs.InsertMany(batch);
                }
                Console.WriteLine($"CareerLogs copied");
            }

            var userCount1 = userColl.CountDocuments(FilterDefinition<BsonDocument>.Empty);
            var userCount2 = newUserColl.CountDocuments(FilterDefinition<BsonDocument>.Empty);
            if (userCount2 > 0)
                newUserColl.DeleteMany(FilterDefinition<BsonDocument>.Empty);
            userCount2 = newUserColl.CountDocuments(FilterDefinition<BsonDocument>.Empty);
            Console.WriteLine($"Users: src {userCount1}; dest {userCount2}");
            if (userCount2 == 0)
            {
                Console.WriteLine($"Copying users...");
                var uItems = userColl.Find(FilterDefinition<BsonDocument>.Empty).ToList();
                foreach (var item in uItems)
                {
                }
                newUserColl.InsertMany(uItems);
                Console.WriteLine($"Users copied");
            }

            var roleCount1 = roleColl.CountDocuments(FilterDefinition<BsonDocument>.Empty);
            var roleCount2 = newRoleColl.CountDocuments(FilterDefinition<BsonDocument>.Empty);
            //if (roleCount2 > 0)
            //    newRoleColl.DeleteMany(FilterDefinition<BsonDocument>.Empty);
            //roleCount2 = newRoleColl.CountDocuments(FilterDefinition<BsonDocument>.Empty);
            Console.WriteLine($"Roles: src {roleCount1}; dest {roleCount2}");
            if (roleCount2 == 0)
            {
                Console.WriteLine($"Copying roles...");
                var rItems = roleColl.Find(FilterDefinition<BsonDocument>.Empty).ToList();
                foreach (var item in rItems)
                {
                }
                newRoleColl.InsertMany(rItems);
                Console.WriteLine($"Roles copied");
            }
        }
    }
}
