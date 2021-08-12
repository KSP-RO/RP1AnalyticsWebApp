namespace RP1AnalyticsWebApp.Models
{
    public class CareerListItem
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string User { get; set; }

        public string Token { get; set; }

        public CareerListItem(CareerLog c)
        {
            Id = c.Id;
            Name = c.Name;
            User = c.UserLogin;
            Token = c.Token;
        }
    }
}
