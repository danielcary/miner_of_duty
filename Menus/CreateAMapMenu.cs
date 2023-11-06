using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Miner_Of_Duty.LobbyCode;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework;
using Miner_Of_Duty.Game.Editor;
using Miner_Of_Duty.Game;

namespace Miner_Of_Duty.Menus
{
    public class CreateAMapMenu : IMenuOwner
    {
        public delegate void CreateMap(string mapName, string password, string teamAName, string teamBName, GameModes gameMode, int worldSize, WorldEditor.WorldGeneration generation, bool trees, bool weapons, bool editing);
        protected CreateMap createMap;
        protected Menu.BackPressed back;

        protected string mapName = "New Custom Map";
        protected string password = null;
        protected string teamAName = "Team Silverback";
        protected string teamBName = "Team Wavves";
        protected GameModes gameMode = GameModes.CustomTDM;
        protected int worldSize = 128;
        protected bool trees = true;
        protected bool weapons = true;
        protected bool editing = true;

        protected WorldEditor.WorldGeneration worldGeneration = WorldEditor.WorldGeneration.Random;

        protected Menu drawingNameMenu, drawingEditMenu;

        protected Menu nameMenu;
        protected Menu editMenu;

        protected Menu nameGenerationMenu, editGenerationMenu;
        protected Menu nameSettingsMenu, editSettingsMenu;

