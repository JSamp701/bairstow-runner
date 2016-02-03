using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ma420_Assignments.Chapter7;

namespace BairstowRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Please enter an output file-name for data (note: .csv will be automatically appended): ");
            string outfileName = Console.ReadLine() + ".csv";
            Console.Write("How many problems to solve? ");
            using (StreamWriter writer = new StreamWriter(outfileName)){
                for(int numtosolve = Convert.ToInt32(Console.ReadLine()); numtosolve > 0; --numtosolve)
                {
                    Console.Write("Please enter name for problem: ");
                    string name = Console.ReadLine();
                    Console.Write("Please enter error percentage (example, enter 0.001 for 0.1% allowed error): ");
                    double error = getDouble();
                    Console.Write("Please enter maximum iterations: ");
                    int iterations = Convert.ToInt32(Console.ReadLine());
                    Console.Write("Please enter initial s guess: ");
                    double sGuess = getDouble();
                    Console.Write("Please enter initial r guess: ");
                    double rGuess = getDouble();

                    Console.Write("Please enter the highest power of x.  For example, if the highest power of x in the polynomial is x^5, then please enter 5. ");
                    int numSlots = Convert.ToInt32(Console.ReadLine()) + 1;

                    Console.WriteLine("We will now begin entering the values of the coefficients of the polynomial.  To do so, please enter the coefficients for the prompted power of x (where x0 is x^0, x1 is x_1, x2 is x^2, etc).");
                    double[] polynomial = new double[numSlots];
                    for (int i = 0; i < numSlots; ++i)
                    {
                        Console.Write("x^" + i + ": ");
                        polynomial[i] = getDouble();
                    }

                    Console.Write("The polynomial you entered is f(x) = ");
                    Console.Write(polynomial[0]);
                    for (int i = 1; i < numSlots; ++i)
                    {
                        Console.Write(" + " + polynomial[i] + "x^" + i);
                    }
                    Console.Write("\n");

                    Bairstow b = new Bairstow(polynomial, error, iterations, sGuess, rGuess);
                    Tuple<double, double>[] roots = b.calculateRoots();
                    List<List<Bairstow.IterationResult>> iterationData = b.getIterationResults();

                    writer.WriteLine("Problem Name: " + name);
                    writer.WriteLine();
                    writer.WriteLine();
                    writer.WriteLine("ROOTS");
                    writer.WriteLine("REAL Component, COMPLEX Component");
                    for(int i = 0; i < roots.Length; ++i)
                    {
                        writer.WriteLine(roots[i].Item1 + "," + roots[i].Item2);
                    }
                    if (iterationData[0].Count > 0)
                    {
                        writer.WriteLine();
                        writer.WriteLine();
                        writer.WriteLine("ITERATION RESULTS");
                        writer.WriteLine("S,R,DELTA-S,DELTA-R,ERROR-S,ERROR-R");
                        writer.WriteLine();
                        for (int i = 0; i < iterationData.Count(); ++i)
                        {
                            List<Bairstow.IterationResult> currentSet = iterationData[i];
                            if (currentSet.Count == 0) continue;
                            writer.WriteLine("ITERATION SET #" + i);
                            for (int j = 0; j < currentSet.Count; ++j)
                            {
                                Bairstow.IterationResult current = currentSet[j];
                                writer.WriteLine(current.iterS + "," + current.iterR + "," + current.deltaS + "," + current.deltaR + "," + current.iterSError + "," + current.iterRError);
                            }
                            writer.WriteLine();
                        }
                    }
                    writer.WriteLine("\n\n");
                }
            }

            int bob = 2;
        }
        static double getDouble()
        {
            bool good = false;
            double value = 0;
            do
            {
                string valueString = Console.ReadLine();
                try
                {
                    value = Convert.ToDouble(valueString);
                    good = true;
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Error: " + valueString + " is not a valid number. Please enter a valid number.");
                }
            } while (!good);
            return value;
        }
    }
}
