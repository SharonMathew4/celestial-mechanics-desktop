#include "Framebuffer.h"
#include <cstdio>

bool Framebuffer::Create(int width, int height) {
    _width  = width;
    _height = height;

    glGenFramebuffers(1, &_fbo);
    glBindFramebuffer(GL_FRAMEBUFFER, _fbo);

    CreateAttachments();

    GLenum status = glCheckFramebufferStatus(GL_FRAMEBUFFER);
    if (status != GL_FRAMEBUFFER_COMPLETE) {
        fprintf(stderr, "Framebuffer incomplete: 0x%X\n", status);
        glBindFramebuffer(GL_FRAMEBUFFER, 0);
        return false;
    }

    glBindFramebuffer(GL_FRAMEBUFFER, 0);
    return true;
}

void Framebuffer::Destroy() {
    DeleteAttachments();
    if (_fbo) { glDeleteFramebuffers(1, &_fbo); _fbo = 0; }
}

void Framebuffer::Bind() {
    glBindFramebuffer(GL_FRAMEBUFFER, _fbo);
    glViewport(0, 0, _width, _height);
}

void Framebuffer::Unbind() {
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
}

void Framebuffer::Resize(int width, int height) {
    if (width == _width && height == _height) return;
    _width  = width;
    _height = height;

    glBindFramebuffer(GL_FRAMEBUFFER, _fbo);
    DeleteAttachments();
    CreateAttachments();
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
}

void Framebuffer::CreateAttachments() {
    // Color texture
    glGenTextures(1, &_colorTex);
    glBindTexture(GL_TEXTURE_2D, _colorTex);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, _width, _height, 0, GL_RGBA, GL_UNSIGNED_BYTE, nullptr);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, _colorTex, 0);

    // Depth-stencil renderbuffer
    glGenRenderbuffers(1, &_depthRbo);
    glBindRenderbuffer(GL_RENDERBUFFER, _depthRbo);
    glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH24_STENCIL8, _width, _height);
    glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_STENCIL_ATTACHMENT, GL_RENDERBUFFER, _depthRbo);
}

void Framebuffer::DeleteAttachments() {
    if (_colorTex) { glDeleteTextures(1, &_colorTex); _colorTex = 0; }
    if (_depthRbo) { glDeleteRenderbuffers(1, &_depthRbo); _depthRbo = 0; }
}
