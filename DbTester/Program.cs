using System;
using MySqlConnector;

var connString = "Server=localhost;Database=HRManagementDB;User=root;Password=;";
using var conn = new MySqlConnection(connString);
conn.Open();
using var cmd = new MySqlCommand("SELECT `Email`, `PasswordHash` FROM `UserInfos` WHERE `Email`='admin@admin.com'", conn);
using var reader = cmd.ExecuteReader();
while (reader.Read()) {
    Console.WriteLine($"Email: {reader.GetString(0)}");
    Console.WriteLine($"PasswordHash: {reader.GetString(1)}");
}
