using Tweetinvi;
using Tweetinvi.Models;
using TwitterSourcer.Api.EC2;
using TwitterSourcer.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

var twitterCredentials = builder.Configuration
                                .GetSection("Twitter")
                                .Get<TwitterCredentialOptions>()
                                ?? throw new ArgumentNullException("twitterCredentials");

builder.Services.AddScoped<ITwitterClient, TwitterClient>((services) =>
{
    var appCredentials = new ConsumerOnlyCredentials(
            twitterCredentials.Key,
            twitterCredentials.Secret,
            twitterCredentials.Token);

    return new TwitterClient(appCredentials);
});

builder.Services.Configure<Ec2Options>(builder.Configuration.GetSection("Ec2"));

builder.Services.AddScoped<Ec2Manager>();

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
