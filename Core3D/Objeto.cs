using OpenTK.Mathematics;

namespace Proyecto_3D.Core3D
{
    internal class Objeto : IDisposable
    {
        private readonly Dictionary<int, Cara> _caras = new();
        private int _nextId = 1;
        public IReadOnlyDictionary<int, Cara> Caras => _caras; 
        private Vector3 _centro;
        public Vector3 Posicion;
        public Vector3 RotacionDeg = Vector3.Zero; // en grados
        public Vector3 Dimensiones = Vector3.One;
        public bool    Visible    = true;

        public int CantidadCaras => _caras.Count;

        public Objeto(Dictionary<int, Cara>? caras, Vector3 centro)
        {
            if (caras != null)
            {
                foreach (var kv in caras)
                {
                    _caras[kv.Key] = kv.Value;
                    _nextId = Math.Max(_nextId, kv.Key + 1);
                }
            }
            _centro   = centro;
            Posicion  = centro; 
        }

        public int AddCara(Cara cara)
        {
            int id = _nextId++;
            _caras[id] = cara;
            return id;
        }

        public void AddCaras(IEnumerable<Cara> caras)
        {
            foreach (var c in caras) AddCara(c);
        }

        public bool TryGetCara(int key, out Cara cara) => _caras.TryGetValue(key, out cara!);
        public Cara GetCara(int key) => _caras[key];

        public void RemoveCara(int key)
        {
            if (_caras.Remove(key, out var c)) c.Dispose();
        }


        public Vector3 GetCentro() => _centro;
        public void    SetCentro(Vector3 c) => _centro = c;


        public void SetColorCara(int key, Vector4 color)
        {
            if (_caras.TryGetValue(key, out var cara))
                cara.SetColor(color);
        }

        public void SetColorTodas(Vector4 color)
        {
            foreach (var cara in _caras.Values)
                cara.SetColor(color);
        }

        public void Draw(Shader shader) => Draw(shader, Matrix4.Identity);

        public void Draw(Shader shader, Matrix4 parent)
        {
            if (!Visible || _caras.Count == 0) return;

            var T  = Matrix4.CreateTranslation(Posicion);
            var RX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(RotacionDeg.X));
            var RY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(RotacionDeg.Y));
            var RZ = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(RotacionDeg.Z));
            var S  = Matrix4.CreateScale(Dimensiones);

            var local = T * RZ * RY * RX * S;
            var model = parent * local;

            shader.Use();

            foreach (var cara in _caras.Values)
                cara.Draw(shader, model);
        }

        public Vector3 CalcularCentroMasa()
        {
            if (_caras.Count == 0) return Vector3.Zero;
            Vector3 suma = Vector3.Zero;
            int n = 0;
            foreach (var cara in _caras.Values)
            {
                suma += cara.CalcularCentroMasa();
                n++;
            }
            return (n > 0) ? (suma / n) : Vector3.Zero;
        }

        public void Dispose()
        {
            foreach (var cara in _caras.Values)
                cara.Dispose();
            _caras.Clear();
        }
    }
}
