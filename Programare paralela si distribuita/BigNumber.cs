#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace PDPLab6
{
    public static class P1
    {
        // Define some out of range value as a constant.
        private const int NaN = -1;
        private const int N = 1000;

        private static readonly Random R = new Random();
        // The 2D array of partial sums.
        // PartialSums[i, j] = the sum of the elements from position i to position j, i<=j.
        private static readonly int[,] PartialSums = new int[N, N];

        // Solves the problem.
        public static void KSum()
        {
            // Generate a random array of values.
            var x = new int[N];
            for (var i = 0; i < N; ++i)
                x[i] = R.Next(1, 10);

            Console.WriteLine(string.Join(" ", x));

            // Initialize partial sums on the diagonal with the original array values (i=j).
            for (var i = 0; i < N; ++i)
            {
                for (var j = 0; j < N; ++j)
                    PartialSums[i, j] = NaN; // The out-of-bounds marker variable NaN.
                PartialSums[i, i] = x[i];
            }

            for (var k = 0; k < N; ++k)
                Console.Write("{0} ", KSum(0, k));
            Console.WriteLine();
        }

        // Computes the sum of the elements from position start to position end, where start<=end.
        private static int KSum(int start, int end)
        {
            if (start > end) // Exception...
                throw new InvalidOperationException("Start must be less or equal to end!");
            if (start == end) // We already know it since it is a single element (i=j => diagonal).
                return PartialSums[start, end];
            if (PartialSums[start, end] != NaN) // We have already computed it.
                return PartialSums[start, end];

            var middle = (start + end)/2;

            var low = PartialSums[start, middle];
            Task<int> lowTask = null;
            if (low == NaN) // If we don't know it, we compute it.
                lowTask = Task.Factory.StartNew(() => KSum(start, middle));

            var high = PartialSums[middle + 1, end];
            Task<int> highTask = null;
            if (high == NaN) // If we don't know it we compute it.
                highTask = Task.Factory.StartNew(() => KSum(middle + 1, end));

            if (lowTask != null)
            {
                low = lowTask.Result;
                PartialSums[start, middle] = low; // Update partial sums.
            }

            if (highTask != null)
            {
                high = highTask.Result;
                PartialSums[middle + 1, end] = high; // Update partial sums.
            }

            var result = low + high;
            PartialSums[start, end] = result; // Update partial sums.
            return result;
        }
    }

    // Custom queue implementation with a marker for "Done".
    public sealed class BigNumberQueue : Queue<Tuple<int, int>>
    {
        public volatile bool Done; // https://msdn.microsoft.com/ro-ro/library/x13ttww7.aspx
    }

    public sealed class BigNumber
    {
        private static readonly Random R = new Random();

        public readonly int[] Digits;

        public BigNumber(params int[] digits)
        {
            Digits = digits.Reverse().ToArray();
        }

        public override string ToString()
        {
            return string.Join(string.Empty, Digits.Reverse().ToArray());
        }

        public override bool Equals(object obj)
        {
            var bigNumber = obj as BigNumber;

            if (Digits.Length != bigNumber?.Digits.Length)
                return false;

            for (var i = 0; i <= Digits.Length; ++i)
                if (Digits[i] != bigNumber.Digits[i])
                    return false;

            return true;
        }

        public override int GetHashCode()
        {
            return Digits.GetHashCode();
        }

        public static BigNumber RandomBigNumber()
        {
            const int sizeMin = 100;
            const int sizeMax = 1000;

            var size = R.Next(sizeMin, sizeMax);
            var digits = new int[size];
            for (var i = 0; i < size; ++i)
                digits[i] = R.Next(1, 10);

            return new BigNumber(digits);
        }

        private static BigNumberQueue AddParallel(BigNumberQueue a, BigNumber b)
        {
            var result = new BigNumberQueue();

            Task.Factory.StartNew(() =>
            {
                var transport = 0;

                int i;
                for (i = 0; i < b.Digits.Length; ++i)
                {
                    while (!a.Done && (a.Count == 0)) // Waiting for a digit.
                    {
                    }

                    if (a.Done && (a.Count == 0)) // Finished all digits from "a", just adding "b".
                        lock (result)
                        {
                            result.Enqueue(new Tuple<int, int>(i, (transport + b.Digits[i])%10));
                            transport = (transport + b.Digits[i])/10;
                        }

                    if (a.Count > 0) // We have a digit from a previous computation.
                        lock (a)
                        {
                            var digit = a.Dequeue();
                            if (digit.Item1 != i)
                                throw new InvalidOperationException("Out of order!");

                            lock (result)
                            {
                                result.Enqueue(new Tuple<int, int>(i, (transport + digit.Item2 + b.Digits[i])%10));
                                transport = (transport + digit.Item2 + b.Digits[i])/10;
                            }
                        }
                }

                while (!(a.Done && (a.Count == 0))) // Finished all digits from "b", just adding "a".
                    if (a.Count > 0)
                        lock (a)
                        {
                            var digit = a.Dequeue();

                            lock (result)
                            {
                                result.Enqueue(new Tuple<int, int>(i, (transport + digit.Item2)%10));
                                transport = (transport + digit.Item2)/10;
                                i++;
                            }
                        }

                if (transport > 0) // We can have at most one extra digit in the transport variable.
                    lock (result)
                    {
                        result.Enqueue(new Tuple<int, int>(i, transport));
                    }

                result.Done = true; // Signal that we have finished the computations.
            });

            return result;
        }

        public static BigNumber AddParallel(params BigNumber[] numbers)
        {
            if (numbers.Length <= 1)
                throw new InvalidOperationException("Array too small!");

            var first = numbers[0];
            var firstQueue = new BigNumberQueue();
            for (var i = 0; i < first.Digits.Length; ++i)
                firstQueue.Enqueue(new Tuple<int, int>(i, first.Digits[i]));
            firstQueue.Done = true;

            var queues = new BigNumberQueue[numbers.Length];
            queues[0] = firstQueue;
            for (var i = 1; i < numbers.Length; ++i)
                queues[i] = AddParallel(queues[i - 1], numbers[i]);

            while (!queues[numbers.Length - 1].Done)
            {
            }

            var result = new List<int>();
            while (queues[numbers.Length - 1].Count > 0)
                result.Add(queues[numbers.Length - 1].Dequeue().Item2);
            result.Reverse();
            return new BigNumber(result.ToArray());
        }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            const int size = 100;
            var numbers = new BigNumber[size];
            for (var i = 0; i < size; ++i)
                numbers[i] = BigNumber.RandomBigNumber();
            Console.WriteLine(BigNumber.AddParallel(numbers));
        }
    }
}