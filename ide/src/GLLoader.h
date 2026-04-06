#pragma once
#include <cstddef>

// ---- GL Types ----
typedef unsigned int   GLenum;
typedef unsigned char  GLboolean;
typedef unsigned int   GLbitfield;
typedef int            GLint;
typedef unsigned int   GLuint;
typedef int            GLsizei;
typedef float          GLfloat;
typedef char           GLchar;
typedef ptrdiff_t      GLsizeiptr;
typedef ptrdiff_t      GLintptr;
typedef unsigned char  GLubyte;

// ---- GL Constants ----
#define GL_TRUE                    1
#define GL_FALSE                   0
#define GL_NO_ERROR                0
#define GL_FLOAT                   0x1406
#define GL_UNSIGNED_BYTE           0x1401
#define GL_TRIANGLES               0x0004
#define GL_COLOR_BUFFER_BIT        0x00004000
#define GL_DEPTH_BUFFER_BIT        0x00000100
#define GL_DEPTH_TEST              0x0B71
#define GL_BLEND                   0x0BE2
#define GL_CULL_FACE               0x0B44
#define GL_BACK                    0x0405
#define GL_SRC_ALPHA               0x0302
#define GL_ONE_MINUS_SRC_ALPHA     0x0303
#define GL_TEXTURE_2D              0x0DE1
#define GL_TEXTURE0                0x84C0
#define GL_TEXTURE_MIN_FILTER      0x2801
#define GL_TEXTURE_MAG_FILTER      0x2800
#define GL_TEXTURE_WRAP_S          0x2802
#define GL_TEXTURE_WRAP_T          0x2803
#define GL_LINEAR                  0x2601
#define GL_NEAREST                 0x2600
#define GL_CLAMP_TO_EDGE           0x812F
#define GL_RGB                     0x1907
#define GL_RGBA                    0x1908
#define GL_ARRAY_BUFFER            0x8892
#define GL_STATIC_DRAW             0x88E4
#define GL_VERTEX_SHADER           0x8B31
#define GL_FRAGMENT_SHADER         0x8B30
#define GL_COMPILE_STATUS          0x8B81
#define GL_LINK_STATUS             0x8B82
#define GL_INFO_LOG_LENGTH         0x8B84
#define GL_FRAMEBUFFER             0x8D40
#define GL_COLOR_ATTACHMENT0       0x8CE0
#define GL_DEPTH_STENCIL_ATTACHMENT 0x821A
#define GL_DEPTH24_STENCIL8        0x88F0
#define GL_FRAMEBUFFER_COMPLETE    0x8CD5
#define GL_RENDERBUFFER            0x8D41
#define GL_VERSION                 0x1F02
#define GL_RENDERER                0x1F01
#define GL_VENDOR                  0x1F00

