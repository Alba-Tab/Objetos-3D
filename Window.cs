using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Proyecto_3D.Core3D;
using Proyecto_3D.Serializacion;
using System;

namespace Proyecto_3D
{
    public class Window : GameWindow
    {
        private Shader shader = null!;
        private Objeto _pc   = null!;
        private Matrix4 view, projection;
        // Cámara
        private float vistaHorizontal = 0f;   // yaw   (grados)
        private float vistaVertical   = 15f;  // pitch (grados)
        private float distancia       = 2.2f; // radio órbita
        private Vector2 lastMousePos;
        private bool firstMouse = true;

        public Window() 
            : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            ClientSize = new Vector2i(800, 600),
            Title = "PC-Objetos 3D",
        })
        {
        }
        
        protected override void OnLoad()
        {
            base.OnLoad();
            
            GraphicsConfig.InicializarGLDefault();

            shader = new Shader("Assets/shader.vert", "Assets/shader.frag");

            projection = Matrix4.CreatePerspectiveFieldOfView(
               MathHelper.DegreesToRadians(45.0f),
               Size.X / (float)Size.Y,
               0.1f,
               100.0f
           );
            shader.Use();
            shader.SetMatrix4("projection", projection);

            _pc = ObjetoImporter.LoadFromJson("objetos/pc.json");

            ActualizarView();
            shader.SetMatrix4("view", view);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                       
            shader.Use();
            shader.SetMatrix4("view", view);
            
            _pc.Draw(shader);

            GL.BindVertexArray(0);
            SwapBuffers();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            _pc.Dispose();
            shader.Dispose();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (KeyboardState.IsKeyDown(Keys.Escape)) Close();

            if (MouseState.IsButtonDown(MouseButton.Left))
            {
                if (firstMouse)
                {
                    lastMousePos = new Vector2(MouseState.X, MouseState.Y);
                    firstMouse = false;
                }
                var cur = new Vector2(MouseState.X, MouseState.Y);
                var nuevaPos = cur - lastMousePos;
                lastMousePos = cur;
                vistaHorizontal += nuevaPos.X * 0.4f;
                vistaVertical -= nuevaPos.Y * 0.3f;
                vistaVertical = MathHelper.Clamp(vistaVertical, -85f, 85f);
                ActualizarView();
            }
            else
            {
                firstMouse = true;
            }
            if (KeyboardState.IsKeyPressed(Keys.J))
            {
                ObjetoExporter.SaveAsJson(_pc, "pc.json");
                Console.WriteLine("✔ Objeto exportado a pc.json");
            }   
            if (KeyboardState.IsKeyPressed(Keys.L))
            {
                // Limpia el actual para no fugar recursos
                _pc?.Dispose();
                _pc = ObjetoImporter.LoadFromJson("objetos/pc.json");
                Console.WriteLine("✔ Objeto cargado desde pc.json");
            }
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);

            projection = Matrix4.CreatePerspectiveFieldOfView(
                                MathHelper.DegreesToRadians(45.0f),
                                e.Width / (float)e.Height,
                                0.1f,
                                100.0f
                                );

            shader.Use();
            shader.SetMatrix4("projection", projection);
        }

        private Objeto BuildPC()
        {
            var caras = new Dictionary<int, Cara>();
            int id = 1;

            var negro     = new Vector4(0.08f, 0.08f, 0.10f, 1f);
            var gris      = new Vector4(0.25f, 0.26f, 0.28f, 1f);
            var grisClaro = new Vector4(0.75f, 0.75f, 0.78f, 1f);
            var azul      = new Vector4(0.23f, 0.45f, 0.85f, 1f);

            Vector3 origen = Vector3.Zero;
            // CPU (torre)
            caras.Add(id++, CrearCubo(
                posicion: origen + new Vector3(-0.7f, 0.4f, 0f),
                dimensiones: new Vector3(0.35f, 0.80f, 0.40f),
                color: gris));

            // Monitor - Base
            caras.Add(id++, CrearCubo(
                posicion: origen + new Vector3(0f, 0.015f, 0f),
                dimensiones: new Vector3(0.45f, 0.03f, 0.25f),
                color: gris));

            // Monitor - Soporte
            caras.Add(id++, CrearCubo(
                posicion: origen + new Vector3(0f, 0.15f, 0f),
                dimensiones: new Vector3(0.06f, 0.25f, 0.06f),
                color: gris));

            // Pantalla
            caras.Add(id++, CrearCubo(
                posicion: origen + new Vector3(0f, 0.5f, 0f),
                dimensiones: new Vector3(0.90f, 0.55f, 0.035f),
                color: negro));

            // “Contenido” de pantalla (lámina frontal muy delgada)
            caras.Add(id++, CrearCubo(
                posicion: origen + new Vector3(0f, 0.5f, 0.018f),
                dimensiones: new Vector3(0.86f, 0.50f, 0.004f),
                color: azul));

            // Teclado
            caras.Add(id++, CrearCubo(
                posicion: origen + new Vector3(0f, 0.01f, 0.30f),
                dimensiones: new Vector3(0.40f, 0.02f, 0.20f),
                color: grisClaro));

            // Mouse
            caras.Add(id++, CrearCubo(
                posicion: origen + new Vector3(0.32f, 0.015f, 0.30f),
                dimensiones: new Vector3(0.10f, 0.03f, 0.06f),
                color: grisClaro));

            var objeto = new Objeto(caras, origen);
            
            return objeto;
        }

        // Genera 6 caras (12 triángulos) de un cubo/paralelepípedo centrado en `posicion`.
       private Cara CrearCubo(Vector3 posicion, Vector3 dimensiones, Vector4 color)
        {
            var cara = new Cara();

            float hx = dimensiones.X * 0.5f;
            float hy = dimensiones.Y * 0.5f;
            float hz = dimensiones.Z * 0.5f;

            // 8 esquinas centradas en el origen (espacio local)
            Vector3 p000 = new Vector3(-hx, -hy, -hz) + posicion;
            Vector3 p001 = new Vector3(-hx, -hy, +hz) + posicion;
            Vector3 p010 = new Vector3(-hx, +hy, -hz) + posicion;
            Vector3 p011 = new Vector3(-hx, +hy, +hz) + posicion;
            Vector3 p100 = new Vector3(+hx, -hy, -hz) + posicion;
            Vector3 p101 = new Vector3(+hx, -hy, +hz) + posicion;
            Vector3 p110 = new Vector3(+hx, +hy, -hz) + posicion;
            Vector3 p111 = new Vector3(+hx, +hy, +hz) + posicion;

            // Caras del cubo (cada una formada por 2 triángulos)
            // Frontal (+Z)
            AddFace(cara, p101, p001, p011, p111);
            // Trasera (-Z)
            AddFace(cara, p100, p110, p010, p000);
            // Izquierda (-X)
            AddFace(cara, p000, p010, p011, p001);
            // Derecha (+X)
            AddFace(cara, p101, p111, p110, p100);
            // Superior (+Y)
            AddFace(cara, p010, p110, p111, p011);
            // Inferior (-Y)
            AddFace(cara, p000, p001, p101, p100);

            cara.SetColor(color);
            cara.DrawEdges = true;
            return cara;
        }

        // Helper: agrega dos triángulos a partir de un quad ABCD (CCW).
        private void AddFace(Cara cara, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            cara.Add(new Triangulo(a, b, c));
            cara.Add(new Triangulo(a, c, d));
        }

        private void ActualizarView()
        {
            // Cámara orbital alrededor del origen (donde armamos la PC)
            float VistaYRad = MathHelper.DegreesToRadians(vistaHorizontal);
            float VistaXRad = MathHelper.DegreesToRadians(vistaVertical);

            var target = Vector3.Zero;
            var eye = new Vector3(
                distancia * MathF.Cos(VistaXRad) * MathF.Sin(VistaYRad),
                distancia * MathF.Sin(VistaXRad),
                distancia * MathF.Cos(VistaXRad) * MathF.Cos(VistaYRad)
            );

            view = Matrix4.LookAt(eye, target, Vector3.UnitY);
        }
    }
}
