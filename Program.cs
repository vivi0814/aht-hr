using HR_WorkFlow.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
    options.Conventions.Add(new ModulePageRouteModelConvention()) //註冊自訂路由規則
    
);
//有新增Service 要在這裡註冊
builder.Services.AddScoped<SqlDatabase>();      
builder.Services.AddScoped<MenuBase>();
builder.Services.AddScoped<Common>();
builder.Services.AddScoped<Excel>();
builder.Services.AddDistributedMemoryCache(); // 使用記憶體快取來儲存 Session 資料
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
    });

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();



app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
 {
     endpoints.MapRazorPages();
 });

app.MapRazorPages();

app.Run();


class ModulePageRouteModelConvention : IPageRouteModelConvention
{
    public void Apply(PageRouteModel model)
    {
        var moduleFolderName = "Module/";
        var viewEnginePath = model.ViewEnginePath;
        
        var oldSelectors = model.Selectors.ToList();
        model.Selectors.Clear();
        foreach (var selector in oldSelectors)
        {
            selector.AttributeRouteModel.Template = selector.AttributeRouteModel.Template.Replace(moduleFolderName, string.Empty);
            model.Selectors.Add(selector);
        }
    }
}