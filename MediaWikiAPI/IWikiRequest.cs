using System.Collections.Generic;

namespace MediaWikiAPI
{
    public interface IWikiRequest
    {
        WikiResponse Send(string method, string baseUrl, Dictionary<string, string> parameters = null, 
            Dictionary<string, string> headers = null, Dictionary<string, string> cookies = null);
    }
}
