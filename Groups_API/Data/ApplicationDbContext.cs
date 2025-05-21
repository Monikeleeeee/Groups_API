using Microsoft.EntityFrameworkCore;
using Groups_API.Models.Domain;
using Groups_API.Models.Enums;

namespace Groups_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase("GroupExpenseDb");
            }
        }

        public DbSet<Group> Groups { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<GroupMembership> GroupMemberships { get; set; }
        public DbSet<Debt> Debts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionSplit> TransactionSplits { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var group1 = new Group { Id = 1, Title = "Trip to Paris" };
            var group2 = new Group { Id = 2, Title = "Weekend BBQ" };
            var group3 = new Group { Id = 3, Title = "Ski Trip" };

            modelBuilder.Entity<Group>().HasData(group1, group2, group3);

            var me = new Member { Id = 1, Name = "Me" };
            var alice = new Member { Id = 2, Name = "Alice" };
            var bob = new Member { Id = 3, Name = "Bob" };
            var charlie = new Member { Id = 4, Name = "Charlie" };
            var dave = new Member { Id = 5, Name = "Dave" };

            modelBuilder.Entity<Member>().HasData(me, alice, bob, charlie, dave);

            modelBuilder.Entity<GroupMembership>().HasData(
                new GroupMembership { Id = 1, GroupId = 1, MemberId = 1, Balance = 0 },
                new GroupMembership { Id = 2, GroupId = 1, MemberId = 2, Balance = 10 },
                new GroupMembership { Id = 3, GroupId = 1, MemberId = 3, Balance = -10 },

                new GroupMembership { Id = 4, GroupId = 2, MemberId = 1, Balance = 0 },
                new GroupMembership { Id = 5, GroupId = 2, MemberId = 3, Balance = 5 },
                new GroupMembership { Id = 6, GroupId = 2, MemberId = 4, Balance = -5 },

                new GroupMembership { Id = 7, GroupId = 3, MemberId = 1, Balance = -15 },
                new GroupMembership { Id = 8, GroupId = 3, MemberId = 4, Balance = 15 },
                new GroupMembership { Id = 9, GroupId = 3, MemberId = 5, Balance = 0 }
            );

            modelBuilder.Entity<Debt>().HasData(
                new Debt { Id = 1, GroupId = 1, DebtorId = 3, CreditorId = 2, Amount = 10 },
                new Debt { Id = 2, GroupId = 2, DebtorId = 4, CreditorId = 3, Amount = 5 },
                new Debt { Id = 3, GroupId = 3, DebtorId = 1, CreditorId = 4, Amount = 15 }
            );

            modelBuilder.Entity<Transaction>().HasData(
                new Transaction
                {
                    Id = 1,
                    GroupId = 1,
                    PayerId = 2,
                    TotalAmount = 30,
                    Date = new DateTime(2024, 1, 1),
                    SplitType = SplitType.Equal
                },
                new Transaction
                {
                    Id = 2,
                    GroupId = 1,
                    PayerId = 3,
                    TotalAmount = 20,
                    Date = new DateTime(2024, 1, 2),
                    SplitType = SplitType.Equal
                },
                new Transaction
                {
                    Id = 3,
                    GroupId = 2,
                    PayerId = 3,
                    TotalAmount = 50,
                    Date = new DateTime(2024, 2, 1),
                    SplitType = SplitType.Equal
                },
                new Transaction
                {
                    Id = 4,
                    GroupId = 3,
                    PayerId = 4, 
                    TotalAmount = 75,
                    Date = new DateTime(2024, 3, 1),
                    SplitType = SplitType.Equal
                }
            );

            modelBuilder.Entity<TransactionSplit>().HasData(
                new TransactionSplit { Id = 1, TransactionId = 1, MemberId = 1, Amount = 10 },
                new TransactionSplit { Id = 2, TransactionId = 1, MemberId = 2, Amount = 10 },
                new TransactionSplit { Id = 3, TransactionId = 1, MemberId = 3, Amount = 10 },

                new TransactionSplit { Id = 4, TransactionId = 2, MemberId = 1, Amount = 10 },
                new TransactionSplit { Id = 5, TransactionId = 2, MemberId = 2, Amount = 10 },
                new TransactionSplit { Id = 6, TransactionId = 2, MemberId = 3, Amount = 0 },

                new TransactionSplit { Id = 7, TransactionId = 3, MemberId = 1, Amount = 20 },
                new TransactionSplit { Id = 8, TransactionId = 3, MemberId = 3, Amount = 30 },
                new TransactionSplit { Id = 9, TransactionId = 3, MemberId = 4, Amount = 0 },

                new TransactionSplit { Id = 10, TransactionId = 4, MemberId = 1, Amount = 25 },
                new TransactionSplit { Id = 11, TransactionId = 4, MemberId = 4, Amount = 50 },
                new TransactionSplit { Id = 12, TransactionId = 4, MemberId = 5, Amount = 0 }
            );


            modelBuilder.Entity<GroupMembership>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.GroupMemberships)
                .HasForeignKey(gm => gm.GroupId);

            modelBuilder.Entity<GroupMembership>()
                .HasOne(gm => gm.Member)
                .WithMany(m => m.GroupMemberships)
                .HasForeignKey(gm => gm.MemberId);

            modelBuilder.Entity<Debt>()
                .HasOne(d => d.Group)
                .WithMany(g => g.Debts)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Debt>()
                .HasOne(d => d.Debtor)
                .WithMany()
                .HasForeignKey(d => d.DebtorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Debt>()
                .HasOne(d => d.Creditor)
                .WithMany()
                .HasForeignKey(d => d.CreditorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Debt>()
                .HasIndex(d => new { d.GroupId, d.DebtorId, d.CreditorId })
                .IsUnique();
        }

    }
}
