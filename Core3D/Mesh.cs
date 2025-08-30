using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Proyecto_3D.Core3D
{
    internal class Mesh : IDisposable
    {
        public int vbo, vaoSolid, eboSolid, vaoEdges, eboEdges;
        private int _triCount = 0;
        private int _lineCount = 0;
        private bool _disposed;

        public Mesh(float[] vertices, uint[] triangleIndices, uint[] lineIndices = null)
        {
            // --- VAO de sólidos ---
            vaoSolid = GL.GenVertexArray();
            GL.BindVertexArray(vaoSolid);

            //VBO compartido 
            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float),
                           vertices, BufferUsageHint.StaticDraw);

            //ATRIBUTOS
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float,
                                    false, 3 * sizeof(float), 0);
                        
                //EBO triángulos
            eboSolid = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboSolid);
            GL.BufferData(BufferTarget.ElementArrayBuffer, triangleIndices.Length * sizeof(uint),
                           triangleIndices, BufferUsageHint.StaticDraw);
            _triCount = triangleIndices.Length;


            if (lineIndices != null && lineIndices.Length > 0)
            {
                // --- VAO de aristas ---
                vaoEdges = GL.GenVertexArray();
                GL.BindVertexArray(vaoEdges);

                // Reutiliza el mismo VBO y layout de atributos
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

                // Crea un nuevo EBO para las aristas

                eboEdges = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboEdges);
                GL.BufferData(BufferTarget.ElementArrayBuffer, lineIndices.Length * sizeof(uint),
                              lineIndices, BufferUsageHint.StaticDraw);
                _lineCount = lineIndices.Length;
            }
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

        }
        public void DrawTriangles()
        {
            // Renderizar el cubo
            // enlazar el VAO de sólidos y dibujar los elementos
            if (_triCount == 0) return;
            GL.BindVertexArray(vaoSolid);
            GL.DrawElements(PrimitiveType.Triangles, _triCount, DrawElementsType.UnsignedInt, 0);

        }
        public void DrawLines(float lineWidth = 2f)
        {
            // 2) Contornos
            // enlazar el VAO de aristas y dibujar los elementos
            if (_lineCount == 0 || vaoEdges == 0) return;
            GL.BindVertexArray(vaoEdges);
            GL.LineWidth(lineWidth);
            GL.DrawElements(PrimitiveType.Lines, _lineCount, DrawElementsType.UnsignedInt, 0);
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (vbo != 0) GL.DeleteBuffer(vbo);
            if (eboSolid != 0) GL.DeleteBuffer(eboSolid);
            if (eboEdges != 0) GL.DeleteBuffer(eboEdges);
            if (vaoSolid != 0) GL.DeleteVertexArray(vaoSolid);
            if (vaoEdges != 0) GL.DeleteVertexArray(vaoEdges);
        }
    }
}
