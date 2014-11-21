using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;

namespace MediaWikiAPI
{
    public class Wiki
    {
        // Base address of the wiki
        private string _baseAddress = "http://localhost";
        private string _apiBase = "/api.php";

        // HttpRequest class to use to interact with wiki.
        private IWikiRequest _wikiRequest = null;

        // Credentials set when logged in to the wiki.
        private string _lgToken = string.Empty;
        private string _lgUserId = string.Empty;
        private string _lgUserName = string.Empty;
        private string _cookiePrefix = string.Empty;
        private string _sessionId = string.Empty;

        public Wiki(string baseAddress)
        {
            _baseAddress = baseAddress + _apiBase;
            _wikiRequest = new WikiRequest();
        }

        public Wiki(string baseAddress, IWikiRequest wikiRequest)
        {
            _baseAddress = baseAddress + _apiBase;
            _wikiRequest = wikiRequest;
        }

        /// <summary>
        /// Log in to the wiki.  If successful, set the session credentials.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void Login(string username, string password)
        {
            string token = string.Empty;
            string session = string.Empty;
            string cookiePrefix = string.Empty;

            // Get a session ID for the login operation
            Dictionary<string, string> postData = new Dictionary<string, string>();
            postData.Add("action", "login");
            postData.Add("lgname", username);
            postData.Add("lgpassword", password);
            postData.Add("format", "json");
            WikiResponse response = _wikiRequest.Send("POST", _baseAddress, postData);

            // Read the response to determine the outcome of the login operation
            if (response.Status != "OK") throw new Exception("Error logging in to wiki");

            // Parse the JSON response to determine success or failure of the login operation
            JsonReader reader = new JsonTextReader(new StringReader(response.Content));
            reader.DateParseHandling = DateParseHandling.None;
            JObject json = JObject.Load(reader);

            /*
             * Example JSON
             * 
             * {
             *  "login": {
             *  "result": "NeedToken",
             *  "token": "05041f6825fee077ae136a223c6fe36a",
             *  "cookieprefix": "mediawiki",
             *  "sessionid": "j8ev98c523544sa3jvb6s92er6"
             *  }
             * }
             */

            if (json["error"] != null)
            {
                string errorCode = json["error"]["code"].Value<string>();
                string errorInfo = json["error"]["info"].Value<string>();
                throw new Exception(string.Format("Error logging in: {0} - {1}", errorCode, errorInfo));
            }
            else if (json["login"] != null)
            {
                string result = json["login"]["result"].Value<string>();
                if (result != "NeedToken") throw new Exception(string.Format("Unknown result value logging in: {0}", result));

                token = json["login"]["token"].Value<string>();
                session = json["login"]["sessionid"].Value<string>();
                cookiePrefix = json["login"]["cookieprefix"].Value<string>();
            }
            else
            {
                throw new Exception(string.Format("Unknown response logging in: {0}", response.Content));
            }

            // Complete the login operation and (if successful) set the session credentials
            postData = new Dictionary<string, string>();
            postData.Add("action", "login");
            postData.Add("lgname", username);
            postData.Add("lgpassword", password);
            postData.Add("lgtoken", token);
            postData.Add("format", "json");

            Dictionary<string, string> headerData = new Dictionary<string, string>();
            headerData.Add("Cookie", string.Format("{0}_session={1};path=/;HttpOnly", cookiePrefix, session));
            response = _wikiRequest.Send("POST", _baseAddress, postData, headerData);

            // Read the response to determine the outcome of the login operation
            if (response.Status != "OK") throw new Exception("Error logging in to wiki");

            // Parse the JSON response to determine success or failure of the login operation
            reader = new JsonTextReader(new StringReader(response.Content));
            reader.DateParseHandling = DateParseHandling.None;
            json = JObject.Load(reader);

            /*
             * Example JSON
             * 
             * {
             *  "login": {
             *    "result": "Success",
             *    "lguserid": 2,
             *    "lgusername": "Root",
             *    "lgtoken": "6d878daa1e17d282e1587312531af82f",
             *    "cookieprefix": "mediawiki",
             *    "sessionid": "tch5b5ch34fcr5c8pf11lqerh2"
             *  }
             * }
             */

            if (json["error"] != null)
            {
                string errorCode = json["error"]["code"].Value<string>();
                string errorInfo = json["error"]["info"].Value<string>();
                throw new Exception(string.Format("Error logging in: {0} - {1}", errorCode, errorInfo));
            }
            else if (json["login"] != null)
            {
                string result = json["login"]["result"].Value<string>();
                if (result != "Success") throw new Exception(string.Format("Unknown result value logging in: {0}", result));

                _lgUserId = json["login"]["lguserid"].Value<string>();
                _lgUserName = json["login"]["lgusername"].Value<string>();
                _lgToken = json["login"]["lgtoken"].Value<string>();
                _sessionId = json["login"]["sessionid"].Value<string>();
                _cookiePrefix = json["login"]["cookieprefix"].Value<string>();
            }
            else
            {
                throw new Exception(string.Format("Unknown response logging in: {0}", response.Content));
            }
        }

