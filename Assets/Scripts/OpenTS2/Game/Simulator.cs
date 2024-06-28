using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.SimAntics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Game
{
    public class Simulator : MonoBehaviour
    {
        public static Simulator Instance { get; private set; }
        public VM VirtualMachine => _virtualMachine;
        private VM _virtualMachine;
        public Context SimulationContext = Context.Neighborhood;
        public enum Context
        {
            Lot = 1,
            Neighborhood = 2
        }
        /// <summary>
        /// Number of ticks to run per second.
        /// </summary>
        public int TickRate = 20;

        private void Awake()
        {
            Instance = this;
            _virtualMachine = new VM();
        }

        private void Start()
        {
            CreateGlobalObjects();
        }

        private void CreateGlobalObjects()
        {
            var objects = ObjectManager.Instance.Objects;

            foreach(var obj in objects)
            {
                if ((obj.GlobalSimObject & (int)SimulationContext) == (int)SimulationContext)
                {
                    CreateObject(obj);
                }
            }
        }

        public VMEntity CreateObject(ObjectDefinitionAsset objectDefinition)
        {
            var entity = new VMEntity(objectDefinition);
            try
            {
                _virtualMachine.AddEntity(entity);

                UpdateObjectData(entity);

                var initFunction = entity.ObjectDefinition.Functions.GetFunction(ObjectFunctionsAsset.FunctionNames.Init);

                if (initFunction.ActionTree != 0)
                    entity.RunTreeImmediately(initFunction.ActionTree);
            }
            catch(SimAnticsException e)
            {
                HandleSimAnticsException(e);
            }
            return entity;
        }

        void UpdateObjectData(VMEntity entity)
        {
            entity.SetObjectData(VMObjectData.Room, -1);
            entity.SetObjectData(VMObjectData.ObjectID, entity.ID);
        }

        public void HandleSimAnticsException(SimAnticsException exception)
        {
            Debug.LogError(exception.ToString());
        }

        public void Kill()
        {
            Destroy(gameObject);
        }

        public static Simulator Create(Context context)
        {
            var gameObject = new GameObject($"{context} Simulation");
            var simulator = gameObject.AddComponent<Simulator>();
            simulator.SimulationContext = context;
            return simulator;
        }
    }
}
