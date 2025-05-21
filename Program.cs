using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ForQab.Service;
using ForQab.DataAccess.Models;
using ForQab.Presentation.Filters;
using ForQab.Repository.Abstract;
using ForQab.Repository.Concrete;
using ForQab.Service.Abstract;
using ForQab.Service.Concrete;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MyDbContext>(options =>
 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AuthDbContext>(options =>
 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); 

builder.Services.AddScoped<IExpertService, ExpertService>();
builder.Services.AddScoped<IExpertRepository, ExpertRepository>();
builder.Services.AddScoped<ISubProfessionRepository, SubProfessionRepository>();
builder.Services.AddScoped<ISubProfessionService, SubProfessionService>();
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<ICommissionRepository, CommissionRepository>();
builder.Services.AddScoped<ICommissionService, CommissionService>();
builder.Services.AddScoped<IRepresentativeRepository, RepresentativeRepository>();
builder.Services.AddScoped<IRepresentativeService, RepresentativeService>();
builder.Services.AddScoped<IMonitorRepository, MonitorRepository>();
builder.Services.AddScoped<IMonitorService, MonitorService>();
builder.Services.AddScoped<IHeadMonitorRepository, HeadMonitorRepository>();
builder.Services.AddScoped<IHeadMonitorService, HeadMonitorService>();
builder.Services.AddScoped<IVolunteerRepository, VolunteerRepository>();
builder.Services.AddScoped<IVolunteerService, VolunteerService>();
builder.Services.AddScoped<IDistrictRepository, DistrictRepository>();
builder.Services.AddScoped<IDistrictService, DistrictService>();
builder.Services.AddScoped<IExamBuildingRepository, ExamBuildingRepository>();
builder.Services.AddScoped<IExamBuildingService, ExamBuildingService>();
builder.Services.AddScoped<IKonsRepository, KonsRepository>();
builder.Services.AddScoped<IKonsService, KonsService>();
builder.Services.AddScoped<INaturaService, NaturaService>();
builder.Services.AddScoped<INaturaRepository, NaturaRepository>();
builder.Services.AddScoped<IWorkerService, WorkerService>();
builder.Services.AddScoped<IWorkerRepository, WorkerRepository>();
builder.Services.AddScoped<IFederationRepository, FederationRepository>();
builder.Services.AddScoped<IFederationService, FederationService>();
builder.Services.AddScoped<ISectionRepository, SectionRepository>();
builder.Services.AddScoped<IExamMonitorRepository, ExamMonitorRepository>();
builder.Services.AddScoped<IExamExpertSubProfessionRepository, ExamExpertSubProfessionRepository>();
builder.Services.AddScoped<IMinistryRepresentativeService, MinistryRepresentativeService>();
builder.Services.AddScoped<IMinistryRepresentativeRepository, MinistryRepresentativeRepository>();
builder.Services.AddScoped<IBadgeExportService, BadgeExportService>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = false;
})
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

//builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<AuthDbContext>();

builder.Services.AddScoped(typeof(SectionValidationFilter<>));


// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        // Presentation/Views location-u əlavə edin
        options.ViewLocationFormats.Add("/Presentation/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/Presentation/Views/Shared/{0}.cshtml");
    });
builder.Services.AddRazorPages();


var app = builder.Build();

//var pathBase = app.Configuration["PathBase"];
//if (!string.IsNullOrEmpty(pathBase))
//{
//    app.UsePathBase(pathBase);
//}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage(); // hata detaylarını gösteren sayfa
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();