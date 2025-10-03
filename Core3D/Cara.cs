// File: Proyecto_3D/Core3D/Cara.cs
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace Proyecto_3D.Core3D
{
    public class Cara : IDisposable
    {
        public string Name { get; set; }
        public Transform Transform;
        public Objeto? Padre { get; set; }
        public Dictionary<int, Triangulo> Hijos { get; } = new();

        private float[] _vertices;
        private uint[] _triangles;
        private uint[] _edges;

        private Mesh? _mesh;
        private Vector4 _color = new(1, 1, 1, 1);
        private int _nextId = 1;
        private bool _disposed = false;

        public float EdgeWidth { get; set; } = 1.75f;

    public Vector3 CentroDeMasa { get; private set; }

        public Cara(string name, float[] vertices, uint[] triangleIndices, uint[]? lineIndices = null, Objeto? padre = null)
        {
            Name = name;
            Transform = new Transform();
            if (padre != null) Padre = padre;

            _vertices = vertices ?? Array.Empty<float>();
            _triangles = triangleIndices ?? Array.Empty<uint>();
            _edges = lineIndices ?? Array.Empty<uint>();

            _mesh = new Mesh(_vertices, _triangles, _triangles);

        CalcularCentroDeMasa();
        }

    public void CalcularCentroDeMasa()
    {
        if (_vertices == null || _vertices.Length < 3) { CentroDeMasa = Vector3.Zero; return; }
        int count = _vertices.Length / 3;
        Vector3 suma = Vector3.Zero;
        for (int i = 0; i < count; i++)
        {
        suma += new Vector3(_vertices[i * 3], _vertices[i * 3 + 1], _vertices[i * 3 + 2]);
        }
        CentroDeMasa = suma / count;
    }

        public float[] GetVertices()  => _vertices;
        public uint[]  GetTriangles() => _triangles;
        public uint[]  GetEdges()     => _edges;
        public Vector4 GetColor()     => _color;

        public void SetColor(Vector4 color) => _color = color;

        public void Add(Triangulo triangulo)
        {
            triangulo.Padre = this;
            Hijos.Add(_nextId++, triangulo);
        }

        public Matrix4 WorldMatrix()
        {
            var local = Transform.LocalMatrix();
            return Padre != null ? Padre.WorldMatrix() * local : local;
        }

        public void Draw(Shader shader, Matrix4 viewProjection)
        {
            EnsureMesh();

            if (_mesh == null) return;

            Matrix4 model = WorldMatrix();
            Matrix4 mvp = model * viewProjection;

            shader.Use();
            shader.SetMatrix4("mvp", mvp);
            shader.SetVector4("uColor", _color);

            GL.PolygonOffset(1.0f, 1.0f);
            _mesh.DrawTriangles();
            
            //_mesh.DrawLines(EdgeWidth);
        }

        private void EnsureMesh()
        {
            if (_mesh == null && _vertices.Length > 0)
            {
                _mesh = new Mesh(_vertices, _triangles, _edges);
            }
        }    


        public void Dispose()
        {
            if (_disposed) return;
            _mesh?.Dispose();
            _disposed = true;
        }
    }
}
