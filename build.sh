rm -rf BossRush/bin
dotnet restore
dotnet build
rm -rf ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/BossRush/BepInEx/plugins/BossRush
cp -r BossRush/bin/Debug/netstandard2.0/  ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/BossRush/BepInEx/plugins/BossRush
cp -r BossRush/libs/YAU.dll  ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/BossRush/BepInEx/plugins/BossRush/YAU.dll

rm build/*

cp BossRush/bin/Debug/netstandard2.0/*.dll build
rm build/YAU.dll
cp manifest.json build
cp README.md build
cp icon.png build
cd build
zip ../bossrush.zip *
cd ..

