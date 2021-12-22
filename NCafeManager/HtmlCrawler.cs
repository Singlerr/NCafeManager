using OpenQA.Selenium.Edge;

namespace Pipelines
{
    internal class HtmlCrawler
    {
        private static HtmlCrawler _instance;
        private readonly EdgeDriverService _driverService;
        private readonly EdgeOptions _options;
        private EdgeDriver _driver;

        private HtmlCrawler()
        {
            _driverService = EdgeDriverService.CreateDefaultService();
            _driverService.HideCommandPromptWindow = true;

            _options = new EdgeOptions();
            _options.AddArgument("disable-gpu");
            _options.AddArgument("headless");
            InitializeDriver();
        }

        public void InitializeDriver()
        {
            _driver = new EdgeDriver(_driverService, _options);
        }

        public void Navigate(string url)
        {
            _driver.Navigate().GoToUrl(url);
        }


        public void Exit()
        {
            _driver.Close();
            _driver.Quit();
            _instance = null;
        }

        public EdgeDriver GetDriver()
        {
            return _driver;
        }

        public static HtmlCrawler GetInstance()
        {
            if (_instance == null)
                return _instance = new HtmlCrawler();
            return _instance;
        }
    }
}