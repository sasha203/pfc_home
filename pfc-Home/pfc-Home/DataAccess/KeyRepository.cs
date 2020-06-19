using Google.Cloud.Kms.V1;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace pfc_Home.DataAccess
{
    public class KeyRepository
    {

        string projectId;
        string locationId;
        string keyRingId;
        string keyId;


        public KeyRepository()
        {
            projectId = "pfc-home";
            locationId = "us-west1";
            keyRingId = "pfc-keyring";
            keyId = "pfc-key1";
        }


        public string Encrypt(string plaintext)
        {
            // Create the client.
            KeyManagementServiceClient client = KeyManagementServiceClient.Create();

            // Build the key name.
            CryptoKeyName keyName = new CryptoKeyName(projectId, locationId, keyRingId, keyId);

            //Encrypt data 
            string cipher = client.Encrypt(keyName, ByteString.CopyFromUtf8(plaintext)).Ciphertext.ToBase64();


            return cipher;
        }


        public string Decrypt(string cipher)
        {

            // Create the client.
            KeyManagementServiceClient client = KeyManagementServiceClient.Create();

            // Build the key name.
            CryptoKeyName keyName = new CryptoKeyName(projectId, locationId, keyRingId, keyId);


            DecryptResponse result = client.Decrypt(keyName, ByteString.FromBase64(cipher));

            //convert the result to byteArray
            byte[] plaintext = result.Plaintext.ToByteArray();

            return Encoding.UTF8.GetString(plaintext);
        }




    }
}