# Expanded Clothes
A mod for My Winter Car, Small tweaks for improved logic clothes

<i>I was embarrassed to post the source code because it consists of vibecode, but this moment had to come</i>

[![Expanded Clothes — NexusMods](https://raw.githubusercontent.com/morrics/Assets/refs/heads/main/MWC_page.png)](https://www.nexusmods.com/mywintercar/mods/532)


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
            // Use handlers so that if the player has not installed the mod, no errors appear in the console
            // DO NOT MAKE YOUR MOD DEPENDENT ON MINE IF IT IS NOT NECESSARY
            EX_Load();
      }
}

private void EX_Load()
{
    // create shelf
    Expanded_Clothes.API.Shelf("YOURMODID", new Vector3(0f, 0f, 0f), new Vector3(270f, 0f, 0f));
    // create coat rack with random hats (boolean true)
    Expanded_Clothes.API.CoatRack("YOURMODID", new Vector3(0f, 0f, 04f), new Vector3(0f, 0f, 0f), true);
    // create washer
    Expanded_Clothes.API.Washer("YOURMODID", new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f));
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
