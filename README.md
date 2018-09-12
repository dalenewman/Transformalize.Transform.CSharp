
### Overview

This adds C# transform to Transformalize.  It is a plug-in compatible with Transformalize 0.3.6-beta.

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

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.251)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical cores and 4 physical cores
Frequency=2742188 Hz, Resolution=364.6723 ns, Timer=TSC
  [Host]       : .NET Framework 4.6.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2633.0  [AttachedDebugger]
  LegacyJitX64 : .NET Framework 4.6.2 (CLR 4.0.30319.42000), 64bit LegacyJIT/clrjit-v4.7.2633.0;compatjit-v4.7.2633.0
  LegacyJitX86 : .NET Framework 4.6.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2633.0

Jit=LegacyJit  Runtime=Clr  

```
|                                   Method |          Job | Platform |     Mean |     Error |    StdDev | Scaled | ScaledSD |
|----------------------------------------- |------------- |--------- |---------:|----------:|----------:|-------:|---------:|
|                          &#39;500 test rows&#39; | LegacyJitX64 |      X64 | 57.04 ms | 0.8820 ms | 0.8250 ms |   1.00 |     0.00 |
| &#39;500 test rows with 3 csharp transforms&#39; | LegacyJitX64 |      X64 | 58.41 ms | 1.0703 ms | 0.9488 ms |   1.02 |     0.02 |
|                                          |              |          |          |           |           |        |          |
|                          &#39;500 test rows&#39; | LegacyJitX86 |      X86 | 61.24 ms | 1.1669 ms | 1.2486 ms |   1.00 |     0.00 |
| &#39;500 test rows with 3 csharp transforms&#39; | LegacyJitX86 |      X86 | 57.75 ms | 0.9459 ms | 0.8848 ms |   0.94 |     0.02 |
