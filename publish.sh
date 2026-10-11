# bash ./publish.sh

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$ROOT/src/SaveManager.UI/SaveManager.UI.csproj"
OUT="$ROOT/publish/linux-x64"

if ! command -v dotnet >/dev/null 2>&1; then
    if [[ -x "$HOME/.dotnet/dotnet" ]]; then
        DOTNET="$HOME/.dotnet/dotnet"
    else
        echo "error: dotnet not found on PATH. Install the .NET 10 SDK." >&2
        exit 1
    fi
else
    DOTNET="dotnet"
fi

if [[ ! -f "$PROJECT" ]]; then
    echo "error: project not found at $PROJECT" >&2
    exit 1
fi

rm -rf "$OUT"

printf '\n==> linux-x64\n'

"$DOTNET" publish "$PROJECT" \
    -c Release \
    -r linux-x64 \
    --self-contained false \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:DebugType=None \
    -p:DebugSymbols=false \
    -o "$OUT"

find "$OUT" -maxdepth 1 -name '*.pdb' -delete

count="$(find "$OUT" -maxdepth 1 -type f | wc -l)"
total="$(du -sm "$OUT" | cut -f1)"

printf '    %s file(s), %s MB\n' "$count" "$total"

while IFS= read -r file; do
    printf '      %-28s %6s MB\n' "$(basename "$file")" "$(du -m "$file" | cut -f1)"
done < <(find "$OUT" -maxdepth 1 -type f | sort)

printf '\ndone.\n  run it with:  ./publish/linux-x64/SaveManager\n\n'