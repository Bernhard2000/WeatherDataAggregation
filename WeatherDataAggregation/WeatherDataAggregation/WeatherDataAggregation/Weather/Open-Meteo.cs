using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace WeatherDataAggregation.Weather;

public static class Open_Meteo
{
    private static Uri base_url = new Uri("https://archive-api.open-meteo.com/v1/");
    public static WeatherData fetchCurrentData(Location location)
    {
        using HttpClient client = new HttpClient();
        var uri = new Uri(base_url, $"forecast?latitude={location.Latitude}&longitude={location.Longitude}&current=Temperature");
        HttpResponseMessage response = client.GetAsync(uri).Result;
        response.EnsureSuccessStatusCode();
        WeatherData data = new WeatherData();
        if (response.IsSuccessStatusCode)
        {
            string responseBody = response.Content.ReadAsStringAsync().Result;
            JsonNode json = JsonNode.Parse(responseBody);
            data.Temperature = json["main"]["temp"].ToString();
            data.Humidity = json["main"]["humidity"].ToString();
            data.WindSpeed = json["wind"]["speed"].ToString();
            data.Pressure = json["main"]["pressure"].ToString();
            data.Cloudiness = json["clouds"]["all"].ToString();
            data.WeatherDescription = json["weather"][0]["description"].ToString();
        }
        return data;
    }

    public static  async Task<WeatherData[]> fetchHistoricDataHourly(Location location, DateTime startDate, DateTime endDate)
    {
        if (DateTime.Now - endDate < TimeSpan.FromDays(5))
        {
            throw new ArgumentException("End date must be at least 5 days in the past");
        }
                using HttpClient client = new HttpClient();
                var uri = new Uri(base_url,
                    $"archive?latitude={location.Latitude.ToString(CultureInfo.InvariantCulture)}&longitude={location.Longitude.ToString(CultureInfo.InvariantCulture)}&start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}&hourly=temperature_2m&timezone=auto");
                HttpResponseMessage response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                JsonNode json = JsonNode.Parse(responseBody);
                var time = json["hourly"]["time"].AsArray();
                WeatherData[] weatherDataArr = new WeatherData[time.Count];
                if (time != null)
                {
                    for (int i = 0; i < time.Count; i++)
                    {

                        WeatherData weatherData = new WeatherData();

                        if (json["hourly"]["temperature_2m"][i] != null)
                            weatherData.Temperature = json["hourly"]["temperature_2m"][i].ToString();
                        /*if (json["hourly"]["relative_humidity_2m"] != null)
                            weatherData.Humidity = json["hourly"]["relative_humidity_2m"][i].ToString();
                        if (json["hourly"]["dew_point_2m"] != null)
                            weatherData.DewPoint = json["hourly"]["dew_point_2m"][i].ToString();
                        if (json["hourly"]["apparent_temperature"] != null)
                            weatherData.FeelsLike = json["hourly"]["apparent_temperature"][i].ToString();
                        if (json["hourly"]["precipitation_probability"] != null)
                            weatherData.Precipitation = json["hourly"]["precipitation_probability"][i].ToString();*/

                        weatherData.Time = time[i].ToString();

                        weatherDataArr[i] = weatherData;
                    }
                }

                return weatherDataArr;
    }
    
    public static async Task<WeatherData[]> fetchHistoricDataDaily(Location location, DateTime startDate, DateTime endDate)
    {
        if (DateTime.Now - endDate < TimeSpan.FromDays(5))
        {
            throw new ArgumentException("End date must be at least 5 days in the past");
        }

        using HttpClient client = new HttpClient();
        var uri = new Uri(base_url,
            $"archive?latitude={location.Latitude.ToString(CultureInfo.InvariantCulture)}&longitude={location.Longitude.ToString(CultureInfo.InvariantCulture)}&start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}&daily=temperature_2m_mean,temperature_2m_max,temperature_2m_min&timezone=auto");
        HttpResponseMessage response = await client.GetAsync(uri);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        JsonNode json = JsonNode.Parse(responseBody);
        var time = json["daily"]["time"].AsArray();
        WeatherData[] weatherDataArr = new WeatherData[time.Count];
        if (time != null)
        {
            for (int i = 0; i < time.Count; i++)
            {
                WeatherData weatherData = new WeatherData();

                if (json["daily"]["temperature_2m_mean"][i] != null)
                    weatherData.Temperature = json["daily"]["temperature_2m_mean"][i].ToString();

                if (json["daily"]["temperature_2m_max"][i] != null)
                    weatherData.MaximumTemperature = json["daily"]["temperature_2m_max"][i].ToString();
                
                if (json["daily"]["temperature_2m_min"][i] != null)
                    weatherData.MinimumTemperature = json["daily"]["temperature_2m_min"][i].ToString();
                weatherData.Time = time[i].ToString();

                weatherDataArr[i] = weatherData;
            }
        }

        return weatherDataArr;
    }
    
    public static async Task<WeatherData[]> FetchForecastData(Location location)
    {
        using HttpClient client = new HttpClient();
        var uri = new Uri($"https://api.open-meteo.com/v1/forecast?latitude={location.Latitude.ToString(CultureInfo.InvariantCulture)}&longitude={location.Longitude.ToString(CultureInfo.InvariantCulture)}&hourly=temperature_2m,precipitation&daily=temperature_2m_max,temperature_2m_min&timezone=auto&past_days=5");
        HttpResponseMessage response = await client.GetAsync(uri);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        JsonNode json = JsonNode.Parse(responseBody);

        var hourlyData = json["hourly"]["temperature_2m"].AsArray();
        var hourlyPrecipitation = json["hourly"]["precipitation"].AsArray();
        var dailyDataMax = json["daily"]["temperature_2m_max"].AsArray();
        var dailyDataMin = json["daily"]["temperature_2m_min"].AsArray();
        var time = json["hourly"]["time"].AsArray();

        WeatherData[] weatherDataArr = new WeatherData[hourlyData.Count];

        for (int i = 0; i < hourlyData.Count; i++)
        {
            WeatherData weatherData = new WeatherData();

            weatherData.Temperature = hourlyData[i].ToString();
            weatherData.Precipitation = hourlyPrecipitation[i].ToString();
            weatherData.Time = time[i].ToString();
            weatherDataArr[i] = weatherData;
        }
        


        return weatherDataArr;
    }
}