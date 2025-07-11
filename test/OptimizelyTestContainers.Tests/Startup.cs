using EPiServer.Cms.Shell;
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Commerce.Internal;
using EPiServer.Data.SchemaUpdates;
using EPiServer.Scheduler;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OptimizelyTestContainers.Tests;

public class Startup(IWebHostEnvironment webHostingEnvironment)
{
    public void ConfigureServices(IServiceCollection services)
    {
        if (webHostingEnvironment.IsDevelopment())
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(webHostingEnvironment.ContentRootPath, "App_Data"));

            services.Configure<SchedulerOptions>(options => options.Enabled = false);
        }

        services
            .AddCmsAspNetIdentity<ApplicationUser>()
            .AddCms()
            .AddAdminUserRegistration()
            .AddEmbeddedLocalization<Startup>();
        
        // Remove the schema updater from the container if we're not running Commerce
        /*if (!includeCommerce)
        {*/
            //services.RemoveImplementation<ISchemaUpdater, CommerceDatabaseSchemaUpdater>();
            //services.RemoveImplementation<SchemaUpdaterBase, CommerceDatabaseSchemaUpdater>();
        /*}*/

        // TODO: Runs all initializable modules even if commerce is not included!
        // Solve with custom IAssemblyScanner?
        /*
        if (!includeCommerce)
        {
            services.Replace(new ServiceDescriptor(typeof(IAssemblyScanner)))
            services.AddSingleton<IAssemblyScanner, ExcludeCommerceAssemblyScanner>();
        }
        */
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapContent();
        });
    }
}
