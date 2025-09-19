using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace Proyecto_3D.Core3D
{
    public sealed class Triangulo
    {
        public Vector3 A { get; private set; }
        public Vector3 B { get; private set; }
        public Vector3 C { get; private set; }
        public Cara? Padre { get; set; }

        public Triangulo(Vector3 a, Vector3 b, Vector3 c)
        {
            A = a; B = b; C = c;
            // Crear mesh para el triángulo
            float[] vertices = new float[]
            {
                A.X, A.Y, A.Z,
                B.X, B.Y, B.Z,
                C.X, C.Y, C.Z
            };
            uint[] indices = new uint[] { 0, 1, 2 };
            
        }
    }
}
