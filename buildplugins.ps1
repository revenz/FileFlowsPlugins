Remove-Item Builds  -Recurse -ErrorAction SilentlyContinue

$revision = (git rev-list --count --first-parent HEAD) -join "`n"


$json = "[`n"

Get-ChildItem -Path .\ -Filter *.csproj -Recurse -File -Name | ForEach-Object {
    # update version number of builds
    (Get-Content $_) `
        -replace '(?<=(Version>([\d]+\.){3}))([\d]+)(?=<)', $revision |
    Out-File $_

    $name = [System.IO.Path]::GetFileNameWithoutExtension($_) 
    $version = [Regex]::Match((Get-Content $_), "(?<=(Version>))([\d]+\.){3}[\d]+(?=<)").Value
    
    $json += "`t{`n"
    $json += "`t`t""Name"": ""$name"",`n"
    $json += "`t`t""Version"": ""$version"",`n"
    $json += "`t`t""Package"": ""https://github.com/revenz/FileFlowsPlugins/blob/master/Builds/" + $name + ".zip?raw=true""`n"
    $json += "`t},`n"

    # build an instance for FileFlow local code
    # dotnet build $_ /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary --output:../FileFlows/Server/Plugins
    # build instance to be published to repo
    dotnet build $_ /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary --output:Builds/$name
    
    Remove-Item Builds/$name/Plugin.dll -ErrorAction SilentlyContinue
    Remove-Item Builds/$name/*.deps.json -ErrorAction SilentlyContinue
    Remove-Item Builds/$name/ref -Recurse -ErrorAction SilentlyContinue

    # zip build 
    Compress-Archive -Path Builds/$name/* -DestinationPath Builds/$name.zip
    Remove-Item Builds/$name -Recurse -ErrorAction SilentlyContinue
}
$json = $json.Substring(0, $json.lastIndexOf(',')) + "`n"
$json += ']';

Set-Content -Path 'plugins.json' -Value $json

Remove-Item ../FileFlows/Server/Plugins/Plugin.dll -ErrorAction SilentlyContinue
Remove-Item ../FileFlows/Server/Plugins/*.deps.json -ErrorAction SilentlyContinue
Remove-Item ../FileFlows/Server/Plugins/ref -Recurse -ErrorAction SilentlyContinue
