namespace Optimizely.TestContainers;

public class Program
{
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    // Split Startup into two classes StartupWithCmsAndCommerce and StartupWithCms to be able to test both in isolation
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureCmsDefaults()
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<StartupWithCmsAndCommerce>());
}
