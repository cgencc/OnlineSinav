using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnlineSinav.API.Data;
using OnlineSinav.API.Models;
using OnlineSinav.API.Repositories;
using OnlineSinav.API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, AppRole>(options => {
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ITokenManager, TokenManager>();

var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
    };
});

builder.Services.AddCors(opt => opt.AddPolicy("AllowAll",
    b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Online Sýnav API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Lütfen Token deðerini buraya yapýþtýrýn. Örnek: eyJhbGci...",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[] { } }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll"); 
app.UseAuthentication(); 
app.UseAuthorization();  

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

    try
    {
        if (!await roleMgr.RoleExistsAsync("Teacher")) await roleMgr.CreateAsync(new AppRole { Name = "Teacher" });
        if (!await roleMgr.RoleExistsAsync("Student")) await roleMgr.CreateAsync(new AppRole { Name = "Student" });

        var teacherEmail = "hoca@onlinesinav.com";
        if (await userMgr.FindByEmailAsync(teacherEmail) == null)
        {
            var teacher = new AppUser { UserName = "sinavhocasi", Email = teacherEmail, FullName = "Sýnav Yöneticisi" };
            await userMgr.CreateAsync(teacher, "Sinav123!");
            await userMgr.AddToRoleAsync(teacher, "Teacher");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Tohumlama Hatasý: " + ex.Message);
    }
}

app.Run();