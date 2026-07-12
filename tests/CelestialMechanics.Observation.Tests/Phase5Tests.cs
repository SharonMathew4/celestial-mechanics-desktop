using CelestialMechanics.Math;
using CelestialMechanics.Observation.Camera;
using CelestialMechanics.Observation.Coordinates;
using CelestialMechanics.Observation.Events;
using CelestialMechanics.Observation.Objects;
using CelestialMechanics.Observation.Scene;
using CelestialMechanics.Observation.Search;
using CelestialMechanics.Observation.Selection;
using CelestialMechanics.Observation.Time;
using CelestialMechanics.Observation.Universe;
using Xunit;

namespace CelestialMechanics.Observation.Tests;

// ═══════════════════════════════════════════════════════════════════════
// Universe Manager Tests
// ═══════════════════════════════════════════════════════════════════════

public sealed class UniverseManagerTests
{
    private static (UniverseManager manager, EventBus bus, UniverseHierarchy hierarchy) Create()
    {
        var bus = new EventBus();
        var hierarchy = new UniverseHierarchy();
        var manager = new UniverseManager(hierarchy, bus);
        manager.Initialize();
        return (manager, bus, hierarchy);
    }

    [Fact]
    public void Initialize_SetsActiveState()
    {
        var (manager, _, _) = Create();
        Assert.Equal(UniverseState.Active, manager.State);
    }

    [Fact]
    public void Register_AddsCelestialBody()
    {
        var (manager, _, _) = Create();
        var star = new Star("sun", "Sun");
        manager.Register(star);

        Assert.Equal(1, manager.Count);
        Assert.Same(star, manager.GetById("sun"));
    }

    [Fact]
    public void Register_DuplicateId_Throws()
    {
        var (manager, _, _) = Create();
        var star1 = new Star("star1", "Star One");
        var star2 = new Star("star1", "Star Two");

        manager.Register(star1);
        Assert.Throws<InvalidOperationException>(() => manager.Register(star2));
    }

    [Fact]
    public void Register_PublishesObjectCreatedEvent()
    {
        var (manager, bus, _) = Create();
        CelestialBody? received = null;
        bus.Subscribe(UniverseEvent.ObjectCreated, e => received = e.Payload as CelestialBody);

        var planet = new Planet("earth", "Earth");
        manager.Register(planet);

        Assert.Same(planet, received);
    }

    [Fact]
    public void Remove_RemovesCelestialBody()
    {
        var (manager, _, _) = Create();
        var star = new Star("vega", "Vega");
        manager.Register(star);

        bool result = manager.Remove("vega");

        Assert.True(result);
        Assert.Equal(0, manager.Count);
        Assert.Null(manager.GetById("vega"));
    }

    [Fact]
    public void Remove_PublishesObjectRemovedEvent()
    {
        var (manager, bus, _) = Create();
        CelestialBody? removed = null;
        bus.Subscribe(UniverseEvent.ObjectRemoved, e => removed = e.Payload as CelestialBody);

        var star = new Star("sirius", "Sirius");
        manager.Register(star);
        manager.Remove("sirius");

        Assert.Same(star, removed);
    }

    [Fact]
    public void Remove_NonExistent_ReturnsFalse()
    {
        var (manager, _, _) = Create();
        Assert.False(manager.Remove("nonexistent"));
    }

    [Fact]
    public void GetByType_ReturnsCorrectBodies()
    {
        var (manager, _, _) = Create();
        manager.Register(new Star("s1", "Star1"));
        manager.Register(new Star("s2", "Star2"));
        manager.Register(new Planet("p1", "Planet1"));

        var stars = manager.GetByType(CelestialBodyType.Star);
        var planets = manager.GetByType(CelestialBodyType.Planet);
        var moons = manager.GetByType(CelestialBodyType.Moon);

        Assert.Equal(2, stars.Count);
        Assert.Single(planets);
        Assert.Empty(moons);
    }

    [Fact]
    public void GetAll_ReturnsAllRegistered()
    {
        var (manager, _, _) = Create();
        manager.Register(new Star("s1", "Star1"));
        manager.Register(new Planet("p1", "Planet1"));
        manager.Register(new Moon("m1", "Moon1"));

        Assert.Equal(3, manager.GetAll().Count);
    }

