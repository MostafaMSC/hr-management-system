using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using HR.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HR.Infrastructure.Services;

public class FirebasePushNotificationService : IFirebaseService
{
    private readonly ILogger<FirebasePushNotificationService> _logger;
    private static bool _isFirebaseInitialized = false;

    public FirebasePushNotificationService(IConfiguration configuration, ILogger<FirebasePushNotificationService> logger)
    {
        _logger = logger;

        if (!_isFirebaseInitialized)
        {
            try
            {
                var credentialPath = configuration["Firebase:CredentialPath"];
                if (!string.IsNullOrEmpty(credentialPath) && File.Exists(credentialPath))
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(credentialPath)
                    });
                    _isFirebaseInitialized = true;
                    _logger.LogInformation("Firebase App initialized successfully.");
                }
                else
                {
                    _logger.LogWarning("Firebase CredentialPath is missing or invalid in appsettings.json. Push notifications will be simulated.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase App.");
            }
        }
    }

    public async Task<bool> SendPushNotificationAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null)
    {
        if (string.IsNullOrEmpty(fcmToken)) return false;

        if (!_isFirebaseInitialized)
        {
            _logger.LogInformation("SIMULATED PUSH NOTIFICATION: [To: {FcmToken}] {Title} - {Body}", fcmToken, title, body);
            return true;
        }

        try
        {
            var message = new Message()
            {
                Token = fcmToken,
                Notification = new Notification()
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>()
            };

            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("Successfully sent message: {Response}", response);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to token {FcmToken}", fcmToken);
            return false;
        }
    }

    public async Task<bool> SendMulticastNotificationAsync(List<string> fcmTokens, string title, string body, Dictionary<string, string>? data = null)
    {
        if (fcmTokens == null || !fcmTokens.Any()) return false;

        var validTokens = fcmTokens.Where(t => !string.IsNullOrEmpty(t)).ToList();
        if (!validTokens.Any()) return false;

        if (!_isFirebaseInitialized)
        {
            _logger.LogInformation("SIMULATED MULTICAST PUSH NOTIFICATION: [To {Count} tokens] {Title} - {Body}", validTokens.Count, title, body);
            return true;
        }

        try
        {
            var message = new MulticastMessage()
            {
                Tokens = validTokens,
                Notification = new Notification()
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>()
            };

            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
            _logger.LogInformation("Successfully sent multicast message: {SuccessCount} successful, {FailureCount} failed", response.SuccessCount, response.FailureCount);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending multicast push notification.");
            return false;
        }
    }
}
