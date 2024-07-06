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
        public bool DeleteEntityOnError = true;
        public enum Context
        {
            Lot = 1,
            Neighborhood = 2
        }
        /// <summary>
        /// Rate of simulator ticking, in seconds.
        /// </summary>
        public float TickRate = 0.05f;
        private float _timer = 0f;

        private void Awake()
        {
            Instance = this;
            _virtualMachine = new VM();
            _virtualMachine.ExceptionHandler += HandleException;
        }

        private void Start()
        {
            CreateGlobalObjects();
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            var timesToTick = Mathf.FloorToInt(_timer / TickRate);
            _virtualMachine.Tick();
            _timer -= timesToTick * TickRate;
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
            _virtualMachine.AddEntity(entity);

            try
            {
                UpdateObjectData(entity);

                var initFunction = entity.ObjectDefinition.Functions.GetFunction(ObjectFunctionsAsset.FunctionNames.Init);

                if (initFunction.ActionTree != 0)
                    entity.RunTreeImmediately(initFunction.ActionTree);

                var mainFunction = entity.ObjectDefinition.Functions.GetFunction(ObjectFunctionsAsset.FunctionNames.Main);

                if (mainFunction.ActionTree != 0) {
                    entity.PushTreeToThread(entity.MainThread, mainFunction.ActionTree);
                }
            }
            catch(Exception e)
            {
                HandleException(e, entity);
            }
            return entity;
        }

        void UpdateObjectData(VMEntity entity)
        {
            entity.SetObjectData(VMObjectData.Room, -1);
            entity.SetObjectData(VMObjectData.ObjectID, entity.ID);
        }

        public void HandleException(Exception exception, VMEntity entity)
        {
            if (exception is SimAnticsException)
            {
                HandleSimAnticsException(exception as SimAnticsException);
                return;
            }
            Debug.LogError($"Non-SimAntics exception caused by entity {entity.ID} - {entity.ObjectDefinition.FileName}\n{exception}");
            if (DeleteEntityOnError)
                entity.Delete();
        }

        public void HandleSimAnticsException(SimAnticsException exception)
        {
            Debug.LogError(exception.ToString());
            if (DeleteEntityOnError)
                exception.StackFrame.Thread.Entity.Delete();
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
