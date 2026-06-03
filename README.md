# Atis.DependencyInjection

[![NuGet Version](https://img.shields.io/nuget/v/Atis.DependencyInjection.svg)](https://www.nuget.org/packages/Atis.DependencyInjection)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Build Status](https://github.com/atintegratedsystems/atis-dependency-injection/actions/workflows/build.yml/badge.svg)](https://github.com/atintegratedsystems/atis-dependency-injection/actions)

A lightweight framework for **.NET library authors** who need to manage their library's internal dependency injection while giving consumers a clean, structured way to customize or extend it.

---

## Who Is This For?

This library is **not** for application developers wiring up their own app's DI container. It is for developers who are **building a .NET library or framework** — such as an ORM, an HTTP client library, or any component that has its own internal services — and want to:

- Manage internal service registrations cleanly
- Let consumers override or extend those registrations
- Cache the built `IServiceProvider` so it is not rebuilt on every use
- Validate that all required services are registered before first use

---

## The Recommended Design Pattern

Before diving into the API, it is important to understand the design pattern this library is built around, because getting this right is what makes your library feel polished and intuitive to consumers.

### Give Your Library a Facade

Your library should expose a single **facade class** as the main entry point for consumers. Think of `DbContext` in Entity Framework Core — consumers create an instance of it, and everything else happens internally. They never deal with service registrations directly.

```csharp
// This is what your consumer writes
var context = new MyDataContext();
var results = context.Customers.ToList();
```

The consumer should not need to know anything about dependency injection. It all happens behind the scenes inside your facade.

### All API Paths Lead to IServiceProvider

Every public property or method on your facade that needs an internal service should resolve it from a single `IServiceProvider`. This provider is initialized **lazily** — only when the consumer first uses your API — and then **cached** for all subsequent calls.

```csharp
public class DataContext
{
    private IServiceProvider _serviceProvider;

    private IServiceProvider ServiceProvider
        => _serviceProvider ??= BuildServiceProvider();

    // Every API property resolves from the same provider
    public IRepository<Customer> Customers
        => ServiceProvider.GetRequiredService<IRepository<Customer>>();

    public IRepository<Order> Orders
        => ServiceProvider.GetRequiredService<IRepository<Order>>();
}
```

This means no matter which property the consumer touches first, the provider is built once and reused everywhere.

### Use OnConfiguring for Consumer Customization

Rather than asking consumers to pass configuration through a constructor, expose an `OnConfiguring` method they can override. This is the same pattern used by Entity Framework Core and feels natural to most .NET developers.

```csharp
public class MyDataContext : DataContext
{
    protected override void OnConfiguring(DataContextConfiguration config)
    {
        config.UseSqlServer("Server=.;Database=MyDb;Trusted_Connection=True;");
    }
}
```

---

## Installation

```
dotnet add package Atis.DependencyInjection
```

Or via the NuGet Package Manager in Visual Studio, search for `Atis.DependencyInjection`.

---

## Core Concepts

Before looking at code, here is a quick overview of each type and its role:

| Type | Role |
|---|---|
| `ServiceBuilderBase` | You subclass this in your library. It defines what services your library needs and registers them. |
| `ServiceManagerBase` | You subclass this in your library. It builds and caches the `IServiceProvider`. |
| `ServiceCharacteristic` | Describes a service — its lifetime (`Transient`, `Scoped`, `Singleton`) and whether multiple registrations are allowed. |
| `IServiceContextConfiguration` | The configuration object passed to `OnConfiguring`. Consumers use this to register extensions. |
| `ServiceContextConfiguration` | The default implementation of `IServiceContextConfiguration`. Ready to use as-is. |
| `IServiceContextExtension` | Consumers implement this to register their custom services into your library's container. |
| `CoreServicesNotInitializedException` | Thrown when a required service was not registered, helping catch mistakes early. |

---

## How It All Works — The Three Phases

Understanding the three phases is the key to understanding this library. It is important to know that calling `OnConfiguring` does **not** immediately register any services. Registration is deferred until the `IServiceProvider` is actually needed.

### Phase 1 — Collection (your consumer's code runs)

When the consumer creates your facade, `OnConfiguring` is called. The consumer calls something like `config.UseSqlServer(...)` which internally calls `config.AddOrUpdateExtension(new SqlServerExtension(...))`.

At this point, the `SqlServerExtension` instance is simply **stored** inside the configuration object. No services are registered yet. Think of this phase as the consumer writing down their intentions.

```
new MyDataContext()
  └── OnConfiguring(config) called
        └── config.UseSqlServer("connection string")
              └── config.AddOrUpdateExtension(new SqlServerExtension(...))
                    └── SqlServerExtension stored internally ← nothing registered yet
```

### Phase 2 — Build (triggered on first API use)

When the consumer accesses any API property for the first time, your facade's `IServiceProvider` getter fires. Since the provider has not been built yet, it calls `ServiceManagerBase.GetOrAdd(config)`.

This is where the actual service registration happens, in this exact order:

1. A fresh `IServiceCollection` is created
2. Each stored extension's `AddServices` method is called — **consumer registrations go in first**
3. Your library's `AddCoreServices` runs — **library defaults fill in anything the consumer did not override**
4. `ValidateCoreServicesAdded` checks that all required services are present
5. `IServiceProvider` is built and cached

```
context.Customers  ← first API access
  └── ServiceProvider getter fires
        └── ServiceManagerBase.GetOrAdd(config)
              ├── SqlServerExtension.AddServices(services) ← consumer services registered first
              ├── AddCoreServices()                        ← library defaults registered second
              ├── ValidateCoreServicesAdded()              ← all required services present?
              └── serviceCollection.BuildServiceProvider() ← provider built and cached
```

The reason consumer extensions run **before** library defaults is intentional — it gives consumers the ability to override any service your library would otherwise register by default.

### Phase 3 — Cache (all subsequent API uses)

Every subsequent API call finds the cached `IServiceProvider` immediately. The build process never runs again for the same configuration.

```
context.Orders  ← second API access
  └── ServiceProvider getter fires
        └── ServiceManagerBase.GetOrAdd(config)
              └── provider found in cache → returned immediately
```

---

## Implementation Guide — Library Author

This section walks through building a fictional ORM library called `DataLib` that uses `Atis.DependencyInjection` internally.

### Step 1 — Define Your Internal Service Interfaces

These are your library's internal contracts. Consumers will never see most of these — they are implementation details.

```csharp
// Internal services your ORM needs
public interface IQueryExecutor { ... }
public interface IConnectionFactory { ... }
public interface IChangeTracker { ... }
```

### Step 2 — Implement ServiceBuilderBase

Subclass `ServiceBuilderBase` to declare what services your library owns and how they should be registered.

```csharp
public class DataLibServiceBuilder : ServiceBuilderBase
{
    // Maps each service type to its characteristics
    private static readonly Dictionary<Type, ServiceCharacteristic> _characteristics
        = new Dictionary<Type, ServiceCharacteristic>
        {
            { typeof(IQueryExecutor),    new ServiceCharacteristic(ServiceLifetime.Transient) },
            { typeof(IConnectionFactory), new ServiceCharacteristic(ServiceLifetime.Singleton) },
            { typeof(IChangeTracker),    new ServiceCharacteristic(ServiceLifetime.Scoped) },
        };

    public DataLibServiceBuilder(IServiceCollection serviceCollection)
        : base(serviceCollection)
    {
    }

    // Tell the base class the characteristics of each service
    protected override ServiceCharacteristic GetServiceCharacteristic(Type serviceType)
    {
        if (!_characteristics.TryGetValue(serviceType, out var characteristic))
            throw new InvalidOperationException(
                $"No characteristic defined for '{serviceType.FullName}'.");
        return characteristic;
    }

    // Tell the base class which types must be registered for validation
    protected override IReadOnlyCollection<Type> GetCoreServiceTypes()
    {
        return _characteristics.Keys;
    }

    // Register your library's default implementations
    // These run after consumer extensions, so consumers can override any of these
    public override void AddCoreServices()
    {
        this.TryAdd<IQueryExecutor, DefaultQueryExecutor>();
        this.TryAdd<IConnectionFactory, DefaultConnectionFactory>();
        this.TryAdd<IChangeTracker, DefaultChangeTracker>();
    }
}
```

### Step 3 — Implement ServiceManagerBase

Subclass `ServiceManagerBase` to connect it to your builder.

```csharp
public class DataLibServiceManager : ServiceManagerBase
{
    protected override ServiceBuilderBase CreateServiceBuilder(IServiceCollection serviceCollection)
    {
        return new DataLibServiceBuilder(serviceCollection);
    }
}
```

### Step 4 — Create Your Configuration Class

This is what gets passed to `OnConfiguring`. It extends `ServiceContextConfiguration` so consumers can call `AddOrUpdateExtension` on it, and you can add your own extension methods on top.

```csharp
public class DataContextConfiguration : ServiceContextConfiguration
{
    // Extension methods like UseSqlServer will be added here
}
```

### Step 5 — Create Your Facade

This is the class consumers will actually use. It wires everything together.

```csharp
public abstract class DataContext
{
    private static readonly DataLibServiceManager _serviceManager = new DataLibServiceManager();
    private IServiceProvider _serviceProvider;

    protected DataContext()
    {
        var config = new DataContextConfiguration();
        this.OnConfiguring(config);
        _serviceProvider = _serviceManager.GetOrAdd(config);
    }

    // Consumers override this to configure the context
    protected virtual void OnConfiguring(DataContextConfiguration config) { }

    // All API properties resolve from the same provider
    public IRepository<T> Set<T>() where T : class
        => _serviceProvider.GetRequiredService<IRepository<T>>();
}
```

### Step 6 — Create Extension Methods for Clean Configuration

This is what makes your library feel polished. Instead of exposing `AddOrUpdateExtension` directly, wrap it in meaningful extension methods.

```csharp
public static class DataContextConfigurationExtensions
{
    public static DataContextConfiguration UseSqlServer(
        this DataContextConfiguration config,
        string connectionString)
    {
        config.AddOrUpdateExtension(new SqlServerExtension(connectionString));
        return config;
    }

    public static DataContextConfiguration UsePostgreSql(
        this DataContextConfiguration config,
        string connectionString)
    {
        config.AddOrUpdateExtension(new PostgreSqlExtension(connectionString));
        return config;
    }
}
```

Each extension class implements `IServiceContextExtension` and registers provider-specific services:

```csharp
public class SqlServerExtension : IServiceContextExtension
{
    private readonly string _connectionString;

    public SqlServerExtension(string connectionString)
    {
        _connectionString = connectionString;
    }

    // This is called by ServiceManagerBase during provider build
    // This is where actual DI registrations happen
    public void AddServices(IServiceCollection services)
    {
        services.AddSingleton<IConnectionFactory>(
            _ => new SqlServerConnectionFactory(_connectionString));
        services.AddTransient<IQueryExecutor, SqlServerQueryExecutor>();
    }
}
```

---

## Implementation Guide — Library Consumer

Once your library is built using the pattern above, consuming it is straightforward.

### Basic Usage

```csharp
public class AppDataContext : DataContext
{
    protected override void OnConfiguring(DataContextConfiguration config)
    {
        config.UseSqlServer("Server=.;Database=AppDb;Trusted_Connection=True;");
    }
}

// Usage
var context = new AppDataContext();
var customers = context.Set<Customer>().GetAll();
```

### Overriding a Library Service

If a consumer wants to replace one of your library's internal services with their own implementation, they implement `IServiceContextExtension` directly.

```csharp
// Consumer's custom implementation
public class MyCustomQueryExecutor : IQueryExecutor
{
    // custom implementation
}

// Consumer's extension that registers it
public class MyCustomExtension : IServiceContextExtension
{
    public void AddServices(IServiceCollection services)
    {
        // Registers before library defaults, so this takes precedence
        services.AddTransient<IQueryExecutor, MyCustomQueryExecutor>();
    }
}

// Wired up in OnConfiguring
public class AppDataContext : DataContext
{
    protected override void OnConfiguring(DataContextConfiguration config)
    {
        config.UseSqlServer("Server=.;Database=AppDb;Trusted_Connection=True;");
        config.AddOrUpdateExtension(new MyCustomExtension());
    }
}
```

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
