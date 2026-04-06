#pragma once
#include "GLLoader.h"

class Framebuffer {
public:
    bool Create(int width, int height);
    void Destroy();
    void Bind();
    void Unbind();
    void Resize(int width, int height);

    GLuint GetColorTexture() const { return _colorTex; }
    int    GetWidth()  const { return _width; }
    int    GetHeight() const { return _height; }

private:
    GLuint _fbo        = 0;
    GLuint _colorTex   = 0;
    GLuint _depthRbo   = 0;
    int    _width      = 0;
    int    _height     = 0;

    void CreateAttachments();
    void DeleteAttachments();
};
