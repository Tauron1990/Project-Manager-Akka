using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Akka.Util;
using Newtonsoft.Json;

namespace TimeTracker.Data
{
    public sealed class HolidayManager
    {
        private const string FileNameBase = "Holidays-";
        
        public async Task<IEnumerable<int>> RequestFor(DateTime mouth)
        {

        }

        public async Task<bool> IsHoliday(DateTime mouth, int day)
        {

        }

        private int _cachedYear = -1;
        private ImmutableList<Feiertage> _cachedList = ImmutableList<Feiertage>.Empty;
        private SemaphoreSlim _lock = new(0, 1);

        private async Task<ImmutableList<Feiertage>> RequestYear(int year)
        {
            await _lock.WaitAsync();

            try
            {
                if (_cachedYear == year)
                    return _cachedList;

                string yearFileName = $"{FileNameBase}{year}.json";
                if (File.Exists(yearFileName))
                {
                    var tryRead = await Try.FromAsync(async () =>
                                                      {
                                                          var content = await File.ReadAllTextAsync(yearFileName);
                                                          return JsonConvert.DeserializeObject<ImmutableList<Feiertage>>(content);
                                                      });

                    if(tryRead.IsSuccess)

                }
            }
            finally
            {
                _lock.Release();
            }

            void ApplyCache(ImmutableList<Feiertage> list)
            {
                _cachedYear = year;
                _cachedList = list;
            }
        }
    }
}