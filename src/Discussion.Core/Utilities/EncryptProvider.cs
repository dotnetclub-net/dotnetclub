using System.Security.Cryptography;

namespace Discussion.Core.Utilities
{
    public class EncryptProvider
    {
        /// <summary>
        /// Create RSAParameters
        /// </summary>
        /// <returns></returns>
        public static RSAParameters GenerateParameters()
        {
            using (var key = new RSACryptoServiceProvider(2048))
            {
                return key.ExportParameters(true);
            }
        }
    }
}