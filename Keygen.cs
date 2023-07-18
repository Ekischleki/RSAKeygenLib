
using System.Numerics;
using DataTypeStoreLib;

namespace RSAKeygenLib
{
    public class Keygen
    {
        /// <summary>
        /// Generate an PublicPrivateKeypair object.
        /// </summary>
        /// <param name="byteSize">The byte lenght of the initially generated prime numbers. If you're using this Keypair for securety applications, this should be at least 128. A larger byteSize means more computing time.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static PublicPrivateKeypair GenerateRSAKeypair(int byteSize = 256)
        {
            
            List<BigInteger> primes = GenPrimes.GenLargePrimes(2, Environment.ProcessorCount, byteSize);
            PublicPrivateKeypair result = GenerateRSAKeypair(primes[0], primes[1]);
            if (result.PrivateKey.CryptUsingKeypair(result.PublicKey.CryptUsingKeypair(2)) != 2) throw new Exception("Keypair wasn't generated successfully.");
            return result;
        }
        /// <summary>
        /// Generate a PublicPrivateKeypair object given two user-choosen primes.
        /// </summary>
        /// <param name="prime1"></param>
        /// <param name="prime2"></param>
        /// <returns></returns>
        public static PublicPrivateKeypair GenerateRSAKeypair(BigInteger prime1, BigInteger prime2)
        {
            BigInteger n = prime1 * prime2;
            BigInteger tOfN = (prime1 - 1) * (prime2 - 1);
            BigInteger e = RandomBigInteger.GetRandom(2, tOfN - 1);
            if (e < 0)
                Console.WriteLine($"e should be between 2 and {tOfN - 1}, but it is\n{e}");
            
            while (FindGCF(e, tOfN) != 1)
            {


                e++;
                if (e > tOfN - 1)
                    e = 2;



                if (e > tOfN - 1)
                {
                    e = RandomBigInteger.GetRandom(2, tOfN - 1);
                }
            }
            BigInteger d = FindEGCD(e, tOfN);
            return new PublicPrivateKeypair(d, e, n);
        }
        private static BigInteger FindGCF(BigInteger a, BigInteger b)
        {
            BigInteger mod = 1;
            while (mod != 0)
            {
                mod = a % b;
                a = b;
                b = mod;
            }
            return a;
        }

        private static BigInteger FindEGCD(BigInteger e, BigInteger tOfN) //Extended greatest common divisor algorithm:https://en.wikipedia.org/wiki/Extended_Euclidean_algorithm
        {
            BigInteger o1 = tOfN;
            BigInteger o2 = e;
            BigInteger o3;
            BigInteger t1 = tOfN;
            BigInteger t2 = 1;
            BigInteger t3;

            while (o1 != 1)
            {
                o3 = o1 - (o2 * (o1 / o2));
                t3 = t1 - (t2 * (o1 / o2));
                if (o3 < 0)
                    o3 = ProperBigIntModNegPos(o3, tOfN);
                if (t3 < 0)
                    t3 = ProperBigIntModNegPos(t3, tOfN);
                o1 = o2;
                t1 = t2;
                o2 = o3;
                t2 = t3;
            }
            return t1;
        }
        private static BigInteger ProperBigIntModNegPos(BigInteger a, BigInteger b) //For some weird reason is the mod calculation not handeled properly, neg % pos = pos, not neg
        {
            if (a.Sign != -1)
            {
                throw new Exception("The function ProperBigIntModNegPos can only be used, if a is negativ and b positiv");
            }
            if (b.Sign != 1)
            {
                throw new Exception("The function ProperBigIntModNegPos can only be used, if a is negativ and b positiv");
            }
            BigInteger tempMod = a * -1 % b;
            if (tempMod == 0)
                return 0;
            return b - tempMod;

        }
    }



    public class PublicPrivateKeypair
    {
        public KeyPair PrivateKey;
        public KeyPair PublicKey;

        public PublicPrivateKeypair(Region region)
        {
            List<Region> keyPairsRegions = region.FindSubregionWithNameArray("KeyPair").ToList();
            List<KeyPair> keyPairs = new();
            keyPairsRegions.ForEach(region => keyPairs.Add(new(region)));
            if (keyPairs[0].KeyPairType == KeyPair.PUBLIC)
            { // If first key is the public key, the second key must be private key, else turn it around.
                PublicKey = keyPairs[0];
                PrivateKey = keyPairs[1];
            }
            else
            {
                PublicKey = keyPairs[1];
                PrivateKey = keyPairs[0];
            }

        }

        public Region Save
        {
            get
            {
                Region region = new("PublicPrivateKeyPair", new List<Region>(), new());
                region.SubRegions.Add(PrivateKey.Save);
                region.SubRegions.Add(PublicKey.Save);
                return region;
            }
        }
        public PublicPrivateKeypair(BigInteger aPrivateKey, BigInteger aPublicKey, BigInteger aKeyPart2)
        {
            PrivateKey = new KeyPair(KeyPair.PRIVATE, aPrivateKey, aKeyPart2);
            PublicKey = new KeyPair(KeyPair.PUBLIC, aPublicKey, aKeyPart2);
        }
        public void PrintKeypair()
        {
            Console.WriteLine($"Private Key:\n{PrivateKey.keyPartOne}\nPublic Key:\n{PublicKey.keyPartOne}\nKey part two:\n{PublicKey.keyPartTwo}");
            return;
        }
    }

    public class KeyPair
    {
        public BigInteger keyPartOne;
        public BigInteger keyPartTwo;
        public const string PUBLIC = "public";
        public const string PRIVATE = "private";



        public KeyPair(Region region)
        {
            keyPartOne = BigInteger.Parse(region.FindDirectValue("keyPartOne").value);
            keyPartTwo = BigInteger.Parse(region.FindDirectValue("keyPartTwo").value);
            switch (region.FindDirectValue("keyPairType").value)
            {
                case PUBLIC:
                    keyPairType = PUBLIC;
                    break;

                case PRIVATE:
                    keyPairType = PRIVATE;
                    break;

                default:
                    throw new Exception("Invalid keyPairType in region");
            }
        }

        public Region Save
        {
            get
            {
                Region region = new("KeyPair", new List<Region>(), new());
                region.directValues.Add(new("keyPairType", keyPairType, false));
                region.directValues.Add(new("keyPartOne", keyPartOne.ToString(), false));
                region.directValues.Add(new("keyPartTwo", keyPartTwo.ToString(), false));
                return region;
            }
        }


        public KeyPair(string akeyPairType, BigInteger akeyPartOne, BigInteger akeyPartTwo)
        {
            keyPartOne = akeyPartOne;
            keyPartTwo = akeyPartTwo;
            keyPairType = ValidateKeyPairType(akeyPairType);
        }
        private string keyPairType;
        public string KeyPairType
        {
            get { return keyPairType; }
            set { keyPairType = ValidateKeyPairType(value); }
        }

        public BigInteger CryptUsingKeypair(BigInteger number)
        {
            return BigInteger.ModPow(number, keyPartOne, keyPartTwo);
        }

        private static string ValidateKeyPairType(string testType)
        {
            //Will return default value if invalid. Will return input value if valid
            switch (testType.ToLower())
            {
                case PRIVATE:
                    return PRIVATE;
                case PUBLIC:
                    return PUBLIC;
                default:
                    return PRIVATE;
            }
        }
    }
}
