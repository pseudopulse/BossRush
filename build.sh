rm -rf BossRush/bin
dotnet restore
dotnet build
rm -rf ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/BossRush/BepInEx/plugins/BossRush
cp -r BossRush/libs/YAU.dll  BossRush/bin/Debug/netstandard2.1/
cp -r BossRush/bin/Debug/netstandard2.1/  ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/BossRush/BepInEx/plugins/BossRush

rm build/*

cp BossRush/bin/Debug/netstandard2.1/*.dll build
cp manifest.json build
cp README.md build
cp icon.png build
cd build
zip ../bossrush.zip *
cd ..

