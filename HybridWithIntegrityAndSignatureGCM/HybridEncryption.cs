﻿/*
MIT License

Copyright (c) 2020

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Security.Cryptography;

namespace Pluralsight.HybridWithIntegrityAndSignatureGCM
{
    public class HybridEncryption
    {
        private readonly AesGCMEncryption _aes = new AesGCMEncryption();

        public static byte[] ComputeHMACSha256(byte[] toBeHashed, byte[] hmacKey)
        {
            using (var hmacSha256 = new HMACSHA256(hmacKey))
            {
                return hmacSha256.ComputeHash(toBeHashed);
            }
        }

        public EncryptedPacket EncryptData(byte[] original, NewRSA rsaParams,
                                           NewDigitalSignature digitalSignature)
        {
            // Create AES session key.
            var sessionKey = _aes.GenerateRandomNumber(32);

            var encryptedPacket = new EncryptedPacket { 
                Iv = _aes.GenerateRandomNumber(12) };

            // Encrypt data with AES-GCM
            (byte[] ciphereText, byte[] tag) encrypted = 
                _aes.Encrypt(original, sessionKey, encryptedPacket.Iv, null);

            encryptedPacket.EncryptedData = encrypted.ciphereText;

            encryptedPacket.Tag = encrypted.tag;

            encryptedPacket.EncryptedSessionKey = rsaParams.Encrypt(sessionKey);

            encryptedPacket.SignatureHMAC = 
                ComputeHMACSha256(
                    Combine(encryptedPacket.EncryptedData, encryptedPacket.Iv), 
                    sessionKey);

            encryptedPacket.Signature = 
                digitalSignature.SignData(encryptedPacket.SignatureHMAC);

            return encryptedPacket;
        }

        public byte[] DecryptData(EncryptedPacket encryptedPacket, NewRSA rsaParams,
                                  NewDigitalSignature digitalSignature)
        {
            var decryptedSessionKey = 
                rsaParams.Decrypt(encryptedPacket.EncryptedSessionKey);

            byte[] newHMAC = ComputeHMACSha256(
                Combine(encryptedPacket.EncryptedData, encryptedPacket.Iv), 
                decryptedSessionKey);

            if (!Compare(encryptedPacket.SignatureHMAC, newHMAC))
            {
                throw new CryptographicException(
                    "HMAC for decryption does not match encrypted packet.");
            }

            if (!digitalSignature.VerifySignature(
                                                encryptedPacket.Signature, 
                                                encryptedPacket.SignatureHMAC))
            {
                throw new CryptographicException(
                    "Digital Signature can not be verified.");
            }

            var decryptedData = _aes.Decrypt(encryptedPacket.EncryptedData, 
                                             decryptedSessionKey,
                                             encryptedPacket.Iv, 
                                             encryptedPacket.Tag, 
                                             null);

            return decryptedData;
        }

        private static byte[] Combine(byte[] first, byte[] second)
        {
            var ret = new byte[first.Length + second.Length];

            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);

            return ret;
        }

        private static bool Compare(byte[] array1, byte[] array2)
        {
            var result = array1.Length == array2.Length;

            for (var i = 0; i < array1.Length && i < array2.Length; ++i)
            {
                result &= array1[i] == array2[i];
            }

            return result;
        }
    }
}
