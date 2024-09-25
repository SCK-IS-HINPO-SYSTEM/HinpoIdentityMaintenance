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
//appsetting.jsonからDB接続文字列を取得
var connectionString = builder.Configuration.GetConnectionString("HinpoIdentity") ??
    throw new InvalidOperationException("Connection string 'HinpoIdentity' not found.");

//Identity用のDB接続文字列をセットする
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
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); //ロックアウトが発生した場合にユーザーがロックアウトされる時間。
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<ApplicationDbContext>()
    .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider);

builder.Services.ConfigureApplicationCookie(options => {
    options.AccessDeniedPath = "/Login/AccessDenied";
    options.Cookie.Name = ".HinpoIdentity.Cookie";
    options.Cookie.Path = "/";          //ここを/ukeireV3とか/SSO1にすると共有どころかログインすら出来ない。変えてはいけないものらしい。
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.LoginPath = "/Home/Dummy";// "Controllerを使用しないのでPageModel対応に変更 by fukuda
    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    options.SlidingExpiration = true;
});

//【認可ポリシー設定】
builder.Services.AddAuthorization(options => {   // 以下は該当WFで使用される画面の必要な権限をすべて追加する必要がある
    // 検査実績登録WF 
    options.AddPolicy("InspecResultOnly", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Confirm") ||
            context.User.IsInRole("All_Viewing")
        )
    );
});
// セッションを使う
builder.Services.AddSession(options => {
    // セッションクッキーの名前を変えるなら
    options.Cookie.Name = "session";
    options.Cookie.Path = "/";          //ここを/ukeireV3とか/SSO1にすると共有どころかログインすら出来ない。変えてはいけないものらしい。
    options.IdleTimeout = TimeSpan.FromDays(30);
    options.Cookie.HttpOnly = true;  // デフォルトで true
    // デフォルトは false。true に設定しないとセッション状態は
    // 機能しない可能性あり
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
        //メインメニューをルートのデフォルトとして追加
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


#region  検証メッセージローカライズで使用
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

builder.Host.UseNLog(); //NLog：依存性注入のための NLog のセットアップ

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
} else {
    app.UseExceptionHandler("/Shared/Error");
    app.UseHsts();
}

//MiddleWareで言語切り替えするために、設定の場所を移動 by fukuda 20240314
var supportedCultures = new[]
{
    new CultureInfo("ja"),
    new CultureInfo("en"),
};

// 標準の言語切り替え機能を有効にします。対応しているのは「クエリ文字列」「Cookie」「Accept-Language HTTP ヘッダー」です。
app.UseRequestLocalization(new RequestLocalizationOptions {
    DefaultRequestCulture = new RequestCulture("ja"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<HinpoIdentityMaintenanceAuthorizationMiddleware>(); // 認可ポリシーのチェック処理 UseRouting→UseAuthenticationの直後じゃないと都合が悪いらしい
app.UseAuthorization();
app.UseSession();                           // Middlewareの中でSessionを使う場合は、UseSession()をUseMiddlewareの前に記述する必要があります。
app.UseMiddleware<HinpoIdentityMaintenanceMiddleware>();

#pragma warning disable ASP0014
app.UseEndpoints(endpoints => {
    endpoints.MapRazorPages();
});
#pragma warning restore ASP0014

app.Run();
