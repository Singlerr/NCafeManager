using System.Collections.Generic;

namespace Pipelines
{
    internal class PackedArticles
    {
        public List<Article> Articles;
        public string MenuName;

        public PackedArticles()
        {
            Articles = new List<Article>();
        }
    }
}