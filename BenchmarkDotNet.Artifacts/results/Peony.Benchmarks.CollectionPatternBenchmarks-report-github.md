```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6466/22H2/2022Update)
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.200
  [Host] : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v3
  Dry    : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v3

Job=Dry  IterationCount=1  LaunchCount=1  
RunStrategy=ColdStart  UnrollFactor=1  WarmupCount=1  

```
| Method                               | WorkMultiplier | Mean      | Error | Ratio | Gen0       | Allocated   | Alloc Ratio |
|------------------------------------- |--------------- |----------:|------:|------:|-----------:|------------:|------------:|
| MultipleLinqCount                    | 32             | 106.12 ms |    NA |  0.96 |          - |           - |          NA |
| SinglePassCount                      | 32             |  69.36 ms |    NA |  0.63 |          - |           - |          NA |
| GameBoyCpuDecoder_Decode32KB         | 32             | 105.54 ms |    NA |  0.96 | 35000.0000 | 225662208 B |          NA |
| OrderBy_ToList                       | 32             | 172.49 ms |    NA |  1.56 |  1000.0000 |   7052064 B |          NA |
| OrderBy_ToArray                      | 32             | 203.53 ms |    NA |  1.85 |  1000.0000 |   7051008 B |          NA |
| Dictionary_TryGetValue_Opcodes       | 32             | 110.22 ms |    NA |  1.00 |          - |           - |          NA |
| FrozenDictionary_TryGetValue_Opcodes | 32             | 107.45 ms |    NA |  0.97 |          - |           - |          NA |
