﻿using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Projekat_PrviDeo
{
    public class GifServer
    {
        private HttpListener listener;
        private readonly object fileLock = new object(); // Lock object

        public GifServer()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5050/");
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

            string filePath = SearchForGif(rootDirectory, filename);
            if (filePath != null)
            {
                byte[] fileData = ReadFile(filePath);
                if (fileData != null)
                {
                    ServeFile(context, fileData);
                }
                else
                {
                    SendNotFound(context, filename);
                }
            }
            else
            {
                SendNotFound(context, filename);
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

        private byte[] ReadFile(string filePath)
        {
            lock (fileLock) 
            {
                try
                {
                    return File.ReadAllBytes(filePath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error reading file: " + e.Message);
                    return null;
                }
            }
        }

        private void SendNotFound(HttpListenerContext context, string filename)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write("Gif not found: " + filename);
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
