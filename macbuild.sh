cd libuv/
rm -rf build
rm -rf uv.xcodeproj
mkdir -p build
git clone https://git.chromium.org/external/gyp.git build/gyp
./gyp_uv.py -f xcode -Duv_library=shared_library
xcodebuild ARCHS="i386" -project uv.xcodeproj -configuration Release -target All
cp build/Release/libuv.dylib ../lib/