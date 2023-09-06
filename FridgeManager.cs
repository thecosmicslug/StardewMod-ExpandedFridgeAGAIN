using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Network;
using StardewValley.Locations;
using StardewModdingAPI;
using StardewModdingAPI.Events;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

//TODO: Add Github update key before uploading to nexus.

namespace ExpandedFridge
{
    //* Handles the tracking and implementation of managing mini fridges in each farmhouse so they can be hidden and accessed from the main fridge.
    public class FridgeManager
    {
        private static ModEntry _entry = null;
        private List<Chest> _fridges = new List<Chest>();
        bool _inFridgeMenu = false;

        //* Constructor starts tracking needed event for tracking fridge inventory menu.
        public FridgeManager(ModEntry entry)
        {
            _entry = entry;
            _entry.Helper.Events.Display.MenuChanged += OnMenuChanged;
            _entry.Helper.Events.GameLoop.DayStarted += OnDayStarted;
            _entry.Helper.Events.GameLoop.DayEnding += OnDayEnding;
            
            ModEntry.DebugLog("FridgeManager is now running.", LogLevel.Warn);

        }

        //* Get Locations
        //TODO: Need to test split-screen & multiplayer.
        public void MultiplayerMode(){
            if(Game1.IsMultiplayer){
                if (LocalMultiplayer.IsLocalMultiplayer()){
                    ModEntry.DebugLog("Multiplayer (Split Screen) detected.");
                }
                else if (Game1.IsMasterGame){
                    ModEntry.DebugLog("Multiplayer (Host) detected.");
                }else{
                    ModEntry.DebugLog("Multiplayer (Client) detected.");
                }
            }else{
                ModEntry.DebugLog("Singleplayer mode detected.");
            }
        }
        //* Get an array of all locations that have fridges.
        //WARNING: If not on Master Game it could miss locations with fridges.
        //* Must use request locations or other way to ensure all locations on remote players.
        public static GameLocation[] GetFridgeLocations(){

            List<GameLocation> gLocations = new List<GameLocation>();
            if(Game1.IsMultiplayer){
                //* Check Locations for Multiplayer.
                foreach (GameLocation location in _entry.Helper.Multiplayer.GetActiveLocations()){
                    //* FarmHouse has a fridge, but check it is enabled.
                    if((location is FarmHouse) && (location as FarmHouse).upgradeLevel > 0){
                        gLocations.Add(location);
                    }
                    //* There can be multiplayer cabins on Farm
                    else if (location is Farm){
                        var bFoundCabin = false;
                        foreach (var building in (location as Farm).buildings){
                            //* Check cabins for enabled fridges.
                            if (building.isCabin && building.daysOfConstructionLeft.Value <= 0 && (building.indoors.Value as FarmHouse).upgradeLevel > 0){
                                bFoundCabin = true;
                            }
                        }
                        if(bFoundCabin){
                            gLocations.Add(location);
                        }
                    }
                    //* Ginger Island. Another fridge location!
                    else if (location is IslandFarmHouse){
                        IslandFarmHouse GingerFarm = location as IslandFarmHouse;
                        //TODO: Test this on a savegame without ginger island unlocked.
                        if(GingerFarm.visited.Value){
                            gLocations.Add(location);
                        }
                    }
                }
            }else{
                //* Check locations for Singleplayer.
                foreach (GameLocation location in Game1.locations){
                    //* FarmHouse has a fridge, but check it is enabled.
                    if((location is FarmHouse) && (location as FarmHouse).upgradeLevel > 0){
                        gLocations.Add(location);
                    }
                    //* There can be multiplayer cabins on Farm
                    else if (location is Farm){
                        var bFoundCabin = false;
                        foreach (var building in (location as Farm).buildings){
                            //* Check cabins for enabled fridges.
                            if (building.isCabin && building.daysOfConstructionLeft.Value <= 0 && (building.indoors.Value as FarmHouse).upgradeLevel > 0){
                                bFoundCabin = true;
                            }
                        }
                        if(bFoundCabin){
                            gLocations.Add(location);
                        }
                    }
                    //* Ginger Island. Another fridge location!
                    else if (location is IslandFarmHouse){
                        IslandFarmHouse GingerFarm = location as IslandFarmHouse;
                        if(GingerFarm.visited.Value){
                            gLocations.Add(location);
                        }
                    }
                }
            }
            return gLocations.ToArray();
        }

