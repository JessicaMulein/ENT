using System.IO;

namespace Ent;

public class EntFileCalc
{
    public static EntCalc.EntCalcResult CalculateFile(ref FileStream inStream)
    {
        var entCalc = new EntCalc(binmode: false);
        while (inStream.Position < inStream.Length)
        {
            entCalc.AddSample(buf: (byte)inStream.ReadByte(),
                Fold: false);
        }

        return entCalc.EndCalculation();
    }

    public static EntCalc.EntCalcResult CalculateFile(string inFileName)
    {
        var instream = new FileStream(path: inFileName,
            mode: FileMode.Open,
            access: FileAccess.Read,
            share: FileShare.None);

        instream.Position = 0;

        var tmpRes = CalculateFile(inStream: ref instream);

        instream.Close();

        return tmpRes;
    }
}
