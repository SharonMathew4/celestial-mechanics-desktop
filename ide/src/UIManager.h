#pragma once

struct GLFWwindow;

class UIManager {
public:
    bool Init(GLFWwindow* window);
    void Shutdown();
    void BeginFrame();
    void EndFrame();
    void Render(int windowWidth, int windowHeight);

    float GetMenuBarHeight() const { return _menuBarHeight; }
    float GetControlBarHeight() const { return _controlBarHeight; }
    bool  WantsQuit() const { return _wantsQuit; }

private:
    void DrawMenuBar();
    void DrawControlBar(int windowWidth, int windowHeight);

    float _menuBarHeight    = 0.0f;
    float _controlBarHeight = 48.0f;
    float _timeScale        = 1.0f;
    bool  _wantsQuit        = false;
};
