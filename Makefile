all: build
BEPINEX_VERSION = 5

clean:
	@dotnet clean CS2ModsTesting.csproj

restore:
	@dotnet restore CS2ModsTesting.csproj

build: clean restore
	@dotnet build CS2ModsTesting.csproj /p:BepInExVersion=$(BEPINEX_VERSION)
	

run: 
	E:\SteamLibrary\steamapps\common\Cities Skylines II\Cities2.exe -developerMode

package-win:
	@-mkdir dist
	@cmd /c copy /y "bin\Debug\netstandard2.1\0Harmony.dll" "dist\"
	@cmd /c copy /y "bin\Debug\netstandard2.1\CS2ModsTesting.dll" "dist\"
	@echo Packaged to dist/

package-unix: build
	@-mkdir dist
	@cp bin/Debug/netstandard2.1/0Harmony.dll dist
	@cp bin/Debug/netstandard2.1/CS2ModsTesting.dll dist
	@echo Packaged to dist/