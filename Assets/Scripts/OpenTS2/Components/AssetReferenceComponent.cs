using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Scenes.Lot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Components
{
    public class AssetReferenceComponent : MonoBehaviour
    {
        public List<AbstractAsset> References = new List<AbstractAsset>();

        public void AddReference(params AbstractAsset[] assets)
        {
            References.AddRange(assets);
        }

        /// <summary>
        /// Indicates that this component should re-evaluate itself for changes made at runtime
        /// </summary>
        public virtual void Invalidate() { }        
    }
    
    // Seems like functionality is identical between certain lot components that utilize patterns.
    // Moved the duplicated code to one unit so we can make improvements

    //**CURRENTLY UNUSED**
    /// <summary>
    /// Serves functionality for loading pattern assets from a pattern map (floors, walls, etc.)
    /// <para/>
    /// </summary>
    public abstract class AssetPatternReferenceComponent : AssetReferenceComponent
    {
        /// <summary>
        /// The material to use when the requested material could not be loaded using the <see cref="LoadPatterns"/>
        /// default implementation
        /// </summary>
        protected abstract string FallbackMaterial { get; }
        /// <summary>
        /// The texture to use for the thickness of the object
        /// <para/>Walls and floors utilize this to show they're three-dimensional and it applied to <see cref="_patterns"/>[0]
        /// </summary>
        protected abstract string ThicknessTexture { get; }

        protected abstract int FloorCount { get; }

        protected PatternMeshCollection _patterns;
        protected PatternDescriptor[] _loadedPatternDescriptions;

        protected abstract Dictionary<ushort, StringMapEntry> Map { get; }
        protected virtual Dictionary<string, string> Builtins { get; } = null;

        /// <summary>
        /// Base load patterns function
        /// </summary>
        protected virtual void LoadPatterns()
        {
            var contentProvider = ContentProvider.Get();
            var catalogManager = CatalogManager.Get();
            var patternMap = Map;

            ushort highestId = patternMap.Count == 0 ? (ushort)0 : patternMap.Keys.Max();
            PatternDescriptor[] patterns = new PatternDescriptor[highestId + 2];

            bool changesMade = false;
            bool previousStateExisting = _loadedPatternDescriptions != null;

            foreach (StringMapEntry entry in patternMap.Values)
            {
                //Persistent data -- calls from Invalidate() will call this with data already loaded, in which case
                //We can save time by not reloading the same loaded data
                // TODO
                if (previousStateExisting && _loadedPatternDescriptions.Length == patterns.Length &&
                    _loadedPatternDescriptions[entry.Id + 1].Name != null) // loaded
                { // PatternDescriptor is a struct -- check the name see if it's null, if not, the pattern is loaded already
                    patterns[entry.Id + 1] = _loadedPatternDescriptions[entry.Id + 1];
                    continue;
                }
                changesMade = true;

                string materialName = null;
                if (uint.TryParse(entry.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint guid))
                {
                    var catalogEntry = catalogManager.GetEntryById(guid);

                    materialName = catalogEntry?.Material ?? FallbackMaterial;
                }
                else if (Builtins != null && !Builtins.TryGetValue(entry.Value, out materialName))
                {
                    materialName = entry.Value.StartsWith("wall_") ? (entry.Value + "_base") : ("wall_" + entry.Value + "_base");
                }

                if (materialName == null)
                {
                    // Explicitly remove this pattern.
                    continue;
                }

                // Try fetch the texture using the material name.
                var material = LoadMaterial(contentProvider, materialName);

                try
                {
                    // Note: Sometimes walls use the standard material, but we want them to use a special shader for cutaways.
                    patterns[entry.Id + 1] = new PatternDescriptor(
                        materialName,
                        material == null ? null : material?.GetAsUnityMaterial("Wallpaper")
                    );
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            if (previousStateExisting)
            {
                if (!changesMade) return; // no changes to wall patterns since last load 

                patterns[0] = _loadedPatternDescriptions[0];
                _loadedPatternDescriptions = patterns;
                _patterns.UpdatePatterns(patterns);
                return;
            }

            PostLoadPatterns(contentProvider, ref patterns);

            _loadedPatternDescriptions = patterns;
            //_patterns = new PatternMeshCollection(gameObject, patterns, Array.Empty<PatternVariant>(), this, FloorCount);
        }

        protected virtual ScenegraphMaterialDefinitionAsset LoadMaterial(ContentProvider contentProvider, string name)
        {
            var material = contentProvider.GetAsset<ScenegraphMaterialDefinitionAsset>(new ResourceKey($"{name}_txmt", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXMT));

            AddReference(material);

            return material;
        }

        protected virtual void PostLoadPatterns(ContentProvider contentProvider, ref PatternDescriptor[] patterns)
        {

        }
    }
}
