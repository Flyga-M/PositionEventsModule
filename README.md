# PositionEventsModule
A module for Blish-HUD which provides functionality to efficiently bundle position checking.

After the module loaded, you register an area for a specific map. When the player enters or leaves 
this area, a provided callback will be invoked and inform your module of this interaction.

## Types of areas (Bounding Objects)
### BoundingObjectBox
An axis aligned Bounding Box (Cuboid).
```
BoundingObjectBox(Vector3 min, Vector3 max);
```
> [!CAUTION]
> Make sure, that min < max for every axis. As a safeguard, you can use
> `BoundingObjectBox(BoundingBox.CreateFromPoints(new Vector3[] { min, max }));`
### BoundingObjectSphere
A sphere.
```
BoundingObjectSphere(Vector3 center, float radius);
```
### BoundingObjectPrism
An axis aligned prism. Think of a cylinder, just less round. Aligned with the z-Axis (top) by default.
```
BoundingObjectPrism(float top, float bottom, IEnumerable<Vector2> polygon, Axis3 alignment = Axis3.Z);
```
### Boolean Operations
The [Position Events](https://github.com/Flyga-M/PositionEvents) Package implements capabilities to 
add (Union), subtract (Difference) and intersect (Intersection) Bounding Objects from/with each other.

There are multiple ways to access these boolean operations.
1. Directly call the constructor of the Bounding Object Groups (not recommended).
 See PositionEvents.[Area](https://github.com/Flyga-M/PositionEvents/tree/master/Area) for implementation.
```
BoundingObjectGroupUnion(IEnumerable<IBoundingObject> content);
BoundingObjectGroupDifference(IBoundingObject positive, IEnumerable<IBoundingObject> negatives);
BoundingObjectGroupIntersection(IEnumerable<IBoundingObject> content);
```
2. Using PositionEvents.[IBoundingObjectExtensions](https://github.com/Flyga-M/PositionEvents/blob/master/_Extensions/IBoundingObjectExtensions.cs)
```
// assumes boundingObject and other to be IBoundingObjects (any of the ones
// mentioned above, including the boolean operations)
boundingObject = boundingObject.Union(other);
boundingObject = boundingObject.Difference(other);
boundingObject = boundingObject.Intersection(other);
```
3. Using PositionEvents.Area.[BoundingObjectBuilder](https://github.com/Flyga-M/PositionEvents/blob/master/Area/_Builder/BoundingObjectBuilder.cs)
```
// assumes oneBoundingObject, anotherBoundingObject, differentBoundingObject and
// anotherDifferentBoundingObject to be IBoundingObjects (any of the ones
// mentioned above, including the boolean operations)
BoundingObjectBuilder builder = new BoundingObjectBuilder()
    .Add(oneBoundingObject) // base
    .Add(anotherBoundingObject) // Union
    .Subtract(differentBoundingObject) // Difference
    .Intersect(anotherDifferentBoundingObject); // Intersection

IBoundingObject boundingObject = builder.Build();
```
### Implement your own
You need a different geometrical shape for your area? Implement the
 [IBoundingObject interface](https://github.com/Flyga-M/PositionEvents/blob/master/Area/_Interfaces/IBoundingObject.cs)
 and make your own.

## Using the Position Events Module with your Blish-HUD module
### Setup
1. Add the PositionEvents Package as a reference to your module. It is available as a [NuGet](https://www.nuget.org/packages/PositionEvents) package.
2. Add the Position Events Module [.dll](https://github.com/Flyga-M/PositionEventsModule/releases/) as a reference to your module.
3. Add the Position Events Module as a dependency to your module manifest.
```
dependencies":{
    "bh.blishhud": "^1.0.0",
    "Flyga.PositionEvents": "^0.2.0"
}
```
> [!TIP]
> Make sure that the references of the PositionEvents Package and the Position Events Module have **Copy Local** set
> to **False** in the properties of the reference. You don't need to ship them with your module, since they will
> already be present, because of your modules dependence on the Position Events Module.

### Usage inside your module
1. Retrieve a reference to the Position Events Module instance during your `LoadAsync` method.
> [!WARNING]
> Make sure to avoid module load order conflicts. For reference see the [Position Events Example](https://github.com/Flyga-M/PositionEventsExample).

2. Register your desired areas with the Position Events Module.
```
// Assumes you retrieved a reference to the Position Events Module before
// calling this
IBoundingObject area = new BoundingObjectBox(new Vector3(0), new Vector3(10, 20, 30));
_positionEventsModule?.RegisterArea(this, 15, area, OnAreaJoinedOrLeft);
```

3. Use the output with a callback action
```
private void OnAreaJoinedOrLeft(PositionData positionData, bool isInside)
{
    if (isInside)
    {
        Logger.Info("Area joined."); // or do something useful
        return;
    }
    Logger.Info("Area left."); // or do something useful
}
```

> [!TIP]
> For an example implementation, take a look at [Position Events Example](https://github.com/Flyga-M/PositionEventsExample).

## Debugging your areas
As a rudimentary way to debug your areas, this module provides the option to display your area
 as an `IEntity` in the world.

![Gif displaying an example of an area in debug mode](https://github.com/Flyga-M/PositionEventsModule/blob/master/Resources/debug.gif)

 To make use of this feature, simply set the `debug` parameter to `true`, when registering the area.
 ```
 _positionEventsModule?.RegisterArea(this, 15, area, OnAreaJoinedOrLeft, debug: true);
 ```
 > [!WARNING]
 > You should never ship your module with the debug functionality enabled.