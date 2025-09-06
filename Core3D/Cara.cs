using OpenTK.Mathematics;

namespace Proyecto_3D.Core3D
{
    internal class Cara : IDisposable
    {
        private readonly List<Triangulo> _tris = new();
        private Mesh? _mesh;

        public Vector4 Color = new(1,1,1,1);
        public bool   DrawEdges= false;
        public float  EdgeLineWidth = 2f;
        public Vector4 EdgeColor = new(0f, 0f, 0f, 1f);
        public IReadOnlyList<Triangulo> Triangulos => _tris;

        public void Add(Triangulo t)
        {
            _tris.Add(t);
            ReconstruirBuffers();
        }

        public void SetColor (Vector4 color)
        {
            Color = color;
        }

        public void AddRange(IEnumerable<Triangulo> ts)
        {
            _tris.AddRange(ts);
            ReconstruirBuffers();
        }

        private void ReconstruirBuffers()
        {
            _mesh?.Dispose();
            _mesh = null;

            if (_tris.Count == 0) return;

            var positions = new List<float>(_tris.Count * 9);
            var triIndices = new List<uint>(_tris.Count * 3);
            var edgeSet = new HashSet<(uint, uint)>(); // Para almacenar aristas únicas
            uint baseIndex = 0;

            foreach (var t in _tris)
            {
                // posiciones
                positions.Add(t.A.X); positions.Add(t.A.Y); positions.Add(t.A.Z);
                positions.Add(t.B.X); positions.Add(t.B.Y); positions.Add(t.B.Z);
                positions.Add(t.C.X); positions.Add(t.C.Y); positions.Add(t.C.Z);

                // triángulos
                triIndices.Add(baseIndex + 0);
                triIndices.Add(baseIndex + 1);
                triIndices.Add(baseIndex + 2);

                // aristas
                AddEdge(edgeSet, baseIndex + 0, baseIndex + 1);
                AddEdge(edgeSet, baseIndex + 1, baseIndex + 2);
                AddEdge(edgeSet, baseIndex + 2, baseIndex + 0);

                baseIndex += 3;
            }

            // Convertir el conjunto de aristas únicas a una lista de índices
            var edgeIdx = new List<uint>(edgeSet.Count * 2);
            foreach (var (start, end) in edgeSet)
            {
                edgeIdx.Add(start);
                edgeIdx.Add(end);
            }

            _mesh = new Mesh(positions.ToArray(), triIndices.ToArray(), edgeIdx.ToArray());
        }

        private void AddEdge(HashSet<(uint, uint)> edgeSet, uint start, uint end)
        {
            var edge = (start < end) ? (start, end) : (end, start);
            if (!edgeSet.Contains(edge))
            {
                edgeSet.Add(edge);
            }
        }

        public void Draw(Shader shader, Matrix4 model)
        {
            if (_mesh == null) return;

            shader.SetMatrix4("model", model);
            shader.SetVector4("uColor", Color);
            _mesh.DrawTriangles();

            if (DrawEdges)
            {
                shader.SetVector4("uColor", EdgeColor);
                _mesh.DrawLines(EdgeLineWidth);
            }
        }

        public Vector3 CalcularCentroMasa()
        {
            if (_tris.Count == 0) return Vector3.Zero;
            Vector3 sum = Vector3.Zero;
            foreach (var t in _tris) sum += t.Centro();
            return sum / _tris.Count;
        }

        public void Dispose()
        {
            _mesh?.Dispose();
            _mesh = null;
        }
    }
}
