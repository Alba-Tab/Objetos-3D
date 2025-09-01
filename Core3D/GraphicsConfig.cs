using OpenTK.Graphics.OpenGL4;

namespace Proyecto_3D.Core3D
{
    public static class GraphicsConfig
    {
        /// Configura las opciones básicas de OpenGL para renderizado 3D
        public static void InicializarOpenGl()
        {
            // Para render 3D 
            GL.Enable(EnableCap.DepthTest); 
            GL.DepthFunc(DepthFunction.Less);

            // Back-face culling habilitado para mejor rendimiento
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(TriangleFace.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);

            // Polygon offset SOLO para el relleno (evita z-fighting con el contorno)
            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(1.0f, 1.0f); // empuja el fill un poco "hacia atrás"

            // (Opcional) líneas más suaves y multisample
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.Multisample);
        }

        /// <summary>
        /// Establece el color de fondo de la ventana
        /// </summary>
        public static void SetBackgroundColor(float r, float g, float b, float a = 1.0f)
        {
            GL.ClearColor(r, g, b, a);
        }

        /// <summary>
        /// Configuración predeterminada con fondo gris oscuro
        /// </summary>
        public static void InicializarGLDefault()
        {
            InicializarOpenGl();
            SetBackgroundColor(0.12f, 0.13f, 0.15f, 1.0f);
        }
    }
}
