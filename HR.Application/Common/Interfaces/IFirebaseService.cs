namespace HR.Application.Common.Interfaces;

public interface IFirebaseService
{
    Task<bool> SendPushNotificationAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null);
    Task<bool> SendMulticastNotificationAsync(List<string> fcmTokens, string title, string body, Dictionary<string, string>? data = null);
}
