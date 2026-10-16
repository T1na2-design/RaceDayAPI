using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.DTOs.Enrolments;
using RaceDay.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public EnrolmentsController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get enrolments. Participants see own only; Organisers see all.
        /// Supports filters: participantId, eventId, status, dateFrom, dateTo
        /// </summary>
        [HttpGet]
        [Authorize]
        [SwaggerOperation(Summary = "Get enrolments (participant own / organiser all)")]
        public async Task<IActionResult> GetEnrolments([FromQuery] int? participantId = null, [FromQuery] int? eventId = null,
            [FromQuery] string status = null, [FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null)
        {
            var user = HttpContext.User;
            var idClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (idClaim == null) return Unauthorized();
            if (!int.TryParse(idClaim.Value, out var userId)) return Unauthorized();

            var role = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            var q = _db.Enrolments.AsQueryable();

            if (role == "Participant")
            {
                // find participant id
                var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserId == userId);
                if (participant == null) return Forbid();
                q = q.Where(e => e.ParticipantId == participant.Id);
            }
            else if (participantId.HasValue)
            {
                q = q.Where(e => e.ParticipantId == participantId.Value);
            }

            if (eventId.HasValue) q = q.Where(e => e.EventId == eventId.Value);
            if (!string.IsNullOrEmpty(status)) q = q.Where(e => e.Status == status);
            if (dateFrom.HasValue) q = q.Where(e => e.EnrolmentDate >= dateFrom.Value.Date);
            if (dateTo.HasValue) q = q.Where(e => e.EnrolmentDate <= dateTo.Value.Date);

            var items = await q.Select(e => new EnrolmentResponse
            {
                Id = e.Id,
                ParticipantId = e.ParticipantId,
                EventId = e.EventId,
                CategoryId = e.CategoryId,
                Status = e.Status,
                EnrolmentDate = e.EnrolmentDate
            }).ToListAsync();

            return Ok(items);
        }

        /// <summary>
        /// Get specific enrolment. Participant can view own; Organiser can view all.
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize]
        [SwaggerOperation(Summary = "Get enrolment by id")]
        public async Task<IActionResult> GetEnrolment(int id)
        {
            var enrol = await _db.Enrolments.Include(e => e.Participant).FirstOrDefaultAsync(e => e.Id == id);
            if (enrol == null) return NotFound();

            var user = HttpContext.User;
            var idClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (idClaim == null) return Unauthorized();
            if (!int.TryParse(idClaim.Value, out var userId)) return Unauthorized();

            var role = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (role == "Participant")
            {
                var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserId == userId);
                if (participant == null) return Forbid();
                if (enrol.ParticipantId != participant.Id) return Forbid();
            }

            var resp = new EnrolmentResponse
            {
                Id = enrol.Id,
                ParticipantId = enrol.ParticipantId,
                EventId = enrol.EventId,
                CategoryId = enrol.CategoryId,
                Status = enrol.Status,
                EnrolmentDate = enrol.EnrolmentDate
            };

            return Ok(resp);
        }

        /// <summary>
        /// Participant only: create an enrolment for a category.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Participant")]
        [SwaggerOperation(Summary = "Create enrolment (participant only)")]
        public async Task<IActionResult> CreateEnrolment([FromBody] EnrolmentCreateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = HttpContext.User;
            var idClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (idClaim == null) return Unauthorized();
            if (!int.TryParse(idClaim.Value, out var userId)) return Unauthorized();

            var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserId == userId);
            if (participant == null) return Forbid();

            var cat = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.Id == request.CategoryId);
            if (cat == null) return NotFound(new { message = "Category not found." });

            // check capacity
            if (cat.MaxParticipants.HasValue)
            {
                var current = await _db.Enrolments.CountAsync(e => e.CategoryId == cat.Id && e.Status == "Registered");
                if (current >= cat.MaxParticipants.Value) return BadRequest(new { message = "Category full." });
            }

            // check event registration dates
            var ev = cat.Event;
            if (ev.RegistrationOpenDate.HasValue && DateTime.UtcNow < ev.RegistrationOpenDate.Value) return BadRequest(new { message = "Registration not open." });
            if (ev.RegistrationCloseDate.HasValue && DateTime.UtcNow > ev.RegistrationCloseDate.Value) return BadRequest(new { message = "Registration closed." });

            // ensure participant not already enrolled in same category/event
            var already = await _db.Enrolments.AnyAsync(e => e.ParticipantId == participant.Id && e.CategoryId == cat.Id && e.EventId == cat.EventId && e.Status == "Registered");
            if (already) return Conflict(new { message = "Already enrolled." });

            var enrol = new Enrolment
            {
                ParticipantId = participant.Id,
                EventId = cat.EventId,
                CategoryId = cat.Id,
                Status = "Registered",
                EnrolmentDate = DateTime.UtcNow
            };

            _db.Enrolments.Add(enrol);
            await _db.SaveChangesAsync();

            var resp = new EnrolmentResponse { Id = enrol.Id };
            return CreatedAtAction(nameof(GetEnrolment), new { id = enrol.Id }, resp);
        }
    }
}
