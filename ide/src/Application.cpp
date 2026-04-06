#include "Application.h"
#include "GLLoader.h"
#include "Renderer.h"
#include "UIManager.h"
#include "SimulationCore.h"
#include <cstdio>

#define GLFW_INCLUDE_NONE
#include <GLFW/glfw3.h>

void Application::FramebufferSizeCallback(GLFWwindow* window, int width, int height) {
    Application* app = static_cast<Application*>(glfwGetWindowUserPointer(window));
    if (app && width > 0 && height > 0) {
        app->_width  = width;
        app->_height = height;
    }
}

bool Application::Init() {
    if (!glfwInit()) {
        fprintf(stderr, "Failed to initialize GLFW\n");
        return false;
    }

    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 4);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 5);
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
    glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GLFW_TRUE);

    _window = glfwCreateWindow(_width, _height, "Celestial Mechanics IDE", nullptr, nullptr);
    if (!_window) {
        fprintf(stderr, "Failed to create GLFW window\n");
        glfwTerminate();
        return false;
    }

    glfwMakeContextCurrent(_window);
    glfwSwapInterval(1); // VSync
    glfwSetWindowUserPointer(_window, this);
    glfwSetFramebufferSizeCallback(_window, FramebufferSizeCallback);

    if (!glLoaderInit()) {
        fprintf(stderr, "Failed to load OpenGL functions\n");
        return false;
    }

    printf("OpenGL %s\n", glGetString(GL_VERSION));
    printf("Renderer: %s\n", glGetString(GL_RENDERER));

    // Enable blending globally
    glEnable(GL_BLEND);
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
    glEnable(GL_DEPTH_TEST);

    _renderer = new Renderer();
    if (!_renderer->Init(_width, _height)) {
        fprintf(stderr, "Failed to initialize Renderer\n");
        return false;
    }

    _ui = new UIManager();
    if (!_ui->Init(_window)) {
        fprintf(stderr, "Failed to initialize UIManager\n");
        return false;
    }

    _sim = new SimulationCore();
    _sim->Init();

    return true;
}

void Application::Run() {
    while (!glfwWindowShouldClose(_window)) {
        glfwPollEvents();

        if (_ui->WantsQuit())
            glfwSetWindowShouldClose(_window, GLFW_TRUE);

        // 1. Update simulation (empty placeholder)
        _sim->Update(0.016f);

        // 2. Render scene to FBO
        _renderer->RenderScene(_width, _height);

        // 3. Bind default framebuffer, draw FBO texture fullscreen
        _renderer->RenderToScreen(_width, _height);

        // 4. Render UI overlay (use window size for ImGui, not framebuffer size)
        int winW, winH;
        glfwGetWindowSize(_window, &winW, &winH);
        _ui->BeginFrame();
        _ui->Render(winW, winH);
        _ui->EndFrame();

        // 5. Present
        glfwSwapBuffers(_window);
    }
}

void Application::Shutdown() {
    if (_sim) { _sim->Shutdown(); delete _sim; _sim = nullptr; }
    if (_ui)  { _ui->Shutdown(); delete _ui; _ui = nullptr; }
    if (_renderer) { _renderer->Shutdown(); delete _renderer; _renderer = nullptr; }
    if (_window) { glfwDestroyWindow(_window); _window = nullptr; }
    glfwTerminate();
}
