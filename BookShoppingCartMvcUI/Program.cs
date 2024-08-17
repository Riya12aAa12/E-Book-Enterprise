using BookShoppingCartMvcUI;
using BookShoppingCartMvcUI.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Identity services
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true; // Requires email confirmation for sign-in
        options.Password.RequireDigit = true; // Requires at least one digit in password
        options.Password.RequiredLength = 6; // Minimum length for password
        options.Password.RequireLowercase = true; // Requires at least one lowercase letter in password
        options.Password.RequireNonAlphanumeric = false; // Does not require non-alphanumeric characters
        options.Password.RequireUppercase = true; // Requires at least one uppercase letter in password
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI() // Adds default UI for Identity
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IHomeRepository, HomeRepository>();
builder.Services.AddTransient<ICartRepository, CartRepository>();
builder.Services.AddTransient<IUserOrderRepository, UserOrderRepository>();
builder.Services.AddTransient<IStockRepository, StockRepository>();
builder.Services.AddTransient<IGenreRepository, GenreRepository>();
builder.Services.AddTransient<IFileService, FileService>();
builder.Services.AddTransient<IBookRepository, BookRepository>();
builder.Services.AddTransient<IReportRepository, ReportRepository>();

var app = builder.Build();

// Method to create roles
static async Task CreateRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roleNames = { "USER", "ADMIN" };
    IdentityResult roleResult;

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            // Create the role if it does not exist
            roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

// Ensure roles are created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await CreateRoles(services);
}

// Uncomment it when you run the project first time, It will register an admin
// using (var scope = app.Services.CreateScope())
// {
//     await DbSeeder.SeedDefaultData(scope.ServiceProvider);
// }

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Ensure Authentication is added
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Ensure Razor Pages are mapped

app.Run();
