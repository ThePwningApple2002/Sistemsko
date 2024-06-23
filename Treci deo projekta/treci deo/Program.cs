using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WordCloudSharp;

namespace treci_deo
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            using var cts = new CancellationTokenSource();
            var cocktailService = new CocktailService(new HttpClient());
            var analyzer = new IngredientAnalyzer();
            var visualizer = new ResultVisualizer();

            Console.WriteLine("Započinjem analizu sastojaka bezalkoholnih koktela...");
            Console.WriteLine("Pritisnite 'Q' za izlazak iz programa.");

            var ingredientStream = await cocktailService.GetNonAlcoholicCocktailIngredientsStream(cts.Token);

            var subscription = ingredientStream
                .Buffer(TimeSpan.FromSeconds(5))
                .Select(ingredients => analyzer.AnalyzeIngredients(ingredients))
                .Subscribe(
                    analysis =>
                    {
                        visualizer.DisplayResults(analysis);
                        visualizer.GenerateWordCloud(analysis);
                    },
                    ex => Console.WriteLine($"Došlo je do greške: {ex.Message}"),
                    () => Console.WriteLine("Obrada završena.")
                );

            while (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Q)
            {
                await Task.Delay(100);
            }

            cts.Cancel();
            subscription.Dispose();
            Console.WriteLine("Program završen.");
        }


    }

}