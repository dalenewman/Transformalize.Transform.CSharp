nuget pack Transformalize.Transform.CSharp.nuspec -OutputDirectory "c:\temp\modules"
nuget pack Transformalize.Transform.CSharp.Autofac.nuspec -OutputDirectory "c:\temp\modules"

nuget push "c:\temp\modules\Transformalize.Transform.CSharp.0.3.6-beta.nupkg" -source https://api.nuget.org/v3/index.json
nuget push "c:\temp\modules\Transformalize.Transform.CSharp.Autofac.0.3.6-beta.nupkg" -source https://api.nuget.org/v3/index.json






