using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NETCore.MailKit.Core;
using System.Text;
using UsersManagement.Helpers;
using UsersManagement.Models;
using UsersManagement.Seeders;  
using UsersManagement.Services;

var builder = WebApplication.CreateBuilder(args);
// ============================================
// 1. SERVICE REGISTRATION PHASE
// ============================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "UsersManagement",
            Version = "v1"
        });

    // JWT AUTHORIZATION
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter JWT Token"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

builder.Services.AddDbContext<UsersManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],
//            ValidAudience = builder.Configuration["Jwt:Audience"],
//            IssuerSigningKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(
//                    builder.Configuration["Jwt:Key"]
//                    ?? throw new InvalidOperationException("JWT Key is not configured.")))
//        };
//    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<UsersManagement.Services.EmailService>();


// ADD CORS SERVICE HERE (Before builder.Build())
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy",      
        policy =>
        {
            policy
                .WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


// ============================================
// 2. BUILD THE APP
// ============================================
var app = builder.Build();

// ============================================
// 3. MIDDLEWARE PIPELINE & SEEDING PHASE
// ============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<UsersManagementContext>();
    DatabaseSeeder.Seed(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// USE CORS MIDDLEWARE HERE (After builder.Build() and before Auth)
app.UseCors("AngularPolicy");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();