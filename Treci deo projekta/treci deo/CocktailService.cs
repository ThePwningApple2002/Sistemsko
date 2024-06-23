using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace treci_deo
{
    public class CocktailService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://www.thecocktaildb.com/api/json/v1/1/";

        public CocktailService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IObservable<string>> GetNonAlcoholicCocktailIngredientsStream(CancellationToken cancellationToken)
        {
            var subject = new Subject<string>();
            var cocktails = await GetNonAlcoholicCocktails();

            _ = Task.Run(async () =>
            {
                try
                {
                    foreach (var cocktail in cocktails)
                    {
                        if (cancellationToken.IsCancellationRequested) break;
                        var ingredients = await GetCocktailIngredients(cocktail);
                        foreach (var ingredient in ingredients)
                        {
                            if (cancellationToken.IsCancellationRequested) break;
                            subject.OnNext(ingredient);
                            await Task.Delay(100, cancellationToken);
                        }
                    }
                }
       
                catch (Exception ex)
                {
                    subject.OnError(ex);
                }
                finally
                {
                    subject.OnCompleted();
                }
            }, cancellationToken);

            return subject.AsObservable();
        }

        private async Task<IEnumerable<string>> GetNonAlcoholicCocktails()
        {
            var response = await _httpClient.GetStringAsync($"{BaseUrl}filter.php?a=Non_Alcoholic");
            var json = JObject.Parse(response);
            return json["drinks"].Select(d => d["idDrink"].ToString());
        }

        private async Task<IEnumerable<string>> GetCocktailIngredients(string cocktailId)
        {
            var response = await _httpClient.GetStringAsync($"{BaseUrl}lookup.php?i={cocktailId}");
            var json = JObject.Parse(response);
            var drink = json["drinks"].First;

            return Enumerable.Range(1, 15)
                .Select(i => drink[$"strIngredient{i}"]?.ToString())
                .Where(i => !string.IsNullOrEmpty(i))
                .Select(i => i.ToLower());
        }
    }
}
