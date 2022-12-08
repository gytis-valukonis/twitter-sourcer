using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System.Text.Json;
using TwitterSourcer.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TwitterSourcer.Transform;

public class Function
{
    private readonly TweetRepository _repository = new ();
    private readonly AmazonCloudWatchClient _cloudwatch = new AmazonCloudWatchClient(RegionEndpoint.EUCentral1);

    public Function() {}

    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach(var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        var tweet = JsonSerializer.Deserialize<Tweet>(message.Body);

        if (tweet == null)
        {
            return;
        }

        context.Logger.LogInformation($"Incoming tweet from Location: {tweet.Location}");

        try
        {
            await _repository.StoreAsync(tweet);

            await AddMetricsAsync(tweet);

            context.Logger.LogInformation($"Processed message of Id: {message.MessageId}");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"${ex.Message} | Stacktrace: ${ex.Message}");
        }
        
        await Task.CompletedTask;
    }

    private async Task AddMetricsAsync(Tweet tweet)
    {
        var utcNow = DateTime.UtcNow;

        await _cloudwatch.PutMetricDataAsync(new PutMetricDataRequest
        {
            Namespace = "Tweets",
            MetricData = new List<MetricDatum>
                {
                    new MetricDatum
                    {
                        Unit = StandardUnit.Count,
                        Value = 1,
                        MetricName = "Tweets",
                        TimestampUtc = utcNow,
                        Dimensions = new List<Dimension>
                        {
                            new Dimension
                            {
                                Name = "Count",
                                Value = "With-in-one-hour"
                            }
                        }
                    },
                    new MetricDatum
                    {
                        Unit = StandardUnit.Count,
                        Value = tweet.Content.Length,
                        MetricName = "Tweet Character Count",
                        TimestampUtc = utcNow,
                        Dimensions = new List<Dimension>
                        {
                            new Dimension
                            {
                                Name = "Count",
                                Value = "With-in-one-hour"
                            }
                        }
                    }
                }
        });
    }
}