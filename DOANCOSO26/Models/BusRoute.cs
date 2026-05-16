//namespace DOANCOSO26.Models
//{
//    public class BusRoute
//    {
//        public int Id { get; set; }
//        public string? Start { get; set; }
//        public string? End { get; set; }
//        public string? Distance { get; set; }
//        public string? Time { get; set; }
//        public string? StartLatitude { get; set; }
//        public string? Price { get; set; }
//        public string? StartLongitude { get; set; }

//        public string? EndLatitude { get; set; }

//        public string? EndLongitude { get; set; }
//        public string DisplayName => $"{Start} - {End}";
//        public ICollection<Stop>? Stops { get; set; }
//        public string? ImageUrl { get; set; }
//        public List<RouteImage>? Images { get; set; }
//        public ICollection<BusTrip>? BusTrips { get; set; } = new List<BusTrip>();
//    }
//}
namespace DOANCOSO26.Models
{
    public class BusRoute
    {
        public int Id { get; set; }

        // FK Stop
        public int? StartStopId { get; set; }

        public int? EndStopId { get; set; }

        // Navigation
        public virtual Stop? StartStop { get; set; }

        public virtual Stop? EndStop { get; set; }

        public string? Distance { get; set; }

        public string? Time { get; set; }

        //public string? StartLatitude { get; set; }

        public string? Price { get; set; }

  

        // Hiển thị
        public string DisplayName =>
            $"{StartStop?.Name} - {EndStop?.Name}";

        public ICollection<Stop>? Stops { get; set; }

        public string? ImageUrl { get; set; }

        public List<RouteImage>? Images { get; set; }

        public ICollection<BusTrip>? BusTrips { get; set; }
            = new List<BusTrip>();
    }
}
