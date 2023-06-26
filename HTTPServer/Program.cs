using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: Call CreateRedirectionRulesFile() function to create the rules of redirection 
            CreateRedirectionRulesFile();

            String redirectionRules = Configuration.redirectionRulesPath;
            //Start server
            // 1) Make server object on port 1000
            // 2) Start Server
            Server server = new Server(1000, redirectionRules);
            server.StartServer();
        }

        static void CreateRedirectionRulesFile()
        {
            // TODO: Create file named redirectionRules.txt
            // each line in the file specify a redirection rule
            FileStream filestream = new FileStream(@"C:\Users\Khaled\Downloads\Project Server\Template[2021-2022]\HTTPServer\bin\Debug\redirectionRules.txt", FileMode.Open);

            StreamWriter file_write = new StreamWriter(filestream);
            // example: "aboutus.html,aboutus2.html"
            // means that when making request to aboustus.html,, it redirects me to aboutus2
            file_write.WriteLine(@"aboutus.html,aboutus2.html");

            file_write.Close();
        }
         
    }
}
