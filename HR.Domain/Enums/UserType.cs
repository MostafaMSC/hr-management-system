namespace HR.Domain.Enums
{
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public enum UserType
    {
        Employee,
        HR,
        Manager,
        Administrator
    }
}
