using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Dapper.DbAccess;

using GraphQL.Data;
using GraphQL.GraphQL;
using GraphQL.GraphQL.DataItem;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mongo.Models;
using Mongo.Services;
using Serilog;
using Serilog.Events;
using L = GraphQL.GraphQL.Lists;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting the web host");
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context,services,configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());
builder.Configuration.AddJsonFile("config/appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"config/appsettings.{builder.Environment.EnvironmentName}.json", 
    optional: false,
    reloadOnChange: true);
var cred = new ClientSecretCredential(builder.Configuration["KeyVaultConfig:TenantId"], builder.Configuration["KeyVaultConfig:ClientId"], builder.Configuration["KeyVaultConfig:ClientSecretId"]);
var client = new SecretClient(new Uri(builder.Configuration["KeyVaultConfig:KVUrl"]), cred);
builder.Configuration.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions()
{  ReloadInterval = TimeSpan.FromMinutes(10) });

builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddType<ItemType>()
    .AddType<L.ListType>()
    .AddMutationType<Mutation>()
    .AddProjections()
    .AddSorting()
    .AddFiltering();
builder.Services.AddPooledDbContextFactory<ApiDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("Default")
    ));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddSingleton<ISqlDataAccess, SqlDataAccess>();
builder.Services.AddSingleton<IUserData, UserData>();
builder.Services.Configure<ExampleSettings>(builder.Configuration.GetSection("ExampleSetting")); 
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));
builder.Services.AddSingleton<IDatabaseSettings>(sp => sp.GetRequiredService<IOptions<DatabaseSettings>>().Value);


builder.Services.AddSingleton<MongoService>();

builder.Services.AddScoped<IProductService, ProductService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSerilogRequestLogging(configure =>
{
    configure.MessageTemplate = "HTTP {RequestMethod} {RequestPath} ({UserId}) responded {StatusCode} in {Elapsed:0.0000}ms";
}); // We want to log all HTTP requests

if(app.Environment.IsDevelopment()){
    //app.UseSwagger();
    //app.UseSwaggerUI();
}
if (!app.Environment.IsDevelopment())
{
    // Do not add exception handler for dev environment. In dev,
    // we get the developer exception page with detailed error info.
    app.UseExceptionHandler(errorApp =>
    {
        // Logs unhandled exceptions. For more information about all the
        // different possibilities for how to handle errors see
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-5.0
        errorApp.Run(async context =>
        {
            // Return machine-readable problem details. See RFC 7807 for details.
            // https://datatracker.ietf.org/doc/html/rfc7807#page-6
            var pd = new ProblemDetails
            {
                Type = "https://demo.api.com/errors/internal-server-error",
                Title = "An unrecoverable error occurred",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "This is a demo error used to demonstrate problem details",
            };
            pd.Extensions.Add("RequestId", context.TraceIdentifier);
            await context.Response.WriteAsJsonAsync(pd, pd.GetType(), null, contentType: "application/problem+json");
        });
    });
}
app.UseSwagger();
app.UseSwaggerUI();
//app.UseHttpsRedirection();
app.ConfigureApi();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapGraphQL();
    endpoints.MapControllers();
});

app.Run();

