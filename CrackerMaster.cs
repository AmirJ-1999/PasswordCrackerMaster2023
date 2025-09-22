
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PasswordCrackerMaster2023.model;
using PasswordCrackerMaster2023.util;

namespace PasswordCrackerMaster2023
   

{
    public class CrackerMaster
    {
        //List<String> chunks = new List<string>();
        BlockingCollection<List<String>> chunks = new BlockingCollection<List<string>>();
        public CrackerMaster()
        {
            //read the password file
            List<UserInfo> userInfos =  PasswordFileHandler.ReadPasswordFile("passwords.txt");

            //read the dictionary and create chunks

            CreateChunks("webster-dictionary-reduced.txt");
        }

        internal void Listen(IPAddress loopback, int port)
        {
            TcpListener server  = new TcpListener(loopback, port);

            server.Start();

            TcpClient client = server.AcceptTcpClient();
            Stream ns = client.GetStream();
            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns);

            sw.AutoFlush = true;

            String request = sr.ReadLine();

            if(request == "password")
            {
                //send passwords list 

               String password = sendPasswords();
                sw.WriteLine(password);


            }else if(request == "chunk")
            {
                //send a chunk
                // String chunk = sendChunk();


                 String  chunk =  JsonSerializer.Serialize(getChunk());

                 sw.WriteLine(chunk);

               // var chunk1 = JsonSerializer.Serialize(getChunk());

               // var chunk2 = JsonSerializer.Serialize(getChunk());

               // Console.WriteLine(chunk1);
              //  Console.WriteLine("---------------------------------------------------------------------");
              //  Console.WriteLine(chunk2);
            }

            Console.WriteLine(request);

            Console.ReadKey();

        }


        public String sendPasswords()
        {
            return "mypassword";
        }

        public List<String> getChunk()
        {
            return chunks.Take();
        }

        private void CreateChunks(string filename)
        {
           // List<UserInfoClearText> result = new List<UserInfoClearText>();

            using (FileStream fs = new FileStream("webster-dictionary.txt", FileMode.Open, FileAccess.Read))

            using (StreamReader dictionary = new StreamReader(fs))
            {
                int counter = 0;
                List<String> words = new List<string>();

                while (!dictionary.EndOfStream)
                {
                    String dictionaryEntry = dictionary.ReadLine();

                    counter++;

                    if(counter % 10000 != 0)
                    {
                        words.Add(dictionaryEntry);
                    }else
                    {
                        chunks.Add(words);
                        words = new List<string>();
                    }
                    
                  //  IEnumerable<UserInfoClearText> partialResult = CheckWordWithVariations(dictionaryEntry, userInfos);
                   // result.AddRange(partialResult);
                }

                chunks.Add(words);
            }
        }
    }
}
