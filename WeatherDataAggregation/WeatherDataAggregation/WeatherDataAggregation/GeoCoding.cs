using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace WeatherDataAggregation;

public static class GeoCoding
{
    //I know it's horrible practice to include API keys in code, but I really don't  care about a key to a free API, generated with a throwaway email alias
    private static string api_key = "6668698e8d611408745444ybxf017a3";
    private static Uri base_url = new Uri("https://geocode.maps.co/");
        
    public static async Task<List<Location>> GetLocationAsync(string locationName)
    {
        using HttpClient client = new HttpClient();
        var uri = new Uri(base_url, $"search?api_key={api_key}&q={locationName}");
        HttpResponseMessage response = await client.GetAsync(uri);
        List<Location> locations = new List<Location>();
        response.EnsureSuccessStatusCode();
        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            JsonNode json = JsonNode.Parse(responseBody);
            
            if (json != null)
            {
                foreach (var result in json.AsArray())
                {
                    Location loc = new Location();
                    loc.Name = result["display_name"].ToString();
                    loc.Latitude = Double.Parse(result["lat"].ToString(), CultureInfo.InvariantCulture);
                    loc.Longitude = Double.Parse(result["lon"].ToString(), CultureInfo.InvariantCulture);
                    loc.ShortName = result["display_name"].ToString().Split(",")[0] +  ", " + result["display_name"].ToString().Split(",")[1];
                    locations.Add(loc);
                }
                
            }
        }
        return locations;
    }

}