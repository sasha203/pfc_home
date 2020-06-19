using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;

namespace pfc_Home.Hashing
{
    public class RSAHash
    {
        RSACryptoServiceProvider RSA;

        public RSAHash()
        {
            RSA = new RSACryptoServiceProvider();
        }
        

        #region Return keys in XML Format

        public string GetPublicKey()
        {
            return RSA.ToXmlString(false);
        }

        public string GetPrivateKey()
        {
            return RSA.ToXmlString(true);
        }
        #endregion



        static bool _optimalAsymmetricEncryptionPadding = false;

        public int GetMaxDataLength(int keySize)
        {
            if (_optimalAsymmetricEncryptionPadding)
            {
                return ((keySize - 384) / 8) + 7;
            }
            return ((keySize - 384) / 8) + 37;
        }


        public bool IsKeySizeValid(int keySize)
        {
            return keySize >= 384 &&
                   keySize <= 16384 &&
                   keySize % 8 == 0;
        }


        byte[] EncryptByteArray(byte[] data, string publicKeyXml, int keySize)
        {
            try
            {
                using (var provider = new RSACryptoServiceProvider(keySize))
                {
                    provider.FromXmlString(publicKeyXml);
                    return provider.Encrypt(data, _optimalAsymmetricEncryptionPadding);
                }
            }
            catch (Exception e)
            {
                //Add message in errorLog.
                return null;
            }
        }


        //Encrypts the input
        public string Encrypt(string text, string publicKeyXml, int keySize)
        {
            byte[] encrypted;

            try
             {
                encrypted = EncryptByteArray(Encoding.UTF8.GetBytes(text), publicKeyXml, keySize);

            }
            catch (Exception e)
            {
                //Add message in errorLog.
                return null;
            }

            return Convert.ToBase64String(encrypted);
        }


        private byte[] DecryptByteArray(byte[] data, string publicAndPrivateKeyXml, int keySize)
        {

            try
            {
                using (var provider = new RSACryptoServiceProvider(keySize))
                {
                    provider.FromXmlString(publicAndPrivateKeyXml);
                    return provider.Decrypt(data, _optimalAsymmetricEncryptionPadding);
                }
            }
            catch (Exception e)
            {
                //Add message in errorLog.
                return null;
            }

        }

        //Decrypts the input
        public string Decrypt(string text, string publicAndPrivateKeyXml, int keySize)
        {
            byte[] decrypted;

            try
            {
                decrypted = DecryptByteArray(Convert.FromBase64String(text), publicAndPrivateKeyXml, keySize);
            }
            catch (Exception e)
            {
                //Add message in errorLog.
                return null;
            }

            return Encoding.UTF8.GetString(decrypted);
        }


    }
}