    [Fact]
    public void SyncToSceneGraph_CreatesNodes()
    {
        var (manager, _, _) = Create();
        var sceneManager = new SceneManager();

        var star = new Star("sun", "Sun");
        star.Position = new Vec3d(10, 20, 30);
        manager.Register(star);

        manager.SyncToSceneGraph(sceneManager);

        var node = sceneManager.FindNode("sun");
        Assert.NotNull(node);
        Assert.Equal("Sun", node!.Name);
        Assert.Equal("Star", node.NodeType);
        Assert.Equal(10.0, node.Transform.Position.X);
    }

    [Fact]
    public void Shutdown_ClearsAllBodies()
    {
        var (manager, _, _) = Create();
        manager.Register(new Star("s1", "Star1"));
        manager.Register(new Star("s2", "Star2"));

        manager.Shutdown();

        Assert.Equal(0, manager.Count);
        Assert.Equal(UniverseState.Uninitialized, manager.State);
    }
}

// ═══════════════════════════════════════════════════════════════════════
// Celestial Body Factory Tests
// ═══════════════════════════════════════════════════════════════════════

public sealed class CelestialBodyFactoryTests
{
    private static (CelestialBodyFactory factory, UniverseManager manager, SceneManager scene) Create()
    {
        var bus = new EventBus();
        var hierarchy = new UniverseHierarchy();
        var universe = new UniverseManager(hierarchy, bus);
        universe.Initialize();
        var scene = new SceneManager();
        var factory = new CelestialBodyFactory(universe, scene, bus);
        return (factory, universe, scene);
    }

    [Fact]
    public void CreateStar_RegistersAndCreatesNode()
    {
        var (factory, manager, scene) = Create();
        var star = factory.CreateStar("Betelgeuse", "bet");

        Assert.NotNull(star);
        Assert.Equal("Betelgeuse", star.Name);
        Assert.Equal(CelestialBodyType.Star, star.ObjectType);
        Assert.Same(star, manager.GetById("bet"));
        Assert.NotNull(scene.FindNode("bet"));
    }

    [Fact]
    public void CreatePlanet_RegistersAndCreatesNode()
    {
        var (factory, manager, scene) = Create();
        var planet = factory.CreatePlanet("Mars", "mars");

        Assert.Equal(CelestialBodyType.Planet, planet.ObjectType);
        Assert.Same(planet, manager.GetById("mars"));
        Assert.NotNull(scene.FindNode("mars"));
    }

    [Fact]
    public void CreateMoon_RegistersAndCreatesNode()
    {
        var (factory, manager, scene) = Create();
        var moon = factory.CreateMoon("Europa", "europa");

        Assert.Equal(CelestialBodyType.Moon, moon.ObjectType);
        Assert.Same(moon, manager.GetById("europa"));
    }

    [Fact]
    public void CreateAsteroid_RegistersAndCreatesNode()
    {
        var (factory, manager, _) = Create();
        var asteroid = factory.CreateAsteroid("Ceres", "ceres");

        Assert.Equal(CelestialBodyType.Asteroid, asteroid.ObjectType);
        Assert.Same(asteroid, manager.GetById("ceres"));
    }

    [Fact]
    public void CreateComet_RegistersAndCreatesNode()
    {
        var (factory, manager, _) = Create();
        var comet = factory.CreateComet("Halley", "halley");

        Assert.Equal(CelestialBodyType.Comet, comet.ObjectType);
        Assert.Same(comet, manager.GetById("halley"));
    }

    [Fact]
    public void CreateGalaxy_RegistersAndCreatesNode()
    {
        var (factory, manager, _) = Create();
        var galaxy = factory.CreateGalaxy("Andromeda", "m31");

        Assert.Equal(CelestialBodyType.Galaxy, galaxy.ObjectType);
        Assert.Same(galaxy, manager.GetById("m31"));
    }

    [Fact]
    public void CreateNebula_RegistersAndCreatesNode()
    {
        var (factory, manager, _) = Create();
        var nebula = factory.CreateNebula("Orion Nebula", "m42");

        Assert.Equal(CelestialBodyType.Nebula, nebula.ObjectType);
        Assert.Same(nebula, manager.GetById("m42"));
    }

