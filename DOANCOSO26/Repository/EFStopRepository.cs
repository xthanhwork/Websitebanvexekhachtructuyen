using Microsoft.EntityFrameworkCore;
using       DOANCOSO26.Data;
using DOANCOSO26.Models;

namespace DOANCOSO26.Repository
{
    public class EFStopRepository : IStopRepository
    {
        private readonly ApplicationDbContext _context;

        public EFStopRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Stop>> GetAllAsync()
        {
            return await _context.Stops.ToListAsync();
        }

        public async Task<Stop> GetByIdAsync(int id)
        {
            return await _context.Stops
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Stop stop)
        {
            _context.Stops.Add(stop);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Stop stop)
        {
            _context.Stops.Update(stop);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var stop = await _context.Stops.FindAsync(id);
            _context.Stops.Remove(stop);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Stop>> GetStopsByBusRouteId(int busRouteId)
        {
            var route = await _context.BusRoutes
                .Include(br => br.StartStop)
                .Include(br => br.EndStop)
                .FirstOrDefaultAsync(br => br.Id == busRouteId);

            var stops = new List<Stop>();

            if (route?.StartStop != null)
            {
                stops.Add(route.StartStop);
            }

            if (route?.EndStop != null)
            {
                stops.Add(route.EndStop);
            }

            return stops;
        }
        public async Task<IEnumerable<Stop>> GetStopsByBusTripIdAsync(int busTripId)
        {
            var busTrip = await _context.BusTrips
                                        .Include(bt => bt.BusRoute)
                                        .ThenInclude(br => br.Stops)
                                        .FirstOrDefaultAsync(bt => bt.Id == busTripId);

            return busTrip?.BusRoute?.Stops ?? Enumerable.Empty<Stop>();
        }
    }
}
