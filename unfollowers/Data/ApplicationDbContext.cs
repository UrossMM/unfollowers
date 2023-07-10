using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using unfollowers.Models;

namespace unfollowers.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<FollowersStatistic> FollowersStatistics { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Preklopite metodu OnModelCreating kako biste konfigurisali mapiranje entiteta
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FollowersStatistic>()
            .Property(x => x.InitialState)
            .HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<List<UserInfo>>(v));


            modelBuilder.Entity<FollowersStatistic>()
            .Property(x => x.UnfollowMe)
            .HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<List<UnfollowerInfo>>(v));

        }
    }
}
