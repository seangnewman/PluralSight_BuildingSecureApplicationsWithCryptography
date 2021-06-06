﻿using System;
using System.Security.Cryptography;
using System.Text;

namespace Pluralsight.Asymetric
{
	class NewRSA
    {
        private RSA rsa; 
 
        public NewRSA()
        {
            rsa = RSA.Create(2048);
        }

        public byte[] Encrypt(string dataToEncrypt)
        {
            return rsa.Encrypt(Encoding.UTF8.GetBytes(dataToEncrypt), RSAEncryptionPadding.OaepSHA256);
        }

        public byte[] Encrypt(byte[] dataToEncrypt)
        {
            return rsa.Encrypt(dataToEncrypt, RSAEncryptionPadding.OaepSHA256);
        }

        public byte[] Decrypt(byte[] dataToDecrypt)
        {
            return rsa.Decrypt(dataToDecrypt, RSAEncryptionPadding.OaepSHA256);
        }

        public byte[] ExportPrivateKey(int numberOfIterations, string password)
        {
            byte[] encryptedPrivateKey = new byte[2000];
           
            PbeParameters keyParams = new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, numberOfIterations);

            var arraySpan = new Span<byte>(encryptedPrivateKey);
            rsa.TryExportEncryptedPkcs8PrivateKey(Encoding.UTF8.GetBytes(password), keyParams, arraySpan, out _);

            //encryptedPrivateKey = rsa.ExportEncryptedPkcs8PrivateKey(Encoding.UTF8.GetBytes(password), keyParams);

            return encryptedPrivateKey;
        }

        public void ImportEncryptedPrivateKey(byte[] encryptedKey, string password)
        {
            rsa.ImportEncryptedPkcs8PrivateKey(Encoding.UTF8.GetBytes(password), encryptedKey, out _);
        }

        public byte[] ExportPublicKey()
        {
            return rsa.ExportRSAPublicKey();
        }

        public void ImportPublicKey(byte[] publicKey)
        {
            rsa.ImportRSAPublicKey(publicKey, out _);
        }
    }
}
