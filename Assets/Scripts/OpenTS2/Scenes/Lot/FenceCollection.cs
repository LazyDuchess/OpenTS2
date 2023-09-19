using OpenTS2.Common;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Content.DBPF;
using OpenTS2.Content;
using OpenTS2.Files.Formats.DBPF;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTS2.Scenes.Lot
{
    public class FenceCollection
    {
        private CatalogFenceAsset _asset;
        private GameObject _parent;
        private bool _hasDiag;

        private ScenegraphResourceAsset _railCres;
        private ScenegraphResourceAsset _diagRailCres;
        private ScenegraphResourceAsset _postCres;

        private List<GameObject> _rails;
        private List<GameObject> _diagRails;
        private List<GameObject> _posts;

        private bool _isVisible = true;

        public FenceCollection(ContentProvider contentProvider, GameObject parent, uint guid)
        {
            var catalog = CatalogManager.Get();

            _asset = catalog.GetFenceById(guid);

            if (_asset == null)
            {
                return;
            }

            _hasDiag = _asset.DiagRail != null;
            _parent = parent;

            _railCres = contentProvider.GetAsset<ScenegraphResourceAsset>(
                new ResourceKey(_asset.Rail + "_cres", GroupIDs.Scenegraph,
                    TypeIDs.SCENEGRAPH_CRES));

            _diagRailCres = _hasDiag ? contentProvider.GetAsset<ScenegraphResourceAsset>(
                new ResourceKey(_asset.DiagRail + "_cres", GroupIDs.Scenegraph,
                    TypeIDs.SCENEGRAPH_CRES)) : null;

            _postCres = contentProvider.GetAsset<ScenegraphResourceAsset>(
                new ResourceKey(_asset.Post + "_cres", GroupIDs.Scenegraph,
                    TypeIDs.SCENEGRAPH_CRES));

            _rails = new List<GameObject>();
            _diagRails = new List<GameObject>();
            _posts = new List<GameObject>();
        }

        public void AddRail(float fromX, float fromY, float toX, float toY, float fromElevation, float toElevation)
        {
            float direction = Mathf.Rad2Deg * Mathf.Atan2(toY - fromY, toX - fromX);
            Vector3 shearVec = new Vector3(toX - fromX, toY - fromY, toElevation - fromElevation);
            float magnitude = new Vector2(toX - fromX, toY - fromY).magnitude;
            float shearMagnitude = shearVec.magnitude;

            GameObject model;

            if (magnitude > 1.1 && _diagRailCres != null)
            {
                model = _diagRailCres.CreateRootGameObject();

                model.transform.parent = _parent.transform;

                magnitude /= Mathf.Sqrt(2);

                _diagRails.Add(model);
            }
            else
            {
                if (_railCres != null)
                {
                    model = _railCres.CreateRootGameObject();

                    model.transform.parent = _parent.transform;

                    _rails.Add(model);
                }
                else
                {
                    return;
                }
            }

            Transform modelSpace = model.transform.GetChild(0);
            modelSpace.localScale = new Vector3(magnitude, 1, 1);

            if (shearVec.z != 0)
            {
                // Shear transform
                // Because Unity doesn't support submitting your own transform matrix for some reason,
                // We need to combine a bunch of transformations to perform a shear.
                // In this case, we want the Z dimension to be sheared with x and y left intact.

                float realAngle = Mathf.Atan2(shearVec.z, magnitude);
                float shearAngle = realAngle > 0 ? Mathf.PI / 2 - realAngle : Mathf.PI / -2 - realAngle;

                var top = new GameObject("skew_top").transform;
                top.SetParent(model.transform);
                var mid = new GameObject("skew_mid").transform;
                mid.SetParent(top);
                var bot = new GameObject("skew_bot").transform;
                bot.SetParent(mid);
                modelSpace.SetParent(bot);
                modelSpace.localRotation = Quaternion.identity;

                top.localRotation = Quaternion.Euler(0, -realAngle * Mathf.Rad2Deg, 0);
                mid.localRotation = Quaternion.Euler(0, 45, 0);
                bot.localRotation = Quaternion.Euler(0, (-shearAngle / 2) * Mathf.Rad2Deg, 0);

                float initialScale = shearMagnitude;
                float finalScale = Mathf.Sqrt(2);

                top.localScale = new Vector3(finalScale / Mathf.Sin(shearAngle), 1, finalScale);
                mid.localScale = new Vector3(Mathf.Sin(shearAngle / 2), 1, Mathf.Cos(shearAngle / 2));
                bot.localScale = new Vector3(initialScale, 1, 1 / initialScale);

                modelSpace = top;
            }

            modelSpace.localPosition = new Vector3(fromX, fromY, fromElevation);
            modelSpace.localRotation = Quaternion.Euler(0, 0, direction) * modelSpace.localRotation;
        }

        public void AddPost(float x, float y, float elevation)
        {
            if (_postCres != null)
            {
                var model = _postCres.CreateRootGameObject();

                model.transform.parent = _parent.transform;

                model.transform.GetChild(0).localPosition = new Vector3(x, y, elevation);

                _posts.Add(model);
            }
        }

        public void Clear()
        {
            foreach (GameObject rail in _rails)
            {
                GameObject.Destroy(rail);
            }

            foreach (GameObject rail in _diagRails)
            {
                GameObject.Destroy(rail);
            }

            foreach (GameObject post in _posts)
            {
                GameObject.Destroy(post);
            }

            _rails.Clear();
            _posts.Clear();
        }

        private void SetObjectVisibility(List<GameObject> objects, bool visible)
        {
            foreach (GameObject rail in objects)
            {
                foreach (MeshRenderer renderer in rail.GetComponentsInChildren<MeshRenderer>())
                {
                    renderer.shadowCastingMode = visible ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                }
            }
        }

        public void SetVisible(bool visible)
        {
            if (_isVisible != visible)
            {
                _isVisible = visible;

                SetObjectVisibility(_rails, visible);
                SetObjectVisibility(_diagRails, visible);
                SetObjectVisibility(_posts, visible);
            }
        }
    }
}