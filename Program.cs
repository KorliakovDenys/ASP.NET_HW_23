using ASP.NET_HW_23.Data;
using ASP.NET_HW_23.Models;
using ASP.NET_HW_23.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IDbService, DbService>();
builder.Services.AddSingleton<IRepository<Message>>(serviceProvider =>
    new Repository<Message>(serviceProvider.GetRequiredService<IDbService>(), "Messages"));

var app = builder.Build();

var webSocketOptions = new WebSocketOptions {
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};

app.UseWebSockets(webSocketOptions);

app.MapControllers();

app.Run();