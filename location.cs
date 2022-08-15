//Demonstrate Sockets
using System;
using System.Net.Sockets;
using System.IO;
public class Whois
{
    static void Main(string[] args)
    {
        //Creating a TCP Client
        TcpClient location = new TcpClient();

        //Protocol is a double variable that is used to track which protocol is being used
        double protocol = 0.0;

        //Connecting to a server based upon the inputs from the console.
        string address = CheckPresent(ref args, "-h");
        int port = int.Parse(CheckPresent(ref args, "-p"));

        //Checking if no argument has been provided
        if (args.Length < 1)
        {
            Console.WriteLine("ERROR: No arguments provided");
        }
        //Checking if the request should be a HTTP/1.1 style request
        else if (CheckPresent(ref args, "-h1") != null)
        {
            protocol = 1.1;
        }
        //Checking if instead it's HTTP/1.0 style
        else if (CheckPresent(ref args, "-h0") != null)
        {
            protocol = 1.0;
        }
        //HTTP/0.9
        else if (CheckPresent(ref args, "-h9") != null)
        {
            protocol = 0.9;
        }
        //If it is none of the former it is instead launched as a whois request, as this is default
        else
        {
            protocol = 0.5;
        }

        //Similarly, checking if too many arguments have been provided
        if (args.Length > 2)
        {
            Console.WriteLine("ERROR: Too many arguments provided");
        }
        else if (protocol == 1.1)
        {
            OneOneConnect(args, location, port, address);
        }
        else if (protocol == 1.0)
        {
            OneZeroConnect(args, location, port, address);
        }
        else if (protocol == 0.9)
        {
            ZeroNineConnect(args, location, port, address);
        }
        else if (protocol == 0.5)
        {
            WhoIsConnect(args, location, port, address);
        }
    }

    /// <summary>
    /// This method checks if a given value is present within the provided arguments
    /// It has a multitude of applications, from checking what type of request style is being used,
    /// to which port and address should be connected to
    /// </summary>
    /// <param name="args">The args parameter refers to the arguments supplied by the user on launch. 
    /// It is passed as a ref so that the code can alter what's inside if need be.</param>
    /// <param name="param">The param variable is the thing that is being searched for within the arguments</param>
    /// <returns>The value returned is whatever the code is trying to unpick from the arguments, be it the port, address or style format.</returns>
    static string CheckPresent(ref string[] args, string param)
    {
        int pos = Array.IndexOf(args, param);
        if (pos > -1)
        {
            string[] args2;
            if (args[pos] == "-h0" || args[pos] == "-h1" || args[pos] == "-h9")
            {
                args2 = new string[args.Length - 1];
            }
            else
            {
                args2 = new string[args.Length - 2];
            }
            string connection;
            try
            {
                connection = args[pos + 1];
            }
            catch { connection = args[pos]; }
            int y = 0;
            for (int x = 0; x < args.Length; x++)
            {
                if (args[pos] == "-h0" || args[pos] == "-h1" || args[pos] == "-h9")
                {
                    if (x == pos)
                    {
                        continue;
                    }
                    else
                    {
                        args2[y] = args[x];
                        y++;
                    }
                }
                else
                {
                    if (x == pos || x == pos + 1)
                    {
                        continue;
                    }
                    else
                    {
                        args2[y] = args[x];
                        y++;
                    }
                }
            }
            args = args2;
            return connection;
        }
        else
        {
            if (param == "-h")
            {
                return "whois.net.dcs.hull.ac.uk";
            }
            else if (param == "-p")
            {
                return "43";
            }
            else
            {
                return null;
            }
        }
    }

    //The next four methods refer to the four protocols for requests, each one has a slightly different format based upon the specification
    //A large amount of the explanation for the code is visible in the whoisconnect method, as the methods have similar function
    static void WhoIsConnect(string[] args, TcpClient location, int port, string address)
    {

        try
        {
            location.Connect(address, port);
        }
        catch (Exception e) { Console.WriteLine(e.ToString()); }

        //Setting a timeout to 1000ms
        location.ReceiveTimeout = 1000;
        location.SendTimeout = 1000;

        //Creating a stream reader and writer
        StreamWriter sw = new StreamWriter(location.GetStream());
        StreamReader sr = new StreamReader(location.GetStream());

        //Checking whether the arguments are for getting or setting values
        //case 1 refer to a single argument - a get request
        //case 2 refers to 2 arguments - a set request
        //This code is repeated with different formats in the other methods
        switch (args.Length)
        {
            case 1:
                try
                {
                    sw.WriteLine(args[0]);
                    sw.Flush();
                    string response = sr.ReadLine();
                    Console.WriteLine(args[0] + " is " + response);
                }
                catch { Console.WriteLine("Invalid argument provided"); };
                break;
            case 2:
                try
                {
                    sw.WriteLine(args[0] + " " + args[1]);
                    sw.Flush();
                }
                catch { Console.WriteLine("Invalid arguments provided"); };
                if (sr.ReadLine() == "OK")
                {
                    Console.WriteLine(args[0] + " location changed to be " + args[1]);
                }
                else
                {
                    Console.WriteLine("ERROR: no entries found");
                }
                break;
            default:
                throw new Exception("Incorrect number of arguments");
        }
        sr.Close();
        sw.Close();
    }

