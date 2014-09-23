using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.IO;
using System.Net;
using System.Net.Sockets;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WirelessTWAIN
{
    class ServerManager
    {

        TcpListener server = null;
        NetworkStream stream = null;
        WirelessTWAIN twain = null;
        Byte[] imageData;

        public ServerManager(WirelessTWAIN twain)
        {
            this.twain = twain;
        }

        public void run()
        {
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 2015;
                IPAddress localAddr = IPAddress.Parse("192.168.8.84"); // server IP

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");
                    data = null;

                    // Get a stream object for reading and writing
                    stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        JObject jobj = JObject.Parse(data);
                        JToken token = jobj.GetValue("type");
                        if (token != null)
                        {
                            string result = token.ToString();
                            Console.WriteLine("Received: {0}", result);

                            if (result.Equals("data"))
                            {
                                stream.Write(imageData, 0, imageData.Length);
                                stream.Flush();
                                imageData = null;
                            }
                            else if (result.Equals("info"))
                            {
                                twain.scanImage();
                            }
                        }
                    }

                    stream = null;
                    // Shutdown and end connection
                    Console.WriteLine("close connection");
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }

        }

        public void prepareData(Byte[] data)
        {
            this.imageData = data;
        }

        public void sendData()
        {
            if (stream != null && imageData != null)
            {
                JObject jobj = new JObject();
                jobj.Add("length", imageData.Length);
                string msg = jobj.ToString();
                byte[] msgBytes = System.Text.Encoding.ASCII.GetBytes(msg);
                stream.Write(msgBytes, 0, msgBytes.Length);
                stream.Flush();
            }
        }
    }
}
