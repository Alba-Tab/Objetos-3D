using OpenTK.Mathematics;

namespace Proyecto_3D.Core3D
{
    internal class Figura : IDisposable
    {
        
        public List<Cara> Caras { get; private set; }= new();

        public Vector3 Posicion { get; set; }
        public Vector3 Rotacion { get; set; }
        public Vector3 Escala { get; set; }

        public Vector3 Dimensiones { get; set; } // Cambio de Escala a Dimensiones para consistencia

        private Vector4 _color;
        public Vector4 Color 
        { 
            get => _color;
            set { _color = value; SetColorCaras(value); }
        }

        public Figura(Vector3 posicion = default, Vector3 rotacion = default, Vector3 dimensiones = default, Vector4 color = default)
        {
            Posicion = posicion;
            Rotacion = rotacion;
            Dimensiones = dimensiones == default ? Vector3.One : dimensiones;
            _color = color == default ? new Vector4(1f, 1f, 1f, 1f) : color;
        }

        public void AgregarCara(Cara cara)
        {
            Caras.Add(cara);
            cara.SetColor(_color);
        }

        public void SetColorCaras(Vector4 color)
        {
            foreach (var cara in Caras)
                cara.SetColor(color);
        }

        public void SetColorCara(int indiceCara, Vector4 color)
        {
            if (indiceCara >= 0 && indiceCara < Caras.Count)
                Caras[indiceCara].SetColor(color);
        }

        public void Draw(Shader shader, Matrix4 parent)
        {
            var T  = Matrix4.CreateTranslation(Posicion);
            var RX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotacion.X));
            var RY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotacion.Y));
            var RZ = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotacion.Z));
            var S  = Matrix4.CreateScale(Dimensiones);

            var model = T * RX * RY * RZ * S;
            var composed = model * parent;

            foreach (var cara in Caras) cara.Draw(shader, composed);
        }

        public void Dispose()
        {
            foreach (var cara in Caras) cara.Dispose();
            Caras.Clear();
        }
    }
}
