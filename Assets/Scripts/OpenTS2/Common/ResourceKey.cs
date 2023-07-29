/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Common.Utils;
using OpenTS2.Files.Formats.DBPF;

namespace OpenTS2.Common
{
    /// <summary>
    /// Type, Group and Instance ID reference for packaged resources.
    /// </summary>
    public struct ResourceKey
    {
        public static ResourceKey DIR
        {
            get { return s_dir; }
        }

        public uint InstanceID
        {
            get { return _instanceID; }
        }

        public uint InstanceHigh
        {
            get { return _instanceHigh; }
        }

        public uint GroupID
        {
            get { return _groupID; }
        }

        public uint TypeID
        {
            get { return _typeID; }
        }

        static readonly ResourceKey s_dir = new ResourceKey(0x286B1F03, 0xE86B1EEF, 0xE86B1EEF);

        private readonly uint _instanceID;
        private readonly uint _instanceHigh;
        private readonly uint _groupID;
        private readonly uint _typeID;

        /// <summary>
        /// Returns new TGI with its Group ID replaced with Groups.Local, but only if our Group ID equals localGroupID
        /// </summary>
        /// <param name="localGroupID">Local Group ID</param>
        /// <returns></returns>
        public ResourceKey GlobalGroupID(uint localGroupID)
        {
            if (this.GroupID == localGroupID)
                return WithGroupID(GroupIDs.Local);
            return this;
        }

        /// <summary>
        /// Returns new TGI with its Group ID replaced with localGroupID, but only if our Group ID equals Groups.Local
        /// </summary>
        /// <param name="localGroupID">Local Group ID</param>
        /// <returns></returns>
        public ResourceKey LocalGroupID(uint localGroupID)
        {
            if (this.GroupID == GroupIDs.Local)
                return WithGroupID(localGroupID);
            return this;
        }
        public ResourceKey WithGroupID(uint groupID)
        {
            return new ResourceKey(this.InstanceID, this.InstanceHigh, groupID, this.TypeID);
        }

        /// <summary>
        /// Creates a Instance ID, Group ID and Type ID reference.
        /// </summary>
        /// <param name="instanceID">Instance ID</param>
        /// <param name="groupID">Group ID</param>
        /// <param name="typeID">Type ID</param>
        public ResourceKey(uint instanceID, uint groupID, uint typeID)
        {
            this._instanceID = instanceID;
            this._instanceHigh = 0x00000000;
            this._groupID = groupID;
            this._typeID = typeID;
        }

        /// <summary>
        /// Creates a Instance ID, Instance (High), Group ID and Type ID reference.
        /// </summary>
        /// <param name="instanceID">Instance ID</param>
        /// <param name="instanceHigh">High Instance ID</param>
        /// <param name="groupID">Group ID</param>
        /// <param name="typeID">Type ID</param>
        public ResourceKey(uint instanceID, uint instanceHigh, uint groupID, uint typeID)
        {
            this._instanceID = instanceID;
            this._instanceHigh = instanceHigh;
            this._groupID = groupID;
            this._typeID = typeID;
        }

        /// <summary>
        /// Creates a Instance ID, Instance (High), Group ID and Type ID reference.
        /// Instance IDs are automatically hashed from a filename.
        /// </summary>
        /// <param name="filename">Filename for Instance ID</param>
        /// <param name="groupID">Group ID</param>
        /// <param name="typeID">Type ID</param>
        public ResourceKey(string filename, uint groupID, uint typeID)
        {
            this._instanceID = FileUtils.LowHash(filename);
            this._instanceHigh = FileUtils.HighHash(filename);
            this._groupID = groupID;
            this._typeID = typeID;
        }
        /// <summary>
        /// Creates a Instance ID, Instance (High), Group ID and Type ID reference.
        /// Instance IDs and Group ID are automatically hashed from filenames.
        /// </summary>
        /// <param name="filename">Filename for Instance ID</param>
        /// <param name="groupName">Group name/Package name for Group ID</param>
        /// <param name="typeID">Type ID</param>
        public ResourceKey(string filename, string groupName, uint typeID)
        {
            this._instanceID = FileUtils.LowHash(filename);
            this._instanceHigh = FileUtils.HighHash(filename);
            this._groupID = FileUtils.GroupHash(groupName);
            this._typeID = typeID;
        }
        /// <summary>
        /// Creates a Instance ID, Instance (High), Group ID and Type ID reference.
        /// Group ID is automatically hashed from a filename.
        /// </summary>
        /// <param name="instanceID">Instance ID</param>
        /// <param name="instanceHigh">High Instance ID</param>
        /// <param name="groupName">Group name/Package name for Group ID</param>
        /// <param name="typeID">Type ID</param>
        public ResourceKey(uint instanceID, uint instanceHigh, string groupName, uint typeID)
        {
            this._instanceID = instanceID;
            this._instanceHigh = instanceHigh;
            this._groupID = FileUtils.GroupHash(groupName);
            this._typeID = typeID;
        }
        /// <summary>
        /// Creates a Instance ID, Group ID and Type ID reference.
        /// Group ID is automatically hashed from a filename.
        /// </summary>
        /// <param name="instanceID">Instance ID</param>
        /// <param name="groupName">Group name/Package name for Group ID</param>
        /// <param name="typeID">Type ID</param>
        public ResourceKey(uint instanceID, string groupName, uint typeID)
        {
            this._instanceID = instanceID;
            this._instanceHigh = 0x00000000;
            this._groupID = FileUtils.GroupHash(groupName);
            this._typeID = typeID;
        }

        /// <summary>
        /// Creates a ResourceKey for scenegraph file names such as:
        /// "N002_Lot3!imposter_shpe"
        ///
        /// These are handled a little more specially than just usual file names. They can encode group ids in the
        /// file name as well as have custom prefixes.
        /// </summary>
        public static ResourceKey ScenegraphResourceKey(string filename, uint groupID, uint typeID)
        {
            if (filename.Contains('!'))
            {
                filename = filename.Substring(filename.LastIndexOf('!') + 1);
            }

            return new ResourceKey(filename, groupID, typeID);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + _instanceID.GetHashCode();
                hash = hash * 23 + _instanceHigh.GetHashCode();
                hash = hash * 23 + _typeID.GetHashCode();
                hash = hash * 23 + _groupID.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return (obj is ResourceKey other && this.Equals(other));
        }

        public bool Equals(ResourceKey obj)
        {
            return (InstanceHigh == obj.InstanceHigh && InstanceID == obj.InstanceID && GroupID == obj.GroupID && TypeID == obj.TypeID);
        }

        public override string ToString()
        {
            return "T: 0x"+TypeID.ToString("X8")+", G: 0x"+ GroupID.ToString("X8")+", I(High): 0x"+ InstanceHigh.ToString("X8") + ", I: 0x" + InstanceID.ToString("X8");
        }
    }
}
