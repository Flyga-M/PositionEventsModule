# PositionEventsModule
A module for Blish-HUD which provides functionality to efficiently bundle position checking.

## Using the Position Events Module with your Blish-HUD module
### Setup
1. Add the PositionEvents Package as a reference to your module. It is available as a [NuGet](https://www.nuget.org/packages/PositionEvents) package.
2. Add the Position Events Module [.dll](https://github.com/Flyga-M/PositionEventsModule/releases/) as a reference to your module.
3. Add the Position Events Module as a dependency to your module manifest.
```
dependencies": {
    "bh.blishhud": "^1.0.0",
	"Flyga.PositionEvents": "^0.1.0"
  }
```
> [!TIP]
> Make sure that the references of the PositionEvents Package and the Position Events Module have **Copy Local** set
> to **False** in the properties of the reference. You don't need to ship them with your module, since they will
> already be present, because of your modules dependence on the Position Events Module.

### Usage inside your module
1. Retrieve a reference to the Position Events Module instance during your `LoadAsync` method.
```
private static PositionEventsModule _positionEventsModule;

protected override Task LoadAsync()
{
    foreach (ModuleManager item in GameService.Module.Modules)
    {
        if (item.Manifest.Namespace == "Flyga.PositionEvents")
        {
            if (item.ModuleInstance is PositionEventsModule positionEventsModule)
            {
                _positionEventsModule = positionEventsModule;
            }
            else
            {
                Logger.Error("Unable to detect required Position Events Module.");
            }
                    
            break;
        }
    }

    return Task.CompletedTask;
}
```

2. Register your desired areas with the Position Events Module.
```
IBoundingObject area = new BoundingObjectBox(new BoundingBox(new Vector3(0), new Vector3(10, 20, 30)));
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
> For an example implementation, take a look at [Position Events Examples](https://github.com/Flyga-M/PositionEventsExample).

## Debugging your areas
As a rudimentary way to debug your areas, this module provides the option to display your area
 as an `IEntity` in the world.

 To make use of this feature, simply set the `debug` parameter to `true`, when registering the area.
 ```
 _positionEventsModule?.RegisterArea(this, 15, area, OnAreaJoinedOrLeft, true);
 ```
 > [!WARNING]
 > You should never ship your module with the debug functionality enabled.