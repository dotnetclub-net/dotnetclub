using OpenQA.Selenium;
using TechTalk.SpecFlow;

namespace Discussion.E2E.UserAccount
{
    [Binding]
    public class StepDefinitions : Steps
    {
        private IWebDriver _browser;

        public StepDefinitions()
        {
            _browser = ScenarioContext.Current["browser"] as IWebDriver;
        }

        [Given(@"I am on the dotnet club homepage")]
        public void GivenTheUserExists()
        {
            _browser.Navigate().GoToUrl("https://preview.dotnetclub.net");
        }

    }   
}