        //* Detects fridge menu status and invokes OnFridge methods.
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            //* if fridge menu before, also accept any mini fridge as fridge menu 
            bool fridgeMenuLast = _inFridgeMenu;
            _inFridgeMenu = !fridgeMenuLast ? IsMenuOfCurrentFridge(e.NewMenu) : IsMenuOfCurrentFridges(e.NewMenu);
                
            //* invoke methods as needed
            if (fridgeMenuLast && _inFridgeMenu)
                OnFridgeUpdated(e.NewMenu as ItemGrabMenu);
            else if (fridgeMenuLast && !_inFridgeMenu)
                OnFridgeClosed();
            else if (!fridgeMenuLast && _inFridgeMenu)
                OnFridgeOpened(e.NewMenu as ItemGrabMenu);
        }

        //* Move mini fridges out of reach if option and master game.
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (Game1.IsMasterGame && _entry.Config.HideMiniFridges)
            {
                ModEntry.DebugLog("OnDayStarted(): Hiding mini-fridges from view.", LogLevel.Warn);
                modUtilities.MoveAllMiniFridges(true);
            }
        }
        
        //* Move mini fridges into reach if option and master game.
        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            if (Game1.IsMasterGame && _entry.Config.HideMiniFridges)
            {
                ModEntry.DebugLog("OnDayEnding(): move mini-fridges back for saving.", LogLevel.Warn);
                modUtilities.MoveAllMiniFridges(false);
            }
        }
        //* Is given menu of a main fridge in same location.
        private bool IsMenuOfCurrentFridge(IClickableMenu menu)
        {
            if (menu is ItemGrabMenu && modUtilities.CurrentLocation is FarmHouse)
                return (menu as ItemGrabMenu).context == (modUtilities.CurrentLocation as FarmHouse).fridge.Value;
            if (menu is ItemGrabMenu && modUtilities.CurrentLocation is IslandFarmHouse)
                return (menu as ItemGrabMenu).context == (modUtilities.CurrentLocation as IslandFarmHouse).fridge.Value;
            return false;
        }

        //* Is given menu of any fridge inventory tabs currently registred.
        private bool IsMenuOfCurrentFridges(IClickableMenu menu)
        {
            if (menu is ItemGrabMenu)
            {
                var grab = menu as ItemGrabMenu;
                foreach (var f in _fridges)
                    if (grab.context == f)
                        return true;
            }
            return false;
        }

        private bool _customMenuInitiated = false;
        private ItemGrabMenu _menu = null;

        //* Updates the menu reference of the fridge.
        private void OnFridgeUpdated(ItemGrabMenu menu)
        {
            ModEntry.DebugLog("Fridge tab changed.");
            _menu = menu;
        }

        //* Releases custom menu, mutexes and unsubscribes events.
        private void OnFridgeClosed()
        {
            if (_customMenuInitiated)
            {
                _entry.Helper.Events.Display.RenderingActiveMenu -= DrawBeforeActiveMenu;
                _entry.Helper.Events.Input.ButtonPressed -= RecieveButtonPressed;
                _entry.Helper.Events.Input.MouseWheelScrolled -= RecieveMouseWheelScrolled;

                //* release mutex if mini fridge selected
                if (_selectedTab > 0 && _selectedTab < _fridges.Count)
                    _fridges[_selectedTab].mutex.ReleaseLock();
                
                _menu = null;
                _fridges.Clear();
                ClearCustomComponents();
                _customMenuInitiated = false;
            }
            ModEntry.DebugLog("Fridge closed");
        }

        //* Initiates custom menu with references and events.
        private void OnFridgeOpened(ItemGrabMenu menu)
        {

            if (modUtilities.CurrentLocation is FarmHouse){
                //TODO: Test if this includes the cabins
                var farmHouse = modUtilities.CurrentLocation as FarmHouse;
                var miniFridges = modUtilities.GetAllMiniFridgesInLocation(farmHouse);
                _fridges.Add(farmHouse.fridge.Value);
                _fridges.AddRange(miniFridges);
                ModEntry.DebugLog("Fridge opened in " + farmHouse.Name);
            }
            else if (modUtilities.CurrentLocation is IslandFarmHouse){
                var farmHouse = modUtilities.CurrentLocation as IslandFarmHouse;
                var miniFridges = modUtilities.GetAllMiniFridgesInLocation(farmHouse);
                _fridges.Add(farmHouse.fridge.Value);
                _fridges.AddRange(miniFridges);
                ModEntry.DebugLog("Fridge opened in " + farmHouse.Name);
            }

            _menu = menu;
            _entry.Helper.Events.Display.RenderingActiveMenu += DrawBeforeActiveMenu;
            _entry.Helper.Events.Input.ButtonPressed += RecieveButtonPressed;
            _entry.Helper.Events.Input.MouseWheelScrolled += RecieveMouseWheelScrolled;
            UpdateCustomComponents();
            _customMenuInitiated = true;

        }

        //* Components for inventory tabs.
        private List<ClickableComponent> _fridgeTabs = new List<ClickableComponent>();
        private List<Color> _fridgeTabsColors = new List<Color>();
        private ClickableTextureComponent _rightArrowButton;
        private ClickableTextureComponent _leftArrowButton;

        private int _selectedTab = 0;
        private int _rootTab = 0;
        private bool _updateTabColors = false;

        //* Some constant offsets for adjusting positions of components.
        private const float textOffsetX = Game1.tileSize * 0.35F;
        private const float textOffsetY = Game1.tileSize * 0.38F;
        private const float textOffsetYSelected = Game1.tileSize * 0.25F;
        private const float textOffsetXSelected = Game1.tileSize * 0.32F;
        private const int colorOffsetX = (int)(Game1.tileSize * 0.2F);
        private const int colorOffsetY = (int)(Game1.tileSize * 0.2F);
        private const float colorSizeModY = 0.65F;
        private const float colorSizeModX = 0.625F;

        //* Updates needed menu components.
        private void UpdateCustomComponents()
        {
            //* create clickable components for tabs
            int i = 0;
            int labelX = _menu.xPositionOnScreen + Game1.tileSize / 2;
            int labelY = (int)(_menu.yPositionOnScreen + Game1.tileSize * 3.3F);
            for (int ii = 0; ii < _fridges.Count; ii++)
            {
                _fridgeTabs.Add(new ClickableComponent(new Rectangle(labelX + Game1.tileSize * i, labelY, Game1.tileSize, Game1.tileSize), (i).ToString(), (i++).ToString()));
                _fridgeTabsColors.Add(_fridges[ii].playerChoiceColor.Value);
            }

            //* left right arrow components for scrolling
            _rightArrowButton = new ClickableTextureComponent(new Rectangle(labelX + 12 + 12 * Game1.tileSize, labelY + 24, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false);
            _leftArrowButton = new ClickableTextureComponent(new Rectangle(labelX + -Game1.tileSize, labelY + 24, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false);
        }

        //* Clears custom menu components.
        private void ClearCustomComponents()
        {
            _fridgeTabs.Clear();
            _fridgeTabsColors.Clear();
            _rightArrowButton = null;
            _leftArrowButton = null;
            _selectedTab = 0;
            _rootTab = 0;
            _updateTabColors = false;
        }
        
        //* Switch leftmost fridge tab to the right.
        private void NextRootTab()
        {
            if (_rootTab + 12 < _fridges.Count)
            {
                _rootTab++;
                Game1.playSound("bigSelect");
            }
        }

        //* Switch leftmost fridge tab to the left.
        private void LastRootTab()
        {
            if (_rootTab > 0)
            {
                _rootTab--;
                Game1.playSound("bigSelect");
            }
        }

        //* Clamps the leftmost fridge tab so selected tab is visible.
        private void ClampRootTab()
        {
            if (_selectedTab > _rootTab + 11)
                _rootTab = _selectedTab - 11;
            else if (_rootTab > _selectedTab)
                _rootTab = _selectedTab;
        }

        //* Sets the selected tab and updates the inventory menu.
        private void SetSelectedTab(int tab)
        {
            if (tab >= 0 && tab < _fridges.Count && tab != _selectedTab)
            {
                if (tab == 0)
                {
                    //* release mutex if mini fridge selected
                    if (_selectedTab > 0 && _selectedTab < _fridges.Count)
                        _fridges[_selectedTab].mutex.ReleaseLock();

                    _selectedTab = tab;
                    if (tab > 0)
                        _menu = modUtilities.GetNewItemGrabMenuFromChest(_fridges[tab], true);
                    else
                        _menu = modUtilities.GetNewItemGrabMenuFromChest(_fridges[tab], false);

                    Game1.activeClickableMenu = _menu;
                    Game1.playSound("smallSelect");
                }
                else
                {
                    //* request mini fridge to be opened
                    _fridges[tab].mutex.RequestLock(() =>
                    {
                        //* release mutex if mini fridge selected
                        if (_selectedTab > 0 && _selectedTab < _fridges.Count)
                            _fridges[_selectedTab].mutex.ReleaseLock();

                        _selectedTab = tab;
                        if (tab > 0)
                            _menu = modUtilities.GetNewItemGrabMenuFromChest(_fridges[tab], true);
                        else
                            _menu = modUtilities.GetNewItemGrabMenuFromChest(_fridges[tab], false);

                        Game1.activeClickableMenu = _menu;
                        Game1.playSound("smallSelect");
                    }, () =>
                    {
                        Game1.playSound("cancel");
                    });
                }
            }
        }

        //* Draws the custom overlay for the inventory menu.
        private void DrawBeforeActiveMenu(object sender, RenderingActiveMenuEventArgs e)
        {
            var igm = (Game1.activeClickableMenu as ItemGrabMenu);

            //* never allow an ItemGrabMenu to render its background while we are drawing FridgeMenu, it will do it instead.
            if (igm != null && igm.drawBG)
                igm.drawBG = false;
            e.SpriteBatch.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);

            //* if flagged, update tabs colors
            if (_updateTabColors)
            {
                _updateTabColors = false;
                _fridgeTabsColors.Clear();
                foreach (var c in _fridges)
                    _fridgeTabsColors.Add(c.playerChoiceColor.Value);
            }

            //* draw tabs
            int i = 1;
            for (int index = _rootTab; index < _fridgeTabs.Count && i++ <= 12; index++)
            {
                ClickableComponent tab = _fridgeTabs[index];

                int width = Game1.tileSize / 2;
                int height = width;
                int xpos = tab.bounds.X - (_rootTab * Game1.tileSize);

                IClickableMenu.drawTextureBox(e.SpriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), xpos, tab.bounds.Y, tab.bounds.Width, tab.bounds.Height, _selectedTab == index ? Color.White : new Color(0.3f, 0.3f, 0.3f, 1f), 1, false);
                Color tabCol = index == 0 ? Color.BurlyWood : _fridgeTabsColors[index] == Color.Black ? Color.BurlyWood : _fridgeTabsColors[index];
                e.SpriteBatch.Draw(Game1.staminaRect, new Rectangle(xpos + colorOffsetX, tab.bounds.Y + colorOffsetY, (int)(tab.bounds.Width * colorSizeModX), (int)(tab.bounds.Height * colorSizeModY)), tabCol);
                //NOTE: Make compatible with 'Better Chests' here?.
                if (index == 0)
                {
                    const float scaleSize = 2f;
                    Rectangle rectForBigCraftable = Object.getSourceRectForBigCraftable(modUtilities.MiniFridgeSheetIndex);
                    rectForBigCraftable.Height -= 16;
                    e.SpriteBatch.Draw(Game1.bigCraftableSpriteSheet, new Vector2(xpos + 32f, tab.bounds.Y + 32f + 21f), new Microsoft.Xna.Framework.Rectangle?(rectForBigCraftable), Color.White, 0.0f, new Vector2(8f, 16f), scaleSize, SpriteEffects.None, 1);
                }
                else
                {
                    int digit = int.Parse(tab.label);
                    if (digit < 10)
                        Utility.drawTinyDigits(digit, e.SpriteBatch, new Vector2(xpos + textOffsetX, tab.bounds.Y + textOffsetY), 4f, 1f, Color.White);
                    else
                        Utility.drawTinyDigits(digit, e.SpriteBatch, new Vector2(xpos + textOffsetX - 10, tab.bounds.Y + textOffsetY), 4f, 1f, Color.White);
                }

                if (_selectedTab != index)
                    e.SpriteBatch.Draw(Game1.staminaRect, new Rectangle(xpos + colorOffsetX, tab.bounds.Y + colorOffsetY, (int)(tab.bounds.Width * colorSizeModX), (int)(tab.bounds.Height * colorSizeModY)), new Color(0, 0, 0, 0.4f));
            }

            //* draw arrows if too many fridges
            if (_fridges.Count > 12)
            {
                if (_rootTab > 0)
                    _leftArrowButton.draw(e.SpriteBatch);
                if (_rootTab + 12 < _fridges.Count)
                    _rightArrowButton.draw(e.SpriteBatch);
            }
        }

        //* Recieves input and responds to custom menu button presses and hotkeys.
        private void RecieveButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.MouseLeft)
            {
                Point mouse = new Point(Game1.getMouseX(true), Game1.getMouseY(true));

                int i = 1;
                for (int index = _rootTab; index < _fridgeTabs.Count && i++ <= 12; index++)
                {
                    ClickableComponent tab = _fridgeTabs[index];

                    int width = Game1.tileSize / 2;
                    int height = width;

                    int xpos = tab.bounds.X - (_rootTab * Game1.tileSize);

                    var r = new Rectangle(xpos, tab.bounds.Y, tab.bounds.Width, tab.bounds.Height);
                    if (r.Contains(mouse))
                    {
                        SetSelectedTab(index);
                        break;
                    }
                }

                if (_leftArrowButton.containsPoint(mouse.X, mouse.Y))
                    LastRootTab();
                else if (_rightArrowButton.containsPoint(mouse.X, mouse.Y))
                    NextRootTab();

                if (_menu.chestColorPicker != null && _menu.chestColorPicker.isWithinBounds(mouse.X, mouse.Y))
                {
                    _updateTabColors = true;
                }
            }
            else if (e.Button == _entry.Config.NextFridgeTabButton)
            {
                SetSelectedTab(_selectedTab + 1);
                ClampRootTab();
            }
            else if (e.Button == _entry.Config.LastFridgeTabButton)
            {
                SetSelectedTab(_selectedTab - 1);
                ClampRootTab();
            }
        }

        //* Mouse wheel scrolling of tabs.
        private void RecieveMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if (e.Delta > 0)
            {
                SetSelectedTab(_selectedTab + 1);
                ClampRootTab();
            }
            else
            {
                SetSelectedTab(_selectedTab - 1);
                ClampRootTab();
            }
        }
    }
}
