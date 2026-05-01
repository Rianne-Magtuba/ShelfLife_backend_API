using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Data_Layer.Configuration;


namespace Data_Layer.Extensions
{
    public static class DependencyInjectionExtensions
    {
        // Matches the secret mount path configured in deploy.yml
        private const string CloudRunSecretPath = "/secrets/firebase-sa";

        public static IServiceCollection AddFirestoreDatabase(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FirestoreOptions>(
                configuration.GetSection(FirestoreOptions.SectionName));

            services.AddSingleton<FirestoreDb>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<FirestoreOptions>>().Value;

                var firestoreBuilder = new FirestoreDbBuilder
                {
                    ProjectId = options.ProjectId
                };

                if (File.Exists(CloudRunSecretPath))
                {
                    // CLOUD RUN: reads the firebase-service-account secret
                    // mounted by Secret Manager at /secrets/firebase-sa
                    firestoreBuilder.Credential = GoogleCredential
                        .FromFile(CloudRunSecretPath)
                        .CreateScoped("https://www.googleapis.com/auth/datastore");
                }
                else if (!string.IsNullOrEmpty(options.CredentialsPath)
                         && File.Exists(options.CredentialsPath))
                {
                    // LOCAL DEV: reads your D:\Data\Downloads\... path
                    // from appsettings.Development.json — completely unchanged
                    firestoreBuilder.CredentialsPath = options.CredentialsPath;
                }
                // Fallback: FirestoreDbBuilder uses Application Default Credentials

                return firestoreBuilder.Build();
            });

            return services;
        }
    }
}