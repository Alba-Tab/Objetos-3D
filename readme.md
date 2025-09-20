# Codigo para generar escenario y cubos quitado

```C#


// =======================
        // Construcción de escena
        // =======================
        private Escenario BuildEjes()
        {
            var ejes = new Escenario("Ejes");

            var redTransparent   = new Vector4(1f, 0f, 0f, 0.1f);
            var greenTransparent = new Vector4(0f, 1f, 0f, 0.1f);
            var blueTransparent  = new Vector4(0f, 0f, 1f, 0.1f);

            // Cubos ultrafinos que hacen de planos
            AgregarCubo(ejes, "Eje YZ", Vector3.Zero, new Vector3(0.0001f, 10f, 10f), redTransparent);
            AgregarCubo(ejes, "Eje XZ", Vector3.Zero, new Vector3(10f, 0.0001f, 10f), greenTransparent);
            AgregarCubo(ejes, "Eje XY", Vector3.Zero, new Vector3(10f, 10f, 0.0001f), blueTransparent);


            return ejes;
        }

        private void AgregarCubo(Escenario esc, string nombre, Vector3 posicion, Vector3 dimensiones, Vector4 color)
        {
            var cara = CrearCubo(dimensiones, color);   // esta función ya crea triángulos y edges
            var obj = new Objeto(nombre, esc);
            obj.Transform.Traslacion = posicion;
            obj.Add(cara);
            esc.Add(nombre, obj);
        }
        // Escena de PC de escritorio
        private Escenario BuildPC()
        {
            var escenario = new Escenario("PC");

            // CPU
            var cpu = CrearObjetoCubo(
                "CPU", escenario,
                posicion: new Vector3(-0.7f, 0.4f, 0f),
                dimensiones: new Vector3(0.35f, 0.80f, 0.40f),
                color: new Vector4(0.25f, 0.26f, 0.28f, 1f)
            );
            escenario.Add("CPU", cpu);

            // Monitor compuesto
            var monitor = CrearMonitor(
                parent: escenario,
                basePos: new Vector3(0f, 0.015f, 0f),
                baseDim: new Vector3(0.45f, 0.03f, 0.25f),
                baseColor: new Vector4(0.25f, 0.26f, 0.28f, 1f),
                soportePos: new Vector3(0f, 0.15f, 0f),
                soporteDim: new Vector3(0.06f, 0.25f, 0.06f),
                soporteColor: new Vector4(0.25f, 0.26f, 0.28f, 1f),
                pantallaPos: new Vector3(0f, 0.5f, 0f),
                pantallaDim: new Vector3(0.90f, 0.55f, 0.035f),
                pantallaColor: new Vector4(0.08f, 0.08f, 0.10f, 1f),
                contenidoPos: new Vector3(0f, 0.5f, 0.018f),
                contenidoDim: new Vector3(0.86f, 0.50f, 0.004f),
                contenidoColor: new Vector4(0.23f, 0.45f, 0.85f, 1f)
            );
            escenario.Add("Monitor", monitor);

            // Teclado
            var teclado = CrearObjetoCubo(
                "Teclado", escenario,
                posicion: new Vector3(0f, 0.01f, 0.30f),
                dimensiones: new Vector3(0.40f, 0.02f, 0.20f),
                color: new Vector4(0.75f, 0.75f, 0.78f, 1f)
            );
            escenario.Add("Teclado", teclado);

            // Mouse
            var mouse = CrearObjetoCubo(
                "Mouse", escenario,
                posicion: new Vector3(0.32f, 0.015f, 0.30f),
                dimensiones: new Vector3(0.10f, 0.03f, 0.06f),
                color: new Vector4(0.75f, 0.75f, 0.78f, 1f)
            );
            escenario.Add("Mouse", mouse);

            return escenario;
        }

        // Helpers de construcción


        // Crea un cubo con 6 caras independientes
        private Objeto CrearObjetoCubo(string nombre, Escenario parent, Vector3 posicion, Vector3 dimensiones, Vector4 color)
        {
            var obj = new Objeto(nombre, parent);
            obj.Transform.Traslacion = posicion;
            var caras = CrearCarasCubo(dimensiones, color);
            foreach (var cara in caras)
                obj.Add(cara);
            return obj;
        }

        // Crea las 6 caras de un cubo, cada una como un objeto Cara independiente
        private List<Cara> CrearCarasCubo(Vector3 dimensiones, Vector4 color)
        {
            float hx = dimensiones.X * 0.5f;
            float hy = dimensiones.Y * 0.5f;
            float hz = dimensiones.Z * 0.5f;

            // Vértices de un cubo centrado en el origen
            var v = new Vector3[] {
                new(-hx, -hy,  hz), // 0
                new( hx, -hy,  hz), // 1
                new( hx,  hy,  hz), // 2
                new(-hx,  hy,  hz), // 3
                new(-hx, -hy, -hz), // 4
                new( hx, -hy, -hz), // 5
                new( hx,  hy, -hz), // 6
                new(-hx,  hy, -hz)  // 7
            };

            // Definición de las 6 caras (cada una con 4 vértices)
            var caras = new List<Cara>();
            // Frente
            caras.Add(CrearCaraPlano("Frente", new[] { v[0], v[1], v[2], v[3] }, color));
            // Atrás
            caras.Add(CrearCaraPlano("Atras", new[] { v[5], v[4], v[7], v[6] }, color));
            // Izquierda
            caras.Add(CrearCaraPlano("Izquierda", new[] { v[4], v[0], v[3], v[7] }, color));
            // Derecha
            caras.Add(CrearCaraPlano("Derecha", new[] { v[1], v[5], v[6], v[2] }, color));
            // Arriba
            caras.Add(CrearCaraPlano("Arriba", new[] { v[3], v[2], v[6], v[7] }, color));
            // Abajo
            caras.Add(CrearCaraPlano("Abajo", new[] { v[4], v[5], v[1], v[0] }, color));
            return caras;
        }

        // Crea una cara plana (cuadrada) a partir de 4 vértices
        private Cara CrearCaraPlano(string nombre, Vector3[] vertices, Vector4 color)
        {
            // 4 vértices, 2 triángulos
            float[] verts = new float[] {
                vertices[0].X, vertices[0].Y, vertices[0].Z,
                vertices[1].X, vertices[1].Y, vertices[1].Z,
                vertices[2].X, vertices[2].Y, vertices[2].Z,
                vertices[3].X, vertices[3].Y, vertices[3].Z
            };
            uint[] tris = new uint[] { 0, 1, 2, 0, 2, 3 };
            uint[] edges = new uint[] { 0,1, 1,2, 2,3, 3,0 };
            var cara = new Cara(nombre, verts, tris, edges);
            cara.SetColor(color);
            cara.EdgeWidth = 2.0f;
            return cara;
        }

        private Objeto CrearMonitor(
            Escenario parent,
            Vector3 basePos, Vector3 baseDim, Vector4 baseColor,
            Vector3 soportePos, Vector3 soporteDim, Vector4 soporteColor,
            Vector3 pantallaPos, Vector3 pantallaDim, Vector4 pantallaColor,
            Vector3 contenidoPos, Vector3 contenidoDim, Vector4 contenidoColor)
        {
            var monitor = new Objeto("Monitor", parent);

            // Base
            var carasBase = CrearCarasCubo(baseDim, baseColor);
            foreach (var cara in carasBase)
            {
                cara.Transform.Traslacion = basePos;
                monitor.Add(cara);
            }

            // Soporte
            var carasSoporte = CrearCarasCubo(soporteDim, soporteColor);
            foreach (var cara in carasSoporte)
            {
                cara.Transform.Traslacion = soportePos;
                monitor.Add(cara);
            }

            // Pantalla
            var carasPantalla = CrearCarasCubo(pantallaDim, pantallaColor);
            foreach (var cara in carasPantalla)
            {
                cara.Transform.Traslacion = pantallaPos;
                monitor.Add(cara);
            }

            // Contenido
            var carasContenido = CrearCarasCubo(contenidoDim, contenidoColor);
            foreach (var cara in carasContenido)
            {
                cara.Transform.Traslacion = contenidoPos;
                monitor.Add(cara);
            }

            return monitor;
        }

        // dentro de Window.CrearCubo(...)
        private Cara CrearCubo(Vector3 dimensiones, Vector4 color)
        {
            float hx = dimensiones.X * 0.5f;
            float hy = dimensiones.Y * 0.5f;
            float hz = dimensiones.Z * 0.5f;

            var vertices = new float[]
            {
                -hx, -hy,  hz,  hx, -hy,  hz,  hx,  hy,  hz,  -hx,  hy,  hz,
                -hx, -hy, -hz,  hx, -hy, -hz,  hx,  hy, -hz,  -hx,  hy, -hz,
            };

            var tri = new uint[]
            {
                0,1,2, 0,2,3,   5,4,7, 5,7,6,
                4,0,3, 4,3,7,   1,5,6, 1,6,2,
                3,2,6, 3,6,7,   4,5,1, 4,1,0
            };

            // 12 aristas (pares)
            var edges = new uint[]
            {
                0,1, 1,2, 2,3, 3,0,
                4,5, 5,6, 6,7, 7,4,
                0,4, 1,5, 2,6, 3,7
            };

            // PASA edges aquí
            var cara = new Cara("Cubo", vertices, tri, edges);
            cara.SetColor(color);
            cara.EdgeWidth = 2.0f; // opcional

            return cara;
        }


        private void AddFace(Cara cara, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            cara.Add(new Triangulo(a, b, c));
            cara.Add(new Triangulo(a, c, d));
        }
```
