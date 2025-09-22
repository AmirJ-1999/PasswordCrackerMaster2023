// FILE: PasswordCrackerMaster2023/CrackerMaster.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PasswordCrackerCentralized.util;
using PasswordCrackerMaster2023.model; // <-- korrekt model
using PasswordCrackerMaster2023.util;  // <-- korrekt util

namespace PasswordCrackerMaster2023
{
    public class CrackerMaster
    {
        private readonly BlockingCollection<List<string>> _chunks = new BlockingCollection<List<string>>();
        private readonly List<UserInfo> _userInfos;

        public CrackerMaster()
        {
            _userInfos = PasswordFileHandler.ReadPasswordFile("passwords.txt");
            CreateChunks("webster-dictionary.txt", chunkSize: 10000);
            Console.WriteLine("Chunks ready.");
        }

        internal void Listen(IPAddress bindAddress, int port)
        {
            TcpListener server = new TcpListener(bindAddress, port);
            server.Start();
            Console.WriteLine($"Master listening on {bindAddress}:{port}");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Task.Run(() => HandleClient(client));
            }
        }

        private void HandleClient(TcpClient client)
        {
            using (client)
            using (var ns = client.GetStream())
            using (var sr = new System.IO.StreamReader(ns, Encoding.UTF8))
            using (var sw = new System.IO.StreamWriter(ns, Encoding.UTF8) { AutoFlush = true })
            {
                try
                {
                    string request = sr.ReadLine();
                    Console.WriteLine("Request: " + request);

                    if (request == "chunk")
                    {
                        var chunkObj = GetChunk();
                        var json = JsonSerializer.Serialize(chunkObj);
                        Console.WriteLine("Sending chunk size (items): " + chunkObj.Count);
                        sw.WriteLine(json);
                    }
                    else if (request == "passwords")
                    {
                        var lines = File.ReadAllLines("passwords.txt");
                        sw.WriteLine(JsonSerializer.Serialize(lines));
                    }
                    else
                    {
                        sw.WriteLine("unknown");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("HandleClient error: " + e.Message);
                }
            }
        }

        private List<string> GetChunk() => _chunks.Take();

        private void CreateChunks(string filename, int chunkSize)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            using (var dictionary = new System.IO.StreamReader(fs, Encoding.UTF8))
            {
                int counter = 0;
                List<string> words = new List<string>(chunkSize);

                while (!dictionary.EndOfStream)
                {
                    string entry = dictionary.ReadLine();
                    if (entry == null) continue;

                    words.Add(entry);
                    counter++;

                    if (counter % chunkSize == 0)
                    {
                        _chunks.Add(new List<string>(words));
                        words.Clear();
                    }
                }

                if (words.Count > 0)
                    _chunks.Add(new List<string>(words));
            }
        }
    }
}
