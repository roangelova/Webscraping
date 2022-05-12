using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Globalization;
using WebScraping;

//Load the document
HtmlDocument data = new HtmlDocument();
data.Load("data.html");

//Extract data
var productNames = data.DocumentNode.SelectNodes("//div[@class='item']//figure//a")
    .Select(x => x.Element("img").GetAttributeValue("alt", "")).ToList();

var productPrices = data.DocumentNode.SelectNodes("//div[@class='pricing']/p/span")
    .ToArray().Select(x => x.InnerText).ToList();

var productRatings = data.DocumentNode.SelectNodes("/div[@class='item']").ToArray()
    .Select(x => x.Attributes["rating"].Value)
    .ToList();

//Prepare data 
var ratings = productRatings.Select(x => decimal.Parse(x, CultureInfo.InvariantCulture)).ToList();
var normalizedRatings = new List<decimal>();

foreach (var rating in ratings)
{
    if (rating > 10.00m)
    {
        normalizedRatings.Add((rating / 100) * 5.0m);
    }
    else if (rating > 5.00m)
    {
        normalizedRatings.Add((rating / 10) * 5.0m);
    }
    else
    {
        normalizedRatings.Add(rating);
    }
}

var decodedNames = productNames.Select(x => HtmlAgilityPack.HtmlEntity.DeEntitize(x));

//Initialize data 
var items = new List<Item>();

for (int i = 0; i < productNames.Count; i++)
{
    var priceElements = productPrices.ElementAt(i).Split("$", StringSplitOptions.RemoveEmptyEntries).ToArray();

    items.Add(new Item
    {
        ProductName = decodedNames.ElementAt(i),
        Price = priceElements[1].Replace(",", ""),
        Rating = Math.Round(normalizedRatings.ElementAt(i), 1)
    });
}

//Serialize data
var ItemsAsJson = JsonConvert.SerializeObject(items, Formatting.Indented);

Console.WriteLine(ItemsAsJson);