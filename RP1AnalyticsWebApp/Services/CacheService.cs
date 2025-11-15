using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Caching.Hybrid;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using RP1AnalyticsWebApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RP1AnalyticsWebApp.Services
{
    public class CacheService
    {
        private const string CacheKey = "AllCareerLogs";

        private readonly HybridCache _cache;
        private readonly TelemetryClient _telemetry;
        private readonly ICareerLogDatabaseSettings _dbSettings;

        public CacheService(HybridCache cache, TelemetryClient telemetry, ICareerLogDatabaseSettings dbSettings)
        {
            _cache = cache;
            _telemetry = telemetry;
            _dbSettings = dbSettings;
        }

        public async Task<List<CareerLog>> FetchAllCareersAsync(IMongoCollection<CareerLog> careerLogs)
        {
            CachedCareerLogs cacheEntry = await _cache.GetOrCreateAsync(
                CacheKey,
                FetchFromDB(careerLogs));

            return cacheEntry.Items;
        }

        public async Task InvalidateAsync()
        {
            _telemetry.TrackEvent("InvalidateCache");
            await _cache.RemoveAsync(CacheKey);
        }

        public async Task InitCacheAsync(CancellationToken ct = default)
        {
            _telemetry.TrackEvent("InitCacheAsync");
            IMongoCollection<CareerLog> careerLogs = CareerLogService.CreateCareerLogsMongoClient(_dbSettings);

            await _cache.GetOrCreateAsync(
                CacheKey,
                FetchFromDB(careerLogs),
                cancellationToken: ct);
        }

        private Func<CancellationToken, ValueTask<CachedCareerLogs>> FetchFromDB(IMongoCollection<CareerLog> careerLogs)
        {
            return async cancel =>
            {
                var sw = Stopwatch.StartNew();
                int totalCount = (int)await careerLogs.EstimatedDocumentCountAsync(cancellationToken: cancel);
                int batchCount = Math.Clamp(totalCount / 50, 1, 10);
                int defBatchSize = totalCount / batchCount;

                var subLists = new List<CareerLog>[batchCount];
                await Parallel.ForAsync(0, batchCount, async (int i, CancellationToken ct) =>
                {
                    var sw = Stopwatch.StartNew();
                    int skip = i * defBatchSize;
                    int curBatchSize = i == batchCount - 1 ? int.MaxValue : defBatchSize;
                    List<CareerLog> itemBatch = await careerLogs.AsQueryable().Skip(skip).Take(curBatchSize).ToListAsync(ct);
                    Console.WriteLine($"AllCareerLogs subbatch: {sw.ElapsedMilliseconds:N0} ms");
                    subLists[i] = itemBatch;
                });

                List<CareerLog> allItems = subLists.SelectMany(l => l).ToList();
                Console.WriteLine($"AllCareerLogs: {allItems.Count} items in {sw.ElapsedMilliseconds:N0} ms");
                _telemetry.TrackEvent("FetchAllCareers", metrics: new Dictionary<string, double>
                {
                    { "QueryDuration", sw.ElapsedMilliseconds }
                });
                return new CachedCareerLogs { Items = allItems };
            };
        }
    }
}
