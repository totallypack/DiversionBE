using Diversion.DTOs;
using Diversion.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diversion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubInterestsController(DiversionDbContext context) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubInterestDto>>> GetSubInterests()
        {
            var subInterests = await _context.SubInterests
                .Select(si => new SubInterestDto
                {
                    Id = si.Id,
                    Name = si.Name,
                    InterestId = si.InterestId,
                    Description = si.Description,
                    IconUrl = si.IconUrl
                }).ToListAsync();

            return Ok(subInterests);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubInterestWithInterestDto>> GetSubInterest(Guid id)
        {
            var subInterest = await _context.SubInterests
                .Include(si => si.Interest)
                .Where(si  => si.Id == id)
                .Select(si => new SubInterestWithInterestDto
                {
                    Id = si.Id,
                    Name = si.Name,
                    Description = si.Description,
                    IconUrl = si.IconUrl,
                    Interest = new InterestDto
                    {
                        Id = si.Interest.Id,
                        Name = si.Interest.Name,
                        Description = si.Interest.Description,
                        IconUrl = si.Interest.IconUrl
                    }
                }).FirstOrDefaultAsync();

            if (subInterest == null)
                return NotFound();
            return Ok(subInterest);
        }

        [HttpGet("interest/{interestId}")]
        public async Task<ActionResult<IEnumerable<SubInterestDto>>> GetSubInterestsByInterest(Guid interestId)
        {
            var subInterests = await _context.SubInterests
                .Where(si => si.InterestId == interestId)
                .Select(si => new SubInterestDto
                {
                    Id = si.Id,
                    Name = si.Name,
                    InterestId = si.InterestId,
                    Description = si.Description,
                    IconUrl = si.IconUrl
                })
                .ToListAsync();

            return Ok(subInterests);
        }
    }
}
