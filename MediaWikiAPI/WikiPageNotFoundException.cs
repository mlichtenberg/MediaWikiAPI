using System;

namespace MediaWikiAPI
{
    [Serializable]
    public class WikiPageNotFoundException : Exception
    {
        public string ErrorMessage
        {
            get { return base.Message.ToString(); }
        }

        public WikiPageNotFoundException (string errorMessage) : base(errorMessage) { }
        public WikiPageNotFoundException(string errorMessage, Exception innerEx) : base(errorMessage, innerEx) { }
    }
}
