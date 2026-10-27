using Microsoft.EntityFrameworkCore;

namespace SystemQMS.Models
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }
       public DbSet<Users> users { get; set; }
        public DbSet<Roles> roles { get; set; }
        public DbSet<EvaluationTemplate> evaluationTemplates { get; set; }
        public DbSet<Evaluation> evaluations { get; set; }
        public DbSet<Counter> counters { get; set; }
        public DbSet<CounterChange> counterChanges { get; set; }
        public DbSet<Service> services { get; set; }
        public DbSet<Device> devices { get; set; }
        public DbSet<NumberFomular> numberFomulars { get; set; }
        public DbSet<SheduleActive> shedules { get; set; }
        public DbSet<Unit> units { get; set; }
        public DbSet<Booking> bookings { get; set; }
        public DbSet<Guest> guests { get; set; }
        public DbSet<Diary> diaries { get; set; }
        public DbSet<Sticker> stickers { get; set; }
        public DbSet<History> histories { get; set; }
        public DbSet<Notification> notifications { get; set; }
        public DbSet<UnitConfig> unitConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.roleId);
            modelBuilder.Entity<Users>()
                .HasOne(u => u.Unit)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.unitId);
            modelBuilder.Entity<Users>()
                .HasOne(u => u.Counter)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.counterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Users>()
                .HasMany(u => u.Diaries)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.userId);
            modelBuilder.Entity<Users>()
                .HasMany(u => u.Stickers)
                .WithOne(s => s.Users)
                .HasForeignKey(s => s.userId);

            modelBuilder.Entity<Roles>()
                .HasMany(r => r.Users)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.roleId);

            modelBuilder.Entity<Counter>()
                .HasMany(c => c.Users)
                .WithOne(u => u.Counter)
                .HasForeignKey(u => u.counterId);
            modelBuilder.Entity<Counter>()
                .HasOne(c => c.Unit)
                .WithMany(u => u.Counters)
                .HasForeignKey(c => c.unitId);
            modelBuilder.Entity<Counter>()
                .HasMany(c => c.CounterSend)
                .WithOne(cc => cc.CounterSender)
                .HasForeignKey(cc => cc.id);
            modelBuilder.Entity<Counter>()
                .HasMany(c => c.CounterReceive)
                .WithOne(cc => cc.CounterReceiver)
                .HasForeignKey(cc => cc.id);
           modelBuilder.Entity<Counter>()
                .HasMany(c => c.Stickers)
                .WithOne(s => s.Counter)
                .HasForeignKey(s => s.counterId);
            modelBuilder.Entity<Counter>()
                .HasMany(c => c.Evaluations)
                .WithOne(e => e.Counter)
                .HasForeignKey(e => e.counterId);
            modelBuilder.Entity<Counter>()
                .HasOne(c => c.Unit)
                .WithMany(u => u.Counters)
                .HasForeignKey(c => c.unitId);

            modelBuilder.Entity<CounterChange>()
                .HasOne(cc => cc.CounterSender)
                .WithMany(c => c.CounterSend)
                .HasForeignKey(cc => cc.id);
            modelBuilder.Entity<CounterChange>()
                .HasOne(cc => cc.CounterReceiver)
                .WithMany(c => c.CounterReceive)
                .HasForeignKey(cc => cc.id);
            modelBuilder.Entity<CounterChange>()
                .HasOne(cc => cc.Sticker)
                .WithMany(s => s.CounterChanges)
                .HasForeignKey(cc => cc.sttId);

           modelBuilder.Entity<Service>()
                .HasMany(s => s.NumberFomulars)
                .WithOne(nf => nf.Service)
                .HasForeignKey(nf => nf.serviceId);
            modelBuilder.Entity<Service>()
                .HasMany(s => s.Bookings)
                .WithOne(b => b.Service)
                .HasForeignKey(b => b.serviceId);

            modelBuilder.Entity<Service>()
                .HasMany(s => s.Stickers)
                .WithOne(st => st.Service)
                .HasForeignKey(st => st.serviceId);
            modelBuilder.Entity<Service>()
                .HasMany(s => s.Evaluations)
                .WithOne(e => e.Service)
                .HasForeignKey(e => e.serviceId);
            modelBuilder.Entity<Service>()
                .HasMany(s => s.SheduleActives)
                .WithOne(sa => sa.Service)
                .HasForeignKey(sa => sa.serviceId);
            modelBuilder.Entity<Service>()
                .HasOne(s => s.Unit)
                .WithMany(u => u.Services)
                .HasForeignKey(s => s.unitId);


            modelBuilder.Entity<NumberFomular>()
                .HasOne(nf => nf.Service)
                .WithMany(s => s.NumberFomulars)
                .HasForeignKey(nf => nf.serviceId);

            modelBuilder.Entity<SheduleActive>()
                .HasOne(sa => sa.Service)
                .WithMany(s => s.SheduleActives)
                .HasForeignKey(sa => sa.serviceId);

             modelBuilder.Entity<Unit>()
                .HasMany(u => u.Services)
                .WithOne(s => s.Unit)
                .HasForeignKey(s => s.unitId);
            modelBuilder.Entity<Unit>()
                .HasMany(u => u.Users)
                .WithOne(us => us.Unit)
                .HasForeignKey(us => us.unitId);
            modelBuilder.Entity<Unit>()
                .HasMany(g => g.Guests)
                .WithOne(u => u.Unit)
                .HasForeignKey(u => u.unitId);
            modelBuilder.Entity<Unit>()
                .HasMany(d => d.Diaries)
                .WithOne(u => u.Unit)
                .HasForeignKey(u => u.unitId);
            modelBuilder.Entity<Unit>()
                .HasMany(c => c.Counters)
                .WithOne(u => u.Unit)
                .HasForeignKey(u => u.unitId);
            modelBuilder.Entity<Unit>()
                .HasMany(st => st.Stickers)
                .WithOne(s => s.Unit)   
                .HasForeignKey(s => s.unitId);
            modelBuilder.Entity<Unit>()
                .HasMany(n => n.Notifications)
                .WithOne(u => u.Unit)
                .HasForeignKey(u => u.unitId);
            modelBuilder.Entity<Unit>()
                .HasMany(e => e.Evaluations)
                .WithOne(u => u.Unit)
                .HasForeignKey(u => u.unitId);
            modelBuilder.Entity<Unit>()
                .HasMany(et => et.EvaluationTemplates)
                .WithOne(u => u.Unit)
                .HasForeignKey(u => u.unitId);

            modelBuilder.Entity<Sticker>()
                .HasMany(st => st.CounterChanges)
                .WithOne(cc => cc.Sticker)
                .HasForeignKey(cc => cc.sttId);
            modelBuilder.Entity<Sticker>()
                .HasMany(e =>e.Evaluations)
                .WithOne(e => e.Sticker)
                .HasForeignKey(st => st.sttId);
            modelBuilder.Entity<Sticker>()
                .HasMany(b => b.Bookings)
                .WithOne(b => b.Sticker)
                .HasForeignKey(st => st.sttId);
            modelBuilder.Entity<Sticker>()
                .HasOne(u => u.Unit)
                .WithMany(c => c.Stickers)
                .HasForeignKey(st => st.unitId);
            modelBuilder.Entity<Sticker>()
                .HasOne(c => c.Counter)
                .WithMany(s => s.Stickers)
                .HasForeignKey(st => st.counterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sticker>()
                .HasOne(s => s.Service)
                .WithMany(st => st.Stickers)
                .HasForeignKey(st => st.serviceId);
            modelBuilder.Entity<Sticker>()
                .HasOne(u => u.Users)
                .WithMany(s => s.Stickers)
                .HasForeignKey(st => st.userId);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Service)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.serviceId);
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Sticker)
                .WithMany(st => st.Bookings)
                .HasForeignKey(b => b.sttId);
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Guest)
                .WithMany(g => g.Bookings)
                .HasForeignKey(b => b.guestId);

            modelBuilder.Entity<Guest>()
                .HasMany(g => g.Bookings)
                .WithOne(b => b.Guest)
                .HasForeignKey(b => b.guestId);
            modelBuilder.Entity<Guest>()
                .HasOne(g => g.Unit)
                .WithMany(u => u.Guests)
                .HasForeignKey(g => g.unitId);
            modelBuilder.Entity<Guest>()
                .HasMany(g => g.Histories)
                .WithOne(d => d.Guest)
                .HasForeignKey(d => d.guestId);

            modelBuilder.Entity<History>()
                .HasKey(h => new { h.notiId, h.guestId });
            modelBuilder.Entity<History>()
                .HasOne(h => h.Guest)
                .WithMany(g => g.Histories)
                .HasForeignKey(h => h.guestId);
            modelBuilder.Entity<History>()
                .HasOne(h => h.Notification)
                .WithMany(n => n.Histories)
                .HasForeignKey(h => h.notiId);

            modelBuilder.Entity<Notification>()
                .HasMany(n => n.Histories)
                .WithOne(h => h.Notification)
                .HasForeignKey(h => h.notiId);
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Unit)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.unitId);

            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Service)
                .WithMany(s => s.Evaluations)
                .HasForeignKey(e => e.serviceId);
            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Counter)
                .WithMany(g => g.Evaluations)
                .HasForeignKey(e => e.counterId);
            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Sticker)
                .WithMany(s => s.Evaluations)
                .HasForeignKey(e => e.sttId);
            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Unit)
                .WithMany(u => u.Evaluations)
                .HasForeignKey(e => e.unitId);

            modelBuilder.Entity<EvaluationTemplate>()
                .HasOne(et => et.Unit)
                .WithMany(u => u.EvaluationTemplates)
                .HasForeignKey(et => et.unitId);

            modelBuilder.Entity<Diary>()
                .HasOne(d => d.User)
                .WithMany(u => u.Diaries)
                .HasForeignKey(d => d.userId);
            modelBuilder.Entity<Diary>()
                .HasOne(d => d.Unit)
                .WithMany(u => u.Diaries)
                .HasForeignKey(d => d.unitId);

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
