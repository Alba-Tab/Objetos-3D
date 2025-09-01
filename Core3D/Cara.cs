
using OpenTK.Mathematics;

namespace Proyecto_3D.Core3D
{
    internal class Cara
    {
        private Dictionary<int, Triangulo> _triangulos= new();
        private int _nextId = 1;
        private Vector3 Centro;
        public Vector4 Color { get; set; } = new Vector4(1f, 1f, 1f, 1f);


        public void Add(Triangulo triangulo)
        {
            _triangulos.Add(_nextId, triangulo);
            _nextId++;
        }
        public Triangulo GetTriangulo(int key)
        {
            return _triangulos[key];
        }
        public void SetCentro(Vector3 c)
        {
            Centro = c;
        }
        public void SetColor(int key, Vector4 color)
        {
            Color = color;
            _triangulos[key].Color = color;
        }
        
        public void SetColor(Vector4 color)
        {
            Color = color;
            foreach (var triangulo in _triangulos.Values)
            {
                triangulo.Color = color;
            }
        }
        public void Draw(Shader shader, Matrix4 parent)
        {
            foreach (var triangulo in _triangulos.Values)
            {
                triangulo.Draw(shader, parent);
            }
        }
        public Vector3 CalcularCentroMasa()
        {
            Vector3 suma = Vector3.Zero;
            foreach (var triangulo in _triangulos.Values)
                suma += triangulo.CalcularCentro();

            if (_triangulos.Count > 0) suma /= _triangulos.Count;

            return suma;
        }

        public void Dispose()
        {
            foreach (var triangulo in _triangulos.Values)
                triangulo.Dispose();    
        }
    }
}
