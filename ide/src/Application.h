#pragma once

struct GLFWwindow;
class Renderer;
class UIManager;
class SimulationCore;

class Application {
public:
    bool Init();
    void Run();
    void Shutdown();

private:
    static void FramebufferSizeCallback(GLFWwindow* window, int width, int height);

    GLFWwindow*     _window   = nullptr;
    Renderer*       _renderer = nullptr;
    UIManager*      _ui       = nullptr;
    SimulationCore* _sim      = nullptr;
    int             _width    = 1280;
    int             _height   = 720;
};
