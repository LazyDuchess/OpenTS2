using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content.DBPF
{
    public struct ObjectFunction
    {
        public ushort CheckTree;
        public ushort ActionTree;
        public static ObjectFunction Default => _default;
        private static ObjectFunction _default = new ObjectFunction(0, 0);

        public ObjectFunction(ushort actionTree, ushort checkTree)
        {
            ActionTree = actionTree;
            CheckTree = checkTree;
        }
    }
    public class ObjectFunctionsAsset : AbstractAsset
    {
        public string FileName;

        public enum FunctionNames
        {
            Init,
            Main,
            Load,
            Cleanup,
            QueueSkipped,
            AllowIntersection,
            WallAdjacencyChanged,
            RoomChanged,
            DynamicMultiTileUpdate,
            Placement,
            Pickup,
            UserPlacement,
            UserPickup,
            LevelInfoRequest,
            ServingSurface,
            Portal,
            Gardening,
            WashHands,
            Prep,
            Cook,
            Surface,
            Dispose,
            Food,
            PickupFromSlot,
            WashDish,
            EatingSurface,
            Sit,
            Stand,
            Clean,
            Repair,
            UIEvent,
            Restock,
            WashClothes,
            StartLiveMode,
            StopLiveMode,
            LinkObjects,
            MessageHandler,
            PreRoute,
            PostRoute,
            GoalCheck,
            ReactionHandler,
            AlongRouteCallback,
            Awareness,
            Reset,
            LookAtTarget,
            WalkOver,
            UtilityStateChange,
            SetModelByType,
            GetModelType,
            Delete,
            UserDelete,
            JustMovedIn,
            PreventPlaceInSlot,
            GlobalAwareness,
            ObjectUpdatedByDesignMode
        }

        public ObjectFunction[] Functions;

        public bool HasFunction(FunctionNames function)
        {
            if ((int)function >= Functions.Length)
                return false;
            return true;
        }

        public ObjectFunction GetFunction(FunctionNames function)
        {
            if (!HasFunction(function))
                return ObjectFunction.Default;
            return Functions[(int)function];
        }
    }
}
