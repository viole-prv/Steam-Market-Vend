using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;
using RestSharp;
using System.Net;

namespace Steam_Market_Vend
{
    public class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly string ConfigDirectory = "config";
        private static readonly string ConfigFile = Path.Combine(ConfigDirectory, "config.json");

        private static IConfig? Config;

        private static bool EMERGENCY;

        private static bool QUICK;
        private static bool FOIL;

        #region Cookie

        public class ICookie
        {
            [JsonProperty("name")]
            public string? Name { get; set; }

            [JsonProperty("value")]
            public string? Value { get; set; }

            [JsonProperty("path")]
            public string? Path { get; set; }

            [JsonProperty("domain")]
            public string? Domain { get; set; }
        }

        #endregion

        private static List<ICookie> Cookie = new();

        public static void Main()
        {
            Console.Title = "$ ";

            if (!Directory.Exists(ConfigDirectory)) Directory.CreateDirectory(ConfigDirectory);

            var LoggingConfiguration = new LoggingConfiguration();

            LoggingConfiguration.AddRule(
                LogLevel.Info,
                LogLevel.Fatal,
                new ColoredConsoleTarget("ColoredConsole") { Layout = $@"${{date:format=yyyy-MM-dd HH\:mm\:ss}} | ${{logger}} : ${{message}}${{onexception:inner= ${{exception:format=toString,Data}}}}" }
            );

            LogManager.Configuration = LoggingConfiguration;

            (string? ErrorMessage, Config) = IConfig.Load(ConfigFile);

            if (Config == null)
            {
                Logger.Warn(ErrorMessage);

                return;
            }

            #region QUICK

            Console.Write("QUICK: ");

            QUICK = Console.ReadKey(true).Key == ConsoleKey.Enter;

            #endregion

            #region FOIL

            Console.Clear();

            Console.Write("FOIL: ");

            FOIL = Console.ReadKey(true).Key == ConsoleKey.Enter;

            #endregion

            Console.Clear();

        Retry:

            Console.Clear();

            string? JSON = "", Line;

            while (!string.IsNullOrWhiteSpace(Line = Console.ReadLine()))
            {
                JSON += Line;
            }

            if (Helper.IsValidJson(JSON))
            {
                var X = JsonConvert.DeserializeObject<List<ICookie>>(JSON);

                if (X == null || X.Count == 0)
                {
                    goto Retry;
                }

                Cookie = X;
            }
            else
            {
                goto Retry;
            }

            Console.Clear();

            _ = Inventory();

            Console.ReadLine();
        }

