using Microsoft.AspNetCore.Hosting;
using RP1AnalyticsWebApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace RP1AnalyticsWebApp.Services
{
    public class HistoricalBenchmarkService
    {
        private readonly IWebHostEnvironment _environment;
        private List<HistoricalBenchmark> _benchmarks;

        public HistoricalBenchmarkService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public List<HistoricalBenchmark> GetHistoricalBenchmarks()
        {
            if (_benchmarks != null)
            {
                return _benchmarks;
            }

            string path = Path.Combine(_environment.ContentRootPath, "Configs", "historicalBenchmarks.json");
            if (!File.Exists(path))
            {
                _benchmarks = new List<HistoricalBenchmark>(0);
                return _benchmarks;
            }

            var json = File.ReadAllText(path);
            _benchmarks = JsonSerializer.Deserialize<List<HistoricalBenchmark>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<HistoricalBenchmark>(0);

            return _benchmarks;
        }
    }
}
