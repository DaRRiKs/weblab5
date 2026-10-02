using ProductClient.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IProductApiService, ProductApiService>();
builder.Services.AddHttpClient("ProductApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ProductApi:BaseAddress"]!);
});

var app = builder.Build();

app.UseRouting();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
