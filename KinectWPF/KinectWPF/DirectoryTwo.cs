using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectWPF
{
    public static class DirectoryTwo
    {
        public static void RecursiveDelete(string path)
        {
            var entries = Directory.GetFileSystemEntries(path);

            foreach (var entry in entries)
            {
                FileAttributes attributes = File.GetAttributes(entry);

                if (attributes.HasFlag(FileAttributes.Directory))
                {
                    RecursiveDelete(entry);
                    Directory.Delete(entry);
                }
                else
                    File.Delete(entry);
            }
        }
    }
}
