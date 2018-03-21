using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CfdiUpload
{
    public class LogUtils
    {
        public static void LogErrorMessage(string message)
        {
            string filename = string.Format("logfile-{0}.log", DateTime.Now.ToString("dd/MM/yyyy"));
            if (!File.Exists(Path.Combine(Environment.CurrentDirectory, filename))) //No File? Create
            {
                File.Create(Path.Combine(Environment.CurrentDirectory, filename));
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(Path.Combine(Environment.CurrentDirectory, filename), true))
                {
                    file.WriteLine(string.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), message));
                }
            }
            else
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(Path.Combine(Environment.CurrentDirectory, filename), true))
                {
                    file.WriteLine(string.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), message));
                }
            }

        }
    }
}
