//using System.Net.Http;
//using Newtonsoft.Json;

//namespace DOANCOSO26.Services
//{
//    public class GoogleMapService
//    {
//        private readonly string _apiKey = "YOUR_GOOGLE_MAPS_API_KEY";

//        public async Task<(double distanceKm, double durationMin)> GetDistanceAsync(
//            double startLat,
//            double startLng,
//            double endLat,
//            double endLng)
//        {
//            string url =
//                $"https://maps.googleapis.com/maps/api/distancematrix/json" +
//                $"?origins={startLat},{startLng}" +
//                $"&destinations={endLat},{endLng}" +
//                $"&mode=driving" +
//                $"&language=vi-VN" +
//                $"&key={_apiKey}";

//            using (HttpClient client = new HttpClient())
//            {
//                var response = await client.GetStringAsync(url);

//                dynamic json = JsonConvert.DeserializeObject(response);

//                // 🔥 CHECK 1: API có trả rows không
//                if (json?.rows == null || json.rows.Count == 0)
//                    return (0, 0);

//                // 🔥 CHECK 2: elements có tồn tại không
//                if (json.rows[0]?.elements == null || json.rows[0].elements.Count == 0)
//                    return (0, 0);

//                var element = json.rows[0].elements[0];

//                // 🔥 CHECK 3: status phải OK
//                if (element.status != "OK")
//                    return (0, 0);

//                // 🔥 CHECK 4: tránh null distance/duration
//                if (element.distance == null || element.duration == null)
//                    return (0, 0);

//                double distanceKm = element.distance.value / 1000.0;
//                double durationMin = element.duration.value / 60.0;

//                return (distanceKm, durationMin);
//            }
//        }
//    }
//}
using System.Net.Http;
using Newtonsoft.Json;

namespace DOANCOSO26.Services
{
    public class GoogleMapService
    {
        public async Task<(double distanceKm, double durationMin)> GetDistanceAsync(
            double startLat,
            double startLng,
            double endLat,
            double endLng)
        {
            string url =
                $"http://router.project-osrm.org/route/v1/driving/" +
                $"{startLng},{startLat};{endLng},{endLat}?overview=false";

            using HttpClient client = new HttpClient();

            var response = await client.GetStringAsync(url);
            dynamic json = JsonConvert.DeserializeObject(response);

            if (json?.routes == null || json.routes.Count == 0)
                return (0, 0);

            double distanceKm = json.routes[0].distance / 1000.0;
            double durationMin = json.routes[0].duration / 60.0;

            return (distanceKm, durationMin);
        }
    }
}