using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Proyecto_3D.Core3D;
using System;

namespace Proyecto_3D
{
    public class Window : GameWindow
    {
        Shader shader = null!;
        private Objeto _pc = null!;

        private Matrix4 view;
        private Matrix4 projection;

        // Rotación del modelo con el mouse en grados
        private float vistaHorizontal = 0f;
        private float vistaVertical = 15f;
        private float distancia = 2f;
        private Vector2 lastMousePos;
        private bool firstMouse = true;

        public Window() 
            : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            ClientSize = new Vector2i(800, 600),
            Title = "PC con Objetos 3D - Nueva Arquitectura",
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

            _pc = BuildPC();

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
            if (KeyboardState.IsKeyDown(Keys.Escape))
                Close();
            
            //zoom
            float scroll = MouseState.ScrollDelta.Y;
            if (scroll != 0f)
            {
                distancia -= scroll * 0.2f;
                distancia = MathHelper.Clamp(distancia, 0.5f, 10f);
                ActualizarView();
            }

            if (MouseState.IsButtonDown(MouseButton.Left))
            {
                if (firstMouse)
                {
                    lastMousePos = new Vector2(MouseState.X, MouseState.Y);
                    firstMouse = false;
                }

                var cur = new Vector2(MouseState.X, MouseState.Y);
                var delta = cur - lastMousePos;
                lastMousePos = cur;

                vistaHorizontal += delta.X * 0.4f;
                vistaVertical -= delta.Y * 0.3f;
                vistaVertical = MathHelper.Clamp(vistaVertical, -85f, 85f);

                ActualizarView();
            }
            else
            {
                firstMouse = true;
            }
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

            shader.Use();
            shader.SetMatrix4("projection", projection);
        }

        private Objeto BuildPC()
        {
            var figuras = new Dictionary<int, Figura>();
            int id = 1;

            var negro = new Vector4(0.08f, 0.08f, 0.10f, 1f);
            var gris = new Vector4(0.25f, 0.26f, 0.28f, 1f);
            var grisClaro = new Vector4(0.75f, 0.75f, 0.78f, 1f);
            var azul = new Vector4(0.23f, 0.45f, 0.85f, 1f);

            Vector3 origen = Vector3.Zero;

            // CPU
            figuras.Add(id++, CrearFiguraCubo(
                posicion: origen + new Vector3(-0.7f, 0.4f, 0f),
                dimensiones: new Vector3(0.35f, 0.80f, 0.40f),
                color: gris));

            // Monitor - Base
            figuras.Add(id++, CrearFiguraCubo(
                posicion: origen + new Vector3(0f, 0.015f, 0f),
                dimensiones: new Vector3(0.45f, 0.03f, 0.25f),
                color: gris));

            // Monitor - Soporte
            figuras.Add(id++, CrearFiguraCubo(
                posicion: origen + new Vector3(0f, 0.15f, 0f),
                dimensiones: new Vector3(0.06f, 0.25f, 0.06f),
                color: gris));

            // Pantalla
            figuras.Add(id++, CrearFiguraCubo(
                posicion: origen + new Vector3(0f, 0.5f, 0f),
                dimensiones: new Vector3(0.90f, 0.55f, 0.035f),
                color: negro));

            // Contenido de pantalla
            figuras.Add(id++, CrearFiguraCubo(
                posicion: origen + new Vector3(0f, 0.5f, 0.018f),
                dimensiones: new Vector3(0.86f, 0.50f, 0.004f),
                color: azul));

            // Teclado
            figuras.Add(id++, CrearFiguraCubo(
                posicion: origen + new Vector3(0f, 0.01f, 0.30f),
                dimensiones: new Vector3(0.40f, 0.02f, 0.20f),
                color: grisClaro));

            // Mouse
            figuras.Add(id++, CrearFiguraCubo(
                posicion: origen + new Vector3(0.32f, 0.015f, 0.30f),
                dimensiones: new Vector3(0.10f, 0.03f, 0.06f),
                color: grisClaro));

            return new Objeto(figuras, origen);
        }

        private Figura CrearFiguraCubo(Vector3 posicion, Vector3 dimensiones, Vector4 color)
        {
            var figura = new Figura(posicion, Vector3.Zero, dimensiones, color);

            var caraFrontal = new Cara();
            var trianguloFrontal = new Triangulo(Vector3.Zero, Vector3.Zero, dimensiones);
            AddCaraFrontal(trianguloFrontal, dimensiones);
            trianguloFrontal.Color = color;
            caraFrontal.Add(trianguloFrontal);
            figura.AgregarCara(caraFrontal);

            var caraTrasera = new Cara();
            var trianguloTrasero = new Triangulo(Vector3.Zero, Vector3.Zero, dimensiones);
            AddCaraTrasera(trianguloTrasero, dimensiones);
            trianguloTrasero.Color = color;
            caraTrasera.Add(trianguloTrasero);
            figura.AgregarCara(caraTrasera);

            var caraIzquierda = new Cara();
            var trianguloIzquierdo = new Triangulo(Vector3.Zero, Vector3.Zero, dimensiones);
            AddCaraIzquierda(trianguloIzquierdo, dimensiones);
            trianguloIzquierdo.Color = color;
            caraIzquierda.Add(trianguloIzquierdo);
            figura.AgregarCara(caraIzquierda);

            var caraDerecha = new Cara();
            var trianguloDerecho = new Triangulo(Vector3.Zero, Vector3.Zero, dimensiones);
            AddCaraDerecha(trianguloDerecho, dimensiones);
            trianguloDerecho.Color = color;
            caraDerecha.Add(trianguloDerecho);
            figura.AgregarCara(caraDerecha);

            var caraInferior = new Cara();
            var trianguloInferior = new Triangulo(Vector3.Zero, Vector3.Zero, dimensiones);
            AddCaraInferior(trianguloInferior, dimensiones);
            trianguloInferior.Color = color;
            caraInferior.Add(trianguloInferior);
            figura.AgregarCara(caraInferior);

            var caraSuperior = new Cara();
            var trianguloSuperior = new Triangulo(Vector3.Zero, Vector3.Zero, dimensiones);
            AddCaraSuperior(trianguloSuperior, dimensiones);
            trianguloSuperior.Color = color;
            caraSuperior.Add(trianguloSuperior);
            figura.AgregarCara(caraSuperior);

            return figura;
        }

