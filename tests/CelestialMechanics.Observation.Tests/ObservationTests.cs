using CelestialMechanics.Math;
using CelestialMechanics.Observation.Catalog;
using CelestialMechanics.Observation.Core;
using CelestialMechanics.Observation.Database;
using CelestialMechanics.Observation.Import;
using CelestialMechanics.Observation.Rendering;
using CelestialMechanics.Observation.Resources;
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

        Assert.Single(parent.Children);
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

        Assert.Contains("AstronomicalObjects", tables);
        Assert.Contains("CatalogReferences", tables);
        Assert.Contains("StellarMetadata", tables);
        Assert.Contains("Annotations", tables);

        await dbService.DisconnectAsync();
        Assert.False(dbService.IsConnected);
    }

    [Fact]
    public async Task TestHipparcosImporterPipeline()
    {
        using var dbService = new DatabaseService();
        await dbService.ConnectAsync(":memory:");
        await DatabaseInitializer.InitializeSchemaAsync(dbService);

        var repo = new AstronomicalObjectRepository(dbService);
        var importer = new HipparcosImporter(repo);

        var tempFile = Path.GetTempFileName();
        try
        {
            // Write 3 entries: 2 valid, 1 invalid (out of bounds Dec)
            var stars = new[]
            {
                new StarEntry(201, 120.0, 10.0, 50f, 1.2f, 0.5f, -0.2f, "G2"),
                new StarEntry(202, 380.0, -45.0, 10f, 5.0f, 0f, 0f, "M0"), // Invalid RA > 360
                new StarEntry(203, 90.0, -10.0, 100f, 3.4f, -1.0f, 1.5f, "A0")
            };
            HipparcosBinaryReader.WriteCatalog(tempFile, stars);

            var dataSource = new FileDataSource(tempFile);
            var settings = new ImportSettings { BatchSize = 10 };
            
            long progressReportedCount = 0;
            var job = new ImportJob("Hipparcos", dataSource, settings, p =>
            {
                progressReportedCount = p.ObjectsImported;
            });

            var result = await importer.ImportAsync(job);

            Assert.True(result.Success);
            Assert.Equal(2, result.ImportedCount);
            Assert.Equal(1, result.SkippedCount);
            Assert.Single(result.Errors);
            Assert.Equal(2, progressReportedCount);

            // Verify they are loaded into SQLite
            var dbStars = await repo.GetStarsAsync("Hipparcos");
            Assert.Equal(2, dbStars.Count);
            Assert.Contains(dbStars, s => s.Id == 201);
            Assert.Contains(dbStars, s => s.Id == 203);
            Assert.DoesNotContain(dbStars, s => s.Id == 202);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task TestImportManagerAndStarProvider()
    {
        var bootstrap = new ObservationBootstrap();
        bootstrap.Initialize();

        var dbService = bootstrap.ServiceProvider.GetRequiredService<DatabaseService>();
        // Reconnect to in-memory to isolate tests from local files
        await dbService.DisconnectAsync();
        await dbService.ConnectAsync(":memory:");
        await DatabaseInitializer.InitializeSchemaAsync(dbService);

        var importManager = bootstrap.ServiceProvider.GetRequiredService<ImportManager>();
        var repo = bootstrap.ServiceProvider.GetRequiredService<AstronomicalObjectRepository>();

        var tempFile = Path.GetTempFileName();
        try
        {
            var stars = new[]
            {
                new StarEntry(501, 10.0, -20.0, 100f, 6.0f, 0.1f, -0.1f, "K")
            };
            HipparcosBinaryReader.WriteCatalog(tempFile, stars);

            var job = new ImportJob("Hipparcos", new FileDataSource(tempFile), new ImportSettings());
            var result = await importManager.RunImportAsync(job);

            Assert.True(result.Success);
            Assert.Equal(1, result.ImportedCount);

            // Retrieve provider and load
            var starProvider = bootstrap.ServiceProvider.GetRequiredService<StarProvider>();
            Assert.False(starProvider.IsLoaded);

            await starProvider.LoadAsync();
            Assert.True(starProvider.IsLoaded);
            Assert.Single(starProvider.Stars);
            Assert.Equal(501, starProvider.Stars[0].Id);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
            
            bootstrap.Shutdown();
        }
    }

    [Fact]
    public void TestRenderQueueCategorization()
    {
        var queue = new RenderQueue();
        
        var node1 = new SceneNode("node1", "Node 1") { NodeType = "Star" };
        var node2 = new SceneNode("node2", "Node 2") { NodeType = "Planet" };
        var node3 = new SceneNode("node3", "Node 3") { NodeType = "Star" };

        queue.Enqueue(node1);
        queue.Enqueue(node2);
        queue.Enqueue(node3);

        Assert.True(queue.CategorizedNodes.ContainsKey("Star"));
        Assert.True(queue.CategorizedNodes.ContainsKey("Planet"));
        Assert.Equal(2, queue.CategorizedNodes["Star"].Count);
        Assert.Single(queue.CategorizedNodes["Planet"]);

        queue.Clear();
        Assert.Empty(queue.CategorizedNodes);
    }

    [Fact]
    public void TestResourceLoaderCaching()
    {
        var loader = new ResourceLoader();
        
        // Test shaders caching
        var source1 = loader.Shaders.LoadShaderSource("ColorShader", "void main() {}");
        var source2 = loader.Shaders.LoadShaderSource("ColorShader", "void main() { color = red; }");
        
        Assert.Same(source1, source2);
        Assert.Equal("void main() {}", source2);

        // Test textures caching
        var tex1 = loader.Textures.LoadTexture("Assets/nebula.png");
        var tex2 = loader.Textures.LoadTexture("Assets/nebula.png");

        Assert.Same(tex1, tex2);
    }

    [Fact]
    public void TestRaycastingAndPicking()
    {
        var sceneManager = new SceneManager();
        var picker = new ScenePicker(sceneManager);

        var starNode = new SceneNode("sirius", "Sirius") { NodeType = "Star" };
        starNode.Transform.Position = new Vec3d(0, 0, 50); // Set along Z axis
        sceneManager.Root.AddChild(starNode);

        // Ray pointing straight along Z axis
        var pickingRay = new Ray(new Vec3d(0, 0, 0), new Vec3d(0, 0, 1));
        var picked = picker.PickNode(pickingRay, boundingSphereRadius: 2.0);

        Assert.NotNull(picked);
        Assert.Same(starNode, picked);

        // Ray pointing away (along X axis) should miss
        var missingRay = new Ray(new Vec3d(0, 0, 0), new Vec3d(1, 0, 0));
        var missed = picker.PickNode(missingRay, boundingSphereRadius: 2.0);
        Assert.Null(missed);
    }

    [Fact]
    public void TestGridSettingsAndAbstractions()
    {
        var settings = new RenderSettings();
        Assert.True(settings.ShowGrid);
        Assert.Equal("Equatorial", settings.ActiveGridType);

        settings.ActiveGridType = "Galactic";
        Assert.Equal("Galactic", settings.ActiveGridType);

        var context = new RenderContext
        {
            DeltaTime = 0.016f,
            AspectRatio = 1.6f
        };
        Assert.Equal(0.016f, context.DeltaTime);
        Assert.Equal(1.6f, context.AspectRatio);
    }
}
