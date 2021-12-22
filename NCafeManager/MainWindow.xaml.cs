using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using Conditions;
using Pipelines;
using Utilities;
using Condition = Conditions.Condition;
using MessageBox = System.Windows.MessageBox;

namespace NCafeManager
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow _instance;

        public MainWindow()
        {
            InitializeComponent();

            _instance = this;
        }

        public void ClearLog()
        {
            log.Items.Clear();
        }

        public void ResetProgressBar()
        {
            __status_bar.Value = 0;
        }

        public void SetProgressBarMax(int max)
        {
            __status_bar.Maximum = max;
        }

        public void IncrementProgressBar(int v)
        {
            __status_bar.Value = __status_bar.Value + v;
        }

        public void SetProgressBarValue(int v)
        {
            __status_bar.Value = v;
        }

        public void AppendLog(string l)
        {
            log.Items.Add(l);
            log.SelectedIndex = log.Items.Count - 1;
            log.ScrollIntoView(log.SelectedItem);
        }

        public static MainWindow GetInstance()
        {
            return _instance;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (HtmlCrawler.GetInstance().GetDriver() != null)
                HtmlCrawler.GetInstance().GetDriver().Quit();
        }

        private void __keyword_add_Click(object sender, RoutedEventArgs e)
        {
            if (__input_keyword.Text.Length > 0)
            {
                if (__keywords.Items.Count == 0)
                {
                    __keywords.Items.Add(__input_keyword.Text);
                    return;
                }

                var orFlag = __keyword_or.IsChecked.Value;
                if (orFlag)
                {
                    //ADD AS OR OPERATION
                    __keywords.Items.Add("OR");
                    __keywords.Items.Add(__input_keyword.Text);
                }
                else
                {
                    //ADD AS AND OPERATION
                    __keywords.Items.Add("AND");
                    __keywords.Items.Add(__input_keyword.Text);
                }
            }
        }

        private ConditionParentPipeline CompileKeywordsConditionPipeline()
        {
            var parentPipeline = new ConditionParentPipeline();
            for (var i = 0; i < __keywords.Items.Count; i++)
                //FIRST
                if (i == 0)
                {
                    var keyword = __keywords.Items[i].ToString();
                    //예를 들어 List가
                    //A
                    //OR
                    //B
                    //일경우, 여기서 constructor로 들어가는 OperationType는 해당 키워드 바로 위의 operation으로 지정된다.
                    //지금은 첫번째 키워드이므로, 위에 operation이 존재하지 않으므로 None이다.
                    var pipe = new ConditionSingleKeywordPipeline(parentPipeline, keyword, ConditionOperationType.None);
                    parentPipeline.AddLast(pipe);
                }
                else
                {
                    if (__keywords.Items[i].ToString().Equals("OR", StringComparison.InvariantCulture) ||
                        __keywords.Items[i].ToString().Equals("AND", StringComparison.InvariantCulture))
                        continue;
                    var operation = __keywords.Items[i - 1].ToString();
                    var keyword = __keywords.Items[i].ToString();
                    var pipe = new ConditionSingleKeywordPipeline(parentPipeline, keyword, Condition.Parse(operation));
                    parentPipeline.AddLast(pipe);
                }

            if (__keyword_include.IsChecked.Value)
                parentPipeline.PipelineState = ConditionPipelineState.Include;
            if (__keyword_not_requisite.IsChecked.Value)
                parentPipeline.PipelineState = ConditionPipelineState.NotRequisite;
            if (__keyword_unused.IsChecked.Value)
                parentPipeline.PipelineState = ConditionPipelineState.Exclude;

            return parentPipeline;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!__url.Text.Trim().StartsWith(Const.BaseDomain))
            {
                MessageBox.Show("알맞은 카페 주소를 입력해주십시오.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Const.CafeDomain = __url.Text;
            if (__path.Text.Length <= 0)
            {
                MessageBox.Show("저장할 위치를 선택해주십시오.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (__menus.Items.Count <= 0)
            {
                MessageBox.Show("적어도 하나 이상의 검색할 게시판이 있어야 합니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            var articleFetchPipeline = new ParentPipeline();
            Pipeline[] articleFetchPipelines =
                { new FetchMenuPipeline(articleFetchPipeline), new FetchArticlePipeline(articleFetchPipeline) };

            foreach (var fetchPipeline in articleFetchPipelines) articleFetchPipeline.AddLast(fetchPipeline);


            var menuList = new List<string>();
            foreach (var it in __menus.Items)
                menuList.Add(it.ToString().Trim().TrimEnd());

            var task = articleFetchPipeline.Filter(menuList);
            var awaiter = task.GetAwaiter();
            awaiter.OnCompleted(delegate
            {
                var map = task.Result as Hashtable;
                var conditionPipeline = new ConditionPipelineSet();

                conditionPipeline.ConditionAuthorParentPipeline = CompileAuthorConditionPipeline();
                conditionPipeline.ConditionKeywordParentPipeline = CompileKeywordsConditionPipeline();
                conditionPipeline.ConditionTimeParentPipeline = CompileTimeConditionPipeline();

                var list = new List<KeyValuePair<ConditionPipelineSet, UrlPacker>>();

                foreach (DictionaryEntry entry in map)
                    list.Add(new KeyValuePair<ConditionPipelineSet, UrlPacker>(conditionPipeline,
                        entry.Value as UrlPacker));

                var parentPipe = new ParentPipeline();
                var articleDetailPipeline = new FetchArticleDetailsPipeline(parentPipe);
                parentPipe.AddLast(articleDetailPipeline);
                var detailTask = parentPipe.Filter(list);
                var detailAwaiter = detailTask.GetAwaiter();
                detailAwaiter.OnCompleted(delegate
                {
                    var packedArticleList = detailTask.Result as List<KeyValuePair<string, PackedArticles>>;
                    ResetProgressBar();
                    AppendLog("CSV 파일로 저장 중...");
                    AppendLog("위치: " + __path.Text);
                    var writer = new ArticleWriter(packedArticleList);
                    var t = writer.Write(__path.Text);
                    var w = t.GetAwaiter();
                    w.OnCompleted(() =>
                    {
                        MessageBox.Show("모든 작업이 완료되었습니다.", "완료", MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                    t.Start();
                    t.Wait();
                });
                detailTask.Start();
            });
            task.Start();
        }

        private ConditionParentPipeline CompileAuthorConditionPipeline()
        {
            var parentPipeline = new ConditionParentPipeline();
            foreach (var it in __authors.Items)
                parentPipeline.AddLast(new ConditionAuthorPipeline(parentPipeline, it.ToString(),
                    ConditionOperationType.Or));
            if (__author_include.IsChecked.Value)
                parentPipeline.PipelineState = ConditionPipelineState.Include;
            if (__author_not_requisite.IsChecked.Value)
                parentPipeline.PipelineState = ConditionPipelineState.NotRequisite;
            if (__author_unused.IsChecked.Value)
                parentPipeline.PipelineState = ConditionPipelineState.Exclude;
            return parentPipeline;
        }

        private ConditionParentPipeline CompileTimeConditionPipeline()
        {
            var parentPipeline = new ConditionParentPipeline();
            for (var i = 0; i < __time.Items.Count; i++)
            {
                var value = __time.Items[i].ToString();
                if (i == 0)
                {
                    var type = Condition.Parse(__time.Items[i + 1].ToString());
                    parentPipeline.AddLast(new ConditionTimePipeline(parentPipeline,
                        DateUtils.ParseDateTime(value).Value, type));
                }
                else
                {
                    if (value.Equals("after", StringComparison.OrdinalIgnoreCase) ||
                        value.Equals("before", StringComparison.OrdinalIgnoreCase))
                        continue;
                    var type = Condition.Parse(__time.Items[i + 1].ToString());
                    parentPipeline.AddLast(new ConditionTimePipeline(parentPipeline,
                        DateUtils.ParseDateTime(value).Value, type));
                }
            }

            if (__time_include.IsChecked.Value)
                parentPipeline.PipelineState = ConditionPipelineState.Include;
            if (__time_not_requisite.IsChecked.Value)
                parentPipeline.PipelineState = ConditionPipelineState.NotRequisite;
            if (__time_unused.IsChecked.Value)
                parentPipeline.PipelineState = ConditionPipelineState.Exclude;
            return parentPipeline;
        }

        private void __author_add_Click(object sender, RoutedEventArgs e)
        {
            if (__input_author.Text.Length > 0) __authors.Items.Add(__input_keyword.Text);
        }

        private void __time_add_Click(object sender, RoutedEventArgs e)
        {
            if (__input_time.Text.Length > 0)
            {
                var time = DateUtils.ParseDateTime(__input_time.Text);
                if (time.HasValue)
                {
                    if (__time_after.IsChecked.Value)
                    {
                        __time.Items.Add(__input_time.Text);
                        __time.Items.Add("After");
                    }
                    else
                    {
                        __time.Items.Add(__input_time.Text);
                        __time.Items.Add("Before");
                    }
                }
                else
                {
                    MessageBox.Show("Incorrect date format", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void __add_menu_Click(object sender, RoutedEventArgs e)
        {
            if (__menu.Text.Length > 0)
            {
                if (__menus.Items.Contains(__menu.Text))
                {
                    MessageBox.Show("이미 해당 게시판이 추가되어있습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                __menus.Items.Add(__menu.Text);
            }
        }

        private void __keyword_include_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void __browse_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) __path.Text = dialog.SelectedPath;
            }
        }
    }
}