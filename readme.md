```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.1742)
Intel Core i5-10500 CPU 3.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.203
  [Host]     : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2


```
| Method    | Mean    | Error    | StdDev   | Allocated |
|---------- |--------:|---------:|---------:|----------:|
| Optimized | 1.387 s | 0.0131 s | 0.0116 s |   1.88 KB |
