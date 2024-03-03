using Newtonsoft.Json;
using RestSharp;
using System.Net;
using Viole_Logger_Interactive;

namespace SteamMarketVend
{
    public partial class Program
    {
        private readonly static Logger Logger = new();

        private static readonly string ConfigDirectory = "config";
        private static readonly string ConfigFile = Path.Combine(ConfigDirectory, "config.json");

        private static IConfig? Config;

        private static bool QUICK;
        private static bool FOIL;

        #region Cookie

        public partial class ICookie
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

            (string? ErrorMessage, Config) = IConfig.Load(ConfigDirectory, ConfigFile);

            if (Config == null)
            {
                Logger.LogGenericError(ErrorMessage);

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

            if (Logger.Helper.IsValidJson(JSON))
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

        public enum EResult
        {
            Invalid = 0,
            OK = 1,
            Fail = 2,
            NoConnection = 3,
            InvalidPassword = 5,
            LoggedInElsewhere = 6,
            InvalidProtocolVer = 7,
            InvalidParam = 8,
            FileNotFound = 9,
            Busy = 10,
            InvalidState = 11,
            InvalidName = 12,
            InvalidEmail = 13,
            DuplicateName = 14,
            AccessDenied = 15,
            Timeout = 16,
            Banned = 17,
            AccountNotFound = 18,
            InvalidSteamID = 19,
            ServiceUnavailable = 20,
            NotLoggedOn = 21,
            Pending = 22,
            EncryptionFailure = 23,
            InsufficientPrivilege = 24,
            LimitExceeded = 25,
            Revoked = 26,
            Expired = 27,
            AlreadyRedeemed = 28,
            DuplicateRequest = 29,
            AlreadyOwned = 30,
            IPNotFound = 31,
            PersistFailed = 32,
            LockingFailed = 33,
            LogonSessionReplaced = 34,
            ConnectFailed = 35,
            HandshakeFailed = 36,
            IOFailure = 37,
            RemoteDisconnect = 38,
            ShoppingCartNotFound = 39,
            Blocked = 40,
            Ignored = 41,
            NoMatch = 42,
            AccountDisabled = 43,
            ServiceReadOnly = 44,
            AccountNotFeatured = 45,
            AdministratorOK = 46,
            ContentVersion = 47,
            TryAnotherCM = 48,
            PasswordRequiredToKickSession = 49,
            AlreadyLoggedInElsewhere = 50,
            Suspended = 51,
            Cancelled = 52,
            DataCorruption = 53,
            DiskFull = 54,
            RemoteCallFailed = 55,
            PasswordUnset = 56,
            ExternalAccountUnlinked = 57,
            PSNTicketInvalid = 58,
            ExternalAccountAlreadyLinked = 59,
            RemoteFileConflict = 60,
            IllegalPassword = 61,
            SameAsPreviousValue = 62,
            AccountLogonDenied = 63,
            CannotUseOldPassword = 64,
            InvalidLoginAuthCode = 65,
            AccountLogonDeniedNoMail = 66,
            HardwareNotCapableOfIPT = 67,
            IPTInitError = 68,
            ParentalControlRestricted = 69,
            FacebookQueryError = 70,
            ExpiredLoginAuthCode = 71,
            IPLoginRestrictionFailed = 72,
            AccountLockedDown = 73,
            AccountLogonDeniedVerifiedEmailRequired = 74,
            NoMatchingURL = 75,
            BadResponse = 76,
            RequirePasswordReEntry = 77,
            ValueOutOfRange = 78,
            UnexpectedError = 79,
            Disabled = 80,
            InvalidCEGSubmission = 81,
            RestrictedDevice = 82,
            RegionLocked = 83,
            RateLimitExceeded = 84,
            AccountLoginDeniedNeedTwoFactor = 85,
            ItemDeleted = 86,
            AccountLoginDeniedThrottle = 87,
            TwoFactorCodeMismatch = 88,
            TwoFactorActivationCodeMismatch = 89,
            AccountAssociatedToMultiplePartners = 90,
            NotModified = 91,
            NoMobileDevice = 92,
            TimeNotSynced = 93,
            SMSCodeFailed = 94,
            AccountLimitExceeded = 95,
            AccountActivityLimitExceeded = 96,
            PhoneActivityLimitExceeded = 97,
            RefundToWallet = 98,
            EmailSendFailure = 99,
            NotSettled = 100,
            NeedCaptcha = 101,
            GSLTDenied = 102,
            GSOwnerDenied = 103,
            InvalidItemType = 104,
            IPBanned = 105,
            GSLTExpired = 106,
            InsufficientFunds = 107,
            TooManyPending = 108,
            NoSiteLicensesFound = 109,
            WGNetworkSendExceeded = 110,
            AccountNotFriends = 111,
            LimitedUserAccount = 112,
            CantRemoveItem = 113,
            AccountHasBeenDeleted = 114,
            AccountHasAnExistingUserCancelledLicense = 115,
            DeniedDueToCommunityCooldown = 116,
        }

        #region Inventory

        public class IInventory
        {
            #region Asset

            [JsonProperty("assets")]
            public List<IAsset>? Asset { get; set; }

            public class IAsset
            {
                [JsonProperty("classid")]
                public string? ClassID { get; set; }

                [JsonProperty("assetid")]
                public string? AssetID { get; set; }
            }

            #endregion

            #region Description

            [JsonProperty("descriptions")]
            public List<IDescription>? Description { get; set; }

            public class IDescription
            {
                [JsonProperty("classid")]
                public string? ClassID { get; set; }

                [JsonProperty("name")]
                public string? Name { get; set; }

                [JsonProperty("type")]
                public string? Type { get; set; }

                [JsonProperty("market_hash_name")]
                public string? MarketHashName { get; set; }

                [JsonProperty("market_fee_app")]
                public int MarketFeeApp { get; set; }

                [JsonProperty("marketable")]
                public byte Marketable { get; set; }
            }

            #endregion

            [JsonProperty("last_assetid")]
            public string? LastAssetID { get; set; }

            [JsonProperty("success")]
            public EResult Success { get; set; }
        }

        private static async Task Inventory()
        {
            ulong StartAssetID = 0;

            var Asset = new List<IInventory.IAsset>();
            var Description = new List<IInventory.IDescription>();

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

                            if ((int)Execute.StatusCode == 429)
                            {
                                Logger.LogWarning("Слишком много запросов!");

                                await Task.Delay(TimeSpan.FromMinutes(2.5));

                                continue;
                            }

                            if (string.IsNullOrEmpty(Execute.Content))
                            {
                                if (Execute.StatusCode == 0 || Execute.StatusCode == HttpStatusCode.OK)
                                {
                                    Logger.LogWarning("Ответ пуст!");
                                }
                                else
                                {
                                    Logger.LogWarning($"Ошибка: {Execute.StatusCode}.");
                                }
                            }
                            else
                            {
                                if (Execute.StatusCode == 0 || Execute.StatusCode == HttpStatusCode.OK)
                                {
                                    if (Logger.Helper.IsValidJson(Execute.Content))
                                    {
                                        try
                                        {
                                            var JSON = JsonConvert.DeserializeObject<IInventory>(Execute.Content);

                                            if (JSON == null || JSON.Asset == null || JSON.Description == null)
                                            {
                                                Logger.LogWarning($"Ошибка: {Execute.Content}");
                                            }
                                            else
                                            {
                                                if (JSON.Success == EResult.OK)
                                                {
                                                    Asset.AddRange(JSON.Asset);
                                                    Description.AddRange(JSON.Description);

                                                    if (string.IsNullOrEmpty(JSON.LastAssetID) || !ulong.TryParse(JSON.LastAssetID, out ulong LastAssetID))
                                                    {
                                                        Work = false;
                                                    }
                                                    else
                                                    {
                                                        StartAssetID = LastAssetID;
                                                    }

                                                    Console.Title = $"$ {Asset.Count} - {Description.Count} | ID: {StartAssetID}";
                                                }
                                                else
                                                {
                                                    Logger.LogWarning($"Ошибка: {JSON.Success}");
                                                }
                                            }

                                            break;
                                        }
                                        catch (Exception e)
                                        {
                                            Logger.LogException(e);
                                        }
                                    }
                                    else
                                    {
                                        Logger.LogWarning($"Ошибка: {Execute.Content}");
                                    }
                                }
                                else
                                {
                                    Logger.LogWarning($"Ошибка: {Execute.StatusCode}.");
                                }
                            }

                            await Task.Delay(2500);
                        }
                        catch (Exception e)
                        {
                            Logger.LogGenericException(e);
                        }
                    }

