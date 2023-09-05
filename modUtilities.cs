using System;
using System.Collections.Generic;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Menus;
using StardewValley.Locations;
using Microsoft.Xna.Framework;
using System.Collections.Specialized;
using System.Runtime.InteropServices;

namespace ExpandedFridge
{
    //* Collections of static methods and variables.
    class modUtilities
    {
        public const int MiniFridgeSheetIndex = 216;
        public const int OutOfBoundsTileY = -300;

        //* Wrapper for getting players current location.
        public static GameLocation CurrentLocation { get { return Game1.player.currentLocation; } }
        
        //* Wrapper for checking point inside map bounds.
        public static bool IsPointInsideMapBounds(Vector2 point, GameLocation location)
        {
            return location.isTileOnMap(point);
        }

        //* Get a free tile for chest placement in a location.
        //WARNING: This can return a value outside the map bounds.
        public static Vector2 GetFreeTileInLocation(GameLocation location)
        {
            for (int h = 0; h <= location.map.Layers[0].LayerHeight; h++)
                for (int w = 0; w <= location.map.Layers[0].LayerWidth; w++)
                    //* check if tile in width and height is placeable and not on wall
                    if (location.isTileLocationTotallyClearAndPlaceable(w, h) && (!(location is DecoratableLocation) || !(location as DecoratableLocation).isTileOnWall(w, h)))
                        return new Vector2(w, h);

            int y = 0;
            int x = 0;

            //* move in y direction untill no other potential offmap objects are there
            while (location.isObjectAtTile(x, y))
                y++;

            ModEntry.DebugLog("Warning, object might become placed out of bounds at tile x:" + x + ", y:" + y + " in location: " + location.Name, StardewModdingAPI.LogLevel.Warn);

            //* return that position
            return new Vector2(x, y);
        }

        //* Creates a new inventory menu from a chest with option for showing the color picker.
        public static ItemGrabMenu GetNewItemGrabMenuFromChest(Chest chest, bool showColorPicker)
        {
            var igm = new ItemGrabMenu((IList<Item>)chest.items, false, true, new
                    InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems),
                    new ItemGrabMenu.behaviorOnItemSelect(chest.grabItemFromInventory), (string)null,
                    new ItemGrabMenu.behaviorOnItemSelect(chest.grabItemFromChest), false, true, true, true, true, 1,
                    !showColorPicker ? (Item)null : (Item)chest, !showColorPicker ? -1 : 1, (object)chest);
            if (igm.chestColorPicker != null)
            {
                var r = igm.colorPickerToggleButton.bounds;
                r.Y -= 128+32;
                igm.colorPickerToggleButton.bounds = r;
                igm.chestColorPicker.itemToDrawColored = null;
            }
            if (igm.fillStacksButton != null)
            {
                igm.fillStacksButton.bounds = new Rectangle(igm.xPositionOnScreen + igm.width, igm.yPositionOnScreen + igm.height / 3 - 64 - 64 - 16, 64, 64);
            }
            return igm;
        }

        //* Get an array of mini fridge chests that exists in given location. They are sorted by their tile coordinates with Y as higher priority.
        public static Chest[] GetAllMiniFridgesInLocation(GameLocation location)
        {
            ModEntry.DebugLog("Searching for Mini-Fridges...");
            List<Chest> miniFridges = new List<Chest>();

            foreach(StardewValley.Object obj in location.Objects.Values){

                if (obj != null && obj.bigCraftable.Value && obj is Chest && obj.ParentSheetIndex == MiniFridgeSheetIndex)
                {
                    Chest chest_tmp = obj as Chest;
                    miniFridges.Add(chest_tmp);
                    ModEntry.DebugLog("Found a mini-fridge at X: " + chest_tmp.TileLocation.X + " Y: " + chest_tmp.TileLocation.Y);
                }
            }
            return miniFridges.ToArray();
        }

