using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace HTTPServer
{
    class Server
    {
        Socket serverSocket;

        public Server(int portNumber, string redirectionMatrixPath)
        {
            //TODO: call this.LoadRedirectionRules passing redirectionMatrixPath to it
            this.LoadRedirectionRules(redirectionMatrixPath);
            //TODO: initialize this.serverSocket
            IPEndPoint ipend = new IPEndPoint( IPAddress.Parse("127.0.0.1"), 1000);  // new IPEndPoint(IPAddress.Any, 1000); // 
            
            serverSocket = new Socket(AddressFamily.InterNetwork,
                                      SocketType.Stream,
                                      ProtocolType.Tcp);

            serverSocket.Bind(ipend);
        }

        public void StartServer()
        {
            // TODO: Listen to connections, with large backlog.
            Console.WriteLine("Listening....");

            serverSocket.Listen(100);
            // TODO: Accept connections in while loop and start a thread for each connection on function "Handle Connection"
            while (true)
            {
                //TODO: accept connections and start thread for each accepted connection.
                Socket client = serverSocket.Accept();

                Console.WriteLine("Accepted....");

                Thread thread = new Thread(new ParameterizedThreadStart(HandleConnection));

                thread.Start(client);
            }
        }

        public void HandleConnection(object obj)
        {
            // TODO: Create client socket
            Socket Client_Socket = (Socket)obj;
            // set client socket ReceiveTimeout = 0 to indicate an infinite time-out period
            Client_Socket.ReceiveTimeout = 0;
            // TODO: receive requests in while true until remote client closes the socket.
            while (true)
            {
                try
                {
                    // TODO: Receive request
                    byte[] Receive_Data = new byte[1024];

                    int Received_Length = Client_Socket.Receive(Receive_Data);

                    string Data = Encoding.ASCII.GetString(Receive_Data);

                    Console.WriteLine(Data);
                    //Console.WriteLine("KHALED");
                    // TODO: break the while loop if receivedLen==0
                    if (Received_Length == 0)
                        break;
                    // TODO: Create a Request object using received request string
                    Request request = new Request(Data);
                    // TODO: Call HandleRequest Method that returns the response
                    Response response = HandleRequest(request);

                    string resev = response.ResponseString;

                    Console.WriteLine(resev);

                    byte[] respon = Encoding.ASCII.GetBytes(resev);
                    // TODO: Send Response back to client
                    Client_Socket.Send(respon);

                }
                catch (Exception ex)
                {
                    // TODO: log exception using Logger class
                    Logger.LogException(ex);
                }
            }

            // TODO: close client socket
            Client_Socket.Close();
        }

        Response HandleRequest(Request request)
        {
            //throw new NotImplementedException();
            string content;
            StatusCode code;
            try
            {
                //TODO: check for bad request 
                if(!request.ParseRequest())
                {
                    code =StatusCode.BadRequest;
                    content = "<!DOCTYPE html><html><body><h1>400 Bad Request</h1><p>400 Bad Request</p></body></html>";
                }

                //TODO: map the relativeURI in request to get the physical path of the resource.
                string[] name = request.relativeURI.Split('/');

                string Physical_Path = Configuration.RootPath + '\\' + name[1];

                //TODO: check for redirect
                for (int i = 0; i < Configuration.RedirectionRules.Count; i++)
                {
                    if('/' + Configuration.RedirectionRules.Keys.ElementAt(i).ToString() == request.relativeURI)
                    {
                        code = StatusCode.Redirect;

                        request.relativeURI = '/' + Configuration.RedirectionRules.Values.ElementAt(i).ToString();

                        name[1] = Configuration.RedirectionRules.Values.ElementAt(i).ToString();

                        Physical_Path = Configuration.RootPath + '\\' + name[1];

                        content = File.ReadAllText(Physical_Path);

                        string location = "http://localhost:1000/" + name[1];
                        Console.WriteLine(location);

                // Create OK response
                        Response response = new Response( code, "text/html" , content , location);

                        return response; 
                    }
                }
                //TODO: check file exists
                if(!File.Exists(Physical_Path))
                {
                    Physical_Path = Configuration.RootPath + '\\' + "Not Found HTML";

                    code =StatusCode.NotFound;

                    content = File.ReadAllText(Physical_Path);
                }
                //TODO: read the physical file
                else
                {
                    code = StatusCode.OK;

                    content = File.ReadAllText(Physical_Path);
                }
                Response response1 = new Response( code, "text/html", content,null);

                return response1;
            }
            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                Logger.LogException(ex);
                // TODO: in case of exception, return Internal Server Error. 
                code =StatusCode.InternalServerError;

                content = "Internal Error";

                Response response = new Response(code, "text/html", content,null);

                return response;
            }
        }

        private string GetRedirectionPagePathIFExist(string relativePath)
        {
            // using Configuration.RedirectionRules return the redirected page path if exists else returns empty
            for (int i = 0; i < Configuration.RedirectionRules.Count; i++)
            {
                if (relativePath == '/' + Configuration.RedirectionRules.Keys.ElementAt(i).ToString())
                {
                    string Redirection_Path = '/' + Configuration.RedirectionRules.Values.ElementAt(i).ToString();

                    string Physical_Path = Configuration.RedirectionRules.Values.ElementAt(i).ToString();

                    return Physical_Path;
                }
            }
            
            return string.Empty;
        }

        private string LoadDefaultPage(string defaultPageName)
        {
            string content="";
            string filePath = Path.Combine(Configuration.RootPath, defaultPageName);
            // TODO: check if filepath not exist log exception using Logger class and return empty string
            try
            {
                // else read file and return its content
                throw new NotImplementedException();
                if(File.Exists(filePath))
                {
                    content = File.ReadAllText(filePath);
                }
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
                
            }
            return content;
        }

        private void LoadRedirectionRules(string filePath)
        {
            try
            {
                // TODO: using the filepath paramter read the redirection rules from file 
                FileStream File_Stream = new FileStream(filePath, FileMode.Open);

                StreamReader Stream_Reader = new StreamReader(File_Stream);
                // then fill Configuration.RedirectionRules dictionary 
                string line = "";
                while (Stream_Reader.Peek()!=0 && line == null)
                {
                    line = Stream_Reader.ReadLine();
                    if(line != null)
                    {
                        string[] Data = line.Split(',');
                        if (Data[0] == "")
                        {
                            break;
                        }
                        Configuration.RedirectionRules.Add(Data[0], Data[1]);
                    }
                }
                File_Stream.Close();
            }
            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                Logger.LogException(ex);
                Environment.Exit(1);
            }
        }
    }
}
