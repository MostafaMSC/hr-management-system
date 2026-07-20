using System;
using Npgsql;

var connString = "Host=localhost;Port=5432;Database=HRManagementDB;Username=postgres;Password=postgres";
using var conn = new NpgsqlConnection(connString);
conn.Open();
using var cmd = new NpgsqlCommand("SELECT \"Email\", \"PasswordHash\" FROM \"UserInfos\" WHERE \"Email\"='admin@admin.com'", conn);
using var reader = cmd.ExecuteReader();
while (reader.Read()) {
    Console.WriteLine($"Email: {reader.GetString(0)}");
    Console.WriteLine($"PasswordHash: {reader.GetString(1)}");
}