        /// <summary>
        /// Query the wiki for the content of the specified page.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public string GetContent(string title)
        {
            string content = string.Empty;

            Dictionary<string, string> queryString = new Dictionary<string, string>();
            queryString.Add("action", "query");
            queryString.Add("prop", "revisions");
            queryString.Add("rvprop", "content");
            queryString.Add("format", "json");
            queryString.Add("titles", title);
            WikiResponse response = _wikiRequest.Send("GET", _baseAddress, queryString);

            string jsonContent = response.Content;

            // Parse content from JSON response using LINQ-to-JSON provided by Newtonsoft's JSON.Net
            JsonReader reader = new JsonTextReader(new StringReader(response.Content));
            reader.DateParseHandling = DateParseHandling.None;
            JObject json = JObject.Load(reader);

            if (json["error"] != null)
            {
                string errorCode = json["error"]["code"].Value<string>();
                string errorInfo = json["error"]["info"].Value<string>();
                throw new Exception(string.Format("Error retrieving content for page '{0}': {1} - {2}", title, errorCode, errorInfo));
            }
            else if (json["query"] != null)
            {
                // Example JSON containing page content
                /*
                 * {"query":
                 *      {"pages":
                 *          {"1":
                 *              {"pageid":1,
                 *               "ns":0,
                 *               "title":"Test",
                 *               "revisions":[{"*":"Hello World!"}]
                 *              }
                 *          }
                 *      }
                 *  }
                 */
                // Example JSON for a missing page
                /*
                 * {"query": 
                 *      {"pages": 
                 *          {"-1": 
                 *              {"ns": 0,
                 *               "title": "Test",
                 *               "missing": ""
                 *              }
                 *          }
                 *      }
                 *  }
                 *
                 */
                if (json["query"]["pages"].First().First()["missing"] == null)
                {
                    content = json["query"]["pages"].First().First()["revisions"].First()["*"].Value<string>();
                }
                else
                {
                    throw new WikiPageNotFoundException(string.Format("Page '{0}' not found", title));
                }
            }
            else
            {
                throw new Exception(string.Format("Unknown response getting content for page '{0}': {1}", title, response.Content));
            }
            
            return content;
        }

