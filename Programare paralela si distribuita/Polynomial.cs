#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MPI;
using OpenCLTemplate;
using Environment = MPI.Environment;

#endregion

namespace Karatsuba
{
    // A sealed class can not be inherited from.
    // The equivalent in Java is the "final" keyword.
    [Serializable] // Note the "Serializable" attribute needed to mark this class for MPI communication.
    public sealed class Polynomial
    {
        // Our random number generator.
        public static readonly Random R = new Random();

        // The degree of the polynomial.
        public readonly int Degree;
        // The array of coefficients. Its length should always be degree + 1.
        // For example a polynomial with degree 0 has a single coefficient.
        public readonly int[] Coefficients;

        public Polynomial(params int[] coefficients)
        {
            if (coefficients == null || coefficients.Length == 0)
                throw new InvalidOperationException("Invalid coefficients specification!");

            Degree = coefficients.Length - 1;
            Coefficients = coefficients;
        }

        // Next we override the 3 basic methods that all C# objects share.
        public override string ToString()
        {
            // String operations generate a lot of garbage since they are immutable.
            // To avoid this we use a string builder. This is the recommended approach.
            var sb = new StringBuilder();

            for (var i = 0; i <= Degree; ++i)
                sb.AppendFormat("{0}{1}*X^{2}", Coefficients[i] >= 0 ? "+" : "", Coefficients[i], i);

            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            var polynomial = obj as Polynomial;

            if (Degree != polynomial?.Degree)
                return false;

            for (var i = 0; i <= Degree; ++i)
                if (Coefficients[i] != polynomial.Coefficients[i])
                    return false;

            return true;
        }

        // Auto implemented by ReSharper.
        public override int GetHashCode()
        {
            unchecked
            {
                return (Degree * 397) ^ Coefficients.GetHashCode();
            }
        }

        // Generates a random polynomial of a given degree.
        public static Polynomial RandomPolynomial(int degree)
        {
            if (degree < 0)
                throw new InvalidOperationException("Invalid degree specification!");

            var coefficients = new int[degree + 1];

            for (var i = 0; i <= degree; ++i)
                coefficients[i] = R.Next(1, 10);

            return new Polynomial(coefficients);
        }

        // Shifts a polynomial to the left by a given ofset.
        // Equivalent to multiplying with "X^offset".
        // For example, X^2 shifted by offset 3 becomes X^5.
        // "^" denotes the power function.
        public static Polynomial Shift(Polynomial p, int offset)
        {
            if (offset < 0)
                throw new InvalidOperationException("Invalid offset specification!");

            var coefficients = new int[p.Degree + 1 + offset];
            Array.Copy(p.Coefficients, 0, coefficients, offset, p.Degree + 1);
            return new Polynomial(coefficients);
        }

        // Overloaded the shift ("<<") operator.
        // Makes working with objects more elegant and concise.
        // Although "behind the scenes" it is still a function of two arguments.
        // Notice that all it does is call the "Shift" static method.
        // For example "p << 3" is equivalent to Shift(p, 3).
        public static Polynomial operator <<(Polynomial p, int offset)
        {
            return Shift(p, offset);
        }

        // Simple addition. No need for parallelization.
        public static Polynomial Add(Polynomial a, Polynomial b)
        {
            var min = Math.Min(a.Degree, b.Degree);
            var max = Math.Max(a.Degree, b.Degree);

            var coefficients = new int[max + 1];

            for (var i = 0; i <= min; ++i)
                coefficients[i] = a.Coefficients[i] + b.Coefficients[i];

            for (var i = min + 1; i <= max; ++i)
                if (i <= a.Degree)
                    coefficients[i] = a.Coefficients[i];
                else
                    coefficients[i] = b.Coefficients[i];

            return new Polynomial(coefficients);
        }

        public static Polynomial operator +(Polynomial a, Polynomial b)
        {
            return Add(a, b);
        }

