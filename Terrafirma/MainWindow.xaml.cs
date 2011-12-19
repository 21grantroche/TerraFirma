﻿/*
Copyright (c) 2011, Sean Kasun
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Windows.Interop;
using System.Collections;
using System.Threading;

namespace Terrafirma
{
    public class TileInfo
    {
        public string name;
        public UInt32 color;
        public bool hasExtra;
        public double light;
        public double lightR, lightG, lightB;
        public bool transparent,solid;
        public bool isStone, isGrass;
        public Int16 blend;
        public int u, v, minu, maxu, minv, maxv;
        public bool isHilighting;
        public List<TileInfo> variants;
    }
    class TileInfos
    {
        public TileInfos(XmlNodeList nodes)
        {
            info = new TileInfo[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                int id = Convert.ToInt32(nodes[i].Attributes["num"].Value);
                info[id] = new TileInfo();
                loadInfo(info[id], nodes[i]);
            }
        }
        public TileInfo this[int id] //no variantions
        {
            get { return info[id]; }
        }
        public TileInfo this[int id, Int16 u, Int16 v]
        {
            get { return find(info[id], u, v); }
        }
        public ArrayList Items()
        {
            ArrayList items = new ArrayList();
            for (int i = 0; i < info.Length; i++)
                items.Add(info[i]);
            return items;
        }

        private TileInfo find(TileInfo info, Int16 u, Int16 v)
        {
            foreach (TileInfo vars in info.variants)
            {
                // must match *all* restrictions... and we take the first match we find.
                if ((vars.u < 0 || vars.u == u) &&
                    (vars.v < 0 || vars.v == v) &&
                    (vars.minu < 0 || vars.minu <= u) &&
                    (vars.minv < 0 || vars.minv <= v) &&
                    (vars.maxu < 0 || vars.maxu > u) &&
                    (vars.maxv < 0 || vars.maxv > v))
                    return find(vars, u, v); //check for sub-variants
            }
            // if we get here, there are no variants that match
            return info;
        }
        private double parseDouble(string value)
        {
            return Double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        }
        private Int16 parseInt(string value)
        {
            return Int16.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        }
        private UInt32 parseColor(string color)
        {
            UInt32 c = 0;
            for (int j = 0; j < color.Length; j++)
            {
                c <<= 4;
                if (color[j] >= '0' && color[j] <= '9')
                    c |= (byte)(color[j] - '0');
                else if (color[j] >= 'A' && color[j] <= 'F')
                    c |= (byte)(10 + color[j] - 'A');
                else if (color[j] >= 'a' && color[j] <= 'f')
                    c |= (byte)(10 + color[j] - 'a');
            }
            return c;
        }
        private void loadInfo(TileInfo info, XmlNode node)
        {
            info.name = node.Attributes["name"].Value;
            info.color = parseColor(node.Attributes["color"].Value);
            info.hasExtra = node.Attributes["hasExtra"] != null;
            info.light = (node.Attributes["light"] == null) ? 0.0 : parseDouble(node.Attributes["light"].Value);
            info.lightR = (node.Attributes["lightr"] == null) ? 0.0 : parseDouble(node.Attributes["lightr"].Value);
            info.lightG = (node.Attributes["lightg"] == null) ? 0.0 : parseDouble(node.Attributes["lightg"].Value);
            info.lightB = (node.Attributes["lightb"] == null) ? 0.0 : parseDouble(node.Attributes["lightb"].Value);
            info.transparent = node.Attributes["letLight"] != null;
            info.solid = node.Attributes["solid"] != null;
            info.isStone = node.Attributes["isStone"] != null;
            info.isGrass = node.Attributes["isGrass"] != null;
            if (node.Attributes["blend"] != null)
                info.blend = parseInt(node.Attributes["blend"].Value);
            else
                info.blend = -1;
            info.variants = new List<TileInfo>();
            if (node.HasChildNodes)
                for (int i = 0; i < node.ChildNodes.Count; i++)
                    info.variants.Add(newVariant(info, node.ChildNodes[i]));
        }
        private TileInfo newVariant(TileInfo parent, XmlNode node)
        {
            TileInfo info = new TileInfo();
            info.name = (node.Attributes["name"] == null) ? parent.name : node.Attributes["name"].Value;
            info.color = (node.Attributes["color"] == null) ? parent.color : parseColor(node.Attributes["color"].Value);
            info.transparent = (node.Attributes["letLight"] == null) ? parent.transparent : true;
            info.solid = (node.Attributes["solid"] == null) ? parent.solid : true;
            info.light = (node.Attributes["light"] == null) ? parent.light : parseDouble(node.Attributes["light"].Value);
            info.lightR = (node.Attributes["lightr"] == null) ? parent.lightR : parseDouble(node.Attributes["lightr"].Value);
            info.lightG = (node.Attributes["lightg"] == null) ? parent.lightG : parseDouble(node.Attributes["lightg"].Value);
            info.lightB = (node.Attributes["lightb"] == null) ? parent.lightB : parseDouble(node.Attributes["lightb"].Value);
            info.u = (node.Attributes["u"] == null) ? -1 : parseInt(node.Attributes["u"].Value);
            info.v = (node.Attributes["v"] == null) ? -1 : parseInt(node.Attributes["v"].Value);
            info.minu = (node.Attributes["minu"] == null) ? -1 : parseInt(node.Attributes["minu"].Value);
            info.maxu = (node.Attributes["maxu"] == null) ? -1 : parseInt(node.Attributes["maxu"].Value);
            info.minv = (node.Attributes["minv"] == null) ? -1 : parseInt(node.Attributes["minv"].Value);
            info.maxv = (node.Attributes["maxv"] == null) ? -1 : parseInt(node.Attributes["maxv"].Value);
            info.variants = new List<TileInfo>();
            if (node.HasChildNodes)
                for (int i = 0; i < node.ChildNodes.Count; i++)
                    info.variants.Add(newVariant(info, node.ChildNodes[i]));
            return info;
        }

        private TileInfo[] info;
    };
    struct WallInfo
    {
        public string name;
        public UInt32 color;
    }
    struct Tile
    {
        public bool isActive;
        public byte type;
        public byte wall;
        public byte liquid;
        public bool isLava;
        public Int16 u, v, wallu, wallv;
        public double light,lightR,lightG,lightB;
        public bool hasWire;
    }
    struct ChestItem
    {
        public byte stack;
        public string name;
    }
    struct Chest
    {
        public Int32 x, y;
        public ChestItem[] items;
    }
    struct Sign
    {
        public string text;
        public Int32 x, y;
    }
    struct NPC
    {
        public string name;
        public float x, y;
        public bool isHomeless;
        public Int32 homeX, homeY;
        public int sprite;
        public int num;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int MapVersion = 0x25;
        const int MaxTile = 149;
        const int MaxWall = 31;

        const double MaxScale = 16.0;
        const double MinScale = 1.0;

        double curX, curY, curScale;
        byte[] bits;
        WriteableBitmap mapbits;
        DispatcherTimer resizeTimer;
        int curWidth, curHeight, newWidth, newHeight;
        bool loaded = false;
        Tile[] tiles = null;
        Int32 tilesWide=0, tilesHigh=0;
        Int32 spawnX, spawnY;
        Int32 groundLevel,rockLevel;
        string[] worlds;
        string currentWorld;
        List<Chest> chests = new List<Chest>();
        List<Sign> signs = new List<Sign>();
        List<NPC> npcs = new List<NPC>();
        Render render;

        TileInfos tileInfos;
        WallInfo[] wallInfo;
        UInt32 skyColor, earthColor, rockColor, hellColor, lavaColor, waterColor;
        bool isHilight = false;

        public MainWindow()
        {
            InitializeComponent();

            fetchWorlds();

           

            XmlDocument xml=new XmlDocument();
            string xmlData = string.Empty;
            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Terrafirma.tiles.xml"))
            {
                xml.Load(stream);
            }
            tileInfos = new TileInfos(xml.GetElementsByTagName("tile"));
            XmlNodeList wallList = xml.GetElementsByTagName("wall");
            wallInfo = new WallInfo[wallList.Count+1];
            for (int i = 0; i < wallList.Count; i++)
            {
                int id = Convert.ToInt32(wallList[i].Attributes["num"].Value);
                wallInfo[id].name = wallList[i].Attributes["name"].Value;
                wallInfo[id].color = parseColor(wallList[i].Attributes["color"].Value);
            }
            XmlNodeList globalList = xml.GetElementsByTagName("global");
            for (int i = 0; i < globalList.Count; i++)
            {
                string kind = globalList[i].Attributes["id"].Value;
                UInt32 color=parseColor(globalList[i].Attributes["color"].Value);
                switch (kind)
                {
                    case "sky":
                        skyColor = color;
                        break;
                    case "earth":
                        earthColor = color;
                        break;
                    case "rock":
                        rockColor = color;
                        break;
                    case "hell":
                        hellColor = color;
                        break;
                    case "water":
                        waterColor = color;
                        break;
                    case "lava":
                        lavaColor = color;
                        break;
                }
            }

            render = new Render(tileInfos,wallInfo,skyColor,earthColor,rockColor,hellColor,waterColor,lavaColor);
            //this resize timer is used so we don't get killed on the resize
            resizeTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(20), DispatcherPriority.Normal,
                delegate
                {
                    resizeTimer.IsEnabled = false;
                    curWidth = newWidth;
                    curHeight = newHeight;
                    mapbits = new WriteableBitmap(curWidth, curHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                    Map.Source = mapbits;
                    bits = new byte[curWidth * curHeight * 4];
                    Map.Width = curWidth;
                    Map.Height = curHeight;
                    if (loaded)
                        RenderMap();
                    else
                    {
                        var rect=new Int32Rect(0,0,curWidth,curHeight);
                        for (int i=0;i<curWidth*curHeight*4;i++)
                            bits[i]=0xff;
                        mapbits.WritePixels(rect,bits,curWidth*4,0);
                    }
                },
                Dispatcher) {IsEnabled=false};
            curWidth = 496;
            curHeight = 400;
            newWidth = 496;
            newHeight = 400;
            mapbits = new WriteableBitmap(curWidth, curHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            Map.Source = mapbits;
            bits = new byte[curWidth * curHeight * 4];
            curX = curY = 0;
            curScale = 1.0;
        }

        
       

        private void fetchWorlds()
        {
            string path=Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = Path.Combine(path, "My Games");
            path = Path.Combine(path, "Terraria");
            path = Path.Combine(path, "Worlds");
            if (Directory.Exists(path))
                worlds = Directory.GetFiles(path, "*.wld");
            else
            {
                worlds = new string[0];
                Worlds.IsEnabled = false;
            }
            int numItems = 0;
            for (int i = 0; i < worlds.Length && numItems < 9; i++)
            {
                MenuItem item = new MenuItem();

                using (BinaryReader b = new BinaryReader(File.OpenRead(worlds[i])))
                {
                    b.ReadUInt32(); //skip map version
                    item.Header = b.ReadString();
                }
                item.Command = MapCommands.OpenWorld;
                item.CommandParameter = i;
                CommandBindings.Add(new CommandBinding(MapCommands.OpenWorld, OpenWorld));
                item.InputGestureText = String.Format("Ctrl+{0}", (numItems + 1));
                InputBinding inp = new InputBinding(MapCommands.OpenWorld, new KeyGesture(Key.D1 + numItems, ModifierKeys.Control));
                inp.CommandParameter = i;
                InputBindings.Add(inp);
                Worlds.Items.Add(item);
                numItems++;
            }
        }
        private UInt32 parseColor(string color)
        {
            UInt32 c = 0;
            for (int j = 0; j < color.Length; j++)
            {
                c <<= 4;
                if (color[j] >= '0' && color[j] <= '9')
                    c |= (byte)(color[j] - '0');
                else if (color[j] >= 'A' && color[j] <= 'F')
                    c |= (byte)(10 + color[j] - 'A');
                else if (color[j] >= 'a' && color[j] <= 'f')
                    c |= (byte)(10 + color[j] - 'a');
            }
            return c;
        }

        private void Load(string world)
        {
            currentWorld = world;
            bool foundInvalid=false;
            string invalid="";
            using (BinaryReader b = new BinaryReader(File.OpenRead(world)))
            {
                uint version = b.ReadUInt32(); //now we care about the version
                if (version > MapVersion) // new map format
                {
                    MessageBox.Show("Unsupported Map Version");
                    loaded = false;
                    return;
                }
                Title = b.ReadString();
                b.BaseStream.Seek(20, SeekOrigin.Current); //skip id and bounds
                int oldSize = tilesHigh * tilesWide;
                tilesHigh = b.ReadInt32();
                tilesWide = b.ReadInt32();
                spawnX = b.ReadInt32();
                spawnY = b.ReadInt32();
                groundLevel = (int)b.ReadDouble();
                rockLevel = (int)b.ReadDouble();
                int flaglen = 48;
                if (version >= 0x17)
                    flaglen += 5;
                if (version >= 0x1d)
                    flaglen += 3;
                if (version >= 0x20)
                    flaglen++;
                if (version >= 0x22)
                    flaglen++;
                if (version >= 0x25)
                    flaglen++;
                b.BaseStream.Seek(flaglen, SeekOrigin.Current); //skip flags and other settings
                try
                {
                    if (tiles == null)
                        tiles = new Tile[tilesWide * tilesHigh];
                    else if (oldSize < tilesWide * tilesHigh) //only resize if we need to.
                        Array.Resize(ref tiles, tilesWide * tilesHigh);
                }
                catch (OutOfMemoryException e)
                {
                    MessageBox.Show("Oops, ran out of memory!");
                    loaded = false;
                    return;
                }
                for (int i = 0; i < tilesWide * tilesHigh; i++)
                {
                    tiles[i].isActive = b.ReadBoolean();
                    if (tiles[i].isActive)
                    {
                        tiles[i].type = b.ReadByte();
                        if (tiles[i].type > MaxTile) // something screwy in the map
                        {
                            tiles[i].isActive = false;
                            foundInvalid = true;
                            invalid=String.Format("{0} is not a valid tile type",tiles[i].type);
                        }
                        else if (tileInfos[tiles[i].type].hasExtra)
                        {
                            // torches didn't have extra in older versions.
                            if (version < 0x1c && tiles[i].type == 4)
                            {
                                tiles[i].u = -1;
                                tiles[i].v = -1;
                            }
                            else
                            {
                                tiles[i].u = b.ReadInt16();
                                tiles[i].v = b.ReadInt16();
                                if (tiles[i].type == 144) //timer
                                    tiles[i].v = 0;
                            }
                        }
                        else
                        {
                            tiles[i].u = -1;
                            tiles[i].v = -1;
                        }
                    }
                    if (version <= 0x19)
                        b.ReadBoolean(); //skip obsolete hasLight
                    if (b.ReadBoolean())
                    {
                        tiles[i].wall = b.ReadByte();
                        if (tiles[i].wall > MaxWall)  // bad wall
                        {
                            foundInvalid = true;
                            invalid = String.Format("{0} is not a valid wall type", tiles[i].wall);
                            tiles[i].wall = 0;
                        }
                        tiles[i].wallu = -1;
                        tiles[i].wallv = -1;
                    }
                    else
                        tiles[i].wall = 0;
                    if (b.ReadBoolean())
                    {
                        tiles[i].liquid = b.ReadByte();
                        tiles[i].isLava = b.ReadBoolean();
                    }
                    else
                        tiles[i].liquid = 0;
                    if (version >= 0x21)
                        tiles[i].hasWire = b.ReadBoolean();
                    else
                        tiles[i].hasWire = false;
                    if (version >= 0x19) //RLE
                    {
                        int rle = b.ReadInt16();
                        for (int r = 0; r < rle; r++)
                            tiles[i + r + 1] = tiles[i];
                        i += rle;
                    }
                }
                chests.Clear();
                for (int i = 0; i < 1000; i++)
                {
                    if (b.ReadBoolean())
                    {
                        Chest chest = new Chest();
                        chest.items = new ChestItem[20];
                        chest.x = b.ReadInt32();
                        chest.y = b.ReadInt32();
                        for (int ii = 0; ii < 20; ii++)
                        {
                            chest.items[ii].stack = b.ReadByte();
                            if (chest.items[ii].stack > 0)
                            {
                                string name = b.ReadString();
                                string prefix = "";
                                if (version >= 0x24) //item prefixes
                                {
                                    int pfx = b.ReadByte();
                                    if (pfx<prefixes.Length)
                                        prefix = prefixes[pfx];
                                }
                                if (prefix != "")
                                    prefix += " ";
                                chest.items[ii].name = prefix + name;
                            }
                        }
                        chests.Add(chest);
                    }
                }
                signs.Clear();
                for (int i = 0; i < 1000; i++)
                {
                    if (b.ReadBoolean())
                    {
                        Sign sign = new Sign();
                        sign.text = b.ReadString();
                        sign.x = b.ReadInt32();
                        sign.y = b.ReadInt32();
                        signs.Add(sign);
                    }
                }
                npcs.Clear();
                NPCs.Items.Clear();
                while (b.ReadBoolean())
                {
                    NPC npc = new NPC();
                    npc.name = b.ReadString();
                    npc.x = b.ReadSingle();
                    npc.y = b.ReadSingle();
                    npc.isHomeless = b.ReadBoolean();
                    npc.homeX = b.ReadInt32();
                    npc.homeY = b.ReadInt32();

                    npc.sprite = 0;
                    if (npc.name == "Merchant") { npc.sprite = 17; npc.num = 2; }
                    if (npc.name == "Nurse") { npc.sprite = 18; npc.num = 3; }
                    if (npc.name == "Arms Dealer") { npc.sprite = 19; npc.num = 6; }
                    if (npc.name == "Dryad") { npc.sprite = 20; npc.num = 5; }
                    if (npc.name == "Guide") { npc.sprite = 22; npc.num = 1; }
                    if (npc.name == "Old Man") { npc.sprite = 37; npc.num = 0; }
                    if (npc.name == "Demolitionist") { npc.sprite = 38; npc.num = 4; }
                    if (npc.name == "Clothier") { npc.sprite = 54; npc.num = 7; }
                    if (npc.name == "Goblin Tinkerer") { npc.sprite = 107; npc.num = 9; }
                    if (npc.name == "Wizard") { npc.sprite = 108; npc.num = 10; }
                    if (npc.name == "Mechanic") { npc.sprite = 124; npc.num = 8; }
                    if (npc.name == "Santa Claus") { npc.sprite = 142; npc.num = 11; }
                    
                    npcs.Add(npc);

                    if (!npc.isHomeless)
                    {
                        MenuItem item = new MenuItem();
                        item.Header = String.Format("Jump to {0}'s Home", npc.name);
                        item.Click += new RoutedEventHandler(jumpNPC);
                        item.Tag = npc;
                        NPCs.Items.Add(item);
                        NPCs.IsEnabled = true;
                    }
                    else
                    {
                        MenuItem item = new MenuItem();
                        item.Header = String.Format("Jump to {0}'s Location", npc.name);
                        item.Click += new RoutedEventHandler(jumpNPC);
                        item.Tag = npc;
                        NPCs.Items.Add(item);
                        NPCs.IsEnabled = true;
                    }
                }
                // if (version>=0x1f) read the names of the following npcs:
                // merchant, nurse, arms dealer, dryad, guide, clothier, demolitionist,
                // tinkerer and wizard
                // if (version>=0x23) read the name of the mechanic
            }

            calculateLight();
            if (foundInvalid)
                MessageBox.Show("Found problems with the map: "+invalid+"\nIt may not display properly.", "Warning");

            render.SetWorld(tilesWide, tilesHigh, groundLevel, rockLevel,npcs);
            //load info
            loaded = true;
        }

        private string[] prefixes ={
                                       "",
                                       "Large",         //1
                                       "Massive",       //2
                                       "Dangerous",     //3
                                       "Savage",        //4
                                       "Sharp",         //5
                                       "Pointy",        //6
                                       "Tiny",          //7
                                       "Terrible",      //8
                                       "Small",         //9
                                       "Dull",          //10
                                       "Unhappy",       //11
                                       "Bulky",         //12
                                       "Shameful",      //13
                                       "Heavy",         //14
                                       "Light",         //15
                                       "Sighted",       //16
                                       "Rapid",         //17
                                       "Hasty",         //18
                                       "Intimidating",  //19
                                       "Deadly",        //20
                                       "Staunch",       //21
                                       "Awful",         //22
                                       "Lethargic",     //23
                                       "Awkward",       //24
                                       "Powerful",      //25
                                       "Mystic",        //26
                                       "Adept",         //27
                                       "Masterful",     //28
                                       "Inept",         //29
                                       "Ignorant",      //30
                                       "Deranged",      //31
                                       "Intense",       //32
                                       "Taboo",         //33
                                       "",              //34
                                       "Furious",       //35
                                       "Keen",          //36
                                       "Superior",      //37
                                       "Forceful",      //38
                                       "Broken",        //39
                                       "Damaged",       //40
                                       "Shoddy",        //41
                                       "Quick",         //42
                                       "Deadly",        //43
                                       "Agile",         //44
                                       "Nimble",        //45
                                       "Murderous",     //46
                                       "Slow",          //47
                                       "Sluggish",      //48
                                       "Lazy",          //49
                                       "Annoying",      //50
                                       "Nasty",         //51
                                       "Manic",         //52
                                       "Hurtful",       //53
                                       "Strong",        //54
                                       "Unpleasant",    //55
                                       "Weak",          //56
                                       "Ruthless",      //57
                                       "Frenzying",     //58
                                       "Godly",         //59
                                       "Demonic",       //60
                                       "Zealous",       //61
                                       "Hard",          //62
                                       "Guarding",      //63
                                       "Armored",       //64
                                       "Warding",       //65
                                       "Arcane",        //66
                                       "Precise",       //67
                                       "Lucky",         //68
                                       "Jagged",        //69
                                       "Spiked",        //70
                                       "Angry",         //71
                                       "Menacing",      //72
                                       "Brisk",         //73
                                       "Fleeting",      //74
                                       "Hasty",         //75
                                       "Quick",         //76
                                       "Wild",          //77
                                       "Rash",          //78
                                       "Intrepid",      //79
                                       "Violent",       //80
                                       "Legendary",     //81
                                       "Unreal",        //82
                                       "Mythical"       //83
                                  };

        void jumpNPC(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            NPC npc = (NPC)item.Tag;
            if (npc.isHomeless)
            {
                curX = npc.x/16;
                curY = npc.y/16;
            }
            else
            {
                curX = npc.homeX;
                curY = npc.homeY;
            }
            RenderMap();
        }
        


        private void RenderMap()
        {
            var rect = new Int32Rect(0, 0, curWidth, curHeight);

            double startx = curX - (curWidth / (2 * curScale));
            double starty = curY - (curHeight / (2 * curScale));
            try
            {
                render.Draw(curWidth, curHeight, startx, starty, curScale, ref bits,
                    isHilight, Lighting1.IsChecked?1:Lighting2.IsChecked?2:0,
                    UseTextures.IsChecked && curScale > 2.0,ShowHouses.IsChecked,ShowWires.IsChecked, ref tiles);
            }
            catch (System.NotSupportedException e)
            {
                MessageBox.Show(e.ToString(), "Not supported");
            }

            //draw map here with curX,curY,curScale
            mapbits.WritePixels(rect, bits, curWidth * 4, 0);
        }

        private void Map_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
                newHeight = (int)e.NewSize.Height;
            if (e.WidthChanged)
                newWidth = (int)e.NewSize.Width;
            if (e.WidthChanged || e.HeightChanged)
            {
                resizeTimer.IsEnabled = true;
                resizeTimer.Stop();
                resizeTimer.Start();
            }
            e.Handled = true;
        }

        private void Map_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            curScale += (double)e.Delta / 500.0;
            if (curScale < MinScale)
                curScale = MinScale;
            if (curScale > MaxScale)
                curScale = MaxScale;
            if (loaded)
                RenderMap();

        }

        private void Map_MouseMove(object sender, MouseEventArgs e)
        {
            if (Map.IsMouseCaptured)
            {
                Point curPos = e.GetPosition(Map);
                Vector v = start - curPos;
                curX += v.X / curScale;
                curY += v.Y / curScale;
                if (curX < 0) curX = 0;
                if (curY < 0) curY = 0;
                if (curX > tilesWide) curX = tilesWide;
                if (curY > tilesHigh) curY = tilesHigh;
                start = curPos;
                if (loaded)
                    RenderMap();
            }
            else
            {
                Point curPos = e.GetPosition(Map);
                Vector v = start - curPos;
                if (v.X > 50 || v.Y > 50)
                    CloseAllPops();

                int sx, sy;
                getMapXY(curPos, out sx, out sy);
                if (sx >= 0 && sx < tilesWide && sy >= 0 && sy < tilesHigh)
                {
                    int offset = sy + sx * tilesHigh;
                    string label = "Nothing";
                    if (tiles[offset].wall > 0)
                        label = wallInfo[tiles[offset].wall].name;
                    if (tiles[offset].liquid > 0)
                        label = tiles[offset].isLava ? "Lava" : "Water";
                    if (tiles[offset].isActive)
                        label = tileInfos[tiles[offset].type,tiles[offset].u,tiles[offset].v].name;
                    statusText.Text = String.Format("{0},{1} {2}", sx, sy, label);
                }
                else
                    statusText.Text = "";
            }
        }

        Point start;
        private void Map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CloseAllPops();

            Map.Focus();
            Map.CaptureMouse();
            start = e.GetPosition(Map);
        }

        private void Map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Map.ReleaseMouseCapture();
        }
        private SignPopup signPop=null;
        private ChestPopup chestPop=null;

        private void CloseAllPops()
        {
            if (signPop != null)
            {
                signPop.IsOpen = false;
                signPop = null;
            }
            if (chestPop != null)
            {
                chestPop.IsOpen = false;
                chestPop = null;
            }
        }

        private void getMapXY(Point p, out int sx, out int sy)
        {
            double startx = curX - (curWidth / (2 * curScale));
            double starty = curY - (curHeight / (2 * curScale));
            int blocksWide = (int)(curWidth / Math.Floor(curScale)) + 2;
            int blocksHigh = (int)(curHeight / Math.Floor(curScale)) + 2;
            double adjustx = ((curWidth / curScale) - blocksWide) / 2;
            double adjusty = ((curHeight / curScale) - blocksHigh) / 2;

            if (UseTextures.IsChecked && curScale > 2.0)
            {
                sx = (int)(p.X / Math.Floor(curScale) + startx + adjustx);
                sy = (int)(p.Y / Math.Floor(curScale) + starty + adjusty);
            }
            else
            {
                sx = (int)(p.X / curScale + startx);
                sy = (int)(p.Y / curScale + starty);
            }
        }

        private void Map_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseAllPops();
            Point curPos = e.GetPosition(Map);
            start = curPos;
            int sx, sy;
            getMapXY(curPos, out sx, out sy);
            foreach (Chest c in chests)
            {
                //chests are 2x2, and their x/y is upper left corner
                if ((c.x == sx || c.x + 1 == sx) && (c.y == sy || c.y + 1 == sy))
                {
                    ArrayList items = new ArrayList();
                    for (int i = 0; i < c.items.Length; i++)
                    {
                        if (c.items[i].stack > 0)
                            items.Add(String.Format("{0} {1}", c.items[i].stack, c.items[i].name));
                    }
                    chestPop = new ChestPopup(items);
                    chestPop.IsOpen = true;
                }
            }
            foreach (Sign s in signs)
            {
                //signs are 2x2, and their x/y is upper left corner
                if ((s.x == sx || s.x + 1 == sx) && (s.y == sy || s.y + 1 == sy))
                {
                    signPop = new SignPopup(s.text);
                    signPop.IsOpen = true;
                }
            }
        }

        int moving = 0; //moving bitmask
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            bool changed = false;
            switch (e.Key)
            {
                case Key.Up:
                case Key.W:
                    moving |= 1;
                    break;
                case Key.Down:
                case Key.S:
                    moving |= 2;
                    break;
                case Key.Left:
                case Key.A:
                    moving |= 4;
                    break;
                case Key.Right:
                case Key.D:
                    moving |= 8;
                    break;
                case Key.PageUp:
                case Key.E:
                    curScale += 1.0;
                    if (curScale > MaxScale)
                        curScale = MaxScale;
                    changed = true;
                    break;
                case Key.PageDown:
                case Key.Q:
                    curScale -= 1.0;
                    if (curScale < MinScale)
                        curScale = MinScale;
                    changed = true;
                    break;
            }
            if (moving != 0)
            {
                double speed = 10.0;
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    speed *= 2;
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    speed *= 10.0;
                if ((moving & 1) != 0) //up
                    curY -= speed / curScale;
                if ((moving & 2) != 0) //down
                    curY += speed / curScale;
                if ((moving & 4) != 0) //left
                    curX -= speed / curScale;
                if ((moving & 8) != 0) //right
                    curX += speed / curScale;

                if (curX < 0) curX = 0;
                if (curY < 0) curY = 0;
                if (curX > tilesWide) curX = tilesWide;
                if (curY > tilesHigh) curY = tilesHigh;
                changed = true;
            }
            if (changed)
            {
                e.Handled = true;
                if (loaded)
                    RenderMap();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.W:
                    moving &= ~1;
                    break;
                case Key.Down:
                case Key.S:
                    moving &= ~2;
                    break;
                case Key.Left:
                case Key.A:
                    moving &= ~4;
                    break;
                case Key.Right:
                case Key.D:
                    moving &= ~8;
                    break;
            }
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Terraria Worlds|*.wld";
            var result = dlg.ShowDialog();
            if (result == true)
            {
                Loading load = new Loading();
                load.Show();
                Load(dlg.FileName);
                if (!loaded)
                {
                    load.Close();
                    return;
                }
                curX = spawnX;
                curY = spawnY;
                if (render.Textures.Valid)
                {
                    UseTextures.IsChecked = true;
                    curScale = 16.0;
                }
                RenderMap();
                load.Close();
            }
        }
        private void OpenWorld(object sender, ExecutedRoutedEventArgs e)
        {
            int id = (int)e.Parameter;
            Loading load = new Loading();
            load.Show();
            Load(worlds[id]);
            if (!loaded)
            {
                load.Close();
                return;
            }
            curX = spawnX;
            curY = spawnY;
            if (render.Textures.Valid)
            {
                UseTextures.IsChecked = true;
                curScale = 16.0;
            }
            RenderMap();
            load.Close();
        }
        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void OpenWorld_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void JumpToSpawn_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            curX = spawnX;
            curY = spawnY;
            RenderMap();
        }
        private void Lighting_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == MapCommands.NoLight)
            {
                Lighting1.IsChecked = false;
                Lighting2.IsChecked = false;
            }
            else if (e.Command == MapCommands.Lighting)
            {
                Lighting0.IsChecked = false;
                Lighting2.IsChecked = false;
            }
            else
            {
                Lighting0.IsChecked = false;
                Lighting1.IsChecked = false;
            }
            RenderMap();
        }
        private void Texture_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!render.Textures.Valid)
                return;
            if (UseTextures.IsChecked)
                UseTextures.IsChecked = false;
            else
                UseTextures.IsChecked = true;
            RenderMap();
        }
        private void TexturesUsed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = UseTextures.IsChecked;
        }
        private void Redraw(object sender, ExecutedRoutedEventArgs e)
        {
            RenderMap();
        }

        private void Refresh_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Loading load = new Loading();
            load.Show();
            Load(currentWorld);
            if (loaded)
                RenderMap();
            load.Close();
        }

        private void Hilight_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ArrayList items = tileInfos.Items();
            HilightWin h = new HilightWin(items);
            if (h.ShowDialog() == true)
            {
                h.SelectedItem.isHilighting = true;
                // also hilight the subvariants
                hiliteVariants(h.SelectedItem);
                isHilight = true;
                RenderMap();
            }
        }
        private void hiliteVariants(TileInfo info)
        {
            foreach (TileInfo v in info.variants)
            {
                v.isHilighting = true;
                hiliteVariants(v);
            }
        }

        private void HilightStop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            isHilight = false;
            RenderMap();
        }
        private void IsHilighting(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isHilight;
        }
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "Png Image|*.png";
            dlg.Title = "Save Map Image";
            if (dlg.ShowDialog() == true)
            {
                var saveOpts = new SaveOptions();
                saveOpts.CanUseTexture = render.Textures.Valid;
                if (saveOpts.ShowDialog() == true)
                {

                    Saving save = new Saving();
                    save.Show();
                    byte[] pixels;
                    int wd, ht;
                    double sc, startx, starty;

                    if (saveOpts.EntireMap)
                    {
                        wd = tilesWide;
                        ht = tilesHigh;
                        sc = 1.0;
                        startx = 0.0;
                        starty = 0.0;
                    }
                    else
                    {
                        if (saveOpts.UseZoom)
                            sc = curScale;
                        else if (saveOpts.UseTextures)
                            sc = 16.0;
                        else
                            sc = 1.0;

                        wd = (int)((curWidth / curScale) * sc);
                        ht = (int)((curHeight / curScale) * sc);
                        startx = curX - (wd / (2 * sc));
                        starty = curY - (ht / (2 * sc));
                    }
                    pixels = new byte[wd * ht * 4];

                    render.Draw(wd, ht, startx, starty, sc,
                        ref pixels, false, Lighting1.IsChecked?1:Lighting2.IsChecked?2:0,
                        saveOpts.UseTextures && curScale > 2.0,ShowHouses.IsChecked,ShowWires.IsChecked, ref tiles);

                    BitmapSource source = BitmapSource.Create(wd, ht, 96.0, 96.0,
                        PixelFormats.Bgr32, null, pixels, wd * 4);
                    FileStream stream = new FileStream(dlg.FileName, FileMode.Create);
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(source));
                    encoder.Save(stream);
                    stream.Close();
                    save.Close();
                }
            }

        }
        private void MapLoaded(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = loaded;
        }

        private void initWindow(object sender, EventArgs e)
        {
            checkVersion();

            HwndSource hwnd = HwndSource.FromVisual(Map) as HwndSource;

            render.Textures = new Textures(hwnd.Handle);

            if (!render.Textures.Valid) //couldn't find textures?
                UseTextures.IsEnabled = false;
        }

        private void calculateLight()
        {
            // turn off all light
            for (int i = 0; i < tilesWide * tilesHigh; i++)
            {
                tiles[i].light = 0.0;
                tiles[i].lightR = 0.0;
                tiles[i].lightG = 0.0;
                tiles[i].lightB = 0.0;
            }
            // light up light sources
            for (int y = 0; y < tilesHigh; y++)
            {
                for (int x = 0; x < tilesWide; x++)
                {
                    int offset = x * tilesHigh + y;

                    TileInfo inf = tileInfos[tiles[offset].type, tiles[offset].u, tiles[offset].v];
                    if ((!tiles[offset].isActive || inf.transparent) &&
                        (tiles[offset].wall == 0 || tiles[offset].wall==21) && tiles[offset].liquid < 255 && y < groundLevel) //sunlight
                    {
                        tiles[offset].light = 1.0;
                        tiles[offset].lightR = 1.0;
                        tiles[offset].lightG = 1.0;
                        tiles[offset].lightB = 1.0;
                    }
                    if (tiles[offset].liquid > 0 && tiles[offset].isLava) //lava
                    {
                        tiles[offset].light = Math.Max(tiles[offset].light, (tiles[offset].liquid / 255)*0.38+0.1275);
                        // colored lava light's brightness is not affected by its level
                        tiles[offset].lightR = Math.Max(tiles[offset].lightR, 0.66);
                        tiles[offset].lightG = Math.Max(tiles[offset].lightG, 0.39);
                        tiles[offset].lightB = Math.Max(tiles[offset].lightB, 0.13);
                    }
                    tiles[offset].light = Math.Max(tiles[offset].light, inf.light);
                    tiles[offset].lightR = Math.Max(tiles[offset].lightR, inf.lightR);
                    tiles[offset].lightG = Math.Max(tiles[offset].lightG, inf.lightG);
                    tiles[offset].lightB = Math.Max(tiles[offset].lightB, inf.lightB);
                }
            }
            // spread light
            for (int y = 0; y < tilesHigh; y++)
            {
                for (int x = 0; x < tilesWide; x++)
                {
                    int offset = x * tilesHigh + y;
                    double delta = 0.04;
                    TileInfo inf = tileInfos[tiles[offset].type, tiles[offset].u, tiles[offset].v];
                    if (tiles[offset].isActive && !inf.transparent) delta = 0.16;
                    if (y > 0)
                    {
                        if (tiles[offset - 1].light - delta > tiles[offset].light)
                            tiles[offset].light = tiles[offset - 1].light - delta;
                        if (tiles[offset - 1].lightR - delta > tiles[offset].lightR)
                            tiles[offset].lightR = tiles[offset - 1].lightR - delta;
                        if (tiles[offset - 1].lightG - delta > tiles[offset].lightG)
                            tiles[offset].lightG = tiles[offset - 1].lightG - delta;
                        if (tiles[offset - 1].lightB - delta > tiles[offset].lightB)
                            tiles[offset].lightB = tiles[offset - 1].lightB - delta;
                    }
                    if (x > 0)
                    {
                        if (tiles[offset - tilesHigh].light - delta > tiles[offset].light)
                            tiles[offset].light = tiles[offset - tilesHigh].light - delta;
                        if (tiles[offset - tilesHigh].lightR - delta > tiles[offset].lightR)
                            tiles[offset].lightR = tiles[offset - tilesHigh].lightR - delta;
                        if (tiles[offset - tilesHigh].lightG - delta > tiles[offset].lightG)
                            tiles[offset].lightG = tiles[offset - tilesHigh].lightG - delta;
                        if (tiles[offset - tilesHigh].lightB - delta > tiles[offset].lightB)
                            tiles[offset].lightB = tiles[offset - tilesHigh].lightB - delta;
                    }
                }
            }
            // spread light backwards
            for (int y = tilesHigh - 1; y >= 0; y--)
            {
                for (int x = tilesWide - 1; x >= 0; x--)
                {
                    int offset = x * tilesHigh + y;
                    double delta = 0.04;
                    TileInfo inf = tileInfos[tiles[offset].type, tiles[offset].u, tiles[offset].v];
                    if (tiles[offset].isActive && !inf.transparent) delta = 0.16;
                    if (y < tilesHigh - 1)
                    {
                        if (tiles[offset + 1].light - delta > tiles[offset].light)
                            tiles[offset].light = tiles[offset + 1].light - delta;
                        if (tiles[offset + 1].lightR - delta > tiles[offset].lightR)
                            tiles[offset].lightR = tiles[offset + 1].lightR - delta;
                        if (tiles[offset + 1].lightG - delta > tiles[offset].lightG)
                            tiles[offset].lightG = tiles[offset + 1].lightG - delta;
                        if (tiles[offset + 1].lightB - delta > tiles[offset].lightB)
                            tiles[offset].lightB = tiles[offset + 1].lightB - delta;
                    }
                    if (x < tilesWide - 1)
                    {
                        if (tiles[offset + tilesHigh].light - delta > tiles[offset].light)
                            tiles[offset].light = tiles[offset + tilesHigh].light - delta;
                        if (tiles[offset + tilesHigh].lightR - delta > tiles[offset].lightR)
                            tiles[offset].lightR = tiles[offset + tilesHigh].lightR - delta;
                        if (tiles[offset + tilesHigh].lightG - delta > tiles[offset].lightG)
                            tiles[offset].lightG = tiles[offset + tilesHigh].lightG - delta;
                        if (tiles[offset + tilesHigh].lightB - delta > tiles[offset].lightB)
                            tiles[offset].lightB = tiles[offset + tilesHigh].lightB - delta;
                    }
                }
            }
        }

        private void checkVersion()
        {
            Version newVersion = null;
            string url = "";
            XmlTextReader reader = null;
            ThreadStart start = delegate()
            {
                try
                {
                    reader = new XmlTextReader("http://seancode.com/terrafirma/version.xml");
                    reader.MoveToContent();
                    string elementName = "";
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "terrafirma")
                    {
                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                                elementName = reader.Name;
                            else
                            {
                                if (reader.NodeType == XmlNodeType.Text && reader.HasValue)
                                {
                                    switch (elementName)
                                    {
                                        case "version":
                                            newVersion = new Version(reader.Value);
                                            break;
                                        case "url":
                                            url = reader.Value;
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
                Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                if (newVersion != null && curVersion.CompareTo(newVersion) < 0)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                        {
                            if (MessageBox.Show(this, "Download the new version?", "New version detected",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                System.Diagnostics.Process.Start(url);
                            }
                        }));
                }
            };
            new Thread(start).Start();
        }
    }
}
