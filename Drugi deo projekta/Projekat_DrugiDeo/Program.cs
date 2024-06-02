using Projekat_DrugiDeo;
using System;
using System.Threading.Tasks;

namespace Projekat_DrugiDeo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cache = new Cache(10); // Postavljanje limita keša na 10
            var gifServer = new GifServer(cache);

            await gifServer.StartAsync();
        }
    }
}
