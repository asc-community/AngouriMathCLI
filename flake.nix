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
        buildDotnetModule {
          pname = "amcli";
          version = builtins.readFile VERSION/VERSION;
          src = ./.;
          nugetDeps = ./deps.nix;
          nativeBuildInputs = [
            pkgs.installShellFiles
          ];
          dotnet-sdk = dotnetCorePackages.sdk_7_0;
          dotnet-runtime = dotnetCorePackages.runtime_7_0;
          projectFile = "./amcli.csproj";
          preConfigure = ''
            mv CLI.csproj amcli.csproj
          '';
          postInstall = ''
            installManPage amcli.1
          '';
        };
        meta = {
          mainProgram = "amcli";
        };
    });
  };
}
