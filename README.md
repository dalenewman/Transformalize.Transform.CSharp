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

Note: This library is susceptible to a known memory leak associated with running 
dyanamically loaded c# assemblies in the host's `AppDomain`.  If you use it in a 
long-running Transformalize service, set a `max-memory` in the root node (e.g. `500 mb`).  If the process exceeds the `max-memory`, it will exit with error.  Have your service restart automatically.  This *work-around* is acceptable when you can't afford the performance hit introduced by running c# in a remote `AppDomain`.