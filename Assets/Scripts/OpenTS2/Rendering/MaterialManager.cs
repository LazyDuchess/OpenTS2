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
    /// <summary>
    /// Manages parsing material definitions.
    /// </summary>
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

            RegisterMaterial<TextureAlphaMaterial>();
            RegisterMaterial<VertexAlphaMaterial>();
            RegisterMaterial<PhongAlphaMaterial>();
            RegisterMaterial<PhongTextureMaterial>();

            RegisterMaterial<SimSkinMaterial>();
            RegisterMaterial<SimStandardMaterial>();

            // Ideally we'd just disable the mesh renderer to save a draw call but Maxis had to make it weird of course.
            // Could maybe have an ApplyToRenderer(MeshRenderer, MaterialDef) function that does just that if the material def is Null instead.
            RegisterMaterial<NullMaterial>();
            // Lot imposter roof shader depends on local space, which gets altered when batching.
            Batching.MarkShadersNoBatching("OpenTS2/LotImposterRoof");
        }

        public static Material Parse(ScenegraphMaterialDefinitionAsset definition)
        {
            if (!s_materials.TryGetValue(definition.MaterialDefinition.Type, out AbstractMaterial material))
                throw new KeyNotFoundException($"Can't find material type {definition.MaterialDefinition.Type} for {definition.MaterialDefinition.MaterialName}");
            return material.Parse(definition);
        }

        public static void RegisterMaterial<T>() where T : AbstractMaterial
        {
            var mat = Activator.CreateInstance(typeof(T)) as AbstractMaterial;
            s_materials[mat.Name] = mat;
        }
    }
}
