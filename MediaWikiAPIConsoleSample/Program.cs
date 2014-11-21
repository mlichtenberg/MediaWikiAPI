using MediaWikiAPI;
using System;

namespace MediaWikiAPIConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            //Wiki wiki = new Wiki("http://192.168.56.101");
            Wiki wiki = new Wiki("http://192.168.56.101", new WikiRequestMock());

            string title = "Test2";
            string content = string.Empty;

            // Add page
            try
            {
                content = "Test Page";
                Wiki.EditPageResponse editResponse = wiki.EditPage(title, content);
                content = string.Empty;
            }
            catch(Exception ex)
            {
                content = ex.Message;
            }

            Console.WriteLine();
            if (string.IsNullOrWhiteSpace(content))
                Console.WriteLine(string.Format("Page '{0}' added", title));
            else 
                Console.WriteLine(string.Format("Error adding page '{0}': {1}", title, content));
            Console.ReadKey();

            // Retrieve page content
            try
            {
                content = wiki.GetContent(title);
            }
            catch (Exception ex)
            {
                content = ex.Message;
            }

            Console.WriteLine();
            Console.WriteLine(string.Format("Content of '{0}': {1}", title, content));
            Console.ReadKey();
           
            // Edit page
            try
            {
                Wiki.EditPageResponse editResponse = wiki.EditPage(title, content + ".1");
                content = string.Empty;
            }
            catch (Exception ex)
            {
                content = ex.Message;
            }

            Console.WriteLine();
            if (string.IsNullOrWhiteSpace(content))
                Console.WriteLine(string.Format("Page '{0}' edited", title));
            else
                Console.WriteLine(string.Format("Error editing page '{0}': {1}", title, content));
            Console.ReadKey();

            // Retrieve page content
            try
            {
                content = wiki.GetContent(title);
            }
            catch (Exception ex)
            {
                content = ex.Message;
            }

            Console.WriteLine();
            Console.WriteLine(string.Format("Content of '{0}': {1}", title, content));
            Console.ReadKey();
            
            // Login
            try
            {
                wiki.Login("root", "m0b0t");
                content = string.Empty;
            }
            catch(Exception ex)
            {
                content = ex.Message;
            }

            Console.WriteLine();
            if (string.IsNullOrWhiteSpace(content))
                Console.WriteLine("Login successful");
            else
                Console.WriteLine(string.Format("Login failed: {0}", content));
            Console.ReadKey();

            // Delete a page
            try
            {
                wiki.DeletePage(title);
                content = string.Empty;
            }
            catch(Exception ex)
            {
                content = ex.Message;
            }

            Console.WriteLine();
            if (string.IsNullOrWhiteSpace(content))
                Console.WriteLine("Delete successful");
            else
                Console.WriteLine(string.Format("Delete failed: {0}", content));
            Console.ReadKey();
        }
    }
}
