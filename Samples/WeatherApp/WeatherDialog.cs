using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Framework.Builder.Witai;
using Microsoft.Bot.Framework.Builder.Witai.Dialogs;
using Microsoft.Bot.Framework.Builder.Witai.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace WitWeather
{
    [Serializable]
    [WitModel("Access Token")]
    public class WeatherDialog : WitDialog<object>
    {
        [WitAction("getMyForecast")]
        public async Task GetForecast(IDialogContext context, WitResult result)
        {
            //adding location to context
            this.WitContext["location"] =  result.Entities["location"][0].Value;

            //yahoo weather API
            var temp = await GetWeather(this.WitContext["location"]);

            //adding temp to context
            this.WitContext["forecast"] =  temp;
        }

        private async Task<int> GetWeather(object location)
        {
            //weather API request
            //the loop is because the apis don't always return a result
            while (true)
            {
                try
                {
                    var client = new HttpClient();
                    var response = await client.GetAsync($"https://query.yahooapis.com/v1/public/yql?q=select%20item.condition%20from%20weather.forecast%20where%20woeid%20in%20(select%20woeid%20from%20geo.places(1)%20where%20text%3D%22{location}%22)&format=json&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys");
                    var json = await response.Content.ReadAsStringAsync();
                    dynamic data = JsonConvert.DeserializeObject(json);
                    return (Convert.ToInt32(data.query.results.channel.item.condition.temp) - 32) * 5 / 9;
                }
                catch (Exception) { }
            }
        }
    }
}