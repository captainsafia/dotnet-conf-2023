using System.Reflection;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// ASP0025
builder.Services.AddAuthentication();
builder.Services.AddAuthorization(configure: options => 
    options.AddPolicy("required-role", policy =>
        policy.RequireRole("office-admin")));

var app = builder.Build();

// ASP0016
app.MapGet("/asp0016", context => Task.FromResult(context.Request.Query["name"]));

// ASP0020
app.MapGet("/asp0020/{todo}", (Todo todo) => todo);

// ASP0024
app.MapPost("/asp0024", ([FromBody] Todo todo, [FromBody] ITodoService todoService)
    => todoService.AddTodo(todo));

app.Run();

public record Todo(int Id, string Name, bool IsCompleted);

public interface ITodoService
{
    void AddTodo(Todo todo);
}