    [Fact]
    public void CreateBlackHole_RegistersAndCreatesNode()
    {
        var (factory, manager, _) = Create();
        var bh = factory.CreateBlackHole("Sagittarius A*", "sgra");

        Assert.Equal(CelestialBodyType.BlackHole, bh.ObjectType);
        Assert.Same(bh, manager.GetById("sgra"));
    }

    [Fact]
    public void CreateSpacecraft_RegistersAndCreatesNode()
    {
        var (factory, manager, _) = Create();
        var sc = factory.CreateSpacecraft("Voyager 1", "v1");

        Assert.Equal(CelestialBodyType.Spacecraft, sc.ObjectType);
        Assert.Same(sc, manager.GetById("v1"));
    }

    [Fact]
    public void CreateStar_AutoGeneratesId_WhenNull()
    {
        var (factory, manager, _) = Create();
        var star = factory.CreateStar("Polaris");

        Assert.NotNull(star.Id);
        Assert.NotEmpty(star.Id);
        Assert.Same(star, manager.GetById(star.Id));
    }

    [Fact]
    public void CreateStar_SceneNodeType_MatchesBodyType()
    {
        var (factory, _, scene) = Create();
        var star = factory.CreateStar("Rigel", "rigel");

        var node = scene.FindNode("rigel");
        Assert.NotNull(node);
        Assert.Equal("Star", node!.NodeType);
    }
}

// ═══════════════════════════════════════════════════════════════════════
// Time Manager Tests
// ═══════════════════════════════════════════════════════════════════════

public sealed class TimeManagerTests
{
    [Fact]
    public void DefaultState_IsJ2000_NotPlaying()
    {
        var bus = new EventBus();
        var tm = new TimeManager(bus);

        Assert.Equal(2451545.0, tm.CurrentJulianDate);
        Assert.False(tm.IsPlaying);
        Assert.False(tm.IsReversed);
    }

    [Fact]
    public void Play_SetsIsPlaying()
    {
        var bus = new EventBus();
        var tm = new TimeManager(bus);

        tm.Play();
        Assert.True(tm.IsPlaying);
        Assert.False(tm.IsReversed);
    }

    [Fact]
    public void Pause_StopsPlaying()
    {
        var bus = new EventBus();
        var tm = new TimeManager(bus);

        tm.Play();
        tm.Pause();
        Assert.False(tm.IsPlaying);
    }

    [Fact]
    public void Reverse_SetsReverseAndPlaying()
    {
        var bus = new EventBus();
        var tm = new TimeManager(bus);

        tm.Reverse();
        Assert.True(tm.IsPlaying);
        Assert.True(tm.IsReversed);
    }

    [Fact]
    public void Tick_AdvancesTime_WhenPlaying()
    {
        var bus = new EventBus();
        var tm = new TimeManager(bus);

        tm.Play();
        tm.TimeScale = 86400.0; // 1 day per second
        double before = tm.CurrentJulianDate;

        tm.Tick(1.0); // 1 real second

        Assert.True(tm.CurrentJulianDate > before);
        Assert.Equal(before + 1.0, tm.CurrentJulianDate, 5);
    }

    [Fact]
    public void Tick_DoesNotAdvance_WhenPaused()
    {
        var bus = new EventBus();
        var tm = new TimeManager(bus);

        tm.Pause();
        double before = tm.CurrentJulianDate;

        tm.Tick(1.0);

        Assert.Equal(before, tm.CurrentJulianDate);
    }

    [Fact]
    public void Tick_Reverse_DecreasesTime()
    {
        var bus = new EventBus();
        var tm = new TimeManager(bus);

        tm.Reverse();
        tm.TimeScale = 86400.0;
        double before = tm.CurrentJulianDate;

        tm.Tick(1.0);

        Assert.True(tm.CurrentJulianDate < before);
    }

    [Fact]
    public void SetTimeScale_AppliesPreset()
    {
        var bus = new EventBus();
        var tm = new TimeManager(bus);

        tm.SetTimeScale(TimeScalePreset.Speed100x);
        Assert.Equal(TimeScalePreset.Speed100x, tm.ActivePreset);
        Assert.Equal(100.0, tm.TimeScale);
    }

