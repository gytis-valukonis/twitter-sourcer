using Amazon.DynamoDBv2.DataModel;

namespace TwitterSourcer.Core;

[DynamoDBTable("TwitterSourcer_Tweets")]
public class Tweet
{
    [DynamoDBHashKey]
    public string Date { get; set; } = default!;

    [DynamoDBRangeKey]
    public DateTime CreatedAt { get; set; } = default!;

    public string Id { get; set; } = default!;

    public string AuthorId { get; set; } = default!;
    public string Location { get; set; } = default!;
    public string Source { get; set; } = default!;

    public string Content { get; set; } = default!;

}
