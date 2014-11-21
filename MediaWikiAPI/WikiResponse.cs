namespace MediaWikiAPI
{
    public class WikiResponse
    {
        private string _status = string.Empty;

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        private string _content = string.Empty;

        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }
    }
}
