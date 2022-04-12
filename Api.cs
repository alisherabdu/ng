using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mongo.Models;
using Mongo.Services;
using Polly;
using Serilog;

internal static class Api{
    public static void ConfigureApi(this WebApplication app){
        app.MapGet("/DapperUsers", GetUsers);
        app.MapGet("/ConfigSettings", GetSettings);
        app.MapGet("/Error", Error);
        
        
        app.MapGet("/request-context", (IDiagnosticContext diagnosticContext) =>
        {
            // You can enrich the diagnostic context with custom properties.
            // They will be logged with the HTTP request.
            diagnosticContext.Set("UserId", "someone");
        });
    }

    private static async Task<IResult> GetUsers(IUserData data){
        try
        {
           return Results.Ok(await data.GetUsers()); 
        }
        catch (Exception ex)
        {
            
            return Results.Problem(ex.Message);
        }

    }
    
    private static async Task<IResult> Error(){
            return Results.Problem();
    }
  
    
    

    private static IResult GetSettings(
        IConfiguration config,
        IOptions<ExampleSettings> options,
        IOptionsMonitor<ExampleSettings> optionsMonitor,
        IOptionsSnapshot<ExampleSettings> optionsSnapshot
        ,IOptions<DatabaseSettings> dboptions
    ){

        try
        {
            var retryPolicy = Policy.Handle<Exception>()
                .RetryAsync(2, async (ex, count, context) =>
                {
                    //(config as IConfigurationRoot).Reload();
                });

            return Results.Ok(new {
               config = config.GetValue<string>("ExampleSetting:One"),
               optionValueSingleton = options.Value.One,
               optionMonitorTransient = optionsMonitor.CurrentValue.One,
               optionSnapshotScoped = optionsSnapshot.Value.One,               
               optionMonitorTransientTwo = optionsMonitor.CurrentValue.Two
               ,dbSettingoptions = dboptions.Value.MongoConnectionString

           }); 
        }
        catch (Exception ex)
        {
            
            return Results.Problem(ex.Message);
        }

    }
}