    [Fact]
    public void SetTimeScale_Paused_StopsClock()
    {
        var bus = new EventBus();
        var tm = new TimeManager(bus);

        tm.Play();
        tm.SetTimeScale(TimeScalePreset.Paused);

        Assert.False(tm.IsPlaying);
    }

    [Fact]
    public void SetTime_SetsJulianDate()
    {
        var bus = new EventBus();
        var tm = new TimeManager(bus);

        tm.SetTime(2460000.0);
        Assert.Equal(2460000.0, tm.CurrentJulianDate);
    }

    [Fact]
    public void ResetToJ2000_ResetsAllState()
    {
        var bus = new EventBus();
        var tm = new TimeManager(bus);

        tm.Play();
        tm.SetTime(2460000.0);
        tm.TimeScale = 100.0;
        tm.Reverse();

        tm.ResetToJ2000();

        Assert.Equal(2451545.0, tm.CurrentJulianDate);
        Assert.False(tm.IsPlaying);
        Assert.False(tm.IsReversed);
    }

    [Fact]
    public void Tick_PublishesTimeChangedEvent()
    {
        var bus = new EventBus();
        var tm = new TimeManager(bus);

        double? receivedJd = null;
        bus.Subscribe(UniverseEvent.TimeChanged, e => receivedJd = (double)e.Payload!);

        tm.Play();
        tm.TimeScale = 86400.0;
        tm.Tick(1.0);

        Assert.NotNull(receivedJd);
    }

    [Fact]
    public void TimeChanged_Event_FiresOnTick()
    {
        var bus = new EventBus();
        var tm = new TimeManager(bus);

        double? reported = null;
        tm.TimeChanged += jd => reported = jd;

        tm.Play();
        tm.TimeScale = 1.0;
        tm.Tick(1.0);

        Assert.NotNull(reported);
    }
}

// ═══════════════════════════════════════════════════════════════════════
// Julian Date Tests
// ═══════════════════════════════════════════════════════════════════════

public sealed class JulianDateTests
{
    [Fact]
    public void J2000_IsCorrectValue()
    {
        Assert.Equal(2451545.0, JulianDate.J2000.Value);
    }

    [Fact]
    public void FromDateTime_ToDateTime_Roundtrip()
    {
        var dt = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var jd = JulianDate.FromDateTime(dt);
        var back = jd.ToDateTime();

        Assert.Equal(dt.Year, back.Year);
        Assert.Equal(dt.Month, back.Month);
        Assert.Equal(dt.Day, back.Day);
        Assert.Equal(dt.Hour, back.Hour);
    }

    [Fact]
    public void J2000_ToDateTime_IsJan1_2000_Noon()
    {
        var dt = JulianDate.J2000.ToDateTime();
        Assert.Equal(2000, dt.Year);
        Assert.Equal(1, dt.Month);
        Assert.Equal(1, dt.Day);
        Assert.Equal(12, dt.Hour);
    }

    [Fact]
    public void AddDays_Works()
    {
        var jd = JulianDate.J2000.AddDays(365.25);
        Assert.Equal(2451545.0 + 365.25, jd.Value);
    }

    [Fact]
    public void AddSeconds_Works()
    {
        var jd = JulianDate.J2000.AddSeconds(86400.0);
        Assert.Equal(2451546.0, jd.Value, 10);
    }

    [Fact]
    public void Operators_Work()
    {
        var a = new JulianDate(100.0);
        var b = new JulianDate(50.0);

        Assert.Equal(50.0, a - b);
        Assert.Equal(150.0, (a + 50.0).Value);
        Assert.True(a > b);
        Assert.True(b < a);
        Assert.True(a != b);
        Assert.True(a == new JulianDate(100.0));
    }
}

// ═══════════════════════════════════════════════════════════════════════
// Search Service Tests
// ═══════════════════════════════════════════════════════════════════════

public sealed class SearchServiceTests
{
    private static (SearchService search, UniverseManager manager) Create()
    {
        var bus = new EventBus();
        var hierarchy = new UniverseHierarchy();
        var manager = new UniverseManager(hierarchy, bus);
        manager.Initialize();
        var search = new SearchService(manager);
        return (search, manager);
    }

