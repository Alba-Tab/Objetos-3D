#version 330 core
out vec4 FragColor;
uniform vec4 uColor; // Cambiado a vec4 para recibir RGBA

void main() {
    FragColor = uColor;
}
