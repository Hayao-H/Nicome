using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Nicome.Utils
{
    class IO
    {
        public static string GetRootDir()
        {
            string? name = Assembly.GetExecutingAssembly().GetName().Name;
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),name==null?"Nicome":name);
        }

        public static string GetOrCreateRootDir()
        {
            string root = GetRootDir();

            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            return root;
        }
    }
}
