#define GLFW_INCLUDE_NONE
#include <GLFW/glfw3.h>
#include "GLLoader.h"

PFN_glEnable                    cm_glEnable = nullptr;
PFN_glDisable                   cm_glDisable = nullptr;
PFN_glBlendFunc                 cm_glBlendFunc = nullptr;
PFN_glClearColor                cm_glClearColor = nullptr;
PFN_glClear                     cm_glClear = nullptr;
PFN_glViewport                  cm_glViewport = nullptr;
PFN_glDrawArrays                cm_glDrawArrays = nullptr;
PFN_glGetError                  cm_glGetError = nullptr;
PFN_glGetString                 cm_glGetString = nullptr;
PFN_glGetIntegerv               cm_glGetIntegerv = nullptr;
PFN_glGenTextures               cm_glGenTextures = nullptr;
PFN_glBindTexture               cm_glBindTexture = nullptr;
PFN_glDeleteTextures            cm_glDeleteTextures = nullptr;
PFN_glTexImage2D                cm_glTexImage2D = nullptr;
PFN_glTexParameteri             cm_glTexParameteri = nullptr;
PFN_glActiveTexture             cm_glActiveTexture = nullptr;
PFN_glCullFace                  cm_glCullFace = nullptr;
PFN_glCreateShader              cm_glCreateShader = nullptr;
PFN_glDeleteShader              cm_glDeleteShader = nullptr;
PFN_glShaderSource              cm_glShaderSource = nullptr;
PFN_glCompileShader             cm_glCompileShader = nullptr;
PFN_glGetShaderiv               cm_glGetShaderiv = nullptr;
PFN_glGetShaderInfoLog          cm_glGetShaderInfoLog = nullptr;
PFN_glCreateProgram             cm_glCreateProgram = nullptr;
PFN_glDeleteProgram             cm_glDeleteProgram = nullptr;
PFN_glAttachShader              cm_glAttachShader = nullptr;
PFN_glLinkProgram               cm_glLinkProgram = nullptr;
PFN_glUseProgram                cm_glUseProgram = nullptr;
PFN_glGetProgramiv              cm_glGetProgramiv = nullptr;
PFN_glGetProgramInfoLog         cm_glGetProgramInfoLog = nullptr;
PFN_glGetUniformLocation        cm_glGetUniformLocation = nullptr;
PFN_glUniform1i                 cm_glUniform1i = nullptr;
PFN_glUniform1f                 cm_glUniform1f = nullptr;
PFN_glEnableVertexAttribArray   cm_glEnableVertexAttribArray = nullptr;
PFN_glVertexAttribPointer       cm_glVertexAttribPointer = nullptr;
PFN_glGenFramebuffers           cm_glGenFramebuffers = nullptr;
PFN_glDeleteFramebuffers        cm_glDeleteFramebuffers = nullptr;
PFN_glBindFramebuffer           cm_glBindFramebuffer = nullptr;
PFN_glFramebufferTexture2D      cm_glFramebufferTexture2D = nullptr;
PFN_glCheckFramebufferStatus    cm_glCheckFramebufferStatus = nullptr;
PFN_glGenRenderbuffers          cm_glGenRenderbuffers = nullptr;
PFN_glDeleteRenderbuffers       cm_glDeleteRenderbuffers = nullptr;
PFN_glBindRenderbuffer          cm_glBindRenderbuffer = nullptr;
PFN_glRenderbufferStorage       cm_glRenderbufferStorage = nullptr;
PFN_glFramebufferRenderbuffer   cm_glFramebufferRenderbuffer = nullptr;
PFN_glGenVertexArrays           cm_glGenVertexArrays = nullptr;
PFN_glDeleteVertexArrays        cm_glDeleteVertexArrays = nullptr;
PFN_glBindVertexArray           cm_glBindVertexArray = nullptr;

// Token-paste suppresses macro expansion, so GL_LOAD(glEnable) correctly
// resolves to: cm_glEnable = (PFN_glEnable)glfwGetProcAddress("glEnable")
#define GL_LOAD(fn) cm_##fn = (PFN_##fn)glfwGetProcAddress(#fn)

bool glLoaderInit() {
    GL_LOAD(glEnable);
    GL_LOAD(glDisable);
    GL_LOAD(glBlendFunc);
    GL_LOAD(glClearColor);
    GL_LOAD(glClear);
    GL_LOAD(glViewport);
    GL_LOAD(glDrawArrays);
    GL_LOAD(glGetError);
    GL_LOAD(glGetString);
    GL_LOAD(glGetIntegerv);
    GL_LOAD(glGenTextures);
    GL_LOAD(glBindTexture);
    GL_LOAD(glDeleteTextures);
    GL_LOAD(glTexImage2D);
    GL_LOAD(glTexParameteri);
    GL_LOAD(glActiveTexture);
    GL_LOAD(glCullFace);
    GL_LOAD(glCreateShader);
    GL_LOAD(glDeleteShader);
    GL_LOAD(glShaderSource);
    GL_LOAD(glCompileShader);
    GL_LOAD(glGetShaderiv);
    GL_LOAD(glGetShaderInfoLog);
    GL_LOAD(glCreateProgram);
    GL_LOAD(glDeleteProgram);
    GL_LOAD(glAttachShader);
    GL_LOAD(glLinkProgram);
    GL_LOAD(glUseProgram);
    GL_LOAD(glGetProgramiv);
    GL_LOAD(glGetProgramInfoLog);
    GL_LOAD(glGetUniformLocation);
    GL_LOAD(glUniform1i);
    GL_LOAD(glUniform1f);
    GL_LOAD(glEnableVertexAttribArray);
    GL_LOAD(glVertexAttribPointer);
    GL_LOAD(glGenFramebuffers);
    GL_LOAD(glDeleteFramebuffers);
    GL_LOAD(glBindFramebuffer);
    GL_LOAD(glFramebufferTexture2D);
    GL_LOAD(glCheckFramebufferStatus);
    GL_LOAD(glGenRenderbuffers);
    GL_LOAD(glDeleteRenderbuffers);
    GL_LOAD(glBindRenderbuffer);
    GL_LOAD(glRenderbufferStorage);
    GL_LOAD(glFramebufferRenderbuffer);
    GL_LOAD(glGenVertexArrays);
    GL_LOAD(glDeleteVertexArrays);
    GL_LOAD(glBindVertexArray);

    return cm_glEnable && cm_glClear && cm_glCreateShader &&
           cm_glGenFramebuffers && cm_glGenVertexArrays && cm_glBindTexture;
}

#undef GL_LOAD
