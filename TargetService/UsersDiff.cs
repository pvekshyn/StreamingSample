namespace TargetService;

public static class UsersDiff
{
    public static IEnumerable<UserAction> Execute(IEnumerable<User> source, IEnumerable<User> target)
    {
        var sourceEnumerator = source.GetEnumerator();
        var targetEnumerator = target.GetEnumerator();

        var sourceHasNext = sourceEnumerator.MoveNext();
        var targetHasNext = targetEnumerator.MoveNext();

        while (sourceHasNext || targetHasNext)
        {
            var sourceUser = sourceEnumerator.Current;
            var targetUser = targetEnumerator.Current;

            if (sourceUser?.Id == targetUser?.Id)
            {
                sourceHasNext = sourceEnumerator.MoveNext();
                targetHasNext = targetEnumerator.MoveNext();
            }
            else if (sourceUser?.Id < targetUser?.Id || !targetHasNext)
            {
                sourceHasNext = sourceEnumerator.MoveNext();
                yield return new UserAction { Action = Action.Add, User = sourceUser };
            }
            else if (sourceUser?.Id > targetUser?.Id || !sourceHasNext)
            {
                targetHasNext = targetEnumerator.MoveNext();
                yield return new UserAction { Action = Action.Remove, User = targetUser };
            }
        }
    }
}
