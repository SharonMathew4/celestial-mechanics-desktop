using System.Runtime.InteropServices;

namespace CelestialMechanics.Desktop.Infrastructure;

/// <summary>
/// P/Invoke declarations for Win32 APIs needed for OpenGL context creation.
/// </summary>
internal static partial class Win32Interop
{
    // ── Window Styles ──────────────────────────────────────────────────
    public const int WS_CHILD = 0x40000000;
    public const int WS_VISIBLE = 0x10000000;
    public const int WS_CLIPCHILDREN = 0x02000000;
    public const int WS_CLIPSIBLINGS = 0x04000000;

    // ── Window Class Styles ────────────────────────────────────────────
    public const int CS_OWNDC = 0x0020;
    public const int CS_HREDRAW = 0x0002;
    public const int CS_VREDRAW = 0x0001;

    // ── Window Messages ────────────────────────────────────────────────
    public const int WM_SIZE = 0x0005;

    // ── Pixel Format Descriptor flags ──────────────────────────────────
    public const uint PFD_DRAW_TO_WINDOW = 0x00000004;
    public const uint PFD_SUPPORT_OPENGL = 0x00000020;
    public const uint PFD_DOUBLEBUFFER = 0x00000001;
    public const byte PFD_TYPE_RGBA = 0;
    public const byte PFD_MAIN_PLANE = 0;

    // ── Structures ─────────────────────────────────────────────────────

    [StructLayout(LayoutKind.Sequential)]
    public struct PIXELFORMATDESCRIPTOR
    {
        public ushort nSize;
        public ushort nVersion;
        public uint dwFlags;
        public byte iPixelType;
        public byte cColorBits;
        public byte cRedBits;
        public byte cRedShift;
        public byte cGreenBits;
        public byte cGreenShift;
        public byte cBlueBits;
        public byte cBlueShift;
        public byte cAlphaBits;
        public byte cAlphaShift;
        public byte cAccumBits;
        public byte cAccumRedBits;
        public byte cAccumGreenBits;
        public byte cAccumBlueBits;
        public byte cAccumAlphaBits;
        public byte cDepthBits;
        public byte cStencilBits;
        public byte cAuxBuffers;
        public byte iLayerType;
        public byte bReserved;
        public uint dwLayerMask;
        public uint dwVisibleMask;
        public uint dwDamageMask;

        public static PIXELFORMATDESCRIPTOR Default => new()
        {
            nSize = (ushort)Marshal.SizeOf<PIXELFORMATDESCRIPTOR>(),
            nVersion = 1,
            dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER,
            iPixelType = PFD_TYPE_RGBA,
            cColorBits = 32,
            cDepthBits = 24,
            cStencilBits = 8,
            iLayerType = PFD_MAIN_PLANE,
        };
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct WNDCLASSEX
    {
        public int cbSize;
        public int style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string? lpszMenuName;
        public string lpszClassName;
        public IntPtr hIconSm;
    }

    // ── user32.dll ─────────────────────────────────────────────────────

    [LibraryImport("user32.dll", EntryPoint = "CreateWindowExA", StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr CreateWindowEx(
        int dwExStyle, string lpClassName, string lpWindowName, int dwStyle,
        int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyWindow(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    public static partial IntPtr GetDC(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    public static partial int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [LibraryImport("user32.dll")]
    public static partial IntPtr DefWindowProcA(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    // Using DllImport for RegisterClassEx because WNDCLASSEX has string fields
    // that the LibraryImport source generator cannot marshal automatically.
    [DllImport("user32.dll", EntryPoint = "RegisterClassExA", CharSet = CharSet.Ansi)]
    public static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);

    [DllImport("user32.dll", EntryPoint = "UnregisterClassA", CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);

    // ── gdi32.dll ──────────────────────────────────────────────────────

    [LibraryImport("gdi32.dll")]
    public static partial int ChoosePixelFormat(IntPtr hdc, ref PIXELFORMATDESCRIPTOR ppfd);

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetPixelFormat(IntPtr hdc, int iPixelFormat, ref PIXELFORMATDESCRIPTOR ppfd);

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SwapBuffers(IntPtr hdc);

    // ── opengl32.dll ───────────────────────────────────────────────────

    [LibraryImport("opengl32.dll")]
    public static partial IntPtr wglCreateContext(IntPtr hdc);

    [LibraryImport("opengl32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool wglDeleteContext(IntPtr hglrc);

    [LibraryImport("opengl32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);

    [LibraryImport("opengl32.dll", EntryPoint = "wglGetProcAddress", StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr wglGetProcAddress(string lpszProc);

    // ── kernel32.dll ───────────────────────────────────────────────────

    [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleA", StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr GetModuleHandle(string? lpModuleName);

    [LibraryImport("kernel32.dll", EntryPoint = "GetProcAddress", StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
}
