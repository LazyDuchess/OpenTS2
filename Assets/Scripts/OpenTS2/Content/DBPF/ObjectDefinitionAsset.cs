using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content.DBPF
{
    public class ObjectDefinitionAsset : AbstractAsset
    {
        public string FileName;

        public uint GUID
        {
            get { return (uint)(GUID2 << 16) + (uint)GUID1; }
            set
            {
                GUID1 = (ushort)(value & 0xFFFF0000);
                GUID2 = (ushort)(value & 0x0000FFFF);
            }
        }

        public uint ProxyGUID
        {
            get { return (uint)(ProxyGUID2 << 16) + (uint)ProxyGUID1; }
            set
            {
                ProxyGUID1 = (ushort)(value & 0xFFFF0000);
                ProxyGUID2 = (ushort)(value & 0x0000FFFF);
            }
        }

        public uint JobObjectGUID
        {
            get { return (uint)(JobObjGUID2 << 16) + (uint)JobObjGUID1; }
            set
            {
                JobObjGUID1 = (ushort)(value & 0xFFFF0000);
                JobObjGUID2 = (ushort)(value & 0x0000FFFF);
            }
        }

        public uint BaseGUID
        {
            get { return (uint)(BaseGUID2 << 16) + (uint)BaseGUID1; }
            set
            {
                BaseGUID1 = (ushort)(value & 0xFFFF0000);
                BaseGUID2 = (ushort)(value & 0x0000FFFF);
            }
        }

        public uint ObjectModelGUID
        {
            get { return (uint)(ObjModelGUID2 << 16) + (uint)ObjModelGUID1; }
            set
            {
                ObjModelGUID1 = (ushort)(value & 0xFFFF0000);
                ObjModelGUID2 = (ushort)(value & 0x0000FFFF);
            }
        }

        public ushort GameVer1 { get; set; }
        public ushort GameVer2 { get; set; }
        public ushort InitialStackSize { get; set; }
        public ushort DefaultWallAdjacentFlags { get; set; }
        public ushort DefaultPlacementFlags { get; set; }
        public ushort DefaultWallPlacementFlags { get; set; }
        public ushort DefaultAllowedHeightFlags { get; set; }
        public ushort InteractionTableIDPointer { get; set; }
        public ushort InteractionGroup { get; set; }
        public ushort ObjType { get; set; }
        public ushort MultiTileMasterID { get; set; }
        public ushort MultiTileSubIndex { get; set; }
        public ushort UseDefaultPlacementFlags { get; set; }
        public ushort LookAtScore { get; set; }
        public ushort GUID1 { get; set; }
        public ushort GUID2 { get; set; }
        public ushort ItemIsUnlockable { get; set; }
        public ushort CatalogUseFlags { get; set; }
        public ushort BuyPrice { get; set; }
        public ushort BodyStringsIDPointer { get; set; }
        public ushort SlotIDPointer { get; set; }
        public ushort DiagonalSelectorGUID1 { get; set; }
        public ushort DiagonalSelectorGUID2 { get; set; }
        public ushort GridAlignedSelectorGUID1 { get; set; }
        public ushort GridAlignedSelectorGUID2 { get; set; }
        public ushort ObjOwnershipFlags { get; set; }
        public ushort IgnoreGlobalSimField { get; set; }
        public ushort CannotMoveOutWith { get; set; }
        public ushort Hauntable { get; set; }
        public ushort ProxyGUID1 { get; set; }
        public ushort ProxyGUID2 { get; set; }
        public ushort SlotGroup { get; set; }
        public ushort AspirationFlags { get; set; }
        public ushort MemorySubjectiveFeeling { get; set; }
        public ushort SalePrice { get; set; }
        public ushort InitialDepreciation { get; set; }
        public ushort DailyDepreciation { get; set; }
        public ushort SelfDepreciating { get; set; }
        public ushort Depreciationlimit { get; set; }
        public ushort RoomSortflags { get; set; }
        public ushort FunctionSortFlags { get; set; }
        public ushort CatalogStringsIDPointer { get; set; }
        public ushort IsGlobalSimObj { get; set; }
        public ushort ToolTipNameType { get; set; }
        public ushort TemplateVer { get; set; }
        public ushort NicenessMultiplier { get; set; }
        public ushort NoDuplicateOnPlacement { get; set; }
        public ushort WantCategory { get; set; }
        public ushort NoNewNameFromTemplate { get; set; }
        public ushort ObjVer { get; set; }
        public ushort DefaultThumbnailID { get; set; }
        public ushort MotiveEffectsID { get; set; }
        public ushort JobObjGUID1 { get; set; }
        public ushort JobObjGUID2 { get; set; }
        public ushort CatalogPopupID { get; set; }
        public ushort IgnoreCurrentModelIndexInIcons { get; set; }
        public ushort LevelOffset { get; set; }
        public ushort HasShadow { get; set; }
        public ushort NumAttributes { get; set; }
        public ushort NumObjArrays { get; set; }
        public ushort FrontDirection { get; set; }
        public ushort MultiTileLeadObj { get; set; }
        public ushort ExpansionFlag { get; set; }
        public ushort ChairEntryFlags { get; set; }
        public ushort TileWidth { get; set; }
        public ushort InhibitSuitCopying { get; set; }
        public ushort BuildModeType { get; set; }
        public ushort BaseGUID1 { get; set; }
        public ushort BaseGUID2 { get; set; }
        public ushort ObjModelGUID1 { get; set; }
        public ushort ObjModelGUID2 { get; set; }
        public ushort BuildModeSubsort { get; set; }
        public ushort FootprintMask { get; set; }
        public ushort HungerRating { get; set; }
        public ushort ComfortRating { get; set; }
        public ushort HygieneRating { get; set; }
        public ushort BladderRating { get; set; }
        public ushort EnergyRating { get; set; }
        public ushort FunRating { get; set; }
        public ushort RoomRating { get; set; }
        public ushort SkillFlags { get; set; }
        public ushort NumTypeAttributes { get; set; }
        public ushort MiscFlags { get; set; }
        public ushort FunctionPointerSubSortAndSubtype { get; set; }
        public ushort DowntownSort { get; set; }
        public ushort KeepBuying { get; set; }
        public ushort VacationSort { get; set; }
        public ushort ResetLotAction { get; set; }
        public ushort ObjType3d { get; set; }
        public ushort CommunitySort { get; set; }
        public ushort DreamFlags { get; set; }
    }
}
