using OpenTK.Mathematics;

namespace Proyecto_3D.Core3D
{
    public class Escenario : IDisposable
    {
        public string Name { get; set; }
        public Transform Transform;
        public Dictionary<string, Objeto> Hijos { get; } = new();
        public Vector3 CentroDeMasa { get; private set; }
        public Escenario(String name)
        {
            Name = name; Transform = new Transform();
        }
        public void Add(string nombre, Objeto obj)
        {
            if (Hijos.ContainsKey(nombre))
                throw new ArgumentException($"El objeto con nombre '{nombre}' ya existe en el escenario.");
            obj.SetPadre(this);
            Hijos[nombre] = obj;
            CalcularCentroDeMasa();
        }

        public void CalcularCentroDeMasa()
        {
            if (Hijos.Count == 0) { CentroDeMasa = Vector3.Zero; return; }
            Vector3 suma = Vector3.Zero;
            int count = 0;
            foreach (var obj in Hijos.Values)
            {
                obj.CalcularCentroDeMasa();
                suma += obj.CentroDeMasa;
                count++;
            }
            CentroDeMasa = count > 0 ? suma / count : Vector3.Zero;
        }
        public Objeto? GetObjeto(string nombre)
        {
            return Hijos.TryGetValue(nombre, out var obj) ? obj : null;
        }
        public Matrix4 WorldMatrix()
        {
            var local = Transform.LocalMatrix();
            return local;
        }
        public void Draw(Shader shader, Matrix4 ViewProjection)
        {
            foreach (var obj in Hijos.Values)
                obj.Draw(shader, ViewProjection);
        }
        public void Dispose()
        {
            foreach (var obj in Hijos.Values)
                obj.Dispose();
            Hijos.Clear();
        }
    }
}