using Microsoft.AspNetCore.Builder;
using System.Data.SqlClient;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var values = new List<(int, string, string, decimal, decimal, decimal)>();

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/getshopinfo", (int? recordNum) =>
{
    StringBuilder sb = new();
    using (SqlConnection c = new SqlConnection("Server=tcp:planetexpress.database.windows.net,1433;Initial Catalog=AmazoniaCheckout;Persist Security Info=False;User ID=AC_API;Password=PgTeam2023;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
    {
        c.Open();
        SqlCommand command = new SqlCommand($"SELECT * FROM items LEFT JOIN itemImages ON items.itemID=itemImages.itemID WHERE items.itemID={recordNum}", c);
        SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            sb.Append(reader.GetInt32(0) + " | ");
            sb.Append(reader.GetString(1) + " | ");
            sb.Append(reader.GetString(2) + " | ");
            sb.Append(reader.GetDecimal(3) + " | ");
            try
            {
                sb.Append(reader.GetDecimal(4) + " | ");
            }
            catch
            {
                sb.Append("NULL | ");
            }
            sb.Append(reader.GetDecimal(5) + " | ");
            try
            {
                sb.Append(reader.GetString(6));
            }
            catch
            {
                sb.Append("NULL");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}).WithName("GetShopInfo");


app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}