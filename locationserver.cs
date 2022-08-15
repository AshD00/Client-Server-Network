//Demonstrate Sockets
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
public class Whois
{
    static void Main()
    {
        //Creating my test dictionary and initialising the server method
        Dictionary<string, string> database = new Dictionary<string, string>();
        database.Add("test", "positive");
        //Calling the runserver method to initialise a server and accept a client
        runServer(database);
    }
    /// <summary>
    /// The method that's called to create a server and await a client connection
    /// </summary>
    /// <param name="database">The dictionary database</param>
    static void runServer(Dictionary<string, string> database)
    {
        //Establishing our TCP listeners, streams and sockets
        TcpListener listener;
        Socket connection;
        NetworkStream socketStream;
        try
        {
            //Creating a TCP socket to listen on port 43 for any connection requests from any IP
            listener = new TcpListener(IPAddress.Any, 43);
            listener.Start();
            Console.WriteLine("Server started listening.");

            //A loop to handle all requests
            while (true)
            {
                //Creates a socket when a request is received to handle it
                connection = listener.AcceptSocket();
                socketStream = new NetworkStream(connection);
                Console.WriteLine("Connection Received");

                //The method responsible for controlling incoming and outgoing data
                doRequest(socketStream, database);

                //Closing the stream and connection
                socketStream.Close();
                connection.Close();
            }
        }
        catch (Exception e) { Console.WriteLine(e.ToString()); }
    }

    /// <summary>
    /// A method to add a value to the database
    /// </summary>
    /// <param name="database">The dictionary with all the keys and values in it</param>
    /// <param name="key">The key we're adding to/updating (the username)</param>
    /// <param name="value">The value we're adding (location of the user)</param>
    /// <returns>The method returns true when it's added to/updated the database</returns>
    static bool addToDatabase(ref Dictionary<string, string> database, string key, string value)
    {
        //Checking if the value is already inside the database
        if (database.ContainsKey(key))
        {
            database[key] = value;
            return true;
        }
        //If it does not it is created and added
        else
        {
            database.Add(key, value);
            return true;
        }
    }

