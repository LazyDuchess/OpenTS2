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
        private Dictionary<string, bool> m_DeletedPackagesByName = new Dictionary<string, bool>();
        private List<string> m_DeletedPackages = new List<string>();

        public ContentChanges(ContentProvider contentProvider, Files.Filesystem fileSystem)
        {
            this.contentProvider = contentProvider;
            this.fileSystem = fileSystem;
        }

        void SelfClear()
        {
            m_DeletedPackages.Clear();
            m_DeletedPackagesByName.Clear();
        }
        /// <summary>
        /// Clear all runtime changes made to packages.
        /// </summary>
        public void Clear()
        {
            SelfClear();
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
        /// Restore a package marked as deleted.
        /// </summary>
        /// <param name="packagePath">Path to package</param>
        public void RestorePackage(string packagePath)
        {
            m_DeletedPackages.Remove(packagePath);
            m_DeletedPackagesByName.Remove(packagePath);
        }
        /// <summary>
        /// Mark a package as deleted
        /// </summary>
        /// <param name="packagePath">Path to package</param>
        public void DeletePackage(string packagePath)
        {
            if (IsPackageDeleted(packagePath))
                return;
            m_DeletedPackagesByName[packagePath] = true;
            m_DeletedPackages.Add(packagePath);
        }
        /// <summary>
        /// Check if a package is marked as deleted, even if it still technically exists.
        /// </summary>
        /// <param name="packagePath">Path to package</param>
        /// <returns></returns>
        public bool IsPackageDeleted(string packagePath)
        {
            var realPath = fileSystem.GetRealPath(packagePath);
            if (m_DeletedPackagesByName.ContainsKey(realPath))
                return true;
            return false;
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
                    if (m_DeletedPackagesByName.ContainsKey(element.FilePath))
                    {
                        RestorePackage(element.FilePath);
                    }
                    element.WriteToFile();
                }
            }
            foreach(var element in m_DeletedPackages)
            {
                fileSystem.Delete(element);
            }
            SelfClear();
        }
    }
}
