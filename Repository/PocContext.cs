using Balance.Controllers;
using Microsoft.EntityFrameworkCore;

public class PocContext : DbContext
{

    public PocContext(DbContextOptions<PocContext> options) : base(options) { }

    public virtual DbSet<Event> Events { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}