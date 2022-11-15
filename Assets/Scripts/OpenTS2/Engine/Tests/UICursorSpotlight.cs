using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Engine.Tests
{
    public class UICursorSpotlight : MonoBehaviour
    {
        private Mesh _mesh = null;
        public MeshFilter MeshFilter;
        public Canvas Canvas;
        public Vector3 Origin;
        public Vector3 BoxSize;
        public float Thickness = 1f;
        public int Resolution = 8;
        public float Roundness = 0.14f;

        public bool FollowMouse = true;
        public Vector3 MovePosition = Vector3.zero;

        void Update()
        {
            if (FollowMouse)
            {
                RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    Canvas.transform as RectTransform,
                    Input.mousePosition, Canvas.worldCamera,
                    out MovePosition);
                MovePosition = this.transform.InverseTransformPoint(MovePosition);
            }
            MakeMeshNew(MovePosition);
        }

        public void MakeRoundedRectangle(int vertexCount, float edgeRound, Vector3 origin, Vector3 boxSize, ref List<Vector3> verts, ref List<int> indices, ref List<Color> colors)
        {
            if (vertexCount < 2)
                vertexCount = 2;
            int sides = 1;
            int vCount = vertexCount * 4 * sides + sides;
            int triCount = (vertexCount * 4) * sides;
            var m_Vertices = new Vector3[vCount];
            var m_Normals = new Vector3[vCount];
            var m_Triangles = new int[triCount * 3];
            float f = 1f / (vertexCount - 1);
            m_Vertices[0] = Vector3.zero;
            int count = vertexCount * 4;
            for (int i = 0; i < vertexCount; i++)
            {
                float s = Mathf.Sin((float)i * Mathf.PI * 0.5f * f);
                float c = Mathf.Cos((float)i * Mathf.PI * 0.5f * f);
                float tl = Mathf.Clamp01(edgeRound);
                float tr = Mathf.Clamp01(edgeRound);
                float bl = Mathf.Clamp01(edgeRound);
                float br = Mathf.Clamp01(edgeRound);
                Vector2 v1 = new Vector3(-1f + tl - c * tl, 1 - tl + s * tl);
                Vector2 v2 = new Vector3(1f - tr + s * tr, 1f - tr + c * tr);
                Vector2 v3 = new Vector3(1f - br + c * br, -1f + br - s * br);
                Vector2 v4 = new Vector3(-1f + bl - s * bl, -1f + bl - c * bl);

                m_Vertices[1 + i] = v1 * boxSize;
                m_Vertices[1 + vertexCount + i] = v2 * boxSize;
                m_Vertices[1 + vertexCount * 2 + i] = v3 * boxSize;
                m_Vertices[1 + vertexCount * 3 + i] = v4 * boxSize;
            }
            for (int i = 0; i < count + 1; i++)
            {
                m_Normals[i] = -Vector3.forward;
            }

            for (int i = 0; i < count; i++)
            {
                m_Triangles[i * 3] = 0 + verts.Count;
                m_Triangles[i * 3 + 1] = i + 1 + verts.Count;
                m_Triangles[i * 3 + 2] = i + 2 + verts.Count;
            }
            m_Triangles[count * 3 - 1] = 1 + verts.Count;

            verts.AddRange(m_Vertices);
            for(var i=0;i<m_Vertices.Length;i++)
            {
                colors.Add(Color.white);
            }
            indices.AddRange(m_Triangles);
        }

        void MakeMeshNew(Vector3 position)
        {
            position.z = Origin.z;

            if (_mesh == null)
            {
                _mesh = new Mesh();
                _mesh.MarkDynamic();
            }
            var verts = new List<Vector3>();
            var indices = new List<int>();
            var colors = new List<Color>();

            MakeRoundedRectangle(Resolution, Roundness, Origin, BoxSize, ref verts, ref indices, ref colors);
            ExtrudeIntoPos(ref verts, ref colors, ref indices, position);
            ExtrudeOutwards(ref indices, ref verts, ref colors, Thickness);
            
            _mesh.Clear();
            _mesh.vertices = verts.ToArray();
            _mesh.colors = colors.ToArray();
            _mesh.triangles = indices.ToArray();
            MeshFilter.sharedMesh = _mesh;
        }

        void ExtrudeIntoPos(ref List<Vector3> verts, ref List<Color> colors, ref List<int> indices, Vector3 position)
        {
            var originVertIndex = verts.Count;
            var originVert = position;
            verts.Add(originVert);
            colors.Add(Color.white);
            List<Segment> segments = new List<Segment>();
            var vertCounter = 0;
            for (var i = 0; i < indices.Count; i++)
            {
                vertCounter += 1;
                var currentVertCounter = vertCounter;
                var beginIndex = indices[i];
                int insideIndex = 0;
                int endIndex;
                if (vertCounter == 3)
                {
                    endIndex = indices[i - 2];
                    vertCounter = 0;
                }
                else
                {
                    endIndex = indices[i + 1];
                }
                if (currentVertCounter == 1)
                {
                    insideIndex = indices[i + 2];
                }
                else if (currentVertCounter == 2)
                {
                    insideIndex = indices[i - 1];
                }
                else if (currentVertCounter == 3)
                {
                    insideIndex = indices[i - 1];
                }
                var segment = new Segment(beginIndex, endIndex, insideIndex);
                segments.Add(segment);
            }
            var finalSegments = new List<Segment>();
            for (var i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                var dupe = CheckForDuplicate(segment, segments, i);
                if (!dupe)
                {
                    finalSegments.Add(segment);
                }
            }
            foreach(var segment in finalSegments)
            {
                var heading = segment.GetHeading(verts);
                var middle = segment.GetMiddle(verts);
                var cursorHeading = (middle - position).normalized;
                if (Vector3.Dot(heading, cursorHeading) > 0f)
                    continue;
                indices.Add(originVertIndex);
                
                indices.Add(segment.endIndex);
                indices.Add(segment.startIndex);

            }
        }

        class Segment
        {
            public int startIndex;
            public int endIndex;
            public int insideIndex;

            public Vector3 GetMiddle(List<Vector3> verts)
            {
                var middle = (verts[startIndex] + verts[endIndex]) / 2;
                return middle;
            }

            public Vector3 GetHeading(List<Vector3> verts)
            {
                
                var headingForward = (verts[startIndex] - verts[endIndex]).normalized;
                var heading = Vector3.Cross(headingForward, Vector3.forward);
                //if (Vector3.Dot(heading, headingMiddle) <= 0f)
                //    heading = -heading;
                heading.Normalize();
                return heading;
            }

            public Segment(int startIndex, int endIndex, int insideIndex)
            {
                this.startIndex = startIndex;
                this.endIndex = endIndex;
                this.insideIndex = insideIndex;
            }

            public override int GetHashCode()
            {
                return (this.startIndex & this.endIndex & this.insideIndex);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as Segment);
            }

            public bool Equals(Segment segment)
            {
                if (segment.startIndex == startIndex && segment.endIndex == endIndex)
                    return true;
                if (segment.endIndex == startIndex && segment.startIndex == endIndex)
                    return true;
                return false;
            }
        }
        /*
        private Segment FindNeighbor(Segment segment, List<Segment> segments, int index)
        {
            var newSegments = segments.ToList();
            newSegments.RemoveRange(0, index + 1);
            foreach(var element in newSegments)
            {
                if (element.startIndex == segment.startIndex)
                    return element;
                if (element.startIndex == segment.endIndex)
                    return element;
                if (element.endIndex == segment.startIndex)
                    return element;
                if (element.endIndex == segment.endIndex)
                    return element;
            }
            return null;
        }
        
        private Vector3 GetProportionPoint(Vector3 point, double segment,
                                  double length, double dx, double dy)
        {
            double factor = segment / length;

            return new Vector3((float)(point.x - dx * factor),
                              (float)(point.y - dy * factor));
        }
        
        double GetLength(double dx, double dy)
        {
            return Math.Sqrt(dx * dx + dy * dy);
        }
        */
        void ExtrudeOutwards(ref List<int> indices, ref List<Vector3> verts, ref List<Color> colors, float thickness)
        {
            List<Segment> segments = new List<Segment>();
            var vertCounter = 0;
            for(var i=0;i<indices.Count; i++)
            {
                vertCounter += 1;
                var currentVertCounter = vertCounter;
                var beginIndex = indices[i];
                int insideIndex = 0;
                int endIndex;
                if (vertCounter == 3)
                {
                    endIndex = indices[i - 2];
                    vertCounter = 0;
                }
                else
                {
                    endIndex = indices[i + 1];
                }
                if (currentVertCounter == 1)
                {
                    insideIndex = indices[i + 2];
                }
                else if (currentVertCounter == 2)
                {
                    insideIndex = indices[i - 1];
                }
                else if (currentVertCounter == 3)
                {
                    insideIndex = indices[i - 1];
                }
                var segment = new Segment(beginIndex, endIndex, insideIndex);
                //Debug.DrawLine(verts[segment.startIndex], verts[segment.endIndex], Color.red, 1f);
                segments.Add(segment);
            }
            var finalSegments = new List<Segment>();
            for(var i=0;i<segments.Count;i++)
            {
                var segment = segments[i];
                var dupe = CheckForDuplicate(segment, segments, i);
                if (!dupe)
                {
                    finalSegments.Add(segment);
                    //Debug.DrawLine(verts[segment.startIndex], verts[segment.endIndex], Color.green, 0.1f);
                    //Debug.DrawRay(segment.GetMiddle(verts), segment.GetHeading(verts) * 10f, Color.red, 0.1f);
                }
            }
            var extrudedVertices = new Dictionary<int, int>();
            for(var i=0;i<finalSegments.Count;i++)
            {
                var segment = finalSegments[i];

                var startIndex = segment.startIndex;
                var endIndex = segment.endIndex;

                var startVertex = verts[segment.startIndex];
                var endVertex = verts[segment.endIndex];

                var ogStartVertex = startVertex;
                var ogEndVertex = endVertex;

                var heading = segment.GetHeading(verts);

                startVertex += heading * thickness;
                endVertex += heading * thickness;

                var startAlready = false;
                var endAlready = false;

                if (extrudedVertices.ContainsKey(startIndex))
                {
                    var middle = (verts[extrudedVertices[startIndex]] + startVertex) / 2;
                    var headingtoMiddle = (middle - ogStartVertex).normalized;
                    startAlready = true;
                    startVertex = ogStartVertex + (headingtoMiddle * thickness);
                }
                
                if (extrudedVertices.ContainsKey(endIndex))
                {
                    var middle = (verts[extrudedVertices[endIndex]] + endVertex) / 2;
                    var headingtoMiddle = (middle - ogEndVertex).normalized;
                    endAlready = true;
                    endVertex = ogEndVertex + (headingtoMiddle * thickness);
                }

                if (!startAlready)
                {
                    var index = verts.Count;
                    verts.Add(startVertex);
                    colors.Add(new Color(1f,1f,1f,0f));
                    extrudedVertices[startIndex] = index;
                }
                else
                {
                    verts[extrudedVertices[startIndex]] = startVertex;
                }

                if (!endAlready)
                {
                    var index = verts.Count;
                    verts.Add(endVertex);
                    colors.Add(new Color(1f, 1f, 1f, 0f));
                    extrudedVertices[endIndex] = index;
                }
                else
                {
                    verts[extrudedVertices[endIndex]] = endVertex;
                }
            }
            foreach (var segment in finalSegments)
            {
                var startIndex = segment.startIndex;
                var endIndex = segment.endIndex;

                var exStartIndex = extrudedVertices[startIndex];
                var exEndIndex = extrudedVertices[endIndex];

                var startVertex = verts[startIndex];
                var endVertex = verts[endIndex];

                var exStartVertex = verts[exStartIndex];
                var exEndVertex = verts[exEndIndex];

                var orderDistance = Vector3.Distance(exStartVertex, endVertex);
                var order2Distance = Vector3.Distance(exEndVertex, startVertex);

                if (orderDistance < order2Distance)
                {
                    indices.Add(startIndex);
                    indices.Add(exStartIndex);
                    indices.Add(endIndex);

                    indices.Add(exStartIndex);
                    indices.Add(exEndIndex);
                    indices.Add(endIndex);
                }
                else
                {
                    indices.Add(startIndex);
                    indices.Add(exEndIndex);
                    indices.Add(endIndex);

                    indices.Add(exStartIndex);
                    indices.Add(exEndIndex);
                    indices.Add(startIndex);
                }
            }
        }

        bool CheckForDuplicate(Segment segment, List<Segment> segmentList, int index)
        {
            var newList = segmentList.ToList();
            newList.RemoveAt(index);
            if (newList.IndexOf(segment) >= 0)
            {
                return true;
            }
            return false;
        }
    }
}
