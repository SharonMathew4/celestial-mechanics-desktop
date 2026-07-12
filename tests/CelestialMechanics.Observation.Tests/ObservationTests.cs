using CelestialMechanics.Math;
using CelestialMechanics.Observation.Catalog;
using CelestialMechanics.Observation.Core;
using CelestialMechanics.Observation.Database;
using CelestialMechanics.Observation.Scene;
using CelestialMechanics.Observation.Services;
using CelestialMechanics.Observation.World;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace CelestialMechanics.Observation.Tests;

public sealed class ObservationTests
{
    [Fact]
    public void TestEquatorialToCartesian_SimpleAngles()
    {
        // RA = 0, Dec = 0, Dist = 10 => X = 10, Y = 0, Z = 0
        var pos1 = CoordinateTransforms.EquatorialToCartesian(0.0, 0.0, 10.0);
        Assert.Equal(10.0, pos1.X, 5);
        Assert.Equal(0.0, pos1.Y, 5);
        Assert.Equal(0.0, pos1.Z, 5);

        // RA = 90, Dec = 0, Dist = 10 => X = 0, Y = 10, Z = 0
        var pos2 = CoordinateTransforms.EquatorialToCartesian(90.0, 0.0, 10.0);
        Assert.Equal(0.0, pos2.X, 5);
        Assert.Equal(10.0, pos2.Y, 5);
        Assert.Equal(0.0, pos2.Z, 5);

        // RA = 0, Dec = 90, Dist = 10 => X = 0, Y = 0, Z = 10
        var pos3 = CoordinateTransforms.EquatorialToCartesian(0.0, 90.0, 10.0);
        Assert.Equal(0.0, pos3.X, 5);
        Assert.Equal(0.0, pos3.Y, 5);
        Assert.Equal(10.0, pos3.Z, 5);
    }

    [Fact]
    public void TestSexagesimalToCartesian()
    {
        var pos = CoordinateTransforms.EquatorialToCartesian(6.0, 0.0, 0.0, 30.0, 0.0, 0.0, 100.0);
        
        var expectedRaRad = 90.0 * System.Math.PI / 180.0;
        var expectedDecRad = 30.0 * System.Math.PI / 180.0;
        var expectedX = 100.0 * System.Math.Cos(expectedDecRad) * System.Math.Cos(expectedRaRad);
        var expectedY = 100.0 * System.Math.Cos(expectedDecRad) * System.Math.Sin(expectedRaRad);
        var expectedZ = 100.0 * System.Math.Sin(expectedDecRad);

        Assert.Equal(expectedX, pos.X, 5);
        Assert.Equal(expectedY, pos.Y, 5);
        Assert.Equal(expectedZ, pos.Z, 5);
    }

    [Fact]
    public async Task TestCatalogBinaryReaderWriter()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var stars = new[]
            {
                new StarEntry(1, 45.0, 10.0, 100f, 1.2f, 10f, -5f, "G2V"),
                new StarEntry(2, 120.0, -45.0, 50f, 4.5f, -2f, 15f, "A0Iab")
            };

            HipparcosBinaryReader.WriteCatalog(tempFile, stars);
            var readStars = HipparcosBinaryReader.ReadCatalog(tempFile);

            Assert.Equal(2, readStars.Count);

            Assert.Equal(1, readStars[0].Id);
            Assert.Equal("G2V", readStars[0].SpectralType);
            Assert.Equal(2, readStars[1].Id);
            Assert.Equal("A0Iab", readStars[1].SpectralType);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task TestObservationCatalogLoad()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var stars = new[]
            {
                new StarEntry(101, 10.0, 20.0, 10f, 5.0f, 0f, 0f, "M")
            };

            HipparcosBinaryReader.WriteCatalog(tempFile, stars);

            var catalog = new ObservationCatalog(tempFile);
            Assert.False(catalog.IsLoaded);

            await catalog.LoadAsync();
            Assert.True(catalog.IsLoaded);
            Assert.Equal(1, catalog.ObjectCount);
            Assert.Equal(101, catalog.Stars[0].Id);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void TestSceneGraphHierarchy()
    {
        var parent = new SceneNode("parent", "Parent Node");
        var child = new SceneNode("child", "Child Node");

        parent.AddChild(child);

        Assert.Equal(1, parent.Children.Count);
        Assert.Same(parent, child.Parent);

        child.Transform.Position = new Vec3d(1, 2, 3);
        Assert.Equal(1.0, child.Transform.Position.X);

        var manager = new SceneManager();
        manager.Root.AddChild(parent);

        var found = manager.FindNode("child");
        Assert.Same(child, found);

        parent.RemoveChild(child);
        Assert.Empty(parent.Children);
        Assert.Null(child.Parent);
    }

    [Fact]
    public void TestWorldSystem()
    {
        var manager = new WorldManager();
        var universe = manager.ActiveUniverse;
        
        Assert.NotNull(universe.Settings);
        Assert.Equal(1.0, universe.Settings.BaseScale);

        var sector = new Sector("sector1", new Vec3d(0, 0, 0), 100.0);
        universe.AddSector(sector);

        Assert.True(universe.Sectors.ContainsKey("sector1"));

        sector.AddNode("node1");
        Assert.Contains("node1", sector.LoadedNodeIds);

        manager.SetObjectVisibility("node1", true);
        Assert.Contains("node1", manager.VisibleObjectIds);

        manager.SetObjectVisibility("node1", false);
        Assert.Empty(manager.VisibleObjectIds);
    }

    [Fact]
    public async Task TestDatabaseInfrastructureInMemory()
    {
        using var dbService = new DatabaseService();
        await dbService.ConnectAsync(":memory:");
        
        Assert.True(dbService.IsConnected);

        await DatabaseInitializer.InitializeSchemaAsync(dbService);

        var conn = dbService.GetConnection();
        using var cmd = new SqliteCommand("SELECT name FROM sqlite_master WHERE type='table';", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        
        var tables = new System.Collections.Generic.List<string>();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        Assert.Contains("Stars", tables);
        Assert.Contains("Annotations", tables);

        await dbService.DisconnectAsync();
        Assert.False(dbService.IsConnected);
    }

    [Fact]
    public void TestCatalogProviders()
    {
        var bootstrap = new ObservationBootstrap();
        bootstrap.Initialize();

        var catalogService = bootstrap.ServiceProvider.GetRequiredService<ICatalogService>();
        Assert.NotNull(catalogService);
        Assert.Equal(5, catalogService.Providers.Count);

        Assert.Contains(catalogService.Providers, p => p.Name == "Stars");
        Assert.Contains(catalogService.Providers, p => p.Name == "Planets");
    }

    [Fact]
    public void TestCameraControlsAndDI()
    {
        var bootstrap = new ObservationBootstrap();
        var camera = bootstrap.ServiceProvider.GetRequiredService<ICameraService>();
        
        Assert.NotNull(camera);
        Assert.Equal(50.0f, camera.Distance);

        // Test strafe vertical
        camera.MoveVertical(1.0f, 0.1f);
        // Target should be shifted up
        Assert.True(camera.Target.Y > 0.0f);
    }
}
