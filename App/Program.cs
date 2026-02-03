using System.Configuration;
using System.Text;
using App.Data;
using App.Models;
using App.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;
if (env.IsDevelopment())
{
    builder.Configuration.AddEnvironmentVariables();
}

var host = Environment.GetEnvironmentVariable("MYSQLHOST")
    ?? throw new Exception("MYSQLHOST not set");

var port = Environment.GetEnvironmentVariable("MYSQLPORT")
    ?? "3306";

var database = Environment.GetEnvironmentVariable("MYSQLDATABASE")
    ?? throw new Exception("MYSQL_DATABASE not set");

var user = Environment.GetEnvironmentVariable("MYSQLUSER")
    ?? throw new Exception("MYSQL_USER not set");

var password = Environment.GetEnvironmentVariable("MYSQLPASSWORD")
    ?? throw new Exception("MYSQL_PASSWORD not set");

var connectionString = $"Server={host};Port={port};Database={database};User={user};Password={password};";



// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(9, 0, 0)));
});

builder.Services.AddScoped<TokenService>();

builder.Services.AddIdentity<UserModel, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 16;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["JWT_KEY"] ?? throw new Exception("JWT_KEY not set in .env.");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT_ISSUER"],
        ValidAudience = builder.Configuration["JWT_AUDIENCE"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("auth"))
            {
                context.Token = context.Request.Cookies["auth"];
                Console.WriteLine($"Token retrieved from cookie: {context.Token?.Substring(0, 20)}...");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
