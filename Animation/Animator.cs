using OpenTK.Mathematics;

namespace Proyecto_3D.Core3D
{
    public class Frame
    {
        public float Time;
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;

        public Frame(float time, Transform transform)
        {
            Time = time;
            Position = transform.Traslacion;
            Rotation = transform.Rotacion;
            Scale = transform.Escala;
        }
    }

    public class AnimationClip
    {
        public string Name;
        public List<Frame> Keyframes = new();
        public bool Loop;
        
        public float Duration => Keyframes.Count > 0 ? Keyframes[Keyframes.Count - 1].Time : 0f;

        public AnimationClip(string name, bool loop = true)
        {
            Name = name;
            Loop = loop;
        }

        public void Addframe(float time, Transform transform)
        {
            Keyframes.Add(new Frame(time, transform));
            Keyframes.Sort((a, b) => a.Time.CompareTo(b.Time));
        }
    }

    public class AnimationRecorder
    {
        private AnimationClip? _clip;
        private bool _isRecording;
        private float _time;
        private float _interval;
        private float _timeSinceLastKeyframe;

        public bool IsRecording => _isRecording;

        public AnimationRecorder(float keyframeInterval = 0.1f)
        {
            _interval = keyframeInterval;
        }

        public void StartRecording(string name)
        {
            _clip = new AnimationClip(name, loop: false);
            _isRecording = true;
            _time = 0f;
            _timeSinceLastKeyframe = 0f;
            Console.WriteLine($"🎬 Grabando: {name}");
        }

        public void UpdateRecording(float deltaTime, Transform transform)
        {
            if (!_isRecording || _clip == null) return;

            _time += deltaTime;
            _timeSinceLastKeyframe += deltaTime;

            if (_timeSinceLastKeyframe >= _interval)
            {
                _clip.Addframe(_time, transform);
                _timeSinceLastKeyframe = 0f;
            }
        }

        public AnimationClip? StopRecording()
        {
            if (!_isRecording) return null;
            
            _isRecording = false;
            Console.WriteLine($"⏹️ Detenido: {_clip?.Keyframes.Count} keyframes, {_clip?.Duration:F2}s");
            return _clip;
        }
    }

    public class AnimationPlayer
    {
        private AnimationClip? _clip;
        private float _time;
        private bool _isPlaying;

        public bool IsPlaying => _isPlaying;
        public AnimationClip? CurrentClip => _clip;

        public void Play(AnimationClip clip)
        {
            _clip = clip;
            _time = 0f;
            _isPlaying = true;
            Console.WriteLine($"▶️ Reproduciendo: {clip.Name}");
        }

        public void Stop()
        {
            _isPlaying = false;
            _time = 0f;
        }

        public void ApplyToTransform(Transform target, float deltaTime)
        {
            if (!_isPlaying || _clip == null || _clip.Keyframes.Count == 0)
                return;

            _time += deltaTime;

            // Manejar loop
            if (_time >= _clip.Duration)
            {
                if (_clip.Loop)
                    _time %= _clip.Duration;
                else
                {
                    _isPlaying = false;
                    return;
                }
            }

            // Interpolar entre keyframes
            var transform = Interpolate(_clip.Keyframes, _time);
            if (transform != null)
            {
                target.Traslacion = transform.Position;
                target.Rotacion = transform.Rotation;
                target.Escala = transform.Scale;
            }
        }

        private Frame? Interpolate(List<Frame> keyframes, float time)
        {
            if (keyframes.Count == 0) return null;
            if (keyframes.Count == 1) return keyframes[0];

            Frame? prev = null;
            Frame? next = null;

            foreach (var kf in keyframes)
            {
                if (kf.Time <= time) prev = kf;
                if (kf.Time > time)
                {
                    next = kf;
                    break;
                }
            }

            if (prev == null) return keyframes[0];
            if (next == null) return keyframes[^1];

            float t = (time - prev.Time) / (next.Time - prev.Time);
            
            return new Frame(time, new Transform(
                Vector3.Lerp(prev.Position, next.Position, t),
                Vector3.Lerp(prev.Rotation, next.Rotation, t),
                Vector3.Lerp(prev.Scale, next.Scale, t)
            ));
        }
    }
}