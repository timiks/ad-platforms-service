using System.Collections.Immutable;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace AdPlatformsService;

[ApiController]
[Route("ad-platforms")]
public class AdPlatformsController : ControllerBase
{
    private static ImmutableDictionary<string, ImmutableList<string>> _adPlatformsLocationsData
        = ImmutableDictionary<string, ImmutableList<string>>.Empty;

    // For tests
    public static ImmutableDictionary<string, ImmutableList<string>> GetDataForTesting()
        => _adPlatformsLocationsData;
    public static void SetDataForTesting(ImmutableDictionary<string, ImmutableList<string>> testData)
    {
        _adPlatformsLocationsData = testData;
    }

    [HttpPost("upload")]
    public IActionResult UploadAdPlatforms(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required.");

        var newData = new Dictionary<string, HashSet<string>>();

        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            // Parse file data format
            // (Line format): {Ad Platform}:{locations:{/a},{/a/b},{/a/b/c}}
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var lineParts = line.Split(':', 2);

                if (lineParts.Length != 2 || string.IsNullOrWhiteSpace(lineParts[0]))
                    continue;

                var adPlatform = lineParts[0].Trim();
                var locations = lineParts[1].Split(',');

                foreach (var location in locations)
                {
                    var normLocation = location.Trim();
                    if (string.IsNullOrEmpty(normLocation))
                        continue;

                    // Normalize
                    if (!normLocation.StartsWith('/'))
                        normLocation = "/" + normLocation;
                    normLocation = normLocation.TrimEnd('/');

                    if (!newData.TryGetValue(normLocation, out var adPlatforms))
                    {
                        adPlatforms = [];
                        newData[normLocation] = adPlatforms;
                    }
                    adPlatforms.Add(adPlatform);
                }
            }
        }

        // Set loaded data
        _adPlatformsLocationsData = newData
            .ToImmutableDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToImmutableList()
            );

        return Ok();
    }

    [HttpGet]
    public IActionResult GetAdPlatforms([FromQuery] string location)
    {
        if (string.IsNullOrEmpty(location))
            return BadRequest("Location is required.");

        // Normalize
        if (!location.StartsWith('/'))
            location = "/" + location;
        location = location.TrimEnd('/');

        var result = new HashSet<string>();

        // Use Span and StringBuilder for better perfomance
        var locationSpan = location.AsSpan();
        var sb = new StringBuilder();

        // Local method to avoid duplcation
        void CheckLocationPrefix()
        {
            if (sb.Length > 0)
            {
                var locationPrefix = sb.ToString();
                if (_adPlatformsLocationsData.TryGetValue(locationPrefix, out var adPlatforms))
                {
                    result.UnionWith(adPlatforms);
                }
            }
        }

        for (int i = 0; i < locationSpan.Length; i++)
        {
            if (locationSpan[i] == '/')
                CheckLocationPrefix();

            sb.Append(locationSpan[i]);

            if (i == locationSpan.Length - 1)
                CheckLocationPrefix();
        }

        return Ok(result.ToList());
    }
}