    [Fact]
    public void FindByName_CaseInsensitiveSubstring()
    {
        var (search, manager) = Create();
        manager.Register(new Star("s1", "Betelgeuse"));
        manager.Register(new Star("s2", "Rigel"));
        manager.Register(new Planet("p1", "Beta Pictoris b"));

        var results = search.FindByName("bet");
        Assert.Equal(2, results.Count); // Betelgeuse and Beta Pictoris b
    }

    [Fact]
    public void FindByName_EmptyQuery_ReturnsEmpty()
    {
        var (search, manager) = Create();
        manager.Register(new Star("s1", "Sirius"));

        var results = search.FindByName("");
        Assert.Empty(results);
    }

    [Fact]
    public void FindByName_NoMatch_ReturnsEmpty()
    {
        var (search, manager) = Create();
        manager.Register(new Star("s1", "Sirius"));

        var results = search.FindByName("xyz");
        Assert.Empty(results);
    }

    [Fact]
    public void FindById_ReturnsExact()
    {
        var (search, manager) = Create();
        var star = new Star("hip27989", "Betelgeuse");
        manager.Register(star);

        var result = search.FindById("hip27989");
        Assert.Same(star, result);
    }

    [Fact]
    public void FindById_NotFound_ReturnsNull()
    {
        var (search, _) = Create();
        Assert.Null(search.FindById("nonexistent"));
    }

    [Fact]
    public void FindByType_ReturnsCorrectBodies()
    {
        var (search, manager) = Create();
        manager.Register(new Star("s1", "Star1"));
        manager.Register(new Star("s2", "Star2"));
        manager.Register(new Planet("p1", "Planet1"));

        var stars = search.FindByType(CelestialBodyType.Star);
        Assert.Equal(2, stars.Count);

        var galaxies = search.FindByType(CelestialBodyType.Galaxy);
        Assert.Empty(galaxies);
    }

    [Fact]
    public void FindByName_SearchesCatalogReferences()
    {
        var (search, manager) = Create();
        var star = new Star("s1", "Betelgeuse");
        star.CatalogReferences["HIP"] = "27989";
        manager.Register(star);

        var results = search.FindByName("27989");
        Assert.Single(results);
        Assert.Same(star, results[0]);
    }
}

// ═══════════════════════════════════════════════════════════════════════
// Selection Manager Tests
// ═══════════════════════════════════════════════════════════════════════

public sealed class SelectionManagerTests
{
    private static (SelectionManager selection, UniverseManager manager, EventBus bus) Create()
    {
        var bus = new EventBus();
        var hierarchy = new UniverseHierarchy();
        var manager = new UniverseManager(hierarchy, bus);
        manager.Initialize();
        var selection = new SelectionManager(manager, bus);
        return (selection, manager, bus);
    }

    [Fact]
    public void Select_SetsSelectedBody()
    {
        var (selection, manager, _) = Create();
        var star = new Star("s1", "Sirius");
        manager.Register(star);

        selection.Select(star);

        Assert.True(selection.HasSelection);
        Assert.Same(star, selection.SelectedBody);
        Assert.Equal("s1", selection.SelectedObjectId);
    }

    [Fact]
    public void ClearSelection_ClearsBody()
    {
        var (selection, manager, _) = Create();
        var star = new Star("s1", "Sirius");
        manager.Register(star);

        selection.Select(star);
        selection.ClearSelection();

        Assert.False(selection.HasSelection);
        Assert.Null(selection.SelectedBody);
        Assert.Null(selection.SelectedObjectId);
    }

    [Fact]
    public void SelectById_Works()
    {
        var (selection, manager, _) = Create();
        var planet = new Planet("earth", "Earth");
        manager.Register(planet);

        selection.SelectById("earth");

        Assert.Same(planet, selection.SelectedBody);
    }

    [Fact]
    public void BodySelectionChanged_FiresOnSelect()
    {
        var (selection, manager, _) = Create();
        var star = new Star("s1", "Vega");
        manager.Register(star);

        CelestialBody? received = null;
        selection.BodySelectionChanged += body => received = body;

        selection.Select(star);

        Assert.Same(star, received);
    }

