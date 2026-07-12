using System;
using System.Collections.Generic;

namespace CelestialMechanics.Observation.Resources;

/// <summary>
/// Simple in-memory cache representation for central resources.
/// </summary>
public sealed class AssetCache<T>
{
    private readonly Dictionary<string, T> _cache = new(StringComparer.OrdinalIgnoreCase);

    public bool TryGet(string key, out T? asset)
    {
        if (_cache.TryGetValue(key, out var val))
        {
            asset = val;
            return true;
        }
        asset = default;
        return false;
    }

    public void Add(string key, T asset)
    {
        _cache[key] = asset;
    }

    public void Clear()
    {
        _cache.Clear();
    }
}

/// <summary>
/// Centralized manager for Shader resources.
/// </summary>
public sealed class ShaderManager
{
    private readonly AssetCache<string> _shaderCache = new();

    public string LoadShaderSource(string name, string rawSource)
    {
        if (_shaderCache.TryGet(name, out var source))
        {
            return source!;
        }

        _shaderCache.Add(name, rawSource);
        return rawSource;
    }
}

/// <summary>
/// Centralized manager for Texture resources.
/// </summary>
public sealed class TextureManager
{
    private readonly AssetCache<object> _textureCache = new();

    public object LoadTexture(string path)
    {
        if (_textureCache.TryGet(path, out var tex))
        {
            return tex!;
        }

        // Simulating loading a GL texture
        var newTex = new object();
        _textureCache.Add(path, newTex);
        return newTex;
    }
}

/// <summary>
/// Centralized manager for Mesh resources.
/// </summary>
public sealed class MeshManager
{
    private readonly AssetCache<object> _meshCache = new();

    public object LoadMesh(string key, float[] vertices)
    {
        if (_meshCache.TryGet(key, out var mesh))
        {
            return mesh!;
        }

        var newMesh = new object();
        _meshCache.Add(key, newMesh);
        return newMesh;
    }
}

/// <summary>
/// Centralized manager for Material properties.
/// </summary>
public sealed class MaterialManager
{
    private readonly AssetCache<object> _materialCache = new();

    public object LoadMaterial(string key)
    {
        if (_materialCache.TryGet(key, out var mat))
        {
            return mat!;
        }

        var newMat = new object();
        _materialCache.Add(key, newMat);
        return newMat;
    }
}

/// <summary>
/// Centralized manager for font glyph cache.
/// </summary>
public sealed class FontManager
{
    private readonly AssetCache<object> _fontCache = new();

    public object LoadFont(string name, int size)
    {
        var key = $"{name}_{size}";
        if (_fontCache.TryGet(key, out var font))
        {
            return font!;
        }

        var newFont = new object();
        _fontCache.Add(key, newFont);
        return newFont;
    }
}

/// <summary>
/// High-level resource loader orchestrator.
/// </summary>
public sealed class ResourceLoader
{
    public ShaderManager Shaders { get; } = new();
    public TextureManager Textures { get; } = new();
    public MeshManager Meshes { get; } = new();
    public MaterialManager Materials { get; } = new();
    public FontManager Fonts { get; } = new();
}
