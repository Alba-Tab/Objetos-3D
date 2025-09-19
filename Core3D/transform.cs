using OpenTK.Mathematics;

namespace Proyecto_3D.Core3D
{
    public class Transform
    {
        public Vector3 Traslacion;
        public Vector3 Rotacion;
        public Vector3 Escala;
        public bool Enabled;

        public Transform(Vector3? pos = null, Vector3? rotDeg = null, Vector3? scale = null, bool enabled = true)
        {
            Traslacion = pos ?? Vector3.Zero;
            Rotacion = rotDeg ?? Vector3.Zero;
            Escala = scale ?? Vector3.One;
            Enabled = enabled;
        }
        public Matrix4 LocalMatrix()
        {
            if (!Enabled) return Matrix4.Identity;
            var s = Matrix4.CreateScale(Escala);
            var r = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(
                MathHelper.DegreesToRadians(Rotacion.X),
                MathHelper.DegreesToRadians(Rotacion.Y),
                MathHelper.DegreesToRadians(Rotacion.Z)));
            var t = Matrix4.CreateTranslation(Traslacion);
            return t * r * s; // por qu�: TRS en orden simple y legible
        }

    }
}