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
    }
}