                    await Task.Delay(1000);
                }

                Console.Title = $"$ ";
                Console.Clear();

                int Success = 0;
                decimal Approximate = 0;

                foreach ((IInventory.IDescription X, int Index) in Description
                    .Select((x, i) => (X: x, Index: i))
                    .ToList())
                {
                    Console.Title = $"$ {Index}/{Description.Count} | SUCCESS: {Success} ≈ {Math.Round(Approximate / 100, 2)}";

                    if (string.IsNullOrEmpty(X.Type) || string.IsNullOrEmpty(X.ClassID) || string.IsNullOrEmpty(X.MarketHashName)) continue;

                    if (X.Marketable == 0) continue;

                    if (!FOIL && X.Type.Contains("Foil Trading Card")) continue;

                    if (X.Type.Contains("Trading Card"))
                    {
                        foreach (string? AssetID in Asset
                                .Where(x => x.ClassID == X.ClassID)
                                .Select(x => x.AssetID)
                                .ToList())
                        {
                            if (string.IsNullOrEmpty(AssetID)) continue;

                            var Logger = new Logger(AssetID);

                            var Price = await GetPrice(X.MarketHashName, Logger);

                            if (Price > 0)
                            {
                                int FeePrice = Helper.GetFeePrice((int)Price.Value);

                                if (QUICK)
                                {
                                    FeePrice -= 1;
                                }

                                if (FeePrice > 0)
                                {
                                    var Vend = await GetVend(AssetID, FeePrice, Logger);

                                    if (Vend == null) continue;

                                    string Message = $"{X.Name} - {Math.Round((double)FeePrice / 100, 2)}{(Vend.Success ? (Vend.Confirmation == 1 ? $" | {(Vend.Mobile ? "Подтверждение по мобильному телефону" : Vend.EMail ? "Подтверждение по электронной почте" : "Неизвестно")}" : "") : $" | Ошибка: {Vend.Message?.ToUpper()}")}";

                                    if (Vend.Success)
                                    {
                                        Success++;

                                        Approximate += FeePrice;

                                        Logger.LogInfo(Message);
                                    }
                                    else
                                    {
                                        Logger.LogError(Message);
                                    }

                                    Console.Title = $"$ {Index}/{Description.Count} | SUCCESS: {Success} ≈ {Math.Round(Approximate / 100, 2)}";

                                    if (QUICK) break;

                                    Thread.Sleep(2500);
                                }
                            }
                        }

                        Thread.Sleep(2500);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogGenericException(e);
            }
        }

