using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;

namespace Projekat_PrviDeo
{
    public class GifServer
    {
        private HttpListener listener;
        private ConcurrentDictionary<string, byte[]> cache;

        public GifServer()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5050/");
            cache = new ConcurrentDictionary<string, byte[]>();
        }

        public void Start()
        {
            listener.Start();
            Console.WriteLine("Listening...");

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                ThreadPool.QueueUserWorkItem(HandleRequest, context);
            }
        }

        private void HandleRequest(object state)
        {
            var context = (HttpListenerContext)state;
            string filename = context.Request.Url.AbsolutePath.TrimStart('/');
            string rootDirectory = Directory.GetCurrentDirectory();

            Console.WriteLine("Requested: " + filename);

            byte[] fileData;
            if (!cache.TryGetValue(filename, out fileData))
            {
                string filePath = SearchForGif(rootDirectory, filename);
                if (filePath != null)
                {
                    fileData = File.ReadAllBytes(filePath);
                    cache.TryAdd(filename, fileData); // Add to cache using thread-safe method
                    Console.WriteLine("Loaded from disk and cached: " + filename);
                }
            }

            if (fileData != null)
            {
                ServeFile(context, fileData);
            }
            else
            {
                Console.WriteLine("File not found: " + filename);
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write("Gif not found: " + filename);
                }
                context.Response.OutputStream.Close();
            }
        }

        private void ServeFile(HttpListenerContext context, byte[] fileData)
        {
            try
            {
                context.Response.ContentType = "image/gif";
                context.Response.ContentLength64 = fileData.Length;
                context.Response.OutputStream.Write(fileData, 0, fileData.Length);
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
