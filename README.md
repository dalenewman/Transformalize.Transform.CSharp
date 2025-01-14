### Overview

This adds C# transform to Transformalize.

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
