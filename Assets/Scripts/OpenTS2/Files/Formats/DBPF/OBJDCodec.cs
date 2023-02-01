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

            foreach (string element in SerialOrder)
            {
                reader.Seek(SeekOrigin.Begin, offset);

                if (element != "unused")
                {
                    asset.GetType().GetProperty(element).SetValue(BoxedAsset, reader.ReadUInt16());
                }

                offset += sizeof(short);
            }

            asset = (ObjectDefinitionAsset)BoxedAsset;

            return asset;
        }

        private readonly string[] SerialOrder =
        {
            "GameVer1",
            "GameVer2",
            "InitialStackSize",
            "DefaultWallAdjacentFlags",
            "DefaultPlacementFlags",
            "DefaultWallPlacementFlags",
            "DefaultAllowedHeightFlags",
            "InteractionTableIDPointer",
            "InteractionGroup",
            "ObjType",
            "MultiTileMasterID",
            "MultiTileSubIndex",
            "UseDefaultPlacementFlags",
            "LookAtScore",
            "GUID1",
            "GUID2",
            "ItemIsUnlockable",
            "CatalogUseFlags",
            "BuyPrice",
            "BodyStringsIDPointer",
            "SlotIDPointer",
            "DiagonalSelectorGUID1",
            "DiagonalSelectorGUID2",
            "GridAlignedSelectorGUID1",
            "DiagonalSelectorGUID2",
            "ObjOwnershipFlags",
            "IgnoreGlobalSimField",
            "CannotMoveOutWith",
            "Hauntable",
            "ProxyGUID1",
            "ProxyGUID2",
            "SlotGroup",
            "AspirationFlags",
            "MemorySubjectiveFeeling",
            "SalePrice",
            "InitialDepreciation",
            "DailyDepreciation",
            "SelfDepreciating",
            "Depreciationlimit",
            "RoomSortflags",
            "FunctionSortFlags",
            "CatalogStringsIDPointer",
            "IsGlobalSimObj",
            "ToolTipNameType",
            "TemplateVer",
            "NicenessMultiplier",
            "NoDuplicateOnPlacement",
            "WantCategory",
            "NoNewNameFromTemplate",
            "ObjVer",
            "DefaultThumbnailID",
            "MotiveEffectsID",
            "JobObjGUID1",
            "JobObjGUID2",
            "CatalogPopupID",
            "IgnoreCurrentModelIndexInIcons",
            "LevelOffset",
            "HasShadow",
            "NumAttributes",
            "NumObjArrays",
            "unused",
            "FrontDirection",
            "unused",
            "MultiTileLeadObj",
            "ExpansionFlag",
            "unused",
            "ChairEntryFlags",
            "TileWidth",
            "InhibitSuitCopying",
            "BuildModeType",
            "BaseGUID1",
            "BaseGUID2",
            "ObjModelGUID1",
            "ObjModelGUID2",
            "BuildModeSubsort",
            "unused",
            "unused",
            "FootprintMask",
            "unused",
            "unused",
            "unused",
            "unused",
            "HungerRating",
            "ComfortRating",
            "HygieneRating",
            "BladderRating",
            "EnergyRating",
            "FunRating",
            "RoomRating",
            "SkillFlags",
            "NumTypeAttributes",
            "MiscFlags",
            "unused",
            "unused",
            "FunctionPointerSubSortAndSubtype",
            "DowntownSort",
            "KeepBuying",
            "VacationSort",
            "ResetLotAction",
            "ObjType3d",
            "CommunitySort",
            "DreamFlags"
        };
    }
}
