using System.Reflection;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ASP0016
app.MapGet("/asp0016", (HttpContext context) => Task.FromResult(context.Request.Query["name"]));

// ASP0020
app.MapGet("/asp0020/{todo}", (Todo todo) => todo);

// ASP0024
app.MapPost("/asp0024", ([FromBody] Todo todo, [FromBody] ITodoService todoService)
    => todoService.AddTodo(todo));

// ASP0015
app.MapGet("/asp0015", (HttpRequest request) =>
{
    if (request.Headers["Content-Type"] != "application/json")
    {
        return null;
    }

    var versionNumber = request.Headers["X-Api-Version"];
    var branchName = request.Headers["X-Branch-Name"];

    return new { versionNumber, branchName };
});

app.Run();

public record Todo(int Id, string Name, bool IsCompleted);

public interface ITodoService
{
    void AddTodo(Todo todo);
}