// ---- Function Pointer Types ----
typedef void           (*PFN_glEnable)(GLenum);
typedef void           (*PFN_glDisable)(GLenum);
typedef void           (*PFN_glBlendFunc)(GLenum, GLenum);
typedef void           (*PFN_glClearColor)(GLfloat, GLfloat, GLfloat, GLfloat);
typedef void           (*PFN_glClear)(GLbitfield);
typedef void           (*PFN_glViewport)(GLint, GLint, GLsizei, GLsizei);
typedef void           (*PFN_glDrawArrays)(GLenum, GLint, GLsizei);
typedef GLenum         (*PFN_glGetError)(void);
typedef const GLubyte* (*PFN_glGetString)(GLenum);
typedef void           (*PFN_glGetIntegerv)(GLenum, GLint*);
typedef void           (*PFN_glGenTextures)(GLsizei, GLuint*);
typedef void           (*PFN_glBindTexture)(GLenum, GLuint);
typedef void           (*PFN_glDeleteTextures)(GLsizei, const GLuint*);
typedef void           (*PFN_glTexImage2D)(GLenum, GLint, GLint, GLsizei, GLsizei, GLint, GLenum, GLenum, const void*);
typedef void           (*PFN_glTexParameteri)(GLenum, GLenum, GLint);
typedef void           (*PFN_glActiveTexture)(GLenum);
typedef void           (*PFN_glCullFace)(GLenum);
typedef GLuint         (*PFN_glCreateShader)(GLenum);
typedef void           (*PFN_glDeleteShader)(GLuint);
typedef void           (*PFN_glShaderSource)(GLuint, GLsizei, const GLchar* const*, const GLint*);
typedef void           (*PFN_glCompileShader)(GLuint);
typedef void           (*PFN_glGetShaderiv)(GLuint, GLenum, GLint*);
typedef void           (*PFN_glGetShaderInfoLog)(GLuint, GLsizei, GLsizei*, GLchar*);
typedef GLuint         (*PFN_glCreateProgram)(void);
typedef void           (*PFN_glDeleteProgram)(GLuint);
typedef void           (*PFN_glAttachShader)(GLuint, GLuint);
typedef void           (*PFN_glLinkProgram)(GLuint);
typedef void           (*PFN_glUseProgram)(GLuint);
typedef void           (*PFN_glGetProgramiv)(GLuint, GLenum, GLint*);
typedef void           (*PFN_glGetProgramInfoLog)(GLuint, GLsizei, GLsizei*, GLchar*);
typedef GLint          (*PFN_glGetUniformLocation)(GLuint, const GLchar*);
typedef void           (*PFN_glUniform1i)(GLint, GLint);
typedef void           (*PFN_glUniform1f)(GLint, GLfloat);
typedef void           (*PFN_glEnableVertexAttribArray)(GLuint);
typedef void           (*PFN_glVertexAttribPointer)(GLuint, GLint, GLenum, GLboolean, GLsizei, const void*);
typedef void           (*PFN_glGenFramebuffers)(GLsizei, GLuint*);
typedef void           (*PFN_glDeleteFramebuffers)(GLsizei, const GLuint*);
typedef void           (*PFN_glBindFramebuffer)(GLenum, GLuint);
typedef void           (*PFN_glFramebufferTexture2D)(GLenum, GLenum, GLenum, GLuint, GLint);
typedef GLenum         (*PFN_glCheckFramebufferStatus)(GLenum);
typedef void           (*PFN_glGenRenderbuffers)(GLsizei, GLuint*);
typedef void           (*PFN_glDeleteRenderbuffers)(GLsizei, const GLuint*);
typedef void           (*PFN_glBindRenderbuffer)(GLenum, GLuint);
typedef void           (*PFN_glRenderbufferStorage)(GLenum, GLenum, GLsizei, GLsizei);
typedef void           (*PFN_glFramebufferRenderbuffer)(GLenum, GLenum, GLenum, GLuint);
typedef void           (*PFN_glGenVertexArrays)(GLsizei, GLuint*);
typedef void           (*PFN_glDeleteVertexArrays)(GLsizei, const GLuint*);
typedef void           (*PFN_glBindVertexArray)(GLuint);

// ---- Function Pointer Declarations (prefixed to avoid opengl32.lib conflicts) ----
extern PFN_glEnable                    cm_glEnable;
extern PFN_glDisable                   cm_glDisable;
extern PFN_glBlendFunc                 cm_glBlendFunc;
extern PFN_glClearColor                cm_glClearColor;
extern PFN_glClear                     cm_glClear;
extern PFN_glViewport                  cm_glViewport;
extern PFN_glDrawArrays                cm_glDrawArrays;
extern PFN_glGetError                  cm_glGetError;
extern PFN_glGetString                 cm_glGetString;
extern PFN_glGetIntegerv               cm_glGetIntegerv;
extern PFN_glGenTextures               cm_glGenTextures;
extern PFN_glBindTexture               cm_glBindTexture;
extern PFN_glDeleteTextures            cm_glDeleteTextures;
extern PFN_glTexImage2D                cm_glTexImage2D;
extern PFN_glTexParameteri             cm_glTexParameteri;
extern PFN_glActiveTexture             cm_glActiveTexture;
extern PFN_glCullFace                  cm_glCullFace;
extern PFN_glCreateShader              cm_glCreateShader;
extern PFN_glDeleteShader              cm_glDeleteShader;
extern PFN_glShaderSource              cm_glShaderSource;
extern PFN_glCompileShader             cm_glCompileShader;
extern PFN_glGetShaderiv               cm_glGetShaderiv;
extern PFN_glGetShaderInfoLog          cm_glGetShaderInfoLog;
extern PFN_glCreateProgram             cm_glCreateProgram;
extern PFN_glDeleteProgram             cm_glDeleteProgram;
extern PFN_glAttachShader              cm_glAttachShader;
extern PFN_glLinkProgram               cm_glLinkProgram;
extern PFN_glUseProgram                cm_glUseProgram;
extern PFN_glGetProgramiv              cm_glGetProgramiv;
extern PFN_glGetProgramInfoLog         cm_glGetProgramInfoLog;
extern PFN_glGetUniformLocation        cm_glGetUniformLocation;
extern PFN_glUniform1i                 cm_glUniform1i;
extern PFN_glUniform1f                 cm_glUniform1f;
extern PFN_glEnableVertexAttribArray   cm_glEnableVertexAttribArray;
extern PFN_glVertexAttribPointer       cm_glVertexAttribPointer;
extern PFN_glGenFramebuffers           cm_glGenFramebuffers;
extern PFN_glDeleteFramebuffers        cm_glDeleteFramebuffers;
extern PFN_glBindFramebuffer           cm_glBindFramebuffer;
extern PFN_glFramebufferTexture2D      cm_glFramebufferTexture2D;
extern PFN_glCheckFramebufferStatus    cm_glCheckFramebufferStatus;
extern PFN_glGenRenderbuffers          cm_glGenRenderbuffers;
extern PFN_glDeleteRenderbuffers       cm_glDeleteRenderbuffers;
extern PFN_glBindRenderbuffer          cm_glBindRenderbuffer;
extern PFN_glRenderbufferStorage       cm_glRenderbufferStorage;
extern PFN_glFramebufferRenderbuffer   cm_glFramebufferRenderbuffer;
extern PFN_glGenVertexArrays           cm_glGenVertexArrays;
extern PFN_glDeleteVertexArrays        cm_glDeleteVertexArrays;
extern PFN_glBindVertexArray           cm_glBindVertexArray;

