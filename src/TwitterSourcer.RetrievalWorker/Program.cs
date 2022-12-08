using Microsoft.Extensions.Configuration;
using TwitterSourcer.RetrievalWorker;
using TwitterSourcer.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Tweetinvi.Models;
using Tweetinvi;

internal class Program
{
    private static async Task Main(string[] args)
    {
        await Host.CreateDefaultBuilder()
            .UseSystemd()
            .ConfigureServices((context, services) =>
            {
                services.AddHostedService<TweetRetrievalWorker>();

                services.AddSingleton<SqsService>();

                services.Configure<SqsOptions>(
                    context.Configuration.GetRequiredSection("Sqs"));

                var twitterCredentials = context.Configuration
                                                    .GetRequiredSection("Twitter")
                                                    .Get<TwitterCredentialOptions>()
                                                    ?? throw new ArgumentNullException("twitterCredentials");

                services.AddSingleton<ITwitterClient, TwitterClient>((services) =>
                {
                    var appCredentials = new ConsumerOnlyCredentials(
                            twitterCredentials.Key,
                            twitterCredentials.Secret,
                            twitterCredentials.Token);

                    return new TwitterClient(appCredentials);
                });
            })
            .RunConsoleAsync();
    }
}