using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Installer;
public static class HttpClientProgressExtensions {
    public static async Task DownloadDataAsync(this HttpClient client, string request_url, Stream destination, IProgress<float> progress = null, CancellationToken cancellationToken = default(CancellationToken)) {
        using (var response = await client.GetAsync(request_url, HttpCompletionOption.ResponseHeadersRead)) {
            var content_length = response.Content.Headers.ContentLength;
            using (var download = await response.Content.ReadAsStreamAsync()) {
                // no progress... no contentLength... very sad
                if (progress is null || !content_length.HasValue) {
                    await download.CopyToAsync(destination);
                    return;
                }
                // Such progress and contentLength much reporting Wow!
                var progress_wrapper = new Progress<long>(total_bytes => progress.Report(GetProgressPercentage(total_bytes, content_length.Value)));
                await download.CopyToAsync(destination, 81920, progress_wrapper, cancellationToken);
            }
        }

        float GetProgressPercentage(float total_bytes, float current_bytes) => (total_bytes / current_bytes) * 100f;
    }

    static async Task CopyToAsync(this Stream source, Stream destination, int buffer_size, IProgress<long> progress = null, CancellationToken cancellationToken = default(CancellationToken)) {
        if (buffer_size < 0)
            throw new ArgumentOutOfRangeException(nameof(buffer_size));
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (!source.CanRead)
            throw new InvalidOperationException($"'{nameof(source)}' is not readable.");
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));
        if (!destination.CanWrite)
            throw new InvalidOperationException($"'{nameof(destination)}' is not writable.");

        var buffer = new byte[buffer_size];
        long total_bytes_read = 0;
        int bytes_read;
        while ((bytes_read = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0) {
            await destination.WriteAsync(buffer, 0, bytes_read, cancellationToken).ConfigureAwait(false);
            total_bytes_read += bytes_read;
            progress?.Report(total_bytes_read);
        }
    }
}