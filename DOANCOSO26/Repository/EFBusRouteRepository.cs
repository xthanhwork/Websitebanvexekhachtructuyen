using DOANCOSO26.Data;
using DOANCOSO26.Models;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;

namespace DOANCOSO26.Repository
{
    public class EFBusRouteRepository : IBusRouteRepository
    {
        private readonly ApplicationDbContext _context;

        public EFBusRouteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tương tự như EFProductRepository, nhưng cho BusTrip
        public async Task<IEnumerable<BusRoute>> GetAllAsync()
        {

            //return await _context.BusRoutes.ToListAsync();
            return await _context.BusRoutes
            .Include(x => x.StartStop)
            .Include(x => x.EndStop)
            .Include(x => x.BusTrips)
            .ToListAsync();
        }

        public async Task<BusRoute> GetByIdAsync(int id)
        {
            return await _context.BusRoutes
                .Include(x => x.StartStop)
                .Include(x => x.EndStop)
                .Include(x => x.BusTrips)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task AddAsync(BusRoute BusRoute)
        {
            _context.BusRoutes.Add(BusRoute);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BusRoute BusRoute)
        {
            _context.BusRoutes.Update(BusRoute);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var BusRoute = await _context.BusRoutes.FindAsync(id);
            _context.BusRoutes.Remove(BusRoute);
            await _context.SaveChangesAsync();
        }
        public async Task<List<string>> GetAllStartLocationsAsync()
        {
            return await _context.BusRoutes
                .Where(br => br.StartStop != null)
                .Select(br => br.StartStop.Name)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<string>> GetAllEndLocationsAsync()
        {
            return await _context.BusRoutes
                .Where(br => br.EndStop != null)
                .Select(br => br.EndStop.Name)
                .Distinct()
                .ToListAsync();
        }
    }
}
