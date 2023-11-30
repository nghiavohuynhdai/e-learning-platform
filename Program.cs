using AutoMapper;
using Cursus.Data;
using Cursus.Data.Interface;
using Cursus.Entities;
using Cursus.GlobalExceptionHandler;
using Cursus.ObjectMapping;
using Cursus.Repositories;
using Cursus.Repositories.Interfaces;
using Cursus.Services;
using Cursus.Services.Interfaces;
using Cursus.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using AspNetCoreRateLimit;
using Cursus.Authentication;
using Cursus.Constants;
using Cursus.DTO;
using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.PostgreSql;
using payment.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

//....

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(options =>
        options.WithOrigins(builder.Configuration["AllowedHosts"].Split())
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging())
{
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme()
                {
                    In = ParameterLocation.Header,
                    Name = "Bearer",
                    Reference = new OpenApiReference()
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    },
                },
                new List<string>()
            }
        });
    });
}

if (builder.Environment.IsStaging() || builder.Environment.IsProduction())
{
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseColouredConsoleLogProvider()
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("LocalDbContext")));

    builder.Services.AddHangfireServer();
}

builder.Services.AddDbContext<MyDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("LocalDbContext"));
});
builder.Services.AddSingleton<IMongoDbContext, MongoDbContext>();

builder.Services.AddControllers(options => { options.Filters.Add<GlobalExceptionHandler>(); });

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<MyDbContext>();
    // .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
        };
        options.Events = new ApplicationJwtBearEvents();
    });

builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddAutoMapper(typeof(CursusAutoMapperProfile).Assembly);
builder.Services.AddTransient<ISectionService, SectionService>();
builder.Services.AddTransient<ICourseService, CourseService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddTransient<ICourseCatalogService, CourseCatalogService>();
builder.Services.AddTransient<ICatalogService, CatalogService>();
builder.Services.AddTransient<ICartService, CartService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ILessonService, LessonService>();
builder.Services.AddTransient<IGoogleService, GoogleService>();
builder.Services.AddTransient<IQuizService, QuizService>();
builder.Services.AddTransient<IInstructorService, InstructorService>();
builder.Services.AddTransient<IVnPayService, VnPayService>();
builder.Services.AddTransient<IDataService, DataService>();
builder.Services.AddTransient<IAssignmentService, AssignmentService>();
builder.Services.AddSingleton<IRedisService>(new RedisService(
    !builder.Environment.IsDevelopment()
        ? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
        : builder.Configuration.GetConnectionString("LocalRedis"))
);
builder.Services.AddTransient<IAdminService, AdminService>();

builder.Services.AddScoped<IQuizAnswerRepository, QuizAnswerRepository>();
builder.Services.AddScoped<IQuizAnswerService, QuizAnswerService>();
var app = builder.Build();

app.UseIpRateLimiting();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "Support APP API"); });
}

if (app.Environment.IsStaging() || app.Environment.IsProduction())
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions()
    {
        Authorization = new[]
        {
            new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
            {
                RequireSsl = false,
                SslRedirect = false,
                LoginCaseSensitive = true,
                Users = new[]
                {
                    new BasicAuthAuthorizationUser
                    {
                        Login = "hangfire_admin",
                        PasswordClear = "@CursusOJT123"
                    }
                }
            })
        }
    });
    app.MapHangfireDashboard();
    RecurringJob.AddOrUpdate<IDataService>(service => service.FindAndUpdateExpiredStatusOrders(), "*/30 * * * *");
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();