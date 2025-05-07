using chatbot;
using chatbot.Sanayii.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// ✅ Standard Swagger setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Regiseration of the ChatService
builder.Services.AddHttpClient<IChatService, ChatService>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
    app.UseSwagger();
    app.UseSwaggerUI(); // Optional: customize with options
}

app.UseHttpsRedirection();




app.UseAuthorization();

app.MapControllers();

app.Run();
