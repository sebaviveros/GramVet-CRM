# =============================================================================
#  Respaldo diario de la base GramVetCRM (SQL Server Express)
#  Lo ejecuta una Tarea Programada de Windows (ver Register-ScheduledTask abajo).
#  Deja el .bak en C:\GramVet\backups y borra los de más de 14 días.
# =============================================================================
$ErrorActionPreference = "Stop"

$db    = "GramVetCRM"
$dir   = "C:\GramVet\backups"
$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$file  = Join-Path $dir "$($db)_$stamp.bak"

# Backup completo (INIT/FORMAT = archivo nuevo por corrida)
# -C = confiar en el certificado autofirmado de SQL (equivale a TrustServerCertificate=True;
#      obligatorio con ODBC Driver 18, que cifra y valida el cert por defecto)
$sql = "BACKUP DATABASE [$db] TO DISK = N'$file' WITH INIT, FORMAT, NAME = N'$db-full';"
sqlcmd -S "localhost\SQLEXPRESS" -E -C -b -Q $sql

# Retención: eliminar respaldos de más de 14 días
Get-ChildItem $dir -Filter "*.bak" |
    Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-14) } |
    Remove-Item -Force
