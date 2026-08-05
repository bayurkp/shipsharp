using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShipSharp.Infrastructure.Data;

namespace ShipSharp.Tests.Integration;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            services.PostConfigure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
            {
                var stjFormatter = options.OutputFormatters.OfType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonOutputFormatter>().FirstOrDefault();
                if (stjFormatter != null)
                {
                    var serializerOptions = stjFormatter.SerializerOptions;
                    options.OutputFormatters.Remove(stjFormatter);
                    options.OutputFormatters.Insert(0, new StreamSystemTextJsonOutputFormatter(serializerOptions));
                }
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }
}

public class StreamSystemTextJsonOutputFormatter : Microsoft.AspNetCore.Mvc.Formatters.TextOutputFormatter
{
    private readonly System.Text.Json.JsonSerializerOptions _jsonSerializerOptions;

    public StreamSystemTextJsonOutputFormatter(System.Text.Json.JsonSerializerOptions jsonSerializerOptions)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
        SupportedEncodings.Add(System.Text.Encoding.UTF8);
        SupportedMediaTypes.Add("application/json");
        SupportedMediaTypes.Add("text/json");
    }

    public override async Task WriteResponseBodyAsync(Microsoft.AspNetCore.Mvc.Formatters.OutputFormatterWriteContext context, System.Text.Encoding selectedEncoding)
    {
        var httpContext = context.HttpContext;
        var responseStream = httpContext.Response.Body;
        await System.Text.Json.JsonSerializer.SerializeAsync(responseStream, context.Object, context.ObjectType ?? typeof(object), _jsonSerializerOptions, httpContext.RequestAborted);
    }
}
