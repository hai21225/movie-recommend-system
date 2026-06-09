
public class InteractionService: IInteractionService
{
    private readonly IInteractionRepository _interactionRepository;
    public InteractionService(IInteractionRepository interactionRepository)
    {
        _interactionRepository = interactionRepository;
    }

    public async Task<List<UserInteractionsDto>> GetUserInteractionHistory(int userId)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("Invalid user ID");
        }
       var list = await _interactionRepository.GetUserInteractionHistory(userId);
        return list.Select(i => new UserInteractionsDto
        {
              ShowId = i.ShowId,
              IsLiked = i.Liked,
              InteractedAt = i.Timestamp,
         }).ToList();
    }

    public async Task<bool> IsContentLikedByUser(int userId, string showId)
    {
        if ( userId<=0 || showId=="")
        {
            return false;
        }
        return await _interactionRepository.IsContentLikedByUser(userId, showId);
    }

    public async Task<bool> SetLikeStatus(int userId, string showId, bool isLiked)
    {
        if (userId <= 0 || showId == "")
        {
            return false;
        }
        return await _interactionRepository.SetLikeStatus(userId, showId, isLiked);
    }
}