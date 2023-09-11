using System;
using System.Collections.Generic;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Menus;
using StardewValley.Locations;
using Microsoft.Xna.Framework;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using StardewModdingAPI;

namespace ExpandedFridge
{
    //* Collections of static methods and variables.
    class modUtilities
    {
        public const int MiniFridgeSheetIndex = 216;
        private const int OutOfBoundsTileY = -300;

        //* Wrapper for getting players current location.
        public static GameLocation CurrentLocation { get { return Game1.player.currentLocation; } }
        
        //* Wrapper for checking point inside map bounds.
        private static bool IsPointInsideMapBounds(Vector2 point, GameLocation location)
        {
            return location.isTileOnMap(point);
        }

        //* Get a free tile for chest placement in a location.
        //WARNING: This can return a value outside the map bounds.
        private static Vector2 GetFreeTileInLocation(GameLocation location)
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

            ModEntry.DebugLog("WARNING: Object might become placed out of bounds at tile X:" + x + " Y:" + y + " in " + location.NameOrUniqueName, LogLevel.Warn);

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
            List<Chest> miniFridges = new List<Chest>();
            foreach(StardewValley.Object obj in location.Objects.Values){
                if (obj != null && obj.bigCraftable.Value && obj is Chest && obj.ParentSheetIndex == MiniFridgeSheetIndex){
                    Chest chest_tmp = obj as Chest;
                    miniFridges.Add(chest_tmp);
                }
            }
            return miniFridges.ToArray();
        }
        
        //* Moves all mini fridges in all farmhouses out of the map bounds.
        public static void HideMiniFridgesInLocation(GameLocation location)
        {
            List<Vector2> miniFridgePositions = new List<Vector2>();

            //* find all mini-fridges positions.
            foreach(StardewValley.Object obj in location.objects.Values){
                if (obj != null && obj.bigCraftable.Value && obj is Chest && obj.ParentSheetIndex == MiniFridgeSheetIndex){
                    if (IsPointInsideMapBounds(obj.TileLocation, location)){
                        miniFridgePositions.Add(obj.TileLocation);
                    }
                }
            }

            //* Quit here if we dont have any mini-fridges
            if (miniFridgePositions.Count == 0){
                ModEntry.DebugLog("No mini-fridges found!");
                return;
            }
            
            //* move them.
            ModEntry.DebugLog("Moving mini-fridges out of view..");
            foreach (var v in miniFridgePositions)
            {
                int x = 0;
                Vector2 newPosition = new Vector2(x, OutOfBoundsTileY);

                while (location.objects.ContainsKey(newPosition))
                    newPosition.X = ++x;

                StardewValley.Object obj = location.objects[v];
                obj.TileLocation = newPosition;

                location.objects.Remove(v);
                location.objects.Add(newPosition, obj);
                ModEntry.DebugLog("Moved mini-fridge from X:" + v.X + " Y:" + v.Y + " to X:" + newPosition.X +  " Y:" + newPosition.Y);
            }

            ModEntry.DebugLog(location.NameOrUniqueName + " Finished!");
        }

        //* Moves all mini fridges in the location back into map bounds.
        public static void ShowMiniFridgesInLocation(GameLocation location)
        {
            //* find all mini-fridges positions.
            List<Vector2> miniFridgePositions = new List<Vector2>();
            foreach(StardewValley.Object obj in location.objects.Values){
                if (obj != null && obj.bigCraftable.Value && obj is Chest && obj.ParentSheetIndex == MiniFridgeSheetIndex){
                    if (!IsPointInsideMapBounds(obj.TileLocation, location)){
                        miniFridgePositions.Add(obj.TileLocation);
                    }
                }
            }

            //* Quit here if we dont have any mini-fridges
            if (miniFridgePositions.Count == 0){
                ModEntry.DebugLog("No mini-fridges found!");
                return;
            }

            //* Move them
            ModEntry.DebugLog("Moving mini-fridges back into view..");
            foreach (Vector2 v in miniFridgePositions)
            {
                Vector2 newPosition = GetFreeTileInLocation(location);
                StardewValley.Object obj = location.objects[v];
                obj.TileLocation = newPosition;

                location.objects.Remove(v);
                location.objects.Add(newPosition, obj);
                ModEntry.DebugLog("Moved mini-fridge from X:" + v.X + " Y:" + v.Y + " to X:" + newPosition.X +  " Y:" + newPosition.Y);
                
            }
            ModEntry.DebugLog(location.NameOrUniqueName + " Finished!");
        }
    }
}