    [Fact]
    public void BodySelectionChanged_FiresOnClear()
    {
        var (selection, manager, _) = Create();
        var star = new Star("s1", "Vega");
        manager.Register(star);
        selection.Select(star);

        CelestialBody? received = star; // Will be set to null
        selection.BodySelectionChanged += body => received = body;

        selection.ClearSelection();

        Assert.Null(received);
    }

    [Fact]
    public void SelectionChanged_PublishesToEventBus()
    {
        var (selection, manager, bus) = Create();
        var star = new Star("s1", "Altair");
        manager.Register(star);

        CelestialBody? eventPayload = null;
        bus.Subscribe(UniverseEvent.SelectionChanged, e => eventPayload = e.Payload as CelestialBody);

        selection.Select(star);

        Assert.Same(star, eventPayload);
    }

    [Fact]
    public void ISelectionService_Select_Works()
    {
        var (selection, manager, _) = Create();
        var star = new Star("s1", "Deneb");
        manager.Register(star);

        // Use the ISelectionService interface
        Services.ISelectionService service = selection;
        service.Select("s1");

        Assert.Equal("s1", service.SelectedObjectId);
        Assert.True(service.HasSelection);
    }
}

// ═══════════════════════════════════════════════════════════════════════
// Hierarchy Tests
// ═══════════════════════════════════════════════════════════════════════

public sealed class HierarchyTests
{
    [Fact]
    public void CelestialBody_AddChild_SetsParent()
    {
        var sun = new Star("sun", "Sun");
        var earth = new Planet("earth", "Earth");

        sun.AddChild(earth);

        Assert.Same(sun, earth.Parent);
        Assert.Single(sun.Children);
        Assert.Same(earth, sun.Children[0]);
    }

    [Fact]
    public void CelestialBody_RemoveChild_ClearsParent()
    {
        var sun = new Star("sun", "Sun");
        var earth = new Planet("earth", "Earth");

        sun.AddChild(earth);
        sun.RemoveChild(earth);

        Assert.Null(earth.Parent);
        Assert.Empty(sun.Children);
    }

    [Fact]
    public void CelestialBody_AddChild_SelfReference_Throws()
    {
        var star = new Star("s1", "Star");
        Assert.Throws<InvalidOperationException>(() => star.AddChild(star));
    }

    [Fact]
    public void CelestialBody_AddChild_ReparentsFromOldParent()
    {
        var sun = new Star("sun", "Sun");
        var otherStar = new Star("other", "Other Star");
        var earth = new Planet("earth", "Earth");

        sun.AddChild(earth);
        otherStar.AddChild(earth);

        Assert.Empty(sun.Children);
        Assert.Same(otherStar, earth.Parent);
        Assert.Single(otherStar.Children);
    }

    [Fact]
    public void UniverseHierarchy_SetParent_TracksRelationship()
    {
        var hierarchy = new UniverseHierarchy();
        hierarchy.Register("sun");
        hierarchy.Register("earth");
        hierarchy.Register("moon");

        hierarchy.SetParent("earth", "sun");
        hierarchy.SetParent("moon", "earth");

        Assert.Equal("sun", hierarchy.GetParent("earth"));
        Assert.Equal("earth", hierarchy.GetParent("moon"));
        Assert.Contains("earth", hierarchy.GetChildren("sun"));
        Assert.Contains("moon", hierarchy.GetChildren("earth"));
    }

    [Fact]
    public void UniverseHierarchy_GetRoots_ReturnsOrphans()
    {
        var hierarchy = new UniverseHierarchy();
        hierarchy.Register("sun");
        hierarchy.Register("sirius");
        hierarchy.Register("earth");
        hierarchy.SetParent("earth", "sun");

        var roots = hierarchy.GetRoots();

        Assert.Contains("sun", roots);
        Assert.Contains("sirius", roots);
        Assert.DoesNotContain("earth", roots);
    }

    [Fact]
    public void UniverseHierarchy_GetDescendants_Recursive()
    {
        var hierarchy = new UniverseHierarchy();
        hierarchy.Register("sun");
        hierarchy.Register("earth");
        hierarchy.Register("moon");
        hierarchy.Register("iss");

        hierarchy.SetParent("earth", "sun");
        hierarchy.SetParent("moon", "earth");
        hierarchy.SetParent("iss", "earth");

        var descendants = hierarchy.GetDescendants("sun");

        Assert.Equal(3, descendants.Count);
        Assert.Contains("earth", descendants);
        Assert.Contains("moon", descendants);
        Assert.Contains("iss", descendants);
    }

