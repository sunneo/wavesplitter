using System;
using System.IO;

namespace WaveTextFormatter
{
	public class WaveTxtParserClass
	{
		public static int Main (string[] args)
		{
            FileNameTableDocument doc = new FileNameTableDocument("test.txt");
            doc.Parse();
            Console.WriteLine(doc.ToString());
            Console.ReadLine();
			return 0;
		}
	}
}
