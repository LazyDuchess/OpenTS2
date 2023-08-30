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
        private static Simulator _instance;

        private void Awake()
        {
            _instance = this;
            _virtualMachine = new VM();
        }

        private void Start()
        {
            CreateGlobalObjects();
        }

        public Simulator Get()
        {
            if (_instance != null)
                return this;
            return null;
        }

        private void CreateGlobalObjects()
        {
            var objManager = ObjectManager.Get();
            if (objManager == null)
                throw new NullReferenceException("Can't create global objects, Object Manager not constructed!");

            var objects = objManager.Objects;

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

        public void HandleSimAnticsException(SimAnticsException exception)
        {
            Debug.LogError(exception.ToString());
        }
    }
}