        private static async Task Inventory()
        {
            ulong StartAssetID = 0;

            var AssetList = new List<ISteam.IInventory.IAsset>();
            var DescriptionList = new List<ISteam.IInventory.IDescription>();

            try
            {
                var Client = new RestClient(
                    new RestClientOptions()
                    {
                        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Safari/537.36",
                        MaxTimeout = 300000
                    });

                foreach (var X in Cookie)
                {
                    try
                    {
                        Client.AddCookie(X.Name!, X.Value!, X.Path!, X.Domain!);
                    }
                    catch { }
                }

                bool Work = true;

                while (Work)
                {
                    var Request = new RestRequest($"https://steamcommunity.com/inventory/{Config!.SteamID}/{Config!.AppID}/{Config!.ContextID}?count=5000{(StartAssetID > 0 ? $"&start_assetid={StartAssetID}" : "")}");

                    for (byte i = 0; i < 3; i++)
                    {
                        try
                        {
                            var Execute = await Client.ExecuteGetAsync(Request);

                            if (Execute.StatusCode == HttpStatusCode.TooManyRequests)
                            {
                                Logger.Warn("Слишком много запросов!");

                                return;
                            }

                            if (string.IsNullOrEmpty(Execute.Content))
                            {
                                if (Execute.StatusCode == 0 || Execute.StatusCode == HttpStatusCode.OK)
                                {
                                    Logger.Warn("Ответ пуст!");
                                }
                                else
                                {
                                    Logger.Warn($"Ошибка: {Execute.StatusCode}.");
                                }
                            }
                            else
                            {
                                if (Execute.StatusCode == 0 || Execute.StatusCode == HttpStatusCode.OK)
                                {
                                    if (Helper.IsValidJson(Execute.Content))
                                    {
                                        try
                                        {
                                            var JSON = JsonConvert.DeserializeObject<ISteam.IInventory>(Execute.Content);

                                            if (JSON == null || JSON.AssetList == null || JSON.DescriptionList == null)
                                            {
                                                Logger.Warn($"Ошибка: {Execute.Content}");
                                            }
                                            else
                                            {
                                                if (JSON.Success == ISteam.EResult.OK)
                                                {
                                                    AssetList.AddRange(JSON.AssetList);

                                                    DescriptionList.AddRange(JSON.DescriptionList
                                                        .Where(x => x.Marketable > 0)
                                                        .Where(x => x.Type.Contains("Trading Card"))
                                                    );

                                                    if (ulong.TryParse(JSON.LastAssetID, out ulong LastAssetID))
                                                    {
                                                        StartAssetID = LastAssetID;
                                                    }
                                                    else
                                                    {
                                                        Work = false;
                                                    }

                                                    Console.Title = $"$ {AssetList.Count} - {DescriptionList.Count} | ID: {StartAssetID}";
                                                }
                                                else
                                                {
                                                    Logger.Warn($"Ошибка: {JSON.Success}");
                                                }
                                            }

                                            break;
                                        }
                                        catch (Exception e)
                                        {
                                            Logger.Error(e);
                                        }
                                    }
                                    else
                                    {
                                        Logger.Warn($"Ошибка: {Execute.Content}");
                                    }
                                }
                                else
                                {
                                    Logger.Warn($"Ошибка: {Execute.StatusCode}.");
                                }
                            }

                            await Task.Delay(2500);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                        }
                    }

                    await Task.Delay(1000);
                }

                Console.Title = $"$ ";
                Console.Clear();

                int Success = 0;
                decimal Approximate = 0;

                if (!FOIL) DescriptionList.RemoveAll(x => x.Type.Contains("Foil Trading Card"));

                foreach ((ISteam.IInventory.IDescription Value, int Index) in DescriptionList
                    .Select((x, i) => (Value: x, Index: i + 1))
                    .ToList())
                {
                    if (EMERGENCY) break;

                    Console.Title = $"$ {Index}/{DescriptionList.Count} | SUCCESS: {Success} ≈ {Math.Round(Approximate / 100, 2)}";

                    foreach (string ID in AssetList
                        .Where(x =>
                            x.ClassID == Value.ClassID &&
                            x.InstanceID == Value.InstanceID
                        )
                        .Select(x => x.AssetID)
                        .ToList())
                    {
                        if (EMERGENCY) break;

                        var Bot = new IBot(
                            ID, 
                            Value.Name, 
                            Value.MarketHashName
                        );

                        var Price = await Bot.GetPrice();

                        if (Price.HasValue)
                        {
                            int FeePrice = Helper.GetFeePrice((int)Price.Value);

                            if (QUICK)
                            {
                                FeePrice -= 1;
                            }

                            if (FeePrice > 0)
                            {
                                var T = await Bot.TrySell(FeePrice);

                                if (T)
                                {
                                    Success++;
                                    Approximate += FeePrice;
                                }

                                Console.Title = $"$ {Index}/{DescriptionList.Count} | SUCCESS: {Success} ≈ {Math.Round(Approximate / 100, 2)}";

                                if (QUICK) break;

                                Thread.Sleep(2500);
                            }
                        }
                    }

                    Thread.Sleep(2500);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public class IBot
        {
            private readonly string ID;

            private readonly string Name;
            private readonly string MarketHashName;
               
            private readonly Logger Logger;

            public IBot(string ID, string Name, string MarketHashName)
            {
                this.ID = ID;

                this.Name = Name;
                this.MarketHashName = MarketHashName;

                Logger = LogManager.GetLogger(ID);
            }

            #region Price

            public class IPrice
            {
                [JsonProperty("success")]
                public ISteam.EResult Success { get; set; }

                [JsonProperty("lowest_price")]
                public string? Price { get; set; }
            }

            public async Task<decimal?> GetPrice()
            {
                var Client = new RestClient(
                    new RestClientOptions()
                    {
                        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Safari/537.36",
                        MaxTimeout = 300000
                    });

                var Request = new RestRequest("https://steamcommunity.com/market/priceoverview");

                foreach (var X in Cookie)
                {
                    try
                    {
                        Client.AddCookie(X.Name!, X.Value!, X.Path!, X.Domain!);
                    }
                    catch { }
                }

                Request.AddParameter("country", Config!.Country);
                Request.AddParameter("currency", Config!.Currency);
                Request.AddParameter("appid", Config!.AppID);
                Request.AddParameter("market_hash_name", MarketHashName);

                for (byte i = 0; i < 3; i++)
                {
                    try
                    {
                        var Execute = await Client.ExecuteGetAsync(Request);

                        if (Execute.StatusCode == HttpStatusCode.TooManyRequests)
                        {
                            Logger.Warn("Слишком много запросов!");

                            EMERGENCY = true;

                            break;
                        }

                        if (string.IsNullOrEmpty(Execute.Content))
                        {
                            if (Execute.StatusCode == 0 || Execute.StatusCode == HttpStatusCode.OK)
                            {
                                Logger.Warn("Ответ пуст!");
                            }
                            else
                            {
                                Logger.Warn($"Ошибка: {Execute.StatusCode}.");
                            }
                        }
                        else
                        {
                            if (Execute.StatusCode == 0 || Execute.StatusCode == HttpStatusCode.OK)
                            {
                                if (Helper.IsValidJson(Execute.Content))
                                {
                                    try
                                    {
                                        var JSON = JsonConvert.DeserializeObject<IPrice>(Execute.Content);

                                        if (JSON == null)
                                        {
                                            Logger.Warn($"Ошибка: {Execute.Content}");
                                        }
                                        else
                                        {
                                            if (JSON.Success == ISteam.EResult.OK)
                                            {
                                                if (string.IsNullOrEmpty(JSON.Price))
                                                {
                                                    Logger.Warn($"Ошибка: Цена не установлена!");
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        return Helper.ToPrice(JSON.Price);
                                                    }
                                                    catch (FormatException)
                                                    {
                                                        Logger.Warn($"Ошибка: Не удалось преобразовать строку в числовой тип данных: {JsonConvert.SerializeObject(JSON, Formatting.Indented)}");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Logger.Warn($"Ошибка: {JSON.Success}");
                                            }
                                        }

                                        break;
                                    }
                                    catch (Exception e)
                                    {
                                        Logger.Error(e);
                                    }
                                }
                                else
                                {
                                    Logger.Warn($"Ошибка: {Execute.Content}");
                                }
                            }
                            else
                            {
                                Logger.Warn($"Ошибка: {Execute.StatusCode}.");
                            }
                        }

                        await Task.Delay(2500);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }
                }

                return null;
            }

            #endregion

            #region Sell

            public class ISell
            {
                [JsonProperty("requires_confirmation")]
                public uint Confirmation { get; set; }

                [JsonProperty("needs_mobile_confirmation")]
                public bool Mobile { get; set; }

                [JsonProperty("needs_email_confirmation")]
                public bool EMail { get; set; }

                [JsonProperty("message")]
                public string? Message { get; set; }

                [JsonProperty("success")]
                public bool Success { get; set; }
            }

            public async Task<bool> TrySell(int Price)
            {
                var Client = new RestClient(
                    new RestClientOptions()
                    {
                        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Safari/537.36",
                        MaxTimeout = 300000
                    });

                var Request = new RestRequest("https://steamcommunity.com/market/sellitem/");

                foreach (var X in Cookie)
                {
                    try
                    {
                        Client.AddCookie(X.Name!, X.Value!, X.Path!, X.Domain!);
                    }
                    catch { }
                }

                Request.AddParameter("appid", Config!.AppID);
                Request.AddParameter("contextid", Config!.ContextID);
                Request.AddParameter("assetid", ID);
                Request.AddParameter("amount", 1);
                Request.AddParameter("price", Price);

                foreach (var T in Client.CookieContainer.GetAllCookies().Where(x => x.Name.ToUpper() == "SESSIONID"))
                {
                    Request.AddParameter(T.Name, T.Value);
                }

                Request.AddHeader("referer", $"https://steamcommunity.com/profiles/{Config!.SteamID}/inventory");

                for (byte i = 0; i < 3; i++)
                {
                    try
                    {
                        var Execute = await Client.ExecutePostAsync(Request);

                        if (Execute.StatusCode == HttpStatusCode.TooManyRequests)
                        {
                            Logger.Warn("Слишком много запросов!");

                            EMERGENCY = true;

                            break;
                        }

                        if (string.IsNullOrEmpty(Execute.Content))
                        {
                            if (Execute.StatusCode == 0 || Execute.StatusCode == HttpStatusCode.OK)
                            {
                                Logger.Warn("Ответ пуст!");
                            }
                            else
                            {
                                Logger.Warn($"Ошибка: {Execute.StatusCode}.");
                            }
                        }
                        else
                        {
                            if (Execute.StatusCode == 0 || Execute.StatusCode == HttpStatusCode.OK)
                            {
                                if (Helper.IsValidJson(Execute.Content))
                                {
                                    try
                                    {
                                        var JSON = JsonConvert.DeserializeObject<ISell>(Execute.Content);

                                        if (JSON == null)
                                        {
                                            Logger.Warn($"Ошибка: {Execute.Content}.");
                                        }
                                        else
                                        {
                                            string Message = $"{Name} - {Math.Round((double)Price / 100, 2)}{(JSON.Success ? (JSON.Confirmation == 1 ? $" | {(JSON.Mobile ? "Подтверждение по мобильному телефону" : JSON.EMail ? "Подтверждение по электронной почте" : "Неизвестно")}" : "") : $" | Ошибка: {JSON.Message?.ToUpper()}")}";

                                            if (JSON.Success)
                                            {
                                                Logger.Info(Message);
                                            }
                                            else
                                            {
                                                Logger.Error(Message);
                                            }

                                            return JSON.Success;
                                        }

                                        break;
                                    }
                                    catch (Exception e)
                                    {
                                        Logger.Error(e);
                                    }
                                }
                                else
                                {
                                    Logger.Warn($"Ошибка: {Execute.Content}");
                                }
                            }
                            else
                            {
                                Logger.Warn($"Ошибка: {Execute.StatusCode}.");
                            }
                        }

                        await Task.Delay(2500);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }
                }

                return false;
            }

            #endregion
        }
    }
}