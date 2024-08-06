using Newtonsoft.Json;

namespace Steam_Market_Vend
{
    public class IConfig
    {
        [JsonIgnore]
        private static string? File { get; set; }

        private static readonly SemaphoreSlim Semaphore = new(1, 1);

        [JsonProperty]
        public long SteamID { get; set; }

        public bool ShouldSerializeSteamID() => SteamID > 0;

        [JsonProperty]
        public uint AppID { get; set; }

        public bool ShouldSerializeAppID() => AppID > 0;

        [JsonProperty]
        public uint ContextID { get; set; }

        public bool ShouldSerializeContextID() => ContextID > 0;   
        
        [JsonProperty]
        public string? Country { get; set; }

        public bool ShouldSerializeCountry() => !string.IsNullOrEmpty(Country);     
        
        [JsonProperty]
        public uint Currency { get; set; }

        public bool ShouldSerializeCurrency() => Currency > 0;

        public static (string? ErrorMessage, IConfig? Config) Load(string _File)
        {
            File = _File;

            if (!string.IsNullOrEmpty(File) && !System.IO.File.Exists(File))
            {
                System.IO.File.WriteAllText(File, JsonConvert.SerializeObject(new IConfig(), Formatting.Indented));
            }

            string Json;

            try
            {
                Json = System.IO.File.ReadAllText(File);
            }
            catch (Exception e)
            {
                return (e.Message, null);
            }

            if (string.IsNullOrEmpty(Json) || Json.Length == 0)
            {
                return ("Данные равны нулю!", null);
            }

            IConfig Config;

            try
            {
                Config = JsonConvert.DeserializeObject<IConfig>(Json)!;
            }
            catch (Exception e)
            {
                return (e.Message, null);
            }

            if (Config == null)
            {
                return ("Глобальный конфиг равен нулю!", null);
            }

            return (null, Config);
        }

        public async void Save()
        {
            if (string.IsNullOrEmpty(File) || (this == null)) return;

            string JSON = JsonConvert.SerializeObject(this, Formatting.Indented);
            string _ = File + ".new";

            await Semaphore.WaitAsync();

            try
            {
                System.IO.File.WriteAllText(_, JSON);

                if (System.IO.File.Exists(File))
                {
                    System.IO.File.Replace(_, File, null);
                }
                else
                {
                    System.IO.File.Move(_, File);
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}
