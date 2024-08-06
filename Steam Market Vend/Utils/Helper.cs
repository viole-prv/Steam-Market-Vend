using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Steam_Market_Vend
{
    public class Helper
    {
        public static bool IsValidJson(string _)
        {
            if (string.IsNullOrWhiteSpace(_))
            {
                return false;
            }

            _ = _.Trim();

            if ((_.StartsWith("{") && _.EndsWith("}")) ||
                (_.StartsWith("[") && _.EndsWith("]")))
            {
                try
                {
                    JToken.Parse(_);

                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }

        public static decimal? ToPrice(string _)
        {
            string[] Split = _.Split(' ');

            if (Split.Length > 1)
            {
                string? Last = Split.LastOrDefault();
                string? First = Split.FirstOrDefault();

                if (!string.IsNullOrEmpty(Last) && !string.IsNullOrEmpty(First))
                {
                    if (decimal.TryParse(First, NumberStyles.Currency,

                        Last == "USD" ? CultureInfo.GetCultureInfo("en-US") :
                        Last == "pуб." ? CultureInfo.GetCultureInfo("ru-RU") :
                        Last == "TL" ? CultureInfo.GetCultureInfo("tr-TR") :

                        CultureInfo.CurrentCulture, out decimal Price))
                    {
                        return Math.Ceiling(Price * 100);
                    }
                }
            }

            return null;
        }

        #region Fee Price

        public class IFeePrice
        {
            public int Fee { get; set; }
            public int Buyer { get; set; }
            public int Receive { get; set; }
        }

        public static int GetFeePrice(int Buyer)
        {
            const double WALLET_FEE_PERCENT = 0.05;
            const double WALLET_PUBLISHER_FEE_PERCENT_DEFAULT = 0.10;

            const int WALLET_FEE_MINIMUM = 1;

            bool Under = false;
            int Iteration = 0;

            int Receive = (int)(Buyer / (WALLET_FEE_PERCENT + WALLET_PUBLISHER_FEE_PERCENT_DEFAULT + WALLET_FEE_MINIMUM));

            var X = FeePrice(Receive);

            while (true)
            {
                if (X.Buyer == Buyer || Iteration >= 10) break;

                if (X.Buyer > Buyer)
                {
                    if (Under)
                    {
                        X = FeePrice(Receive - 1);

                        X.Fee += Buyer - X.Buyer;
                        X.Buyer = Buyer;

                        break;
                    }
                    else
                    {
                        Receive -= 1;
                    }
                }
                else
                {
                    Under = true;
                    Receive += 1;
                }

                X = FeePrice(Receive);

                Iteration += 1;
            }

            static IFeePrice FeePrice(int Receive)
            {
                int Fee = (int)Math.Floor(Math.Max(Receive * WALLET_FEE_PERCENT, WALLET_FEE_MINIMUM));
                int PublisherFee = (int)Math.Floor(Math.Max(Receive * WALLET_PUBLISHER_FEE_PERCENT_DEFAULT, WALLET_FEE_MINIMUM));

                return new IFeePrice
                {
                    Fee = Fee,
                    Buyer = Receive + Fee + PublisherFee,
                    Receive = Receive
                };
            }

            return X.Receive;
        }

        #endregion
    }
}
