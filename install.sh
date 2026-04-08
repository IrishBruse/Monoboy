#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="$SCRIPT_DIR/Monoboy.Desktop/Monoboy.Desktop.csproj"
INSTALL_DIR="${HOME}/.local/bin"
INSTALL_PATH="${INSTALL_DIR}/monoboy"
PUBLISH_DIR="$(mktemp -d)"

if ! command -v dotnet >/dev/null 2>&1; then
    echo "dotnet SDK is required but was not found on PATH."
    exit 1
fi

if [[ ! -f "$PROJECT_PATH" ]]; then
    echo "Could not find desktop project at: $PROJECT_PATH"
    exit 1
fi

uname_s="$(uname -s)"
uname_m="$(uname -m)"

if [[ "$uname_s" != "Linux" ]]; then
    echo "This install script currently supports Linux only."
    exit 1
fi

case "$uname_m" in
    x86_64) RID="linux-x64" ;;
    aarch64|arm64) RID="linux-arm64" ;;
    *)
        echo "Unsupported CPU architecture: $uname_m"
        exit 1
        ;;
esac

echo "Publishing Monoboy for $RID..."
echo "Publish directory: $PUBLISH_DIR"
dotnet publish "$PROJECT_PATH" \
    -c Release \
    -r "$RID" \
    -p:PublishSingleFile=true \
    -o "$PUBLISH_DIR"

# Remove debug artifacts from the published output.
rm -f "$PUBLISH_DIR"/*.pdb "$PUBLISH_DIR"/*.dbg

PUBLISHED_BINARY="$PUBLISH_DIR/Monoboy.Desktop"
if [[ ! -f "$PUBLISHED_BINARY" ]]; then
    echo "Publish succeeded but binary was not found: $PUBLISHED_BINARY"
    exit 1
fi

mkdir -p "$INSTALL_DIR"
install -m 0755 "$PUBLISHED_BINARY" "$INSTALL_PATH"

# Install published native libraries next to the executable so DllImport can resolve them.
while IFS= read -r native_lib; do
    install -m 0644 "$native_lib" "$INSTALL_DIR/"
done < <(compgen -G "$PUBLISH_DIR/*.so*" || true)

echo
echo "Installed: $INSTALL_PATH"
echo "Published files kept at: $PUBLISH_DIR"
echo "Run with:"
echo "  monoboy --debug"
echo
echo "If 'monoboy' is not found, add this to your shell profile:"
echo "  export PATH=\"\$HOME/.local/bin:\$PATH\""
