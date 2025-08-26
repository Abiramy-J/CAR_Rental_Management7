using CAR.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

//namespace CAR
//{
//    public void ConfigureServices(IServiceCollection services)
//    {
//        services.AddControllersWithViews();
//        services.AddDbContext<ApplicationDbContext>(options =>
//            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

//        services.AddSession();
//    }

//    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//    {
//        if (env.IsDevelopment())
//        {
//            app.UseDeveloperExceptionPage();
//        }

//        app.UseStaticFiles();
//        app.UseRouting();
//        app.UseSession();
//        app.UseAuthorization();

//        app.UseEndpoints(endpoints =>
//        {
//            endpoints.MapControllerRoute(
//                name: "default",
//                pattern: "{controller=Account}/{action=Login}/{id?}");
//        });
//    }

//}
