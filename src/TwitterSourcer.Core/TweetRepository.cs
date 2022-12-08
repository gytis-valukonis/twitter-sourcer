using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace TwitterSourcer.Core;
public class TweetRepository
{
    private readonly IAmazonDynamoDB _client;
    private readonly DynamoDBContext _context;

    public TweetRepository()
    {
        _client = new AmazonDynamoDBClient(RegionEndpoint.EUCentral1);
        _context = new DynamoDBContext(_client);
    }

    public async Task StoreAsync(Tweet tweet)
    {
        var utcNow = DateTime.UtcNow;

        var currentUtcDate = GetStringDateRepresentation(utcNow);

        tweet.Date = currentUtcDate;
        tweet.CreatedAt = utcNow;

        await _context.SaveAsync(tweet);
    }

    public async Task<List<Tweet>> GetTweetsAsync(DateTime date)
    {
        var partitionKey = GetStringDateRepresentation(date);

        var query = _context.QueryAsync<Tweet>(partitionKey);

        var tweets = await query.GetRemainingAsync();

        return tweets;
    }

    private static string GetStringDateRepresentation(DateTime dateTime)
    {
        return dateTime.ToString("yyyyMMdd");
    }
}
