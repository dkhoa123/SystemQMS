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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>()
                .HasOne(u => u.role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.roleId);
            modelBuilder.Entity<Users>()
                .HasOne(u => u.unit)
                .WithMany(r => r.users)
                .HasForeignKey(u => u.unitId);
            modelBuilder.Entity<Users>()
                .HasOne(u => u.counter)
                .WithMany(r => r.users)
                .HasForeignKey(u => u.counterId);
            modelBuilder.Entity<Users>()
                .HasMany(u => u.diaries)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.userId);
            modelBuilder.Entity<Users>()
                .HasMany(u => u.stickers)
                .WithOne(s => s.users)
                .HasForeignKey(s => s.userId);

            modelBuilder.Entity<Roles>()
                .HasMany(r => r.Users)
                .WithOne(u => u.role)
                .HasForeignKey(u => u.roleId);

            modelBuilder.Entity<Counter>()
                .HasMany(c => c.users)
                .WithOne(u => u.counter)
                .HasForeignKey(u => u.counterId);
            modelBuilder.Entity<Counter>()
                .HasOne(c => c.unit)
                .WithMany(u => u.counters)
                .HasForeignKey(c => c.unitId);
            modelBuilder.Entity<Counter>()
                .HasMany(c => c.counterSend)
                .WithOne(cc => cc.CounterSender)
                .HasForeignKey(cc => cc.id);
            modelBuilder.Entity<Counter>()
                .HasMany(c => c.counterReceive)
                .WithOne(cc => cc.CounterReceiver)
                .HasForeignKey(cc => cc.id);
           modelBuilder.Entity<Counter>()
                .HasMany(c => c.stickers)
                .WithOne(s => s.counter)
                .HasForeignKey(s => s.counterId);
            modelBuilder.Entity<Counter>()
                .HasMany(c => c.evaluations)
                .WithOne(e => e.Counter)
                .HasForeignKey(e => e.counterId);
            modelBuilder.Entity<Counter>()
                .HasOne(c => c.unit)
                .WithMany(u => u.counters)
                .HasForeignKey(c => c.unitId);

            modelBuilder.Entity<CounterChange>()
                .HasOne(cc => cc.CounterSender)
                .WithMany(c => c.counterSend)
                .HasForeignKey(cc => cc.id);
            modelBuilder.Entity<CounterChange>()
                .HasOne(cc => cc.CounterReceiver)
                .WithMany(c => c.counterReceive)
                .HasForeignKey(cc => cc.id);
            modelBuilder.Entity<CounterChange>()
                .HasOne(cc => cc.Sticker)
                .WithMany(s => s.CounterChanges)
                .HasForeignKey(cc => cc.sttId);

           modelBuilder.Entity<Service>()
                .HasMany(s => s.numberFomulars)
                .WithOne(nf => nf.Service)
                .HasForeignKey(nf => nf.serviceId);
            modelBuilder.Entity<Service>()
                .HasMany(s => s.bookings)
                .WithOne(b => b.Service)
                .HasForeignKey(b => b.serviceId);

            modelBuilder.Entity<Service>()
                .HasMany(s => s.stickers)
                .WithOne(st => st.service)
                .HasForeignKey(st => st.serviceId);
            modelBuilder.Entity<Service>()
                .HasMany(s => s.evaluations)
                .WithOne(e => e.Service)
                .HasForeignKey(e => e.serviceId);
            modelBuilder.Entity<Service>()
                .HasMany(s => s.sheduleActives)
                .WithOne(sa => sa.Service)
                .HasForeignKey(sa => sa.serviceId);
            modelBuilder.Entity<Service>()
                .HasOne(s => s.unit)
                .WithMany(u => u.services)
                .HasForeignKey(s => s.unitId);

            modelBuilder.Entity<NumberFomular>()
                .HasOne(nf => nf.Service)
                .WithMany(s => s.numberFomulars)
                .HasForeignKey(nf => nf.serviceId);

            modelBuilder.Entity<SheduleActive>()
                .HasOne(sa => sa.Service)
                .WithMany(s => s.sheduleActives)
                .HasForeignKey(sa => sa.serviceId);

             modelBuilder.Entity<Unit>()
                .HasMany(u => u.services)
                .WithOne(s => s.unit)
                .HasForeignKey(s => s.unitId);
            modelBuilder.Entity<Unit>()
                .HasMany(u => u.users)
                .WithOne(us => us.unit)
                .HasForeignKey(us => us.unitId);
            modelBuilder.Entity<Unit>()
                .HasMany(g => g.guests)
                .WithOne(u => u.unit)
                .HasForeignKey(u => u.unitId);
            modelBuilder.Entity<Unit>()
                .HasMany(d => d.diaries)
                .WithOne(u => u.Unit)
                .HasForeignKey(u => u.unitId);
            modelBuilder.Entity<Unit>()
                .HasMany(c => c.counters)
                .WithOne(u => u.unit)
                .HasForeignKey(u => u.unitId);
            modelBuilder.Entity<Unit>()
                .HasMany(st => st.stickers)
                .WithOne(s => s.unit)
                .HasForeignKey(s => s.unitId);
            modelBuilder.Entity<Unit>()
                .HasMany(n => n.noti)
                .WithOne(u => u.unit)
                .HasForeignKey(u => u.unitId);
            modelBuilder.Entity<Unit>()
                .HasMany(e => e.evaluations)
                .WithOne(u => u.Unit)
                .HasForeignKey(u => u.unitId);
            modelBuilder.Entity<Unit>()
                .HasMany(et => et.evaluationTemplates)
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
                .HasMany(b => b.bookings)
                .WithOne(b => b.Sticker)
                .HasForeignKey(st => st.sttId);
            modelBuilder.Entity<Sticker>()
                .HasOne(u => u.unit)
                .WithMany(c => c.stickers)
                .HasForeignKey(st => st.unitId);
            modelBuilder.Entity<Sticker>()
                .HasOne(c => c.counter)
                .WithMany(s => s.stickers)
                .HasForeignKey(st => st.counterId);
            modelBuilder.Entity<Sticker>()
                .HasOne(s => s.service)
                .WithMany(st => st.stickers)
                .HasForeignKey(st => st.serviceId);
            modelBuilder.Entity<Sticker>()
                .HasOne(u => u.users)
                .WithMany(s => s.stickers)
                .HasForeignKey(st => st.userId);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Service)
                .WithMany(s => s.bookings)
                .HasForeignKey(b => b.serviceId);
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Sticker)
                .WithMany(st => st.bookings)
                .HasForeignKey(b => b.sttId);
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Guest)
                .WithMany(g => g.bookings)
                .HasForeignKey(b => b.guestId);

            modelBuilder.Entity<Guest>()
                .HasMany(g => g.bookings)
                .WithOne(b => b.Guest)
                .HasForeignKey(b => b.guestId);
            modelBuilder.Entity<Guest>()
                .HasOne(g => g.unit)
                .WithMany(u => u.guests)
                .HasForeignKey(g => g.unitId);
            modelBuilder.Entity<Guest>()
                .HasMany(g => g.histories)
                .WithOne(d => d.guest)
                .HasForeignKey(d => d.guestId);

            modelBuilder.Entity<History>()
                .HasKey(h => new { h.notiId, h.guestId });
            modelBuilder.Entity<History>()
                .HasOne(h => h.guest)
                .WithMany(g => g.histories)
                .HasForeignKey(h => h.guestId);
            modelBuilder.Entity<History>()
                .HasOne(h => h.notification)
                .WithMany(n => n.Histories)
                .HasForeignKey(h => h.notiId);

            modelBuilder.Entity<Notification>()
                .HasMany(n => n.Histories)
                .WithOne(h => h.notification)
                .HasForeignKey(h => h.notiId);
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.unit)
                .WithMany(u => u.noti)
                .HasForeignKey(n => n.unitId);

            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Service)
                .WithMany(s => s.evaluations)
                .HasForeignKey(e => e.serviceId);
            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Counter)
                .WithMany(g => g.evaluations)
                .HasForeignKey(e => e.counterId);
            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Sticker)
                .WithMany(s => s.Evaluations)
                .HasForeignKey(e => e.sttId);
            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Unit)
                .WithMany(u => u.evaluations)
                .HasForeignKey(e => e.unitId);

            modelBuilder.Entity<EvaluationTemplate>()
                .HasOne(et => et.Unit)
                .WithMany(u => u.evaluationTemplates)
                .HasForeignKey(et => et.unitId);

            modelBuilder.Entity<Diary>()
                .HasOne(d => d.User)
                .WithMany(u => u.diaries)
                .HasForeignKey(d => d.userId);
            modelBuilder.Entity<Diary>()
                .HasOne(d => d.Unit)
                .WithMany(u => u.diaries)
                .HasForeignKey(d => d.unitId);
            
            //
            modelBuilder.Entity<Counter>()
                .HasMany(c => c.Services)
                .WithMany(s => s.counters)
                .UsingEntity(j => j.ToTable("counterServices"));
            //Index Unique để tránh trùng lặp counterId trong cùng một unitId
            modelBuilder.Entity<Counter>()
                .HasIndex(c => new { c.unitId, c.counterId })
                .IsUnique();

            
        }
    }
}
