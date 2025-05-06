```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.1742)
Intel Core i5-10500 CPU 3.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method    | Mean    | Error    | StdDev   | Ratio | RatioSD | Gen0         | Gen1        | Gen2       | Allocated     | Alloc Ratio |
|---------- |--------:|---------:|---------:|------:|--------:|-------------:|------------:|-----------:|--------------:|------------:|
| Default   | 8.038 s | 0.1604 s | 0.1575 s |  1.00 |    0.03 | 1583000.0000 | 469000.0000 | 25000.0000 | 9546422.88 KB |       1.000 |
| Optimized | 1.224 s | 0.0061 s | 0.0054 s |  0.15 |    0.00 |            - |           - |          - |       1.88 KB |       0.000 |
