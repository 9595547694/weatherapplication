var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var a = "121323";
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>

    {

        options.RoutePrefix = string.Empty;

        options.SwaggerEndpoint("swagger/v1/swagger.json", "WeatherForecast");

    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
