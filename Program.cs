using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Watermark.BackgroundServices;
using Watermark.Models;
using Watermark.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(o=>{
    o.UseInMemoryDatabase(databaseName:" productDb");
});

builder.Services.AddSingleton(sp=> new ConnectionFactory(){
    Uri=new Uri(builder.Configuration.GetConnectionString("RabbitMQ"))
});

builder.Services.AddSingleton<RabbitMQService>();
builder.Services.AddSingleton<RabbitMQPublisher>();
builder.Services.AddHostedService<ImageBackgroundService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Create}/{id?}");

app.Run();
