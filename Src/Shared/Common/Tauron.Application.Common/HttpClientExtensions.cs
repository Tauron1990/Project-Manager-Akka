using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Tauron;

public static class HttpClientExtensions
{
    public static async ValueTask<TResult?> PostJson<TData, TResult>(this HttpClient client, string url, TData data)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(url, data).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResult>().ConfigureAwait(false);
    }

    public static async ValueTask<TResult?> PostJson<TResult>(this HttpClient client, string url, HttpContent content, CancellationToken token)
    {
        HttpResponseMessage response = await client.PostAsync(url, content, token).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResult>(cancellationToken: token).ConfigureAwait(false);
    }
}