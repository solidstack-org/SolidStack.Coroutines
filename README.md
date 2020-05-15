# SolidStack.Coroutines

Solid Coroutine Library for Unity and .NET

## Installation

Using Unity Package Manager (packages.json):
```json
{
  "scopedRegistries": [
    {
      "name": "SolidStack",
      "url": "https://packages.solidstack.org/",
      "scopes": ["com.solidstack"]
    }
  ],
  "dependencies": {
    "com.solidstack.coroutines": "0.0.2"
  }
}
```

Using NuGet:
```powershell
PM> Install-Package StackExchange.Redis
```


## Documentation

- [Basic Usage](#basic-usage) - getting started and basic usage
- [Unity Integration](#unity-integration) - replacing native unity Coroutines
- [Exception Handling](#exception-handling) - handling errors that happen inside coroutines
- [Task Integration](#task-integration) - conversion between Coroutines and Tasks
- [.NET Standard](#net-standard-implementation) - using outside of Unity
- [Editor and Tests](#editor-and-tests) - using in Unity editor and Tests


## Basic Usage

SolidStack.Coroutines allow you to run coroutines outside of Unity (or any other) native update loop.
It allows you to pass it as an interface, and provide different implementations (e.g. test time). 
You can choose to provide a mock implementation in your unit tests, or use a real one and control how it ticks in your integration tests.


### Unity Setup:

```csharp
ICoroutineService coroutineService = new CoroutineService(); // Create new Coroutine Service

ICoroutineServiceRunner serviceRunner = new GameObject() // Create new GameObject
    .AddComponent<CoroutineServiceRunner>();

serviceRunner.SetCoroutineService(coroutineService); // Tell service to update Coroutine Service callbacks

...

Destroy(serviceRunner.gameObject); // Destroy the gameObject when you're done
```

`CoroutineServiceRunner` also implements `IDisposable` that will destroy a gameObject when `Dispose()` is called.


Starting a coroutine:

```csharp
using SolidStack.Coroutines;
...
IEnumerator RunMyCoroutine()
{
    yield return coroutineService.WaitForSeconds(1.0f);
    Debug.Log("This will print after 1 second!");
}

ICoroutine coro = coroutineService.StartCoroutine(RunMyCoroutine()); // Start

coroutineService.StopCoroutine(coro); // Stop
```

### Wait for methods

Use appropriate WaitFor* method to delay the coroutine execution:

```csharp
IEnumerator RunMyCoroutine()
{
    yield return coroutineService.WaitForSeconds(10f); // Wait for 10 seconds
    yield return coroutineService.WaitForSecondsRealtime(10f); // Wait for 10 "real" seconds since startup
    yield return coroutineService.WaitForLateTick(); // Wait for late update
    yield return coroutineService.WaitForFixedTick(); // Wait for fixed update
    yield return coroutineService.WaitForNextTick(); // Wait for next update
    yield return coroutineService.WaitForNumTicks(5); // Wait for 5 updates
    yield return coroutineService.WaitWhile(() => condition)); // Wait while condition is true
    yield return coroutineService.WaitUntil(() => condition)); // Wait until condition becomes true
}

```

## Unity Integration

To create a new instance of CoroutineService in Unity, use `CoroutineServiceRunner` MonoBehaviour. Service runner component will invoke appropriate Tick methods.

### Using in Unity coroutine

You can use a SolidStack coroutine to block Unity coroutine:

```csharp
class MyBehaviour : MonoBehaviour
{
    IEnumerator RunSolidStackCoroutine()
    {
        yield return coroutineService.WaitForNumTicks(3);
    }

    IEnumerator RunUnityCoroutine()
    {
        yield return coroutineService.StartCoroutine(RunSolidStackCoroutine());
    }

    public void Start()
    {
        StartCoroutine(RunUnityCoroutine()); // Unity coroutine
    }
}
```

## Exception Handling

Unlike Unity Coroutines, SolidStack.Coroutines provides support for exception handling. 
Any exception will be thrown in the frame during the tick. However, you can access `ICoroutine.Exception`:

```csharp
IEnumerator RunCoroutine()
{
    yield return null;
    throw new Exception("Failed");
}

ICoroutine coroutine = coroutineService.StartCoroutine(RunCoroutine());

Debug.Log(coroutine.Exception); // "Failed" exception
```

## Task Integration

SolidStack.Coroutines provides a layer of compatibility with Tasks. You can use an async task to start a Coroutine:

```csharp

async Task DoSomething()
{
    await Task.Delay(1000);
}

ICoroutine coroutine = coroutineService.StartCoroutine(DoSomething());
```

You can also convert a Coroutine into a Task:

```csharp

IEnumerator RunMyCoroutine()
{
    yield return coroutineService.WaitForSeconds(1f);
}

async Task RunMyCoroutineAsync()
{
    await coroutineService.StartCoroutineAsync(() => RunMyCoroutine());
}
```

Tasks can be used inside Coroutines, and Coroutines can be used inside Tasks.


### .NET Standard Implementation

You can use SolidStack.Coroutines with a non-Unity code, e.g. server code or other game engines that use C#.
To do that, install a SolidStack.Coroutines package from the NuGet. 

You will have to implement your own service runner, based on your needs. 

You can update coroutine service time and perform ticks in some sort of a loop:

```csharp
public async Task Run()
{
    while(IsRunning)
    {
        // Update coroutine service time
        TimeSpan timeSinceStartup = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
        coroutineService?.SetTime((float)timeSinceStartup.TotalSeconds);
        coroutineService?.SetTime((float)timeSinceStartup.TotalSeconds);

        // Perform ticks
        coroutineService.TickCoroutines();
        coroutineService.TickFixedCoroutines(); // Tick in your physics engine?
        coroutineService.TickLateCoroutines();

        await Task.Delay(15); // Assuming running 66 times a second
    }
}
```

Example server code is located in [Examples/NetStandard](Examples/NetStandard) directory.


### Editor and Tests

This implementation of Coroutine service supports integration with Unity Editor and test code.

### Unity Playmode Tests

Running a coroutine Unity Playmode integration test is not any different from running a regular Unity code. Refer to [Unity Integration](#unity-integration) section for more examples.
You can also find example under [Examples/Unity](Examples/Unity) directory.

### Unity Editor Mode Tests

Since SolidStack.Coroutines do not use Unity Coroutines, you can Mock ICoroutineService / ICoroutine in your Unit tests. 
Refer to Unity examples for further help.
