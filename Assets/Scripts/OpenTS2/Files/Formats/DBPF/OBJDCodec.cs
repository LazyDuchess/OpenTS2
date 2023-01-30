using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace OpenTS2.Files.Formats.DBPF
{
    /// <summary>
    /// Codec for Object Definitions.
    /// </summary>
    [Codec(TypeIDs.OBJD)]
    public class OBJDCodec : AbstractCodec
    {
        //File Spec: https://modthesims.info/wiki.php?title=4F424A44
        //TODO - Finish

        /// <summary>
        /// Parses OBJD from an array of bytes.
        /// </summary>
        /// <param name="bytes">Bytes to parse</param>
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var asset = new ObjectDefinitionAsset();
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            int offset = 0;

            asset.FileName = reader.ReadNullTerminatedUTF8();
            offset += 64 * sizeof(byte);

            object BoxedAsset = RuntimeHelpers.GetObjectValue(asset);

            foreach (KeyValuePair<int, string> pair in SerialOrder)
            {
                reader.Seek(SeekOrigin.Begin, offset);

                if (pair.Value != "unused")
                {
                    asset.GetType().GetProperty(pair.Value).SetValue(BoxedAsset, reader.ReadUInt16());
                }

                offset += sizeof(short);
            }

            asset = (ObjectDefinitionAsset)BoxedAsset;
            asset.GUID = (uint)(asset.GUID2 << 16) + (uint)asset.GUID1;

            return asset;
        }

        private readonly SortedList<int, string> SerialOrder = new SortedList<int, string>()
        {
            { 1, "GameVer1" },
            { 2, "GameVer2" },
            { 3, "InitialStackSize" },
            { 4, "DefaultWallAdjacentFlags" },
            { 5, "DefaultPlacementFlags" },
            { 6, "DefaultWallPlacementFlags" },
            { 7, "DefaultAllowedHeightFlags" },
            { 8, "InteractionTableIDPointer" },
            { 9, "InteractionGroup" },
            { 10, "ObjType" },
            { 11, "MultiTileMasterID" },
            { 12, "MultiTileSubIndex" },
            { 13, "UseDefaultPlacementFlags" },
            { 14, "LookAtScore" },
            { 15, "GUID1" },
            { 16, "GUID2" },
            { 17, "ItemIsUnlockable" },
            { 18, "CatalogUseFlags" },
            { 19, "BuyPrice" },
            { 20, "BodyStringsIDPointer" },
            { 21, "SlotIDPointer" },
            { 22, "DiagonalSelectorGUID1" },
            { 23, "DiagonalSelectorGUID2" },
            { 24, "GridAlignedSelectorGUID1" },
            { 25, "DiagonalSelectorGUID2" },
            { 26, "ObjOwnershipFlags" },
            { 27, "IgnoreGlobalSimField" },
            { 28, "CannotMoveOutWith" },
            { 29, "Hauntable" },
            { 30, "ProxyGUID1" },
            { 31, "ProxyGUID2" },
            { 32, "SlotGroup" },
            { 33, "AspirationFlags" },
            { 34, "MemorySubjectiveFeeling" },
            { 35, "SalePrice" },
            { 36, "InitialDepreciation" },
            { 37, "DailyDepreciation" },
            { 38, "SelfDepreciating" },
            { 39, "Depreciationlimit" },
            { 40, "RoomSortflags" },
            { 41, "FunctionSortFlags" },
            { 42, "CatalogStringsIDPointer" },
            { 43, "IsGlobalSimObj" },
            { 44, "ToolTipNameType" },
            { 45, "TemplateVer" },
            { 46, "NicenessMultiplier" },
            { 47, "NoDuplicateOnPlacement" },
            { 48, "WantCategory" },
            { 49, "NoNewNameFromTemplate" },
            { 50, "ObjVer" },
            { 51, "DefaultThumbnailID" },
            { 52, "MotiveEffectsID" },
            { 53, "JobObjGUID1" },
            { 54, "JobObjGUID2" },
            { 55, "CatalogPopupID" },
            { 56, "IgnoreCurrentModelIndexInIcons" },
            { 57, "LevelOffset" },
            { 58, "HasShadow" },
            { 59, "NumAttributes" },
            { 60, "NumObjArrays" },
            { 61, "unused" },
            { 62, "FrontDirection" },
            { 63, "unused" },
            { 64, "MultiTileLeadObj" },
            { 65, "ExpansionFlag" },
            { 66, "unused" },
            { 67, "ChairEntryFlags" },
            { 68, "TileWidth" },
            { 69, "InhibitSuitCopying" },
            { 70, "BuildModeType" },
            { 71, "BaseGUID1" },
            { 72, "BaseGUID2" },
            { 73, "ObjModelGUID1" },
            { 74, "ObjModelGUID2" },
            { 75, "BuildModeSubsort" },
            { 76, "unused" },
            { 77, "unused" },
            { 78, "FootprintMask" },
            { 79, "unused" },
            { 80, "unused" },
            { 81, "unused" },
            { 82, "unused" },
            { 83, "HungerRating" },
            { 84, "ComfortRating" },
            { 85, "HygieneRating" },
            { 86, "BladderRating" },
            { 87, "EnergyRating" },
            { 88, "FunRating" },
            { 89, "RoomRating" },
            { 90, "SkillFlags" },
            { 91, "NumTypeAttributes" },
            { 92, "MiscFlags" },
            { 93, "unused" },
            { 94, "unused" },
            { 95, "FunctionPointerSubSortAndSubtype" },
            { 96, "DowntownSort" },
            { 97, "KeepBuying" },
            { 98, "VacationSort" },
            { 99, "ResetLotAction" },
            { 100, "ObjType3d" },
            { 101, "CommunitySort" },
            { 102, "DreamFlags" }
        };
    }
}
