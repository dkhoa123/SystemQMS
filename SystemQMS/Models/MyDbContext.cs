using Microsoft.EntityFrameworkCore;

namespace SystemQMS.Models
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }
       public DbSet<Users> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<EvaluationTemplate> EvaluationTemplates { get; set; }
        public DbSet<Evaluation> Evaluations { get; set; }
        public DbSet<Counter> Counters { get; set; }
        public DbSet<CounterChange> CounterChanges { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<NumberFomular> NumberFomulars { get; set; }
        public DbSet<SheduleActive> Shedules { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Diary> Diaries { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<History> Histories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UnitConfig> UnitConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);
            modelBuilder.Entity<Users>()
                .HasOne(u => u.Unit)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.UnitId);
            modelBuilder.Entity<Users>()
                .HasOne(u => u.Counter)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.CounterId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Users>()
                .HasMany(u => u.Diaries)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.UserId);
            modelBuilder.Entity<Users>()
                .HasMany(u => u.Tickets)
                .WithOne(s => s.Users)
                .HasForeignKey(s => s.UserId);

            modelBuilder.Entity<Roles>()
                .HasMany(r => r.Users)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<Counter>()
                .HasMany(c => c.Users)
                .WithOne(u => u.Counter)
                .HasForeignKey(u => u.CounterId);
            modelBuilder.Entity<Counter>()
                .HasOne(c => c.Unit)
                .WithMany(u => u.Counters)
                .HasForeignKey(c => c.UnitId);
            
           modelBuilder.Entity<Counter>()
                .HasMany(c => c.Tickets)
                .WithOne(s => s.Counter)
                .HasForeignKey(s => s.CounterId);
            modelBuilder.Entity<Counter>()
                .HasMany(c => c.Evaluations)
                .WithOne(e => e.Counter)
                .HasForeignKey(e => e.CounterId);
            modelBuilder.Entity<Counter>()
                .HasOne(c => c.Unit)
                .WithMany(u => u.Counters)
                .HasForeignKey(c => c.UnitId);
            modelBuilder.Entity<Counter>()
                .HasIndex(c => c.CounterId)
                .IsUnique();

            modelBuilder.Entity<CounterChange>()
                .HasOne(cc => cc.CounterSender)
                .WithMany(c => c.CounterSend)
                .HasForeignKey(cc => cc.CounterIdSender) 
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CounterChange>()
                .HasOne(cc => cc.CounterReceiver)
                .WithMany(c => c.CounterReceive)
                .HasForeignKey(cc => cc.CounterIdReceive)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CounterChange>()
                .HasOne(cc => cc.Tickets)
                .WithMany(s => s.CounterChanges)
                .HasForeignKey(cc => cc.SttId);

           modelBuilder.Entity<Service>()
                .HasMany(s => s.NumberFomulars)
                .WithOne(nf => nf.Service)
                .HasForeignKey(nf => nf.ServiceId);
            modelBuilder.Entity<Service>()
                .HasMany(s => s.Bookings)
                .WithOne(b => b.Service)
                .HasForeignKey(b => b.ServiceId);

            modelBuilder.Entity<Service>()
                .HasMany(s => s.Tickets)
                .WithOne(t => t.Service)
                .HasForeignKey(t => t.ServiceId);
            modelBuilder.Entity<Service>()
                .HasMany(s => s.Evaluations)
                .WithOne(e => e.Service)
                .HasForeignKey(e => e.ServiceId);
            modelBuilder.Entity<Service>()
                .HasMany(s => s.SheduleActives)
                .WithOne(sa => sa.Service)
                .HasForeignKey(sa => sa.ServiceId);
            modelBuilder.Entity<Service>()
                .HasOne(s => s.Unit)
                .WithMany(u => u.Services)
                .HasForeignKey(s => s.UnitId);
            modelBuilder.Entity<Service>()
                .HasIndex(s => s.ServiceId)
                .IsUnique();


            modelBuilder.Entity<NumberFomular>()
                .HasOne(nf => nf.Service)
                .WithMany(s => s.NumberFomulars)
                .HasForeignKey(nf => nf.ServiceId);

            modelBuilder.Entity<SheduleActive>()
                .HasOne(sa => sa.Service)
                .WithMany(s => s.SheduleActives)
                .HasForeignKey(sa => sa.ServiceId);

             modelBuilder.Entity<Unit>()
                .HasMany(u => u.Services)
                .WithOne(s => s.Unit)
                .HasForeignKey(s => s.UnitId);
            modelBuilder.Entity<Unit>()
                .HasMany(u => u.Users)
                .WithOne(us => us.Unit)
                .HasForeignKey(us => us.UnitId);
            modelBuilder.Entity<Unit>()
                .HasMany(g => g.Guests)
                .WithOne(u => u.Unit)
                .HasForeignKey(u => u.UnitId);
            modelBuilder.Entity<Unit>()
                .HasMany(d => d.Diaries)
                .WithOne(u => u.Unit)
                .HasForeignKey(u => u.UnitId);
            modelBuilder.Entity<Unit>()
                .HasMany(c => c.Counters)
                .WithOne(u => u.Unit)
                .HasForeignKey(u => u.UnitId);
            modelBuilder.Entity<Unit>()
                .HasMany(st => st.Tickets)
                .WithOne(s => s.Unit)   
                .HasForeignKey(s => s.UnitId);
            modelBuilder.Entity<Unit>()
                .HasMany(n => n.Notifications)
                .WithOne(u => u.Unit)
                .HasForeignKey(u => u.UnitId);
            modelBuilder.Entity<Unit>()
                .HasMany(e => e.Evaluations)
                .WithOne(u => u.Unit)
                .HasForeignKey(u => u.UnitId);
            modelBuilder.Entity<Unit>()
                .HasMany(et => et.EvaluationTemplates)
                .WithOne(u => u.Unit)
                .HasForeignKey(u => u.UnitId);
            modelBuilder.Entity<Unit>()
                .HasMany(d => d.Devices)
                .WithOne(u => u.Unit)
                .HasForeignKey(u => u.UnitId);

            modelBuilder.Entity<Device>()
                .HasOne(d => d.Unit)
                .WithMany(u => u.Devices)
                .HasForeignKey(d => d.UnitId);
            modelBuilder.Entity<Device>()
                .HasIndex(d => d.DeviceId)
                .IsUnique();

            modelBuilder.Entity<Ticket>()
                .HasMany(st => st.CounterChanges)
                .WithOne(cc => cc.Tickets)
                .HasForeignKey(cc => cc.SttId);
            modelBuilder.Entity<Ticket>()
                .HasMany(e =>e.Evaluations)
                .WithOne(e => e.Tickets)
                .HasForeignKey(st => st.SttId);
            modelBuilder.Entity<Ticket>()
                .HasMany(b => b.Bookings)
                .WithOne(b => b.Ticket)
                .HasForeignKey(st => st.SttId);
            modelBuilder.Entity<Ticket>()
                .HasOne(u => u.Unit)
                .WithMany(c => c.Tickets)
                .HasForeignKey(st => st.UnitId);
            modelBuilder.Entity<Ticket>()
                .HasOne(c => c.Counter)
                .WithMany(s => s.Tickets)
                .HasForeignKey(st => st.CounterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(s => s.Service)
                .WithMany(st => st.Tickets)
                .HasForeignKey(st => st.ServiceId);
            modelBuilder.Entity<Ticket>()
                .HasOne(u => u.Users)
                .WithMany(s => s.Tickets)
                .HasForeignKey(st => st.UserId);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Service)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ServiceId);
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Ticket)
                .WithMany(st => st.Bookings)
                .HasForeignKey(b => b.SttId);
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Guest)
                .WithMany(g => g.Bookings)
                .HasForeignKey(b => b.GuestId);

            modelBuilder.Entity<Guest>()
                .HasMany(g => g.Bookings)
                .WithOne(b => b.Guest)
                .HasForeignKey(b => b.GuestId);
            modelBuilder.Entity<Guest>()
                .HasOne(g => g.Unit)
                .WithMany(u => u.Guests)
                .HasForeignKey(g => g.UnitId);
            modelBuilder.Entity<Guest>()
                .HasMany(g => g.Histories)
                .WithOne(d => d.Guest)
                .HasForeignKey(d => d.GuestId);

            modelBuilder.Entity<History>()
                .HasKey(h => new { h.NotiId, h.GuestId });
            modelBuilder.Entity<History>()
                .HasOne(h => h.Guest)
                .WithMany(g => g.Histories)
                .HasForeignKey(h => h.GuestId);
            modelBuilder.Entity<History>()
                .HasOne(h => h.Notification)
                .WithMany(n => n.Histories)
                .HasForeignKey(h => h.NotiId);

            modelBuilder.Entity<Notification>()
                .HasMany(n => n.Histories)
                .WithOne(h => h.Notification)
                .HasForeignKey(h => h.NotiId);
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Unit)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UnitId);

            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Service)
                .WithMany(s => s.Evaluations)
                .HasForeignKey(e => e.ServiceId);
            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Counter)
                .WithMany(g => g.Evaluations)
                .HasForeignKey(e => e.CounterId);
            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Tickets)
                .WithMany(s => s.Evaluations)
                .HasForeignKey(e => e.SttId);
            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Unit)
                .WithMany(u => u.Evaluations)
                .HasForeignKey(e => e.UnitId);

            modelBuilder.Entity<EvaluationTemplate>()
                .HasOne(et => et.Unit)
                .WithMany(u => u.EvaluationTemplates)
                .HasForeignKey(et => et.UnitId);

            modelBuilder.Entity<Diary>()
                .HasOne(d => d.User)
                .WithMany(u => u.Diaries)
                .HasForeignKey(d => d.UserId);
            modelBuilder.Entity<Diary>()
                .HasOne(d => d.Unit)
                .WithMany(u => u.Diaries)
                .HasForeignKey(d => d.UnitId);

            modelBuilder.Entity<Unit>()
                .HasOne(u => u.UnitConfig)
                .WithOne(uc => uc.Unit)
                .HasForeignKey<UnitConfig>(uc => uc.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UnitConfig>()
                .HasOne(uc => uc.EvaluationTemplate)
                .WithMany()
                .HasForeignKey(uc => uc.ActiveEvaluationTemplateId)
                .OnDelete(DeleteBehavior.SetNull);

        }
    }
}
