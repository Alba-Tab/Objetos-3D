using OpenTK.Mathematics;

namespace Proyecto_3D.Core3D
{
    internal class Triangulo
    {
        public List<Vector3> Vertices { get; set; } = new();
        public Vector3 Centro { get; private set; } = Vector3.Zero;
        public Vector4 Color { get; set; } = new Vector4(1f, 1f, 1f, 1f);

        public Vector3 Rotacion { get; set; } = Vector3.Zero; 
        public Vector3 Posicion { get; set; } = Vector3.Zero; 
        public Vector3 Dimensiones { get; set; } = Vector3.One; 

        private Mesh? _mesh;

        public Triangulo()
        {
        }

        public Triangulo(Vector3 posicion, Vector3 rotacion, Vector3 dimensiones)
        {
            Posicion = posicion;
            Rotacion = rotacion;
            Dimensiones = dimensiones;
        }

        public void AddVertex(Vector3 vertex)
        {
            Vertices.Add(vertex);
            CalcularCentro();
        }

        private void InitializeMesh()
        {
            if (Vertices.Count == 0) return;

            float[] vertexArray = new float[Vertices.Count * 3];
            for (int i = 0; i < Vertices.Count; i++)
            {
                vertexArray[i * 3] = Vertices[i].X;
                vertexArray[i * 3 + 1] = Vertices[i].Y;
                vertexArray[i * 3 + 2] = Vertices[i].Z;
            }

            uint[] triangleIndices = new uint[Vertices.Count];
            for (uint i = 0; i < Vertices.Count; i++)
                triangleIndices[i] = i;

            _mesh = new Mesh(vertexArray, triangleIndices);
        }
        
        public Vector3 CalcularCentro()
        {
            if (Vertices.Count == 0)
            {
                Centro = Vector3.Zero;
                return Vector3.Zero;
            }
            Vector3 sum = Vector3.Zero;
            foreach (var v in Vertices)
                sum += v;
            Centro = sum / Vertices.Count;
            return Centro;
        }
        
        public Vector3 UpdateCentro() => CalcularCentro();
        public void Draw(Shader shader, Matrix4 parent)
        {
            if (Vertices.Count == 0) return;

            if (_mesh == null) InitializeMesh();

            var local  =
                Matrix4.CreateTranslation(Posicion) *
                Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotacion.X)) *
                Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotacion.Y)) *
                Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotacion.Z)) *
                Matrix4.CreateScale(Dimensiones);

            var model = local * parent;
            shader.Use();
            shader.SetMatrix4("model", model);
            shader.SetVector4("uColor", Color);

            _mesh?.DrawTriangles();
        }

        public void Dispose()
        {
            _mesh?.Dispose();
        }
        
    }
}
