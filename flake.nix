{
  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
  inputs.my-nix.url = "github:WhiteBlackGoose/my-nix/master";
  outputs = { nixpkgs, my-nix, ... }: 
  let 
    systems = [
      { arch-nix = "x86_64-linux"; arch-dn =   "linux-x64"; }
      { arch-nix = "aarch64-linux"; arch-dn =  "linux-arm64"; }
      { arch-nix = "x86_64-darwin"; arch-dn =  "darwin-x64";    }
      { arch-nix = "aarch64-darwin"; arch-dn = "darwin-arm64"; }
    ]; in
  {
    devShells = nixpkgs.lib.genAttrs systems (arch: {
      default = my-nix.dotnetShell 
        nixpkgs.legacyPackages.${arch.arch-nix}
          (p: [ p.sdk_7_0 ])
          (p: [ ])
          ;
    });

    packages = nixpkgs.lib.genAttrs systems (arch: with nixpkgs.legacyPackages.${arch.arch-nix}; {
      default = 
        buildDotnetPackage {
          pname = "amcli";
          version = builtins.readFile "./VERSION/VERSION";
          src = ./.;
          xBuildFiles = [
            "./CLI.csproj"
          ];
          nativeBuildInputs = [
            pkg-config
          ];
        };
    });
  };
}
