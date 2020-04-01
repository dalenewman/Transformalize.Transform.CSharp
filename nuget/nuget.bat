nuget pack Transformalize.Transform.CSharp.nuspec -OutputDirectory "c:\temp\modules"
nuget pack Transformalize.Transform.CSharp.Autofac.nuspec -OutputDirectory "c:\temp\modules"

REM nuget push "c:\temp\modules\Transformalize.Transform.CSharp.0.6.28-beta.nupkg" -source https://api.nuget.org/v3/index.json
REM nuget push "c:\temp\modules\Transformalize.Transform.CSharp.Autofac.0.6.28-beta.nupkg" -source https://api.nuget.org/v3/index.json






