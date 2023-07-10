namespace unfollowers.Services.CronService
{
    public interface ICronService
    {
        Task CheckNewUnfollowers();
    }
}
