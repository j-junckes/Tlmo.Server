using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Tlmo.Repository;
using Tlmo.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tlmo", Version = "v1" });

  c.AddSecurityDefinition("Bearer",
    new OpenApiSecurityScheme
    {
      Description = "JWT Authorization header using the Bearer scheme.\n\n" +
                    "Example: 'Bearer 12345abcdef'",
      Name = "Authorization",
      In = ParameterLocation.Header,
      Type = SecuritySchemeType.ApiKey,
      Scheme = "Bearer"
    });

  c.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecurityScheme
      {
        Reference = new OpenApiReference
        {
          Type = ReferenceType.SecurityScheme,
          Id = "Bearer"
        },
        Scheme = "oauth2",
        Name = "Bearer",
        In = ParameterLocation.Header,
      },
      new List<string>()
    }
  });

  var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
  var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
  c.IncludeXmlComments(xmlPath);
});

builder.Services.AddDbContext<TlmoContext>(options => options
  .UseNpgsql(BuildConnectionString(builder.Configuration), o => o
    .MigrationsAssembly("Tlmo.Server")
    .UseNodaTime())
  .UseSnakeCaseNamingConvention()
);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "http://localhost:5000",
    ValidAudience = builder.Configuration["Jwt:Audience"] ?? "http://localhost:5000",
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
  };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
  app.UseCors(options => options.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chat");

app.Run();

return;

string BuildConnectionString(IConfiguration config)
{
  var host = config["Database:Host"] ?? "localhost";
  var port = config["Database:Port"] ?? "5432";
  var database = config["Database:Database"] ?? "tlmo";
  var username = config["Database:Username"] ?? "postgres";
  var password = config["Database:Password"] ?? "postgres";

  return
    $"Host={host};Port={port};Database={database};Username={username};Password={password};Include Error Detail=True;";
}