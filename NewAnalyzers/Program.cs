using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// ASP0025
builder.Services.AddAuthentication();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("required-role", policy =>
        policy.RequireRole("office-admin"));

var app = builder.Build();

// ASP0020
app.MapPost("/asp0020", (Todo todo) => todo);

// ASP0024
app.MapPost("/asp0024", ([FromBody] Todo todo, ITodoService todoService)
    => todoService.AddTodo(todo));

app.Run();

public record Todo(int Id, string Name, bool IsCompleted);

public interface ITodoService
{
    void AddTodo(Todo todo);
}