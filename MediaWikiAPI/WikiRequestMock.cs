using System.Collections.Generic;

namespace MediaWikiAPI
{
    public class WikiRequestMock : IWikiRequest
    {
        public WikiResponse Send(string method, string baseUrl, Dictionary<string, string> parameters = null, Dictionary<string, string> headers = null, Dictionary<string, string> cookies = null)
        {
            WikiResponse response = new WikiResponse();
            response.Status = "OK";

            if (parameters["action"] == "query")
            {
                if (parameters.ContainsKey("rvprop"))
                {
                    if (parameters["rvprop"] == "content")
                    {
                        response.Content = "{\"query\":{\"pages\":{\"1\":{\"pageid\":1,\"ns\":0,\"title\":\"Test\",\"revisions\":[{\"*\":\"Hello World!\"}]}}}}";
                    }
                }

                if (parameters.ContainsKey("intoken"))
                {
                    if (parameters["intoken"] == "edit")
                    {
                        response.Content = "{\"query\":{\"pages\":{\"-1\":{\"ns\":0,\"title\":\"Test2\",\"missing\":\"\",\"starttimestamp\":\"2014-11-21T00:46:40Z\",\"edittoken\":\"+\\\\\"}}}}";
                    }
                    else if (parameters["intoken"] == "delete")
                    {
                        response.Content = "{\"query\":{\"pages\":{\"8\":{\"pageid\":8,\"ns\":0,\"title\":\"Test2\",\"touched\":\"2014-11-21T00:49:02Z\",\"lastrevid\":24,\"counter\":0,\"length\":11,\"starttimestamp\":\"2014-11-21T00:49:07Z\",\"deletetoken\":\"491f3ee27f127e29e2391e015b593df4+\\\\\"}}}}";
                    }
                }
            }

            if (parameters["action"] == "login")
            {
                if (parameters.ContainsKey("lgtoken"))
                {
                    response.Content = "{\"login\":{\"result\":\"Success\",\"lguserid\":2,\"lgusername\":\"Root\",\"lgtoken\":\"6d878daa1e17d282e1587312531af82f\",\"cookieprefix\":\"mediawiki\",\"sessionid\":\"tch5b5ch34fcr5c8pf11lqerh2\"}}";
                }
                else
                {
                    response.Content = "{\"login\":{\"result\":\"NeedToken\",\"token\":\"05041f6825fee077ae136a223c6fe36a\",\"cookieprefix\":\"mediawiki\",\"sessionid\":\"j8ev98c523544sa3jvb6s92er6\"}}";
                }
            }

            if (parameters["action"] == "edit")
            {
                response.Content = "{\"edit\":{\"result\":\"Success\",\"pageid\":5,\"title\":\"Test2\",\"oldrevid\":17,\"newrevid\":18}}";
            }

            if (parameters["action"] == "delete")
            {
                response.Content = "{\"delete\":{\"title\":\"Test2\",\"reason\":\"contentwas:\\\"ContentofTest2..1.1.1.1.1.1.1.1\\\"(andtheonlycontributorwas\\\"[[Special:Contributions/192.168.56.1|192.168.56.1]]\\\")\"}}";
            }

            return response;
        }
    }
}
