using Bundle.Client;
using Bundle.Client.Authorization;
using Bundle.Client.Extensions;
using Bundle.Client.Options;
using Microsoft.Extensions.DependencyInjection;

var builder = MinecraftApplication.CreateBuilder();

//добавляем базовые сервисы
builder.Services.AddDefualt();
builder.Services.Configure<ServerOptions>(p =>
{
    p.ServerIP = "serverId";
});

builder.Services.Configure<UserSign>(p =>
{
    p.Login = $"UserName";
});

//Собираем приложение
var app = builder.Build();

app.UseTimeoutDetector();

app.NewMinecraftClient();
app.StartMinecraftClient();
app.StartTimeoutDetector();