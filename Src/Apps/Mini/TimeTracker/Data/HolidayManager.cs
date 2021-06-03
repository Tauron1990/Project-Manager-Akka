using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Akka.Util;
using Newtonsoft.Json;
using Tauron.Application;

namespace TimeTracker.Data
{
    public sealed class HolidayManager : IDisposable
    {
        private const string FileNameBase = "Holidays-";
        private readonly ITauronEnviroment _enviroment;
        private readonly SemaphoreSlim _lock = new(0, 1);
        private readonly HttpClient _httpClient = new();

        public HolidayManager(ITauronEnviroment enviroment) => _enviroment = enviroment;

        public async Task<IEnumerable<int>> RequestFor(DateTime mouth)
        {
            var data = await RequestYear(mouth.Year);

            return from feiertage in data
                   where feiertage.Datum.Month == mouth.Month
                   select feiertage.Datum.Day;
        }

        public async Task<bool> IsHoliday(DateTime mouth, int day) 
            => (await RequestFor(mouth)).Contains(day);

        private int _cachedYear = -1;
        private ImmutableList<Feiertage> _cachedList = ImmutableList<Feiertage>.Empty;

        private async Task<ImmutableList<Feiertage>> RequestYear(int year)
        {
            await _lock.WaitAsync();

            try
            {
                if (_cachedYear == year)
                    return _cachedList;

                string yearFileName = Path.Combine(_enviroment.AppData(), $"{FileNameBase}{year}.json");
                if (File.Exists(yearFileName))
                {
                    var tryRead = await Try.FromAsync(async () =>
                                                      {
                                                          var content = await File.ReadAllTextAsync(yearFileName);
                                                          return JsonConvert.DeserializeObject<ImmutableList<Feiertage>>(content) ?? ImmutableList<Feiertage>.Empty;
                                                      });

                    if (tryRead.IsSuccess)
                    {
                        var list = tryRead.Get();
                        if (list.Count != 0)
                            return ApplyCache(list);
                    }
                }

                using var result = await _httpClient.GetAsync("https://www.spiketime.de/feiertagapi/feiertage/BY/" + year);
                string resultString = await result.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ImmutableList<Feiertage>>(resultString) ?? ImmutableList<Feiertage>.Empty;

                try
                {
                    await File.WriteAllTextAsync(FileNameBase, JsonConvert.SerializeObject(data));
                }
                catch (IOException) { }

                return ApplyCache(data); ;
            }
            finally
            {
                _lock.Release();
            }

            ImmutableList<Feiertage> ApplyCache(ImmutableList<Feiertage> list)
            {
                _cachedYear = year;
                _cachedList = list;

                return list;
            }
        }

        public void Dispose()
        {
            _lock.Dispose();
            _httpClient.Dispose();
        }
    }
}