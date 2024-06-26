using OpenTS2.Common;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content.DBPF
{
    public class ObjectDefinitionAsset : AbstractAsset
    {
        // TODO: Person and Template object types seem to use 0x80 as the SG instance id for some reason.
        public SemiGlobalAsset SemiGlobal => ContentManager.Instance.GetAsset<SemiGlobalAsset>(new ResourceKey(1, GlobalTGI.GroupID, TypeIDs.SEMIGLOBAL));
        public ObjectFunctionsAsset Functions => ContentManager.Instance.GetAsset<ObjectFunctionsAsset>(new ResourceKey(GlobalTGI.InstanceID, GlobalTGI.GroupID, TypeIDs.OBJF));
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
            GlobalSimObject,
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
            ShadowType,
            NumAttributes,
            NumObjArrays,
            ForSaleFlags,
            FrontDirection,
            unused2,
            MultiTileLeadObject,
            ValidEPFlags1,
            ValidEPFlags2,
            ChairEntryFlags,
            TileWidth,
            InhibitSuitCopying,
            BuildModeType,
            BaseGUID1,
            BaseGUID2,
            ObjModelGUID1,
            ObjModelGUID2,
            BuildModeSubsort,
            SelectorCategory,
            SelectorSubCategory,
            FootprintMask,
            ExtendFootprint,
            ObjectSize,
            unused8,
            unused9,
            RatingHunger,
            RatingComfort,
            RatingHygiene,
            RatingBladder,
            RatingEnergy,
            RatingFun,
            RatingRoom,
            RatingSkillFlags,
            NumTypeAttributes,
            MiscFlags,
            unused10,
            unused11,
            FunctionSubSort,
            DowntownSort,
            KeepBuying,
            VacationSort,
            ResetLotAction,
            ObjType3D,
            CommunitySort,
            DreamFlags,
            ThumbnailFlags,
            Unused103,
            Unused104,
            Unused105,
            Unused106,
            Unused107,
        }

        // Makes fields a lot easier to serialize and deserialize, and also to set and get from the VM.
        public ushort[] Fields = new ushort[108];

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
        public ushort GlobalSimObject { get { return this[FieldNames.GlobalSimObject]; } set { this[FieldNames.GlobalSimObject] = value; } }
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
        public ushort ShadowType { get { return this[FieldNames.ShadowType]; } set { this[FieldNames.ShadowType] = value; } }
        public ushort NumAttributes { get { return this[FieldNames.NumAttributes]; } set { this[FieldNames.NumAttributes] = value; } }
        public ushort NumObjArrays { get { return this[FieldNames.NumObjArrays]; } set { this[FieldNames.NumObjArrays] = value; } }
        public ushort FrontDirection { get { return this[FieldNames.FrontDirection]; } set { this[FieldNames.FrontDirection] = value; } }
        public ushort MultiTileLeadObject { get { return this[FieldNames.MultiTileLeadObject]; } set { this[FieldNames.MultiTileLeadObject] = value; } }
        public ushort ValidEPFlags1 { get { return this[FieldNames.ValidEPFlags1]; } set { this[FieldNames.ValidEPFlags1] = value; } }
        public ushort ChairEntryFlags { get { return this[FieldNames.ChairEntryFlags]; } set { this[FieldNames.ChairEntryFlags] = value; } }
        public ushort TileWidth { get { return this[FieldNames.TileWidth]; } set { this[FieldNames.TileWidth] = value; } }
        public ushort InhibitSuitCopying { get { return this[FieldNames.InhibitSuitCopying]; } set { this[FieldNames.InhibitSuitCopying] = value; } }
        public ushort BuildModeType { get { return this[FieldNames.BuildModeType]; } set { this[FieldNames.BuildModeType] = value; } }
        public ushort BuildModeSubsort { get { return this[FieldNames.BuildModeSubsort]; } set { this[FieldNames.BuildModeSubsort] = value; } }
        public ushort FootprintMask { get { return this[FieldNames.FootprintMask]; } set { this[FieldNames.FootprintMask] = value; } }
        public ushort RatingHunger { get { return this[FieldNames.RatingHunger]; } set { this[FieldNames.RatingHunger] = value; } }
        public ushort RatingComfort { get { return this[FieldNames.RatingComfort]; } set { this[FieldNames.RatingComfort] = value; } }
        public ushort RatingHygiene { get { return this[FieldNames.RatingHygiene]; } set { this[FieldNames.RatingHygiene] = value; } }
        public ushort RatingBladder { get { return this[FieldNames.RatingBladder]; } set { this[FieldNames.RatingBladder] = value; } }
        public ushort RatingEnergy { get { return this[FieldNames.RatingEnergy]; } set { this[FieldNames.RatingEnergy] = value; } }
        public ushort RatingFun { get { return this[FieldNames.RatingFun]; } set { this[FieldNames.RatingFun] = value; } }
        public ushort RatingRoom { get { return this[FieldNames.RatingRoom]; } set { this[FieldNames.RatingRoom] = value; } }
        public ushort RatingSkillFlags { get { return this[FieldNames.RatingSkillFlags]; } set { this[FieldNames.RatingSkillFlags] = value; } }
        public ushort NumTypeAttributes { get { return this[FieldNames.NumTypeAttributes]; } set { this[FieldNames.NumTypeAttributes] = value; } }
        public ushort MiscFlags { get { return this[FieldNames.MiscFlags]; } set { this[FieldNames.MiscFlags] = value; } }
        public ushort FunctionSubSort { get { return this[FieldNames.FunctionSubSort]; } set { this[FieldNames.FunctionSubSort] = value; } }
        public ushort DowntownSort { get { return this[FieldNames.DowntownSort]; } set { this[FieldNames.DowntownSort] = value; } }
        public ushort KeepBuying { get { return this[FieldNames.KeepBuying]; } set { this[FieldNames.KeepBuying] = value; } }
        public ushort VacationSort { get { return this[FieldNames.VacationSort]; } set { this[FieldNames.VacationSort] = value; } }
        public ushort ResetLotAction { get { return this[FieldNames.ResetLotAction]; } set { this[FieldNames.ResetLotAction] = value; } }
        public ushort ObjType3D { get { return this[FieldNames.ObjType3D]; } set { this[FieldNames.ObjType3D] = value; } }
        public ushort CommunitySort { get { return this[FieldNames.CommunitySort]; } set { this[FieldNames.CommunitySort] = value; } }
        public ushort DreamFlags { get { return this[FieldNames.DreamFlags]; } set { this[FieldNames.DreamFlags] = value; } }
    }
}
