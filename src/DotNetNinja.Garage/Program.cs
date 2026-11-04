using DotNetNinja.Garage.Configuration;

var builder = WebApplication.CreateBuilder(args);
var startConfig = builder.AddAutoConfiguration();

// Add services to the container.
builder.Services.AddControllersWithViews().WithContext();
builder.Services.AddApplicationServices(startConfig);

var app = builder.Build();

app.UseErrorHandler();
app.UseDefaultMiddleware(true);
app.UseApplicationRouting();


app.Run();
