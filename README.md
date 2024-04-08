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
_positionEventsModule?.RegisterArea(this, 15, area, OnAreaJoinedOrLeft, true);
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