// ---- Map standard GL names to prefixed pointers ----
#define glEnable                    cm_glEnable
#define glDisable                   cm_glDisable
#define glBlendFunc                 cm_glBlendFunc
#define glClearColor                cm_glClearColor
#define glClear                     cm_glClear
#define glViewport                  cm_glViewport
#define glDrawArrays                cm_glDrawArrays
#define glGetError                  cm_glGetError
#define glGetString                 cm_glGetString
#define glGetIntegerv               cm_glGetIntegerv
#define glGenTextures               cm_glGenTextures
#define glBindTexture               cm_glBindTexture
#define glDeleteTextures            cm_glDeleteTextures
#define glTexImage2D                cm_glTexImage2D
#define glTexParameteri             cm_glTexParameteri
#define glActiveTexture             cm_glActiveTexture
#define glCullFace                  cm_glCullFace
#define glCreateShader              cm_glCreateShader
#define glDeleteShader              cm_glDeleteShader
#define glShaderSource              cm_glShaderSource
#define glCompileShader             cm_glCompileShader
#define glGetShaderiv               cm_glGetShaderiv
#define glGetShaderInfoLog          cm_glGetShaderInfoLog
#define glCreateProgram             cm_glCreateProgram
#define glDeleteProgram             cm_glDeleteProgram
#define glAttachShader              cm_glAttachShader
#define glLinkProgram               cm_glLinkProgram
#define glUseProgram                cm_glUseProgram
#define glGetProgramiv              cm_glGetProgramiv
#define glGetProgramInfoLog         cm_glGetProgramInfoLog
#define glGetUniformLocation        cm_glGetUniformLocation
#define glUniform1i                 cm_glUniform1i
#define glUniform1f                 cm_glUniform1f
#define glEnableVertexAttribArray   cm_glEnableVertexAttribArray
#define glVertexAttribPointer       cm_glVertexAttribPointer
#define glGenFramebuffers           cm_glGenFramebuffers
#define glDeleteFramebuffers        cm_glDeleteFramebuffers
#define glBindFramebuffer           cm_glBindFramebuffer
#define glFramebufferTexture2D      cm_glFramebufferTexture2D
#define glCheckFramebufferStatus    cm_glCheckFramebufferStatus
#define glGenRenderbuffers          cm_glGenRenderbuffers
#define glDeleteRenderbuffers       cm_glDeleteRenderbuffers
#define glBindRenderbuffer          cm_glBindRenderbuffer
#define glRenderbufferStorage       cm_glRenderbufferStorage
#define glFramebufferRenderbuffer   cm_glFramebufferRenderbuffer
#define glGenVertexArrays           cm_glGenVertexArrays
#define glDeleteVertexArrays        cm_glDeleteVertexArrays
#define glBindVertexArray           cm_glBindVertexArray

bool glLoaderInit();
