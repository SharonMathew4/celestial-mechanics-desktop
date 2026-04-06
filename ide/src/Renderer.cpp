#include "Renderer.h"
#include <cstdio>
#include <cstring>

// Fullscreen triangle vertex shader (shared by both programs)
static const char* kFullscreenVS = R"(
#version 450 core
out vec2 vUV;
void main() {
    vec2 verts[3] = vec2[3](
        vec2(-1.0, -1.0),
        vec2( 3.0, -1.0),
        vec2(-1.0,  3.0)
    );
    vUV = verts[gl_VertexID] * 0.5 + 0.5;
    gl_Position = vec4(verts[gl_VertexID], 0.0, 1.0);
}
)";

// Gradient + grid background rendered into FBO
static const char* kGradientFS = R"(
#version 450 core
in vec2 vUV;
out vec4 fragColor;
void main() {
    vec3 top    = vec3(0.03, 0.03, 0.08);
    vec3 bottom = vec3(0.008, 0.008, 0.02);
    vec3 color  = mix(bottom, top, vUV.y);

    // subtle grid overlay
    vec2 grid = abs(fract(vUV * 24.0 - 0.5) - 0.5);
    float line = min(grid.x, grid.y);
    float gridAlpha = 1.0 - smoothstep(0.0, 0.04, line);
    color += vec3(0.035) * gridAlpha;

    fragColor = vec4(color, 1.0);
}
)";

// Screen-space pass: draw FBO texture to default framebuffer
static const char* kScreenFS = R"(
#version 450 core
in vec2 vUV;
out vec4 fragColor;
uniform sampler2D screenTexture;
void main() {
    fragColor = texture(screenTexture, vUV);
}
)";

GLuint Renderer::CompileShader(GLenum type, const char* source) {
    GLuint shader = glCreateShader(type);
    glShaderSource(shader, 1, &source, nullptr);
    glCompileShader(shader);

    GLint ok = 0;
    glGetShaderiv(shader, GL_COMPILE_STATUS, &ok);
    if (!ok) {
        char log[512];
        glGetShaderInfoLog(shader, sizeof(log), nullptr, log);
        fprintf(stderr, "Shader compile error:\n%s\n", log);
        glDeleteShader(shader);
        return 0;
    }
    return shader;
}

GLuint Renderer::LinkProgram(GLuint vs, GLuint fs) {
    GLuint program = glCreateProgram();
    glAttachShader(program, vs);
    glAttachShader(program, fs);
    glLinkProgram(program);

    GLint ok = 0;
    glGetProgramiv(program, GL_LINK_STATUS, &ok);
    if (!ok) {
        char log[512];
        glGetProgramInfoLog(program, sizeof(log), nullptr, log);
        fprintf(stderr, "Program link error:\n%s\n", log);
        glDeleteProgram(program);
        return 0;
    }
    return program;
}

bool Renderer::Init(int width, int height) {
    // Empty VAO for attribute-less fullscreen triangle draws
    glGenVertexArrays(1, &_emptyVAO);

    // Compile gradient program
    GLuint vs1 = CompileShader(GL_VERTEX_SHADER, kFullscreenVS);
    GLuint fs1 = CompileShader(GL_FRAGMENT_SHADER, kGradientFS);
    if (!vs1 || !fs1) return false;
    _gradientProgram = LinkProgram(vs1, fs1);
    glDeleteShader(vs1);
    glDeleteShader(fs1);
    if (!_gradientProgram) return false;

    // Compile screen program
    GLuint vs2 = CompileShader(GL_VERTEX_SHADER, kFullscreenVS);
    GLuint fs2 = CompileShader(GL_FRAGMENT_SHADER, kScreenFS);
    if (!vs2 || !fs2) return false;
    _screenProgram = LinkProgram(vs2, fs2);
    glDeleteShader(vs2);
    glDeleteShader(fs2);
    if (!_screenProgram) return false;

    // Set texture sampler uniform
    glUseProgram(_screenProgram);
    glUniform1i(glGetUniformLocation(_screenProgram, "screenTexture"), 0);
    glUseProgram(0);

    // Create framebuffer
    if (!_fbo.Create(width, height)) return false;

    return true;
}

void Renderer::Shutdown() {
    _fbo.Destroy();
    if (_gradientProgram) { glDeleteProgram(_gradientProgram); _gradientProgram = 0; }
    if (_screenProgram)   { glDeleteProgram(_screenProgram); _screenProgram = 0; }
    if (_emptyVAO)        { glDeleteVertexArrays(1, &_emptyVAO); _emptyVAO = 0; }
}

void Renderer::Resize(int width, int height) {
    _fbo.Resize(width, height);
}

void Renderer::RenderScene(int width, int height) {
    if (width != _fbo.GetWidth() || height != _fbo.GetHeight())
        _fbo.Resize(width, height);

    _fbo.Bind();
    glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    glUseProgram(_gradientProgram);
    glBindVertexArray(_emptyVAO);
    glDrawArrays(GL_TRIANGLES, 0, 3);
    glBindVertexArray(0);
    glUseProgram(0);

    _fbo.Unbind();
}

void Renderer::RenderToScreen(int width, int height) {
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
    glViewport(0, 0, width, height);
    glClear(GL_COLOR_BUFFER_BIT);

    glUseProgram(_screenProgram);
    glActiveTexture(GL_TEXTURE0);
    glBindTexture(GL_TEXTURE_2D, _fbo.GetColorTexture());
    glBindVertexArray(_emptyVAO);
    glDrawArrays(GL_TRIANGLES, 0, 3);
    glBindVertexArray(0);
    glUseProgram(0);
}
