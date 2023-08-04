using System.Numerics;
using System.Security.Cryptography;

namespace RSAKeygenLib
{

    public class GenPrimes
    {
        private static int allGeneratedPrimes = 0;

        private static readonly int[] smallPrimes = new int[] //List of small primes for the PrimeQuickscan
        {
            2,
            3,
            5,
            7,
            11,
            13,
            17,
            19,
            23,
            29
        };
        private static bool PrimeQuickscan(BigInteger number)
        {
            //Will check, if the number is devisable by a few small prime numbers. This will eliminate about half of the numbers tested and is way faster than the isPrime function.
            foreach (int check in smallPrimes)
            {
                if (number % check == 0 && number != check)
                    return false; //Is definetly not a prime
            }
            return true; //Could still be no prime
        }
        public static bool IsPrime(BigInteger number, int maxIterations)
        {
            //Rabin miller algorithm for evaluating primes
            if (number == 2)
            {
                return true;
            }
            if (number.IsEven)
            {
                Console.WriteLine("Number is even, so it can't be prime");
                return false;
            }
            if (!PrimeQuickscan(number))
                return false;
            BigInteger s = number - 1;
            BigInteger t = 0;
            BigInteger v;
            while (s % 2 == 0 && s != 0)
            {
                s /= 2;
                t += 1;
            }
            for (long iteration = 0; iteration < maxIterations; iteration++) // Would be cool if it could run in paralell
            {
                BigInteger a = RandomBigInteger.GetRandom(2, number - 1);
                // generate a random number to check between 2 and number -1

                v = BigInteger.ModPow(a, s, number);
                if (v != 1)
                {
                    long i = 0;
                    while (v != number - 1)
                    {
                        if (i == t - 1)
                        {
                            return false; // if we're in this part of the code, we're 100% shure, that this is not a prime
                        }
                        else
                        {
                            i += 1;
                            v = BigInteger.ModPow(v, 2, number);
                        }
                    }
                }
            }
            return true; // could still be, that this isn't a prime

        }

        private static readonly List<BigInteger> generatedLargePrimes = new ();
        public static List<BigInteger> GenLargePrimes(int numOfPrimes, int threads, int bytelnght = 256, int iterations = 64)
        {
            //MULTITHREADING
            generatedLargePrimes.Clear();
            allGeneratedPrimes = 0;
            Task[] tasks1 = new Task[threads];
            
            for (int i = 0; i < threads; i++)
            {
                tasks1[i] = Task.Run(() => FindLargePrimes(bytelnght, numOfPrimes, "toList", iterations)); // this will ensure, that all primes will be generated
            }

            Task.WaitAny(tasks1);
            
            return generatedLargePrimes;
        }

        private static readonly object lockAllGeneratedPrimes = new();
        private static void FindLargePrimes(int byteLenght, int amtOfLargePrimes, string outputPath, int iterations)
        {
            //Console.WriteLine($"Started thread with byteLenght: {byteLenght}, amtOfLargePrimes: {amtOfLargePrimes}, outputPath: {outputPath}");
            BigInteger randomLargeNum;
            try
            
            {
                while (allGeneratedPrimes < amtOfLargePrimes)
                {

                    do
                    {
                        randomLargeNum = GetRandomByteLenghtBigIntiger(byteLenght);
                        if (randomLargeNum.IsEven)
                            randomLargeNum++; // even nubers will not be tested, because they're (almost) always no primes (exept 2)
                        if (allGeneratedPrimes >= amtOfLargePrimes)
                            return;

                    } while (!IsPrime(randomLargeNum, iterations));
                    lock (lockAllGeneratedPrimes)
                    {
                        if (allGeneratedPrimes >= amtOfLargePrimes)
                            return;
                        allGeneratedPrimes++;
                        if ("toList" == outputPath)
                            generatedLargePrimes.Add(randomLargeNum);
                        else
                        {
                            File.AppendAllText(outputPath, Convert.ToString(randomLargeNum) + "\n\n");
                        }
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error happened!?");
                Console.WriteLine(ex);
                Console.ReadKey();
            }
        }









        private static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();
        internal static BigInteger GetRandomLenghtBigIntiger(int lenght)
        {
            byte[] data = new byte[lenght];
            List<int> dataInt = new();
            rng.GetBytes(data);
            foreach (byte Byte in data)
                dataInt.Add(Byte);
            int[] randomLenght = new int[lenght];

            for (int i = 0; i < lenght; i++)
            {
                if (i == 0)
                    randomLenght[i] = (dataInt[i] % 9) + 1;
                else
                    randomLenght[i] = dataInt[i] % 10;


            }

            return BigInteger.Parse(ConvertArrayToString.ConvertIntArrayToString(randomLenght)); //Perfectly clean lol
        }
        public static BigInteger GetRandomByteLenghtBigIntiger(int byteLenght)
        {
            byte[] resultBytes = new byte[byteLenght];
            rng.GetBytes(resultBytes);
            BigInteger result = new(resultBytes);
            if (result.Sign == -1)
                result *= -1;
            return result;
        }
    }
    internal class ConvertArrayToString
    {
        internal static string ConvertIntArrayToString(int[] array)
        {
            string resolution = "";

            foreach (int item in array)
            {
                resolution += Convert.ToString(item);
            }
            return resolution;
        }

    }

    internal class RandomBigInteger
    {
        internal static BigInteger GetRandom(BigInteger min, BigInteger max)
        {
            max++;
            int lenght = (Convert.ToString(max) ?? throw new Exception("Convert.ToString(max) returned null.")).Length - (Convert.ToString(min) ?? throw new Exception("Convert.ToString(min) returned null.")).Length + 10;
            BigInteger result = RSAKeygenLib.GenPrimes.GetRandomLenghtBigIntiger(lenght);
            if (min == max) // Devide by 0 error else
                return min;

            return (result % (max - min)) + min;

        }


    }

}