        private bool calledIt = false;
        public CreateAMapMenu(Menu.BackPressed back, CreateMap createMap)
        {
            this.back = back;
            this.createMap = createMap;

            nameMenu = new Menu(delegate(IMenuOwner sender, string id) {
                if (id == "create")
                {
                    if (password == "")
                        password = null;

                    
                    if(calledIt ==false)
                        createMap.Invoke(mapName, password, teamAName, teamBName, gameMode, worldSize, worldGeneration, trees, weapons, editing);

                    calledIt = true;
                }
                else if (id == "back")
                {
                    back.Invoke(this);
                }
                else if (id == "mapname")
                {
                    if (Guide.IsVisible == false)
                    {
                        try
                        {
                            Guide.BeginShowKeyboardInput((PlayerIndex)Input.ControllingPlayer, "New Custom Map Name",
                                "Enter the name for the new custom map. Anything longer than 15 characters will be cut off", mapName,
                                new AsyncCallback(delegate(IAsyncResult result)
                                    {
                                        string name = Guide.EndShowKeyboardInput(result);

                                        if (name == null)
                                            return;

                                        if (name.Length > 15)
                                            name = name.Substring(0, 15);

                                        for (int i = 0; i < name.Length; i++)
                                        {
                                            if (char.IsLetterOrDigit(name[i]) == false || Resources.Font.Characters.Contains(name[i]) == false)
                                                name = name.Replace(name[i], ' ');
                                        }

                                        mapName = NameFilter.FilterName(name);
  
                                    }), null);
                        }
                        catch (Exception) { }
                    }
                }
                else if (id == "password")
                {
                    if (Guide.IsVisible == false)
                    {
                        try
                        {
                            Guide.BeginShowKeyboardInput((PlayerIndex)Input.ControllingPlayer, "Enter Password", "Enter in the password.", password,
                                new AsyncCallback(delegate(IAsyncResult result)
                                {
                                    string name = Guide.EndShowKeyboardInput(result);

                                    if (name == null)
                                        return;

                                    string filteredPassword = name;

                                    for (int i = 0; i < filteredPassword.Length; i++)
                                    {
                                        if (char.IsLetterOrDigit(name[i]) == false || Resources.Font.Characters.Contains(name[i]) == false)
                                            filteredPassword = filteredPassword.Replace(name[i], ' ');
                                    }

                                    if (name.Equals(filteredPassword) == false)
                                    {
                                        //passwords were different

                                        while (Guide.IsVisible) ;

                                        try
                                        {
                                            Guide.BeginShowMessageBox("Error!", "Password contains invaild characters! Please use only letters or numbers.", new[] { "OK" }, 0, MessageBoxIcon.Error, null, null);
                                        }
                                        catch { }

                                        return;
                                    }

                                    password = name;

                                }), null);
                        }
                        catch (Exception) { }
                    }
                }
                else if (id == "teamaname")
                {
                    if (Guide.IsVisible == false)
                    {
                        try
                        {
                            Guide.BeginShowKeyboardInput((PlayerIndex)Input.ControllingPlayer, "Team A Name",
                                "Enter the name for Team A.", teamAName,
                                new AsyncCallback(delegate(IAsyncResult result)
                                {
                                    string name = Guide.EndShowKeyboardInput(result);

                                    if (name == null)
                                        return;

                                    if (name.Length > 15)
                                        name = name.Substring(0, 15);

                                    for (int i = 0; i < name.Length; i++)
                                    {
                                        if (char.IsLetterOrDigit(name[i]) == false || Resources.Font.Characters.Contains(name[i]) == false)
                                            name = name.Replace(name[i], ' ');
                                    }

                                    teamAName = NameFilter.FilterName(name);

                                }), null);
                        }
                        catch (Exception) { }
                    }
                }
                else if (id == "teambname")
                {
                    if (Guide.IsVisible == false)
                    {
                        try
                        {
                            Guide.BeginShowKeyboardInput((PlayerIndex)Input.ControllingPlayer, "Team B Name",
                                "Enter the name for Team B.", teamBName,
                                new AsyncCallback(delegate(IAsyncResult result)
                                {
                                    string name = Guide.EndShowKeyboardInput(result);

                                    if (name == null)
                                        return;

                                    if (name.Length > 15)
                                        name = name.Substring(0, 15);

                                    for (int i = 0; i < name.Length; i++)
                                    {
                                        if (char.IsLetterOrDigit(name[i]) == false || Resources.Font.Characters.Contains(name[i]) == false)
                                            name = name.Replace(name[i], ' ');
                                    }

                                    teamBName = NameFilter.FilterName(name);

                                }), null);
                        }
                        catch (Exception) { }
                    }
                }
                else if (id == "generation")
                {
                    drawingNameMenu = nameGenerationMenu; 
                    drawingEditMenu = editGenerationMenu;
                }
                else if (id == "settings")
                {
                    drawingNameMenu = nameSettingsMenu;
                    drawingEditMenu = editSettingsMenu;
                }

            }, delegate(object sender) { },
                new MenuElement[]
                {
                    new MenuElement("back", "Back"),
                    new MenuElement("mapname", "Map Name:"),
                    new MenuElement("password", "Password:"),
                    new MenuElement("teamaname", "Team A Name:"),
                    new MenuElement("teambname", "Team B Name:"),
                    new MenuElement("gamemode", "Game Mode:"),
                    new MenuElement("generation", "GENERATION SETTINGS"),
                    new MenuElement("settings", "IN-GAME SETTINGS"),
                    new MenuElement("create", "create map"),
                }, 100);

            


            editMenu = new Menu(delegate(IMenuOwner sender, string id) { }, delegate(object sender) { },
                    new MenuElement[]{
                        new MenuElement("blank", ""),
                        new MenuElement("mapname", mapName),
                        new MenuElement("password", ""),
                        new MenuElement("teamaname", teamAName),
                        new MenuElement("teambname", teamBName),
                        new ValueMenuElement("gameMode", GameModeToString(gameMode), 0, "", 0, 4, 100),
                        new MenuElement("generation", ""),
                        new MenuElement("settings", ""),
                        new MenuElement("blank", "")}, 550);

            nameMenu["back"].Position.X -= 75;
            nameMenu["create"].Position.X -= 75;
            nameMenu["create"].Position.Y += 0;

            (editMenu["gameMode"] as ValueMenuElement).ValueChangedEvent += new ValueMenuElement.ValueChanged(CreateAMapMenu_ValueChangedEvent);

            #region generation Menu
            nameGenerationMenu = new Menu(delegate(IMenuOwner sender, string id) { }, delegate(object sender) { },
                new MenuElement[]
                {
                    new MenuElement("title", "WORLD GENERATION SETTINGS"),
                    new MenuElement("back", "BACK"),
                    new MenuElement("worldsize", "World Size:"),
                    new MenuElement("generation", "Generation Type:"),
                    new MenuElement("trees", "GENERATE TREES:"),
                }, 100, 1);

            nameGenerationMenu["title"].Position.X -= 75;

            editGenerationMenu = new Menu(delegate(IMenuOwner sender, string id)
                {
                    if (id == "back")
                    {
                        drawingNameMenu = nameMenu; drawingEditMenu = editMenu;
                    }
                }, delegate(object sender) { drawingNameMenu = nameMenu; drawingEditMenu = editMenu; },
                new MenuElement[]
                {
                    new MenuElement("blank",""),
                    new MenuElement("back", ""),
                    new ValueMenuElement("worldsize", "", worldSize, "", 16, 128, 50),
                    new ValueMenuElement("generation",  GenerationToString(worldGeneration), 0, "", 0, 3, 100),
                    new BooleanValueMenuElement("trees", "TRUE", true, 400),
                }, 550, 1);

            (editGenerationMenu["worldsize"] as ValueMenuElement).ValueChangedEvent += new ValueMenuElement.ValueChanged(CreateAMapMenu_ValueChangedEvent);
            (editGenerationMenu["generation"] as ValueMenuElement).ValueChangedEvent += new ValueMenuElement.ValueChanged(CreateAMapMenu_ValueChangedEvent);
            (editGenerationMenu["trees"] as BooleanValueMenuElement).ValueChangedEvent += new BooleanValueMenuElement.ValueChanged(CreateAMapMenu_ValueChangedEvent);
            #endregion

            #region name settings menu
            nameSettingsMenu = new Menu(delegate(IMenuOwner sender, string id) { }, delegate(object sender) { },
                new MenuElement[]
                {
                    new MenuElement("title", "IN-GAME SETTINGS"),
                    new MenuElement("back", "BACK"),
                    new MenuElement("weapons", "WEAPONS ENABLED:"),
                    new MenuElement("editing", "MAP EDITING ENABLED:"),
                }, 100, 1);
            nameSettingsMenu["title"].Position.X -= 75;

            editSettingsMenu = new Menu(delegate(IMenuOwner sender, string id)
                {
                    if (id == "back")
                    {
                        drawingNameMenu = nameMenu; drawingEditMenu = editMenu;
                    }
                }, delegate(object sender) { drawingNameMenu = nameMenu; drawingEditMenu = editMenu; },
                new MenuElement[]
                {
                    new MenuElement("blank",""),
                    new MenuElement("back", ""),
                    new BooleanValueMenuElement("weapons", "TRUE", true, 400),
                    new BooleanValueMenuElement("editing", "TRUE", true, 400),
                }, 550, 1);

            (editSettingsMenu["weapons"] as BooleanValueMenuElement).ValueChangedEvent += new BooleanValueMenuElement.ValueChanged(CreateAMapMenu_ValueChangedEvent);
            (editSettingsMenu["editing"] as BooleanValueMenuElement).ValueChangedEvent += new BooleanValueMenuElement.ValueChanged(CreateAMapMenu_ValueChangedEvent);
            #endregion

            drawingEditMenu = editMenu;
            drawingNameMenu = nameMenu;
        }

