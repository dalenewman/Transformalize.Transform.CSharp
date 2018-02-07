This adds C# transform to Transformalize.  It is a plug-in compatible with Transformalize 0.3.3-beta.

Build the Autofac project and put it's output into Transformalize's *plugins* folder.

Use like this:

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

Note: Eventually this library is susceptable to memory leaks associated with running 
dyanamically loaded c# assemblies in the host's `AppDomain`.