using System.Collections.Generic;

namespace Utilities
{
    internal class UrlPacker
    {
        private readonly List<PackedUrl> _packedUrls;


        public UrlPacker()
        {
            _packedUrls = new List<PackedUrl>();
        }

        public void Pack(IEnumerable<PackedUrl> urls)
        {
            _packedUrls.AddRange(urls);
        }

        public void Add(PackedUrl url)
        {
            _packedUrls.Add(url);
        }

        public IEnumerable<PackedUrl> GetUrls()
        {
            return _packedUrls;
        }
        public List<PackedUrl> GetUrlList()
        {
            return _packedUrls;
        }
    }

    public class PackedUrl
    {
        public string Name;
        public string Url;

        public PackedUrl(string name, string url)
        {
            Name = name;
            Url = url;
        }
    }

    public class UrlUtils
    {
        public static string Join(string domain, string path)
        {
            return domain + path;
        }
    }
}