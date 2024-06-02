using Projekat_DrugiDeo;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Projekat_DrugiDeo
{
    public class GifServer
    {
        private readonly HttpListener listener;
        private readonly Cache cache;

        public GifServer(Cache cache)
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5050/");
            this.cache = cache;
        }

        public async Task StartAsync()
        {
            listener.Start();
            Console.WriteLine("Listening...");

            while (true)
            {
                var context = await listener.GetContextAsync();
                _ = HandleRequestAsync(context);
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            string filename = context.Request.Url.AbsolutePath.TrimStart('/');
            string rootPath = Directory.GetCurrentDirectory(); // Koristi rootPath unutar funkcije

            Console.WriteLine("Requested: " + filename);

            byte[] fileData = await GetFileDataAsync(rootPath, filename);
            if (fileData != null)
            {
                await ServeFileAsync(context, fileData);
            }
            else
            {
                await SendNotFoundAsync(context, filename);
            }
        }

        private async Task<byte[]> GetFileDataAsync(string rootPath, string filename)
        {
            if (cache.TryGetValue(filename, out string cachedPath))
            {
                return await File.ReadAllBytesAsync(cachedPath);
            }

            string filePath = await Task.Run(() => SearchForGif(rootPath, filename));
            if (filePath != null)
            {
                cache.Set(filename, filePath);
                return await File.ReadAllBytesAsync(filePath);
            }

            return null;
        }

        private async Task ServeFileAsync(HttpListenerContext context, byte[] fileData)
        {
            try
            {
                context.Response.ContentType = "image/gif";
                context.Response.ContentLength64 = fileData.Length;
                await context.Response.OutputStream.WriteAsync(fileData, 0, fileData.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error serving file: " + e.Message);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                context.Response.OutputStream.Close();
            }
        }

        private async Task SendNotFoundAsync(HttpListenerContext context, string filename)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                await writer.WriteAsync("Gif not found: " + filename);
            }
            context.Response.OutputStream.Close();
        }

        private string SearchForGif(string rootPath, string filename)
        {
            try
            {
                string[] files = Directory.GetFiles(rootPath, filename, SearchOption.AllDirectories);
                if (files.Length > 0)
                    return files[0];
            }
            catch (Exception e)
            {
                Console.WriteLine("Error searching file: " + e.Message);
            }
            return null;
        }
    }
}
