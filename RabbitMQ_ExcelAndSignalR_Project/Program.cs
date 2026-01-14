using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ_ExcelAndSignalR_Project.Hubs;
using RabbitMQ_ExcelAndSignalR_Project.Models;
using RabbitMQ_ExcelAndSignalR_Project.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

//  SignalR
builder.Services.AddSignalR();


// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SqlServer")
    )
);

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

//rabbitmq
builder.Services.AddSingleton(sp => new ConnectionFactory
{
    Uri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ")),
    
});
builder.Services.AddSingleton<RabbitMQPublisher>();
builder.Services.AddSingleton<RabbitMQClientService>();



var app = builder.Build();


// ?? MIGRATION + SEED
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Migration uygula
    context.Database.Migrate();

    // Seed user
    if (!context.Users.Any())
    {
        await userManager.CreateAsync(
            new IdentityUser
            {
                UserName = "deneme",
                Email = "deneme@outlook.com"
            },
            "Password12*"
        );

        await userManager.CreateAsync(
            new IdentityUser
            {
                UserName = "deneme2",
                Email = "deneme2@outlook.com"
            },
            "Password12*"
        );
    }
}


// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

//  SignalR Hub
app.MapHub<MyHub>("/MyHub");


// ?? BAÞLANGIÇ CONTROLLER'I ACCOUNT YAPTIK
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
