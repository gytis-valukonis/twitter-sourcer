using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Microsoft.Extensions.Options;
using Tweetinvi;

namespace TwitterSourcer.Api.EC2;

public class Ec2Manager
{
    private readonly ITwitterClient _twitterClient;
    private readonly IAmazonEC2 _ec2Client;
    private readonly string _instanceId;

    private readonly ILogger<Ec2Manager> _logger;

    public Ec2Manager(ITwitterClient twitterClient, IOptions<Ec2Options> options, ILogger<Ec2Manager> logger)
	{
        _twitterClient = twitterClient;
        _ec2Client = new AmazonEC2Client(RegionEndpoint.EUCentral1);
        _instanceId = options.Value.InstanceId;
        _logger = logger;
    }

    public async Task ManageInstanceAsync()
    {
        var shouldInstanceBeRunning = await ShouldWorkerBeRunningAsync();

        var describeInstancesRequest = new DescribeInstancesRequest
        {
            InstanceIds = new List<string> { _instanceId }
        };

        var response = await _ec2Client.DescribeInstancesAsync(describeInstancesRequest);

        //This is a simple check, but would need to check more states and probably hold until the
        //state is "correct" and only then stop it or launch it.
        var isInstanceRunning = response.Reservations[0].Instances[0].State.Code == 16;

        if (shouldInstanceBeRunning && !isInstanceRunning)
        {
            await StartInstanceAsync();
        }
        else if (!shouldInstanceBeRunning && isInstanceRunning)
        {
            await StopInstanceAsync();
        }
    }

    private async Task StartInstanceAsync()
    {
        try
        {
            var request = new StartInstancesRequest { InstanceIds = new List<string> { _instanceId } };

            await _ec2Client.StartInstancesAsync(request);

            _logger.LogInformation("EC2 Instance started");
        }
        catch(Exception ex) 
        {
            _logger.LogError(ex, "Failure in starting instance");
        }
    }

    private async Task StopInstanceAsync()
    {
        try
        {
            var request = new StopInstancesRequest { InstanceIds = new List<string> { _instanceId } };

            await _ec2Client.StopInstancesAsync(request);

            _logger.LogInformation("EC2 Instance stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failure in stopping instance");
        }
    }

    private async Task<bool> ShouldWorkerBeRunningAsync()
    {
        var response = await _twitterClient.StreamsV2.GetRulesForFilteredStreamV2Async();

        return response.Rules.Any();
    }


}