    [Fact]
    public void UniverseHierarchy_GetAncestors_ReturnsList()
    {
        var hierarchy = new UniverseHierarchy();
        hierarchy.Register("sun");
        hierarchy.Register("earth");
        hierarchy.Register("moon");

        hierarchy.SetParent("earth", "sun");
        hierarchy.SetParent("moon", "earth");

        var ancestors = hierarchy.GetAncestors("moon");

        Assert.Equal(2, ancestors.Count);
        Assert.Equal("earth", ancestors[0]);
        Assert.Equal("sun", ancestors[1]);
    }

    [Fact]
    public void UniverseHierarchy_Remove_OrphansChildren()
    {
        var hierarchy = new UniverseHierarchy();
        hierarchy.Register("sun");
        hierarchy.Register("earth");
        hierarchy.SetParent("earth", "sun");

        hierarchy.Remove("sun");

        Assert.Null(hierarchy.GetParent("earth"));
    }
}

// ═══════════════════════════════════════════════════════════════════════
// Event Bus Tests
// ═══════════════════════════════════════════════════════════════════════

public sealed class EventBusTests
{
    [Fact]
    public void Subscribe_And_Publish_InvokesHandler()
    {
        var bus = new EventBus();
        UniverseEventArgs? received = null;
        bus.Subscribe(UniverseEvent.ObjectCreated, e => received = e);

        bus.Publish(new UniverseEventArgs(UniverseEvent.ObjectCreated, "test"));

        Assert.NotNull(received);
        Assert.Equal(UniverseEvent.ObjectCreated, received!.EventType);
        Assert.Equal("test", received.Payload);
    }

    [Fact]
    public void Unsubscribe_RemovesHandler()
    {
        var bus = new EventBus();
        int count = 0;
        Action<UniverseEventArgs> handler = _ => count++;

        bus.Subscribe(UniverseEvent.TimeChanged, handler);
        bus.Publish(new UniverseEventArgs(UniverseEvent.TimeChanged));
        Assert.Equal(1, count);

        bus.Unsubscribe(UniverseEvent.TimeChanged, handler);
        bus.Publish(new UniverseEventArgs(UniverseEvent.TimeChanged));
        Assert.Equal(1, count); // Not incremented
    }

    [Fact]
    public void Publish_WrongEventType_DoesNotInvoke()
    {
        var bus = new EventBus();
        bool invoked = false;
        bus.Subscribe(UniverseEvent.ObjectCreated, _ => invoked = true);

        bus.Publish(new UniverseEventArgs(UniverseEvent.TimeChanged));

        Assert.False(invoked);
    }

    [Fact]
    public void ClearAll_RemovesAllSubscriptions()
    {
        var bus = new EventBus();
        int count = 0;
        bus.Subscribe(UniverseEvent.ObjectCreated, _ => count++);
        bus.Subscribe(UniverseEvent.TimeChanged, _ => count++);

        bus.ClearAll();

        bus.Publish(new UniverseEventArgs(UniverseEvent.ObjectCreated));
        bus.Publish(new UniverseEventArgs(UniverseEvent.TimeChanged));

        Assert.Equal(0, count);
    }

    [Fact]
    public void GetSubscriberCount_ReturnsCorrect()
    {
        var bus = new EventBus();
        Assert.Equal(0, bus.GetSubscriberCount(UniverseEvent.ObjectCreated));

        bus.Subscribe(UniverseEvent.ObjectCreated, _ => { });
        bus.Subscribe(UniverseEvent.ObjectCreated, _ => { });

        Assert.Equal(2, bus.GetSubscriberCount(UniverseEvent.ObjectCreated));
    }
}

// ═══════════════════════════════════════════════════════════════════════
// Camera Behavior Tests
// ═══════════════════════════════════════════════════════════════════════

public sealed class CameraBehaviorTests
{
    [Fact]
    public void DefaultBehavior_IsFree()
    {
        var camera = new ObservationCamera();
        var controller = new CameraBehaviorController(camera);

        Assert.Equal(CameraBehavior.Free, controller.CurrentBehavior);
        Assert.Null(controller.TrackedBody);
    }

