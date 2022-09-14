using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content
{
    public class ContentChanges
    {
        private ContentProvider contentProvider;
        private Files.Filesystem fileSystem;
        //private Dictionary<string, bool> m_DeletedPackagesByName = new Dictionary<string, bool>();
        //private List<string> m_DeletedPackages = new List<string>();

        public ContentChanges(ContentProvider contentProvider, Files.Filesystem fileSystem)
        {
            this.contentProvider = contentProvider;
            this.fileSystem = fileSystem;
        }
        /// <summary>
        /// Clear all runtime changes made to packages.
        /// </summary>
        public void Clear()
        {
            var entries = contentProvider.ContentEntries;
            foreach (var element in entries)
            {
                if (element.Changes.Dirty)
                {
                    element.Changes.Clear();
                }
            }
        }
        /// <summary>
        /// Write all changes to disk.
        /// </summary>
        public void SaveChangesToDisk()
        {
            var entries = contentProvider.ContentEntries;
            foreach(var element in entries)
            {
                if (element.Changes.Dirty)
                {
                    element.WriteToFile();
                }
            }
        }
    }
}
