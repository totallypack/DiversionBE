namespace Diversion.DTOs;

public class SearchResultsDto
{
    public List<EventDto> Events { get; set; }
    public List<UserSearchDto> Users { get; set; }
    public List<CommunityDto> Communities { get; set; }
}
