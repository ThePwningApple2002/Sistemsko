using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekat_PrviDeo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            GifServer server = new GifServer();
            server.Start();
        }

    }
}
