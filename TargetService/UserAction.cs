namespace TargetService
{

    public enum Action
    {
        Add,
        Remove
    }

    public class UserAction
    {
        public Action Action { get; set; }
        public User User { get; set; }
    }
}
