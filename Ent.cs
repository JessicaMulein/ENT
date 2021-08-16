namespace Ent
{
    using System;

    /// <summary>
    ///	Apply various randomness tests to a stream of bytes
    ///	Original code by John Walker  --  September 1996
    ///	http://www.fourmilab.ch/
    ///	
    /// C# port of ENT (ent - pseudorandom number sequence test)
    /// by Jessica Mulein
    /// jessica@mulein.com
    /// </summary>

    public class EntCalc
    {
        static readonly double[,] chsqt = new double[2, 10]
            {
                {0.5, 0.25, 0.1, 0.05, 0.025, 0.01, 0.005, 0.001, 0.0005, 0.0001},
                {0.0, 0.6745, 1.2816, 1.6449, 1.9600, 2.3263, 2.5758, 3.0902, 3.2905, 3.7190}
            };

        private static readonly int MONTEN = 6;             /* Bytes used as Monte Carlo co-ordinates
													 * This should be no more bits than the mantissa 
													 * of your "double" floating point type. */

        private readonly uint[] monte = new uint[MONTEN];
        private readonly double[] prob = new double[256];   /* Probabilities per bin for entropy */
        private readonly long[] ccount = new long[256];     /* Bins to count occurrences of values */
        private long totalc = 0;                    /* Total bytes counted */

        private int mp;
        private bool sccfirst;
        private long inmont, mcount;
        private double a;
        private double cexp;
        private readonly double incirc;
        private double montex, montey, montePi;
        private double scc, sccun, sccu0, scclast, scct1, scct2, scct3;
        private double ent, chiSq, datasum;

        private readonly bool binary = false;               /* Treat input as a bitstream */

        public struct EntCalcResult
        {
            public double Entropy;
            public double ChiSquare;
            public double Mean;
            public double MonteCarloPiCalc;
            public double SerialCorrelation;
            public long[] OccuranceCount;

            public double ChiProbability;
            public double MonteCarloErrorPct;
            public double OptimumCompressionReductionPct;
            public double ExpectedMeanForRandom;

            public long NumberOfSamples;
        }


        /*  Initialise random test counters.  */
        public EntCalc(bool binmode)
        {
            int i;

            this.binary = binmode;              /* Set binary / byte mode */

            /* Initialise for calculations */

            this.ent = 0.0;                     /* Clear entropy accumulator */
            this.chiSq = 0.0;                   /* Clear Chi-Square */
            this.datasum = 0.0;                 /* Clear sum of bytes for arithmetic mean */

            this.mp = 0;                            /* Reset Monte Carlo accumulator pointer */
            this.mcount = 0;                        /* Clear Monte Carlo tries */
            this.inmont = 0;                        /* Clear Monte Carlo inside count */
            this.incirc = 65535.0 * 65535.0;        /* In-circle distance for Monte Carlo */

            this.sccfirst = true;               /* Mark first time for serial correlation */
            this.scct1 = this.scct2 = this.scct3 = 0.0; /* Clear serial correlation terms */

            this.incirc = Math.Pow(Math.Pow(256.0, MONTEN / 2) - 1, 2.0);

            for (i = 0; i < 256; i++)
            {
                this.ccount[i] = 0;
            }
            this.totalc = 0;
        }

        /*  AddSample  --	Add one or more bytes to accumulation.	*/
        public void AddSample(int buf, bool Fold)
        {
            this.AddSample((byte)buf, Fold);
        }

        public void AddSample(byte buf, bool Fold)
        {
            byte[] tmpByte = new byte[1];
            tmpByte[0] = buf;
            this.AddSample(tmpByte, Fold);
        }

        public void AddSample(byte[] buf, bool Fold)
        {
            int c, bean;

            foreach (byte bufByte in buf)
            {
                bean = 0;       // reset bean

                byte oc = bufByte;

                /*	Have not implemented folding yet. Plan to use System.Text.Encoding to do so.
                 * 
                 *				if (fold && isISOalpha(oc) && isISOupper(oc)) 
                 *				{
                 *					oc = toISOlower(oc);
                 *				}
                 */

                do
                {
                    if (this.binary)
                    {
                        c = ((oc & 0x80));      // Get the MSB of the byte being read in
                    }
                    else
                    {
                        c = oc;
                    }
                    this.ccount[c]++;         /* Update counter for this bin */
                    this.totalc++;

                    /* Update inside / outside circle counts for Monte Carlo computation of PI */

                    if (bean == 0)
                    {
                        this.monte[this.mp++] = oc;       /* Save character for Monte Carlo */
                        if (this.mp >= MONTEN)
                        {     /* Calculate every MONTEN character */
                            int mj;

                            this.mp = 0;
                            this.mcount++;
                            this.montex = this.montey = 0;
                            for (mj = 0; mj < MONTEN / 2; mj++)
                            {
                                this.montex = (this.montex * 256.0) + this.monte[mj];
                                this.montey = (this.montey * 256.0) + this.monte[(MONTEN / 2) + mj];
                            }
                            if ((this.montex * this.montex + this.montey * this.montey) <= this.incirc)
                            {
                                this.inmont++;
                            }
                        }
                    }

                    /* Update calculation of serial correlation coefficient */

                    this.sccun = c;
                    if (this.sccfirst)
                    {
                        this.sccfirst = false;
                        this.scclast = 0;
                        this.sccu0 = this.sccun;
                    }
                    else
                    {
                        this.scct1 = this.scct1 + this.scclast * this.sccun;
                    }
                    this.scct2 = this.scct2 + this.sccun;
                    this.scct3 = this.scct3 + (this.sccun * this.sccun);
                    this.scclast = this.sccun;
                    oc <<= 1;                       // left shift by one
                } while (this.binary && (++bean < 8));      // keep looping if we're in binary mode and while the bean counter is less than 8 (bits)
            }
        }   // end foreach


        /*  EndCalculation  --	Complete calculation and return results.  */
        public EntCalcResult EndCalculation()
        {
            int i;

            /* Complete calculation of serial correlation coefficient */

            this.scct1 = this.scct1 + this.scclast * this.sccu0;
            this.scct2 = this.scct2 * this.scct2;
            this.scc = this.totalc * this.scct3 - this.scct2;
            if (this.scc == 0.0)
            {
                this.scc = -100000;
            }
            else
            {
                this.scc = (this.totalc * this.scct1 - this.scct2) / this.scc;
            }

            /* Scan bins and calculate probability for each bin and
			   Chi-Square distribution */

            this.cexp = this.totalc / (this.binary ? 2.0 : 256.0);  /* Expected count per bin */
            for (i = 0; i < (this.binary ? 2 : 256); i++)
            {
                this.prob[i] = (double)this.ccount[i] / this.totalc;
                this.a = this.ccount[i] - this.cexp;
                this.chiSq = this.chiSq + (this.a * this.a) / this.cexp;
                this.datasum += ((double)i) * this.ccount[i];
            }

            /* Calculate entropy */

            for (i = 0; i < (this.binary ? 2 : 256); i++)
            {
                if (this.prob[i] > 0.0)
                {
                    this.ent += this.prob[i] * Math.Log(1 / this.prob[i], 2);
                }
            }

            /* Calculate Monte Carlo value for PI from percentage of hits
			   within the circle */

            this.montePi = 4.0 * (((double)this.inmont) / this.mcount);

            /* Calculate probability of observed distribution occurring from
			   the results of the Chi-Square test */

            double chiP = Math.Sqrt(2.0 * this.chiSq) - Math.Sqrt((2.0 * (this.binary ? 1 : 255.0)) - 1.0);
            this.a = Math.Abs(chiP);
            for (i = 9; i >= 0; i--)
            {
                if (chsqt[1, i] < this.a)
                {
                    break;
                }
            }

            chiP = (chiP >= 0.0) ? chsqt[0, i] : 1.0 - chsqt[0, i];

            double compReductionPct = ((this.binary ? 1 : 8) - this.ent) / (this.binary ? 1.0 : 8.0);

            /* Return results */
            EntCalcResult result = default(EntCalcResult);
            result.Entropy = this.ent;
            result.ChiSquare = this.chiSq;
            result.ChiProbability = chiP;
            result.Mean = this.datasum / this.totalc;
            result.ExpectedMeanForRandom = this.binary ? 0.5 : 127.5;
            result.MonteCarloPiCalc = this.montePi;
            result.MonteCarloErrorPct = Math.Abs(Math.PI - this.montePi) / Math.PI;
            result.SerialCorrelation = this.scc;
            result.OptimumCompressionReductionPct = compReductionPct;
            result.OccuranceCount = this.ccount;
            result.NumberOfSamples = this.totalc;
            return result;
        }
    }
}
