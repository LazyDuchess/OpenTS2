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
        private readonly ContentProvider _contentProvider;
        //private Dictionary<string, bool> m_DeletedPackagesByName = new Dictionary<string, bool>();
        //private List<string> m_DeletedPackages = new List<string>();

        public ContentChanges(ContentProvider contentProvider)
        {
            this._contentProvider = contentProvider;
        }
        /// <summary>
        /// Clear all runtime changes made to packages.
        /// </summary>
        public void Clear()
        {
            var entries = _contentProvider.ContentEntries;
            foreach (var element in entries)
            {
                if (element.Changes.Dirty)
                {
                    element.Changes.Clear();
                }
            }
        }
        /// <summary>
        /// Write all changes to the filesystem.
        /// </summary>
        public void SaveChanges()
        {
            var entries = _contentProvider.ContentEntries;
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
