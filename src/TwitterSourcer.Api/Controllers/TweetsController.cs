using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TwitterSourcer.Core;

namespace TwitterSourcer.Api.Controllers;

public record TweetByDateResponse(List<TweetModel> Tweets);
public record TweetModel(
	string Id, 
	string AuthorId, 
	string Content, 
	string Source, 
	string Location);

[ApiController]
public class TweetsController : ControllerBase
{
	//TODO: Add setup dependency container to use this, but since there aren't
	//many dependencies, easier to do it this way
	private readonly TweetRepository _tweetRepository = new TweetRepository();

	//TODO: Add pagination 
	[HttpGet("/tweets")]
	[ProducesResponseType(typeof(TweetByDateResponse), 200)]
    [ProducesResponseType(typeof(BadRequestObjectResult), 400)]
    public async Task<ActionResult<TweetByDateResponse>> GetTweetsForDate([Required][FromQuery]DateTime? date)
	{
		if (date == null)
		{
			return BadRequest(ModelState);
		}

		var tweets = await _tweetRepository.GetTweetsAsync(date.Value);

		var tweetModels = tweets.Select(t =>
				new TweetModel(t.Id, t.AuthorId, t.Content, t.Source, t.Location)
			).ToList();

        var model = new TweetByDateResponse(tweetModels);

		return Ok(model);
	}
}
