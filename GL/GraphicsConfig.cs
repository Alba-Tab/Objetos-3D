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

            // Habilitar blending para transparencias
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Back-face culling habilitado para mejor rendimiento
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(TriangleFace.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);
            
        }


        public static void SetBackgroundColor(float r, float g, float b, float a = 1.0f)
        {
            GL.ClearColor(r, g, b, a);
        }

        public static void InicializarGLDefault()
        {
            InicializarOpenGl();
            SetBackgroundColor(0.12f, 0.13f, 0.15f, 1.0f);
        }
    }
}
