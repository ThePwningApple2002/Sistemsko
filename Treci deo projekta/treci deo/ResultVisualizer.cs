using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordCloudSharp;

namespace treci_deo
{
    public class ResultVisualizer
    {
        public void DisplayResults(Dictionary<string, int> analysis)
        {
            Console.WriteLine("\nTrenutni top 10 najčešćih sastojaka u bezalkoholnim koktelima:");
            foreach (var (ingredient, count) in analysis.Take(10))
            {
                Console.WriteLine($"{ingredient}: {count}");
            }
        }

        public void GenerateWordCloud(Dictionary<string, int> analysis)
        {
            var wordCloud = new WordCloud(800, 400);
            var words = analysis.Select(kvp => kvp.Key).ToList();
            var frequencies = analysis.Select(kvp => kvp.Value).ToList(); // Promenjeno u int umesto float

            using (var bitmap = wordCloud.Draw(words, frequencies))
            {
                var outputPath = Path.Combine(Environment.CurrentDirectory, "word_cloud.png");
                bitmap.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);
                Console.WriteLine($"\nNovi Word Cloud je generisan i sačuvan kao: {outputPath}");
            }
        }
    }
}
