# Expanded Clothes
A mod for My Winter Car, Small tweaks for improved logic clothes

<i>I was embarrassed to post the source code because it consists of vibecode, but this moment had to come</i>

## Resources used
- [MSCLoader](https://github.com/piotrulos/MSCModLoader/releases)
- [Universal Shopping System](https://www.nexusmods.com/mywintercar/mods/796)
- [HowMuchIsLeft](https://www.nexusmods.com/mywintercar/mods/724)

## Mod Settings
The mod settings are flexible, practically everything can be disabled

## Skin support
The mod supports custom skins
- [Download default skin pack](https://www.nexusmods.com/Core/Libs/Common/Widgets/DownloadPopUp?id=2788&game_id=8597)

## Expanded Clothes API
The mod contains a simple API for modders.

For example

```c#
using Expanded_Clothes;

// ...

private void Mod_OnLoad()
{
      if (ModLoader.IsModPresent("Expanded_Clothes"))
      {
          // create shelf
          Expanded_Clothes.API.Shelf("Country_Garage", new Vector3(-1132.719f, 2.55f, 1270.121f), new Vector3(270f, 250f, 0f));
          // create coat rack with random hats (boolean true)
          Expanded_Clothes.API.CoatRack("PUB", new Vector3(-1543.4f, 4.23f, 1185.4f), new Vector3(0f, 285f, 0f), true);
          // create washer
          Expanded_Clothes.API.Washer("House", new Vector3(-13.4f, 0f, 3.85f), new Vector3(0f, 0f, 0f));
      }
}
```

API.Register see in [Futufon.cs](Futufon.cs)

## Credits
<b>Huge thanks</b>
<b>honeycomb936</b>: idea with mod, help with code
<b>DUBOVYK</b>: high-quality hand textures
<b>traxr</b>: high-quality clothes
<b>cinnerax</b>: high-quality 3d-models
## Resources used
<b>Script from HowMuchIsLeft</b>: for cloth status <i>[ItemContentDescription.cs](https://github.com/thurbridi/MSC-HowMuchIsLeft/blob/master/HowMuchIsLeft/ItemContentDescription.cs)</i> used in [StateDirty.cs](Open%20Source/StateDirty.cs)
