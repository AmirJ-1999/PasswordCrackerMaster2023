using System;
using System.Net;

namespace PasswordCrackerMaster2023
{
    class Program
    {
        static void Main(string[] args)
        {

            CrackerMaster master = new CrackerMaster();

            master.Listen(IPAddress.Loopback, 6789);
        }
    }
}
