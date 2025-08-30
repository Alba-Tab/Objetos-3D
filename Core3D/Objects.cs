using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Proyecto_3D.Core3D
{
    internal class Objects
    {
        private readonly List<Vector3> _verts = new();
        // Lista de caras (en términos de índices al pool)
        private readonly List<Face> _faces = new();

        public enum TipoFigura
        {
            Box, Pyramid, RegularPolygon,ExtrudePolygon,Polygon, 
            Quad, Triangle, Rectanglle, Prisma,
        }

        public struct Figura
        {
            public TipoFigura Type;
            public Vector3 P0, P1, P2, P3;   // uso gcomun
            public Vector3 Size;            // para Box
            // para piramide
            public Vector2 BaseSize;        
            public float Height; //y para prisma           
            // para rectandulo
            public Vector3? UDir, VDir;      
            public float ULen, VLen;
            // para poligono
            public List<Vector3> Verts;
            // poligono regular
            public float Radius;
            public int Sides;

            public Vector3 Dir;
        }

        public static Objects draw(IEnumerable<Figura> Fig)
        {
            var o = new Objects();
            foreach (var s in Fig)
            {
                switch (s.Type)
                {
                    case TipoFigura.Box:
                        o.AddBoxCentered(s.P0, s.Size); // P0 como centro
                        break;
                    case TipoFigura.Pyramid:
                        o.AddPyramid(s.P0, s.BaseSize, s.Height); // P0 como centro base
                        break;
                    case TipoFigura.Triangle:
                        o.AddTriangle(s.P0, s.P1, s.P2);
                        break;
                    case TipoFigura.Quad:
                        o.AddQuad(s.P0, s.P1, s.P2, s.P3);
                        break;
                    case TipoFigura.Rectanglle:
                        o.AddRectangle(s.P0, s.ULen, s.VLen, s.UDir, s.VDir);
                        break;
                    case TipoFigura.RegularPolygon:
                        o.AddRegularPolygon(s.P0, s.Radius, s.Sides, s.UDir, s.VDir);
                        break;

                    case TipoFigura.ExtrudePolygon:
                        o.AddExtrudePolygon(s.Verts, s.Height, s.Dir);
                        break;

                    case TipoFigura.Polygon:
                        o.AddPolygon(s.Verts);
                        break;
                    case TipoFigura.Prisma:
                        o.addPrisma(s.P0, s.Radius, s.Sides, s.Height, s.UDir, s.VDir);
                        break;
                }
            }
            return o;
        }

        public uint AddVertex(Vector3 v)
        {
            _verts.Add(v);
            return (uint)(_verts.Count - 1);
        }

        public Face AddFace(params uint[] indices)
        {
            var f = new Face(indices);
            _faces.Add(f);
            return f;
        }
        // OBJETOS 2D
        public void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            uint indice0 = AddVertex(a);
            uint indice1 = AddVertex(b);
            uint indice2 = AddVertex(c);
            AddFace(indice0, indice1, indice2);
        }

        public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            uint i0 = AddVertex(a);
            uint i1 = AddVertex(b);
            uint i2 = AddVertex(c);
            uint i3 = AddVertex(d);
            AddFace(i0, i1, i2, i3);
        }

        public void AddPolygon(IList<Vector3> pts)
        {
            if (pts == null || pts.Count < 3) return;

            // 1) agrega todos los vértices y guarda sus índices
            var idx = new uint[pts.Count];
            for (int i = 0; i < pts.Count; i++)
                idx[i] = AddVertex(pts[i]);

            AddFace(idx);
        }

        // OBJETOS 3D
        private void AddRegularPolygon(Vector3 center, float radius, int sides, Vector3? uDir=null, Vector3? vDir = null)
        {
            if (sides < 3) return;

            Vector3 u = (uDir ?? Vector3.UnitX).Normalized();
            Vector3 v = (vDir ?? Vector3.UnitZ).Normalized();
            var pts = new List<Vector3>(sides);

            for (int i = 0; i < sides; i++)
            {
                float ang = MathHelper.TwoPi * i / sides;
                Vector3 p = center + radius * (MathF.Cos(ang) * u + MathF.Sin(ang) * v);
                pts.Add(p);
            }

            AddPolygon(pts); 
        }

        private void AddBoxMinMax(Vector3 min, Vector3 max)
        {
            Vector3 v000 = new(min.X, min.Y, min.Z);
            Vector3 v100 = new(max.X, min.Y, min.Z);
            Vector3 v110 = new(max.X, max.Y, min.Z);
            Vector3 v010 = new(min.X, max.Y, min.Z);

            Vector3 v001 = new(min.X, min.Y, max.Z);
            Vector3 v101 = new(max.X, min.Y, max.Z);
            Vector3 v111 = new(max.X, max.Y, max.Z);
            Vector3 v011 = new(min.X, max.Y, max.Z);

            AddQuad(v001, v101, v111, v011); // +Z
            AddQuad(v100, v000, v010, v110); // -Z
            AddQuad(v000, v001, v011, v010); // -X
            AddQuad(v101, v100, v110, v111); // +X
            AddQuad(v000, v100, v101, v001); // -Y
            AddQuad(v010, v011, v111, v110); // +Y
        }

        
        public void AddBoxCentered(Vector3 center, Vector3 size)
        {
            Vector3 h = size * 0.5f;
            AddBoxMinMax(center - h, center + h);
        }

        
        public void AddPyramid(Vector3 center, Vector2 baseSize, float height)
        {
            float hx = baseSize.X * 0.5f;
            float hz = baseSize.Y * 0.5f;

            Vector3 a = center + new Vector3(-hx, 0, -hz);
            Vector3 b = center + new Vector3(+hx, 0, -hz);
            Vector3 c = center + new Vector3(+hx, 0, +hz);
            Vector3 d = center + new Vector3(-hx, 0, +hz);
            Vector3 cima = center + new Vector3(0, height, 0);


            AddQuad(a, b, c, d);
            AddTriangle(a, b, cima);
            AddTriangle(b, c, cima);
            AddTriangle(c, d, cima);
            AddTriangle(d, a, cima);
        }

        public void AddRectangle(Vector3 origin, float uLen, float vLen, Vector3? uDir, Vector3? vDir)
        {

            Vector3 u = (uDir ?? Vector3.UnitX).Normalized() * uLen;
            Vector3 v = (vDir ?? Vector3.UnitZ).Normalized() * vLen;

            Vector3 A = origin;
            Vector3 B = origin + u;
            Vector3 C = origin + u + v;
            Vector3 D = origin + v;

            AddQuad(A, B, C, D);
        }

        public void addPrisma(Vector3 center, float radius, int sides, float height, Vector3? uDir = null, Vector3? vDir = null)
        {
            if (sides < 3) return;

            Vector3 u = (uDir ?? Vector3.UnitX).Normalized();
            Vector3 v = (vDir ?? Vector3.UnitZ).Normalized();
            var pts = new List<Vector3>(sides);

            for (int i = 0; i < sides; i++)
            {
                float ang = MathHelper.TwoPi * i / sides;
                Vector3 p = center + radius * (MathF.Cos(ang) * u + MathF.Sin(ang) * v);
                pts.Add(p);
            }

            AddExtrudePolygon(pts, height);
        }
        public void AddExtrudePolygon(IList<Vector3> pts, float height, Vector3? dirOpt = null)
        {
            if (pts == null || pts.Count < 3) return;

            Vector3 dir = dirOpt?.Normalized() ?? Vector3.UnitY; 
            Vector3 offset = dir * height;

            int n = pts.Count;

            // frontal 
            AddPolygon(pts);

            // Tapa trasera 
            var back = new List<Vector3>(n);
            for (int i = 0; i < n; i++) back.Add(pts[i] + offset);

            back.Reverse();
            AddPolygon(back);

            // Paredes 
            for (int i = 0; i < n; i++)
            {
                var a = pts[i];
                var b = pts[(i + 1) % n];
                var a2 = a + offset;
                var b2 = b + offset;

                AddQuad(a, a2, b2, b);
            }
        }
        public void Transform(Matrix4 m)
        {
            for (int i = 0; i < _verts.Count; i++)
            {
                var v = _verts[i];
                var v4 = new Vector4(v, 1f);
                v4 = Vector4.TransformRow(v4, m);
                _verts[i] = v4.Xyz / (v4.W == 0 ? 1f : v4.W);
            }
        }

        public Mesh ToMesh()
        {

            var verts = new float[_verts.Count * 3];
            for (int i = 0; i < _verts.Count; i++)
            {
                verts[3 * i + 0] = _verts[i].X;
                verts[3 * i + 1] = _verts[i].Y;
                verts[3 * i + 2] = _verts[i].Z;
            }

            // Triangulación de cada cara 
            var tris = new List<uint>();
            foreach (var f in _faces)
            {
                if (f.Indices.Count == 3)
                {
                    tris.AddRange(f.Indices);
                }
                else if (f.Indices.Count == 4)
                {
                    // quad -> dos triángulos (0,1,2) y (2,3,0)
                    tris.Add(f.Indices[0]);
                    tris.Add(f.Indices[1]);
                    tris.Add(f.Indices[2]);

                    tris.Add(f.Indices[2]);
                    tris.Add(f.Indices[3]);
                    tris.Add(f.Indices[0]);
                }
                else if (f.Indices.Count > 4)
                {

                    uint i0 = f.Indices[0];
                    for (int k = 1; k < f.Indices.Count - 1; k++)
                    {
                        tris.Add(i0);
                        tris.Add(f.Indices[k]);
                        tris.Add(f.Indices[k + 1]);
                    }
                }
            }

            // líneas
            var edgeSet = new HashSet<(uint, uint)>();
            foreach (var f in _faces)
            {
                int n = f.Indices.Count;
                for (int i = 0; i < n; i++)
                {
                    uint a = f.Indices[i];
                    uint b = f.Indices[(i + 1) % n];
                    var e = a < b ? (a, b) : (b, a);
                    edgeSet.Add(e);
                }
            }
            var lines = new List<uint>();
            foreach (var (a, b) in edgeSet)
            {
                lines.Add(a); lines.Add(b);
            }

            return new Mesh(verts, tris.ToArray(), lines.ToArray());
        }


        
    }
}
