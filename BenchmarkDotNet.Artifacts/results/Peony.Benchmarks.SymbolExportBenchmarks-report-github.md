```

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.6466/22H2/2022Update)
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.200-preview.0.26103.119
  [Host] : .NET 10.0.3 (10.0.326.7603), X64 RyuJIT AVX2
  Dry    : .NET 10.0.3 (10.0.326.7603), X64 RyuJIT AVX2

Job=Dry  IterationCount=1  LaunchCount=1  
RunStrategy=ColdStart  UnrollFactor=1  WarmupCount=1  

```
| Method            | Mean      | Error | Allocated |
|------------------ |----------:|------:|----------:|
| ExportPansyFormat |  1.416 ms |    NA | 622.87 KB |
| ImportPansyFormat | 13.036 ms |    NA | 548.98 KB |
| ExportMesenFormat |  4.133 ms |    NA | 370.66 KB |
