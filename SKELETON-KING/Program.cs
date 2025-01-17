namespace SKELETON_KING;

using Microsoft.EntityFrameworkCore;
using KINESIS;
using PUZZLEBOX;
using System.Collections.Concurrent;
using ZORGATH;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();

        string connectionString = builder.Configuration.GetConnectionString("BOUNTY")!;
        builder.Services.AddDbContextFactory<BountyContext>(options =>
        {
            options.UseSqlServer(connectionString, connection => connection.MigrationsAssembly("SKELETON-KING"));
        });

        ConcurrentDictionary<string, SrpAuthSessionData> srpAuthSessions = new();

        builder.Services.AddSingleton<IReadOnlyDictionary<string, IClientRequesterHandler>>(
            new Dictionary<string, IClientRequesterHandler>()
            {
                // NOTE: Please keep this list alphabetized by the string literal in the key.
                {"autocompleteNicks", new AutoCompleteNicksHandler() },
                {"pre_auth", new PreAuthHandler(srpAuthSessions) },
                {"show_simple_stats", new ShowSimpleStatsHandler() },
                {"srpAuth", new SrpAuthHandler(srpAuthSessions, new()) },
            }
        );
        builder.Services.AddSingleton<ChatServer>();

        var app = builder.Build();
        app.MapControllers();

        app.Services.GetRequiredService<ChatServer>().Start();
        app.Run();
    }
}
