# ExpandedFridge  + bugfixes
This project was forked from [https://github.com/Uwazouri/ExpandedFridge]

It now fully supports multiplayer & ginger island content,

I've added `GenericModConfigMenu` support to make managing the settings easier, 

and it also contains UI Scale fix from 'SampaioTS' 

I work in visual studio code on linux, to build on other platforms may well need adjustments.

## Managing Mini Fridges
Access mini fridges through the kitchen fridge using inventory tabs.

Simply place mini fridges in a farm house or cabin with a kitchen and they will be accessible from the kitchen fridge.

by default the next day you will find your placed mini fridges moved away from your living space. This can be disabled in the `config.json` or through `GenericModConfigMenu`.

SMAPI debug commands `FridgeExpanded_ShowFridges` and  `FridgeExpanded_HideFridges` are available if they get stuck.

It should also work fine in multiplayer mode.


## Requirements:
- `SMAPI 4.0+`
- `Stardew Valley 1.6+`


## In-Game Instructions:
Place mini fridges inside a farm house or cabin with a kitchen.

Open the kitchen fridge to access any mini fridges in the location.

Use inventory tabs, mouse scroll wheel or game-pad shoulder triggers to switch between inventory pages (mini fridges).

(Optional) - All mini fridges are moved in and out of the map bounds when day has started/before ending (on by default).

(Optional) - Edit the "config.json" file in the "ExpandedFridgeAGAIN" folder to set hotkeys or hide mini fridges option.


## Change log:
- 3.2.4 ﻿- Fixes the colour picker for mini-fridges. corrects a typo in the debug commands.
- 3.2.3 ﻿- Fixes the stuck fridges issue. Added commands FridgeExpanded_ShowFridges, FridgeExpanded_HideFridges.
- 3.2.2 ﻿- Added Japanese & Brazilian Portuguese translations. improved build process.
- 3.2.1 ﻿- small bugfixes.
- 3.2.0 ﻿- Recompiled & fixed for SDV 1.6
- 3.1.0 ﻿- Forked & Applied UI Scale bugfix.
- 3.0.0 ﻿- Mini Fridge Update (rework of entire mod).
- 2.0.3 ﻿- Changed from day to save events in fridge manipulation. Hopefully solves problems with Save Anywhere.
- 2.0.2 ﻿- Added null check in mutex update and new button placement for remote fridge button.
- 2.0.1 ﻿- Added option to toggle remote button in the fridge upgrade menu.
- 2.0.0 ﻿- Complete rework of entire mod.
- 1.2.8 ﻿- Changed settings for choosing fridge version to SMAPI friendly.
- 1.2.7 ﻿- Updated SMAPI code and added shift + right-click to transfer half stack.
- 1.2.6 ﻿- Fixed bug on mac with new settings file not being read.
- 1.2.5 ﻿- Escape key can be used to close the fridge and new system for choosing fridge version.
- 1.2.4 ﻿- Extra, Added Extra Large Fridge version.
- 1.2.4 ﻿- Added simple support for navigating the menu with a controller.
- 1.2.3 ﻿- Added an option for a scrolling menu.
- 1.2.2 ﻿- Fixed bug where fridge would not work if it was empty.
- 1.2.1 ﻿- Clicking and opening/closing now has sounds.
- 1.2.0 ﻿- Reworked menu manipulation. Should be more stable and work better with Chests Anywhere.
- 1.1.0 ﻿- Right clicking now works.

