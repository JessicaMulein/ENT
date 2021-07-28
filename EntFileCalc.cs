namespace Ent
{
    using System.IO;

    public class EntFileCalc
    {
        public static EntCalc.EntCalcResult CalculateFile(ref FileStream inStream)
        {

            EntCalc entCalc = new EntCalc(false);
            while (inStream.Position < inStream.Length)
            {
                entCalc.AddSample((byte)inStream.ReadByte(), false);
            }

            return entCalc.EndCalculation();
        }

        public static Ent.EntCalc.EntCalcResult CalculateFile(string inFileName)
        {
            FileStream instream = new FileStream(inFileName, FileMode.Open, FileAccess.Read, FileShare.None);

            instream.Position = 0;

            EntCalc.EntCalcResult tmpRes = CalculateFile(ref instream);

            instream.Close();

            return tmpRes;
        }
    }
}
