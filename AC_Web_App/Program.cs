using AC_Web_App;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
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

string ConStr = "Server=tcp:planetexpress.database.windows.net,1433;Initial Catalog=AmazoniaCheckout;Persist Security Info=False;User ID=AC_API;Password=PgTeam2023;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

/*************
 * ITEM INFO *
 *************/

app.MapGet("/getshopinfo", () =>
{
    StringBuilder sb = new();
    using (SqlConnection c = new SqlConnection(ConStr))
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
            item.Qty = reader.GetInt32(6);
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
}).WithName("Get Shop Info");

app.MapGet("/getiteminfo", (int itemID) =>
{
    StringBuilder sb = new();
    using (SqlConnection c = new SqlConnection(ConStr))
    {
        List<ShopItem> items = new List<ShopItem>();
        c.Open();
        SqlCommand command = new SqlCommand($"SELECT * FROM items WHERE itemID={itemID}", c);
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
            foreach (ShopItem item in items)
            {
                if (item.Id == reader.GetInt32(0))
                {
                    item.AddImage(reader.GetString(1));
                }
            }
        }
        return items;
    }
}).WithName("Get Item Info");

/*********
 * LOGIN *
 *********/

app.MapGet("/checkusername", (string username) =>
{
    using (SqlConnection c = new SqlConnection(ConStr))
    {
        int salt = 0;
        c.Open();
        SqlCommand command = new SqlCommand("DECLARE @salt int " +
                                            "EXEC checkusername @name, @salt OUTPUT " +
                                            "SELECT @salt AS passwordSalt", c);
        command.Parameters.AddWithValue("@name", username);
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            try
            {
                salt = reader.GetInt32(0);
            }
            catch
            {
                salt = 0;
            }
        }
        return salt;
    }
}).WithName("Get Username");

app.MapGet("/checklogin", (string username, string passHash) =>
{
    using (SqlConnection c = new SqlConnection(ConStr))
    {
        c.Open();
        SqlCommand command = new SqlCommand("DECLARE @output int " +
                                            "EXEC @output = checklogin @username=@name, @password=@passHash " +
                                            "SELECT @output AS output", c);
        command.Parameters.AddWithValue("@name", username);
        command.Parameters.AddWithValue("@passHash", passHash);
        SqlDataReader reader = command.ExecuteReader();
        User outputuser = new();
        while (reader.Read())
        {
            try
            {
                outputuser.Id = reader.GetInt32(0);

                if(outputuser.Id != 0)
                {
                    outputuser.Name = reader.GetString(1);
                }
                else
                {
                    outputuser.Name = "USER NOT FOUND";
                }

            }
            catch
            {
                outputuser.Id = 0;
                outputuser.Name = "USER NOT FOUND";
            }
        }
        return outputuser;
    }
}).WithName("Check Login");

app.MapGet("/adduser", (string username, string passHash, int salt) =>
{
    using (SqlConnection c = new SqlConnection(ConStr))
    {
        int addStatus = 0;
        c.Open();
        SqlCommand command = new SqlCommand("DECLARE @output int " +
                                            "EXEC @output = adduser @username=@name, @passwordHash=@passHash, @salt=@passSalt " +
                                            "SELECT @output AS output", c);
        command.Parameters.AddWithValue("@name", username);
        command.Parameters.AddWithValue("@passHash", passHash);
        command.Parameters.AddWithValue("@passSalt", salt);
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            try
            {
                addStatus = reader.GetInt32(0);
            }
            catch
            {
                addStatus = 0;
            }
        }
        return addStatus;
    }
}).WithName("Add User");

/*****************
 * SHOPPING CART *
 *****************/

app.MapPost("/updatecart", (int userID, int itemID, int qty) =>
{
    using (SqlConnection c = new SqlConnection(ConStr))
    {
        c.Open();
        SqlCommand command = new SqlCommand("EXEC updatecart @user_id=@user, @item_id=@item, @item_qty=@qty", c);
        command.Parameters.AddWithValue("@user", userID);
        command.Parameters.AddWithValue("@item", itemID);
        command.Parameters.AddWithValue("@qty", qty);
        command.ExecuteNonQuery();
    }
}).WithName("Update Cart");

app.MapGet("/getcart", (int userID) =>
{
    List<CartItem> items = new();
    
    using (SqlConnection c = new SqlConnection(ConStr))
    {

        c.Open();
        SqlCommand command = new SqlCommand("EXEC getcartinfo @user_id=@user", c);
        command.Parameters.AddWithValue("@user", userID);
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            CartItem item = new();
            try
            {
                item.Id = reader.GetInt32(0);
                item.Name = reader.GetString(1);
                item.Qty = reader.GetInt32(2);
                items.Add(item);
            }
            catch(Exception e)
            {
                item.Id = 0;
                item.Name = $"ERROR: {e.Message}";
                item.Qty = 0;
                items.Add(item);
                return items;
            }
        }
        reader.Close();
        command = new SqlCommand($"SELECT * FROM itemImages", c);
        reader = command.ExecuteReader();
        while (reader.Read())
        {
            foreach (CartItem item in items)
            {
                if (item.Id == reader.GetInt32(0))
                {
                    item.AddImage(reader.GetString(1));
                }
            }
        }
        return items;
    }
}).WithName("Get Cart");

app.MapPost("/checkout", (int userID) =>
{
    using (SqlConnection c = new SqlConnection(ConStr))
    {
        c.Open();
        SqlCommand command = new SqlCommand("EXEC checkout @user", c);
        command.Parameters.AddWithValue("@user", userID);
        try
        {
            command.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
    return null;
}).WithName("Checkout");

app.Run();