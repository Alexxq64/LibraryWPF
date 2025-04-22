<# 
clean_tree.ps1 - Полнофункциональный скрипт для отображения структуры проекта 
#>

# Настройки
$outputFile = "project_structure.txt"
$excludedDirs = @("bin", "obj", "Debug", "Release", "packages", "runtimes", ".git", ".gitattributes", ".gitignore", ".vs")
$excludedFiles = @("*.dll", "*.exe", "*.pdb", "*.cache", "*.json")

# Функция для отрисовки дерева
function Get-ProjectTree {
    param (
        [string]$rootPath = $PWD.Path,
        [string]$prefix = ""
    )

    $items = Get-ChildItem -Path $rootPath -Force | 
             Where-Object {
                 $_.Name -notin $excludedDirs -and
                 $_.Extension -notin $excludedFiles -and
		 $_.Name -ne ([System.IO.Path]::GetFileName($PSCommandPath)) -and
                 $_.Name -notmatch "wpftmp|\.suo|\.user"
             } | 
             Sort-Object Name

    $count = $items.Count
    $i = 0

    foreach ($item in $items) {
        $i++
        $isLast = ($i -eq $count)
        
        if ($isLast) {
            $line = $prefix + "`-- " + $item.Name  # Простая замена на ASCII
            $newPrefix = $prefix + "    "  # Пробелы для отступа
        }
        else {
            $line = $prefix + "|-- " + $item.Name  # Простая замена на ASCII
            $newPrefix = $prefix + "|   "  # Пробелы для отступа
        }

        $line | Out-File -FilePath $outputFile -Append -Encoding UTF8
        
        # Если это директория, рекурсивно продолжаем обход
        if ($item.PSIsContainer) {
            Get-ProjectTree -rootPath $item.FullName -prefix $newPrefix
        }
    }
}

# Очищаем предыдущий файл
if (Test-Path $outputFile) { Remove-Item $outputFile }

# Запускаем построение дерева
Get-ProjectTree

# Результат
Write-Host "Project structure saved to $outputFile" -ForegroundColor Green
Get-Content $outputFile
