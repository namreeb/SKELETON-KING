namespace ZORGATH;

public class MatchHistoryOverviewHandler : IClientRequesterHandler
{
    public async Task<IActionResult> HandleRequest(ControllerContext controllerContext, Dictionary<string, string> formData)
    {
        string nickname = formData["nickname"];
        string table = formData["table"];

        using BountyContext bountyContext = controllerContext.HttpContext.RequestServices.GetRequiredService<BountyContext>();
        string? serializedMatchIds = table switch
        {
            "campaign" => await bountyContext.Accounts.Where(a => a.Name == nickname).Select(a => a.PlayerSeasonStatsRanked.SerializedMatchIds).FirstOrDefaultAsync(),
            "campaign_casual" => await bountyContext.Accounts.Where(a => a.Name == nickname).Select(a => a.PlayerSeasonStatsRankedCasual.SerializedMatchIds).FirstOrDefaultAsync(),
            "player" => await bountyContext.Accounts.Where(a => a.Name == nickname).Select(a => a.PlayerSeasonStatsPublic.SerializedMatchIds).FirstOrDefaultAsync(),
            "midwars" => await bountyContext.Accounts.Where(a => a.Name == nickname).Select(a => a.PlayerSeasonStatsMidWars.SerializedMatchIds).FirstOrDefaultAsync(),
            _ => null
        };
        if (serializedMatchIds == null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(PHP.Serialize(await GetMatchHistoryOverview(bountyContext, nickname, serializedMatchIds)));
    }

    private async Task<Dictionary<string, string>> GetMatchHistoryOverview(BountyContext bountyContext, string nickname, string serializedMatchIds)
    {
        Dictionary<string, string> matchHistoryOverview = new();

        List<string> allMatchIds = serializedMatchIds.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
        IEnumerable<int> recentMatchIds = allMatchIds.TakeLast(100).Reverse().Select(matchId => int.Parse(matchId));

        List<PlayerMatchResults> playerMatchResultsList = await bountyContext.PlayerMatchResults
            .Where(results => recentMatchIds.Contains(results.match_id) && (results.nickname.Equals(nickname) || results.nickname.EndsWith($"]{nickname}"))).OrderByDescending(results => results.match_id).ToListAsync();

        for (int i = 0; i < playerMatchResultsList.Count; i++)
        {
            PlayerMatchResults playerMatchResults = playerMatchResultsList[i];

            string matchData = string.Join(',',
                playerMatchResults.match_id,
                playerMatchResults.wins,
                playerMatchResults.team,
                playerMatchResults.herokills,
                playerMatchResults.deaths,
                playerMatchResults.heroassists,
                playerMatchResults.hero_id,
                playerMatchResults.secs,
                playerMatchResults.map,
                playerMatchResults.mdt,
                playerMatchResults.cli_name
            );

            matchHistoryOverview.Add("m" + i, matchData);
        }

        return matchHistoryOverview;
    }
}
