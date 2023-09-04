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
        

        //* Is a given tile within the map bounds of the given location.
        public static bool IsPointInsideMapBounds(Point point, GameLocation location)
        {
            return location.isTileOnMap(point.X, point.Y);
        }

        //* Wrapper for checking point inside map bounds.
        public static bool IsPointInsideMapBounds(Vector2 point, GameLocation location)
        {
            return location.isTileOnMap(point);
        }

        //* Get an array of all locations that have fridges.
        //WARNING: If not on Master Game it could miss locations with fridges.
        //* Must use request locations or other way to ensure all locations on remote players.
        //TODO: rework GetAllFridgeHouses() logic to pass location.
        public static FarmHouse[] GetAllFridgeHouses()
        {
            List<FarmHouse> fridgeLocations = new List<FarmHouse>();
            ModEntry.DebugLog("Looking for fridge locations... ");

            foreach (var location in Game1.locations)
            {
                //* Farm House
                if((location is FarmHouse) && (location as FarmHouse).upgradeLevel > 0){
                    fridgeLocations.Add(location as FarmHouse);
                    ModEntry.DebugLog("Found a fridge at: " + location.name);
                }
                //* Farm Cabins
                else if (location is Farm){

                    foreach (var building in (location as Farm).buildings)
                    {
                        if (building.isCabin && building.daysOfConstructionLeft.Value <= 0 && (building.indoors.Value as FarmHouse).upgradeLevel > 0)
                        {
                            fridgeLocations.Add(building.indoors.Value as FarmHouse);
                            ModEntry.DebugLog("Found a fridge at: " + location.name);
                        }
                    }
                }
                //* Ginger Island
                else if (location is IslandFarmHouse){
                    //TODO: try passing location as IslandFarmHouse once reworked.
                    fridgeLocations.Add(location as FarmHouse);
                    ModEntry.DebugLog("Found a fridge at: " + location.name);
                }
            }

            return fridgeLocations.ToArray();
        }
        
        //* Get a free tile for chest placement in a location.
        //WARNING: This can return a value outside the map bounds.
        public static Vector2 GetFreeTileInLocation(GameLocation location)
        {
            ModEntry.DebugLog("Looking for a free tile..");
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

        //* Moves all mini fridges in all farmhouses out of the map bounds.
        //TODO: Try Passing location to MoveMiniFridgesOutOfMapBounds()
        public static void MoveMiniFridgesOutOfMapBounds()
        {
            
            ModEntry.DebugLog("Begin moving mini-fridges out of view..");

            foreach (var h in GetAllFridgeHouses())
            {
                List<Vector2> miniFridgePositions = new List<Vector2>();

                //* find all mini-fridges positions.
                foreach(StardewValley.Object obj in h.objects.Values){

                    if (obj != null && obj.bigCraftable.Value && obj is Chest && obj.ParentSheetIndex == MiniFridgeSheetIndex)
                    {
                        Chest chest_tmp = obj as Chest;
                        ModEntry.DebugLog("Found a mini-fridge at: X- " + chest_tmp.TileLocation.X + " Y- " + chest_tmp.TileLocation.Y);
                        if (IsPointInsideMapBounds(chest_tmp.TileLocation, h)){
                            ModEntry.DebugLog("(Mini-fridge is insideMapBounds)");
                            miniFridgePositions.Add(chest_tmp.TileLocation);
                        }
                    }
                }
                
                ModEntry.DebugLog("Begin moving!");

                //* move them.
                foreach (var v in miniFridgePositions)
                {
                    int x = 0;
                    Vector2 newPosition = new Vector2(x, OutOfBoundsTileY);

                    while (h.objects.ContainsKey(newPosition))
                        newPosition.X = ++x;

                    StardewValley.Object obj = h.objects[v];
                    obj.tileLocation.Value = newPosition;

                    h.objects.Remove(v);
                    h.objects.Add(newPosition, obj);

                    ModEntry.DebugLog("Old Position: X-" + v.X + " Y-" + v.Y);
                    ModEntry.DebugLog("NEW Position: X-" + newPosition.X + " Y-" + newPosition.Y);
                }

                ModEntry.DebugLog("Finished Moving!");
            }
            ModEntry.DebugLog("Complete!");
        }

        //* Moves all mini fridges in all farmhouses into map bounds.
        //TODO: Try Passing location to MoveMiniFridgesIntoMapBounds()
        public static void MoveMiniFridgesIntoMapBounds()
        {
            ModEntry.DebugLog("Begin moving mini-fridges into view..");

            foreach (var h in GetAllFridgeHouses())
            {
                ModEntry.DebugLog("Getting mini-fridge positions:");
                List<Vector2> miniFridgePositions = new List<Vector2>();

                //* find all mini-fridges positions.
                foreach(StardewValley.Object obj in h.objects.Values){

                    if (obj != null && obj.bigCraftable.Value && obj is Chest && obj.ParentSheetIndex == MiniFridgeSheetIndex)
                    {
                        Chest chest_tmp = obj as Chest;
                        ModEntry.DebugLog("Found a mini-fridge at: X- " + chest_tmp.TileLocation.X + " Y- " + chest_tmp.TileLocation.Y);
                        if (!IsPointInsideMapBounds(chest_tmp.TileLocation, h)){
                            ModEntry.DebugLog("(Mini-fridge is OutsideMapBounds)");
                            miniFridgePositions.Add(chest_tmp.TileLocation);
                        }
                    }
                }

                ModEntry.DebugLog("Begin moving!");
                foreach (Vector2 v in miniFridgePositions)
                {
                    Vector2 newPosition = GetFreeTileInLocation(h);
                    
                    StardewValley.Object obj = h.objects[v];
                    obj.tileLocation.Value = newPosition;

                    h.objects.Remove(v);
                    h.objects.Add(newPosition, obj);

                    ModEntry.DebugLog("Old Position: X-" + v.X + " Y-" + v.Y);
                    ModEntry.DebugLog("NEW Position: X-" + newPosition.X + " Y-" + newPosition.Y);
                }
                ModEntry.DebugLog("Finish Moving!");
            }
        ModEntry.DebugLog("All Done!");
        }
    
    }
}