    [Fact]
    public void SetBehavior_FocusObject_SetsTarget()
    {
        var camera = new ObservationCamera();
        var controller = new CameraBehaviorController(camera);
        var star = new Star("s1", "Sirius");
        star.Position = new Vec3d(100, 200, 300);

        controller.SetBehavior(CameraBehavior.FocusObject, star);

        Assert.Equal(CameraBehavior.FocusObject, controller.CurrentBehavior);
        Assert.Same(star, controller.TrackedBody);
    }

    [Fact]
    public void SetBehavior_Free_ClearsTarget()
    {
        var camera = new ObservationCamera();
        var controller = new CameraBehaviorController(camera);
        var star = new Star("s1", "Sirius");

        controller.SetBehavior(CameraBehavior.FocusObject, star);
        controller.SetBehavior(CameraBehavior.Free);

        Assert.Equal(CameraBehavior.Free, controller.CurrentBehavior);
        Assert.Null(controller.TrackedBody);
    }

    [Fact]
    public void GoToObject_SetsCameraTarget()
    {
        var camera = new ObservationCamera();
        var controller = new CameraBehaviorController(camera);
        var star = new Star("s1", "Polaris");
        star.Position = new Vec3d(50, 60, 70);

        controller.GoToObject(star);

        Assert.Equal(CameraBehavior.FocusObject, controller.CurrentBehavior);
        Assert.Same(star, controller.TrackedBody);
    }
}

// ═══════════════════════════════════════════════════════════════════════
// Coordinate Transform Tests
// ═══════════════════════════════════════════════════════════════════════

public sealed class CoordinateTransformTests
{
    [Fact]
    public void EquatorialToEcliptic_AndBack_Roundtrip()
    {
        var original = new Vec3d(100, 50, 30);
        var ecliptic = CoordinateTransformService.EquatorialToEcliptic(original);
        var back = CoordinateTransformService.EclipticToEquatorial(ecliptic);

        Assert.Equal(original.X, back.X, 8);
        Assert.Equal(original.Y, back.Y, 8);
        Assert.Equal(original.Z, back.Z, 8);
    }

    [Fact]
    public void EquatorialToGalactic_AndBack_Roundtrip()
    {
        var original = new Vec3d(100, 50, 30);
        var galactic = CoordinateTransformService.EquatorialToGalactic(original);
        var back = CoordinateTransformService.GalacticToEquatorial(galactic);

        Assert.Equal(original.X, back.X, 6);
        Assert.Equal(original.Y, back.Y, 6);
        Assert.Equal(original.Z, back.Z, 6);
    }

    [Fact]
    public void HeliocentricToBarycentric_CorrectOffset()
    {
        var helio = new Vec3d(1, 0, 0);
        var sunOffset = new Vec3d(0.01, 0.02, 0.003);

        var bary = CoordinateTransformService.HeliocentricToBarycentric(helio, sunOffset);
        Assert.Equal(1.01, bary.X, 10);
        Assert.Equal(0.02, bary.Y, 10);

        var back = CoordinateTransformService.BarycentricToHeliocentric(bary, sunOffset);
        Assert.Equal(helio.X, back.X, 10);
        Assert.Equal(helio.Y, back.Y, 10);
    }

    [Fact]
    public void HeliocentricToGeocentric_CorrectOffset()
    {
        var bodyHelio = new Vec3d(5, 0, 0);
        var earthHelio = new Vec3d(1, 0, 0);

        var geo = CoordinateTransformService.HeliocentricToGeocentric(bodyHelio, earthHelio);
        Assert.Equal(4.0, geo.X, 10);

        var back = CoordinateTransformService.GeocentricToHeliocentric(geo, earthHelio);
        Assert.Equal(bodyHelio.X, back.X, 10);
    }

    [Fact]
    public void ToCameraRelative_SubtractsCameraPosition()
    {
        var world = new Vec3d(100, 200, 300);
        var camera = new Vec3d(10, 20, 30);

        var relative = CoordinateTransformService.ToCameraRelative(world, camera);
        Assert.Equal(90, relative.X, 10);
        Assert.Equal(180, relative.Y, 10);
        Assert.Equal(270, relative.Z, 10);
    }
}
