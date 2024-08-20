namespace PlayFab.AzureFunctions
{
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public class IoCContainer
    {
        private static IServiceProvider _provider;

        public static IServiceProvider Create()
        {
            return _provider ?? (_provider = ConfigureServices());
        }

        private static IServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            var settings = new Settings(s => Environment.GetEnvironmentVariable(s, EnvironmentVariableTarget.Process));
            services.AddSingleton<ISettings>(settings);
            services.AddTransient<IAzureTableRepository>(s => new AzureTableRepository(settings.CosmosDbConnectionString));
            //services.AddScoped<IParser<RawPlayer>, EmployeeDataParser>();
            //services.AddScoped<IMapper<RawPlayer, Player>, RawEmployeeToEmployeeObjectMapper>();
            return services.BuildServiceProvider();
        }
    }
}
