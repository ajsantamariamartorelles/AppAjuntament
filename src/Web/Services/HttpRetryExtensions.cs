using System.Net.Sockets;
using System.Net;

namespace AppAjuntament.Services
{
    public static class HttpRetryExtensions
    {
        public static async Task<HttpResponseMessage?> GetWithRetryAsync(
            this HttpClient httpClient, 
            string requestUri, 
            int maxRetries = 3,
            int baseDelayMs = 2000,
            ILogger? logger = null,
            CancellationToken cancellationToken = default)
        {
            var attempt = 0;
            Exception? lastException = null;

            while (attempt < maxRetries)
            {
                attempt++;
                
                try
                {
                    logger?.LogInformation("Intentant petició HTTP (intent {Attempt}/{MaxAttempts}): {Uri}", 
                        attempt, maxRetries, requestUri);
                    
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(TimeSpan.FromMinutes(3));
                    
                    var response = await httpClient.GetAsync(requestUri, cts.Token);
                    response.EnsureSuccessStatusCode();
                    
                    logger?.LogInformation("Petició HTTP exitosa en l'intent {Attempt}", attempt);
                    return response;
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || ex.CancellationToken.IsCancellationRequested)
                {
                    lastException = ex;
                    logger?.LogWarning("Timeout en l'intent {Attempt}/{MaxAttempts}: {Error}", attempt, maxRetries, ex.Message);
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    logger?.LogWarning("Error de xarxa en l'intent {Attempt}/{MaxAttempts}: {Error}", attempt, maxRetries, ex.Message);
                }
                catch (SocketException ex)
                {
                    lastException = ex;
                    logger?.LogWarning("Error de socket en l'intent {Attempt}/{MaxAttempts}: {Error}", attempt, maxRetries, ex.Message);
                }
                catch (IOException ex)
                {
                    lastException = ex;
                    logger?.LogWarning("Error d'I/O en l'intent {Attempt}/{MaxAttempts}: {Error}", attempt, maxRetries, ex.Message);
                }
                catch (Exception ex)
                {
                    // Per altres errors no recuperables, no reintentis
                    logger?.LogError(ex, "Error no recuperable en petició HTTP");
                    return null;
                }

                // Si no ós l'óltim intent, espera abans de tornar a intentar
                if (attempt < maxRetries)
                {
                    var delay = baseDelayMs * attempt; // Exponential backoff
                    logger?.LogInformation("Esperant {Delay}ms abans del segóent intent...", delay);
                    await Task.Delay(delay, cancellationToken);
                }
            }

            logger?.LogError(lastException, "Error en petició HTTP desprós de {MaxAttempts} intents: {Uri}", maxRetries, requestUri);
            return null;
        }
    }
}