        // Simple subtraction. No need for parallelization.
        public static Polynomial Subtract(Polynomial a, Polynomial b)
        {
            var min = Math.Min(a.Degree, b.Degree);
            var max = Math.Max(a.Degree, b.Degree);

            var coefficients = new int[max + 1];

            for (var i = 0; i <= min; ++i)
                coefficients[i] = a.Coefficients[i] - b.Coefficients[i];

            for (var i = min + 1; i <= max; ++i)
                if (i <= a.Degree)
                    coefficients[i] = a.Coefficients[i];
                else
                    coefficients[i] = -b.Coefficients[i];

            var degree = coefficients.Length - 1;
            while (coefficients[degree] == 0 && degree > 0)
                degree--;

            var clean = new int[degree + 1];
            Array.Copy(coefficients, 0, clean, 0, degree + 1);
            return new Polynomial(clean);
        }

        public static Polynomial operator -(Polynomial a, Polynomial b)
        {
            return Subtract(a, b);
        }

        // Serial multiplication.
        public static Polynomial Multiply(Polynomial a, Polynomial b)
        {
            var coefficients = new int[a.Degree + b.Degree + 1];

            for (var i = 0; i <= a.Degree; ++i)
            for (var j = 0; j <= b.Degree; ++j)
                coefficients[i + j] += a.Coefficients[i] * b.Coefficients[j];

            return new Polynomial(coefficients);
        }

        public static Polynomial operator *(Polynomial a, Polynomial b)
        {
            return Multiply(a, b);
        }

        // Parallel multiplication.
        public static Polynomial MultiplyRegularParallel(Polynomial a, Polynomial b)
        {
            var coefficients = new int[a.Degree + b.Degree + 1];

            Parallel.For(0, a.Degree + 1, i =>
            {
                for (var j = 0; j <= b.Degree; ++j)
                    Interlocked.Add(ref coefficients[i + j], a.Coefficients[i] * b.Coefficients[j]);
                // Cheapest method for atomic addition of two integers.
            });

            return new Polynomial(coefficients);
        }

        // Parallel Karatsuba multiplication for two polynomials.
        // https://en.wikipedia.org/wiki/Karatsuba_algorithm
        // https://pythonandr.com/2015/10/13/karatsuba-multiplication-algorithm-python-code/
        public static Polynomial MultiplyKaratsubaRecursive(Polynomial x, Polynomial y)
        {
            const int threshold = 100;

            // If the degree of at least one polynomial is below a given threshold, perform regular serial multiplication.
            if (x.Degree <= threshold || y.Degree <= threshold)
                return x * y;

            // For polynomials, the split point is half the maximum degree of the two.
            var length = Math.Max(x.Degree + 1, y.Degree + 1);
            length = length / 2;

            // Split the polynomials into high and low parts respectively.

            var low1 = new int[length];
            Array.Copy(x.Coefficients, 0, low1, 0, low1.Length);

            var high1 = new int[x.Degree + 1 - length];
            Array.Copy(x.Coefficients, length, high1, 0, high1.Length);

            var low2 = new int[length];
            Array.Copy(y.Coefficients, 0, low2, 0, low2.Length);

            var high2 = new int[y.Degree + 1 - length];
            Array.Copy(y.Coefficients, length, high2, 0, high2.Length);

            var h1 = new Polynomial(high1);
            var l1 = new Polynomial(low1);
            var h2 = new Polynomial(high2);
            var l2 = new Polynomial(low2);

            // Apply divide and conquer recursion.
            var t0 = Task<Polynomial>.Factory.StartNew(() => MultiplyKaratsubaRecursive(l1, l2));
            var t1 = Task<Polynomial>.Factory.StartNew(() => MultiplyKaratsubaRecursive(l1 + h1, l2 + h2));
            var t2 = Task<Polynomial>.Factory.StartNew(() => MultiplyKaratsubaRecursive(h1, h2));

            Task.WaitAll(t0, t1, t2);

            var z0 = t0.Result;
            var z1 = t1.Result;
            var z2 = t2.Result;

            // Compute final result.
            return (z2 << (2 * length)) + ((z1 - z2 - z0) << length) + z0;
        }

