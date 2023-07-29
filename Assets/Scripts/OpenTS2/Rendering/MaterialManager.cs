using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Rendering.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Rendering
{
    public static class MaterialManager
    {
        private static Dictionary<string, AbstractMaterial> s_materials = new Dictionary<string, AbstractMaterial>();
        public static void Initialize()
        {
            RegisterMaterial<StandardMaterial>();
            RegisterMaterial<ImposterTerrainMaterial>();
            RegisterMaterial<ImposterWallMaterial>();
            RegisterMaterial<ImposterDualPackedSliceMaterial>();
            RegisterMaterial<ImposterRoofMaterial>();
        }

        public static Material Parse(ScenegraphMaterialDefinitionAsset definition)
        {
            if (!s_materials.TryGetValue(definition.MaterialDefinition.Type, out AbstractMaterial material))
                material = s_materials["StandardMaterial"];
            return material.Parse(definition);
        }

        public static void RegisterMaterial<T>() where T : AbstractMaterial
        {
            var mat = Activator.CreateInstance(typeof(T)) as AbstractMaterial;
            s_materials[mat.Name] = mat;
        }
    }
}
