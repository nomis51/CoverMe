namespace CoverMe.Backend.Playwright.Tests;

[Parallelizable(ParallelScope.Self)]
[Order(-1)]
[TestFixture]
public class Configuration : PageTest
{
    #region Setup

    [OneTimeSetUp]
    public void GlobalSetup()
    {
        SetDefaultExpectTimeout(Constants.GlobalTimeout);
    }

    #endregion
}