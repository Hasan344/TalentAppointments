using AMASServiceReference;
using ForQab.Service.Abstract;
using Microsoft.Extensions.Logging;

namespace ForQab.Service.Concrete
{
    public class AmasPhotoService : IAmasPhotoService
    {
        private readonly ILogger<AmasPhotoService> _logger;

        public AmasPhotoService(ILogger<AmasPhotoService> logger)
        {
            _logger = logger;
        }

        public async Task<string?> FetchPhotoAsBase64Async(string fin, string serialPrefix, string serial)
        {
            // Input-ları normalizasiya et
            fin = (fin ?? "").Trim();
            serialPrefix = (serialPrefix ?? "").Trim();
            serial = (serial ?? "").Trim();

            if (string.IsNullOrEmpty(fin) ||
                string.IsNullOrEmpty(serialPrefix) ||
                string.IsNullOrEmpty(serial))
            {
                _logger.LogWarning(
                    "AMAS: FIN, SerialPrefix və ya Serial boşdur. FIN={FIN}, Prefix={Prefix}, Serial={Serial}",
                    fin, serialPrefix, serial);
                return null;
            }

            // ─── Yeni nəsil vs köhnə nəsil ID ayrı işlənir ───
            // Yeni nəsil (AA, AB):
            //     SERIYA = ""  (boş)
            //     Number = SerialPrefix + Serial  (məs. "AA" + "1234567" = "AA1234567")
            // Köhnə nəsil (AZE, MYİ və s.):
            //     SERIYA = SerialPrefix
            //     Number = Serial
            string seriya;
            string number;

            bool isNewGeneration =
                serialPrefix.Equals("AA", StringComparison.OrdinalIgnoreCase) ||
                serialPrefix.Equals("AB", StringComparison.OrdinalIgnoreCase);

            if (isNewGeneration)
            {
                seriya = "";
                number = serialPrefix + serial;
            }
            else
            {
                seriya = serialPrefix;
                number = serial;
            }

            try
            {
                CommonServiceHeader header = new()
                {
                    Username = "GetPhotoFromIAMAS",
                    Password = "get@photo"
                };

                GetPhotoFromIAMASSoapClient client = new(
                    header,
                    GetPhotoFromIAMASSoapClient.EndpointConfiguration.GetPhotoFromIAMASSoap);

                GetPhotoFromIAMAS24Request request = new()
                {
                    CommonServiceHeader = header,
                    FIN = fin,
                    SERIYA = seriya,
                    Number = number,
                    SAA = ""
                };

                _logger.LogInformation(
                    "AMAS sorğu göndərilir: FIN={FIN}, SERIYA='{SERIYA}', Number={Number} (yeni nəsil={IsNew})",
                    fin, seriya, number, isNewGeneration);

                var result = await client.GetPhotoFromIAMAS24Async(request);

                if (result == null)
                {
                    _logger.LogWarning("AMAS: response null. FIN={FIN}", fin);
                    return null;
                }

                // AMAS xəta cavabı (s sahəsi dolmuşsa, şəxs/şəkil tapılmadı)
                if (!string.IsNullOrEmpty(result.s))
                {
                    _logger.LogWarning(
                        "AMAS xəta cavabı: FIN={FIN}, SERIYA='{SERIYA}', Number={Number}, s={S}",
                        fin, seriya, number, result.s);
                    return null;
                }

                if (result.GetPhotoFromIAMAS24Result == null ||
                    result.GetPhotoFromIAMAS24Result.Length == 0)
                {
                    _logger.LogWarning(
                        "AMAS: şəkil byte[] boşdur. FIN={FIN}, SERIYA='{SERIYA}', Number={Number}",
                        fin, seriya, number);
                    return null;
                }

                string base64 = Convert.ToBase64String(result.GetPhotoFromIAMAS24Result);
                _logger.LogInformation(
                    "AMAS: şəkil uğurla alındı. FIN={FIN}, byteLen={Len}",
                    fin, result.GetPhotoFromIAMAS24Result.Length);

                return $"data:image/jpeg;base64,{base64}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "AMAS SOAP xətası. FIN={FIN}, SERIYA='{SERIYA}', Number={Number}",
                    fin, seriya, number);
                return null;
            }
        }
    }
}