using Microsoft.EntityFrameworkCore;
using FictionalUniversityWebApp.Data;
using TestWebApplication.DAL;
using FictionalUniversityWebApp.DAL.Interfaces;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<EducationDBContext>(options =>
	options.UseSqlServer(builder.Configuration
	.GetConnectionString("EducationDb") ?? throw new InvalidOperationException("Connection string 'EducationDb' not found.")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Course}/{action=Index}/{id?}");

app.Run();
