
### Overview

This adds C# transform to Transformalize.  It is a plug-in compatible with Transformalize 0.3.3-beta.

Build the Autofac project and put it's output into Transformalize's *plugins* folder.

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
                <add name="csharped" t='cs(return text + " " + number;)' />
            </calculated-fields>
        </add>
    </entities>
</cfg>
```

This produces `SomethingWonderful 2`

### Warning

Note: This library is susceptible to a known memory leak associated with running 
dynamically loaded c# assemblies in the host's `AppDomain`.  If you use it in a 
long-running Transformalize service, set a `max-memory` in the root node (e.g. `500 mb`).  If the process exceeds the `max-memory`, it will exit with error.  Have your service restart automatically.  This *work-around* is acceptable when you can't afford the performance hit introduced by running c# in a remote `AppDomain`.

### Benchmark

``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.125)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical cores and 4 physical cores
Frequency=2742192 Hz, Resolution=364.6718 ns, Timer=TSC
  [Host]       : .NET Framework 4.6.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2600.0
  LegacyJitX64 : .NET Framework 4.6.2 (CLR 4.0.30319.42000), 64bit LegacyJIT/clrjit-v4.7.2600.0;compatjit-v4.7.2600.0
  LegacyJitX86 : .NET Framework 4.6.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2600.0

Jit=LegacyJit  Runtime=Clr  

```
|                                   Method |          Job | Platform |     Mean |     Error |    StdDev | Scaled |
|----------------------------------------- |------------- |--------- |---------:|----------:|----------:|-------:|
|                          &#39;500 test rows&#39; | LegacyJitX64 |      X64 | 65.10 ms | 0.3983 ms | 0.3726 ms |   1.00 |
| &#39;500 test rows with 3 csharp transforms&#39; | LegacyJitX64 |      X64 | 66.27 ms | 0.4447 ms | 0.3942 ms |   1.02 |
|                                          |              |          |          |           |           |        |
|                          &#39;500 test rows&#39; | LegacyJitX86 |      X86 | 69.72 ms | 0.4177 ms | 0.3488 ms |   1.00 |
| &#39;500 test rows with 3 csharp transforms&#39; | LegacyJitX86 |      X86 | 70.07 ms | 0.4103 ms | 0.3838 ms |   1.01 |