        #endregion

        #region Price

        public class IPrice
        {
            [JsonProperty("success")]
            public EResult Success { get; set; }

            [JsonProperty("lowest_price")]
            public string? Price { get; set; }
        }

        private static async Task<decimal?> GetPrice(string MarketHashName, Logger Logger)
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

                    if ((int)Execute.StatusCode == 429)
                    {
                        Logger.LogWarning("Слишком много запросов!");

                        await Task.Delay(TimeSpan.FromMinutes(2.5));

                        continue;
                    }

                    if (string.IsNullOrEmpty(Execute.Content))
                    {
                        if (Execute.StatusCode == 0 || Execute.StatusCode == HttpStatusCode.OK)
                        {
                            Logger.LogWarning("Ответ пуст!");
                        }
                        else
                        {
                            Logger.LogWarning($"Ошибка: {Execute.StatusCode}.");
                        }
                    }
                    else
                    {
                        if (Execute.StatusCode == 0 || Execute.StatusCode == HttpStatusCode.OK)
                        {
                            if (Logger.Helper.IsValidJson(Execute.Content))
                            {
                                try
                                {
                                    var JSON = JsonConvert.DeserializeObject<IPrice>(Execute.Content);

                                    if (JSON == null)
                                    {
                                        Logger.LogWarning($"Ошибка: {Execute.Content}");
                                    }
                                    else
                                    {
                                        if (JSON.Success == EResult.OK)
                                        {
                                            if (string.IsNullOrEmpty(JSON.Price))
                                            {
                                                Logger.LogWarning($"Ошибка: Цена не установлена!");
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    return Helper.ToPrice(JSON.Price);
                                                }
                                                catch (FormatException)
                                                {
                                                    Logger.LogGenericError($"Ошибка: Не удалось преобразовать строку в числовой тип данных: {JsonConvert.SerializeObject(JSON, Formatting.Indented)}");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Logger.LogWarning($"Ошибка: {JSON.Success}");
                                        }
                                    }

                                    break;
                                }
                                catch (Exception e)
                                {
                                    Logger.LogException(e);
                                }
                            }
                            else
                            {
                                Logger.LogWarning($"Ошибка: {Execute.Content}");
                            }
                        }
                        else
                        {
                            Logger.LogWarning($"Ошибка: {Execute.StatusCode}.");
                        }
                    }

                    await Task.Delay(2500);
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            }

            return 0;
        }

        #endregion

        #region Vend

        public class IVend
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

        private static async Task<IVend?> GetVend(string AssetID, int Price, Logger Logger)
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
            Request.AddParameter("assetid", AssetID);
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

                    if ((int)Execute.StatusCode == 429)
                    {
                        Logger.LogWarning("Слишком много запросов!");

                        await Task.Delay(TimeSpan.FromMinutes(2.5));

                        continue;
                    }

                    if (string.IsNullOrEmpty(Execute.Content))
                    {
                        if (Execute.StatusCode == 0 || Execute.StatusCode == HttpStatusCode.OK)
                        {
                            Logger.LogWarning("Ответ пуст!");
                        }
                        else
                        {
                            Logger.LogWarning($"Ошибка: {Execute.StatusCode}.");
                        }
                    }
                    else
                    {
                        if (Execute.StatusCode == 0 || Execute.StatusCode == HttpStatusCode.OK)
                        {
                            if (Logger.Helper.IsValidJson(Execute.Content))
                            {
                                try
                                {
                                    var JSON = JsonConvert.DeserializeObject<IVend>(Execute.Content);

                                    if (JSON == null)
                                    {
                                        Logger.LogWarning($"Ошибка: {Execute.Content}.");
                                    }
                                    else
                                    {
                                        return JSON;
                                    }

                                    break;
                                }
                                catch (Exception e)
                                {
                                    Logger.LogException(e);
                                }
                            }
                            else
                            {
                                Logger.LogWarning($"Ошибка: {Execute.Content}");
                            }
                        }
                        else
                        {
                            Logger.LogWarning($"Ошибка: {Execute.StatusCode}.");
                        }
                    }

                    await Task.Delay(2500);
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            }

            return null;
        }

        #endregion
    }
}