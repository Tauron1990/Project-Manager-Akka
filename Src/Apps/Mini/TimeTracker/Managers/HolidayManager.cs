using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tauron.Application;
using TimeTracker.Data;

namespace TimeTracker.Managers
{
    public sealed class HolidayManager : IDisposable
    {
        private const string FileNameBase = "Holidays-";

        private readonly IDisposable _cleanUp;
        private readonly ITauronEnviroment _enviroment;
        private readonly HttpClient _httpClient = new();
        private readonly SemaphoreSlim _lock = new(1);
        private ImmutableList<Feiertage> _cachedList = ImmutableList<Feiertage>.Empty;

        private int _cachedYear = -1;

        public HolidayManager(ITauronEnviroment enviroment)
        {
            _enviroment = enviroment;
            _cleanUp = Disposable.Create(
                (_lock, _httpClient),
                s =>
                {
                    (SemaphoreSlim sema, HttpClient htclient) = s;

                    sema.Dispose();
                    htclient.Dispose();
                });
        }

        void IDisposable.Dispose() => _cleanUp.Dispose();

        public async Task<IEnumerable<int>> RequestFor(DateTime mouth)
        {
            var data = await RequestYear(mouth.Year).ConfigureAwait(false);

            return from feiertage in data
                   where feiertage.Datum.Month == mouth.Month
                   select feiertage.Datum.Day;
        }

        public async Task<bool> IsHoliday(DateTime mouth, int day)
            => (await RequestFor(mouth).ConfigureAwait(false)).Contains(day);

        private async Task<ImmutableList<Feiertage>?> RequestYear(int year)
        {
            await _lock.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_cachedYear == year)
                    return _cachedList;

                string yearFileName = Path.Combine(_enviroment.AppData(), $"{FileNameBase}{year}.json");
                if (File.Exists(yearFileName))
                {
                    var tryRead = await Try.FromAsync(
                        async () =>
                        {
                            var content = await File.ReadAllTextAsync(yearFileName).ConfigureAwait(false);

                            return JsonConvert.DeserializeObject<ImmutableList<Feiertage>>(content);
                        }).ConfigureAwait(false);

                    if (tryRead.IsSuccess)
                    {
                        var list = tryRead.Get();

                        if (list is not null && list.Count != 0)
                            return ApplyCache(list);
                    }
                }

                using var result = await _httpClient.GetAsync("https://www.spiketime.de/feiertagapi/feiertage/BY/" + year).ConfigureAwait(false);
                string resultString = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<ImmutableList<Feiertage>>(resultString);

                try
                {
                    await File.WriteAllTextAsync(yearFileName, JsonConvert.SerializeObject(data)).ConfigureAwait(false);
                }
                catch (IOException) { }

                return data is null ? null : ApplyCache(data);

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
    }
}