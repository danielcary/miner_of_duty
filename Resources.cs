using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Miner_Of_Duty.Game;
using Microsoft.Xna.Framework.Content;
using System.Threading;
using Microsoft.Xna.Framework;
using Miner_Of_Duty.Menus;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Miner_Of_Duty
{
    public static class Resources
    {
        public static SpriteFont Font;
        public static SpriteFont DescriptionFont;
        public static SpriteFont TitleFont;

        public static Texture2D MessageBoxBackTexture;
        public static Texture2D MainMenuTexture;
        public static Texture2D LobbyBackgroundTexture;
        public static Texture2D Help1Texture, Help2Texture, Help3Texture;

        public static Texture2D DannyTexture, CreditsTexture;

        public static Texture2D SelectionMenu9;

        public static Texture2D Muted, IsTalking, CanTalk;
        public static SpriteFont NameFont;

        public static Texture2D WhiteScreen;

        //Classviewer
        public static Texture2D BackTexture;
        public static Texture2D[] GunPics = new Texture2D[13];
        public static Texture2D[] ToolPics = new Texture2D[6];
        public static Dictionary<InventoryItem, Texture2D> ItemPics = new Dictionary<InventoryItem, Texture2D>();

        //inventory
        public static Texture2D SelectionBox, SelectedBox;
        public static Dictionary<InventoryItem, Texture2D> ItemTextures = new Dictionary<InventoryItem, Texture2D>();

        public static Texture2D TeamASpawnBlockTexture, TeamBSpawnBlockTexture;
        public static Texture2D SpawnBlockTexture;
        public static Texture2D ZombieBlockTexture, KingBeachBlockTexture;

        public static Model AreaWallThingModel;

        public static Effect BlockEffect;

        public static BoundingBox HeadBox, ArmBox, BodyBox, LegBox;
        public static Texture2D GrenadeAim;
        public static Texture2D EditCharacterBackgroundTexture;

        public static Texture2D ScoreboardBack;

        public static Texture2D TeamScoreBarTexture;
        public static Texture2D EndTildeTexture;

        public static Texture2D XPBarTexture;
        public static Texture2D XPYellowBarTexture;

        public static Texture2D[] BLOCKTEXTURES = new Texture2D[Block.BLOCKIDS.Length];
        public static Texture2D SELECTIONTEXTURE;
        public static VertexBuffer SelectionBuffer;

        public static Texture2D BlockTextures;

        public static Texture2D FlareTexture;

        public static Model KnifeModel;
        public static Texture2D KnifeTexture;

        public static Model Bucket;
        public static Texture2D LavaBucketTexture, EmptyBucketTexture, WaterBucketTexture;
        public static Model[] GunModels = new Model[13];
        public static Texture2D[] GunModelTextures = new Texture2D[13];

        public static Model Pick, Shovel;
        public static Texture2D PickRock, PickSteel, PickDiamond;
        public static Texture2D ShovelRock, ShovelSteel, ShovelDiamond;

        public static Model[] MuzzleFlare = new Model[13];
        public static Model BulletStreakModel;
        public static Texture2D BulletStreakTexture;

        public static Model FragModel, SmokeFlashModel;
        public static Texture2D FragModelTexture, SmokeModelTexture, FlashModelTexture;

        public static Texture2D HeadShotDeath, NormalDeath, FallDeath, LavaDeath, WaterDeath, KnifeDeath, GrenadeDeath;

        public static Texture2D MiniMapTexture, MiniMapBorderTexture;
        public static Texture2D PlayerDotTexture;

        public static Texture2D StoreTexture;

        public static Texture2D HitMarker;
        public static Texture2D UnderLava, UnderWater;
        public static Texture2D HurtTexture;
        public static Texture2D[] BlockDestroyTextures;
        public static Texture2D HorizontalLineTexture, VerticalLineTexture;


        private static void LoadContent2(ContentManager Content, GraphicsDevice GraphicsDevice)
        {
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(new int[] { 5 });
#endif

            Audio.songs.Add(Audio.SONG_PULSEROCK, Content.Load<Song>("Music/Pulse Rock"));
            Audio.songs.Add(Audio.SONG_RAW, Content.Load<Song>("Music/Raw"));
        }

        public static void LoadContent(ContentManager Content, AsyncCallback callback, GraphicsDevice GraphicsDevice, GraphicsDeviceManager graphics)
        {
            Resources.LobbyBackgroundTexture = Content.Load<Texture2D>("Menu/lobby");
            Resources.Help1Texture = Content.Load<Texture2D>("Menu/help1");
            Resources.Help2Texture = Content.Load<Texture2D>("Menu/help2");
            Resources.Help3Texture = Content.Load<Texture2D>("Menu/help3");


            EditCharacterBackgroundTexture = Content.Load<Texture2D>("Menu/ecback");

            Audio.songs.Add(Audio.SONG_STORMFRONT, Content.Load<Song>("Music/Stormfront"));

            Thread t = new Thread(poop => LoadContent2(Content, GraphicsDevice));
            t.IsBackground = true;
            t.Start();

            Audio.sounds.Add(Audio.SOUND_AMMOPURCHASE, Content.Load<SoundEffect>("Sounds/ui/AmmoPurchase"));
            Audio.sounds.Add(Audio.SOUND_UICLICK, Content.Load<SoundEffect>("Sounds/ui/Click"));
            Audio.sounds.Add(Audio.SOUND_UIERROR, Content.Load<SoundEffect>("Sounds/ui/Error"));
            Audio.sounds.Add(Audio.SOUND_EQUIP, Content.Load<SoundEffect>("Sounds/gun/equip"));
            Audio.sounds.Add(Audio.SOUND_FIRE, Content.Load<SoundEffect>("Sounds/gun/fire"));
            Audio.sounds.Add(Audio.SOUND_RELOAD, Content.Load<SoundEffect>("Sounds/gun/reload"));
            Audio.sounds.Add(Audio.SOUND_FOOTSTEP, Content.Load<SoundEffect>("Sounds/footstep"));
            Audio.sounds.Add(Audio.SOUND_MINEHARD, Content.Load<SoundEffect>("Sounds/minehard"));
            Audio.sounds.Add(Audio.SOUND_MINESOFT, Content.Load<SoundEffect>("Sounds/minesoft"));
            Audio.sounds.Add(Audio.SOUND_PLACEHARD, Content.Load<SoundEffect>("Sounds/placehardblock"));
            Audio.sounds.Add(Audio.SOUND_PLACESOFT, Content.Load<SoundEffect>("Sounds/placesoftblock"));
            Audio.sounds.Add(Audio.SOUND_ZOMBIE, Content.Load<SoundEffect>("Sounds/zombie"));
            Audio.sounds.Add(Audio.SOUND_GRENADE, Content.Load<SoundEffect>("Sounds/grenade"));
            Audio.sounds.Add(Audio.SOUND_SMOKEGRENADE, Content.Load<SoundEffect>("Sounds/smokegrenade"));
            Audio.sounds.Add(Audio.SOUND_SWORD, Content.Load<SoundEffect>("Sounds/sword"));

            Resources.GunPics[GunType.GUNID_12GAUGE] = Content.Load<Texture2D>("ClassViewTextures/12guage");
            Resources.GunPics[GunType.GUNID_AA12] = Content.Load<Texture2D>("ClassViewTextures/AA12");
            Resources.GunPics[GunType.GUNID_AK47] = Content.Load<Texture2D>("ClassViewTextures/ak47");
            Resources.GunPics[GunType.GUNID_COLT45] = Content.Load<Texture2D>("ClassViewTextures/colt");
            Resources.GunPics[GunType.GUNID_DOUBLEBARREL] = Content.Load<Texture2D>("ClassViewTextures/doublebarrel");
            Resources.GunPics[GunType.GUNID_FAL] = Content.Load<Texture2D>("ClassViewTextures/FAL");
            Resources.GunPics[GunType.GUNID_M16] = Content.Load<Texture2D>("ClassViewTextures/m16");
            Resources.GunPics[GunType.GUNID_MAGNUM] = Content.Load<Texture2D>("ClassViewTextures/magnum");
            Resources.GunPics[GunType.GUNID_MP5K] = Content.Load<Texture2D>("ClassViewTextures/MP5K");
            Resources.GunPics[GunType.GUNID_UMP45] = Content.Load<Texture2D>("ClassViewTextures/ump45");
            Resources.GunPics[GunType.GUNID_VECTOR] = Content.Load<Texture2D>("ClassViewTextures/Vector");
            Resources.GunPics[GunType.GUNID_MINIGUN] = Content.Load<Texture2D>("ClassViewTextures/minigun");
            Resources.GunPics[GunType.GUNID_SWORD] = Content.Load<Texture2D>("ClassViewTextures/sword");
            Resources.ToolPics[ToolType.TOOLID_DIAMONDPICK] = Content.Load<Texture2D>("ClassViewTextures/pickdiamond");
            Resources.ToolPics[ToolType.TOOLID_DIAMONDSHOVEL] = Content.Load<Texture2D>("ClassViewTextures/shoveldiamond");
            Resources.ToolPics[ToolType.TOOLID_ROCKPICK] = Content.Load<Texture2D>("ClassViewTextures/pickrock");
            Resources.ToolPics[ToolType.TOOLID_ROCKSHOVEL] = Content.Load<Texture2D>("ClassViewTextures/shovelrock");
            Resources.ToolPics[ToolType.TOOLID_STEELPICK] = Content.Load<Texture2D>("ClassViewTextures/picksteel");
            Resources.ToolPics[ToolType.TOOLID_STEELSHOVEL] = Content.Load<Texture2D>("ClassViewTextures/shovelsteel");
            Resources.ItemPics.Add(InventoryItem.DirtBlock, Content.Load<Texture2D>("ClassViewTextures/dirtblock"));
            Resources.ItemPics.Add(InventoryItem.StoneBlock, Content.Load<Texture2D>("ClassViewTextures/stoneblock"));
            Resources.ItemPics.Add(InventoryItem.GrassBlock, Content.Load<Texture2D>("ClassViewTextures/grassblock"));
            Resources.ItemPics.Add(InventoryItem.Pitfall, Content.Load<Texture2D>("ClassViewTextures/pitfallblock"));
            Resources.ItemPics.Add(InventoryItem.SandBlock, Content.Load<Texture2D>("ClassViewTextures/sandblock"));
            Resources.ItemPics.Add(InventoryItem.GlowBlock, Content.Load<Texture2D>("ClassViewTextures/glowblock"));
            Resources.ItemPics.Add(InventoryItem.EmptyBucket, Content.Load<Texture2D>("ClassViewTextures/emptybucket"));
            Resources.ItemPics.Add(InventoryItem.LavaBucket, Content.Load<Texture2D>("ClassViewTextures/lavabucket"));
            Resources.ItemPics.Add(InventoryItem.SmokeGrenade, Content.Load<Texture2D>("ClassViewTextures/smokegrenade"));
            Resources.ItemPics.Add(InventoryItem.FlashBang, Content.Load<Texture2D>("ClassViewTextures/flashgrenade"));
            Resources.ItemPics.Add(InventoryItem.FragGrenade, Content.Load<Texture2D>("ClassViewTextures/frag"));
            Resources.ItemPics.Add(InventoryItem.Goggles, Content.Load<Texture2D>("ClassViewTextures/goggles"));
            Resources.BackTexture = Content.Load<Texture2D>("ClassViewTextures/rightside");

            Resources.Muted = Content.Load<Texture2D>("Lobby/IsMuted");
            Resources.CanTalk = Content.Load<Texture2D>("Lobby/CanTalk");
            Resources.IsTalking = Content.Load<Texture2D>("Lobby/IsTalking");

            MessageBoxBackTexture = Content.Load<Texture2D>("Menu/msgback");

            BlockEffect = Content.Load<Effect>("BlockEffects/BlockEffect");

            UnderLava = Content.Load<Texture2D>("MiscGame/underLava");
            UnderWater = Content.Load<Texture2D>("MiscGame/underWater");
            HurtTexture = Content.Load<Texture2D>("MiscGame/bloodScreen");

            InfoScreen.ShotFromTexture = Content.Load<Texture2D>("MiscGame/aim");
            PlayerDotTexture = Content.Load<Texture2D>("MiscGame/minimapdot");
            MiniMapTexture = Content.Load<Texture2D>("MiscGame/minimap");
            MiniMapBorderTexture = Content.Load<Texture2D>("MiscGame/mapBorder");

            TeamASpawnBlockTexture = Content.Load<Texture2D>("BlockTextures/teamASpawn");
            TeamBSpawnBlockTexture = Content.Load<Texture2D>("BlockTextures/teamBSpawn");
            SpawnBlockTexture = Content.Load<Texture2D>("BlockTextures/spawn");
            ZombieBlockTexture = Content.Load<Texture2D>("BlockTextures/zombiespawn");
            KingBeachBlockTexture = Content.Load<Texture2D>("BlockTextures/king");

            GunModels[GunType.GUNID_AA12] = Content.Load<Model>("GunModels/aa12");
            GunModelTextures[GunType.GUNID_AA12] = Content.Load<Texture2D>("GunModels/aa12uvmap");
            GunModels[GunType.GUNID_12GAUGE] = Content.Load<Model>("GunModels/12gauge");
            GunModelTextures[GunType.GUNID_12GAUGE] = Content.Load<Texture2D>("GunModels/12gaugeuvmap");
            GunModels[GunType.GUNID_DOUBLEBARREL] = Content.Load<Model>("GunModels/doublebarrel");
            GunModelTextures[GunType.GUNID_DOUBLEBARREL] = Content.Load<Texture2D>("GunModels/doublebarreluvmap");
            GunModels[GunType.GUNID_FAL] = Content.Load<Model>("GunModels/FAL");
            GunModelTextures[GunType.GUNID_FAL] = Content.Load<Texture2D>("GunModels/faluvmap");
            GunModels[GunType.GUNID_M16] = Content.Load<Model>("GunModels/m16");
            GunModelTextures[GunType.GUNID_M16] = Content.Load<Texture2D>("GunModels/m16uvmap");
            GunModels[GunType.GUNID_AK47] = Content.Load<Model>("GunModels/ak47");
            GunModelTextures[GunType.GUNID_AK47] = Content.Load<Texture2D>("GunModels/akuvmap");
            GunModels[GunType.GUNID_MAGNUM] = Content.Load<Model>("GunModels/magnum");
            GunModelTextures[GunType.GUNID_MAGNUM] = Content.Load<Texture2D>("GunModels/magnumuvmap");
            GunModels[GunType.GUNID_COLT45] = Content.Load<Model>("GunModels/colt");
            GunModelTextures[GunType.GUNID_COLT45] = Content.Load<Texture2D>("GunModels/colt45uvmap");
            GunModels[GunType.GUNID_MP5K] = Content.Load<Model>("GunModels/mp5k");
            GunModelTextures[GunType.GUNID_MP5K] = Content.Load<Texture2D>("GunModels/mp5kuvmap");
            GunModels[GunType.GUNID_VECTOR] = Content.Load<Model>("GunModels/vector");
            GunModelTextures[GunType.GUNID_VECTOR] = Content.Load<Texture2D>("GunModels/vectoruvmap");
            GunModels[GunType.GUNID_UMP45] = Content.Load<Model>("GunModels/ump45");
            GunModelTextures[GunType.GUNID_UMP45] = Content.Load<Texture2D>("GunModels/ump45uvmap");

            GunModels[GunType.GUNID_MINIGUN] = Content.Load<Model>("GunModels/minigun");
            GunModelTextures[GunType.GUNID_MINIGUN] = Content.Load<Texture2D>("GunModels/minigunuvmap");


            GunModels[GunType.GUNID_SWORD] = Content.Load<Model>("GunModels/sword");
            GunModelTextures[GunType.GUNID_SWORD] = Content.Load<Texture2D>("GunModels/sworduvmap");

            Bucket = Content.Load<Model>("MiscGame/lavabucket");
            LavaBucketTexture = Content.Load<Texture2D>("MiscGame/lavabucketuvmap");
            WaterBucketTexture = Content.Load<Texture2D>("MiscGame/waterbucketuvmap");
            EmptyBucketTexture = Content.Load<Texture2D>("MiscGame/emptybucketuvmap");

            SetEffectOnModel(GunModels[GunType.GUNID_AA12], BlockEffect);
            SetEffectOnModel(GunModels[GunType.GUNID_12GAUGE], BlockEffect);
            SetEffectOnModel(GunModels[GunType.GUNID_DOUBLEBARREL], BlockEffect);
            SetEffectOnModel(GunModels[GunType.GUNID_FAL], BlockEffect);
            SetEffectOnModel(GunModels[GunType.GUNID_M16], BlockEffect);
            SetEffectOnModel(GunModels[GunType.GUNID_AK47], BlockEffect);
            SetEffectOnModel(GunModels[GunType.GUNID_MAGNUM], BlockEffect);
            SetEffectOnModel(GunModels[GunType.GUNID_COLT45], BlockEffect);
            SetEffectOnModel(GunModels[GunType.GUNID_MP5K], BlockEffect);
            SetEffectOnModel(GunModels[GunType.GUNID_VECTOR], BlockEffect);
            SetEffectOnModel(GunModels[GunType.GUNID_UMP45], BlockEffect);
            SetEffectOnModel(GunModels[GunType.GUNID_MINIGUN], BlockEffect);
            SetEffectOnModel(GunModels[GunType.GUNID_SWORD], BlockEffect);
            SetEffectOnModel(Bucket, BlockEffect);

            AreaWallThingModel = Content.Load<Model>("MiscGame/areawall");
            SetEffectOnModel(AreaWallThingModel, BlockEffect);

            BlockDestroyTextures = new Texture2D[] { Content.Load<Texture2D>("BlockTextures/Destruction/1"), 
                Content.Load<Texture2D>("BlockTextures/Destruction/2"), Content.Load<Texture2D>("BlockTextures/Destruction/3"),
                Content.Load<Texture2D>("BlockTextures/Destruction/4"),Content.Load<Texture2D>("BlockTextures/Destruction/5"),
                Content.Load<Texture2D>("BlockTextures/Destruction/6"),Content.Load<Texture2D>("BlockTextures/Destruction/7"),
                Content.Load<Texture2D>("BlockTextures/Destruction/8"),Content.Load<Texture2D>("BlockTextures/Destruction/9"),
                Content.Load<Texture2D>("BlockTextures/Destruction/10") };

            SELECTIONTEXTURE = Content.Load<Texture2D>("MiscGame/selectionBox");
            BLOCKTEXTURES[Block.BLOCKID_STONE] = Content.Load<Texture2D>("BlockTextures/stone");
            BLOCKTEXTURES[Block.BLOCKID_DIRT] = Content.Load<Texture2D>("BlockTextures/dirt");
            BLOCKTEXTURES[Block.BLOCKID_GRASS] = Content.Load<Texture2D>("BlockTextures/grass");
            BLOCKTEXTURES[Block.BLOCKID_SAND] = Content.Load<Texture2D>("BlockTextures/sand");
            BLOCKTEXTURES[Block.BLOCKID_WOOD] = Content.Load<Texture2D>("BlockTextures/wood");
            BLOCKTEXTURES[Block.BLOCKID_LEAF] = Content.Load<Texture2D>("BlockTextures/leaf");
            BLOCKTEXTURES[Block.BLOCKID_GLASSUSEABLE] = Content.Load<Texture2D>("BlockTextures/glass");
            BLOCKTEXTURES[Block.BLOCKID_GLOWBLOCK] = Content.Load<Texture2D>("BlockTextures/glowblock");
            BLOCKTEXTURES[Block.BLOCKID_GOLD] = Content.Load<Texture2D>("BlockTextures/goldblock");

            BLOCKTEXTURES[Block.BLOCKID_WOODPLANKS] = Content.Load<Texture2D>("BlockTextures/woodplanks");
            BLOCKTEXTURES[Block.BLOCKID_STONEBRICKS] = Content.Load<Texture2D>("BlockTextures/stonebrick");
            BLOCKTEXTURES[Block.BLOCKID_FIREBRICKS] = Content.Load<Texture2D>("BlockTextures/firebrick");

            BLOCKTEXTURES[Block.BLOCKID_COBBLESTONE] = Content.Load<Texture2D>("BlockTextures/cobblestone");
            BLOCKTEXTURES[Block.BLOCKID_MOSSYCOBBLESTONE] = Content.Load<Texture2D>("BlockTextures/mossycobblestone");

            BLOCKTEXTURES[Block.BLOCKID_BLACK] = Content.Load<Texture2D>("BlockTextures/black");
            BLOCKTEXTURES[Block.BLOCKID_RED] = Content.Load<Texture2D>("BlockTextures/red");
            BLOCKTEXTURES[Block.BLOCKID_BLUE] = Content.Load<Texture2D>("BlockTextures/blue");
            BLOCKTEXTURES[Block.BLOCKID_GREY] = Content.Load<Texture2D>("BlockTextures/grey");
            BLOCKTEXTURES[Block.BLOCKID_WHITE] = Content.Load<Texture2D>("BlockTextures/white");
            BLOCKTEXTURES[Block.BLOCKID_YELLOW] = Content.Load<Texture2D>("BlockTextures/yellow");
            BLOCKTEXTURES[Block.BLOCKID_TEAL] = Content.Load<Texture2D>("BlockTextures/teal");
            BLOCKTEXTURES[Block.BLOCKID_ORANGE] = Content.Load<Texture2D>("BlockTextures/orange");
            BLOCKTEXTURES[Block.BLOCKID_GREEN] = Content.Load<Texture2D>("BlockTextures/green");

            BLOCKTEXTURES[Block.BLOCKID_PITFALLBLOCK] = Content.Load<Texture2D>("BlockTextures/grass");

            BlockTextures = Content.Load<Texture2D>("BlockTextures/textures");

            SelectionBox = Content.Load<Texture2D>("MiscGame/SelectionMenu");
            SelectionMenu9 = Content.Load<Texture2D>("MiscGame/SelectionMenu9");
            SelectedBox = Content.Load<Texture2D>("MiscGame/selectedBox");

            LiquidManager.LavaTexture = Content.Load<Texture2D>("BlockTextures/lava");
            LiquidManager.WaterTexture = Content.Load<Texture2D>("BlockTextures/water");

            ItemTextures.Add(InventoryItem.StoneBlock, Content.Load<Texture2D>("selectionItems/stone"));
            ItemTextures.Add(InventoryItem.DirtBlock, Content.Load<Texture2D>("selectionItems/dirt"));
            ItemTextures.Add(InventoryItem.GrassBlock, Content.Load<Texture2D>("selectionItems/grass"));
            ItemTextures.Add(InventoryItem.SandBlock, Content.Load<Texture2D>("selectionItems/sand"));
            ItemTextures.Add(InventoryItem.LeafBlock, Content.Load<Texture2D>("selectionItems/leaf"));
            ItemTextures.Add(InventoryItem.WoodBlock, Content.Load<Texture2D>("selectionItems/wood"));
            ItemTextures.Add(InventoryItem.GlassBlock, Content.Load<Texture2D>("selectionItems/glass"));

            ItemTextures.Add(InventoryItem.Pitfall, Content.Load<Texture2D>("selectionItems/pitfall"));

            ItemTextures.Add(InventoryItem.Spawn1, Content.Load<Texture2D>("selectionItems/spawn"));
            ItemTextures.Add(InventoryItem.Spawn2, Content.Load<Texture2D>("selectionItems/spawn"));
            ItemTextures.Add(InventoryItem.Spawn3, Content.Load<Texture2D>("selectionItems/spawn"));
            ItemTextures.Add(InventoryItem.Spawn4, Content.Load<Texture2D>("selectionItems/spawn"));
            ItemTextures.Add(InventoryItem.Spawn5, Content.Load<Texture2D>("selectionItems/spawn"));
            ItemTextures.Add(InventoryItem.Spawn6, Content.Load<Texture2D>("selectionItems/spawn"));

            ItemTextures.Add(InventoryItem.ZombieSpawn1, Content.Load<Texture2D>("selectionItems/zombiespawn"));
            ItemTextures.Add(InventoryItem.ZombieSpawn2, Content.Load<Texture2D>("selectionItems/zombiespawn"));
            ItemTextures.Add(InventoryItem.ZombieSpawn3, Content.Load<Texture2D>("selectionItems/zombiespawn"));
            ItemTextures.Add(InventoryItem.ZombieSpawn4, Content.Load<Texture2D>("selectionItems/zombiespawn"));
            ItemTextures.Add(InventoryItem.ZombieSpawn5, Content.Load<Texture2D>("selectionItems/zombiespawn"));
            ItemTextures.Add(InventoryItem.ZombieSpawn6, Content.Load<Texture2D>("selectionItems/zombiespawn"));

            ItemTextures.Add(InventoryItem.KingSpawn, Content.Load<Texture2D>("selectionItems/kingofthebeach"));

            ItemTextures.Add(InventoryItem.GoldBlock, Content.Load<Texture2D>("selectionItems/goldblock"));

            ItemTextures.Add(InventoryItem.Stonebricks, Content.Load<Texture2D>("selectionItems/stonebrick"));
            ItemTextures.Add(InventoryItem.WoodPlanks, Content.Load<Texture2D>("selectionItems/woodplanks"));
            ItemTextures.Add(InventoryItem.Firebricks, Content.Load<Texture2D>("selectionItems/firebrick"));

            ItemTextures.Add(InventoryItem.MossyCobblestone, Content.Load<Texture2D>("selectionItems/mossycobblestone"));
            ItemTextures.Add(InventoryItem.Cobblestone, Content.Load<Texture2D>("selectionItems/cobblestone"));

            ItemTextures.Add(InventoryItem.GlowBlock, Content.Load<Texture2D>("selectionItems/glowblock"));

            ItemTextures.Add(InventoryItem.Goggles, Content.Load<Texture2D>("selectionItems/goggles"));

            ItemTextures.Add(InventoryItem.WaterBucket, Content.Load<Texture2D>("selectionItems/waterbucket"));
            ItemTextures.Add(InventoryItem.LavaBucket, Content.Load<Texture2D>("selectionItems/lavabucket"));
            ItemTextures.Add(InventoryItem.EmptyBucket, Content.Load<Texture2D>("selectionItems/emptybucket"));

            ItemTextures.Add(InventoryItem.RedBlock, Content.Load<Texture2D>("selectionItems/red"));
            ItemTextures.Add(InventoryItem.BlueBlock, Content.Load<Texture2D>("selectionItems/blue"));
            ItemTextures.Add(InventoryItem.WhiteBlock, Content.Load<Texture2D>("selectionItems/white"));
            ItemTextures.Add(InventoryItem.GreenBlock, Content.Load<Texture2D>("selectionItems/green"));
            ItemTextures.Add(InventoryItem.BlackBlock, Content.Load<Texture2D>("selectionItems/black"));
            ItemTextures.Add(InventoryItem.GreyBlock, Content.Load<Texture2D>("selectionItems/grey"));
            ItemTextures.Add(InventoryItem.OrangeBlock, Content.Load<Texture2D>("selectionItems/orange"));
            ItemTextures.Add(InventoryItem.TealBlock, Content.Load<Texture2D>("selectionItems/teal"));
            ItemTextures.Add(InventoryItem.YellowBlock, Content.Load<Texture2D>("selectionItems/yellow"));

            ItemTextures.Add(InventoryItem.TeamASpawn1, Content.Load<Texture2D>("selectionItems/teamASpawn"));
            ItemTextures.Add(InventoryItem.TeamASpawn2, Content.Load<Texture2D>("selectionItems/teamASpawn"));
            ItemTextures.Add(InventoryItem.TeamASpawn3, Content.Load<Texture2D>("selectionItems/teamASpawn"));
            ItemTextures.Add(InventoryItem.TeamBSpawn1, Content.Load<Texture2D>("selectionItems/teamBSpawn"));
            ItemTextures.Add(InventoryItem.TeamBSpawn2, Content.Load<Texture2D>("selectionItems/teamBSpawn"));
            ItemTextures.Add(InventoryItem.TeamBSpawn3, Content.Load<Texture2D>("selectionItems/teamBSpawn"));

            ItemTextures.Add(InventoryItem.FlashBang, Content.Load<Texture2D>("selectionItems/flashgrenade"));
            ItemTextures.Add(InventoryItem.FragGrenade, Content.Load<Texture2D>("selectionItems/frag"));
            ItemTextures.Add(InventoryItem.SmokeGrenade, Content.Load<Texture2D>("selectionItems/smokegrenade"));

            ItemTextures.Add(InventoryItem.AA12, Content.Load<Texture2D>("selectionItems/AA12"));
            ItemTextures.Add(InventoryItem.AK47, Content.Load<Texture2D>("selectionItems/ak47"));
            ItemTextures.Add(InventoryItem.Colt45, Content.Load<Texture2D>("selectionItems/colt"));
            ItemTextures.Add(InventoryItem.DoubleBarrel, Content.Load<Texture2D>("selectionItems/doublebarrel"));
            ItemTextures.Add(InventoryItem.FAL, Content.Load<Texture2D>("selectionItems/FAL"));
            ItemTextures.Add(InventoryItem.M16, Content.Load<Texture2D>("selectionItems/m16"));
            ItemTextures.Add(InventoryItem.MP5K, Content.Load<Texture2D>("selectionItems/MP5K"));
            ItemTextures.Add(InventoryItem.SingleBarrel, Content.Load<Texture2D>("selectionItems/12guage"));
            ItemTextures.Add(InventoryItem.UMP45, Content.Load<Texture2D>("selectionItems/ump45"));
            ItemTextures.Add(InventoryItem.Vector, Content.Load<Texture2D>("selectionItems/Vector"));
            ItemTextures.Add(InventoryItem.Magnum, Content.Load<Texture2D>("selectionItems/magnum"));
            ItemTextures.Add(InventoryItem.Minigun, Content.Load<Texture2D>("selectionItems/minigun"));
            ItemTextures.Add(InventoryItem.Sword, Content.Load<Texture2D>("selectionItems/sword"));


            ItemTextures.Add(InventoryItem.AA12Spawn, Content.Load<Texture2D>("selectionItems/AA12"));
            ItemTextures.Add(InventoryItem.AK47Spawn, Content.Load<Texture2D>("selectionItems/ak47"));
            ItemTextures.Add(InventoryItem.Colt45Spawn, Content.Load<Texture2D>("selectionItems/colt"));
            ItemTextures.Add(InventoryItem.DoubleBarrelSpawn, Content.Load<Texture2D>("selectionItems/doublebarrel"));
            ItemTextures.Add(InventoryItem.FALSpawn, Content.Load<Texture2D>("selectionItems/FAL"));
            ItemTextures.Add(InventoryItem.M16Spawn, Content.Load<Texture2D>("selectionItems/m16"));
            ItemTextures.Add(InventoryItem.MP5KSpawn, Content.Load<Texture2D>("selectionItems/MP5K"));
            ItemTextures.Add(InventoryItem.SingleBarrelSpawn, Content.Load<Texture2D>("selectionItems/12guage"));
            ItemTextures.Add(InventoryItem.UMP45Spawn, Content.Load<Texture2D>("selectionItems/ump45"));
            ItemTextures.Add(InventoryItem.VectorSpawn, Content.Load<Texture2D>("selectionItems/Vector"));
            ItemTextures.Add(InventoryItem.MagnumSpawn, Content.Load<Texture2D>("selectionItems/magnum"));

            ItemTextures.Add(InventoryItem.PickDiamond, Content.Load<Texture2D>("selectionItems/pickdiamond"));
            ItemTextures.Add(InventoryItem.PickRock, Content.Load<Texture2D>("selectionItems/pickrock"));
            ItemTextures.Add(InventoryItem.PickSteel, Content.Load<Texture2D>("selectionItems/picksteel"));
            ItemTextures.Add(InventoryItem.ShovelDiamond, Content.Load<Texture2D>("selectionItems/shoveldiamond"));
            ItemTextures.Add(InventoryItem.ShovelRock, Content.Load<Texture2D>("selectionItems/shovelrock"));
            ItemTextures.Add(InventoryItem.ShovelSteel, Content.Load<Texture2D>("selectionItems/shovelsteel"));

            TeamScoreBarTexture = Content.Load<Texture2D>("MiscGame/teamscorebar");
            EndTildeTexture = Content.Load<Texture2D>("MiscGame/endbar");

            Pick = Content.Load<Model>("tools/pick");
            Shovel = Content.Load<Model>("tools/shovel");
            SetEffectOnModel(Pick, BlockEffect);
            SetEffectOnModel(Shovel, BlockEffect);


            PickRock = Content.Load<Texture2D>("tools/pickrock");
            PickSteel = Content.Load<Texture2D>("tools/picksteel");
            PickDiamond = Content.Load<Texture2D>("tools/pickdiamond");

            ShovelRock = Content.Load<Texture2D>("tools/shovelrock");
            ShovelSteel = Content.Load<Texture2D>("tools/shovelsteel");
            ShovelDiamond = Content.Load<Texture2D>("tools/shoveldiamond");

            FallDeath = Content.Load<Texture2D>("MiscGame/falldeath");
            LavaDeath = Content.Load<Texture2D>("MiscGame/firedeath");
            WaterDeath = Content.Load<Texture2D>("MiscGame/raindropofdeath");
            NormalDeath = Content.Load<Texture2D>("MiscGame/normaldeath");
            HeadShotDeath = Content.Load<Texture2D>("MiscGame/headshot");
            KnifeDeath = Content.Load<Texture2D>("MiscGame/knifeDeath");
            GrenadeDeath = Content.Load<Texture2D>("MiscGame/grenadedeath");

            ScoreboardBack = Content.Load<Texture2D>("MiscGame/scoreboardback");
            XPBarTexture = Content.Load<Texture2D>("MiscGame/xpbar");
            XPYellowBarTexture = Content.Load<Texture2D>("MiscGame/xp");
            FlareTexture = Content.Load<Texture2D>("MiscGame/muzzle");
            InGameMenu.SideBack = Content.Load<Texture2D>("MiscGame/sideback");

            MuzzleFlare[GunType.GUNID_12GAUGE] = Content.Load<Model>("MuzzleFlare/12gaugemuzzle");
            MuzzleFlare[GunType.GUNID_AA12] = Content.Load<Model>("MuzzleFlare/aa12muzzle");
            MuzzleFlare[GunType.GUNID_AK47] = Content.Load<Model>("MuzzleFlare/ak47muzzle");
            MuzzleFlare[GunType.GUNID_COLT45] = Content.Load<Model>("MuzzleFlare/coltmuzzle");
            MuzzleFlare[GunType.GUNID_DOUBLEBARREL] = Content.Load<Model>("MuzzleFlare/doublebarrelmuzzle");
            MuzzleFlare[GunType.GUNID_FAL] = Content.Load<Model>("MuzzleFlare/FALmuzzle");
            MuzzleFlare[GunType.GUNID_M16] = Content.Load<Model>("MuzzleFlare/m16muzzle");
            MuzzleFlare[GunType.GUNID_MAGNUM] = Content.Load<Model>("MuzzleFlare/magnummuzzle");
            MuzzleFlare[GunType.GUNID_MP5K] = Content.Load<Model>("MuzzleFlare/mp5kmuzzle");
            MuzzleFlare[GunType.GUNID_UMP45] = Content.Load<Model>("MuzzleFlare/ump45muzzle");
            MuzzleFlare[GunType.GUNID_VECTOR] = Content.Load<Model>("MuzzleFlare/vectormuzzle");
            MuzzleFlare[GunType.GUNID_MINIGUN] = Content.Load<Model>("MuzzleFlare/minigunmuzzle");
            MuzzleFlare[GunType.GUNID_SWORD] = Content.Load<Model>("MuzzleFlare/swordmuzzle");

            for (int i = 0; i < 13; i++)
                SetEffectOnModel(MuzzleFlare[i], BlockEffect);

            FragModel = Content.Load<Model>("Grenades/frag");
            SmokeFlashModel = Content.Load<Model>("Grenades/smoke");
            SetEffectOnModel(FragModel, BlockEffect);
            SetEffectOnModel(SmokeFlashModel, BlockEffect);
            FragModelTexture = Content.Load<Texture2D>("Grenades/fraguvmap");
            FlashModelTexture = Content.Load<Texture2D>("Grenades/flashuvmap");
            SmokeModelTexture = Content.Load<Texture2D>("Grenades/smokeuvmap");

            KnifeTexture = Content.Load<Texture2D>("MiscGame/knifetexture");
            KnifeModel = Content.Load<Model>("MiscGame/knife");
            SetEffectOnModel(KnifeModel, BlockEffect);

            
            HitMarker = Content.Load<Texture2D>("MiscGame/hitmarker");
            HorizontalLineTexture = Content.Load<Texture2D>("MiscGame/crosshairHorizontal");
            VerticalLineTexture = Content.Load<Texture2D>("MiscGame/crosshairVertical");

            BulletStreakModel = Content.Load<Model>("MiscGame/bulletstreak");
            SetEffectOnModel(BulletStreakModel, BlockEffect);
            BulletStreakTexture = Content.Load<Texture2D>("MiscGame/bulletstreakuv");

            Model bodyPart;

            short[] ib = new short[36];
            PositionNormalTextureColor[] data = new PositionNormalTextureColor[24];
            bodyPart = Content.Load<Model>("Body/head");
            bodyPart.Meshes[0].MeshParts[0].IndexBuffer.GetData<short>(ib);
            bodyPart.Meshes[0].MeshParts[0].VertexBuffer.GetData<PositionNormalTextureColor>(data);
            HeadVertexType.HeadIndices = ib;
            HeadVertexType.HeadVertices = data;
            EditCharacter.HeadBaseTexture = Content.Load<Texture2D>("Body/HeadTextures/basehead");
            EditCharacter.HeadEyeTexture = Content.Load<Texture2D>("Body/HeadTextures/eyelayer");
            EditCharacter.HeadHairTexture = Content.Load<Texture2D>("Body/HeadTextures/hairlayer");
            EditCharacter.HeadSkinTexture = Content.Load<Texture2D>("Body/HeadTextures/skinlayer");
            CreateBoundingBox(ref HeadBox, bodyPart);

            ib = new short[36];
            data = new PositionNormalTextureColor[24];
            bodyPart = Content.Load<Model>("Body/body");
            bodyPart.Meshes[0].MeshParts[0].IndexBuffer.GetData<short>(ib);
            bodyPart.Meshes[0].MeshParts[0].VertexBuffer.GetData<PositionNormalTextureColor>(data);
            BodyVertexType.BodyIndices = ib;
            BodyVertexType.BodyVertices = data;
            EditCharacter.BodyPantsTexture = Content.Load<Texture2D>("Body/BodyTextures/pantslayer");
            EditCharacter.BodyShirtTexture = Content.Load<Texture2D>("Body/BodyTextures/shirtlayer");
            EditCharacter.BodySkinTexture = Content.Load<Texture2D>("Body/BodyTextures/skinlayer");
            CreateBoundingBox(ref BodyBox, bodyPart);

            ib = new short[36];
            data = new PositionNormalTextureColor[24];
            bodyPart = Content.Load<Model>("body/arm");
            bodyPart.Meshes[0].MeshParts[0].IndexBuffer.GetData<short>(ib);
            bodyPart.Meshes[0].MeshParts[0].VertexBuffer.GetData<PositionNormalTextureColor>(data);
            ArmVertexType.ArmIndices = ib;
            ArmVertexType.ArmVertices = data;
            EditCharacter.ArmShirtTexture = Content.Load<Texture2D>("Body/ArmTextures/shirtlayer");
            EditCharacter.ArmSkinTexture = Content.Load<Texture2D>("Body/ArmTextures/skinlayer");
            CreateBoundingBox(ref ArmBox, bodyPart);

            ib = new short[36];
            data = new PositionNormalTextureColor[24];
            bodyPart = Content.Load<Model>("Body/leg");
            bodyPart.Meshes[0].MeshParts[0].IndexBuffer.GetData<short>(ib);
            bodyPart.Meshes[0].MeshParts[0].VertexBuffer.GetData<PositionNormalTextureColor>(data);
            LegVertexType.LegIndices = ib;
            LegVertexType.LegVertices = data;
            EditCharacter.LegPantsTexture = Content.Load<Texture2D>("Body/LegTextures/pantslayer");
            EditCharacter.LegSkinTexture = Content.Load<Texture2D>("Body/LegTextures/skinlayer");
            CreateBoundingBox(ref LegBox, bodyPart);

            VertexTextureLight[] verts = new VertexTextureLight[36];
            Block.CreateCube(0, 0, 0, 0, verts);
            SelectionBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexTextureLight), 36, BufferUsage.WriteOnly);
            SelectionBuffer.SetData<VertexTextureLight>(verts);

            EditCharacter.CreateBodyFromColor(GraphicsDevice, out Swarmie.HeadVertexBuffer, out Swarmie.HeadIndexBuffer, out Swarmie.BodyVertexBuffer,
                out Swarmie.BodyIndexBuffer, out Swarmie.ArmVertexBuffer, out Swarmie.ArmIndexBuffer, out Swarmie.LegVertexBuffer, out Swarmie.LegIndexBuffer,
                Color.Red, Color.Black, Color.DarkBlue, Color.DarkGreen, Color.Gray);

            WhiteScreen = new Texture2D(GraphicsDevice, 2, 2, false, SurfaceFormat.Color);
            Color[] colorData = new Color[2 * 2];
            for (int i = 0; i < colorData.Length; i++)
                colorData[i] = Color.White;
            WhiteScreen.SetData<Color>(colorData);


            Game.ParticleSystem.FragParticleSystem ps = new Game.ParticleSystem.FragParticleSystem(Vector3.Zero);
            ps = null;


            GrenadeAim = Content.Load<Texture2D>("MiscGame/grenadeaim");


            


            System.GC.Collect();


            Thread.Sleep(75);


            callback.Invoke(null);
        }

        private static void CreateBoundingBox(ref BoundingBox box, Model model)
        {
            List<Vector3> points = new List<Vector3>();

            //based on http://gamedev.stackexchange.com/questions/2438/how-do-i-create-bounding-boxes-with-xna-4-0
            // Vertex buffer parameters
            int vertexStride = model.Meshes[0].MeshParts[0].VertexBuffer.VertexDeclaration.VertexStride;
            int vertexBufferSize = model.Meshes[0].MeshParts[0].NumVertices * vertexStride;

            // Get vertex data as float
            float[] vertexData = new float[vertexBufferSize / sizeof(float)];
            model.Meshes[0].MeshParts[0].VertexBuffer.GetData<float>(vertexData);

            // Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
            for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
            {
                points.Add(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]));
            }

            box = BoundingBox.CreateFromPoints(points);
        }
        private static void SetEffectOnModel(Model m, Effect e)
        {
            foreach (ModelMesh mesh in m.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    part.Effect = e;
        }
    }

    public struct PositionNormalTextureColor : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;
        public Color Color;

        public VertexDeclaration VertexDeclaration
        {
            get
            {
                return new VertexDeclaration(new VertexElement[]
                    { 
                        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), 
                        new VertexElement(4 * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0), 
                        new VertexElement(4 * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                        new VertexElement(4 * 8, VertexElementFormat.Color, VertexElementUsage.Color, 0) 
                    });
            }
        }
    }
}
