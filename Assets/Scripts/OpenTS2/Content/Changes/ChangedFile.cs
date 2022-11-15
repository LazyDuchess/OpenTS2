using OpenTS2.Common;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content.Changes
{
    public class ChangedFile : AbstractChanged
    {
        protected byte[] FileData;
        public ChangedFile(byte[] fileData, ResourceKey tgi, DBPFFile package, AbstractCodec codec = null)
        {
            this.FileData = fileData;
            if (codec != null)
            {
                this.Asset = codec.Deserialize(fileData, tgi, package);
            }
            this.Entry = new DBPFEntry()
            {
                GlobalTGI = this.Asset.GlobalTGI.LocalGroupID(package.GroupID),
                InternalTGI = this.Asset.GlobalTGI,
                Dynamic = true,
                Package = package
            };
        }
        public ChangedFile(AbstractAsset asset, byte[] fileData) : this(fileData, asset.InternalTGI, asset.Package)
        {

        }
        public override byte[] Bytes
        {
            get
            {
                return FileData;
            }
            set
            {
                FileData = value;
            }
        }
    }
}
