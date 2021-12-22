using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Pipelines;

namespace NCafeManager
{
    internal class ArticleWriter
    {
        private readonly List<KeyValuePair<string, PackedArticles>> _packedArticles;

        public ArticleWriter(List<KeyValuePair<string, PackedArticles>> packedArticles)
        {
            _packedArticles = packedArticles;
        }

        private string EscapeIllegalCSVCharacters(string str)
        {
            var data = str;
            if (data.Contains("\""))
            {
                data = data.Replace("\"", "\"\"");
                data = string.Format("\"{0}\"", data);
            }
            else if (data.Contains(",") || data.Contains(Environment.NewLine))
            {
                data = string.Format("\"{0}\"", data);
            }

            return data;
        }

        public Task Write(string parentPath)
        {
            return new Task(delegate
            {
                _packedArticles.ForEach(pair =>
                {
                    var menuName = pair.Key;
                    var packedArticles = pair.Value;
                    var filteredMenuName = menuName;
                    foreach (var invalidChar in Path.GetInvalidPathChars())
                        filteredMenuName = filteredMenuName.Replace(invalidChar.ToString(), "");

                    var path = parentPath + "/" + filteredMenuName + ".csv";
                    using (var writer = new StreamWriter(path, false, Encoding.UTF8))
                    {
                        writer.WriteLine("제목,작성자,작성 시간,URL");
                        packedArticles.Articles.ForEach(article =>
                        {
                            writer.WriteLine("{0},{1},{2},{3}", EscapeIllegalCSVCharacters(article.Title),
                                EscapeIllegalCSVCharacters(article.Author), article.Time, article.PathUrl);
                        });
                        writer.Flush();
                        writer.Close();
                    }
                });
            });
        }
    }
}