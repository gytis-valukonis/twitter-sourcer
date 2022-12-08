using Microsoft.AspNetCore.Mvc;
using Tweetinvi;
using Tweetinvi.Exceptions;
using Tweetinvi.Models.V2;
using Tweetinvi.Parameters.V2;
using TwitterSourcer.Api.EC2;

namespace TwitterSourcer.Api.Controllers;

public record FilterItemModel(string Id, string Value);
public record FilterListResponse(List<FilterItemModel> Filters);

public record CreateFilterModel(string Value);

[ApiController]
public class FiltersController : ControllerBase
{
    private readonly ITwitterClient _twitterClient;
    private readonly Ec2Manager _ec2Manager;

    public FiltersController(ITwitterClient twitterClient, Ec2Manager ec2Manager)
	{
        _twitterClient = twitterClient;
        _ec2Manager = ec2Manager;
    }

    [HttpGet("/applied-filter-rules")]
    [ProducesResponseType(typeof(FilterListResponse), 200)]
    public async Task<IActionResult> GetCurrentlyAppliedFilters()
    {
        var appliedFilters = await _twitterClient.StreamsV2.GetRulesForFilteredStreamV2Async();

        var filterModels = appliedFilters.Rules
            .Select(r => new FilterItemModel(r.Id, r.Value))
            .ToList();

        return Ok(new FilterListResponse(filterModels));
    }

    [HttpPost("/applied-filter-rules")]
    [ProducesResponseType(typeof(FilterItemModel), 201)]
    [ProducesResponseType(typeof(BadRequestObjectResult), 400)]
    public async Task<IActionResult> ApplyNewFilter([FromBody] CreateFilterModel filter)
    {
        FilteredStreamRulesV2Response? response = null;

        try
        {
            response = await _twitterClient.StreamsV2.AddRulesToFilteredStreamAsync(new FilteredStreamRuleConfig(filter.Value));
        }
        catch (TwitterException ex) when (ex.Content.Contains("RulesCapExceeded"))
        {
            //Would look for a better way to differentiate validation errors.
            //Perhaps making the http calls myself would better allow me to take control of the responses
            ModelState.AddModelError(nameof(filter.Value), "Exceeded rules cap");
        }

        if (response == null)
        {
            ModelState.AddModelError("Unknown", "Unknown validation issue");

            return BadRequest(ModelState);
        }

        //Would produce customer error type, but for now just a simple message will suffice
        if (response.Errors?.Any() == true)
        {
            if (response?.Errors?.Any(e => e.Title == "DuplicateRule") == true) 
            {
                ModelState.AddModelError(nameof(filter.Value), "This filter value already exists");
            }
            else
            {
                ModelState.AddModelError("Unknown", "Unknown validation issue");
            }
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var model = response.Rules
            .Select(r => new FilterItemModel(r.Id, r.Value))
            .First();

        await _ec2Manager.ManageInstanceAsync();

        //TODO: at Get by id endpoint
        return CreatedAtAction(nameof(GetCurrentlyAppliedFilters), model);
    }

    [HttpDelete("/applied-filter-rules/{id}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteAppliedFilter([FromRoute] string id)
    {
        var response = await _twitterClient.StreamsV2.DeleteRulesFromFilteredStreamAsync(id);

        if (response == null || response.Errors?.Length > 0)
        {
            ModelState.AddModelError("Unknown", "Unknown validation issue");
            return BadRequest(ModelState);
        }

        await _ec2Manager.ManageInstanceAsync();

        return NoContent();
    }

}
