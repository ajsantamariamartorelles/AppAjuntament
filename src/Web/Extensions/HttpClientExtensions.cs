using System.Net;

namespace AppAjuntament.Extensions;

public static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage?> GetWithRetryAsync(
        this HttpClient httpClient, 
        string requestUri, 
        int maxRetries = 3, 
        ILogger? logger = null)
    {
        HttpResponseMessage? response = null;
        var delay = TimeSpan.FromSeconds(1);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                logger?.LogInformation("Intentant petició HTTP (intent {Attempt}/{MaxRetries}): {Uri}", 
                    attempt, maxRetries, requestUri);

                response = await httpClient.GetAsync(requestUri);
                
                if (response.IsSuccessStatusCode)
                {
                    logger?.LogInformation("Petició HTTP exitosa en l'intent {Attempt}", attempt);
                    return response;
                }
                else
                {
                    logger?.LogWarning("Petició HTTP fallida amb codi {StatusCode} en l'intent {Attempt}/{MaxRetries}", 
                        response.StatusCode, attempt, maxRetries);
                }
            }
            catch (HttpRequestException ex)
            {
                logger?.LogWarning(ex, "Error de xarxa en l'intent {Attempt}/{MaxRetries}: {Message}", 
                    attempt, maxRetries, ex.Message);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                logger?.LogWarning(ex, "Timeout en l'intent {Attempt}/{MaxRetries}", attempt, maxRetries);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error inesperat en l'intent {Attempt}/{MaxRetries}: {Message}", 
                    attempt, maxRetries, ex.Message);
            }

            // Si no és l'últim intent, esperar abans de tornar a intentar
            if (attempt < maxRetries)
            {
                logger?.LogInformation("Esperant {Delay}ms abans del següent intent...", delay.TotalMilliseconds);
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); // Exponential backoff
            }

            response?.Dispose();
            response = null;
        }

        logger?.LogError("Tots els intents han fallat després de {MaxRetries} intents", maxRetries);
        return null;
    }
}