        /// <summary>
        /// Add a new page or edit an existing page.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns>Enum value indicating whether a page was added, updated, or unchanged.  An error is thrown for any other result.</returns>
        public EditPageResponse EditPage(string title, string content)
        {
            EditPageResponse pageResponse = EditPageResponse.PageUpdated;
            bool newPage = false;
            string editToken = string.Empty;
            string startTimestamp = string.Empty;

            // Get an edit token and timestamp (if the page exists) for the edit operation
            Dictionary<string, string> queryString = new Dictionary<string, string>();
            queryString.Add("action", "query");
            queryString.Add("prop", "info|revisions");
            queryString.Add("intoken", "edit");
            queryString.Add("format", "json");
            queryString.Add("titles", title);

            WikiResponse response = _wikiRequest.Send("GET", _baseAddress, queryString);
            if (response.Status != "OK") throw new Exception("Error getting edit token from wiki");

            // Parse content from JSON response using LINQ-to-JSON provided by Newtonsoft's JSON.Net
            // We can't simply use... 
            //      JObject json = JObject.Parse(jsonContent);
            // ... because it seralizes dates from the raw YYYY-MM-DDTHH:MM:SSZ format into MM/DD/YYYY HH:MM:SS.
            // If we have to pass the date to another MediaWiki API method, that is a problem because 
            // MediaWiki expects the raw format. So, we set up a JsonReader with date parsing disabled.
            JsonReader reader = new JsonTextReader(new StringReader(response.Content));
            reader.DateParseHandling = DateParseHandling.None;
            JObject json = JObject.Load(reader);

            if (json["error"] != null)
            {
                string errorCode = json["error"]["code"].Value<string>();
                string errorInfo = json["error"]["info"].Value<string>();
                throw new Exception(string.Format("Error getting edit token: {0} - {1}", errorCode, errorInfo));
            }
            else if (json["query"] != null)
            {
                // Example JSON response for an edittoken query:
                /*
                 * {"query":
                 *      {"pages":
                 *          {"1":
                 *              {"pageid":1,
                 *               "ns":0,
                 *               "title":"Test",
                 *               "touched":"2014-05-01T19:23:37Z",
                 *               "lastrevid":1,
                 *               "counter":1,
                 *               "length":12,
                 *               "new":"",
                 *               "starttimestamp":"2014-05-13T03:12:06Z",
                 *               "edittoken":"+\\",
                 *               "revisions":[
                 *	                {"revid":1,
                 *	                 "user":"192.168.56.1",
                 *	                 "anon":"",
-                 *	                 "timestamp":"2014-05-01T19:23:37Z",
                 *	                 "comment":"Created page with \"Hello World!\""
                 *	                }]
                 *               }
                 *          }
                 *      }
                 *  }
                 * 
                 */
                newPage = !(json["query"]["pages"].First.First()["missing"] == null);
                editToken = json["query"]["pages"].First().First()["edittoken"].Value<string>();
                startTimestamp = (json["query"]["pages"].First().First()["touched"] != null) ?
                    json["query"]["pages"].First().First()["touched"].Value<string>() :
                    json["query"]["pages"].First().First()["starttimestamp"].Value<string>();
            }
            else
            {
                throw new Exception(string.Format("Unknown response getting edit token: {0}", response.Content));
            }

            // Send the edit to the Mediawiki instance
            Dictionary<string, string> postData = new Dictionary<string, string>();
            postData.Add("action", "edit");
            postData.Add("title", title);
            postData.Add("text", content);
            postData.Add("basetimestamp", startTimestamp);
            postData.Add("format", "json");
            postData.Add("token", System.Net.WebUtility.UrlEncode(editToken));
            response = _wikiRequest.Send("POST", _baseAddress, postData);

            // Read the response to determine the outcome of the edit operation
            if (response.Status != "OK") throw new Exception("Error posting edit to wiki");

            // Parse the JSON response to determine success or failure of the edit operation
            reader = new JsonTextReader(new StringReader(response.Content));
            reader.DateParseHandling = DateParseHandling.None;
            json = JObject.Load(reader);

            if (json["error"] != null)
            {
                string errorCode = json["error"]["code"].Value<string>();
                string errorInfo = json["error"]["info"].Value<string>();
                throw new Exception(string.Format("Error editing the page: {0} - {1}", errorCode, errorInfo));
            }
            else if (json["edit"] != null)
            {
                // Example JSON response for an edit operation
                /*
                 * {
                 *  "edit": {
                 *    "result": "Success",
                 *    "pageid": 5,
                 *    "title": "Test2",
                 *    "oldrevid": 17,
                 *    "newrevid": 18
                 *  }
                 * }
                 */
                string result = json["edit"]["result"].Value<string>();
                if (result != "Success") throw new Exception(string.Format("Unknown result value editing the page: {0}", result));

                if (json["edit"]["new"] != null) pageResponse = EditPageResponse.PageAdded;
                if (json["edit"]["nochange"] != null) pageResponse = EditPageResponse.NoChange;
            }
            else
            {
                throw new Exception(string.Format("Unknown response editing the page: {0}", response.Content));
            }

            return pageResponse;
        }

