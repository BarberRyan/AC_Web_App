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
    using SqlConnection c = new(ConStr);
    List<ShopItem> items = new();
    c.Open();
    SqlCommand command = new($"SELECT * FROM items", c);
    SqlDataReader reader = command.ExecuteReader();
    while (reader.Read())
    {
        ShopItem item = new
        (
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetDecimal(3),
            0,
            reader.GetDecimal(5),
            reader.GetInt32(6)
        );
        try
        {
            item.OldPrice = reader.GetDecimal(4);
        }
        catch
        {
            item.OldPrice = 0;
        }
        items.Add(item);
    }
    reader.Close();
    command = new SqlCommand($"SELECT * FROM itemImages", c);
    reader = command.ExecuteReader();
    while (reader.Read())
    {
        foreach (ShopItem item in items)
        {
            if (item.ID == reader.GetInt32(0))
            {
                item.AddImage(reader.GetString(1));
            }
        }
    }
    c.Close();
    return items;
}).WithName("Get Shop Info");

app.MapGet("/getiteminfo", (int itemID) =>
{
    StringBuilder sb = new();
    using SqlConnection c = new(ConStr);
    List<ShopItem> items = new();
    c.Open();
    SqlCommand command = new($"SELECT * FROM items WHERE itemID={itemID}", c);
    SqlDataReader reader = command.ExecuteReader();
    while (reader.Read())
    {
        ShopItem item = new
     (
         reader.GetInt32(0),
         reader.GetString(1),
         reader.GetString(2),
         reader.GetDecimal(3),
         0,
         reader.GetDecimal(5),
         reader.GetInt32(6)
     );
        try
        {
            item.OldPrice = reader.GetDecimal(4);
        }
        catch
        {
            item.OldPrice = 0;
        }
        items.Add(item);
    }
    reader.Close();
    command = new SqlCommand($"SELECT * FROM itemImages", c);
    reader = command.ExecuteReader();
    while (reader.Read())
    {
        foreach (ShopItem item in items)
        {
            if (item.ID == reader.GetInt32(0))
            {
                item.AddImage(reader.GetString(1));
            }
        }
    }
    c.Close();
    return items;
}).WithName("Get Item Info");

/*********
 * LOGIN *
 *********/

app.MapGet("/checkusername", (string username) =>
{
    using SqlConnection c = new(ConStr);
    int salt = 0;
    c.Open();
    SqlCommand command = new("DECLARE @salt int " +
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
    c.Close();
    return salt;
}).WithName("Get Username");

app.MapGet("/checklogin", (string username, string passHash) =>
{
    using SqlConnection c = new(ConStr);
    c.Open();
    SqlCommand command = new("DECLARE @output int " +
                                        "EXEC @output = checklogin @username=@name, @password=@passHash " +
                                        "SELECT @output AS output", c);
    command.Parameters.AddWithValue("@name", username);
    command.Parameters.AddWithValue("@passHash", passHash);
    SqlDataReader reader = command.ExecuteReader();
    int userID = 0;
    while (reader.Read())
    {
        try
        {
            userID = reader.GetInt32(0);
        }
        catch
        {
            userID = 0;
        }
    }
    c.Close();
    return userID;
}).WithName("Check Login");

app.MapPost("/adduser", (string username, string passHash, int salt) =>
{
    using SqlConnection c = new(ConStr);
    int addStatus = 0;
    c.Open();
    SqlCommand command = new("DECLARE @output int " +
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
    c.Close();
    return addStatus;
}).WithName("Add User");

/*****************
 * SHOPPING CART *
 *****************/

app.MapPost("/updatecart", (int userID, int itemID, int qty) =>
{
    using SqlConnection c = new(ConStr);
    c.Open();

    SqlCommand command = new("EXEC updatecart @user_id=@user, @item_id=@item, @item_qty=@qty", c);
    command.Parameters.AddWithValue("@user", userID);
    command.Parameters.AddWithValue("@item", itemID);
    command.Parameters.AddWithValue("@qty", qty);
    command.ExecuteNonQuery();
    
    c.Close();

}).WithName("Update Cart");

app.MapGet("/getcart", (int userID) =>
{
    List<CartItem> items = new();

    using SqlConnection c = new(ConStr);

    c.Open();
    SqlCommand command = new("EXEC getcartinfo @user_id=@user", c);
    command.Parameters.AddWithValue("@user", userID);
    SqlDataReader reader = command.ExecuteReader();
    while (reader.Read())
    {

        try
        {
            CartItem item = new(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetInt32(2)
            );
            items.Add(item);
        }
        catch (Exception e)
        {
            CartItem item = new(
            0,
            $"Error: {e.Message}",
            0
            );
            items.Add(item);
            c.Close();
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
    c.Close();
    return items;
}).WithName("Get Cart");

app.MapPost("/checkout", (int userID) =>
{
    using (SqlConnection c = new(ConStr))
    {
        c.Open();
        SqlCommand command = new("EXEC checkout @user", c);
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

app.MapPost("/addItem", (string name, string desc, decimal price, decimal oldPrice, int qty) =>
{
    using (SqlConnection c = new(ConStr))
    {
        c.Open();
        SqlCommand command = new("EXEC addItem @name= @itemName, @desc= @itemDesc, @price= @curPrice, @oldPrice= @pastPrice, @qty= @itemQty", c);
        command.Parameters.AddWithValue("@itemName", name);
        command.Parameters.AddWithValue("@itemDesc", desc);
        command.Parameters.AddWithValue("@curPrice", price);
        command.Parameters.AddWithValue("@pastPrice", oldPrice);
        command.Parameters.AddWithValue("@itemQty", qty);

        try
        {
            return command.ExecuteScalar();
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}).WithName("Add Item");

app.MapPost("/addImage", (int itemID, string filename) =>
{
    using (SqlConnection c = new(ConStr))
    {
        c.Open();
        SqlCommand command = new("EXEC addImage @itemID= @ID, @imageName= @imageName", c);
        command.Parameters.AddWithValue("@ID", itemID);
        command.Parameters.AddWithValue("@imageName", filename);      
        try
        {
            return command.ExecuteScalar();
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}).WithName("Add Image");

app.Run();