using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace IBMPonderThisJan2022
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: IBMPonderThisJan2022.exe <n> <d> <true or false, to start over>");
                Console.WriteLine("Example: IBMPonderThisJan2022.exe 7 5 true");
                Console.WriteLine("Example: IBMPonderThisJan2022.exe 8 6 false");
                return;
            }

            int n = Int32.Parse(args[0]);
            int d = Int32.Parse(args[1]);

            if (args[2].Equals("true"))
            {
                if (File.Exists("checkpoint.txt")) File.Delete("checkpoint.txt");
            }

            int[] digits = new int[n];
            int[] startDigits = null;
            long maxCost = Int64.MinValue;
            int[] maxDigits = null;
            long minCost = Int64.MaxValue;
            int[] minDigits = null;

            ReadCheckpoint("checkpoint.txt",
                           ref startDigits,
                           ref d,
                           ref maxCost,
                           ref maxDigits,
                           ref minCost,
                           ref minDigits);

            bool start = false;
            if (startDigits == null)
            {
                start = true;
                startDigits = new int[n];
            }

            Console.WriteLine("Processing starting at {0}...", DateTime.Now.ToString());
            long ticksBefore = DateTime.Now.Ticks;
            int combinationNumber = 0;
            ProcessAll(startDigits,
                       ref start,
                       digits,
                       d,
                       0,
                       new Hashtable(),
                       ref maxCost,
                       ref maxDigits,
                       ref minCost,
                       ref minDigits,
                       ref combinationNumber,
                       new Hashtable());
            long ticksAfter = DateTime.Now.Ticks;
            Console.WriteLine("Finished at {0}...", DateTime.Now.ToString());
            Console.WriteLine("Execution Time: {0}", FormatTime(ticksAfter - ticksBefore));
        }

        public static string FormatTime(long timeInNS)
        {
            string ret = "";

            timeInNS /= 10000000;

            if (timeInNS < 60)
            {
                ret = timeInNS.ToString() + "secs";
            }
            else if (timeInNS < 3600)
            {
                ret = (timeInNS / 60).ToString() + "mins " + (timeInNS % 60).ToString() + "secs";
            }
            else
            {
                ret = (timeInNS / 3600).ToString() + "hrs " + ((timeInNS % 3600) / 60).ToString() + "mins " + (timeInNS % 60).ToString() + "secs";
            }

            return ret;
        }

        public static void ReadCheckpoint(string fileName,
                                          ref int[] digits,
                                          ref int d,
                                          ref long maxCost,
                                          ref int[] maxDigits,
                                          ref long minCost,
                                          ref int[] minDigits)
        {
            digits = null;
            maxDigits = null;
            minDigits = null;

            if (File.Exists(fileName))
            {
                FileInfo fi = new FileInfo(fileName);
                StreamReader sr = fi.OpenText();
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] parts = line.Split(":");
                    if (parts.Length == 2 && !String.IsNullOrEmpty(parts[1]))
                    {
                        switch (parts[0])
                        {
                            case "digits":
                                string[] dp = parts[1].Split(" ");
                                digits = new int[dp.Length];
                                for (int i = 0; i < dp.Length; i++)
                                {
                                    digits[i] = Int32.Parse(dp[i]);
                                }
                                break;
                            case "d":
                                d = Int32.Parse(parts[1]);
                                break;
                            case "maxcost":
                                maxCost = Int32.Parse(parts[1]);
                                break;
                            case "mincost":
                                minCost = Int32.Parse(parts[1]);
                                break;
                            case "maxdigits":
                                string[] md = parts[1].Split(" ");
                                maxDigits = new int[md.Length];
                                for (int i = 0; i < md.Length; i++)
                                {
                                    maxDigits[i] = Int32.Parse(md[i]);
                                }
                                break;
                            case "mindigits":
                                string[] mid = parts[1].Split(" ");
                                minDigits = new int[mid.Length];
                                for (int i = 0; i < mid.Length; i++)
                                {
                                    minDigits[i] = Int32.Parse(mid[i]);
                                }
                                break;
                        }
                    }
                }
                sr.Close();
            }
        }

        public static void WriteCheckpoint(string fileName,
                                           int[] digits,
                                           int d,
                                           long maxCost,
                                           int[] maxDigits,
                                           long minCost,
                                           int[] minDigits,
                                           int combinationNumber)
        {
            FileInfo fi = new FileInfo(fileName);
            StreamWriter sw = fi.CreateText();

            string line = "";

            line = "date:" + DateTime.Now.ToString();
            sw.WriteLine(line);

            line = "digits:";
            if (digits != null)
            {
                for (int i = 0; i < digits.Length; i++) line += digits[i].ToString() + " ";
                line = line.Trim();
            }
            sw.WriteLine(line);

            line = "d:" + d.ToString();
            sw.WriteLine(line);

            line = "maxcost:" + maxCost.ToString();
            sw.WriteLine(line);

            line = "maxdigits:";
            if (maxDigits != null)
            {
                for (int i = 0; i < maxDigits.Length; i++) line += maxDigits[i].ToString() + " ";
                line = line.Trim();
            }
            sw.WriteLine(line);

            line = "mincost:" + minCost.ToString();
            sw.WriteLine(line);

            line = "mindigits:";
            if (minDigits != null)
            {
                for (int i = 0; i < minDigits.Length; i++) line += minDigits[i].ToString() + " ";
                line = line.Trim();
            }
            sw.WriteLine(line);

            line = "combinationnumber:" + combinationNumber.ToString();
            sw.WriteLine(line);

            sw.Close();
        }

        public static void ProcessAll(int[] startDigits,
                                      ref bool start,
                                      int[] digits,
                                      int d,
                                      int currentIndex,
                                      Hashtable usedDigits,
                                      ref long maxCost,
                                      ref int[] maxDigits,
                                      ref long minCost,
                                      ref int[] minDigits,
                                      ref int combinationNumber,
                                      Hashtable primeCache)
        {
            if (start && currentIndex >= digits.Length)
            {
                long totalCost = 0;
                int numberOfPrimes = 0;
                Hashtable primeNumbersUsed = new Hashtable();

                for (int i = 0; i < digits.Length; i++)
                {
                    if (digits[i] != 0)
                    {
                        Hashtable tempUsedDigits = new Hashtable();
                        tempUsedDigits.Add(digits[i], true);
                        Process(digits,
                                i,
                                0,
                                digits[i],
                                tempUsedDigits,
                                primeNumbersUsed,
                                d,
                                ref totalCost,
                                ref numberOfPrimes,
                                primeCache);
                    }
                }

                combinationNumber++;
                
                if (totalCost >= maxCost)
                {
                    if (maxDigits == null) maxDigits = new int[digits.Length];
                    for (int j = 0; j < digits.Length; j++) maxDigits[j] = digits[j];

                    maxCost = totalCost;

                    WriteCheckpoint("checkpoint.txt",
                                    digits,
                                    d,
                                    maxCost,
                                    maxDigits,
                                    minCost,
                                    minDigits,
                                    combinationNumber);
                }
                if (totalCost <= minCost)
                {
                    if (minDigits == null) minDigits = new int[digits.Length];
                    for (int j = 0; j < digits.Length; j++) minDigits[j] = digits[j];

                    minCost = totalCost;

                    WriteCheckpoint("checkpoint.txt",
                                    digits,
                                    d,
                                    maxCost,
                                    maxDigits,
                                    minCost,
                                    minDigits,
                                    combinationNumber);
                }

                /*
                Console.WriteLine();
                //Console.Write("#{0} out of 604800: Numbers:", combinationNumber);
                foreach (int digit in digits) Console.Write(" {0}", digit);
                Console.Write(" => cost = {0}. (Max,Min) = ({1},{2})", totalCost, maxCost, minCost);
                Console.WriteLine();
                //Console.ReadLine();
                */

                return;
            }
            else if (!start && currentIndex >= digits.Length)
            {
                start = true;
                for (int i = 0; i < digits.Length; i++)
                {
                    if (digits[i] != startDigits[i])
                    {
                        start = false;
                        break;
                    }
                }

                if (start)
                {
                    Console.WriteLine("Starting At #{0} combination out of 604800:", combinationNumber);
                    for (int i = 0; i < digits.Length; i++) Console.Write("{0} ", digits[i]);
                    Console.ReadLine();
                }

                combinationNumber++;
                return;
            }

            for (int i = 0; i < 10; i++)
            {
                if (!usedDigits.ContainsKey(i))
                {
                    usedDigits.Add(i, true);
                    digits[currentIndex] = i;
                    ProcessAll(startDigits,
                               ref start,
                               digits,
                               d,
                               currentIndex + 1,
                               usedDigits, 
                               ref maxCost,
                               ref maxDigits,
                               ref minCost,
                               ref minDigits,
                               ref combinationNumber,
                               primeCache);
                    usedDigits.Remove(i);
                }
            }
        }

        public static void Process(int[] digits,
                                   int currentIndex,
                                   long currentCost,
                                   long currentNumber,
                                   Hashtable usedDigits,
                                   Hashtable primeNumbersUsed,
                                   int d,
                                   ref long totalCost,
                                   ref int numberOfPrimes,
                                   Hashtable primeCache)
        {
            if (usedDigits.Count == d)
            {
                if (!primeNumbersUsed.ContainsKey(currentNumber) && IsPrimeMillerRabin(currentNumber, primeCache))
                {
                    numberOfPrimes++;
                    totalCost += currentCost;
                    primeNumbersUsed.Add(currentNumber, true);
                    //Console.WriteLine("#{0} Prime Number: {1}, Cost: {2} (total cost = {3})", numberOfPrimes, currentNumber, currentCost, totalCost);
                }
                return;
            }
            else if (usedDigits.Count > d) return;

            for (int i = 0; i < digits.Length; i++)
            {
                if (i == currentIndex || (digits[i] == 0 && currentNumber == 0)) continue;

                if (!usedDigits.ContainsKey(digits[i]))
                {
                    usedDigits.Add(digits[i], true);
                    int cost1 = Math.Abs(i - currentIndex);
                    int cost2 = (digits.Length + Math.Min(i, currentIndex)) - Math.Max(i, currentIndex);
                    int cost = Math.Min(cost1, cost2);
                    Process(digits,
                            i,
                            currentCost + cost,
                            10 * currentNumber + digits[i],
                            usedDigits,
                            primeNumbersUsed,
                            d,
                            ref totalCost,
                            ref numberOfPrimes,
                            primeCache);
                    usedDigits.Remove(digits[i]);
                }
            }
        }


        public static bool IsPrimeMillerRabin(BigInteger n, Hashtable primeCache)
        {
            if (primeCache.ContainsKey(n)) return true;

            //It does not work well for smaller numbers, hence this check
            int SMALL_NUMBER = 1000;

            if (n <= SMALL_NUMBER)
            {
                bool b = IsPrime(n);
                if (b && !primeCache.ContainsKey(n)) primeCache.Add(n, true);
                return b;
            }

            int MAX_WITNESS = 500;
            for (long i = 2; i <= MAX_WITNESS; i++)
            {
                if (IsPrime(i) && Witness(i, n) == 1)
                {
                    return false;
                }
            }

            if (!primeCache.ContainsKey(n)) primeCache.Add(n, true);

            return true;
        }

        public static BigInteger SqRtN(BigInteger N)
        {
            /*++
             *  Using Newton Raphson method we calculate the
             *  square root (N/g + g)/2
             */
            BigInteger rootN = N;
            int count = 0;
            int bitLength = 1;
            while (rootN / 2 != 0)
            {
                rootN /= 2;
                bitLength++;
            }
            bitLength = (bitLength + 1) / 2;
            rootN = N >> bitLength;

            BigInteger lastRoot = BigInteger.Zero;
            do
            {
                if (lastRoot > rootN)
                {
                    if (count++ > 1000)                   // Work around for the bug where it gets into an infinite loop
                    {
                        return rootN;
                    }
                }
                lastRoot = rootN;
                rootN = (BigInteger.Divide(N, rootN) + rootN) >> 1;
            }
            while (!((rootN ^ lastRoot).ToString() == "0"));
            return rootN;
        }

        public static bool IsPrime(BigInteger n)
        {
            if (n <= 1)
            {
                return false;
            }

            if (n == 2)
            {
                return true;
            }

            if (n % 2 == 0)
            {
                return false;
            }

            for (int i = 3; i <= SqRtN(n) + 1; i += 2)
            {
                if (n % i == 0)
                {
                    return false;
                }
            }
            return true;
        }

        private static int Witness(long a, BigInteger n)
        {
            BigInteger t, u;
            BigInteger prev, curr = 0;
            BigInteger i;
            BigInteger lln = n;

            u = n / 2;
            t = 1;
            while (u % 2 == 0)
            {
                u /= 2;
                t++;
            }

            prev = BigInteger.ModPow(a, u, n);
            for (i = 1; i <= t; i++)
            {
                curr = BigInteger.ModPow(prev, 2, lln);
                if ((curr == 1) && (prev != 1) && (prev != lln - 1)) return 1;
                prev = curr;
            }
            if (curr != 1) return 1;
            return 0;
        }
    }
}
