using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Data_Layer.Configuration;

namespace Data_Layer.Extensions
{
    public static class DependencyInjectionExtensions
    {

        public static IServiceCollection AddFirestoreDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind the configuration section to our Options class
            services.Configure<FirestoreOptions>(configuration.GetSection(FirestoreOptions.SectionName));
            
            // Register FirestoreDb as a Singleton
            services.AddSingleton<FirestoreDb>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<FirestoreOptions>>().Value;

                var builder = new FirestoreDbBuilder
                {
                    ProjectId = options.ProjectId,
                    CredentialsPath = options.CredentialsPath
                };

                return builder.Build();
            });

            return services;
        }
    }
}
