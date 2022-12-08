using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Tweetinvi.Models;
using Tweetinvi;
using Microsoft.Extensions.Options;
using TwitterSourcer.Core;
using Tweetinvi.Streaming.V2;
using System.Globalization;

namespace TwitterSourcer.RetrievalWorker;
internal class TweetRetrievalWorker : IHostedService
{
    private readonly SqsService _sqsService;
    private readonly ITwitterClient _twitterClient;
    private readonly IFilteredStreamV2 _stream;

    public TweetRetrievalWorker(SqsService sqsService, 
        ITwitterClient twitterClient)
    {
        _sqsService = sqsService;

        _twitterClient = twitterClient;

        _stream = _twitterClient.StreamsV2.CreateFilteredStream();

        //A fix for the twitter client exceptions.
        var culture = new CultureInfo("lt-LT");

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        //Would rate limit a bit this event sending if would be possible.
        _stream.TweetReceived += async (sender, args) =>
        {
            //Geo coordinates for the tweet are mostly empty so using user location
            //But location is mostly filled with custom info, would use something more identifying
            //if found
            //Additionally, would use a separate model to separate concerns (no DB attributes)
            var tweet = new Tweet
            {
                Id = args.Tweet.Id,
                AuthorId = args.Tweet.AuthorId,
                Content = args.Tweet.Text,
                Source = args.Tweet.Source,
                Location = args.Includes.Users
                                        .First(u => u.Id == args.Tweet.AuthorId)
                                        .Location
            };

            await _sqsService.StoreEventAsync(tweet);

            Console.WriteLine($"{tweet.Id} | ${tweet.Content} | ${tweet.Location}");
        };


        Console.WriteLine("Storing tweets:");

        _stream.StartAsync();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _stream.StopStream();

        return Task.CompletedTask;
    }
}
