param($path)

echo $path
rm $path/*.md5sum
$files = Get-ChildItem $path
$hashes = [ordered]@{}
$summary = "
> [!important]
> ����ʱ��ע��˶��ļ�MD5�Ƿ���ȷ��

| �ļ��� | MD5 |
| --- | --- |
"

foreach ($i in $files) {
    $name = $i.Name
    $hash = Get-FileHash $i -Algorithm MD5
    $hashString = $hash.Hash
    $hashes.Add($name, $hashString)
    Write-Output $hash.Hash > "${i}.md5sum"
    $summary +=  "| $name | ``${hashString}`` |`n"
}

echo $hashes

$json = ConvertTo-Json $hashes -Compress

$summary +=  "`n<!-- CLASSISLAND_PKG_MD5 ${json} -->" 
echo $summary > "$path/checksums.md"
Write-Host "MD5 Summary:" -ForegroundColor Gray
Write-Host $summary -ForegroundColor Gray
Write-Host "----------" -ForegroundColor Gray

#if (-not $GITHUB_ACTION -eq $null) {
#    'MD5_SUMMARY=' + $summary.Replace("`n", "<<") | Out-File -FilePath $env:GITHUB_ENV -Append
#}
