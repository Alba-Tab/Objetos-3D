using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Proyecto_3D.Core3D
{
    public class Shader
    {
        public int Handle;
        private bool disposedValue = false;

        public Shader(string vertexPath, string fragmentPath)
        {
            string vertexCode = File.ReadAllText(vertexPath);
            string fragmentCode = File.ReadAllText(fragmentPath);

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexCode);
            GL.CompileShader(vertexShader);
            CheckShaderCompile(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentCode);
            GL.CompileShader(fragmentShader);
            CheckShaderCompile(fragmentShader);

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);
            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                throw new Exception($"Error linking shader program:\n{infoLog}");
            }

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        void CheckShaderCompile(int shader)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error compiling shader:\n{infoLog}");
            }
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            int location = GL.GetUniformLocation(Handle, name);
            GL.UniformMatrix4(location, false, ref data);
        }

        public void SetVector3(string name, Vector3 v)
        {
            GL.UseProgram(Handle);
            int loc = GL.GetUniformLocation(Handle, name);
            GL.Uniform3(loc, v);
        }
        public void SetVector4(string name, Vector4 v)
        {
            GL.UseProgram(Handle);
            int loc = GL.GetUniformLocation(Handle, name);
            GL.Uniform4(loc, v);
        }
        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }
        public void Dispose()
        {
            if (disposedValue) return;
            GL.DeleteProgram(Handle);
            disposedValue = true;
            GC.SuppressFinalize(this);
        }

        ~Shader()
        {
            if (!disposedValue)
                Console.WriteLine("¡Fuga de recursos de GPU! ¿Olvidaste llamar a Dispose()?");
        }

    }
}
