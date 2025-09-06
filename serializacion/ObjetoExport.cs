
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTK.Mathematics;
using Proyecto_3D.Core3D; 

namespace Proyecto_3D.Serializacion
{
    internal static class ObjetoExporter 
    {
        private static Vec3Dto V3(Vector3 v) => new Vec3Dto { x = v.X, y = v.Y, z = v.Z };
        private static ColorDto V4(Vector4 v) => new ColorDto { r = v.X, g = v.Y, b = v.Z, a = v.W };

        private static TrianguloDto ToDto(Triangulo t)
            => new TrianguloDto { a = V3(t.A), b = V3(t.B), c = V3(t.C) };

        private static CaraDto ToDto(int id, Cara cara)
        {
            var dto = new CaraDto
            {
                id = id,
                color = V4(cara.Color),
                drawEdges = cara.DrawEdges,
                edgeLineWidth = cara.EdgeLineWidth,
                edgeColor = V4(cara.EdgeColor)
            };

            foreach (var tri in cara.Triangulos)
                dto.triangulos.Add(ToDto(tri));

            return dto;
        }

        public static ObjetoDto ToDto(Objeto obj)
        {
            var dto = new ObjetoDto
            {
                posicion = V3(obj.Posicion),
                rotacionDeg = V3(obj.RotacionDeg),
                dimensiones = V3(obj.Dimensiones),
                visible = obj.Visible,
                centro = V3(obj.GetCentro())
            };

            foreach (var kv in obj.Caras)
                dto.caras.Add(ToDto(kv.Key, kv.Value));

            return dto;
        }

        public static void SaveAsJson(Objeto obj, string path)
        {
            path = "objetos/" + path;
            var dto = ToDto(obj);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };

            var json = JsonSerializer.Serialize(dto, options);
            File.WriteAllText(path, json);
        }
    }
}
