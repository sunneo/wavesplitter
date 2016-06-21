using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WaveSplitter
{
    public class Program
    {
        public static String ApplicationStartPath = Application.StartupPath+"\\";
        public static char[] slashes = { '/', '\\' };
        public static String applicationStartDir(int layers = 0)
        {
            String str = ApplicationStartPath;
            while (true)
            {
                int index1 = str.LastIndexOfAny(slashes);
                if (index1 == -1)
                {
                    break;
                }
                str = str.Substring(0, index1);
                if (layers <= 0)
                {
                    break;
                }
                --layers;

            }
            return str;

        }
        [STAThread]
        static void Main(string[] args)
        {
            Application.Run(new WavSplitForm());
        }
    }
}
