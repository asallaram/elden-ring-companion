using EldenRingSim.DB;
using EldenRingSim.CSVParsing;
using EldenRingSim.Repositories;
using EldenRingSim.Services;
using EldenRingSim.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Warning); 

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();

builder.Services.AddDbContext<EldenRingContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("EldenRingDatabase")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<EldenRingContext>()
.AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(
        builder.Configuration.GetConnectionString("Redis")!, true);
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

// ============= REPOSITORIES =============
builder.Services.AddScoped<IWeaponRepository, WeaponRepository>();
builder.Services.AddScoped<IBossRepository, BossRepository>();
builder.Services.AddScoped<IBossStatsRepository, BossStatsRepository>(); 
builder.Services.AddScoped<IRepository<Talismans>, Repository<Talismans>>();
builder.Services.AddScoped<IRepository<Items>, Repository<Items>>();
builder.Services.AddScoped<IRepository<NPCs>, Repository<NPCs>>();
builder.Services.AddScoped<IRepository<Classes>, Repository<Classes>>();
builder.Services.AddScoped<IRepository<Spirits>, Repository<Spirits>>();
builder.Services.AddScoped<IRepository<Creatures>, Repository<Creatures>>();
builder.Services.AddScoped<IRepository<AshOfWar>, Repository<AshOfWar>>();
builder.Services.AddScoped<IRepository<Ammo>, Repository<Ammo>>();
builder.Services.AddScoped<IRepository<Locations>, Repository<Locations>>();
builder.Services.AddScoped<IPlayerProgressRepository, PlayerProgressRepository>(); 
builder.Services.AddScoped<IBossFightRepository, BossFightRepository>(); 

// Analysis Engine
builder.Services.AddScoped<IAnalysisEngine, AnalysisEngine>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend"); 
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "Elden Ring Simulator is running!");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EldenRingContext>();
    var basePath = "elden-ring-data";

    var weaponCount = db.Weapons.Count();
    
    if (weaponCount > 0)
    {
        Console.WriteLine($"📊 Database already populated with {weaponCount} weapons.");
        Console.WriteLine("💡 Skipping data load. To reload, drop database first.");
    }
    else
    {
        Console.WriteLine("📥 Loading CSV data...\n");
        
        var successCount = 0;
        var failureCount = 0;
        var totalItems = 0;

        void LoadCsv<TParser, TEntity>(string fileName)
            where TParser : CsvParserBase<TEntity>
            where TEntity : GameEntity, new()
        {
            try
            {
                var entityName = typeof(TEntity).Name;
                var filePath = $"{basePath}/{fileName}";
                var parser = Activator.CreateInstance(typeof(TParser), filePath) as CsvParserBase<TEntity>;
                var list = parser?.Parse();
                
                if (list != null && list.Count > 0)
                {
                    var originalCount = list.Count;
                    list = list
                        .GroupBy(item => item.Name)
                        .Select(group => group.First())
                        .ToList();
                    
                    if (typeof(TEntity) == typeof(Spirits))
                    {
                        foreach (var item in list)
                        {
                            item.Id = Guid.NewGuid().ToString();
                        }
                    }

                    db.Set<TEntity>().AddRange(list);
                    db.SaveChanges();
                    
                    Console.WriteLine($"✅ {entityName,-15} → {list.Count,4} items loaded");
                    successCount++;
                    totalItems += list.Count;
                }
            }
            catch (Exception ex)
            {
                var entityName = typeof(TEntity).Name;
                Console.WriteLine($"❌ {entityName,-15} → FAILED: {ex.Message}");
                failureCount++;
            }
        }

        LoadCsv<AmmoCsvParser, Ammo>("ammos.csv");
        LoadCsv<ArmorCsvParser, Armor>("armors.csv");
        LoadCsv<AshOfWarCsvParser, AshOfWar>("ashes.csv");
        LoadCsv<BossesCsvParser, Bosses>("bosses.csv");
        LoadCsv<BossStatsCsvParser, BossStats>("boss-stats.csv");
        LoadCsv<ClassesCsvParser, Classes>("classes.csv");
        LoadCsv<CreaturesCsvParser, Creatures>("creatures.csv");
        LoadCsv<IncantationsCsvParser, Incantations>("incantations.csv");
        LoadCsv<ItemsCsvParser, Items>("items.csv");
        LoadCsv<LocationsCsvParser, Locations>("locations.csv");
        LoadCsv<NPCsCsvParser, NPCs>("npcs.csv");
        LoadCsv<ShieldsCsvParser, Shields>("shields.csv");
        LoadCsv<SorceriesCsvParser, Sorceries>("sorceries.csv");
        LoadCsv<SpiritsCsvParser, Spirits>("spirits.csv");
        LoadCsv<TalismansCsvParser, Talismans>("talismans.csv");
        LoadCsv<WeaponsCsvParser, Weapons>("weapons.csv");
        
        Console.WriteLine($"\n🎉 Data loading complete!");
        Console.WriteLine($"   ✅ Success: {successCount} CSVs");
        Console.WriteLine($"   📦 Total: {totalItems} items");
        if (failureCount > 0)
            Console.WriteLine($"   ❌ Failed: {failureCount} CSVs");
    }
}

Console.WriteLine($"\n🚀 Server running on http://localhost:5019");
app.Run();