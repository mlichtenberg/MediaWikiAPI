using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace MediaWikiAPI
{
    internal class WikiRequest : IWikiRequest
    {
        public WikiResponse Send(string method, string baseUrl, Dictionary<string, string> parameters = null, 
            Dictionary<string, string> headers = null, Dictionary<string, string> cookies = null)
        {
            string content = string.Empty;
            string status = "OK";

            if (method == "GET")
            {
                string queryString = (parameters == null ? string.Empty : "?");
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, string> parameter in parameters)
                    {
                        queryString += (queryString.Length > 1 ? "&" : "");
                        queryString += parameter.Key + "=" + parameter.Value;
                    }
                }

                // Set up the request
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(baseUrl + queryString);

                // Set the headers
                if (headers != null)
                {
                    foreach(KeyValuePair<string, string> header in headers)
                    {
                        httpRequest.Headers.Add(header.Key, header.Value);
                    }
                }

                // Set the cookies
                if (cookies != null)
                {
                    foreach (KeyValuePair<string, string> cookie in cookies)
                    {
                        httpRequest.CookieContainer.Add(new Cookie(cookie.Key, cookie.Value));
                    }
                }

                // Submit the request
                HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                // Read the response from the Wiki
                status = httpResponse.StatusDescription;
                Stream responseStream = httpResponse.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                content = new StreamReader(responseStream, encode).ReadToEnd();

                // Release the resources
                httpResponse.Close();
            }
            else if (method == "POST")
            {
                string postData = string.Empty;
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, string> parameter in parameters)
                    {
                        postData += (postData.Length > 0 ? "&" : "");
                        postData += parameter.Key + "=" + parameter.Value;
                    }
                }

                byte[] data = Encoding.ASCII.GetBytes(postData);

                // Set up the request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(baseUrl);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                // Set the headers
                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                // Set the cookies
                if (cookies != null)
                {
                    request.CookieContainer = new CookieContainer();
                    foreach (KeyValuePair<string, string> cookie in cookies)
                    {
                        request.CookieContainer.Add(new Cookie(cookie.Key, cookie.Value));
                    }
                }

                // Submit the request
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                // Read the response from the wiki
                HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse();
                status = httpResponse.StatusDescription;
                Stream responseStream = httpResponse.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                content = new StreamReader(responseStream, encode).ReadToEnd();

                // Release the resources
                httpResponse.Close();
            }

            // Set up the WikiResponse object to return
            WikiResponse wikiResponse = new WikiResponse();
            wikiResponse.Status = status;
            wikiResponse.Content = content;

            return wikiResponse;
        }
    }
}
