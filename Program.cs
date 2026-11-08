var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

const string AllowAllPolicy = "AllowAllPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Prefijo del microservicio: https://localhost:7126/ms-conciliaciones/...
app.UsePathBase("/ms-conciliaciones");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Swagger UI en https://localhost:7126/ms-conciliaciones/swagger
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/ms-conciliaciones/openapi/v1.json", "Conciliaciones API v1"));
}

app.UseCors(AllowAllPolicy);
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
