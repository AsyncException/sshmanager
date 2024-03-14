using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Installer;
public static class HttpClientProgressExtensions {
    public static async Task DownloadDataAsync(this HttpClient client, string request_url, Stream destination, IProgress<float>? progress = null, CancellationToken cancellation_token = default) {
        using HttpResponseMessage response = await client.GetAsync(request_url, HttpCompletionOption.ResponseHeadersRead, cancellation_token);
        long? content_length = response.Content.Headers.ContentLength;
        using Stream download = await response.Content.ReadAsStreamAsync(cancellation_token);
        
        if (progress is null || !content_length.HasValue) {
            await download.CopyToAsync(destination, cancellation_token);
            return;
        }
        
        Progress<long> progress_wrapper = new(total_bytes => progress.Report(GetProgressPercentage(total_bytes, content_length.Value)));
        await download.CopyToAsync(destination, 81920, progress_wrapper, cancellation_token);
    }

    static float GetProgressPercentage(float total_bytes, float current_bytes) => (total_bytes / current_bytes) * 100f;
    
    static async Task CopyToAsync(this Stream source, Stream destination, int buffer_size, IProgress<long>? progress = null, CancellationToken cancellation_token = default) {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegative(buffer_size);

        if (!source.CanRead) {
            throw new InvalidOperationException($"'{nameof(source)}' is not readable.");
        }

        if (!destination.CanWrite) {
            throw new InvalidOperationException($"'{nameof(destination)}' is not writable.");
        }

        byte[] buffer = new byte[buffer_size];
        long total_bytes_read = 0;
        int bytes_read;
        while ((bytes_read = await source.ReadAsync(buffer, cancellation_token).ConfigureAwait(false)) != 0) {
            await destination.WriteAsync(buffer.AsMemory(0, bytes_read), cancellation_token).ConfigureAwait(false);
            total_bytes_read += bytes_read;
            progress?.Report(total_bytes_read);
        }
    }
}