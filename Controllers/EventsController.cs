using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using RaceDay.Data;
using RaceDay.DTOs.Events;
using RaceDay.DTOs.Categories;
using RaceDay.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public EventsController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Public: Get list of events with filters and pagination.
        /// </summary>
        [HttpGet]
        [SwaggerOperation(Summary = "List events (public)")]
        public async Task<IActionResult> GetEvents([FromQuery] string city = null, [FromQuery] string province = null,
            [FromQuery] string eventType = null, [FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null,
            [FromQuery] string status = null, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 10;

            var q = _db.Events.AsQueryable();
            if (!string.IsNullOrEmpty(city)) q = q.Where(e => e.City == city);
            if (!string.IsNullOrEmpty(province)) q = q.Where(e => e.Province == province);
            if (!string.IsNullOrEmpty(eventType)) q = q.Where(e => e.EventType == eventType);
            if (dateFrom.HasValue) q = q.Where(e => e.EventDate >= dateFrom.Value.Date);
            if (dateTo.HasValue) q = q.Where(e => e.EventDate <= dateTo.Value.Date);
            if (!string.IsNullOrEmpty(status)) q = q.Where(e => e.Status == status);

            var total = await q.CountAsync();
            var items = await q.OrderBy(e => e.EventDate).Skip((page - 1) * limit).Take(limit)
                .Select(e => new EventResponse
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    EventTime = e.EventTime,
                    Venue = e.Venue,
                    City = e.City,
                    Province = e.Province,
                    EventType = e.EventType,
                    Distance = e.Distance,
                    DistanceUnit = e.DistanceUnit,
                    MaxParticipants = e.MaxParticipants,
                    RegistrationOpenDate = e.RegistrationOpenDate,
                    RegistrationCloseDate = e.RegistrationCloseDate,
                    Metadata = e.Metadata,
                    Status = e.Status,
                    OrganiserId = e.OrganiserId
                }).ToListAsync();

            return Ok(new { total, page, limit, items });
        }

        /// <summary>
        /// Public: Get event by id including categories and enrolment counts.
        /// </summary>
        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get event details (public)")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var ev = await _db.Events.Include(e => e.Categories).FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null) return NotFound();

            var response = new EventResponse
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                EventDate = ev.EventDate,
                EventTime = ev.EventTime,
                Venue = ev.Venue,
                City = ev.City,
                Province = ev.Province,
                EventType = ev.EventType,
                Distance = ev.Distance,
                DistanceUnit = ev.DistanceUnit,
                MaxParticipants = ev.MaxParticipants,
                RegistrationOpenDate = ev.RegistrationOpenDate,
                RegistrationCloseDate = ev.RegistrationCloseDate,
                Metadata = ev.Metadata,
                Status = ev.Status,
                OrganiserId = ev.OrganiserId
            };

            foreach (var c in ev.Categories)
            {
                var count = await _db.Enrolments.CountAsync(en => en.CategoryId == c.Id && en.EventId == ev.Id && en.Status == "Registered");
                response.Categories.Add(new CategorySummary { Id = c.Id, CategoryName = c.CategoryName, EnrolmentCount = count });
            }

            return Ok(response);
        }

        /// <summary>
        /// Organiser only: create a new event (organiser becomes owner).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        [SwaggerOperation(Summary = "Create event (organiser only)")]
        public async Task<IActionResult> CreateEvent([FromBody] EventCreateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            if (!int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

            var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserId == userId);
            if (organiser == null) return Forbid();

            var ev = new Event
            {
                Title = request.Title,
                Description = request.Description,
                EventDate = request.EventDate,
                EventTime = request.EventTime,
                Venue = request.Venue,
                City = request.City,
                Province = request.Province,
                EventType = request.EventType,
                Distance = request.Distance,
                DistanceUnit = request.DistanceUnit,
                MaxParticipants = request.MaxParticipants,
                RegistrationOpenDate = request.RegistrationOpenDate,
                RegistrationCloseDate = request.RegistrationCloseDate,
                Metadata = request.Metadata,
                Status = "Draft",
                OrganiserId = organiser.Id
            };

            _db.Events.Add(ev);
            await _db.SaveChangesAsync();

            var resp = new EventResponse { Id = ev.Id };
            return CreatedAtAction(nameof(GetEvent), new { id = ev.Id }, resp);
        }

        /// <summary>
        /// Organiser only: update own event.
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        [SwaggerOperation(Summary = "Update event (organiser only, own events)")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] EventUpdateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            if (!int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

            var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserId == userId);
            if (organiser == null) return Forbid();

            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != organiser.Id) return Forbid();

            ev.Title = request.Title;
            ev.Description = request.Description;
            ev.EventDate = request.EventDate;
            ev.EventTime = request.EventTime;
            ev.Venue = request.Venue;
            ev.City = request.City;
            ev.Province = request.Province;
            ev.EventType = request.EventType;
            ev.Distance = request.Distance;
            ev.DistanceUnit = request.DistanceUnit;
            ev.MaxParticipants = request.MaxParticipants;
            ev.RegistrationOpenDate = request.RegistrationOpenDate;
            ev.RegistrationCloseDate = request.RegistrationCloseDate;
            ev.Metadata = request.Metadata;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Event updated." });
        }

        /// <summary>
        /// Organiser only: delete own event (cascades categories, enrolments, results).
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        [SwaggerOperation(Summary = "Delete event (organiser only, own events)")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userIdClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            if (!int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

            var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserId == userId);
            if (organiser == null) return Forbid();

            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != organiser.Id) return Forbid();

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Public: get categories for an event.
        /// </summary>
        [HttpGet("{id:int}/categories")]
        [SwaggerOperation(Summary = "Get categories for an event (public)")]
        public async Task<IActionResult> GetEventCategories(int id)
        {
            var exists = await _db.Events.AnyAsync(e => e.Id == id);
            if (!exists) return NotFound();

            var cats = await _db.Categories.Where(c => c.EventId == id)
                .Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    EventId = c.EventId,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    Distance = c.Distance,
                    DistanceUnit = c.DistanceUnit,
                    AgeGroupMin = c.AgeGroupMin,
                    AgeGroupMax = c.AgeGroupMax,
                    GenderRestriction = c.GenderRestriction,
                    EntryFee = c.EntryFee,
                    LateEntryFee = c.LateEntryFee,
                    MaxParticipants = c.MaxParticipants,
                    StartTime = c.StartTime,
                    CutOffTime = c.CutOffTime
                }).ToListAsync();

            return Ok(cats);
        }

        /// <summary>
        /// Public: get event stats (total enrolments, category breakdown, demographics).
        /// </summary>
        [HttpGet("{id:int}/stats")]
        [SwaggerOperation(Summary = "Get event statistics (public)")]
        public async Task<IActionResult> GetEventStats(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();

            var totalEnrolments = await _db.Enrolments.CountAsync(en => en.EventId == id && en.Status == "Registered");

            var categoryBreakdown = await _db.Categories
                .Where(c => c.EventId == id)
                .Select(c => new { c.Id, c.CategoryName, Count = _db.Enrolments.Count(en => en.CategoryId == c.Id && en.Status == "Registered") })
                .ToListAsync();

            var genderDemo = await _db.Enrolments
                .Where(en => en.EventId == id)
                .Join(_db.Participants, en => en.ParticipantId, p => p.Id, (en, p) => p)
                .Join(_db.Users, p => p.UserId, u => u.Id, (p, u) => u.Gender)
                .GroupBy(g => g)
                .Select(g => new { Gender = g.Key, Count = g.Count() })
                .ToListAsync();

            return Ok(new { totalEnrolments, categoryBreakdown, genderDemo });
        }
    }
}