        private void AddCaraFrontal(Triangulo triangulo, Vector3 dimensiones)
        {
            float hx = dimensiones.X * 0.5f;
            float hy = dimensiones.Y * 0.5f;
            float hz = dimensiones.Z * 0.5f;

            Vector3[] vertices = {
                new Vector3(-hx, -hy,  hz),
                new Vector3( hx, -hy,  hz),
                new Vector3( hx,  hy,  hz),
                new Vector3( hx,  hy,  hz),
                new Vector3(-hx,  hy,  hz),
                new Vector3(-hx, -hy,  hz)
            };
            foreach (var vertex in vertices)
                triangulo.AddVertex(vertex);
        }

        private void AddCaraTrasera(Triangulo triangulo, Vector3 dimensiones)
        {
            float hx = dimensiones.X * 0.5f;
            float hy = dimensiones.Y * 0.5f;
            float hz = dimensiones.Z * 0.5f;

            Vector3[] vertices = {
                new Vector3(-hx, -hy, -hz),
                new Vector3( hx, -hy, -hz),
                new Vector3( hx,  hy, -hz),
                new Vector3( hx,  hy, -hz),
                new Vector3(-hx,  hy, -hz),
                new Vector3(-hx, -hy, -hz)
            };
            foreach (var vertex in vertices)
                triangulo.AddVertex(vertex);
        }

        private void AddCaraIzquierda(Triangulo triangulo, Vector3 dimensiones)
        {
            float hx = dimensiones.X * 0.5f;
            float hy = dimensiones.Y * 0.5f;
            float hz = dimensiones.Z * 0.5f;

            Vector3[] vertices = {
                new Vector3(-hx,  hy,  hz),
                new Vector3(-hx, -hy,  hz),
                new Vector3(-hx, -hy, -hz),
                new Vector3(-hx, -hy, -hz),
                new Vector3(-hx,  hy, -hz),
                new Vector3(-hx,  hy,  hz)
            };
            foreach (var vertex in vertices)
                triangulo.AddVertex(vertex);
        }

        private void AddCaraDerecha(Triangulo triangulo, Vector3 dimensiones)
        {
            float hx = dimensiones.X * 0.5f;
            float hy = dimensiones.Y * 0.5f;
            float hz = dimensiones.Z * 0.5f;

            Vector3[] vertices = {
                new Vector3( hx,  hy,  hz),
                new Vector3( hx, -hy,  hz),
                new Vector3( hx, -hy, -hz),
                new Vector3( hx, -hy, -hz),
                new Vector3( hx,  hy, -hz),
                new Vector3( hx,  hy,  hz)
            };
            foreach (var vertex in vertices)
                triangulo.AddVertex(vertex);
        }

        private void AddCaraInferior(Triangulo triangulo, Vector3 dimensiones)
        {
            float hx = dimensiones.X * 0.5f;
            float hy = dimensiones.Y * 0.5f;
            float hz = dimensiones.Z * 0.5f;

            Vector3[] vertices = {
                new Vector3(-hx, -hy, -hz),
                new Vector3( hx, -hy, -hz),
                new Vector3( hx, -hy,  hz),
                new Vector3( hx, -hy,  hz),
                new Vector3(-hx, -hy,  hz),
                new Vector3(-hx, -hy, -hz)
            };
            foreach (var vertex in vertices)
                triangulo.AddVertex(vertex);
        }

        private void AddCaraSuperior(Triangulo triangulo, Vector3 dimensiones)
        {
            float hx = dimensiones.X * 0.5f;
            float hy = dimensiones.Y * 0.5f;
            float hz = dimensiones.Z * 0.5f;

            Vector3[] vertices = {
                new Vector3(-hx,  hy, -hz),
                new Vector3( hx,  hy, -hz),
                new Vector3( hx,  hy,  hz),
                new Vector3( hx,  hy,  hz),
                new Vector3(-hx,  hy,  hz),
                new Vector3(-hx,  hy, -hz)
            };
            foreach (var vertex in vertices)
                triangulo.AddVertex(vertex);
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
