using Elasticsearch.Net;
using Nest;

namespace LogReaderApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:3000")
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                    });
            });

            services.AddSingleton<EventLogReaderService>();
            services.AddSingleton<IElasticClient>(provider =>
            {
                var configuration = provider.GetService<IConfiguration>();
                var settings = new ConnectionSettings(new Uri("https://localhost:9200"))
    .BasicAuthentication("elastic", "hassan3459") // Replace with your actual password
    .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
    .DefaultIndex("windows-logs");
                return new ElasticClient(settings);
            });
            services.AddHostedService(provider => provider.GetService<EventLogReaderService>());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // ... standard configuration ...
            app.UseRouting();
            app.UseCors();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
