using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Proyecto_3D.Core3D;
using System.Runtime.CompilerServices;
using static Proyecto_3D.Core3D.Objects;

namespace Proyecto_3D
{
    public class Window : GameWindow
    {

        Shader shader;
        Mesh _mesh;
        private Matrix4 model;
        private Matrix4 view;
        private Matrix4 projection;

        private float angleY = 0f;
        private float angleX = 0f;
        private Vector2 lastMousePos;

        public Window() : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            ClientSize = new Vector2i(800, 600),
            Title = "Hello Cube",
        })
        {
            lastMousePos = new Vector2(MouseState.X, MouseState.Y);
        }
        protected override void OnLoad()
        {
            base.OnLoad();

            // Habilitar test de profundidad para renderizado 3D correcto
            GL.Enable(EnableCap.DepthTest); // asegura que se respete la profundidad
            GL.DepthFunc(DepthFunction.Less);

            // Back-face culling (asumiendo vértices CCW)
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);

            // Polygon offset SOLO para el relleno (evita z-fighting con el contorno)
            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(1.0f, 1.0f); // empuja el fill un poco "hacia atrás"

            // (Opcional) líneas más suaves y multisample
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.Multisample);

 
            Vector3 origen = Vector3.Zero;

            //dibujar respecto a las partes del objeto de manera relativa
            //traslacion otra manera de "sumar"
            var datos = new List<Figura>
            {
                // torre
            new Figura
            {
                Type = TipoFigura.Box,
                P0 = origen + new Vector3(-0.7f , 0.4f , 0f), // centro
                Size = new Vector3(0.35f, 0.80f , 0.40f )
            },
                // base monitor
            new Figura
            {
                Type = TipoFigura.Box,
                P0 = origen + new Vector3(0f, 0.015f , 0f),
                Size = new Vector3(0.45f , 0.03f , 0.25f )
            },
                    // soporte
            new Figura
            {
                Type = TipoFigura.Box,
                P0 = origen + new Vector3(0f, 0.15f, 0f),
                Size = new Vector3(0.06f , 0.25f , 0.06f )
            },
                // pantalla (panel fino)
            new Figura
            {
                Type = TipoFigura.Box,
                P0 = origen + new Vector3(0f, 0.5f , 0f),
                Size = new Vector3(0.90f , 0.55f , 0.04f )
            },
                // teclado
            new Figura
            {
                Type = TipoFigura.Box,
                P0 = origen + new Vector3(0f, 0.01f , 0.30f ),
                Size = new Vector3(0.40f , 0.02f , 0.20f )
            },
            new Figura
            {
                //Type = TipoFigura.RegularPolygon,
                Type=TipoFigura.Prisma,
                P0 = origen + new Vector3(0.4f, 0.01f, 0.30f ),
                Radius=0.08f,
                Sides = 30,
                Height=0.02f
            }
            };
           //definir una estructura que permita dibujar un objeto
           //no hacerlo dependiente de lo que quiero dibujar
           //coordenadas relativas
           //sacar informacion de los vertices o del dibujo, para dibujar desde un archivo, para dibujar se necesitan 6 lineas co ndos metodos para cada una
           //serializo el objeto
           //pasasr de objeto a texto y de texto a objeto
           //definir vertives en el codigo serializarlo y luego usar el serializer para crearlo de nuevo
           //libs

            var obj = draw(datos);
            _mesh = obj.ToMesh();



            shader = new Shader("Assets/shader.vert", "Assets/shader.frag");
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // Configurar matrices de transformación
            model = Matrix4.Identity;
            view = Matrix4.LookAt(Vector3.UnitZ * 3, Vector3.Zero, Vector3.UnitY);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f),
                         Size.X / (float)Size.Y, 0.1f, 100.0f);


            shader.SetMatrix4("model", model);
            shader.SetMatrix4("view", view);
            shader.SetMatrix4("projection", projection);
        }

     
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            // Limpiar tanto el color como el buffer de profundidad

            shader.Use();

            shader.SetMatrix4("model", model);

            // sólido
            shader.SetVector3("uColor", new Vector3(0.75f));
            _mesh.DrawTriangles();

            // contorno
            shader.SetVector3("uColor", new Vector3(0.05f));
            GL.LineWidth(2f);
            _mesh.DrawLines(2f);


            GL.BindVertexArray(0);
            SwapBuffers();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            _mesh.Dispose();
            shader.Dispose();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (KeyboardState.IsKeyDown(Keys.Escape))
                Close();
            var mouse = MouseState;
                
            if (mouse.IsButtonDown(MouseButton.Left))
            {
                var delta = new Vector2(mouse.X, mouse.Y) - lastMousePos;
                angleY += delta.X * 0.5f; // sensibilidad
                angleX += delta.Y * 0.5f;
                model = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(angleY)) *
                    Matrix4.CreateRotationX(MathHelper.DegreesToRadians(angleX));

            }
            lastMousePos = new Vector2(mouse.X, mouse.Y);

        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);

            // Actualizar la matriz de proyección cuando cambie el tamaño
            projection = Matrix4.CreatePerspectiveFieldOfView(
                                MathHelper.DegreesToRadians(45.0f),
                                e.Width / (float)e.Height,
                                0.1f,
                                100.0f
                                );
        }
    }

    
}