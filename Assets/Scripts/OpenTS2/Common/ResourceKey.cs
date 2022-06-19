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

namespace OpenTS2.Common
{
    /// <summary>
    /// Type, Group and Instance ID reference for packaged resources.
    /// </summary>
    public class ResourceKey
    {
        public static ResourceKey Default
        {
            get { return _Default; }
        }
        static ResourceKey _Default = new ResourceKey(0, 0, 0);

        private uint _InstanceID;
        private uint _InstanceHigh;
        private uint _GroupID;
        private uint _TypeID;

        public uint InstanceID
        {
            get { return _InstanceID; }
        }

        public uint InstanceHigh
        {
            get { return _InstanceHigh; }
        }

        public uint GroupID
        {
            get { return _GroupID; }
        }

        public uint TypeID
        {
            get { return _TypeID; }
        }

        /// <summary>
        /// Creates a Instance ID, Group ID and Type ID reference.
        /// </summary>
        /// <param name="instanceID">Instance ID</param>
        /// <param name="groupID">Group ID</param>
        /// <param name="typeID">Type ID</param>
        public ResourceKey(uint instanceID, uint groupID, uint typeID)
        {
            this._InstanceID = instanceID;
            this._InstanceHigh = 0x00000000;
            this._GroupID = groupID;
            this._TypeID = typeID;
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
            this._InstanceID = instanceID;
            this._InstanceHigh = instanceHigh;
            this._GroupID = groupID;
            this._TypeID = typeID;
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
            this._InstanceID = FileUtils.LowHash(filename);
            this._InstanceHigh = FileUtils.HighHash(filename);
            this._GroupID = groupID;
            this._TypeID = typeID;
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
            this._InstanceID = FileUtils.LowHash(filename);
            this._InstanceHigh = FileUtils.HighHash(filename);
            this._GroupID = FileUtils.GroupHash(groupName);
            this._TypeID = typeID;
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
            this._InstanceID = instanceID;
            this._InstanceHigh = instanceHigh;
            this._GroupID = FileUtils.GroupHash(groupName);
            this._TypeID = typeID;
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
            this._InstanceID = instanceID;
            this._InstanceHigh = 0x00000000;
            this._GroupID = FileUtils.GroupHash(groupName);
            this._TypeID = typeID;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + _InstanceID.GetHashCode();
                hash = hash * 23 + _InstanceHigh.GetHashCode();
                hash = hash * 23 + _TypeID.GetHashCode();
                hash = hash * 23 + _GroupID.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ResourceKey);
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
