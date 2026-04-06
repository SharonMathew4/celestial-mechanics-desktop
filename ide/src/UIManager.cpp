#include "UIManager.h"
#include "GLLoader.h"
#include <imgui.h>
#include <imgui_impl_glfw.h>
#include <imgui_impl_opengl3.h>
#include <cstdio>

bool UIManager::Init(GLFWwindow* window) {
    IMGUI_CHECKVERSION();
    ImGui::CreateContext();

    ImGuiIO& io = ImGui::GetIO();
    io.ConfigFlags |= ImGuiConfigFlags_NavEnableKeyboard;

    // Dark theme with custom tweaks
    ImGui::StyleColorsDark();
    ImGuiStyle& style = ImGui::GetStyle();
    style.WindowRounding   = 0.0f;
    style.FrameRounding    = 4.0f;
    style.GrabRounding     = 3.0f;
    style.WindowBorderSize = 0.0f;
    style.FramePadding     = ImVec2(8.0f, 4.0f);
    style.ItemSpacing      = ImVec2(8.0f, 6.0f);

    // Space-dark palette
    ImVec4* c = style.Colors;
    c[ImGuiCol_WindowBg]        = ImVec4(0.06f, 0.06f, 0.09f, 0.94f);
    c[ImGuiCol_MenuBarBg]       = ImVec4(0.08f, 0.08f, 0.12f, 1.00f);
    c[ImGuiCol_Header]          = ImVec4(0.14f, 0.14f, 0.22f, 1.00f);
    c[ImGuiCol_HeaderHovered]   = ImVec4(0.20f, 0.20f, 0.32f, 1.00f);
    c[ImGuiCol_Button]          = ImVec4(0.16f, 0.16f, 0.26f, 1.00f);
    c[ImGuiCol_ButtonHovered]   = ImVec4(0.24f, 0.24f, 0.38f, 1.00f);
    c[ImGuiCol_ButtonActive]    = ImVec4(0.30f, 0.30f, 0.48f, 1.00f);
    c[ImGuiCol_FrameBg]         = ImVec4(0.10f, 0.10f, 0.16f, 1.00f);
    c[ImGuiCol_FrameBgHovered]  = ImVec4(0.14f, 0.14f, 0.22f, 1.00f);
    c[ImGuiCol_SliderGrab]      = ImVec4(0.31f, 0.76f, 0.97f, 0.80f);
    c[ImGuiCol_SliderGrabActive]= ImVec4(0.31f, 0.76f, 0.97f, 1.00f);
    c[ImGuiCol_Text]            = ImVec4(0.91f, 0.92f, 0.94f, 1.00f);
    c[ImGuiCol_Separator]       = ImVec4(0.20f, 0.20f, 0.30f, 1.00f);

    ImGui_ImplGlfw_InitForOpenGL(window, true);
    ImGui_ImplOpenGL3_Init("#version 450");

    return true;
}

void UIManager::Shutdown() {
    ImGui_ImplOpenGL3_Shutdown();
    ImGui_ImplGlfw_Shutdown();
    ImGui::DestroyContext();
}

void UIManager::BeginFrame() {
    ImGui_ImplOpenGL3_NewFrame();
    ImGui_ImplGlfw_NewFrame();
    ImGui::NewFrame();
}

void UIManager::EndFrame() {
    ImGui::Render();
    ImGui_ImplOpenGL3_RenderDrawData(ImGui::GetDrawData());
}

void UIManager::Render(int windowWidth, int windowHeight) {
    DrawMenuBar();
    DrawControlBar(windowWidth, windowHeight);
}

void UIManager::DrawMenuBar() {
    if (ImGui::BeginMainMenuBar()) {
        if (ImGui::BeginMenu("File")) {
            if (ImGui::MenuItem("New"))   printf("[File] New\n");
            if (ImGui::MenuItem("Open"))  printf("[File] Open\n");
            if (ImGui::MenuItem("Save"))  printf("[File] Save\n");
            ImGui::Separator();
            if (ImGui::MenuItem("Exit"))  _wantsQuit = true;
            ImGui::EndMenu();
        }
        if (ImGui::BeginMenu("Edit")) {
            if (ImGui::MenuItem("Undo"))  printf("[Edit] Undo\n");
            if (ImGui::MenuItem("Redo"))  printf("[Edit] Redo\n");
            ImGui::EndMenu();
        }
        if (ImGui::BeginMenu("View")) {
            if (ImGui::MenuItem("Reset View"))  printf("[View] Reset View\n");
            ImGui::EndMenu();
        }
        if (ImGui::BeginMenu("Help")) {
            if (ImGui::MenuItem("About")) printf("[Help] About\n");
            ImGui::EndMenu();
        }

        _menuBarHeight = ImGui::GetWindowSize().y;
        ImGui::EndMainMenuBar();
    }
}

void UIManager::DrawControlBar(int windowWidth, int windowHeight) {
    float barY = (float)windowHeight - _controlBarHeight;

    ImGui::SetNextWindowPos(ImVec2(0.0f, barY));
    ImGui::SetNextWindowSize(ImVec2((float)windowWidth, _controlBarHeight));

    ImGui::PushStyleColor(ImGuiCol_WindowBg, ImVec4(0.06f, 0.06f, 0.10f, 0.85f));
    ImGui::PushStyleVar(ImGuiStyleVar_WindowRounding, 0.0f);
    ImGui::PushStyleVar(ImGuiStyleVar_WindowBorderSize, 0.0f);
    ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding, ImVec2(12.0f, 8.0f));

    ImGuiWindowFlags flags =
        ImGuiWindowFlags_NoTitleBar   | ImGuiWindowFlags_NoResize   |
        ImGuiWindowFlags_NoMove       | ImGuiWindowFlags_NoCollapse |
        ImGuiWindowFlags_NoScrollbar  | ImGuiWindowFlags_NoSavedSettings |
        ImGuiWindowFlags_NoBringToFrontOnFocus;

    ImGui::Begin("##ControlBar", nullptr, flags);

    float btnW = 80.0f;
    float btnH = 28.0f;

    if (ImGui::Button("Add",      ImVec2(btnW, btnH))) printf("[Action] Add\n");
    ImGui::SameLine();
    if (ImGui::Button("Simulate", ImVec2(btnW, btnH))) printf("[Action] Simulate\n");
    ImGui::SameLine();
    if (ImGui::Button("Edit",     ImVec2(btnW, btnH))) printf("[Action] Edit\n");
    ImGui::SameLine();
    if (ImGui::Button("Analyse",  ImVec2(btnW, btnH))) printf("[Action] Analyse\n");
    ImGui::SameLine();
    if (ImGui::Button("Settings", ImVec2(btnW, btnH))) printf("[Action] Settings\n");
    ImGui::SameLine();

    ImGui::SeparatorEx(ImGuiSeparatorFlags_Vertical);
    ImGui::SameLine();

    ImGui::Text("Time:");
    ImGui::SameLine();
    ImGui::SetNextItemWidth(200.0f);
    ImGui::SliderFloat("##TimeScale", &_timeScale, 0.0f, 10.0f, "%.2f");

    ImGui::End();

    ImGui::PopStyleVar(3);
    ImGui::PopStyleColor();
}
