#version 330 core
out vec4 FragColor;
uniform vec3 uColor; // lo estableces desde C#: shader.SetVector3("uColor", ...);

void main() {
    FragColor = vec4(uColor, 1.0);
    //FragColor = vec4(0.5,0.5,0.5,1.0);
}