    static void ZeroNineConnect(string[] args, TcpClient location, int port, string address)
    {
        try
        {
            location.Connect(address, port);
        }
        catch (Exception e) { Console.WriteLine(e.ToString()); }

        location.ReceiveTimeout = 1000;
        location.SendTimeout = 1000;


        //Creating a stream reader and writer
        StreamWriter sw = new StreamWriter(location.GetStream());
        StreamReader sr = new StreamReader(location.GetStream());

        string response = "";

        switch (args.Length)
        {
            case 1:
                try
                {
                    sw.WriteLine("GET /" + args[0]);
                    sw.Flush();
                    response = sr.ReadLine();
                    if (response.Contains("OK"))
                    {
                        sr.ReadLine();
                        while (sr.ReadLine() != "") { }
                        response = sr.ReadToEnd();
                    }
                    else if (response.Contains("404"))
                    {
                        Console.Write("Error: entry not found in database");
                        break;
                    }
                    Console.Write(args[0] + " is " + response);
                }
                catch { Console.WriteLine("Invalid argument provided"); };
                break;

            case 2:
                try
                {
                    sw.WriteLine("PUT /" + args[0]);
                    sw.WriteLine();
                    sw.WriteLine(args[1]);
                    sw.Flush();
                }
                catch { Console.WriteLine("Invalid arguments provided"); };
                response = sr.ReadLine();
                if (response.Contains("OK"))
                {
                    Console.WriteLine(args[0] + " location changed to be " + args[1]);
                }
                else
                {
                    Console.WriteLine("ERROR: no entries found");
                }
                break;
            default:
                throw new Exception("Incorrect number of arguments");
        }
        sr.Close();
        sw.Close();
    }

    static void OneZeroConnect(string[] args, TcpClient location, int port, string address)
    {
        try
        {
            location.Connect(address, port);
        }
        catch (Exception e) { Console.WriteLine(e.ToString()); }

        location.ReceiveTimeout = 1000;
        location.SendTimeout = 1000;


        //Creating a stream reader and writer
        StreamWriter sw = new StreamWriter(location.GetStream());
        StreamReader sr = new StreamReader(location.GetStream());

        string response = "";

        switch (args.Length)
        {
            case 1:
                try
                {
                    sw.WriteLine("GET /?" + args[0] + " HTTP/1.0");
                    sw.WriteLine();
                    sw.Flush();
                    response = sr.ReadLine();
                    if (response.Contains("OK"))
                    {
                        sr.ReadLine();
                        while (sr.ReadLine() != "") { }
                        response = sr.ReadToEnd();
                    }
                    else if (response.Contains("404"))
                    {
                        Console.Write("Error: entry not found in database");
                        break;
                    }
                    Console.Write(args[0] + " is " + response);
                }
                catch { Console.WriteLine("Invalid argument provided"); };
                break;

            case 2:
                try
                {
                    sw.WriteLine("POST /" + args[0] + " HTTP/1.0");
                    sw.WriteLine("Content-Length: " + args[1].Length);
                    sw.WriteLine();
                    sw.Write(args[1]);
                    sw.Flush();
                }
                catch { Console.WriteLine("Invalid arguments provided"); };
                response = sr.ReadLine();
                if (response.Contains("OK"))
                {
                    Console.WriteLine(args[0] + " location changed to be " + args[1]);
                }
                else
                {
                    Console.WriteLine("ERROR: no entries found");
                }
                break;
            default:
                throw new Exception("Incorrect number of arguments");
        }
        sr.Close();
        sw.Close();
    }

    static void OneOneConnect(string[] args, TcpClient location, int port, string address)
    {
        try
        {
            location.Connect(address, port);
        }
        catch (Exception e) { Console.WriteLine(e.ToString()); }

        location.ReceiveTimeout = 1000;
        location.SendTimeout = 1000;


        //Creating a stream reader and writer
        StreamWriter sw = new StreamWriter(location.GetStream());
        StreamReader sr = new StreamReader(location.GetStream());

        string response = "";
        string message;
        bool success = false;

        switch (args.Length)
        {
            case 1:
                try
                {
                    sw.WriteLine("GET /?name=" + args[0] + " HTTP/1.1");
                    sw.WriteLine("Host: " + address);
                    sw.WriteLine();
                    sw.Flush();
                    response = sr.ReadLine();
                    if (response.Contains("OK"))
                    {
                        sr.ReadLine();
                        while (sr.ReadLine() != "") { }
                        response = sr.ReadToEnd();
                        success = true;
                    }
                    else if (response.Contains("404"))
                    {
                        Console.Write("Error: entry not found in database");
                        break;
                    }
                    else if (response.Contains("301"))
                    {
                        while (sr.ReadLine() != "") { }
                        Console.Write(args[0] + " is ");
                        while (response != "</html>")
                        {
                            response = sr.ReadLine();
                            Console.WriteLine(response);
                        }
                    }
                    if (success)
                    {
                        Console.Write(args[0] + " is " + response);
                    }
                }
                catch { Console.WriteLine("Invalid argument provided"); };
                break;
            
            case 2:
                try
                {
                    message = "name=" + args[0] + "&location=" + args[1];
                    sw.WriteLine("POST / HTTP/1.1");
                    sw.WriteLine("Host: " + address);
                    sw.WriteLine("Content-Length: " + message.Length);
                    sw.WriteLine();
                    sw.Write(message);
                    sw.Flush();
                }
                catch { Console.WriteLine("Invalid arguments provided"); };
                response = sr.ReadLine();
                if (response.Contains("OK"))
                {
                    Console.WriteLine(args[0] + " location changed to be " + args[1]);
                }
                else
                {
                    Console.WriteLine("ERROR: no entries found");
                }
                break;
            default:
                throw new Exception("Incorrect number of arguments");
        }
        sr.Close();
        sw.Close();
    }
}