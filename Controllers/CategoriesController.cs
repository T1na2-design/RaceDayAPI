using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using RaceDay.Data;
using RaceDay.DTOs.Categories;
using RaceDay.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Public: list categories with filters.
        /// </summary>
        [HttpGet]
        [SwaggerOperation(Summary = "List categories (public)")]
        public async Task<IActionResult> GetCategories([FromQuery] int? eventId = null, [FromQuery] string categoryType = null,
            [FromQuery] decimal? distanceMin = null, [FromQuery] decimal? distanceMax = null)
        {
            var q = _db.Categories.AsQueryable();
            if (eventId.HasValue) q = q.Where(c => c.EventId == eventId.Value);
            if (!string.IsNullOrEmpty(categoryType)) q = q.Where(c => c.CategoryName.Contains(categoryType));
            if (distanceMin.HasValue) q = q.Where(c => c.Distance >= distanceMin.Value);
            if (distanceMax.HasValue) q = q.Where(c => c.Distance <= distanceMax.Value);

            var items = await q.Select(c => new CategoryResponse
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

            return Ok(items);
        }

        /// <summary>
        /// Public: get category by id.
        /// </summary>
        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get category (public)")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var c = await _db.Categories.FindAsync(id);
            if (c == null) return NotFound();

            var resp = new CategoryResponse
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
            };

            return Ok(resp);
        }

        /// <summary>
        /// Organiser only: create a category for an event.
        /// </summary>
        [HttpPost("/api/events/{eventId:int}/categories")]
        [Authorize(Roles = "Organiser")]
        [SwaggerOperation(Summary = "Create category (organiser only)")]
        public async Task<IActionResult> CreateCategory(int eventId, [FromBody] CategoryCreateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            if (!int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

            var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserId == userId);
            if (organiser == null) return Forbid();

            var ev = await _db.Events.FindAsync(eventId);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != organiser.Id) return Forbid();

            var cat = new Category
            {
                EventId = eventId,
                CategoryName = request.CategoryName,
                Description = request.Description,
                Distance = request.Distance,
                DistanceUnit = request.DistanceUnit,
                AgeGroupMin = request.AgeGroupMin,
                AgeGroupMax = request.AgeGroupMax,
                GenderRestriction = request.GenderRestriction,
                EntryFee = request.EntryFee,
                LateEntryFee = request.LateEntryFee,
                MaxParticipants = request.MaxParticipants,
                StartTime = request.StartTime,
                CutOffTime = request.CutOffTime
            };

            _db.Categories.Add(cat);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = cat.Id }, new { id = cat.Id });
        }

        /// <summary>
        /// Organiser only: update a category.
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        [SwaggerOperation(Summary = "Update category (organiser only)")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryUpdateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            if (!int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

            var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserId == userId);
            if (organiser == null) return Forbid();

            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();

            var ev = await _db.Events.FindAsync(cat.EventId);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != organiser.Id) return Forbid();

            cat.CategoryName = request.CategoryName;
            cat.Description = request.Description;
            cat.Distance = request.Distance;
            cat.DistanceUnit = request.DistanceUnit;
            cat.AgeGroupMin = request.AgeGroupMin;
            cat.AgeGroupMax = request.AgeGroupMax;
            cat.GenderRestriction = request.GenderRestriction;
            cat.EntryFee = request.EntryFee;
            cat.LateEntryFee = request.LateEntryFee;
            cat.MaxParticipants = request.MaxParticipants;
            cat.StartTime = request.StartTime;
            cat.CutOffTime = request.CutOffTime;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Category updated." });
        }

        /// <summary>
        /// Organiser only: delete a category.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        [SwaggerOperation(Summary = "Delete category (organiser only)")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userIdClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            if (!int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

            var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserId == userId);
            if (organiser == null) return Forbid();

            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();

            var ev = await _db.Events.FindAsync(cat.EventId);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != organiser.Id) return Forbid();

            _db.Categories.Remove(cat);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
