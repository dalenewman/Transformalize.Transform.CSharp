REM nuget pack Transformalize.Transform.CSharp.nuspec -OutputDirectory "c:\temp\modules"
REM nuget pack Transformalize.Transform.CSharp.Autofac.nuspec -OutputDirectory "c:\temp\modules"

REM nuget push "c:\temp\modules\Transformalize.Transform.CSharp.0.8.29-beta.nupkg" -source https://api.nuget.org/v3/index.json
REM nuget push "c:\temp\modules\Transformalize.Transform.CSharp.Autofac.0.8.29-beta.nupkg" -source https://api.nuget.org/v3/index.json

nuget push "c:\temp\modules\Transformalize.Transform.CSharp.0.8.29-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json
nuget push "c:\temp\modules\Transformalize.Transform.CSharp.Autofac.0.8.29-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json






