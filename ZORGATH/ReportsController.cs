using System.Data;

namespace ZORGATH;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
public class ReportsController : ControllerBase
{
    public record ReportGetDTO(int IncidentReportID, int? ReporterID, int? AccusedID, int? ReviewerID, int MatchID, IncidentType IncidentType, string Description, int OffenseOccurrenceTime, IncidentStatus IncidentStatus, string ResolutionDetails);

    public ReportsController(BountyContext bountyContext) => _bountyContext = bountyContext;

    private readonly BountyContext _bountyContext;

    [HttpGet("Unprocessed", Name = "Get Unprocessed Reports")]
    //[Authorize(Roles = IdentityRoles.ElevatedPrivilegeRoles)]
    public async Task<IActionResult> GetUnprocessedReports()
    {
        List<IncidentReport> reports = await _bountyContext.IncidentReports.Where(report => report.Status != IncidentStatus.Done && report.Status != IncidentStatus.Rejected).ToListAsync();
        List<ReportGetDTO> output = reports.Select(report => new ReportGetDTO(report.IncidentReportId, report.ReporterId, report.AccusedId, report.ReviewerId, report.MatchId, report.IncidentType, report.Description ?? string.Empty, report.OffenseOccurrenceTime, report.Status, report.ResolutionDetails ?? string.Empty)).ToList();
        return Ok(output);
    }

    [HttpGet("UnderReview", Name = "Get Reports Under Review")]
    //[Authorize(Roles = IdentityRoles.ElevatedPrivilegeRoles)]
    public async Task<IActionResult> GetReportsUnderReview()
    {
        List<IncidentReport> reports = await _bountyContext.IncidentReports.Where(report => report.Status == IncidentStatus.UnderReview).ToListAsync();
        List<ReportGetDTO> output = reports.Select(report => new ReportGetDTO(report.IncidentReportId, report.ReporterId, report.AccusedId, report.ReviewerId, report.MatchId, report.IncidentType, report.Description ?? string.Empty, report.OffenseOccurrenceTime, report.Status, report.ResolutionDetails ?? string.Empty)).ToList();
        return Ok(output);
    }

    [HttpGet("Reporter/{id}", Name = "Get Reports By Reporter ID")]
    //[Authorize(Roles = IdentityRoles.ElevatedPrivilegeRoles)]
    public async Task<IActionResult> GetReportsByReporterId(int id)
    {
        List<IncidentReport> reports = await _bountyContext.IncidentReports.Where(report => report.ReporterId == id).ToListAsync();
        List<ReportGetDTO> output = reports.Select(report => new ReportGetDTO(report.IncidentReportId, report.ReporterId, report.AccusedId, report.ReviewerId, report.MatchId, report.IncidentType, report.Description ?? string.Empty, report.OffenseOccurrenceTime, report.Status, report.ResolutionDetails ?? string.Empty)).ToList();
        return Ok(output);
    }

    [HttpGet("Accused/{id}", Name = "Get Reports By Accused ID")]
    //[Authorize(Roles = IdentityRoles.ElevatedPrivilegeRoles)]
    public async Task<IActionResult> GetReportsByAccusedId(int id)
    {
        List<IncidentReport> reports = await _bountyContext.IncidentReports.Where(report => report.AccusedId == id).ToListAsync();
        List<ReportGetDTO> output = reports.Select(report => new ReportGetDTO(report.IncidentReportId, report.ReporterId, report.AccusedId, report.ReviewerId, report.MatchId, report.IncidentType, report.Description ?? string.Empty, report.OffenseOccurrenceTime, report.Status, report.ResolutionDetails ?? string.Empty)).ToList();

        return Ok(output);
    }

    [HttpGet("Reviewer/{id}", Name = "Get Reports By Reviewer ID")]
    //[Authorize(Roles = IdentityRoles.ElevatedPrivilegeRoles)]
    public async Task<IActionResult> GetReportsByReviewerId(int id)
    {
        List<IncidentReport> reports = await _bountyContext.IncidentReports.Where(report => report.ReviewerId == id).ToListAsync();
        List<ReportGetDTO> output = reports.Select(report => new ReportGetDTO(report.IncidentReportId, report.ReporterId, report.AccusedId, report.ReviewerId, report.MatchId, report.IncidentType, report.Description ?? string.Empty, report.OffenseOccurrenceTime, report.Status, report.ResolutionDetails ?? string.Empty)).ToList();

        return Ok(output);
    }
}
