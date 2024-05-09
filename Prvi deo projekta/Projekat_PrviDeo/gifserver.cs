
using System;
using System.IO;
using System.Net;
using System.Threading;


namespace Projekat_PrviDeo;


public class GifServer
{
    private HttpListener listener;

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
            HttpListenerContext context = listener.GetContext(); // Blokirajući poziv
            Thread thread = new Thread(() => HandleRequest(context));
            thread.Start();
        }
    }

    private void HandleRequest(HttpListenerContext context)
    {
        string filename = context.Request.Url.AbsolutePath.Substring(1); // Skidanje početne kose crte
        string rootDirectory = Directory.GetCurrentDirectory();

        Console.WriteLine(rootDirectory);
        string filePath = SearchForGif(rootDirectory, filename);

        if (filePath != null)
        {
            try
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                context.Response.ContentType = "image/gif";
                context.Response.ContentLength64 = fileData.Length;
                context.Response.OutputStream.Write(fileData, 0, fileData.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error serving file: " + e.Message);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write("Gif not found: " + filename);
            }
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
