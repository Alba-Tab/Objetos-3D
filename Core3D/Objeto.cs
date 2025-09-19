using OpenTK.Mathematics;

namespace Proyecto_3D.Core3D
{
    public class Objeto : IDisposable
    {
        public string Name { get; set; }
        public Transform Transform;
        public Escenario? Padre { get; private set; }
        public Dictionary<int, Cara> Hijos { get; } = new();

        private int _nextId = 1;
        
        public Objeto(string name, Escenario? padre = null)
        {
            Name = name;
            Transform = new Transform();
            if (padre != null) Padre = padre;
        }
        
         internal void SetPadre(Escenario padre) => Padre = padre;
         public void Add(Cara cara)
        {
            cara.Padre = this;
            Hijos.Add(_nextId++, cara);
        }
        
        public Matrix4 WorldMatrix()
        {
            var local = Transform.LocalMatrix();
            return Padre != null ? Padre.WorldMatrix() * local : local;
        }

        public void Draw(Shader shader, Matrix4 ViewProjection)
        {
            foreach (var c in Hijos.Values) c.Draw(shader, ViewProjection);
        }
        public void Dispose()
        {
            foreach (var cara in Hijos.Values) cara.Dispose();
            Hijos.Clear();
        }
    }
}
