namespace DOANCOSO26.Models
{
    public class BusTripImage
    {
        public int Id { get; set; }
        public string Url { get; set; } // Đường dẫn hình ảnh
        public int BusTripId { get; set; }
        public BusTrip? BusTrip { get; set; }
    }
}
