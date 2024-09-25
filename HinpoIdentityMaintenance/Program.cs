using CommonSendMail;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using HinpoCertBusinessLayer;
using HinpoIdentityBusinessLayer;
using HinpoIdentityMaintenance;
using HinpoIdentityMaintenance.Data;
using HinpoIdentityMaintenance.Models;
using HinpoMasterBusinessLayer;
using HinppoCertBusinessLayer;
using MailLogBusinessLayer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NLog.Web;
using System.Globalization;
using System.Reflection;
using WorkFlowBusinessLayer;
using LoggerService;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//appsetting.json����DB�ڑ���������擾
var connectionString = builder.Configuration.GetConnectionString("HinpoIdentity") ??
    throw new InvalidOperationException("Connection string 'HinpoIdentity' not found.");

//Identity�p��DB�ڑ���������Z�b�g����
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var ContentRootPath = Directory.GetCurrentDirectory();
ContentRootPath = @"C:\Inetpub\wwwrootDotNet\SharedKeys";

var keysFolder = Path.Combine(ContentRootPath, "Keys");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
    .SetApplicationName("SharedCookieApp");

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); //���b�N�A�E�g�����������ꍇ�Ƀ��[�U�[�����b�N�A�E�g����鎞�ԁB
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<ApplicationDbContext>()
    .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider);

builder.Services.ConfigureApplicationCookie(options => {
    options.AccessDeniedPath = "/Login/AccessDenied";
    options.Cookie.Name = ".HinpoIdentity.Cookie";
    options.Cookie.Path = "/";          //������/ukeireV3�Ƃ�/SSO1�ɂ���Ƌ��L�ǂ��납���O�C������o���Ȃ��B�ς��Ă͂����Ȃ����̂炵���B
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.LoginPath = "/Home/Dummy";// "Controller���g�p���Ȃ��̂�PageModel�Ή��ɕύX by fukuda
    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    options.SlidingExpiration = true;
});

//�y�F�|���V�[�ݒ�z
builder.Services.AddAuthorization(options => {   // �ȉ��͊Y��WF�Ŏg�p������ʂ̕K�v�Ȍ��������ׂĒǉ�����K�v������
    // �������ѓo�^WF 
    options.AddPolicy("InspecResultOnly", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Confirm") ||
            context.User.IsInRole("All_Viewing")
        )
    );
});
// �Z�b�V�������g��
builder.Services.AddSession(options => {
    // �Z�b�V�����N�b�L�[�̖��O��ς���Ȃ�
    options.Cookie.Name = "session";
    options.Cookie.Path = "/";          //������/ukeireV3�Ƃ�/SSO1�ɂ���Ƌ��L�ǂ��납���O�C������o���Ȃ��B�ς��Ă͂����Ȃ����̂炵���B
    options.IdleTimeout = TimeSpan.FromDays(30);
    options.Cookie.HttpOnly = true;  // �f�t�H���g�� true
    // �f�t�H���g�� false�Btrue �ɐݒ肵�Ȃ��ƃZ�b�V������Ԃ�
    // �@�\���Ȃ��\������
    options.Cookie.IsEssential = true;
});

builder.Services.AddRazorPages()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix, opts => { opts.ResourcesPath = "Resources"; })
    .AddDataAnnotationsLocalization(o => {
        o.DataAnnotationLocalizerProvider = (type, factory) => {
            return factory.Create(typeof(SharedResource));
        };
    })
    .AddRazorPagesOptions(o => {
        //���C�����j���[�����[�g�̃f�t�H���g�Ƃ��Ēǉ�
        //o.Conventions.AddPageRoute("/AspNetUserRolesMnt/Index", "");
        o.Conventions.AddPageRoute("/AspNetUserSearch/Index", "");
    });

builder.Services.AddMvc()
    .AddSessionStateTempDataProvider();
builder.Services.AddScoped<IHinpoMasterServiceReadOnly, HinpoMasterServiceReadOnly>();
builder.Services.AddScoped<IHinpoIdentityService, HinpoIdentityServiceReadAndWrite>();
builder.Services.AddScoped<HinpoIdentityMaintenanceMiddleware>();
builder.Services.Configure<AppSettingsModel>(
    builder.Configuration.GetSection("NetWorkInfo"));
//builder.Services.AddScoped<IWorkFlowService, WorkFlowServiceReadAndWrite>();
builder.Services.AddSingleton<ILoggerManager, LoggerManager>();
builder.Services.AddScoped<IWorkFlowService, WorkFlowServiceReadAndWrite>();


#region  ���؃��b�Z�[�W���[�J���C�Y�Ŏg�p
#pragma warning disable ASP0000
IServiceProvider _serviceProvider = builder.Services.BuildServiceProvider();
#pragma warning restore ASP0000

#pragma warning disable CS8602,CS8604
IStringLocalizer Localizer = _serviceProvider.GetService<IStringLocalizerFactory>()
                   .Create(nameof(SharedResource), new AssemblyName(typeof(SharedResource).Assembly.FullName).Name);
#pragma warning restore CS8602,CS8604
#endregion

builder.Services.AddControllersWithViews(
    options => {
        var authFilter = new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
        options.Filters.Add(authFilter);
    }
);

builder.Host.UseNLog(); //NLog�F�ˑ��������̂��߂� NLog �̃Z�b�g�A�b�v

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
} else {
    app.UseExceptionHandler("/Shared/Error");
    app.UseHsts();
}

//MiddleWare�Ō���؂�ւ����邽�߂ɁA�ݒ�̏ꏊ���ړ� by fukuda 20240314
var supportedCultures = new[]
{
    new CultureInfo("ja"),
    new CultureInfo("en"),
};

// �W���̌���؂�ւ��@�\��L���ɂ��܂��B�Ή����Ă���̂́u�N�G��������v�uCookie�v�uAccept-Language HTTP �w�b�_�[�v�ł��B
app.UseRequestLocalization(new RequestLocalizationOptions {
    DefaultRequestCulture = new RequestCulture("ja"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<HinpoIdentityMaintenanceAuthorizationMiddleware>(); // �F�|���V�[�̃`�F�b�N���� UseRouting��UseAuthentication�̒��ザ��Ȃ��Ɠs���������炵��
app.UseAuthorization();
app.UseSession();                           // Middleware�̒���Session���g���ꍇ�́AUseSession()��UseMiddleware�̑O�ɋL�q����K�v������܂��B
app.UseMiddleware<HinpoIdentityMaintenanceMiddleware>();

#pragma warning disable ASP0014
app.UseEndpoints(endpoints => {
    endpoints.MapRazorPages();
});
#pragma warning restore ASP0014

app.Run();
