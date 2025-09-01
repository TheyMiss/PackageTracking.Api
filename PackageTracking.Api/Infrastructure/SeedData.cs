// Fully generated with AI for testing purposes only.
using PackageTracking.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace PackageTracking.Api.Infrastructure;

public static class SeedData
{
    public static void Initialize(IServiceProvider services, int count = 100)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<AppDbContext>();
        var gen = sp.GetRequiredService<ITrackingNumberGenerator>();

        // If anything exists, skip seeding (idempotent)
        if (db.Packages.Any()) return;

        var rnd = new Random(42); // fixed seed for repeatable results

        // sample data pools
        string[] firstNames = { "Alice", "Bob", "Carol", "Dave", "Eve", "Frank", "Grace", "Heidi", "Ivan", "Judy", "Mallory", "Niaj", "Olivia", "Peggy", "Rupert", "Sybil", "Trent", "Uma", "Victor", "Wendy" };
        string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson" };
        string[] streets = { "Main St", "Oak Ave", "Pine Rd", "Maple Blvd", "Cedar Ln", "Birch Way", "Elm St", "Hill Rd", "River Dr", "Sunset Ave" };
        string[] cities = { "Vilnius", "Kaunas", "Klaipėda", "Šiauliai", "Panevėžys", "Alytus", "Marijampolė", "Ukmergė", "Trakai", "Nida" };

        var now = DateTimeOffset.UtcNow;
        var packages = new List<Package>(capacity: count);

        for (int i = 0; i < count; i++)
        {
            // make sender/recipient
            var senderName = $"{Pick(firstNames, rnd)} {Pick(lastNames, rnd)}";
            var recipName = $"{Pick(firstNames, rnd)} {Pick(lastNames, rnd)}";
            var sender = new PersonInfo
            {
                Name = senderName,
                Address = $"{rnd.Next(1, 200)} {Pick(streets, rnd)}, {Pick(cities, rnd)}",
                Phone = $"+3706{rnd.Next(1000000, 9999999)}"
            };
            var recipient = new PersonInfo
            {
                Name = recipName,
                Address = $"{rnd.Next(1, 200)} {Pick(streets, rnd)}, {Pick(cities, rnd)}",
                Phone = $"+3706{rnd.Next(1000000, 9999999)}"
            };

            // base timestamps within last ~30 days
            var createdAt = now.AddDays(-rnd.Next(0, 30)).AddMinutes(-rnd.Next(0, 1440));
            var history = new List<StatusChange>
            {
                new() { Status = PackageStatus.Created, changedAt = createdAt }
            };

            // choose a realistic path
            // 0: Created only
            // 1: Created -> Sent
            // 2: Created -> Sent -> Accepted
            // 3: Created -> Sent -> Returned
            // 4: Created -> Canceled
            // 5: Created -> Sent -> Canceled
            var path = rnd.Next(0, 6);

            DateTimeOffset last = createdAt;

            if (path >= 1)
            {
                last = last.AddHours(rnd.Next(1, 72));
                history.Add(new StatusChange { Status = PackageStatus.Sent, changedAt = last });
            }

            PackageStatus current = PackageStatus.Created;
            current = path switch
            {
                0 => PackageStatus.Created,
                1 => PackageStatus.Sent,
                2 => PackageStatus.Accepted,
                3 => PackageStatus.Returned,
                4 => PackageStatus.Canceled,
                5 => PackageStatus.Canceled,
                _ => PackageStatus.Created
            };

            // add final step(s) if needed
            if (path == 2) // Accepted
            {
                last = last.AddHours(rnd.Next(1, 120));
                history.Add(new StatusChange { Status = PackageStatus.Accepted, changedAt = last });
            }
            else if (path == 3) // Returned (could be final or not; keep final for simplicity)
            {
                // already added Sent; now Returned
                last = last.AddHours(rnd.Next(1, 96));
                history.Add(new StatusChange { Status = PackageStatus.Returned, changedAt = last });
            }
            else if (path == 4) // Canceled from Created
            {
                last = last.AddHours(rnd.Next(1, 48));
                // (skip Sent) directly to Canceled
                history.Add(new StatusChange { Status = PackageStatus.Canceled, changedAt = last });
            }
            else if (path == 5) // Canceled after Sent
            {
                last = last.AddHours(rnd.Next(1, 72));
                history.Add(new StatusChange { Status = PackageStatus.Canceled, changedAt = last });
            }

            var pkg = new Package
            {
                TrackingNumber = gen.Generate(),
                Sender = sender,
                Recipient = recipient,
                CreatedAt = createdAt,
                CurrentStatus = current,
                History = history
            };

            packages.Add(pkg);
        }

        db.Packages.AddRange(packages);
        db.SaveChanges();

        static T Pick<T>(IReadOnlyList<T> list, Random rnd) => list[rnd.Next(list.Count)];
    }
}