        private void CreateAMapMenu_ValueChangedEvent(string id)
        {
            if (id == "worldsize")
            {
                int difference = (int)(editGenerationMenu["worldsize"] as ValueMenuElement).Value - worldSize;

                worldSize += 2 * difference;

                if(difference != 0)
                    (editGenerationMenu["worldsize"] as ValueMenuElement).Value = worldSize;
            }
            else if (id == "gameMode")
            {
                int val = (int)(editMenu["gameMode"] as ValueMenuElement).Value;


                editMenu["teamaname"].CanSelectMe = true;
                editMenu["teamaname"].NormalTextColor = Color.White;
                editMenu["teamaname"].SelectedTextColor = Color.Green;

                editMenu["teambname"].CanSelectMe = true;
                editMenu["teambname"].NormalTextColor = Color.White;
                editMenu["teambname"].SelectedTextColor = Color.Green;

                nameMenu["teamaname"].CanSelectMe = true;
                nameMenu["teamaname"].NormalTextColor = Color.White;
                nameMenu["teamaname"].SelectedTextColor = Color.Green;

                nameMenu["teambname"].CanSelectMe = true;
                nameMenu["teambname"].NormalTextColor = Color.White;
                nameMenu["teambname"].SelectedTextColor = Color.Green;

                if (val == 0)
                {
                    gameMode = GameModes.CustomTDM;
                }
                else if (val == 1)
                {
                    gameMode = GameModes.CustomSNM;
                }
                else if(val == 2)
                {
                    gameMode = GameModes.CustomFFA;
                }
                else if (val == 3)
                {
                    gameMode = GameModes.CustomSM;
                }
                else if(val == 4)
                    gameMode = GameModes.CustomKB;

                if(val == 2 || val == 3 || val == 4)
                {
                    editMenu["teamaname"].CanSelectMe = false;
                    editMenu["teamaname"].NormalTextColor = Color.Gray;
                    editMenu["teamaname"].SelectedTextColor = Color.Red;

                    editMenu["teambname"].CanSelectMe = false;
                    editMenu["teambname"].NormalTextColor = Color.Gray;
                    editMenu["teambname"].SelectedTextColor = Color.Red;

                    nameMenu["teamaname"].CanSelectMe = false;
                    nameMenu["teamaname"].NormalTextColor = Color.Gray;
                    nameMenu["teamaname"].SelectedTextColor = Color.Red;

                    nameMenu["teambname"].CanSelectMe = false;
                    nameMenu["teambname"].NormalTextColor = Color.Gray;
                    nameMenu["teambname"].SelectedTextColor = Color.Red;
                }
            }
            else if (id == "generation")
            {
                int val = (int)(editGenerationMenu["generation"] as ValueMenuElement).Value;

                if (val == 0)
                    worldGeneration = WorldEditor.WorldGeneration.Random;
                else if (val == 1)
                    worldGeneration = WorldEditor.WorldGeneration.Flat;
                else if (val == 2)
                    worldGeneration = WorldEditor.WorldGeneration.FlatWithCaves;
                else if (val == 3)
                    worldGeneration = WorldEditor.WorldGeneration.Island;
            }
            else if (id == "trees")
            {
                trees = (editGenerationMenu["trees"] as BooleanValueMenuElement).Value;
            }
            else if (id == "weapons")
            {
                weapons = (editSettingsMenu["weapons"] as BooleanValueMenuElement).Value;
            }
            else if (id == "editing")
            {
                editing = (editSettingsMenu["editing"] as BooleanValueMenuElement).Value;
            }
        }

