using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using RaceDay.Data;
using RaceDay.DTOs.Results;
using RaceDay.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ResultsController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get results. Participants see their own results; Organisers see all.
        /// </summary>
        [HttpGet]
        [Authorize]
        [SwaggerOperation(Summary = "Get results (participant own / organiser all)")]
        public async Task<IActionResult> GetResults([FromQuery] int? participantId = null)
        {
            var user = HttpContext.User;
            var idClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (idClaim == null) return Unauthorized();
            if (!int.TryParse(idClaim.Value, out var userId)) return Unauthorized();

            var role = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            var q = _db.Results.AsQueryable();

            if (role == "Participant")
            {
                var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserId == userId);
                if (participant == null) return Forbid();
                q = q.Where(r => r.Enrolment.ParticipantId == participant.Id);
            }
            else if (participantId.HasValue)
            {
                q = q.Where(r => r.Enrolment.ParticipantId == participantId.Value);
            }

            var items = await q.Select(r => new ResultResponse
            {
                Id = r.Id,
                EnrolmentId = r.EnrolmentId,
                FinishTime = r.FinishTime,
                GunTime = r.GunTime,
                ChipTime = r.ChipTime,
                OverallPosition = r.OverallPosition,
                GenderPosition = r.GenderPosition,
                CategoryPosition = r.CategoryPosition,
                Pace = r.Pace,
                IsDisqualified = r.IsDisqualified,
                DisqualificationReason = r.DisqualificationReason,
                ResultNotes = r.ResultNotes,
                RecordedAt = r.RecordedAt
            }).ToListAsync();

            return Ok(items);
        }

        /// <summary>
        /// Organiser only: post a result for an enrolment.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        [SwaggerOperation(Summary = "Create result (organiser only)")]
        public async Task<IActionResult> CreateResult([FromBody] ResultCreateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = HttpContext.User;
            var idClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (idClaim == null) return Unauthorized();
            if (!int.TryParse(idClaim.Value, out var userId)) return Unauthorized();

            var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserId == userId);
            if (organiser == null) return Forbid();

            var enrol = await _db.Enrolments.Include(e => e.Event).FirstOrDefaultAsync(e => e.Id == request.EnrolmentId);
            if (enrol == null) return NotFound(new { message = "Enrolment not found." });

            // Ensure organiser owns the event for which the result is being recorded
            if (enrol.Event.OrganiserId != organiser.Id) return Forbid();

            // if a result already exists for this enrolment, update it
            var existing = await _db.Results.FirstOrDefaultAsync(r => r.EnrolmentId == enrol.Id);
            if (existing != null)
            {
                existing.FinishTime = request.FinishTime;
                existing.GunTime = request.GunTime;
                existing.ChipTime = request.ChipTime;
                existing.OverallPosition = request.OverallPosition;
                existing.GenderPosition = request.GenderPosition;
                existing.CategoryPosition = request.CategoryPosition;
                existing.Pace = request.Pace;
                existing.IsDisqualified = request.IsDisqualified;
                existing.DisqualificationReason = request.DisqualificationReason;
                existing.ResultNotes = request.ResultNotes;
                existing.RecordedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return Ok(new { message = "Result updated.", id = existing.Id });
            }

            var result = new Result
            {
                EnrolmentId = enrol.Id,
                FinishTime = request.FinishTime,
                GunTime = request.GunTime,
                ChipTime = request.ChipTime,
                OverallPosition = request.OverallPosition,
                GenderPosition = request.GenderPosition,
                CategoryPosition = request.CategoryPosition,
                Pace = request.Pace,
                IsDisqualified = request.IsDisqualified,
                DisqualificationReason = request.DisqualificationReason,
                ResultNotes = request.ResultNotes,
                RecordedAt = DateTime.UtcNow
            };

            _db.Results.Add(result);
            await _db.SaveChangesAsync();

            var resp = new ResultResponse { Id = result.Id };
            return CreatedAtAction(nameof(GetResults), new { }, resp);
        }
    }
}
