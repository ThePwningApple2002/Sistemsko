using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using WordCloudSharp;

namespace treci_deo
{
    

    public class IngredientAnalyzer
    {
        public Dictionary<string, int> AnalyzeIngredients(IEnumerable<string> ingredients)
        {
            return ingredients
                .GroupBy(i => i)
                .ToDictionary(g => g.Key, g => g.Count())
                .OrderByDescending(kvp => kvp.Value)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
   

}