        /// <summary>
        /// Delete the specified page.  Must be logged in to delete.
        /// </summary>
        /// <param name="title"></param>
        public void DeletePage(string title)
        {
            if (string.IsNullOrWhiteSpace(_sessionId)) throw new SecurityException("Not logged in.");

            string deleteToken = string.Empty;

            // Get a delete token
            Dictionary<string, string> postData = new Dictionary<string, string>();
            postData.Add("action", "query");
            postData.Add("prop", "info");
            postData.Add("intoken", "delete");
            postData.Add("titles", title);
            postData.Add("format", "json");
            postData.Add("lgtoken", _lgToken);

            Dictionary<string, string> headerData = new Dictionary<string, string>();
            headerData.Add("Cookie", string.Format("{0}UserID={1}; {0}UserName={2}; {0}_session={3}; expires={4};path=/;httponly",
                _cookiePrefix, _lgUserId, _lgUserName, _sessionId, DateTime.Now.AddDays(7).ToUniversalTime().ToString()));

            WikiResponse response = _wikiRequest.Send("POST", _baseAddress, postData, headerData);

            // Read the response to determine the outcome of the login operation
            if (response.Status != "OK") throw new Exception("Error getting delete token from wiki");

            // Parse the JSON response to determine success or failure of the delete token retreival
            JsonReader reader = new JsonTextReader(new StringReader(response.Content));
            reader.DateParseHandling = DateParseHandling.None;
            JObject json = JObject.Load(reader);

            /*
             * Example JSON responses for a deletetoken query
             * 
             * {
             *  "query": {
             *    "pages": {
             *      "2": {
             *        "pageid": 2,
             *        "ns": 0,
             *        "title": "Test2",
             *        "touched": "2014-11-19T21:22:55Z",
             *        "lastrevid": 14,
             *        "counter": 10,
             *        "length": 34,
             *        "starttimestamp": "2014-11-20T03:57:12Z"
             *      }
             *    }
             *  },
             *  "warnings": {
             *    "info": {
             *      "*": "Action 'delete' is not allowed for the current user"
             *    }
             *  }
             * }
             * 
             * 
             * {
             *  "query": {
             *    "pages": {
             *      "7": {
             *        "pageid": 7,
             *        "ns": 0,
             *        "title": "Test2",
             *        "touched": "2014-11-20T04:47:36Z",
             *        "lastrevid": 20,
             *        "counter": 0,
             *        "length": 13,
             *        "starttimestamp": "2014-11-20T04:48:05Z",
             *        "deletetoken": "8b69c20f2c03257900253461c457035a+\\"
             *      }
             *    }
             *  }
             * }
             */

            if (json["error"] != null)
            {
                string errorCode = json["error"]["code"].Value<string>();
                string errorInfo = json["error"]["info"].Value<string>();
                throw new Exception(string.Format("Error getting delete token: {0} - {1}", errorCode, errorInfo));
            }
            if (json["warnings"] != null)
            {
                string warning = json["warnings"]["info"]["*"].Value<string>();
                throw new Exception(string.Format("Error getting delete token: {0}", warning));
            }
            else if (json["query"] != null)
            {
                deleteToken = json["query"]["pages"].First().First()["deletetoken"].Value<string>();
            }
            else
            {
                throw new Exception(string.Format("Unknown response getting delete token: {0}", response.Content));
            }

            // Send the delete request to the Mediawiki instance
            postData = new Dictionary<string, string>();
            postData.Add("action", "delete");
            postData.Add("title", title);
            postData.Add("format", "json");
            postData.Add("token", System.Net.WebUtility.UrlEncode(deleteToken));

            headerData = new Dictionary<string, string>();
            headerData.Add("Cookie", string.Format("{0}UserID={1}; {0}UserName={2}; {0}_session={3}; expires={4};path=/;httponly", 
                _cookiePrefix, _lgUserId, _lgUserName, _sessionId, DateTime.Now.AddDays(7).ToUniversalTime().ToString()));

            response = _wikiRequest.Send("POST", _baseAddress, postData, headerData);

            // Read the response to determine the outcome of the delete operation
            if (response.Status != "OK") throw new Exception("Error posting delete to wiki");

            // Parse the JSON response to determine success or failure of the edit operation
            reader = new JsonTextReader(new StringReader(response.Content));
            reader.DateParseHandling = DateParseHandling.None;
            json = JObject.Load(reader);

            /*
             * Example JSON response for a delete operation
             * 
             * {
             *   "delete": {
             *   "title": "Test2",
             *   "reason": "content was: \"Content of Test 2..1.1.1.1.1.1.1.1\" (and the only contributor was \"[[Special:Contributions/192.168.56.1|192.168.56.1]]\")"
             *   }
             * }
             */

            if (json["error"] != null)
            {
                string errorCode = json["error"]["code"].Value<string>();
                string errorInfo = json["error"]["info"].Value<string>();
                throw new Exception(string.Format("Error deleting the page: {0} - {1}", errorCode, errorInfo));
            }
            else if (json["delete"] != null)
            {
                // Nothing to do
            }
            else
            {
                throw new Exception(string.Format("Unknown response deleting the page: {0}", response.Content));
            }
        }

        public enum EditPageResponse
        {
            PageAdded,
            PageUpdated,
            NoChange
        }
    }
}
