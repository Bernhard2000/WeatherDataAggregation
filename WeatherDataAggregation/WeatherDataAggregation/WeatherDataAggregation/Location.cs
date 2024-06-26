namespace WeatherDataAggregation;

public class Location
{
    public string Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    public string ShortName { get; set; }
    public override string ToString()
    {
        return Name + " (" + Latitude + ", " + Longitude + ")";
    }
}