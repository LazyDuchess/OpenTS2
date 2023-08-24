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

        public enum FieldNames
        {
            GameVer1,
            GameVer2,
            InitialStackSize,
            DefaultWallAdjacentFlags,
            DefaultPlacementFlags,
            DefaultWallPlacementFlags,
            DefaultAllowedHeightFlags,
            InteractionTableIDPointer,
            InteractionGroup,
            ObjType,
            MultiTileMasterID,
            MultiTileSubIndex,
            UseDefaultPlacementFlags,
            LookAtScore,
            GUID1,
            GUID2,
            ItemIsUnlockable,
            CatalogUseFlags,
            BuyPrice,
            BodyStringsIDPointer,
            SlotIDPointer,
            DiagonalSelectorGUID1,
            DiagonalSelectorGUID2,
            GridAlignedSelectorGUID1,
            GridAlignedSelectorGUID2,
            ObjOwnershipFlags,
            IgnoreGlobalSimField,
            CannotMoveOutWith,
            Hauntable,
            ProxyGUID1,
            ProxyGUID2,
            SlotGroup,
            AspirationFlags,
            MemorySubjectiveFeeling,
            SalePrice,
            InitialDepreciation,
            DailyDepreciation,
            SelfDepreciating,
            Depreciationlimit,
            RoomSortflags,
            FunctionSortFlags,
            CatalogStringsIDPointer,
            IsGlobalSimObj,
            ToolTipNameType,
            TemplateVer,
            NicenessMultiplier,
            NoDuplicateOnPlacement,
            WantCategory,
            NoNewNameFromTemplate,
            ObjVer,
            DefaultThumbnailID,
            MotiveEffectsID,
            JobObjGUID1,
            JobObjGUID2,
            CatalogPopupID,
            IgnoreCurrentModelIndexInIcons,
            LevelOffset,
            HasShadow,
            NumAttributes,
            NumObjArrays,
            unused1,
            FrontDirection,
            unused2,
            MultiTileLeadObj,
            ExpansionFlag,
            unused3,
            ChairEntryFlags,
            TileWidth,
            InhibitSuitCopying,
            BuildModeType,
            BaseGUID1,
            BaseGUID2,
            ObjModelGUID1,
            ObjModelGUID2,
            BuildModeSubsort,
            unused4,
            unused5,
            FootprintMask,
            unused6,
            unused7,
            unused8,
            unused9,
            HungerRating,
            ComfortRating,
            HygieneRating,
            BladderRating,
            EnergyRating,
            FunRating,
            RoomRating,
            SkillFlags,
            NumTypeAttributes,
            MiscFlags,
            unused10,
            unused11,
            FunctionPointerSubSortAndSubtype,
            DowntownSort,
            KeepBuying,
            VacationSort,
            ResetLotAction,
            ObjType3d,
            CommunitySort,
            DreamFlags,
            /// <summary>
            /// Just indicates the number of fields in the OBJD field array. Don't use for get/set.
            /// </summary>
            FIELD_COUNT
        }

        // Makes fields a lot easier to serialize and deserialize, and also to set and get from the VM.
        public ushort[] Fields = new ushort[(int)FieldNames.FIELD_COUNT];

        private ushort this[FieldNames fieldName]
        {
            get => Fields[(int)fieldName];
            set => Fields[(int)fieldName] = value;
        }

        // Below we make the definition data into properties to make it more easy to access from C# code. Some of these are returning ushort as placeholder but could be swapped with a more appropriate type if you think it's better.

        public uint GUID
        {
            get { return this[FieldNames.GUID1] + (uint)(this[FieldNames.GUID2] << 16); }
            set
            {
                this[FieldNames.GUID1] = (ushort)(value & 0xFFFF);
                this[FieldNames.GUID2] = (ushort)(value >> 16);
            }
        }

        public uint DiagonalSelectorGUID
        {
            get { return this[FieldNames.DiagonalSelectorGUID1] + (uint)(this[FieldNames.DiagonalSelectorGUID2] << 16); }
            set
            {
                this[FieldNames.DiagonalSelectorGUID1] = (ushort)(value & 0xFFFF);
                this[FieldNames.DiagonalSelectorGUID2] = (ushort)(value >> 16);
            }
        }

        public uint GridAlignedSelectorGUID
        {
            get { return this[FieldNames.GridAlignedSelectorGUID1] + (uint)(this[FieldNames.GridAlignedSelectorGUID2] << 16); }
            set
            {
                this[FieldNames.GridAlignedSelectorGUID1] = (ushort)(value & 0xFFFF);
                this[FieldNames.GridAlignedSelectorGUID2] = (ushort)(value >> 16);
            }
        }

        public uint ProxyGUID
        {
            get { return this[FieldNames.ProxyGUID1] + (uint)(this[FieldNames.ProxyGUID2] << 16); }
            set
            {
                this[FieldNames.ProxyGUID1] = (ushort)(value & 0xFFFF);
                this[FieldNames.ProxyGUID2] = (ushort)(value >> 16);
            }
        }

        public uint JobObjectGUID
        {
            get { return this[FieldNames.JobObjGUID1] + (uint)(this[FieldNames.JobObjGUID2] << 16); }
            set
            {
                this[FieldNames.JobObjGUID1] = (ushort)(value & 0xFFFF);
                this[FieldNames.JobObjGUID2] = (ushort)(value >> 16);
            }
        }

        public uint BaseGUID
        {
            get { return this[FieldNames.BaseGUID1] + (uint)(this[FieldNames.BaseGUID2] << 16); }
            set
            {
                this[FieldNames.BaseGUID1] = (ushort)(value & 0xFFFF);
                this[FieldNames.BaseGUID2] = (ushort)(value >> 16);
            }
        }

        public uint ObjectModelGUID
        {
            get { return this[FieldNames.ObjModelGUID1] + (uint)(this[FieldNames.ObjModelGUID2] << 16); }
            set
            {
                this[FieldNames.ObjModelGUID1] = (ushort)(value & 0xFFFF);
                this[FieldNames.ObjModelGUID2] = (ushort)(value >> 16);
            }
        }

        public ushort GameVer1 { get { return this[FieldNames.GameVer1]; } set { this[FieldNames.GameVer1] = value; } }
        public ushort GameVer2 { get { return this[FieldNames.GameVer2]; } set { this[FieldNames.GameVer2] = value; } }
        public ushort InitialStackSize { get { return this[FieldNames.InitialStackSize]; } set { this[FieldNames.InitialStackSize] = value; } }
        public ushort DefaultWallAdjacentFlags { get { return this[FieldNames.DefaultWallAdjacentFlags]; } set { this[FieldNames.DefaultWallAdjacentFlags] = value; } }
        public ushort DefaultPlacementFlags { get { return this[FieldNames.DefaultPlacementFlags]; } set { this[FieldNames.DefaultPlacementFlags] = value; } }
        public ushort DefaultWallPlacementFlags { get { return this[FieldNames.DefaultWallPlacementFlags]; } set { this[FieldNames.DefaultWallPlacementFlags] = value; } }
        public ushort DefaultAllowedHeightFlags { get { return this[FieldNames.DefaultAllowedHeightFlags]; } set { this[FieldNames.DefaultAllowedHeightFlags] = value; } }
        public ushort InteractionTableIDPointer { get { return this[FieldNames.InteractionTableIDPointer]; } set { this[FieldNames.InteractionTableIDPointer] = value; } }
        public ushort InteractionGroup { get { return this[FieldNames.InteractionGroup]; } set { this[FieldNames.InteractionGroup] = value; } }
        public ushort ObjType { get { return this[FieldNames.ObjType]; } set { this[FieldNames.ObjType] = value; } }
        public ushort MultiTileMasterID { get { return this[FieldNames.MultiTileMasterID]; } set { this[FieldNames.MultiTileMasterID] = value; } }
        public ushort MultiTileSubIndex { get { return this[FieldNames.MultiTileSubIndex]; } set { this[FieldNames.MultiTileSubIndex] = value; } }
        public ushort UseDefaultPlacementFlags { get { return this[FieldNames.UseDefaultPlacementFlags]; } set { this[FieldNames.UseDefaultPlacementFlags] = value; } }
        public ushort LookAtScore { get { return this[FieldNames.LookAtScore]; } set { this[FieldNames.LookAtScore] = value; } }
        public ushort ItemIsUnlockable { get { return this[FieldNames.ItemIsUnlockable]; } set { this[FieldNames.ItemIsUnlockable] = value; } }
        public ushort CatalogUseFlags { get { return this[FieldNames.CatalogUseFlags]; } set { this[FieldNames.CatalogUseFlags] = value; } }
        public ushort BuyPrice { get { return this[FieldNames.BuyPrice]; } set { this[FieldNames.BuyPrice] = value; } }
        public ushort BodyStringsIDPointer { get { return this[FieldNames.BodyStringsIDPointer]; } set { this[FieldNames.BodyStringsIDPointer] = value; } }
        public ushort SlotIDPointer { get { return this[FieldNames.SlotIDPointer]; } set { this[FieldNames.SlotIDPointer] = value; } }
        public ushort ObjOwnershipFlags { get { return this[FieldNames.ObjOwnershipFlags]; } set { this[FieldNames.ObjOwnershipFlags] = value; } }
        public ushort IgnoreGlobalSimField { get { return this[FieldNames.IgnoreGlobalSimField]; } set { this[FieldNames.IgnoreGlobalSimField] = value; } }
        public ushort CannotMoveOutWith { get { return this[FieldNames.CannotMoveOutWith]; } set { this[FieldNames.CannotMoveOutWith] = value; } }
        public ushort Hauntable { get { return this[FieldNames.Hauntable]; } set { this[FieldNames.Hauntable] = value; } }
        public ushort SlotGroup { get { return this[FieldNames.SlotGroup]; } set { this[FieldNames.SlotGroup] = value; } }
        public ushort AspirationFlags { get { return this[FieldNames.AspirationFlags]; } set { this[FieldNames.AspirationFlags] = value; } }
        public ushort MemorySubjectiveFeeling { get { return this[FieldNames.MemorySubjectiveFeeling]; } set { this[FieldNames.MemorySubjectiveFeeling] = value; } }
        public ushort SalePrice { get { return this[FieldNames.SalePrice]; } set { this[FieldNames.SalePrice] = value; } }
        public ushort InitialDepreciation { get { return this[FieldNames.InitialDepreciation]; } set { this[FieldNames.InitialDepreciation] = value; } }
        public ushort DailyDepreciation { get { return this[FieldNames.DailyDepreciation]; } set { this[FieldNames.DailyDepreciation] = value; } }
        public ushort SelfDepreciating { get { return this[FieldNames.SelfDepreciating]; } set { this[FieldNames.SelfDepreciating] = value; } }
        public ushort Depreciationlimit { get { return this[FieldNames.Depreciationlimit]; } set { this[FieldNames.Depreciationlimit] = value; } }
        public ushort RoomSortflags { get { return this[FieldNames.RoomSortflags]; } set { this[FieldNames.RoomSortflags] = value; } }
        public ushort FunctionSortFlags { get { return this[FieldNames.AspirationFlags]; } set { this[FieldNames.FunctionSortFlags] = value; } }
        public ushort CatalogStringsIDPointer { get { return this[FieldNames.CatalogStringsIDPointer]; } set { this[FieldNames.CatalogStringsIDPointer] = value; } }
        public ushort IsGlobalSimObj { get { return this[FieldNames.IsGlobalSimObj]; } set { this[FieldNames.IsGlobalSimObj] = value; } }
        public ushort ToolTipNameType { get { return this[FieldNames.ToolTipNameType]; } set { this[FieldNames.ToolTipNameType] = value; } }
        public ushort TemplateVer { get { return this[FieldNames.TemplateVer]; } set { this[FieldNames.TemplateVer] = value; } }
        public ushort NicenessMultiplier { get { return this[FieldNames.NicenessMultiplier]; } set { this[FieldNames.NicenessMultiplier] = value; } }
        public ushort NoDuplicateOnPlacement { get { return this[FieldNames.NoDuplicateOnPlacement]; } set { this[FieldNames.NoDuplicateOnPlacement] = value; } }
        public ushort WantCategory { get { return this[FieldNames.WantCategory]; } set { this[FieldNames.WantCategory] = value; } }
        public ushort NoNewNameFromTemplate { get { return this[FieldNames.NoNewNameFromTemplate]; } set { this[FieldNames.NoNewNameFromTemplate] = value; } }
        public ushort ObjVer { get { return this[FieldNames.ObjVer]; } set { this[FieldNames.ObjVer] = value; } }
        public ushort DefaultThumbnailID { get { return this[FieldNames.DefaultThumbnailID]; } set { this[FieldNames.DefaultThumbnailID] = value; } }
        public ushort MotiveEffectsID { get { return this[FieldNames.MotiveEffectsID]; } set { this[FieldNames.MotiveEffectsID] = value; } }
        public ushort CatalogPopupID { get { return this[FieldNames.CatalogPopupID]; } set { this[FieldNames.CatalogPopupID] = value; } }
        public ushort IgnoreCurrentModelIndexInIcons { get { return this[FieldNames.IgnoreCurrentModelIndexInIcons]; } set { this[FieldNames.IgnoreCurrentModelIndexInIcons] = value; } }
        public ushort LevelOffset { get { return this[FieldNames.LevelOffset]; } set { this[FieldNames.LevelOffset] = value; } }
        public ushort HasShadow { get { return this[FieldNames.HasShadow]; } set { this[FieldNames.HasShadow] = value; } }
        public ushort NumAttributes { get { return this[FieldNames.NumAttributes]; } set { this[FieldNames.NumAttributes] = value; } }
        public ushort NumObjArrays { get { return this[FieldNames.NumObjArrays]; } set { this[FieldNames.NumObjArrays] = value; } }
        public ushort FrontDirection { get { return this[FieldNames.FrontDirection]; } set { this[FieldNames.FrontDirection] = value; } }
        public ushort MultiTileLeadObj { get { return this[FieldNames.MultiTileLeadObj]; } set { this[FieldNames.MultiTileLeadObj] = value; } }
        public ushort ExpansionFlag { get { return this[FieldNames.ExpansionFlag]; } set { this[FieldNames.ExpansionFlag] = value; } }
        public ushort ChairEntryFlags { get { return this[FieldNames.ChairEntryFlags]; } set { this[FieldNames.ChairEntryFlags] = value; } }
        public ushort TileWidth { get { return this[FieldNames.TileWidth]; } set { this[FieldNames.TileWidth] = value; } }
        public ushort InhibitSuitCopying { get { return this[FieldNames.InhibitSuitCopying]; } set { this[FieldNames.InhibitSuitCopying] = value; } }
        public ushort BuildModeType { get { return this[FieldNames.BuildModeType]; } set { this[FieldNames.BuildModeType] = value; } }
        public ushort BuildModeSubsort { get { return this[FieldNames.BuildModeSubsort]; } set { this[FieldNames.BuildModeSubsort] = value; } }
        public ushort FootprintMask { get { return this[FieldNames.FootprintMask]; } set { this[FieldNames.FootprintMask] = value; } }
        public ushort HungerRating { get { return this[FieldNames.HungerRating]; } set { this[FieldNames.HungerRating] = value; } }
        public ushort ComfortRating { get { return this[FieldNames.ComfortRating]; } set { this[FieldNames.ComfortRating] = value; } }
        public ushort HygieneRating { get { return this[FieldNames.HygieneRating]; } set { this[FieldNames.HygieneRating] = value; } }
        public ushort BladderRating { get { return this[FieldNames.BladderRating]; } set { this[FieldNames.BladderRating] = value; } }
        public ushort EnergyRating { get { return this[FieldNames.EnergyRating]; } set { this[FieldNames.EnergyRating] = value; } }
        public ushort FunRating { get { return this[FieldNames.FunRating]; } set { this[FieldNames.FunRating] = value; } }
        public ushort RoomRating { get { return this[FieldNames.RoomRating]; } set { this[FieldNames.RoomRating] = value; } }
        public ushort SkillFlags { get { return this[FieldNames.SkillFlags]; } set { this[FieldNames.SkillFlags] = value; } }
        public ushort NumTypeAttributes { get { return this[FieldNames.NumTypeAttributes]; } set { this[FieldNames.NumTypeAttributes] = value; } }
        public ushort MiscFlags { get { return this[FieldNames.MiscFlags]; } set { this[FieldNames.MiscFlags] = value; } }
        public ushort FunctionPointerSubSortAndSubtype { get { return this[FieldNames.FunctionPointerSubSortAndSubtype]; } set { this[FieldNames.FunctionPointerSubSortAndSubtype] = value; } }
        public ushort DowntownSort { get { return this[FieldNames.DowntownSort]; } set { this[FieldNames.DowntownSort] = value; } }
        public ushort KeepBuying { get { return this[FieldNames.KeepBuying]; } set { this[FieldNames.KeepBuying] = value; } }
        public ushort VacationSort { get { return this[FieldNames.VacationSort]; } set { this[FieldNames.VacationSort] = value; } }
        public ushort ResetLotAction { get { return this[FieldNames.ResetLotAction]; } set { this[FieldNames.ResetLotAction] = value; } }
        public ushort ObjType3d { get { return this[FieldNames.ObjType3d]; } set { this[FieldNames.ObjType3d] = value; } }
        public ushort CommunitySort { get { return this[FieldNames.CommunitySort]; } set { this[FieldNames.CommunitySort] = value; } }
        public ushort DreamFlags { get { return this[FieldNames.DreamFlags]; } set { this[FieldNames.DreamFlags] = value; } }
    }
}
