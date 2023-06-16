namespace ZORGATH;

public class GetMatchStatsHandler : IClientRequesterHandler
{
    private string _replayServerUrl;
    private record AccountIdWithSelectedUpgradeCodes(int AccountId, List<string> SelectedUpgradeCodes);

    public GetMatchStatsHandler(string replayServerUrl)
    {
        _replayServerUrl = replayServerUrl;
    }

    public async Task<IActionResult> HandleRequest(ControllerContext controllerContext, Dictionary<string, string> formData)
    {
        string cookie = formData["cookie"];
        int matchId = int.Parse(formData["match_id"]);
        using BountyContext bountyContext = controllerContext.HttpContext.RequestServices.GetRequiredService<BountyContext>();

        AccountIdWithSelectedUpgradeCodes? accountIdWithSelectedUpgradeCodes = await bountyContext.Accounts
                .Where(account => account.Cookie == cookie)
                .Select(account => new AccountIdWithSelectedUpgradeCodes(account.AccountId, /*account.User.SelectedUpgradeCodes*/new()))
                .FirstOrDefaultAsync();
        if (accountIdWithSelectedUpgradeCodes == null)
        {
            // Account not found.
            return new NotFoundResult();
        }

        MatchResults? matchResults = await bountyContext.MatchResults.FirstOrDefaultAsync(matchResults => matchResults.match_id == matchId);
        if (matchResults is null)
        {
            // Match results not found.
            return new NotFoundResult();
        }

        List<PlayerMatchResults> playerMatchResults = await bountyContext.PlayerMatchResults.Where(playerMatchResults => playerMatchResults.match_id == matchId).ToListAsync();
        MatchSummary matchSummary = new(matchResults, winningTeam: DetermineWinningTeam(playerMatchResults), _replayServerUrl);

        PlayerMatchResults? playerMatchResult = playerMatchResults.FirstOrDefault(results => results.account_id == accountIdWithSelectedUpgradeCodes.AccountId);
        Dictionary<string, object> response = new()
        {
            ["selected_upgrades"] = accountIdWithSelectedUpgradeCodes.SelectedUpgradeCodes,
            ["match_summ"] = new Dictionary<int, object> { { matchId, matchSummary } },
            ["match_player_stats"] = new Dictionary<int, object> { { matchId, playerMatchResults } }
        };

        // inventory is null if the result is not submitted or the game is remade.
        if (matchResults.inventory != null)
        {
            response["inventory"] = new List<Dictionary<int, Dictionary<string, string>>> { System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, Dictionary<string, string>>>(matchResults.inventory) ?? new Dictionary<int, Dictionary<string, string>>() };
        }

        response["vested_threshold"] = 5;
        response["0"] = true;

        return new OkObjectResult(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Determine the winning team based on whether the players won or lost.
    ///     "1" corresponds to Legion. "2" corresponds to Hellbourne, "0" if remade (or error).
    /// </summary>
    private string DetermineWinningTeam(List<PlayerMatchResults> playerMatchResults)
    {
        foreach (PlayerMatchResults results in playerMatchResults)
        {
            if (results.wins == 1) { return results.team.ToString(); }
        }

        // neither.
        return "0";
    }
}