    /// <summary>
    /// The main method for client requests, called when a client connects
    /// </summary>
    /// <param name="socketStream">The current network stream which the client is connected on</param>
    /// <param name="database">The dictionary with the keys and values stored inside</param>
    static void doRequest(NetworkStream socketStream, Dictionary<string, string> database)
    {
        //Making a timeout in the event of an issue
        //Setting the timeout to 1 second
        socketStream.ReadTimeout = 1000;
        socketStream.WriteTimeout = 1000;

        //Creating a stream reader and writer
        StreamReader sr = new StreamReader(socketStream);
        StreamWriter sw = new StreamWriter(socketStream);

        //Defining some variables for later usage
        string msg, response;
        string[] args;
        double type = 0.0;

        try
        {
            //Receiving the input and turning it into a format our system can use
            msg = sr.ReadLine();

            //The following if statements check which style the request is
            args = msg.Split(new[] { ' ' }, 3);
            if (args.Length > 2)
            {
                //Checking if it's a HTTP 1.0 style request
                if (args[2] == "HTTP/1.0")
                {
                    //1.0 style response
                    //Checking if the received data is an update or a check
                    if (args[0] == "POST")
                    {
                        //Moving through the message until it reaches the content length
                        msg = sr.ReadLine();
                        sr.ReadLine();
                        string[] tempargs = msg.Split(' ');
                        msg = "";
                        //Setting length to equal the length of the content
                        int length = int.Parse(tempargs[1]);
                        //Writing the content to a variable and then passing that to the args array
                        for (int i = 0; i < length; i++)
                        {
                            msg += (char)sr.Read();
                        }
                        msg = ("HTTP/1.0&" + msg);
                        tempargs = msg.Split(new[] { '&' }, 2);
                        args[1] = args[1].Replace("/", "");
                        args = new string[] { tempargs[0], "POST", args[1], tempargs[1] };
                    }
                    //If it's a check then this reads in the message and removes erroneous data
                    else
                    {
                        args[1] = args[1].Replace("/?", "");
                        args = new string[] { args[2], args[0], args[1] };
                    }
                    type = 1.0;
                } 
                //Checking if it's a HTTP 1.1 style request
                else if (args[2] == "HTTP/1.1")
                {
                    //1.1 style response
                    //Checking if the received data is an update or a check
                    if (args[0] == "POST")
                    {
                        //Moving through the message until it reaches the content length
                        while (!msg.Contains("Content-Length"))
                        {
                            msg = sr.ReadLine();
                        }
                        args = msg.Split(' ');
                        msg = "";
                        sr.ReadLine();
                        //Setting length to equal the length of the content
                        int length = int.Parse(args[1]);
                        //Writing the content to a variable and then passing that to the args array
                        for (int i = 0; i < length; i++)
                        {
                            msg += (char)sr.Read();
                        }
                        msg = ("HTTP/1.1&" + msg);
                        args = msg.Split(new[] { '&' }, 3);
                        args = new string[] { args[0], "POST", args[1].Replace("name=", ""), args[2].Replace("location=", "") };
                    }
                    //If it's a check then this reads in the message and removes erroneous data
                    else
                    {
                        args[1] = args[1].Replace("/?name=", "");
                        args = new string[] { args[2], args[0], args[1] };
                    }
                    type = 1.1;
                }
            }
            //Checking if it's a HTTP 0.9 style request
            if (type != 0.0) { }

            else if (args.Length > 1)
            {
                if (((args[0] == "GET" || args[0] == "PUT") && args[1].Contains("/")))
                {
                    //0.9 style response
                    if (args[0] == "PUT")
                    {
                        sr.ReadLine();
                        msg += " ";
                        msg += sr.ReadLine();
                        args = msg.Split(new[] { ' ' }, 3);
                    }
                    args[1] = args[1].Trim('/');
                    type = 0.9;
                }
                else
                {
                    args = msg.Split(new[] { ' ' }, 2);
                }
            }
            else
            {
                args = msg.Split(new[] { ' ' }, 2);
            }

        }
        //In the event of an issue with establishing the connection, the server closes the stream and returns to the while loop
        catch
        {
            socketStream.Close();
            return;
        }

        //a switch statement to handle the request based upon the length of the args array
        switch (args.Length)
        {
            //length one means it's a whois request that only has a name, and the client is thus requesting a simple check on the location
            case 1:
                Console.WriteLine("Request for " + args[0] + " using the whois protocol");
                if (database.TryGetValue(args[0], out response))
                {
                    Console.WriteLine("Replied " + response);
                    sw.WriteLine(response);
                    break;
                }
                else
                {
                    Console.WriteLine("Replied with Error - request not in database");
                    sw.WriteLine("ERROR: no entries found");
                    break;
                }

            //length two could mean it's either a whois request or a HTTP/0.9 request
            //The former includes a location and thus the client is attempting to update the server database
            //The latter is a "GET" request
            case 2:
                //Attempting to update the database
                if (type == 0.0)
                {
                    Console.WriteLine("Request for " + args[0] + " to be moved to " + args[1] + " using the whois protocol");
                    if (addToDatabase(ref database, args[0], args[1]))
                    {
                        Console.WriteLine("Successfully updated");
                        sw.WriteLine("OK");
                        break;
                    }
                    //If it does not update an error is thrown
                    else
                    {
                        throw new Exception("Database could not be updated");
                    }
                }
                else
                {
                    Console.WriteLine("Request for " + args[1] + " using the HTTP/0.9 protocol");
                    if (database.TryGetValue(args[1], out response))
                    {
                        Console.WriteLine("Replied " + response);
                        sw.WriteLine("HTTP/0.9 200 OK");
                        sw.WriteLine("Content-Type: text/plain");
                        sw.WriteLine();
                        sw.WriteLine(response);
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Replied with Error - request not in database");
                        sw.WriteLine("HTTP/0.9 404 Not Found");
                        sw.WriteLine("Content-Type: text/plain");
                        sw.WriteLine();
                        break;
                    }

                }
            
            case 3:
                if (args[0] == "HTTP/1.0")
                {
                    Console.WriteLine("Request for " + args[2] + " using the HTTP/1.0 protocol");
                    //HTTP/1.0 request to fetch from database
                    if (database.TryGetValue(args[2], out response))
                    {
                        Console.WriteLine("Replied " + response);
                        sw.WriteLine("HTTP/1.0 200 OK");
                        sw.WriteLine("Content-Type: text/plain");
                        sw.WriteLine();
                        sw.WriteLine(response);
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Replied with Error - request not in database");
                        sw.WriteLine("HTTP/1.0 404 Not Found");
                        sw.WriteLine("Content-Type: text/plain");
                        sw.WriteLine();
                        break;
                    }
                }
                //HTTP 1.1 style response:
                else if (args[0] == "HTTP/1.1")
                {
                    Console.WriteLine("Request for " + args[2] + " using the HTTP/1.1 protocol");
                    //HTTP/1.1 request to fetch from database
                    if (database.TryGetValue(args[2], out response))
                    {
                        Console.WriteLine("Replied " + response);
                        sw.WriteLine("HTTP/1.1 200 OK");
                        sw.WriteLine("Content-Type: text/plain");
                        sw.WriteLine();
                        sw.WriteLine(response);
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Replied with Error - request not in database");
                        sw.WriteLine("HTTP/1.1 404 Not Found");
                        sw.WriteLine("Content-Type: text/plain");
                        sw.WriteLine();
                        break;
                    }
                }
                //A HTTP 0.9 style request to update the database
                else
                {
                    Console.WriteLine("Request for " + args[1] + " to be moved to " + args[2] + " using the HTTP/0.9 protocol");
                    //Attempting to add the value to the database
                    if (addToDatabase(ref database, args[1], args[2]))
                    {
                        Console.WriteLine("Successfully updated");
                        sw.WriteLine("HTTP/0.9 200 OK");
                        sw.WriteLine("Content-Type: text/plain");
                        sw.WriteLine();
                        break;
                    }
                    //If it does not update an error is thrown
                    else
                    {
                        throw new Exception("Database could not be updated");
                    }
                }
            case 4:
                //HTTP/1.0 request to update the database
                if (args[0] == "HTTP/1.0")
                {
                    Console.WriteLine("Request for " + args[2] + " to be moved to " + args[3] + " using the HTTP/1.0 protocol");
                    if (addToDatabase(ref database, args[2], args[3]))
                    {
                        Console.WriteLine("Successfully updated");
                        sw.WriteLine("HTTP/1.0 200 OK");
                        sw.WriteLine("Content-Type: text/plain");
                        sw.WriteLine();
                        break;
                    }
                    //If it does not update an error is thrown
                    else
                    {
                        throw new Exception("Database could not be updated");
                    }
                }
                //HTTP/1.1 request to update the database
                else
                {
                    Console.WriteLine("Request for " + args[2] + " to be moved to " + args[3] + " using the HTTP/1.1 protocol");
                    if (addToDatabase(ref database, args[2], args[3]))
                    {
                        Console.WriteLine("Successfully updated");
                        sw.WriteLine("HTTP/1.1 200 OK");
                        sw.WriteLine("Content-Type: text/plain");
                        sw.WriteLine();
                        break;
                    }
                    //If it does not update an error is thrown
                    else
                    {
                        throw new Exception("Database could not be updated");
                    }
                }
            default:
                throw new Exception("Incorrect number of arguments");
        }

        //sending the message and closing the write stream
        sw.Flush();
        sw.Close();
        sr.Close();
    }
}