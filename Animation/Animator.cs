using OpenTK.Mathematics;

namespace Proyecto_3D.Core3D
{
    public class Frame
    {
        public float Time;
        public Transform Transform;
        public Frame(float time, Transform transform)
        {
            Time = time;
            Transform = transform;
        }
    }
    public class Animation
    {
        public string Name { get; set; }
        public List<Frame> Frames { get; } = new();
        public Animation(string name)
        {
            Name = name;
        }
    }
    public class Animator
    {
        public Animation Animacion { get; set; }
        public float Time { get; set; } = 0;
        public void Update(float deltaTime)
        {
            if (Animacion.Frames.Count == 0) return;
            Time += deltaTime;
            float totalDuration = Animacion.Frames.Last().Time;
            if (Time > totalDuration) Time = 0;
        }
        public Transform GetTransform()
        {
            if (Animacion.Frames.Count == 0) return new Transform();
            Frame? previous = null;
            Frame? next = null;
            foreach (var frame in Animacion.Frames)
            {
                if (frame.Time <= Time) previous = frame;
                if (frame.Time > Time)
                {
                    next = frame;
                    break;
                }
            }
            if (previous == null) return Animacion.Frames.First().Transform;
            if (next == null) return Animacion.Frames.Last().Transform;
            float factor = (Time - previous.Time) / (next.Time - previous.Time);
            var pos = Vector3.Lerp(previous.Transform.Traslacion, next.Transform.Traslacion, factor);
            var rot = Vector3.Lerp(previous.Transform.Rotacion, next.Transform.Rotacion, factor);
            var scale = Vector3.Lerp(previous.Transform.Escala, next.Transform.Escala, factor);
            return new Transform(pos, rot, scale);
        }

    }
}