        private string GameModeToString(GameModes gameMode)
        {
            if (gameMode == GameModes.CustomTDM)
                return "TEAM DEATHMATCH";
            else if (gameMode == GameModes.CustomFFA)
                return "FREE FOR ALL";
            else if (gameMode == GameModes.CustomSNM)
                return "SEARCH AND MINE";
            else if (gameMode == GameModes.CustomSM)
                return "SWARM MODE";
            else if (gameMode == GameModes.CustomKB)
                return "KING OF THE BEACH";
            else
                return "THIS IS A BUG AND SHOULD NEVER HAPPENED";
        }

        private string GenerationToString(WorldEditor.WorldGeneration generation)
        {
            if (generation == WorldEditor.WorldGeneration.Random)
                return "RANDOM";
            else if (generation == WorldEditor.WorldGeneration.Flat)
                return "FLAT";
            else if (generation == WorldEditor.WorldGeneration.FlatWithCaves)
                return "FLAT WITH CAVES";
            else if (generation == WorldEditor.WorldGeneration.Island)
                return "ISLAND";
            else
                return "THIS IS A BUG AND SHOULD NEVER HAPPENED";
        }

        private Menu temp;
        public void Update(short timePassedInMilliseconds)
        {
            temp = drawingNameMenu;
            drawingNameMenu.Update(timePassedInMilliseconds);
            if(temp == drawingNameMenu)
                drawingEditMenu.Update(timePassedInMilliseconds);

            editMenu["mapname"].Text = mapName;
            editMenu["password"].Text = password == null ? "" : password;
            editMenu["teamaname"].Text = teamAName;
            editMenu["teambname"].Text = teamBName;
            editMenu["gameMode"].Text = GameModeToString(gameMode);
            editGenerationMenu["worldsize"].Text = worldSize.ToString() + "x" + worldSize.ToString();
            editGenerationMenu["generation"].Text = GenerationToString(worldGeneration);
            editGenerationMenu["trees"].Text = trees.ToString();
            editSettingsMenu["weapons"].Text = weapons.ToString();
            editSettingsMenu["editing"].Text = editing.ToString();
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(Resources.LobbyBackgroundTexture, Vector2.Zero, Color.White);
            drawingNameMenu.Draw(sb);
            drawingEditMenu.Draw(sb);
        }
    }

