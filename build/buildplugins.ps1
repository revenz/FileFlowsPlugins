Write-Output "##################################"
Write-Output "###      Building Plugins      ###"
Write-Output "##################################"


& dotnet utils/spellcheck/spellcheck.dll ../
if ($LASTEXITCODE -ne 0 ) {
    Write-Error "Spellcheck failed"
    exit
}

$output = $args[0]
if ([String]::IsNullOrEmpty($output)) {
    $output = '../deploy';
}

if ([System.IO.Directory]::Exists($output) -eq $false) {        
    [System.IO.Directory]::CreateDirectory($output)
}

$output = $output | Resolve-Path

Remove-Item Builds  -Recurse -ErrorAction SilentlyContinue

$revision = (git rev-list --count --first-parent HEAD) -join "`n"
$version = "0.6.2.$revision"

$json = "[`n"

Get-ChildItem -Path ..\ -Filter *.csproj -Recurse -File -Name | ForEach-Object {
    $csproj = '../' + $_
    # update version number of builds
    (Get-Content $csproj) `
        -replace '(?<=(Version>))([\d]+\.){3}[\d]+(?=<)', $version |
    Out-File $csproj

        
    $package = [System.IO.Path]::GetFileNameWithoutExtension($_) 
    Write-Output "Building Plugin $package"
    # build an instance for FileFlow local code
    dotnet build $csproj /p:WarningLevel=1 --configuration Release /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary --output:$output/$package/  
    Remove-Item $output/$package/FileFlows.Plugin.dll -ErrorAction SilentlyContinue
    Remove-Item $output/$package/FileFlows.Plugin.pdb -ErrorAction SilentlyContinue
    Remove-Item $output/$package/*.deps.json -ErrorAction SilentlyContinue
    Remove-Item $output/$package/ref -Recurse -ErrorAction SilentlyContinue

    & dotnet utils/PluginInfoGenerator/PluginInfoGenerator.dll $output/$package/$package.dll $csproj
    
    Move-Item $output/$package/*.plugininfo $output/$package/.plugininfo -Force
    Move-Item $output/$package/*.nfo $output/$package/.nfo -Force

    if ( (Test-Path -Path $output/$package/.plugininfo -PathType Leaf) -and (Test-Path -Path $output/$package/.nfo -PathType Leaf)) {

        # only actually create the plugin if plugins were found in it      
        
        #read nfo file
        # build server needs ../, locally we cant have it
        if ([System.IO.File]::Exists("$output/$package/.nfo") -eq $false) {
            Write-Error "Failed to locate nfo file $output/$package/.nfo"
        }
        $pluginNfo = [System.IO.File]::ReadAllText("$output/$package/.nfo");
        Write-Output "Plugin NFO: $pluginNfo"
        $json += $pluginNfo + ",`n"
        [System.IO.File]::Delete("$output/$package/.nfo")

        Move-Item $output/$package/*.en.json $output/$package/en.json -Force

        # construct .ffplugin file
        $compress = @{
            Path             = "$output/$package/*"
            CompressionLevel = "Optimal"
            DestinationPath  = "$output/$package.zip"
        }
        Write-Output "Creating zip file $output/$package.zip"

        Compress-Archive @compress

        Write-Output "Creating plugin file $output/$package.ffplugin"
        Move-Item "$output/$package.zip" "$output/$package.ffplugin" -Force

        if ([String]::IsNullOrEmpty($output2) -eq $false) {
            Write-Output "Moving file to $output2"        
            Copy-Item "$output/$package.ffplugin" "$output2/" -Force
        }
    }
    else {
        Write-Warning "WARNING: Failed to generate plugin info files for: $package"        
    }

    Remove-Item $output/$package -Recurse -ErrorAction SilentlyContinue
}

$json = $json.Substring(0, $json.lastIndexOf(',')) + "`n"
$json += ']';

Set-Content -Path "$output/plugins.json" -Value $json
