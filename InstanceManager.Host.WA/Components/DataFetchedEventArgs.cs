namespace InstanceManager.Host.WA.Components;


public record DataFetchedEventArgs<TData>(
    TData Data,
    bool IsFromCache,
    bool IsFirstFetch
);
