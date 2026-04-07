using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;
using Silk.NET.OpenGL;

namespace CelestialMechanics.Desktop.Infrastructure;

/// <summary>
/// HwndHost subclass that creates a child HWND with an OpenGL (WGL) context.
/// The GL object can then be used by GLRenderer on a dedicated render thread.
/// </summary>
public class OpenGLHost : HwndHost
{
    private IntPtr _hwnd;
    private IntPtr _hdc;
    private IntPtr _hglrc;
    private GL? _gl;
    private string? _windowClassName;
    private int _pixelWidth;
    private int _pixelHeight;

    public GL? Gl => _gl;
    public IntPtr Hdc => _hdc;
    public IntPtr Hglrc => _hglrc;
    public int PixelWidth => _pixelWidth;
    public int PixelHeight => _pixelHeight;

    public event Action<int, int>? Resized;

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        var hInstance = Win32Interop.GetModuleHandle(null);

        // Register a window class with CS_OWNDC for OpenGL
        _windowClassName = "CelestialGL_" + Guid.NewGuid().ToString("N")[..8];
        _wndProcDelegate = DefaultWndProc; // prevent GC collection of delegate
        var wc = new Win32Interop.WNDCLASSEX
        {
            cbSize = Marshal.SizeOf<Win32Interop.WNDCLASSEX>(),
            style = Win32Interop.CS_OWNDC | Win32Interop.CS_HREDRAW | Win32Interop.CS_VREDRAW,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            hInstance = hInstance,
            lpszClassName = _windowClassName,
        };

        Win32Interop.RegisterClassEx(ref wc);

        // Compute DPI-aware pixel dimensions
        var dpiInfo = VisualTreeHelper.GetDpi(this);
        _pixelWidth = System.Math.Max(1, (int)(ActualWidth * dpiInfo.DpiScaleX));
        _pixelHeight = System.Math.Max(1, (int)(ActualHeight * dpiInfo.DpiScaleY));

        // Create the child window
        _hwnd = Win32Interop.CreateWindowEx(
            0,
            _windowClassName,
            "",
            Win32Interop.WS_CHILD | Win32Interop.WS_VISIBLE | Win32Interop.WS_CLIPSIBLINGS | Win32Interop.WS_CLIPCHILDREN,
            0, 0, _pixelWidth, _pixelHeight,
            hwndParent.Handle,
            IntPtr.Zero,
            hInstance,
            IntPtr.Zero);

        if (_hwnd == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create child HWND for OpenGL viewport.");

        // Get device context
        _hdc = Win32Interop.GetDC(_hwnd);
        if (_hdc == IntPtr.Zero)
            throw new InvalidOperationException("Failed to get device context.");

        // Set pixel format
        var pfd = Win32Interop.PIXELFORMATDESCRIPTOR.Default;
        int pixelFormat = Win32Interop.ChoosePixelFormat(_hdc, ref pfd);
        if (pixelFormat == 0)
            throw new InvalidOperationException("ChoosePixelFormat failed.");
        if (!Win32Interop.SetPixelFormat(_hdc, pixelFormat, ref pfd))
            throw new InvalidOperationException("SetPixelFormat failed.");

        // Create WGL context
        _hglrc = Win32Interop.wglCreateContext(_hdc);
        if (_hglrc == IntPtr.Zero)
            throw new InvalidOperationException("wglCreateContext failed.");

        // Temporarily make current on this thread to build GL API object
        Win32Interop.wglMakeCurrent(_hdc, _hglrc);

        // Build Silk.NET GL API object using our getProcAddress
        var opengl32Handle = Win32Interop.GetModuleHandle("opengl32.dll");
        _gl = GL.GetApi(name =>
        {
            nint addr = Win32Interop.wglGetProcAddress(name);
            if (addr == IntPtr.Zero || addr == (nint)1 || addr == (nint)2 || addr == (nint)3 || addr == (nint)(-1))
                addr = Win32Interop.GetProcAddress(opengl32Handle, name);
            return addr;
        });

        // Release context from UI thread - the render thread will take over
        Win32Interop.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);

        // Subscribe to size changes
        SizeChanged += OnHostSizeChanged;

        return new HandleRef(this, _hwnd);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        SizeChanged -= OnHostSizeChanged;

        if (_hglrc != IntPtr.Zero)
        {
            Win32Interop.wglDeleteContext(_hglrc);
            _hglrc = IntPtr.Zero;
        }

        if (_hdc != IntPtr.Zero && _hwnd != IntPtr.Zero)
        {
            Win32Interop.ReleaseDC(_hwnd, _hdc);
            _hdc = IntPtr.Zero;
        }

        if (_hwnd != IntPtr.Zero)
        {
            Win32Interop.DestroyWindow(_hwnd);
            _hwnd = IntPtr.Zero;
        }

        if (_windowClassName != null)
        {
            Win32Interop.UnregisterClass(_windowClassName, Win32Interop.GetModuleHandle(null));
            _windowClassName = null;
        }

        _gl?.Dispose();
        _gl = null;
    }

    private void OnHostSizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
    {
        var dpiInfo = VisualTreeHelper.GetDpi(this);
        _pixelWidth = System.Math.Max(1, (int)(e.NewSize.Width * dpiInfo.DpiScaleX));
        _pixelHeight = System.Math.Max(1, (int)(e.NewSize.Height * dpiInfo.DpiScaleY));
        Resized?.Invoke(_pixelWidth, _pixelHeight);
    }

    // Store delegate to prevent GC collection
    private WndProcDelegate? _wndProcDelegate;

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private static IntPtr DefaultWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        // Forward mouse messages to the WPF parent so that WPF routed events
        // (MouseDown, MouseMove, MouseWheel, etc.) fire on the ViewportPanel.
        // Without this, the native child HWND consumes all mouse input silently.
        switch (msg)
        {
            case Win32Interop.WM_MOUSEMOVE:
            case Win32Interop.WM_LBUTTONDOWN:
            case Win32Interop.WM_LBUTTONUP:
            case Win32Interop.WM_LBUTTONDBLCLK:
            case Win32Interop.WM_RBUTTONDOWN:
            case Win32Interop.WM_RBUTTONUP:
            case Win32Interop.WM_MBUTTONDOWN:
            case Win32Interop.WM_MBUTTONUP:
                var parentHwnd = Win32Interop.GetParent(hWnd);
                if (parentHwnd != IntPtr.Zero)
                    Win32Interop.PostMessageA(parentHwnd, msg, wParam, lParam);
                return IntPtr.Zero;

            case Win32Interop.WM_MOUSEWHEEL:
                // Mouse wheel messages use screen coordinates; forward as-is
                var parentHwnd2 = Win32Interop.GetParent(hWnd);
                if (parentHwnd2 != IntPtr.Zero)
                    Win32Interop.PostMessageA(parentHwnd2, msg, wParam, lParam);
                return IntPtr.Zero;
        }

        return Win32Interop.DefWindowProcA(hWnd, msg, wParam, lParam);
    }
}
