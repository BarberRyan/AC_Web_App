using AC_Web_App;
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

app.MapGet("/getshopinfo", () =>
{
    StringBuilder sb = new();
    using (SqlConnection c = new SqlConnection("Server=tcp:planetexpress.database.windows.net,1433;Initial Catalog=AmazoniaCheckout;Persist Security Info=False;User ID=AC_API;Password=PgTeam2023;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
    {
        List<ShopItem> items = new List<ShopItem>();
        c.Open();
        SqlCommand command = new SqlCommand($"SELECT * FROM items", c);
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            ShopItem item = new ShopItem();
            item.Id = reader.GetInt32(0);
            item.Name = reader.GetString(1);
            item.Desc = reader.GetString(2);
            item.Price = reader.GetDecimal(3);
            try
            {
                item.OldPrice = reader.GetDecimal(4);
            }
            catch
            {
                item.OldPrice = 0;
            }
            item.Rating = reader.GetDecimal(5);
            items.Add(item);
        }
        reader.Close();
        command = new SqlCommand($"SELECT * FROM itemImages", c);
        reader = command.ExecuteReader();
        while (reader.Read())
        {
            foreach(ShopItem item in items)
            {
                if(item.Id == reader.GetInt32(0))
                {
                    item.AddImage(reader.GetString(1));
                }
            }
        }
        return items;
    }
}).WithName("GetShopInfo");

app.Run();