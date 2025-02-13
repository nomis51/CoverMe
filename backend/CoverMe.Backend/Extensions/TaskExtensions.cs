namespace CoverMe.Backend.Extensions;

public static class TaskExtensions
{
    #region Public methods

    public static async Task WaitUntil(Func<bool> condition, Action action, int delay = 100)
    {
        while (!condition.Invoke())
        {
            await Task.Delay(delay);
        }

        action.Invoke();
    }

    #endregion
}