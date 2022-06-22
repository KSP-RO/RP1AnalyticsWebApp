namespace RP1AnalyticsWebApp.Models
{
    public class Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public override string ToString() => $"X: {X}; Y: {Y}; Z: {Z}";
    }
}