        public static Polynomial MultiplyKaratsubaIterative(Polynomial x, Polynomial y)
        {
            if (x.Degree != y.Degree)
                throw new InvalidOperationException("Only works for polynomials of same degree!");

            var d = Math.Min(x.Degree, y.Degree);
            var n = d + 1;

            var di = new int[n];
            var dpq = new int[n, n];

            for (var i = 0; i <= n - 1; ++i)
                di[i] = x.Coefficients[i] * y.Coefficients[i];

            for (var i = 0; i <= 2 * n - 3; ++i)
            for (var p = 0; p <= i; ++p)
            {
                var q = i - p;
                if (p < n && q < n && q > p)
                    dpq[p, q] = (x.Coefficients[p] + x.Coefficients[q]) * (y.Coefficients[p] + y.Coefficients[q]);
            }

            var result = new int[2 * n - 1];

            result[0] = di[0];
            result[2 * n - 2] = di[n - 1];
            for (var i = 1; i <= 2 * n - 3; ++i)
            {
                for (var p = 0; p <= i; ++p)
                {
                    var q = i - p;
                    if (p < n && q < n && q > p)
                        result[i] += dpq[p, q] - (di[p] + di[q]);
                }

                if (i % 2 == 0)
                    result[i] += di[i / 2];
            }

            return new Polynomial(result);
        }

        private const string KernelCodeDi =
            @"__kernel void StepDi(__global int * n, __global int * x, __global int * y, __global int * di)
{
    int i = get_global_id(0);
    di[i] = x[i] * y[i];
}";

        private static int[] ExecuteStepDi(Polynomial x, Polynomial y)
        {
            if (x.Degree != y.Degree)
                throw new InvalidOperationException("Only works for polynomials of same degree!");

            var d = Math.Min(x.Degree, y.Degree);
            var n = d + 1;

            var di = new int[n];

            CLCalc.InitCL();
            CLCalc.Program.Compile(new[] {KernelCodeDi});
            var kernel = new CLCalc.Program.Kernel("StepDi");

            var nCl = new CLCalc.Program.Variable(new[] {n});
            var xCl = new CLCalc.Program.Variable(x.Coefficients);
            var yCl = new CLCalc.Program.Variable(y.Coefficients);
            var diCl = new CLCalc.Program.Variable(di);
            CLCalc.Program.MemoryObject[] args = {nCl, xCl, yCl, diCl};

            var workers = new[] {n};
            kernel.Execute(args, workers);
            diCl.ReadFromDeviceTo(di);
            return di;
        }

        private const string KernelCodeStepDpq =
            @"__kernel void StepDpq(__global int * n, __global int * x, __global int * y, __global int * dpq)
{
    int i = get_global_id(0);
    for (int p = 0; p <= i; ++p)
        {
            int q = i - p;
            if ((p < n[0]) && (q < n[0]) && (q > p))
                dpq[p * n[0] + q] = (x[p] + x[q])*(y[p] + y[q]);
        }
}";

        private static int[] ExecuteStepDpq(Polynomial x, Polynomial y)
        {
            if (x.Degree != y.Degree)
                throw new InvalidOperationException("Only works for polynomials of same degree!");

            var d = Math.Min(x.Degree, y.Degree);
            var n = d + 1;

            var dpq = new int[n * n];

            CLCalc.InitCL();
            CLCalc.Program.Compile(new[] {KernelCodeStepDpq});
            var kernel = new CLCalc.Program.Kernel("StepDpq");

            var nCl = new CLCalc.Program.Variable(new[] {n});
            var xCl = new CLCalc.Program.Variable(x.Coefficients);
            var yCl = new CLCalc.Program.Variable(y.Coefficients);
            var dpqCl = new CLCalc.Program.Variable(dpq);
            CLCalc.Program.MemoryObject[] args = {nCl, xCl, yCl, dpqCl};

            var workers = new[] {2 * n - 2};
            kernel.Execute(args, workers);
            dpqCl.ReadFromDeviceTo(dpq);
            return dpq;
        }

