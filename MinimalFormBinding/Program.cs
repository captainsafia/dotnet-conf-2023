using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAntiforgery();

var app = builder.Build();

app.UseAntiforgery();

app.MapGet("/", (HttpContext context, IAntiforgery antiforgery) =>
{
    var token = antiforgery.GetAndStoreTokens(context);
    var html = $$"""
        <html>
            <head>
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        margin: 20px;
                    }

                    form {
                        max-width: 400px;
                        margin: 0 auto;
                    }

                    label {
                        display: block;
                        margin-top: 10px;
                    }

                    input, select {
                        width: 100%;
                        padding: 8px;
                        margin-top: 5px;
                        margin-bottom: 10px;
                        box-sizing: border-box;
                    }

                    button {
                        background-color: #4CAF50;
                        color: white;
                        padding: 10px 15px;
                        border: none;
                        border-radius: 4px;
                        cursor: pointer;
                    }

                    button:hover {
                        background-color: #45a049;
                    }
                </style>
            </head>
            <body>
                <form action="/todos" method="POST" enctype="multipart/form-data">
                    <input name="{{token.FormFieldName}}" type="hidden" value="{{token.RequestToken}}" />
                    <label for="name">Todo Name:</label>
                    <input type="text" name="name" />
                    <label for="dueDate">Due Date:</label>
                    <input type="date" name="dueDate" />
                    <label for="isCompleted">Done:</label>
                    <input type="checkbox" name="isCompleted" />
                    <input type="submit" />
                </form>
            </body>
        </html>
    """;
    return Results.Content(html, "text/html");
});

app.MapPost("/todos", ([FromForm] Todo todo) => todo);

app.Run();

class Todo
{
    public string Name { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
    public DateTime DueDate { get; set; } = DateTime.Now.Add(TimeSpan.FromDays(1));
}