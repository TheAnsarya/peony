```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6466/22H2/2022Update)
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.200-preview.0.26103.119
  [Host] : .NET 10.0.3 (10.0.3, 10.0.326.7603), X64 RyuJIT x86-64-v3
  Dry    : .NET 10.0.3 (10.0.3, 10.0.326.7603), X64 RyuJIT x86-64-v3

Job=Dry  IterationCount=1  LaunchCount=1  
RunStrategy=ColdStart  UnrollFactor=1  WarmupCount=1  

```
| Method                               | Mean        | Error | Ratio | Allocated | Alloc Ratio |
|------------------------------------- |------------:|------:|------:|----------:|------------:|
| MultipleLinqCount                    |  2,329.9 μs |    NA |  0.78 |         - |          NA |
| SinglePassCount                      |    392.0 μs |    NA |  0.13 |         - |          NA |
| GameBoyCpuDecoder_Decode32KB         | 14,865.5 μs |    NA |  4.99 | 2351184 B |          NA |
| OrderBy_ToList                       | 11,313.4 μs |    NA |  3.80 |  220376 B |          NA |
| OrderBy_ToArray                      |  8,315.5 μs |    NA |  2.79 |  220344 B |          NA |
| Dictionary_TryGetValue_Opcodes       |  2,977.3 μs |    NA |  1.00 |         - |          NA |
| FrozenDictionary_TryGetValue_Opcodes |  1,908.5 μs |    NA |  0.64 |         - |          NA |