        private const string KernelCodeStepKaratsuba =
            @"__kernel void StepKaratsuba(__global int * n, __global int * x, __global int * y, __global int * di, __global int * dpq, __global int * z)
{
    int i = get_global_id(0)+1;
    for (int p = 0; p <= i; ++p)
        {
            int q = i - p;
            if ((p < n[0]) && (q < n[0]) && (q > p))
                        z[i] += dpq[p * n[0] + q] - (di[p] + di[q]);
        }
    if (i%2 == 0)
        z[i] += di[i/2];
}";

        public static Polynomial MultiplyKaratsubaOpenCl(Polynomial x, Polynomial y)
        {
            if (x.Degree != y.Degree)
                throw new InvalidOperationException("Only works for polynomials of same degree!");

            var d = Math.Min(x.Degree, y.Degree);
            var n = d + 1;

            var di = ExecuteStepDi(x, y);
            var dpq = ExecuteStepDpq(x, y);
            var z = new int[2 * n - 1];

            z[0] = di[0];
            z[2 * n - 2] = di[n - 1];

            CLCalc.InitCL();
            CLCalc.Program.Compile(new[] {KernelCodeStepKaratsuba});
            var kernel = new CLCalc.Program.Kernel("StepKaratsuba");

            var nCl = new CLCalc.Program.Variable(new[] {n});
            var xCl = new CLCalc.Program.Variable(x.Coefficients);
            var yCl = new CLCalc.Program.Variable(y.Coefficients);
            var diCl = new CLCalc.Program.Variable(di);
            var dpqCl = new CLCalc.Program.Variable(dpq);
            var zCl = new CLCalc.Program.Variable(z);
            CLCalc.Program.MemoryObject[] args = {nCl, xCl, yCl, diCl, dpqCl, zCl};

            var workers = new[] {2 * n - 3};
            kernel.Execute(args, workers);
            zCl.ReadFromDeviceTo(z);
            return new Polynomial(z);
        }

        // Splits workload as evenly as possible given a numerator and a denominator.
        // Used to determine the indexes on which MPI slave processes execute.
        // For example, we wish to split the range 1..10 among 3 processes => 1..3, 4..6, 7..10.
        private static IEnumerable<int> DivideEvenly(int numerator, int denominator)
        {
            if (numerator <= 0 || denominator <= 0)
                throw new InvalidOperationException("Invalid division!");

            int mod;
            var div = Math.DivRem(numerator, denominator, out mod);

            for (var i = 0; i < denominator; i++)
                yield return i < mod ? div + 1 : div;
        }

        // Computes the workload start and workload end indexes on which MPI slave processes will execute.
        // It is necessary to do this because the number of slave processes in most cases is not equal to the number of elements in a range.
        // To be more clear, say we want to parallelize a for with "i" from 1 to 100, but we have only 13 slave processes.
        // Each will get a corresponding range on which it will iterate internally (the "workloadStart" and "workloadEnd" output parameters).
        private static void MpiFor(int forStart, int forEnd, int forIncrement, Communicator communicator,
            out int workloadStart,
            out int workloadEnd)
        {
            var forIterations = (forEnd - forStart) / forIncrement + 1;

            var workers = communicator.Size - 1;
            var workload = DivideEvenly(forIterations, workers).ToArray();

            workloadStart = 0;
            workloadEnd = workload[0] - 1;

            for (var i = 1; i < communicator.Rank; ++i)
            {
                workloadStart += workload[i - 1];
                workloadEnd = workloadStart + workload[i] - 1;
            }
        }

        // A slave process can respond with an index and the corresponding value at that index.
        // In this way, the master process knows which elements to populate in the "di" array.
        [Serializable]
        internal struct VectorElement
        {
            public int Index;
            public int Value;
        }

