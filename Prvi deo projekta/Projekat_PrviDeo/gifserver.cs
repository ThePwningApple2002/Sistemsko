using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections.Generic;

namespace Projekat_PrviDeo
{
    public class GifServer
    {
        private HttpListener listener;
        private Dictionary<string, byte[]> cache;

        public GifServer()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5050/");
            cache = new Dictionary<string, byte[]>();
        }

        public void Start()
        {
            listener.Start();
            Console.WriteLine("Listening...");

            while (true)
            {
                HttpListenerContext context = listener.GetContext(); 
                Thread thread = new Thread(() => HandleRequest(context));
                thread.Start();
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            string filename = context.Request.Url.AbsolutePath.Substring(1);
            string rootDirectory = Directory.GetCurrentDirectory();

            Console.WriteLine("Requested: " + filename);

            if (cache.ContainsKey(filename))
            {
                Console.WriteLine("Serving from cache: " + filename);
                ServeFile(context, cache[filename]);
            }
            else
            {
                string filePath = SearchForGif(rootDirectory, filename);
                if (filePath != null)
                {
                    byte[] fileData = File.ReadAllBytes(filePath);
                    cache[filename] = fileData; 
                    Console.WriteLine("Serving from disk and caching: " + filename);
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
