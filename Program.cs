using ezCloud.SignalR;
using ezCloud.SignalR.Hubs;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddRazorPages();

builder.Services.AddSignalR()
    .AddStackExchangeRedis(builder.Configuration.GetSection("Redis")["Connection"], configuration =>
    {
        configuration.Configuration.ConfigurationChannel = builder.Configuration["Redis:Channel"];

    });

builder.Services.AddHostedService<RabbitMQReceiverService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.SetIsOriginAllowed(origin => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors();

app.UseAuthorization();

app.MapRazorPages();

app.MapHub<SignalRHub>("/SignalRHub");

app.Run();
