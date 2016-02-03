using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ma420_Assignments.Chapter7
{
    class Bairstow
    {
        //CONSTANT VARIABLE DEFINITIONS

        const double DEFAULT_ERROR = 0.01;
        const int DEFAULT_MAX_ITERS = 30;
        const double DEFAULT_R = -1;
        const double DEFAULT_S = -1;


        //INSTANCE VARIABLE DEFINITIONS

        List<double[]> coefficients; //stores the coefficients for every iteration

        double allowedError; //the error distance to be under

        double startR, startS;

        int maxIterNum; //the max number of iterations to perform before giving up

        //stores necessary information resulting from a single "performIteration" operation
        public struct IterationResult
        {
            public double iterR, iterS, iterRError, iterSError, deltaR, deltaS;
            public bool dataRelevant, rootFound, allRootsFound, maxIterationsExceeded;
        }

        List<List<IterationResult>> iterResults; //stores the results of the iterations
        public List<List<IterationResult>> getIterationResults() { return iterResults; }

        //returns the most recent, nonempty set of iterations (or null if none exists)
        public List<IterationResult> getLatestIterations()
        {
            List<IterationResult> results = null;
            if (iterResults.Count > 1)
            {
                if (iterResults[iterResults.Count - 1].Count > 0)
                    results = iterResults[iterResults.Count - 1];
                else
                    results = iterResults[iterResults.Count - 2];
            }
            else if (iterResults[0].Count > 0) results = iterResults[0];
            return results;
        }

        int foundRootCount; //how many roots have been found

        Tuple<double, double>[] roots; //the array storing the found roots, Item1 is the real component, Item2 is the imaginary component

        public class MAX_ITER_NUM_REACHED_EXCEPTION : Exception
        {
            int bob = 1;
        }



        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        //PUBLIC METHOD DEFINITIONS

        public static void TestModule()
        {
            //textbook example, in order from least power to highest power of x
            //1.25 -3.875 2.125 2.75 -3.5 1
            double[] testArray = { 1.25, -3.875, 2.125, 2.75, -3.5, 1 };

            IterationResult result;
            Bairstow b = new Bairstow(testArray, 0.001, 30, 0,0);
            do
            {
                result = b.performIteration();
                Console.WriteLine("R: " + result.iterR + " S: " + result.iterS + " R-ERR: " + result.iterRError + " S-ERR: " + result.iterSError + " DELTA-R: " +
                    result.deltaR + " DELTA-S: " + result.deltaS);
                if (result.rootFound) Console.WriteLine();
            } while (result.allRootsFound == false);

            Tuple<double, double>[] roots = b.calculateRoots();
            for(int i = 0; i < roots.Length; ++i)
            {
                Console.WriteLine("REAL: " + roots[i].Item1 + " COMPLEX: " + roots[i].Item2);
            }
        }

        public Bairstow(double[] polynomial) : this(polynomial, DEFAULT_ERROR) { ; }

        public Bairstow(double[] polynomial, double s, double r) : this(polynomial, DEFAULT_ERROR, s, r) { ; }

        public Bairstow(double[] polynomial, double error, double s, double r) : this (polynomial, error, DEFAULT_MAX_ITERS, s, r) { ; }

        public Bairstow(double[] polynomial, double error) : this(polynomial, error, DEFAULT_MAX_ITERS) { ; }

        public Bairstow(double[] polynomial, double error, int maxIters) : this (polynomial, error, maxIters, DEFAULT_S, DEFAULT_R) { ; }

        public Bairstow(double[] polynomial, double error, int maxIters, double s, double r)
        {
            coefficients = new List<double[]>();
            coefficients.Add(polynomial);
            allowedError = error;
            roots = new Tuple<double, double>[polynomial.Length - 1];
            iterResults = new List<List<IterationResult>>();
            iterResults.Add(new List<IterationResult>());
            maxIterNum = maxIters;
            foundRootCount = 0;
            startR = r;
            startS = s;
        }

        // Performs a single iteration over the coefficients.  Returns the results of the iteration as well as stores them in the internal list
        // Will return a placeholder iteration result if all the roots have been found / no more roots need to be found via iterations
        public IterationResult performIteration()
        {
            IterationResult res = new IterationResult();
            if(roots.Length - foundRootCount > 2)
            {
                //if there are more than two roots left to find, perform a generic iteration step method
                res = performIterationStep();
            } 
            else if(roots.Length - foundRootCount == 2)
            {
                //if there are exactly 2 roots left to find, perform quadratic solution method
                res = performQuadraticSolution();
            }
            else if(roots.Length - foundRootCount == 1)
            {
                //if only one root left, calculate it using the solveSingle method(and store it) and set dataRelevant to false
                roots[foundRootCount++] = solveSingle(coefficients[coefficients.Count-1][0], coefficients[coefficients.Count - 1][1]);
                res.dataRelevant = false;
                res.rootFound = true;
            }
            else
            {
                //all the roots have been found, set dataRelevant to false and set allRootsFound to true;
                res.allRootsFound = true;
                res.dataRelevant = false;
            }
            if (roots.Length - foundRootCount == 0)
            {
                //make sure allRootsFound is set to true;
                res.allRootsFound = true;
            }
            return res;
        }

        // Calculate a pair of roots by going through a set of iterations
        // Returns the last 2 roots in the roots array
        // If there is only a single root, it will be stored in Item1 and Item2 will be null
        public Tuple<Tuple<double, double>, Tuple<double, double>> calculateRootPair()
        {
            Tuple<Tuple<double, double>, Tuple<double, double>> results;
            if (roots.Length - foundRootCount > 0) //are we done yet?
            {   //guess not, so we need to find the next pair of roots
                //basically, call performIteration() until the result has rootFound set to true
                IterationResult iterResult = performIteration();
                while(!iterResult.rootFound)
                {
                    iterResult = performIteration();
                }
            }

            //return the last 2 or (if there is only 1) 1 root
            if (roots.Length > 1)
                results = new Tuple<Tuple<double, double>, Tuple<double, double>>(roots[foundRootCount - 2], roots[foundRootCount - 1]);
            else
                results = new Tuple<Tuple<double, double>, Tuple<double, double>>(new Tuple<double, double>(0, 0), roots[roots.Length - 1]);
            
            return results;
        }

        //calculates all the roots (assuming they are not already calculated)
        public Tuple<double, double>[] calculateRoots()
        {
            while (roots.Length - foundRootCount > 0)
            {
                performIteration(); //this does the heavy lifting
            }
            return roots;
        }



        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        //PRIVATE METHOD DEFINITIONS

        //This performs a single, actual iteration of Bairstow's method
        private IterationResult performIterationStep()
        {
            IterationResult result = new IterationResult();
            result.dataRelevant = true;

            //set up the arrays
            double[] aArray = coefficients[coefficients.Count - 1];
            double[] bArray = new double[aArray.Length];
            double[] cArray = new double[bArray.Length];

            //set up the r and s variables
            double r = startR;
            double s = startS;
            List<IterationResult> currSet = iterResults[iterResults.Count - 1];
            if (currSet.Count != 0) //is there current information for this iteration set
            {   //if so, set up the r and s using the previous specified values
                IterationResult prev = currSet[currSet.Count - 1];//iterResults[overIterNum][iterNum - 1];
                r = prev.iterR + prev.deltaR;
                s = prev.iterS + prev.deltaS;
            }
            else if(foundRootCount > 0) //have we found any roots
            {   //if so, use the previous 2 values of r and s as good starting values of r and s
                List<IterationResult> prevSet = iterResults[iterResults.Count - 2];
                IterationResult lastOfPrev = prevSet[prevSet.Count - 1];
                r = lastOfPrev.iterR + lastOfPrev.deltaR;
                s = lastOfPrev.iterS + lastOfPrev.deltaS;
            }

            if(currSet.Count >= maxIterNum)
            {
                result.maxIterationsExceeded = true;
                throw new MAX_ITER_NUM_REACHED_EXCEPTION();
            }

            //Calculate the bArray and cArray values
            //This is possible and necessary for 2 reasons:
            //  1)  Synthetic division can be represented by using a recurrence relation.  I store the reults of the
            //      division in the bArray.
            //  2)  Somehow (dark magicks), the partial derivatives used for calculating the deltas can be bypassed 
            //      by performing a similar synthetic division / recurrence relation on the bArray as we did to the
            //      initial coefficients on the aArray, and then using the values in other calculations.  The 
            //      results of this division are stored in the cArray.
            //
            //  bArray's recurrence relation for b[0] to b[n]
            //      b[n] = a[n]
            //      b[n-1] = a[n-1] + r * b[n]
            //      b[i] = a[i] + r * b[i+1] + s * b[i+2]   //for i = n-2 to 0
            //
            //  cArray's recurrence relation for c[1] to c[n]
            //      c[n] = b[n]
            //      c[n-1] = b[n-1] + r * c[n]
            //      c[i] = b[i] + r * c[i+1] + s * c[i+2]   //for i = n-2 to 1 //but we continue it for laziness / code format's sake
            //
            //To save time, i perform both divisions basically simultaneously
            bArray[bArray.Length - 1] = aArray[bArray.Length - 1];
            bArray[bArray.Length - 2] = aArray[aArray.Length - 2] + r * bArray[bArray.Length - 1];
            cArray[bArray.Length - 1] = bArray[bArray.Length - 1];
            cArray[bArray.Length - 2] = bArray[bArray.Length - 2] + r * cArray[bArray.Length - 1];
            for(int i = bArray.Length - 3; i >= 0; --i)
            {
                bArray[i] = aArray[i] + r * bArray[i + 1] + s * bArray[i + 2];
                cArray[i] = bArray[i] + r * cArray[i + 1] + s * cArray[i + 2];
            }

            //calculate the deltas for the next iteration
            //solve a system of two equations for deltaS and deltaR
            //the c values listed below correspon (somehow) with the partial derivatives needed for calculating the deltas
            //  c2 * deltaR + c3 * deltaS = -b1 
            //  c1 * deltaR + c2 * deltaS = -b0
            double deltaS = 0;
            double deltaR = 0;
            {
                int count = currSet.Count;

                int bob = 1;

                //assign some values for convenience
                double c1 = cArray[1];
                double c2 = cArray[2];
                double c3 = cArray[3];
                double b1 = bArray[1];
                double b0 = bArray[0];

                //solve a system of two equations for deltaS and deltaR
                //  c2 * deltaR + c3 * deltaS = -b1
                //  c1 * deltaR + c2 * deltaS = -b0

                //bad solution that involved maybe dividing by 0
                //ds = ( ( -c1 * b1 / c2 ) + b0) / ( ( c1 * c3 / c2 ) - c2)
                //  deltaS = (((0 - c1) * b1 / c2) + b0) / ( ( c1 * c3 / c2 ) - c2 );
                //(-b1 - c3 * ds) / c2 = dr
                //  deltaR = ((0 - b1) - c3 * deltaS) / c2;

                //because of the earlier, bad solution that would end up with a div by 0 error, I decided to solve
                //the system of equations using a series of matries

                //here we go matrices
                double one_one, one_two, one_three, two_one, two_two, two_three, store_meh;
                one_one = c2;
                one_two = c3;
                one_three = -b1;
                two_one = c1;
                two_two = c2;
                two_three = -b0;

                //Current state of the matrix
                //  [c2   c3  -b1]
                //  [c1   c2  -b0]
                //
                //corresponds to
                //
                //  [1-1      1-2     1-3]
                //  [2-1      2-2     2-3]

                //from here on out, whenever i use num1 or num2 or num3 or .. in a matrix, i'm just referring to some number, not a specific number from a previous diagram
                //when i use a numX in an english statement, assume im referring to the diagram directly above

                //add the bottom row to the top and the top to the bottom in hopes of getting rid of any 0s in the coefficients to deltaR and deltaS
                {
                    bool good = false;
                    double temp_one = 0, temp_two = 0, temp_three = 0, mult = 1;

                    //add some multiple of the bottom row to the top
                    while (!good) {
                        temp_one = one_one + two_one * mult;
                        temp_two = one_two + two_two * mult;
                        temp_three = one_three + two_three * mult;
                        good = (temp_one != 0) && (temp_two != 0); //make sure neither of the important two are 0
                        ++mult;
                    }
                    one_one = temp_one;
                    one_two = temp_two;
                    one_three = temp_three;

                    good = false;
                    mult = 0; //because we may not have to actually add anything to make the bottom happy
                    //add some multiple of the top row to the bottom
                    while (!good)
                    {
                        temp_one = one_one * mult + two_one;
                        temp_two = one_two * mult + two_two;
                        temp_three = one_three * mult + two_three;
                        good = (temp_one != 0) && (temp_two != 0); //make sure neither of the important two are 0
                        ++mult;
                    }
                    two_one = temp_one;
                    two_two = temp_two;
                    two_three = temp_three;
                }

                /*
                //old, bad code for the above code
                //add the bottom row to the top
                one_one += two_one;
                one_two += two_two;
                one_three += two_three;
                //add the top row to the bottom
                two_one += one_one;
                two_two += one_two;
                two_three += one_three;
                */

                //hopefully, every cell has something in it (otherwise I may be screwed...)
                //store one_one and divide that row by it to get
                //  [1    num1    num2]
                store_meh = one_one;
                one_one = one_one / store_meh;
                one_two = one_two / store_meh;
                one_three = one_three / store_meh;


                //current state is
                //  [1    num1    num2]
                //  [num3 num4    num5]

                //subtract num3 from num 3, num1 * num 3 from num4, and num2 * num3 from num5
                //yields a bottom row of
                //  [0    num6    num7]
                store_meh = two_one;
                two_one = 0;
                two_two -= (store_meh * one_two);
                two_three -= (store_meh * one_three);



                //current state is
                //  [1    num1    num2]
                //  [0    num3    num4]

                //divide num3 by num3 to get 1 and num4 by num3
                //yields a bottom row of
                //  [0    1   num5]
                store_meh = two_two;
                two_two = 1;
                two_three = two_three / store_meh;



                //current state is below
                //[1    num1    num2]
                //[0    1       num3]

                //subtract num1 from itself and subtract (num3 * num1) from num2
                //yields a top row of
                //[1    0       num4]

                store_meh = one_two;
                one_two = 0;
                one_three -= (store_meh * two_three);



                //current (and final) state is
                //[1    0   num1]
                //[0    1   num2]

                //so, deltaR = num1 and deltaS = num2
                deltaR = one_three;
                deltaS = two_three;
            }

            //get the errors
            double sErr = /*make sure s isn't zero*/(s != 0) ? Math.Abs(deltaS / s)
                : (deltaS != 0) ? 1 : 0; //if s is zero and deltaS isn't, error should be 100%, otherwise we apparently nailed it head on
            double rErr = /*make sure r isn't zero*/(r != 0) ? Math.Abs(deltaR / r) 
                : (deltaR != 0) ? 1 : 0; //if r is zero and deltaR isn't, error should be 100%, otherwise we apparently nailed it head on

            //assign all the relevant data
            result.iterR = r;
            result.iterS = s;
            result.iterRError = rErr;
            result.iterSError = sErr;
            result.deltaR = deltaR;
            result.deltaS = deltaS;

            

            //if latest error is beneath allowedError, calculate roots using solveRSQuadratic and store the data the roots array as well as update necessary instance variables
            //make sure to set res.rootFound to true;
            if (sErr < allowedError && rErr < allowedError)
            {
                result.rootFound = true; //make sure the result knows it found a root

                //solve for the roots and assign them
                Tuple<Tuple<double, double>, Tuple<double, double>> rootPair = solveRSQuadratic(r, s);
                roots[foundRootCount++] = rootPair.Item1;
                roots[foundRootCount++] = rootPair.Item2;

                //if there are more roots to be found, add another iterationresults list
                if(roots.Length - foundRootCount > 0)
                {
                    iterResults.Add(new List<IterationResult>());

                    //calculate / assign the next set of coefficients
                    //supposedly, the next set of coefficients already reside in bArray
                    double[] nextSet = new double[aArray.Length - 2];
                    for(int i = 0; i < nextSet.Length; ++i)
                    {
                        nextSet[i] = bArray[i + 2];
                    }
                    coefficients.Add(nextSet);
                }
                else
                {
                    result.allRootsFound = true;
                }
            }

            //make sure to add the results to the correct iteration set
            if (!result.rootFound)
                iterResults[iterResults.Count - 1].Add(result);
            else
                iterResults[iterResults.Count - 2].Add(result);

            return result;
        }

        //solves a (normal, not r/s) quadratic equation and stores the results in the roots table
        //this only happens if either the user submitted a quadratic to be solved or the highest power of x is even
        private IterationResult performQuadraticSolution()
        {
            IterationResult result = new IterationResult();
            result.dataRelevant = false;
            result.rootFound = true;

            double[] currentEq = coefficients[coefficients.Count - 1];

            //solve for the roots and assign them
            Tuple<Tuple<double, double>, Tuple<double, double>> rootPair = solveNormalQuadratic(currentEq[2], currentEq[1], currentEq[0]);
            roots[foundRootCount++] = rootPair.Item1;
            roots[foundRootCount++] = rootPair.Item2;

            if(roots.Length - foundRootCount == 0)
            {
                result.allRootsFound = true;
            }
            return result;
        }

        //solves a quadratic of the form 0 = x^2 + rx + s and returns its roots
        //is its own thing because reasons
        private Tuple<Tuple<double, double>, Tuple<double,double> > solveRSQuadratic(double r, double s)
        {
            double discriminant = r * r + 4 * s;
            double real1, real2, complex1, complex2;
            if(discriminant > 0)
            {
                real1 = (r + Math.Sqrt(discriminant)) / 2;
                real2 = (r - Math.Sqrt(discriminant)) / 2;
                complex1 = 0;
                complex2 = 0;
            }
            else
            {
                real1 = r / 2;
                real2 = real1;
                complex1 = Math.Sqrt(Math.Abs(discriminant)) / 2;
                complex2 = 0 - complex1;
            }
            Tuple<Tuple<double, double>, Tuple<double, double>> results;
            results = new Tuple<Tuple<double, double>, Tuple<double, double>>(new Tuple<double, double>(real1, complex1), new Tuple<double, double>(real2, complex2));
            return results;
        }

        //solves a normal quadratic of the form 0 = ax^2 + bx + c and returns its roots
        //is its own thing because reasons
        private Tuple<Tuple<double, double>, Tuple<double,double>> solveNormalQuadratic(double a, double b, double c)
        {
            double discriminant = b * b - 4 * a * c;
            double real1, real2, complex1, complex2;
            if(discriminant > 0)
            {
                real1 = (b + Math.Sqrt(discriminant)) / (2 * a);
                real2 = (b - Math.Sqrt(discriminant)) / (2 * a);
                complex1 = 0;
                complex2 = 0;
            }
            else
            {
                real1 = b / (2 * a);
                real2 = real1;
                complex1 = Math.Sqrt(Math.Abs(discriminant)) / (2 * a);
                complex2 = 0 - complex1;
            }
            Tuple<Tuple<double, double>, Tuple<double, double>> results;
            results = new Tuple<Tuple<double, double>, Tuple<double, double>>(new Tuple<double, double>(real1, complex1), new Tuple<double, double>(real2, complex2));
            return results;
        }

        //solves a single, linear equation of the form 0 = a(1)x + a(0) and returns its root / solution
        private Tuple<double, double> solveSingle(double a0, double a1)
        {
            return new Tuple<double, double>((0 - a0) / a1, 0);
        }
    }
}
