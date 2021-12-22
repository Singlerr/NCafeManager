using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NCafeManager;
using OpenQA.Selenium;
using Utilities;

namespace Pipelines
{
    internal class FetchMenuPipeline : Pipeline
    {
        public FetchMenuPipeline(ParentPipeline parentPipeline) : base(parentPipeline)
        {
        }

        public override Task<object> DoFilter(int step, object preFiltered)
        {
            return new Task<object>(delegate
            {

                MainWindow.GetInstance().Dispatcher.Invoke(() =>
                {
                    MainWindow.GetInstance().ResetProgressBar();
                    MainWindow.GetInstance().AppendLog("작업을 시작하는 중...");
                });

                var urlPacker = new UrlPacker();

                var requestedMenus = preFiltered as List<string>;

                if (requestedMenus == null)
                    throw new NullReferenceException();


                var client = HtmlCrawler.GetInstance();
                client.Navigate(Const.CafeDomain);

                var rootNode = client.GetDriver().FindElement(By.XPath("//div[@class='box-g-m']"));

                var menuNodes = rootNode.FindElements(By.XPath("//a[@target='cafe_main']"));
                MainWindow.GetInstance().Dispatcher.Invoke(() => MainWindow.GetInstance().SetProgressBarMax(menuNodes.Count));
                foreach (var menuNode in menuNodes)
                {
                    var menuName = menuNode.Text;
                    MainWindow.GetInstance().Dispatcher.Invoke(delegate
                    {
                        MainWindow.GetInstance().AppendLog("FOUND MENU = " + menuName);
                        MainWindow.GetInstance().IncrementProgressBar(1);
                    });

                    if (!requestedMenus.Contains(menuName))
                        continue;
                    var menuUrl = menuNode.GetAttribute("href");

                    urlPacker.Add(new PackedUrl(menuName, menuUrl));
                    MainWindow.GetInstance().Dispatcher.Invoke(delegate
                    {
                        MainWindow.GetInstance().AppendLog("SELECTED MENU = " + menuName);
                    });
                }

                MainWindow.GetInstance().Dispatcher.Invoke(delegate
                {
                    MainWindow.GetInstance().ClearLog();
                });
                return urlPacker;
            });
        }
    }
}