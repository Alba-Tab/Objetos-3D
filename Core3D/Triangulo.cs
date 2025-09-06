using OpenTK.Mathematics;

namespace Proyecto_3D.Core3D
{
    internal class Triangulo
    {
        public Vector3 A, B, C;
        public Triangulo(Vector3 a, Vector3 b, Vector3 c) { A = a; B = b; C = c; }

        public Vector3 Centro() => (A + B + C) / 3f;
        public IReadOnlyList<Vector3> Vertices => new[] { A, B, C }; 
    }
}
