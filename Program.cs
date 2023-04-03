using Microsoft.OpenApi.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Artigos esportivos API", Description = "Artigos esportivos API Backend", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Artigos esportivos API V1");
});

var connectionString = "Host=localhost;Port=5432;Pooling=true;Database=artigosesportivos;Username=artigosesportivos;Password=12345678";
await using var dataSource = NpgsqlDataSource.Create(connectionString);

app.MapGet("/users/{id}", async (int id) =>
{
    await using var connection = await dataSource.OpenConnectionAsync();
    await using var cmd = new NpgsqlCommand("SELECT \"UserId\", \"Email\", \"Password\", \"Name\" FROM \"Users\" WHERE \"UserId\" = $1", connection)
    {
        Parameters =
        {
            new() { Value = id }
        }
    };

    NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

    if (reader.HasRows)
    {
        reader.Read();
        User user = new User();
        user.UserId = reader.GetInt32(0);
        user.Email = reader.GetString(1);
        user.Password = reader.GetString(2);
        user.Name = reader.GetString(3);
       
        reader.Close();
        await cmd.ExecuteNonQueryAsync();
        return Results.Ok(user);
    }
    else
    {
        reader.Close();
        return Results.NotFound();
    }
});

app.MapPost("/user", async (User user) =>
{
    await using var connection = await dataSource.OpenConnectionAsync();
    await using var cmd = new NpgsqlCommand("INSERT INTO \"Users\"(\"Email\", \"Password\", \"Name\") VALUES ($1, $2, $3)", connection)
    {
        Parameters =
        {
            new() { Value = user.Email },
            new() { Value = user.Password },
            new() { Value = user.Name }
        }
    };

    await cmd.ExecuteNonQueryAsync();

    return Results.Created($"/users/{user.Name}", user);
});

app.MapPut("/users/{id}", async (int id, User user) =>
{
    await using var connection = await dataSource.OpenConnectionAsync();
    await using var cmd = new NpgsqlCommand("SELECT (\"UserId\") FROM \"Users\" WHERE \"UserId\" = $1")
    {
        Parameters =
        {
            new() { Value = id}
        }
    };

    NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

    if (reader.HasRows)
    {
        reader.Close();
        await using var cmd2 = new NpgsqlCommand("UPDATE \"Users\" SET \"Email\" = $1, \"Password\" = $2, \"Name\" = $3 WHERE \"UserId\" = $4;", connection)
        {
            Parameters =
            {
                new() { Value = user.Email },
                new() { Value = user.Password },
                new() { Value = user.Name },
                new() { Value = id },
            }
        };

        await cmd.ExecuteNonQueryAsync();
        return Results.NoContent();
    }
    else
    {
        reader.Close();
        return Results.NotFound();
    }
});

app.MapDelete("/user/{id}", async (int id) =>
{
    await using var connection = await dataSource.OpenConnectionAsync();
    await using var cmd = new NpgsqlCommand("SELECT (\"UserId\") FROM \"Users\" WHERE \"UserId\" = $1")
    {
        Parameters =
        {
            new() { Value = id }
        }
    };

    NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

    if (reader.HasRows)
    {
        reader.Close();
        await using var cmd2 = new NpgsqlCommand("DELETE FROM \"Users\" WHERE \"UserId\" = $1")
        {
            Parameters =
            {
                new() { Value = id }
            }
        };
        await cmd.ExecuteNonQueryAsync();

        return Results.Ok();
    }
    reader.Close();
    return Results.NotFound();
});

// Ainda não funcional
app.MapGet("/products/getall", async () => 
{
    await using var connection = await dataSource.OpenConnectionAsync();
    await using var cmd = new NpgsqlCommand("SELECT \"ProductId\", \"Name\", \"Value\", \"LastValue\", \"Description\", \"Stock\", \"Sales\" FROM \"Products\"");

    NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

    if (!reader.HasRows)
    {
        return Results.NotFound();
    }

    List<Product> products = new List<Product>();

    while (reader.Read())
    {
        var product = new Product();
        product.ProductId = reader.GetInt32(0);
        product.Name = reader.GetString(1);
        product.Value = reader.GetDecimal(2);
        product.LastValue = reader.GetDecimal(3);
        product.Description = reader.GetString(4);
        product.Stock = reader.GetInt32(5);
        product.Sales = reader.GetInt32(6);

        products.Add(product);
    }

    return Results.Ok(products);
});

// **TODO: Finish Request API for the other objects
app.Run();
