using System;
using System.Collections;
using System.Threading.Tasks;
using NCafeManager;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Utilities;

namespace Pipelines
{
    internal class FetchArticlePipeline : Pipeline
    {
        public FetchArticlePipeline(ParentPipeline pipeline) : base(pipeline)
        {
        }

        public override Task<object> DoFilter(int step, object preFiltered)
        {
            return new Task<object>(delegate
            {
                MainWindow.GetInstance().Dispatcher.Invoke(() => MainWindow.GetInstance().ResetProgressBar());
                if (preFiltered is UrlPacker packer)
                {
                    MainWindow.GetInstance().Dispatcher.Invoke(delegate
                    {
                        MainWindow.GetInstance().AppendLog("FETCHING ARTICLES");
                    });
                    var packerMap = new Hashtable();

                    var client = HtmlCrawler.GetInstance();
                    var waiter = new WebDriverWait(client.GetDriver(), new TimeSpan(0, 0, 30));
                    MainWindow.GetInstance().Dispatcher.Invoke(() =>
                        MainWindow.GetInstance().SetProgressBarMax(packer.GetUrlList().Count));
                    foreach (var packedUrl in packer.GetUrls())
                    {
                        var urlPacker = new UrlPacker();
                        client.Navigate(packedUrl.Url);
                        client.GetDriver().SwitchTo().Frame("cafe_main");
                        var rootNode =
                            waiter.Until(
                                ExpectedConditions.ElementToBeClickable(By.CssSelector("#main-area > div.prev-next")));
                        var node = rootNode.FindElement(By.XPath("//a[@class='on']"));
                        var path = node.GetAttribute("href");
                        var pageUrl = path.Remove(path.Length - 1);
                        urlPacker.Add(new PackedUrl(packedUrl.Name, pageUrl));
                        packerMap[packedUrl.Name] = urlPacker;
                        MainWindow.GetInstance().Dispatcher
                            .Invoke(() => MainWindow.GetInstance().IncrementProgressBar(1));
                    }

                    MainWindow.GetInstance().Dispatcher.Invoke(delegate { MainWindow.GetInstance().ClearLog(); });
                    return packerMap;
                }

                return null;
            });
        }
    }
}