        //* Get an array of all locations that have fridges.
        //WARNING: If not on Master Game it could miss locations with fridges.
        //* Must use request locations or other way to ensure all locations on remote players.
        public static void MoveMiniFridgesIntoMapBounds(){

            foreach (GameLocation location in FridgeManager.GetFridgeLocations())
            {
                //* Farm House
                if((location is FarmHouse) && (location as FarmHouse).upgradeLevel > 0){
                            ModEntry.DebugLog("Found a fridge at: " + location.name);
                            ShowMiniFridgesInLocation(location);
                }
                //* Farm Cabins
                else if (location is Farm){

                    foreach (var building in (location as Farm).buildings)
                    {
                        if (building.isCabin && building.daysOfConstructionLeft.Value <= 0 && (building.indoors.Value as FarmHouse).upgradeLevel > 0)
                        {
                            ModEntry.DebugLog("Found a fridge at: " + location.name);
                            ShowMiniFridgesInLocation(location);
                        }
                    }
                }
                //* Ginger Island
                else if (location is IslandFarmHouse){
                    //TODO: Check we have actually visited and unlocked the IslandFarmHouse.
                    ModEntry.DebugLog("Found a fridge at: " + location.name);
                    ShowMiniFridgesInLocation(location);
                }
            }

        }
        //* Get an array of all locations that have fridges.
        public static void MoveMiniFridgesOutOfMapBounds(){
            
            foreach (GameLocation location in FridgeManager.GetFridgeLocations())
            {
                //* Farm House
                if((location is FarmHouse) && (location as FarmHouse).upgradeLevel > 0){
                    ModEntry.DebugLog("Found a fridge at: " + location.name);
                    HideMiniFridgesInLocation(location);
                }
                //* Farm Cabins
                else if (location is Farm){

                    foreach (var building in (location as Farm).buildings)
                    {
                        if (building.isCabin && building.daysOfConstructionLeft.Value <= 0 && (building.indoors.Value as FarmHouse).upgradeLevel > 0)
                        {
                            ModEntry.DebugLog("Found a fridge at: " + location.name);
                            HideMiniFridgesInLocation(location);
                        }
                    }
                }
                //* Ginger Island
                else if (location is IslandFarmHouse){
                    //TODO: Check we have actually visited and unlocked the IslandFarmHouse.
                    ModEntry.DebugLog("Found a fridge at: " + location.name);
                    HideMiniFridgesInLocation(location);
                }
            }
            
        }

        //* Moves all mini fridges in all farmhouses out of the map bounds.
        public static void HideMiniFridgesInLocation(GameLocation location)
        {
            List<Vector2> miniFridgePositions = new List<Vector2>();

            //* find all mini-fridges positions.
            foreach(StardewValley.Object obj in location.objects.Values){

                if (obj != null && obj.bigCraftable.Value && obj is Chest && obj.ParentSheetIndex == MiniFridgeSheetIndex)
                {
                    Chest chest_tmp = obj as Chest;
                    if (IsPointInsideMapBounds(chest_tmp.TileLocation, location)){
                        miniFridgePositions.Add(chest_tmp.TileLocation);
                    }
                }
            }

            ModEntry.DebugLog("Begin moving mini-fridges out of view..");

            //* move them.
            foreach (var v in miniFridgePositions)
            {
                int x = 0;
                Vector2 newPosition = new Vector2(x, OutOfBoundsTileY);

                while (location.objects.ContainsKey(newPosition))
                    newPosition.X = ++x;

                StardewValley.Object obj = location.objects[v];
                obj.tileLocation.Value = newPosition;

                location.objects.Remove(v);
                location.objects.Add(newPosition, obj);

                ModEntry.DebugLog("Moved mini-fridge from X:" + v.X + " Y:" + v.Y + " to X:" + newPosition.X +  " Y:" + newPosition.Y);
            }

            ModEntry.DebugLog("Finished moving mini-fridges!");
        }

        //* Moves all mini fridges in all farmhouses into map bounds.
        public static void ShowMiniFridgesInLocation(GameLocation location)
        {
            List<Vector2> miniFridgePositions = new List<Vector2>();

            //* find all mini-fridges positions.
            foreach(StardewValley.Object obj in location.objects.Values){

                if (obj != null && obj.bigCraftable.Value && obj is Chest && obj.ParentSheetIndex == MiniFridgeSheetIndex)
                {
                    Chest chest_tmp = obj as Chest;
                    if (!IsPointInsideMapBounds(chest_tmp.TileLocation, location)){
                        miniFridgePositions.Add(chest_tmp.TileLocation);
                    }
                }
            }

            ModEntry.DebugLog("Begin moving mini-fridges into view..");
            foreach (Vector2 v in miniFridgePositions)
            {
                Vector2 newPosition = GetFreeTileInLocation(location);
                
                StardewValley.Object obj = location.objects[v];
                obj.tileLocation.Value = newPosition;

                location.objects.Remove(v);
                location.objects.Add(newPosition, obj);

                ModEntry.DebugLog("Moved mini-fridge from X:" + v.X + " Y:" + v.Y + " to X:" + newPosition.X +  " Y:" + newPosition.Y);
                
            }
            ModEntry.DebugLog("Finished moving mini-fridges!");

        }

    }
}
