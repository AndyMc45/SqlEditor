param([string]$ProjectDir, [string]$OutDir)

Write-Output 'hello'

$result1 = Split-Path -Path $ProjectDir -parent  
$result1 = $result1 + "\SqlEditor.Installer\TranscriptPlugIn\"
$destinationPath = Convert-Path -Path $result1
$OutDirFiltered = $OutDir + "SqlEditor.TranscriptPlugin*.*" 

#copy filtered files
Copy-Item -Path $OutDirFiltered -Destination $destinationPath -Force

#Copy all directories
Get-ChildItem -Path $OutDir -Recurse 	| Where-Object {$_.PSIsContainer} 	| 
		ForEach-Object 
		{   
			$dest = Join-Path $destinationPath $_.FullName.Substring($OutDir.Length);
			New-Item -ItemType Directory - Path $dest -Force;
		}
		

