namespace RP1AnalyticsWebApp.Models
{
    public class CareerListItem
    {
        public string Id { get; set; }

        public string name { get; set; }

        public string token { get; set; }

        public CareerListItem(CareerLog c)
        {
            Id = c.Id;
            name = c.Name;
            token = c.Token;
        }
    }
}
