using System.IO;
using Discussion.E2E.UserAccount;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TechTalk.SpecFlow;

namespace Discussion.E2E
{
    [Binding]
    public sealed class TestBrowser
    {
        private IWebDriver _browser;

        [BeforeScenario]
        public void BeforeScenario()
        {
            var assemblyPath = typeof(StepDefinitions).Assembly.Location;
            
            _browser = new ChromeDriver(Path.GetDirectoryName(assemblyPath));
            ScenarioContext.Current["browser"] = _browser;
        }

        [AfterScenario]
        public void AfterScenario()
        {
            _browser.Quit();
        }
    }

}