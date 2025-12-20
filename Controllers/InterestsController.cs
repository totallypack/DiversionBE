using Diversion.DTOs;
using Diversion.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diversion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InterestsController(DiversionDbContext context) : ControllerBase
    {
        private readonly DiversionDbContext _context = context;

        [HttpGet]
        public async Task<ActionResult<List<InterestDto>>> GetAllInterests()
        {
            var interests = await _context.Interests
                .Select(i => new InterestDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    IconUrl = i.IconUrl,
                })
                .ToListAsync();
            return Ok(interests);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InterestWithSubInterestsDto>> GetInterestById(Guid id)
        {
            var interest = await _context.Interests
                .Where(i => i.Id == id)
                .Select(i => new InterestWithSubInterestsDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    IconUrl = i.IconUrl,
                    SubInterests = i.SubInterests.Select(si => new SubInterestDto
                    {
                        Id = si.Id,
                        Name = si.Name,
                        InterestId = si.InterestId,
                        Description = si.Description,
                        IconUrl = si.IconUrl
                    }).ToList()
                }).FirstOrDefaultAsync();
            if (interest == null)
            {
                return NotFound(new { message = "Interest not found" });
            }

            return Ok(interest);
        }

        [HttpGet("with_subinterests")]
        public async Task<ActionResult<List<InterestWithSubInterestsDto>>> GetAllInterestsWithSubinterests()
        {
            var interests = await _context.Interests
                .Select(i => new InterestWithSubInterestsDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    IconUrl = i.IconUrl,
                    SubInterests = i.SubInterests.Select(si => new SubInterestDto
                    {
                        Id = si.Id,
                        Name = si.Name,
                        InterestId = si.InterestId,
                        Description = si.Description,
                        IconUrl = si.IconUrl
                    }).ToList()
                }).ToListAsync();
            return Ok(interests);
        }
    }
}
