using System;
using System.IO;
using System.Text;

namespace SolidBox.Engine.IO
{
    internal static class ShaderLoader
    {
        public static string LoadShader(string path)
        {
            return File.ReadAllText(path);
        }

        public static string BulidSaveFile(string vertPath, string fragPath) // TODO: нужно ли делать структуру для хранения шейдеров?
        {
            StringBuilder shaderBuilder = new StringBuilder();

            shaderBuilder.AppendLine("#vert-shader");
            shaderBuilder.AppendFormat(@"{1}{0}{1}", File.ReadAllText(vertPath), Environment.NewLine); 

            shaderBuilder.AppendLine("#frag-shader");
            shaderBuilder.AppendFormat(@"{1}{0}{1}", File.ReadAllText(fragPath), Environment.NewLine);

            return shaderBuilder.ToString();
        }
    }
}
