using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.SimAntics;
using UnityEngine;

namespace OpenTS2.Engine.Tests
{
    /// <summary>
    /// Tests loading an object from a lot and then running the SimAntics simulator on it.
    /// </summary>
    public class LotObjectSimulationTest : MonoBehaviour
    {
        public string NeighborhoodPrefix = "N002";
        public int LotID = 22;
        // Which item to load from the lot save table.
        public int ItemIndex = 0;

        private string _nhood;
        private int _lotId;

        void Start()
        {
            Core.InitializeCore();

            ContentLoading.LoadGameContentSync();

            CatalogManager.Instance.Initialize();
            ObjectManager.Create();

            LoadLot(NeighborhoodPrefix, LotID);
        }

        private void LoadLot(string neighborhoodPrefix, int id)
        {
            _nhood = neighborhoodPrefix;
            _lotId = id;

            var contentManager = ContentManager.Instance;

            var lotsFolderPath = Path.Combine(Filesystem.UserDataDirectory, $"Neighborhoods/{NeighborhoodPrefix}/Lots");
            var lotFilename = $"{NeighborhoodPrefix}_Lot{LotID}.package";
            var lotFullPath = Path.Combine(lotsFolderPath, lotFilename);

            if (!File.Exists(lotFullPath))
            {
                return;
            }

            var lotPackage = contentManager.AddPackage(lotFullPath);
            // Get the lot's OBJT / object save type table.
            var saveTable =
                lotPackage.GetAssetByTGI<ObjectSaveTypeTableAsset>(new ResourceKey(instanceID: 0, GroupIDs.Local,
                    TypeIDs.OBJ_SAVE_TYPE_TABLE));

            var saveTypeToGuid = new Dictionary<int, uint>();
            for (var index = 0; index < saveTable.Selectors.Count; index++)
            {
                var selector = saveTable.Selectors[index];
                var def = ObjectManager.Instance.GetObjectByGUID(selector.objectGuid);
                if (def == null)
                {
                    continue;
                }

                saveTypeToGuid[selector.saveType] = selector.objectGuid;

                Debug.Log($"{index}: saveType: {selector.saveType} resource name: {selector.catalogResourceName}, Obj name: {def.FileName}");
            }

            var objectToLoad = saveTable.Selectors[ItemIndex];
            Debug.Log($"Loading object {objectToLoad.catalogResourceName} with guid {objectToLoad.objectGuid:X}");

            var objectDefinition = ObjectManager.Instance.GetObjectByGUID(objectToLoad.objectGuid);
            Debug.Assert(objectDefinition != null, "Could not find objd.");

            // Now load the state of the object.
            var objectState = lotPackage.GetAssetByTGI<SimsObjectAsset>(
                    new ResourceKey(instanceID: (uint)objectToLoad.saveType, GroupIDs.Local, TypeIDs.XOBJ));

            // Create an entity for the object.
            var vm = new VM();
            var entity = new VMEntity(objectDefinition)
            {
                Attributes = objectState.Attrs,
                SemiAttributes = objectState.SemiAttrs,
                Temps = objectState.Temp,
                ObjectData = objectState.Data
            };

            foreach (var frame in objectState.StackFrames)
            {
                Debug.Log("Frame -----");
                Debug.Log($"  TreeId: 0x{frame.TreeId:X}, bhavSaveType: {frame.BhavSaveType}");

                var bhavObjDef = ObjectManager.Instance.GetObjectByGUID(saveTypeToGuid[frame.BhavSaveType]);
                // TODO: add a static method to do this or something. We don't want to make a VMEntity instance just
                // to get the BHAV.
                var bhav = new VMEntity(bhavObjDef).GetBHAV(frame.TreeId);

                var vmFrame = new VMStackFrame(bhav, entity.MainThread)
                {
                    Arguments = frame.Params,
                    Locals = frame.Locals
                };
                entity.MainThread.Frames.Push(vmFrame);

                Debug.Log($"  BHAV TGI: {entity.MainThread.Frames.Peek().BHAV.GlobalTGI}");
                Debug.Log($"  params: ({string.Join(", ", vmFrame.Arguments)})");
            }

            vm.Tick();
        }
    }
}
