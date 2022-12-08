using Amazon;
using Amazon.SQS;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TwitterSourcer.Core;

namespace TwitterSourcer.RetrievalWorker;
public class SqsService
{
    private readonly string _queueUrl;

    private readonly IAmazonSQS _sqs;

    public SqsService(IOptions<SqsOptions> options)
    {
        _queueUrl = options.Value.Url;
        _sqs = new AmazonSQSClient(RegionEndpoint.EUCentral1);
    }

    public async Task StoreEventAsync(Tweet tweet)
    {
        var jsonBody = JsonSerializer.Serialize(tweet);

        //TODO: Would add more resiliency for retrying message sending. 
        await _sqs.SendMessageAsync(_queueUrl, jsonBody);
    }
}
