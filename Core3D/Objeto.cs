
using OpenTK.Mathematics;

namespace Proyecto_3D.Core3D
{
    internal class Objeto
    {
        private readonly Dictionary<int, Figura> _figuras;
        
        private Vector3 _centro { get; set; }
        public Vector3 Posicion;
        public Vector3 Rotacion = Vector3.Zero;
        public Vector3 Dimensiones;
        
        public Objeto(Dictionary<int, Figura> figuras, Vector3 centro)
        {
            _figuras = figuras ?? new Dictionary<int, Figura>();
            _centro = centro;
            Posicion = centro;
            Dimensiones = Vector3.One;
        }


        public void AgregarFigura(int key, Figura figura)
        {
            _figuras.Add(key, figura);
        }
        public Figura GetFigura(int key)
        {
            return _figuras[key];
        }

        public Cara GetCara(int key)
        {
            var figura = _figuras[key];
            return figura.Caras.FirstOrDefault() ?? throw new InvalidOperationException($"No hay caras en la figura {key}");
        }

        public Vector3 GetCentro()
        {
            return _centro;
        }

        public void SetCentro(Vector3 c)
        {
            _centro = c;
        }

        public void SetColorFigura(int key, Vector4 color)
        {
            if (_figuras.ContainsKey(key))  _figuras[key].Color = color;
        }

        public void SetColor(int key, Vector4 color)
        {
            SetColorFigura(key, color);
        }

        public void Draw(Shader shader)
        {
            var T  = Matrix4.CreateTranslation(Posicion);
            var RX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotacion.X));
            var RY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotacion.Y));
            var RZ = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotacion.Z));
            var S  = Matrix4.CreateScale(Dimensiones);

            var model = T * RX * RY * RZ * S;

            foreach (var figura in _figuras.Values)
                figura.Draw(shader, model);
        }

        public Vector3 CalcularCentroMasa()
        {
            Vector3 suma = Vector3.Zero;
            int totalCaras = 0;
            
            foreach (var figura in _figuras.Values)
                foreach (var cara in figura.Caras)
                {
                    suma += cara.CalcularCentroMasa();
                    totalCaras++;
                }

             return (totalCaras > 0) ? (suma / totalCaras) : suma;
        }

        public void Dispose()
        {
            foreach (var figura in _figuras.Values) figura.Dispose();
        }
        public int CantidadFiguras => _figuras.Count;
    }
}
