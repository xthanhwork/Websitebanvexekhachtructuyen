using System.Buffers;

namespace DOANCOSO26.Models
{
    //OperatingStatus dùng để phân biệt xe đang khai thác và ẩn xe.
    public enum BusOperatingStatus
    {
        Active = 1,  // Xe đang khai thác
        Hidden = 2   // Xe ẩn, không khai thác
    }

    public class Bus
    {
        public int Id { get; set; }
        public string Company { get; set; }
        public string BusNumber { get; set; }

        public BusType? BusType { get; set; }

        // Thêm trường trạng thái
        public BusOperatingStatus OperatingStatus { get; set; } = BusOperatingStatus.Active;

        public ICollection<BusTrip>? BusTrips { get; set; } = new List<BusTrip>();
    }
}