    public class EditInfoMapMenu : CreateAMapMenu
    {
        public delegate void EditMap(string name, string password, string teamAName, string teamBName, bool weapons, bool editing);

        private void CreateAMapCatcher(string name, string password, string teamAName, string teamBName, GameModes g, int worldSize, WorldEditor.WorldGeneration worldGeneration, bool trees, bool weapons, bool editint)
        {
            em.Invoke(name, password, teamAName, teamBName, weapons, editint);
        }

        // Im a cheater
        public EditInfoMapMenu(EditMap em)
            : base(null, null)
        {
            this.em = em;
        }

        private EditMap em;
        public EditInfoMapMenu(Menu.BackPressed b, EditMap em, string mapName, string currentPassword, string teamAName, string teamBName, GameModes g, int size, bool weapons, bool editing, WorldEditor.WorldGeneration worldGen, bool trees)
            : base(b, new EditInfoMapMenu(em).CreateAMapCatcher)
        {
            createMap = CreateAMapCatcher;
            this.em = em;

            nameMenu["create"].Text = "APPLY CHANGES";

            foreach(string s in new []{ "gamemode" })
            {
                nameMenu[s].CanSelectMe = false;
                nameMenu[s].NormalTextColor = Color.Gray;
                nameMenu[s].SelectedTextColor = Color.Red;
            }

            foreach (string s in new[] { "worldsize", "generation", "trees" })
            {
                nameGenerationMenu[s].CanSelectMe = false;
                nameGenerationMenu[s].NormalTextColor = Color.Gray;
                nameGenerationMenu[s].SelectedTextColor = Color.Red;
            }


            foreach (string s in new[] { "gameMode" })
            {
                editMenu[s].CanSelectMe = false;
                editMenu[s].NormalTextColor = Color.Gray;
                editMenu[s].SelectedTextColor = Color.Red;
            }

            foreach (string s in new[] { "worldsize", "generation", "trees" })
            {
                editGenerationMenu[s].CanSelectMe = false;
                editGenerationMenu[s].NormalTextColor = Color.Gray;
                editGenerationMenu[s].SelectedTextColor = Color.Red;
            }

            this.mapName = mapName;
            this.password = currentPassword;
            this.teamAName = teamAName;
            this.teamBName = teamBName;
            this.worldSize = size;
            this.gameMode = g;

            this.weapons = weapons;
            this.editing = editing;

            this.worldGeneration = worldGen;
            this.trees = trees;

            if (g == GameModes.CustomFFA || g == GameModes.CustomSM || g == GameModes.CustomKB)
            {
                editMenu["teamaname"].CanSelectMe = false;
                editMenu["teamaname"].NormalTextColor = Color.Gray;
                editMenu["teamaname"].SelectedTextColor = Color.Red;

                editMenu["teambname"].CanSelectMe = false;
                editMenu["teambname"].NormalTextColor = Color.Gray;
                editMenu["teambname"].SelectedTextColor = Color.Red;

                nameMenu["teamaname"].CanSelectMe = false;
                nameMenu["teamaname"].NormalTextColor = Color.Gray;
                nameMenu["teamaname"].SelectedTextColor = Color.Red;

                nameMenu["teambname"].CanSelectMe = false;
                nameMenu["teambname"].NormalTextColor = Color.Gray;
                nameMenu["teambname"].SelectedTextColor = Color.Red;

            }
        }

    }
}
