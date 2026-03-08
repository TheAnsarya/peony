```

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.6466/22H2/2022Update)
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.200-preview.0.26103.119
  [Host] : .NET 10.0.3 (10.0.326.7603), X64 RyuJIT AVX2
  Dry    : .NET 10.0.3 (10.0.326.7603), X64 RyuJIT AVX2

Job=Dry  IterationCount=1  LaunchCount=1  
RunStrategy=ColdStart  UnrollFactor=1  WarmupCount=1  

```
| Method             | Mean      | Error | Ratio | Allocated | Alloc Ratio |
|------------------- |----------:|------:|------:|----------:|------------:|
| Decode6502_32KB    |  7.776 ms |    NA |  1.00 |   3.74 MB |        1.00 |
| Decode65816_32KB   |  7.915 ms |    NA |  1.02 |   3.35 MB |        0.90 |
| DecodeGameBoy_32KB | 11.428 ms |    NA |  1.47 |   2.67 MB |        0.71 |
| DecodeARM7_64KB    |  5.451 ms |    NA |  0.70 |   2.51 MB |        0.67 |
