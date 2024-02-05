using MasterBurger.Services;
using MasterBurger.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;
using MasterBurger.Repositories.Interfaces;
using MasterBurger.Repositories;
using MasterBurger.Components;
using MasterBurger.Models;
using System;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();


//Configuracao da Password
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
  options.Password.RequireDigit = false;
  options.Password.RequireLowercase = false;
  options.Password.RequireNonAlphanumeric = false;
  options.Password.RequireUppercase = false;
  options.Password.RequiredLength = 3;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

var configuration = builder.Configuration;
builder.Services.AddAuthentication().AddGoogle(googleOptions => {
  googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
  googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
});


builder.Services.Configure<IdentityOptions>(options => {
  options.SignIn.RequireConfirmedEmail = true;
});

builder.Services.AddPaging(options => {
  options.ViewName = "Bootstrap5";
  options.PageParameterName = "pageindex";
	options.HtmlIndicatorDown = " <span>&darr;</span>";
	options.HtmlIndicatorUp = " <span>&uarr;</span>";
});


//Servicos
builder.Services.AddScoped<BannerPrincipal>();
builder.Services.AddTransient<IRelatoriosRepository, RelatoriosRepository>();
builder.Services.AddTransient<IProdutoRepository, ProdutoRepository>();
builder.Services.AddTransient<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddTransient<IEncomendaRepository, EncomendaRepository>();
builder.Services.AddTransient<ICupomRepository, CupomRepository>();
builder.Services.AddTransient<IBannerRepository, BannerRepository>();
builder.Services.AddTransient<RoleInitializationService>();
builder.Services.AddScoped<UtilizadorService>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

//Carrinho
builder.Services.AddScoped(sp => CarrinhoCompra.GetCarrinho(sp));


builder.Services.AddMemoryCache();
builder.Services.AddSession();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configurar o pipeline de solicitação HTTP.
if (app.Environment.IsDevelopment()) {
  app.UseMigrationsEndPoint();
} else {
  app.UseExceptionHandler("/Home/Error");
  app.UseStatusCodePagesWithReExecute("/Error/{0}"); // UseStatusCodePagesWithReExecute em vez de UseStatusCodePagesWithRedirects

  app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.UseEndpoints(endpoints => {
  endpoints.MapControllerRoute(
    name: "produtoNovidades",
    pattern: "produto/novidades",
    defaults: new { controller = "Produto", action = "Novidades" });

  endpoints.MapControllerRoute(
      name: "produtoCategoria",
      pattern: "produto/list",
      defaults: new { controller = "Produto", action = "List" });

  endpoints.MapControllerRoute(
    name: "produtoCategoria",
    pattern: "produto/listcat/{categoria?}",
    defaults: new { controller = "Produto", action = "ListCat" });

  endpoints.MapControllerRoute(
    name: "CustomerEncomendas",
    pattern: "customerencomendas/detail/{id}",
    defaults: new { controller = "CustomerEncomendas", action = "Detail" });

  endpoints.MapControllerRoute(
    name: "CustomerIndex",
    pattern: "customerdados",
    defaults: new { controller = "CustomerDados", action = "Index" });

  endpoints.MapControllerRoute(
    name: "DadosRegistar",
    pattern: "customerdados/dadosregistar",
    defaults: new { controller = "CustomerDados", action = "DadosRegistar" });

  endpoints.MapControllerRoute(
    name: "ResultadoPesquisa",
    pattern: "Identity/Produto/ResultadoPesquisa",
    defaults: new { controller = "Produto", action = "ResultadoPesquisa" });

  endpoints.MapControllerRoute(
    name: "Carrinho",
    pattern: "identity/customerdashboard",
    defaults: new { controller = "CustomerDashboard", action = "Index" });

  endpoints.MapControllerRoute(
    name: "Carrinho",
    pattern: "identity/carrinhocompra",
    defaults: new { controller = "CarrinhoCompra", action = "Index" });

  endpoints.MapControllerRoute(
    name: "Privacidade",
    pattern: "/politica-de-privacidade",
    defaults: new { controller = "Home", action = "Privacidade" });

  endpoints.MapControllerRoute(
    name: "Sobre",
    pattern: "/sobre",
    defaults: new { controller = "Home", action = "Sobre" });

  endpoints.MapControllerRoute(
    name: "termos-condicoes-avaliacoes",
    pattern: "/termos-condicoes-avaliacoes",
    defaults: new { controller = "Home", action = "CondicoesAvaliacoes" });


	endpoints.MapControllerRoute(
		name: "AdminEncomendasAction",
		pattern: "Admin/AdminEncomendas/{action}/{id?}",
		defaults: new { area = "Admin", controller = "AdminEncomendas" });

	endpoints.MapControllerRoute(
     name: "AdminEncomendas",
     pattern: "Admin/AdminEncomendas/Details/{id}",
     defaults: new { area = "Admin", controller = "AdminEncomendas", action = "Details" });


  endpoints.MapControllerRoute(
   name: "AdminProdutos",
   pattern: "Admin/AdminProdutos",
   defaults: new { area = "Admin", controller = "AdminProdutos", action = "Index" });

  endpoints.MapControllerRoute(
   name: "AdminCategorias",
   pattern: "Admin/AdminCategorias",
   defaults: new { area = "Admin", controller = "AdminCategorias", action = "Index" });

  endpoints.MapControllerRoute(
    name: "AdminReviews",
    pattern: "Admin/AdminReviews",
    defaults: new { area = "Admin", controller = "AdminReviews", action = "Index" });

  endpoints.MapControllerRoute(
    name: "AdminBanners",
    pattern: "Admin/AdminBanners",
    defaults: new { area = "Admin", controller = "AdminBanners", action = "Index" });

  endpoints.MapControllerRoute(
    name: "AdminUtilizadores",
    pattern: "Admin/AdminUtilizadores",
    defaults: new { area = "Admin", controller = "AdminUtilizadores", action = "Index" });

  endpoints.MapControllerRoute(
     name: "AdminRelatorios",
     pattern: "Admin/AdminRelatorios/{action=Index}",
     defaults: new { area = "Admin", controller = "AdminRelatorios", action = "Index" });


  endpoints.MapAreaControllerRoute(
    name: "Identity",
    areaName: "Identity",
    pattern: "Identity/{controller=Account}/{action=Login}/{id?}");


  endpoints.MapControllerRoute(
    name: "Admin",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

  endpoints.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
});


// Configuração da página de erro 404
app.Use(async (context, next) => {
  if (context.Response.StatusCode == 404) {
    context.Request.Path = "/Error/NotFound";
    await next();
  }
});


// Iniciar os perfis
using (var scope = app.Services.CreateScope()) {
  var services = scope.ServiceProvider;

  try {
    var roleInitializationService = services.GetRequiredService<RoleInitializationService>();
    await roleInitializationService.InitializeRoles();
  } catch (Exception ex) {
    Console.WriteLine($"Ocorreu um erro durante a inicialização de funções. Detalhes: {ex.Message}");
  }
}
app.Run();
