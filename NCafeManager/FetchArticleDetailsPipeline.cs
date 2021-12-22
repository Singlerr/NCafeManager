using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Conditions;
using NCafeManager;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Utilities;

namespace Pipelines
{
    internal class FetchArticleDetailsPipeline : Pipeline
    {
        public FetchArticleDetailsPipeline(ParentPipeline pipeline) : base(pipeline)
        {
        }

        public override Task<object> DoFilter(int step, object preFiltered)
        {
            return new Task<object>(delegate
            {
                if (preFiltered is List<KeyValuePair<ConditionPipelineSet, UrlPacker>> pairs)
                {
                    MainWindow.GetInstance().Dispatcher.Invoke(() => MainWindow.GetInstance().ResetProgressBar());
                    var client = HtmlCrawler.GetInstance();
                    var packedArticleList = new List<KeyValuePair<string, PackedArticles>>();
                    foreach (var pair in pairs)
                    {
                        var packer = pair.Value;
                        var cond = pair.Key;

                        foreach (var packedUrls in packer.GetUrls())
                        {
                            var packedArticles = new PackedArticles();


                            var driverWait = new WebDriverWait(client.GetDriver(), new TimeSpan(0, 0, 30));

                            var menuName = packedUrls.Name;
                            var basePageUrl = packedUrls.Url;
                            var k = 0;
                            IEnumerable<IWebElement> nodes;
                            packedArticles.MenuName = menuName;
                            do
                            {
                                var pageUrl = UrlUtils.Join(basePageUrl, k.ToString());
                                client.Navigate(pageUrl);
                                client.GetDriver().SwitchTo().Frame("cafe_main");
                                var rootNode =
                                    driverWait.Until(
                                        ExpectedConditions.ElementExists(
                                            By.CssSelector("#main-area > div:nth-child(6) > table > tbody")));

                                var noDataElem = rootNode.FindElements(By.ClassName("nodata"));
                                if (noDataElem.Count > 0) break;
                                nodes = rootNode.FindElements(By.TagName("tr"));
                                MainWindow.GetInstance().Dispatcher.Invoke(delegate
                                {
                                    MainWindow.GetInstance().AppendLog("FETCHING DETAILS");
                                    MainWindow.GetInstance().ResetProgressBar();
                                    MainWindow.GetInstance().SetProgressBarMax(nodes.Count());
                                });
                                foreach (var articleNode in nodes)
                                {
                                    MainWindow.GetInstance().Dispatcher.Invoke(() =>
                                        MainWindow.GetInstance().IncrementProgressBar(1));
                                    try
                                    {
                                        articleNode.FindElement(By.ClassName("article"));
                                    }
                                    catch (Exception ex)
                                    {
                                        continue;
                                    }

                                    var titleNode = articleNode.FindElement(By.ClassName("article"));
                                    var title = titleNode.Text;
                                    var dateNode = articleNode.FindElement(By.XPath(".//td[@class='td_date']"));

                                    var authorNode = articleNode.FindElement(By.XPath(".//a[@href='#']"));

                                    var date = dateNode.Text;
                                    DateTime dateTime;
                                    try
                                    {
                                        dateTime = DateTime.ParseExact(date, "yyyy.MM.dd.",
                                            CultureInfo.InvariantCulture);
                                    }
                                    catch (Exception ex)
                                    {
                                        dateTime = DateTime.ParseExact(date, "HH:mm",
                                            CultureInfo.InvariantCulture);
                                    }

                                    var author = authorNode.Text;
                                    var condition = new Condition(title, dateTime
                                        , author,
                                        menuName);
                                    MainWindow.GetInstance().Dispatcher.Invoke(delegate
                                    {
                                        MainWindow.GetInstance().AppendLog("FIND = " + condition);
                                    });
                                    var keywordCond = false;
                                    var authorCond = false;
                                    var timeCond = false;

                                    if (cond.ConditionKeywordParentPipeline != null)
                                    {
                                        cond.ConditionKeywordParentPipeline.Condition = condition;
                                        keywordCond = cond.ConditionKeywordParentPipeline.CheckConditions();
                                        if (!keywordCond && cond.ConditionKeywordParentPipeline.PipelineState ==
                                            ConditionPipelineState.Include)
                                        {
                                            cond.Reset();
                                            continue;
                                        }
                                    }

                                    if (cond.ConditionAuthorParentPipeline != null)
                                    {
                                        cond.ConditionAuthorParentPipeline.Condition = condition;
                                        authorCond = cond.ConditionAuthorParentPipeline.CheckConditions();
                                        if (!authorCond && cond.ConditionAuthorParentPipeline.PipelineState ==
                                            ConditionPipelineState.Include)
                                        {
                                            cond.Reset();
                                            continue;
                                        }
                                    }

                                    if (cond.ConditionTimeParentPipeline != null)
                                    {
                                        cond.ConditionTimeParentPipeline.Condition = condition;
                                        timeCond = cond.ConditionTimeParentPipeline.CheckConditions(true);
                                        if (!timeCond && cond.ConditionTimeParentPipeline.PipelineState ==
                                            ConditionPipelineState.Include)
                                        {
                                            cond.Reset();
                                            continue;
                                        }
                                    }

                                    if (keywordCond || authorCond || timeCond ||
                                        cond.ConditionKeywordParentPipeline == null &&
                                        cond.ConditionAuthorParentPipeline == null &&
                                        cond.ConditionTimeParentPipeline == null)
                                    {
                                        var article = new Article();
                                        article.Author = author;
                                        article.Time = dateTime;
                                        article.Title = title;
                                        article.MenuName = menuName;
                                        article.PathUrl = titleNode.GetAttribute("href");
                                        packedArticles.Articles.Add(article);
                                        MainWindow.GetInstance().Dispatcher.Invoke(delegate
                                        {
                                            MainWindow.GetInstance().AppendLog("SELECTED = " + title);
                                        });
                                    }

                                    cond.Reset();
                                }

                                k++;
                            } while (nodes != null && nodes.Any());

                            packedArticleList.Add(new KeyValuePair<string, PackedArticles>(menuName, packedArticles));
                            MainWindow.GetInstance().Dispatcher
                                .Invoke(delegate { MainWindow.GetInstance().ClearLog(); });
                        }
                    }


                    return packedArticleList;
                }

                return null;
            });
        }
    }
}