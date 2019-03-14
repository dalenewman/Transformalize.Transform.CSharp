### Overview

This adds C# transform to Transformalize. This plug-in ships with the [Transformalize CLI](https://github.com/dalenewman/Transformalize/tree/master/Pipeline.Command).

### Usage

```xml
<cfg name="Test">
   <entities>
      <add name="Test">
         <rows>
            <add text="SomethingWonderful" number="2" />
         </rows>
         <fields>
            <add name="text" />
            <add name="number" type="int" />
         </fields>
         <calculated-fields>
            <add name="csharp" t='cs(return text + " " + number;)' />
         </calculated-fields>
      </add>
   </entities>
</cfg>
```

This produces `SomethingWonderful 2`

### Warning

Note: The first time this runs, it is susceptible to a known memory leak 
associated with compiling and running c# code within a process.

If you use this transform in a long-running process, it's recommended to set 
a `max-memory` in the root node (e.g. `<cfg max-memory="500 mb" />`). 
The Transformalize CLI honors the max memory setting, and will exit if it's 
exceeded.

### Benchmark

``` ini
BenchmarkDotNet=v0.11.4, OS=Windows 10.0.17134.407 (1803/April2018Update/Redstone4)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
Frequency=2742189 Hz, Resolution=364.6722 ns, Timer=TSC
  [Host]       : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.3221.0
  LegacyJitX64 : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit LegacyJIT/clrjit-v4.7.3221.0;compatjit-v4.7.3221.0

Job=LegacyJitX64  Jit=LegacyJit  Platform=X64  
Runtime=Clr  
```
---
|                         Method |     Mean |     Error |    StdDev | Ratio | RatioSD |
|------------------------------- |---------:|----------:|----------:|------:|--------:|
|                    &#39;7777 rows&#39; | 727.8 ms |  4.662 ms |  4.133 ms |  1.00 |    0.00 |
| &#39;7777 rows with csharp&#39; | 750.3 ms | 14.764 ms | 18.131 ms |  1.04 |    0.03 |
