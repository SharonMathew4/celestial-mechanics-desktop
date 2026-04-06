#pragma once
#include "GLLoader.h"
#include "Framebuffer.h"

class Renderer {
public:
    bool Init(int width, int height);
    void Shutdown();
    void RenderScene(int width, int height);
    void RenderToScreen(int width, int height);
    void Resize(int width, int height);

private:
    Framebuffer _fbo;
    GLuint _emptyVAO       = 0;
    GLuint _gradientProgram = 0;
    GLuint _screenProgram   = 0;

    GLuint CompileShader(GLenum type, const char* source);
    GLuint LinkProgram(GLuint vs, GLuint fs);
};