        // A slave process can respond with a row, column and value at that coordinate in the "dpq" matrix.
        [Serializable]
        internal struct MatrixElement
        {
            public int Row;
            public int Column;
            public int Value;
        }

        // Install-Package MPI.NET in the "Package Manager Console" window.
        // http://www.osl.iu.edu/research/mpi.net/documentation/tutorial/hello.php
        // Command Prompt: mpiexec -n 8 Karatsuba.exe
        // 8 is the number of processes (must be greater than 1).
        public static void MultiplyKaratsubaMpi(string[] args)
            // Need to pass the arguments with which "Main" was started.
        {
            using (new Environment(ref args))
            {
                var comm = Communicator.world;

                Polynomial x = null, y = null;
                int n;

                if (comm.Rank == 0) // By convention the channel with index 0 is the master process.
                {
                    const int degree = 100;
                    x = RandomPolynomial(degree);
                    y = RandomPolynomial(degree);

                    n = degree + 1;

                    var di = new int[n];
                    var dpq = new int[n, n];

                    // Broadcast the polynomials to all slave processes so they know what they are working on.
                    comm.Broadcast(ref x, 0);
                    comm.Broadcast(ref y, 0);

                    // Receive the computed element from the "di" vector.
                    for (var i = 1; i <= n; ++i)
                    {
                        var result = comm.Receive<VectorElement>(Communicator.anySource, 1);
                        di[result.Index] = result.Value;
                    }

                    // Receive the computed element from the "dpq" matrix.
                    for (var i = 1; i <= n * (n - 1) / 2; ++i)
                    {
                        var result = comm.Receive<MatrixElement>(Communicator.anySource, 2);
                        dpq[result.Row, result.Column] = result.Value;
                    }

                    // TO DO FOR GRADE 10!!! :)
                    var z = new int[2 * n - 1];
                    z[0] = di[0];
                    z[2 * n - 2] = di[n - 1];
                    for (var i = 1; i <= 2 * n - 3; ++i)
                    {
                        for (var p = 0; p <= i; ++p)
                        {
                            var q = i - p;
                            if (p < n && q < n && q > p)
                                z[i] += dpq[p, q] - (di[p] + di[q]);
                        }

                        if (i % 2 == 0)
                            z[i] += di[i / 2];
                    }

                    var output = new Polynomial(z);
                    // This is just for testing purposes.
                    Console.WriteLine(Equals(x * y, output));
                    Console.WriteLine(Equals(x * y, MultiplyKaratsubaRecursive(x, y)));
                    Console.WriteLine(Equals(x * y, MultiplyKaratsubaIterative(x, y)));
                    Console.WriteLine(Equals(x * y, MultiplyKaratsubaOpenCl(x, y)));
                }
                else // Otherwise it is a slave process.
                {
                    // Receive the broadcasted polynomials.
                    comm.Broadcast(ref x, 0);
                    comm.Broadcast(ref y, 0);

                    n = Math.Min(x.Degree, y.Degree) + 1;

                    // The "for" lower and upper bounds.
                    int workloadStart, workloadEnd;

                    MpiFor(0, n - 1, 1, comm, out workloadStart, out workloadEnd);
                    for (var i = workloadStart; i <= workloadEnd; ++i)
                        comm.Send(new VectorElement {Index = i, Value = x.Coefficients[i] * y.Coefficients[i]}, 0, 1);

                    MpiFor(0, 2 * n - 3, 1, comm, out workloadStart, out workloadEnd);
                    for (var i = workloadStart; i <= workloadEnd; ++i)
                    for (var p = 0; p <= i; ++p)
                    {
                        var q = i - p;
                        if (p < n && q < n && q > p)
                            comm.Send(
                                new MatrixElement
                                {
                                    Row = p,
                                    Column = q,
                                    Value =
                                        (x.Coefficients[p] + x.Coefficients[q]) *
                                        (y.Coefficients[p] + y.Coefficients[q])
                                }, 0, 2);
                    }
                }
            }
        }
    }
}