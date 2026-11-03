using DotNetNinja.Garage.Data;

namespace DotNetNinja.Garage.Constants;

public static class Defaults
{
    public const string DefaultPageSlug = "start";
    public const string DefaultPageText = "Start";


    public static LinkPage CreateDefaultPage() => new LinkPage
    {
        UserId = string.Empty,
        IsDefault = true,
        Slug = Defaults.DefaultPageSlug,
        Text = Defaults.DefaultPageText,
        SortIndex = 1,
        Groups =
        [
            new LinkGroup
        {
            SortIndex = 1,
            Text = "Search and Information",
            Links =
            [
                new LinkItem
                {
                    SortIndex = 1,
                    Href = "https://www.google.com",
                    Text = "Google",
                    Icon = Icons.Google,
                    IconColor = Colors.RoyalBlue
                },
                new LinkItem
                {
                    SortIndex = 2,
                    Href = "https://www.bing.com",
                    Text = "Bing",
                    Icon = Icons.Bing,
                    IconColor = Colors.DodgerBlue
                },
                new LinkItem
                {
                    SortIndex = 3,
                    Href = "https://www.wikipedia.org",
                    Text = "Wikipedia",
                    Icon = Icons.Wikipedia,
                    IconColor = Colors.Black
                },
                new LinkItem
                {
                    SortIndex = 4,
                    Href = "https://maps.google.com",
                    Text = "Google Maps",
                    Icon = Icons.PinMap,
                    IconColor = Colors.ForestGreen
                },
                new LinkItem
                {
                    SortIndex = 5,
                    Href = "https://translate.google.com",
                    Text = "Google Translate",
                    Icon = Icons.Recycle,
                    IconColor = Colors.RoyalBlue
                }
            ]
        },
        new LinkGroup
        {
            SortIndex = 2,
            Text = "News and Weather",
            Links =
            [
                new LinkItem
                {
                    SortIndex = 1,
                    Href = "https://apnews.com",
                    Text = "AP News",
                    Icon = Icons.Newspaper,
                    IconColor = Colors.Black
                },
                new LinkItem
                {
                    SortIndex = 2,
                    Href = "https://www.reuters.com",
                    Text = "Reuters",
                    Icon = Icons.Newspaper,
                    IconColor = Colors.DarkOrange
                },
                new LinkItem
                {
                    SortIndex = 3,
                    Href = "https://weather.com",
                    Text = "Weather.com",
                    Icon = Icons.CloudSun,
                    IconColor = Colors.DodgerBlue
                },
                new LinkItem
                {
                    SortIndex = 4,
                    Href = "https://www.accuweather.com",
                    Text = "AccuWeather",
                    Icon = Icons.Sun,
                    IconColor = Colors.Orange
                },
                new LinkItem
                {
                    SortIndex = 5,
                    Href = "https://news.google.com",
                    Text = "Google News",
                    Icon = Icons.Google,
                    IconColor = Colors.RoyalBlue
                }
            ]
        },
        new LinkGroup
        {
            SortIndex = 3,
            Text = "Shopping and Services",
            Links =
            [
                new LinkItem
                {
                    SortIndex = 1,
                    Href = "https://www.amazon.com",
                    Text = "Amazon",
                    Icon = Icons.Amazon,
                    IconColor = Colors.DarkOrange
                },
                new LinkItem
                {
                    SortIndex = 2,
                    Href = "https://www.walmart.com",
                    Text = "Walmart",
                    Icon = Icons.Cart,
                    IconColor = Colors.RoyalBlue
                },
                new LinkItem
                {
                    SortIndex = 3,
                    Href = "https://www.target.com",
                    Text = "Target",
                    Icon = Icons.Bullseye,
                    IconColor = Colors.Red
                },
                new LinkItem
                {
                    SortIndex = 4,
                    Href = "https://www.ebay.com",
                    Text = "eBay",
                    Icon = Icons.Bag,
                    IconColor = Colors.RoyalBlue
                },
                new LinkItem
                {
                    SortIndex = 5,
                    Href = "https://www.usps.com",
                    Text = "USPS",
                    Icon = Icons.Envelope,
                    IconColor = Colors.Navy
                }
            ]
        },
        new LinkGroup
        {
            SortIndex = 4,
            Text = "Entertainment and Social",
            Links =
            [
                new LinkItem
                {
                    SortIndex = 1,
                    Href = "https://www.youtube.com",
                    Text = "YouTube",
                    Icon = Icons.Youtube,
                    IconColor = Colors.Red
                },
                new LinkItem
                {
                    SortIndex = 2,
                    Href = "https://www.reddit.com",
                    Text = "Reddit",
                    Icon = Icons.Reddit,
                    IconColor = Colors.OrangeRed
                },
                new LinkItem
                {
                    SortIndex = 3,
                    Href = "https://www.facebook.com",
                    Text = "Facebook",
                    Icon = Icons.Facebook,
                    IconColor = Colors.RoyalBlue
                },
                new LinkItem
                {
                    SortIndex = 4,
                    Href = "https://www.instagram.com",
                    Text = "Instagram",
                    Icon = Icons.Instagram,
                    IconColor = Colors.DeepPink
                },
                new LinkItem
                {
                    SortIndex = 5,
                    Href = "https://www.imdb.com",
                    Text = "IMDb",
                    Icon = Icons.Film,
                    IconColor = Colors.Gold
                }
            ]
        }
        ]
    };

}
