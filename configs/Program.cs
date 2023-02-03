using configs;
using configs.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using NLog;

var builder = WebApplication.CreateBuilder(args);

LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));


// Add services to the container.

builder.Services.Configure<ApplicationOptions>(
    builder.Configuration.GetSection(nameof(ApplicationOptions)));
//builder.Services.ConfigureOptions<ApplicationOptionsSetup>();


builder.Services.ConfigureCors();
builder.Services.ConfigureIIsIntegration();
builder.Services.ConfigureLoggerService();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

if(app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
    app.UseHsts();

app.UseHttpsRedirection();

//if we don't specify path it will use wwwroot by default
app.UseStaticFiles();


app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
}) ; //it'll forward proxy headers to current request
//this'll help in linux deployment

//it's best practice to use UseCors before UseAuthorization
app.UseCors("CorsPolicy");


app.UseAuthorization();

app.MapControllers();

app.MapGet("options", (IOptions<ApplicationOptions> options,
    IOptionsSnapshot<ApplicationOptions> optionsSnapshot,
    IOptionsMonitor<ApplicationOptions> optionsMonitor) =>
{
    var response = new
    {
        optionValue = options.Value.ExampleValue,
        SnapShotValue= optionsSnapshot.Value.ExampleValue,
        Monitorvalue= optionsMonitor.CurrentValue.ExampleValue

    };
    return Results.Ok(response);
});


app.Run();
