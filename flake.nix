{
  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
  inputs.my-nix.url = "github:WhiteBlackGoose/my-nix/master";
  outputs = { nixpkgs, my-nix, ... }: 
  let 
    systems = [
      "x86_64-linux"
      "aarch64-linux"
      "x86_64-darwin"
      "aarch64-darwin"
    ]; in
  {
    devShells = nixpkgs.lib.genAttrs systems (arch: {
      default = my-nix.dotnetShell 
        nixpkgs.legacyPackages.${arch}
          (p: [ p.sdk_7_0 ])
          (p: [ ])
          ;
    });

    packages = nixpkgs.lib.genAttrs systems (arch: with nixpkgs.legacyPackages.${arch}; {
      default = 
        stdenv.mkDerivation {
          pname = "amcli";
          version = builtins.readFile VERSION/VERSION;
          src = ./.;
          DOTNET_ROOT = "${dotnetCorePackages.sdk_7_0}";
          nativeBuildInputs = [
            dotnetCorePackages.sdk_7_0
            msbuild
            zlib
            zlib.dev
            openssl
          ];
          buildInputs = [
            dotnetCorePackages.runtime_7_0
          ];
          buildPhase = ''
            runHook preBuild
            mkdir packages
            NUGET_PACKAGES=$(pwd)/packages dotnet restore
            NUGET_PACKAGES=$(pwd)/packages dotnet build -o . --no-restore
            runHook postBuild
          '';
          installPhase = ''
            runHook preInstall
            mkdir -p $out/bin
            mv CLI $out/bin/amcli
            runHook postInstall
          '';
        };
    });
  };
}
