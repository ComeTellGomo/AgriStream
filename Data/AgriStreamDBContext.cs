using AgriStream.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriStream.Data
{
    public class AgriStreamDBContext : DbContext
    {
        public AgriStreamDBContext(
            DbContextOptions<AgriStreamDBContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<DeviceRegistry> DeviceRegistries { get; set; }
    }
}