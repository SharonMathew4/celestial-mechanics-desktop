cmake_minimum_required(VERSION 3.20)
project(CelestialIDE VERSION 0.1.0 LANGUAGES C CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# ==== Build Instructions ====
# Prerequisites: CMake 3.20+, C++17 compiler, Git
# Dependencies (GLFW, ImGui) are fetched automatically.
#
#   mkdir build && cd build
#   cmake ..
#   cmake --build . --config Release
#   ./Release/CelestialIDE        (Windows)
#   ./CelestialIDE                (Linux/macOS)

include(FetchContent)

# --- GLFW ---
set(GLFW_BUILD_DOCS OFF CACHE BOOL "" FORCE)
set(GLFW_BUILD_TESTS OFF CACHE BOOL "" FORCE)
set(GLFW_BUILD_EXAMPLES OFF CACHE BOOL "" FORCE)
FetchContent_Declare(glfw
    GIT_REPOSITORY https://github.com/glfw/glfw.git
    GIT_TAG        3.4
    GIT_SHALLOW    TRUE)
FetchContent_MakeAvailable(glfw)

# --- Dear ImGui ---
FetchContent_Declare(imgui
    GIT_REPOSITORY https://github.com/ocornut/imgui.git
    GIT_TAG        v1.91.6
    GIT_SHALLOW    TRUE)
FetchContent_MakeAvailable(imgui)

add_library(imgui_lib STATIC
    ${imgui_SOURCE_DIR}/imgui.cpp
    ${imgui_SOURCE_DIR}/imgui_draw.cpp
    ${imgui_SOURCE_DIR}/imgui_tables.cpp
    ${imgui_SOURCE_DIR}/imgui_widgets.cpp
    ${imgui_SOURCE_DIR}/backends/imgui_impl_glfw.cpp
    ${imgui_SOURCE_DIR}/backends/imgui_impl_opengl3.cpp)
target_include_directories(imgui_lib PUBLIC
    ${imgui_SOURCE_DIR}
    ${imgui_SOURCE_DIR}/backends)
target_link_libraries(imgui_lib PUBLIC glfw)

# --- Application ---
add_executable(CelestialIDE
    src/main.cpp
    src/GLLoader.cpp
    src/Application.cpp
    src/Renderer.cpp
    src/Framebuffer.cpp
    src/UIManager.cpp
    src/SimulationCore.cpp)
target_include_directories(CelestialIDE PRIVATE src)
target_link_libraries(CelestialIDE PRIVATE imgui_lib glfw)
