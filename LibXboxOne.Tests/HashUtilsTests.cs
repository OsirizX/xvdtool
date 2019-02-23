using System;
using System.IO;
using System.Security.Cryptography;
using Xunit;

namespace LibXboxOne.Tests
{
    public static class HashUtilsData
    {
        public static byte[] Data => new byte[]{
                0x48,0x45,0x4c,0x4c,0x4f,0x2c,0x20,0x49,0x54,0x53,0x20,0x4d,0x45,0x2c,0x20,0x54,
                0x45,0x53,0x54,0x44,0x41,0x54,0x41,0x0a,0x01,0x11,0x21,0x31,0x41,0x51,0x61,0x71};

        public static byte[] Sha256Hash => new byte[]{
                0x3C,0x47,0xF6,0x32,0x57,0x72,0xCC,0x80,0xAE,0x72,0x0D,0xA2,0x4C,0x60,0xD8,0x1A,
                0xBF,0xD1,0x7E,0x8A,0x63,0xA8,0xCC,0xDE,0x4B,0xD9,0xF2,0xB8,0xBB,0xC3,0x82,0x0F};

        public static byte[] Sha1Hash => new byte[]{
                0xD7,0x3E,0x77,0x3C,0xE0,0x56,0x82,0x03,0xE8,0x92,0xFD,0xDD,0xBC,0xD8,0xAA,0x3C,
                0x94,0xE2,0x37,0xC2};

        public static byte[] RsaSignature => new byte[]{
                0x52,0xDA,0x68,0x6B,0x69,0x32,0x48,0x01,0x28,0x7A,0x38,0x24,0x84,0xA7,0xC2,0xCD,
                0x9F,0xDC,0xDC,0xA5,0x98,0x59,0x35,0x9D,0x5E,0x76,0xCF,0x4A,0xA7,0xDB,0xF0,0xEE,
                0x12,0x19,0xA6,0x87,0x0A,0x6F,0xF6,0xE1,0x44,0x57,0xC7,0xBE,0x0E,0x36,0x77,0x70,
                0xA3,0xD3,0x95,0xA0,0x07,0xA3,0x59,0x7F,0xCD,0xEF,0xC2,0x8D,0x79,0x4D,0x7A,0x9C,
                0xC0,0xB8,0x12,0xEC,0x1D,0xF1,0x58,0x4C,0x46,0xC5,0x0A,0xEA,0x8D,0x6C,0x51,0xFC,
                0x21,0xFA,0x25,0x76,0xE7,0xAC,0x51,0xD0,0xDC,0x87,0x82,0x89,0x70,0x41,0x79,0x0D,
                0x04,0xF5,0x75,0xD6,0x6B,0x3D,0xE0,0x77,0x79,0x86,0xF4,0xD5,0x72,0x3B,0x0F,0xC5,
                0xDF,0x68,0xC6,0x88,0x08,0x71,0x98,0x64,0x20,0xAC,0xE1,0x4A,0x4A,0xE7,0xD9,0xF4};
    }

    public class HashUtilsTests
    {
        [Fact]
        public void TestComputeSha256()
        {
            var result = HashUtils.ComputeSha256(HashUtilsData.Data);

            Assert.Equal(HashUtilsData.Sha256Hash.Length, result.Length);
            Assert.Equal(HashUtilsData.Sha256Hash, result);
        }

        [Fact]
        public void TestComputeSha1()
        {
            var result = HashUtils.ComputeSha1(HashUtilsData.Data);

            Assert.Equal(HashUtilsData.Sha1Hash.Length, result.Length);
            Assert.Equal(HashUtilsData.Sha1Hash, result);
        }

        byte[] GetRsaBlob(string blobType, int bits)
        {
            return ResourcesProvider.GetBytes($"RSA_{bits}_{blobType}.bin", ResourceType.RsaKeys);
        }

        [Theory]
        // [InlineData(1024, "RSAPRIVATEBLOB")]
        [InlineData(1024, "RSAFULLPRIVATEBLOB")]
        public void TestSignData(int bits, string blobType)
        {
            var rsaBlob = GetRsaBlob(blobType, bits);
            bool result = HashUtils.SignData(rsaBlob, blobType, HashUtilsData.Data,
                                             out byte[] signature);
            Assert.True(result);
            Assert.NotEqual(HashUtilsData.RsaSignature, signature); // Cant be same, due to PSS


            result = HashUtils.VerifySignature(rsaBlob, blobType, signature,
                                                HashUtilsData.Data);
            Assert.True(result);
        }

        [Fact]
        public void TestSignDataFailPubKey()
        {
            var rsaBlob = GetRsaBlob("RSAPUBLICBLOB", 1024);

            Assert.Throws<CryptographicException>(() =>
            {
                HashUtils.SignData(rsaBlob, "RSAPUBLICBLOB", HashUtilsData.Data,
                                             out byte[] signature);
            });
        }

        [Theory]
        [InlineData(1024, "RSAPUBLICBLOB")]
        [InlineData(1024, "RSAPRIVATEBLOB")]
        [InlineData(1024, "RSAFULLPRIVATEBLOB")]
        public void TestVerifySignature(int bits, string blobType)
        {
            var rsaBlob = GetRsaBlob(blobType, bits);
            bool success = HashUtils.VerifySignature(rsaBlob, blobType, HashUtilsData.RsaSignature,
                                                    HashUtilsData.Data);

            Assert.True(success);
        }
    }
}