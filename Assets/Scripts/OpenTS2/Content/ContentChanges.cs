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
        /// <summary>
        /// Clear all runtime changes made to packages.
        /// </summary>
        public void Clear()
        {
            var entries = ContentManager.Instance.ContentEntries;
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
            var entries = ContentManager.Instance.ContentEntries;
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
