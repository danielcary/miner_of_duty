using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;
using System.Threading;
using Miner_Of_Duty.LobbyCode;
using Microsoft.Xna.Framework.GamerServices;
using Lzf;

namespace Miner_Of_Duty.Menus
{
    public class MapSearchList : IMenuOwner 
    {
        public void SelectFirst()
        {

        }

        public MapMetaInfo[] maps;
        private int[] indices;
        public enum State { Main, Deleting, Refreshing }
        private State state;
        private Menu.BackPressed back;
        private int delay;
        private int dot, dotDelay;
        private int selectedIndex;
        private int offset;

        public delegate void EditMap(MapMetaInfo map);
        private EditMap mapEdit;

        public MapSearchList(Menu.BackPressed back, EditMap mapEdit)
        {
            state = State.Main;
            this.back = back;
            this.mapEdit = mapEdit;
            selectedIndex = 0;
        }

        public void FindMaps()
        {
            try
            {
                state = State.Refreshing;

                MinerOfDuty.SaveDevice.DeviceDisconnected += SaveDevice_DeviceDisconnected;
                string[] results = MinerOfDuty.SaveDevice.GetFiles("Miner Of Duty Custom Maps");

                if (results != null)
                {
                    maps = new MapMetaInfo[results.Length];
                    for (int i = 0; i < results.Length; i++)
                    {
                        filename = results[i];
                        MinerOfDuty.SaveDevice.Load("Miner Of Duty Custom Maps", results[i], LoadMapInfo);
                        if (convertedStream != null)
                        {
                            MinerOfDuty.SaveDevice.Delete("Miner Of Duty Custom Maps", results[i]);
                            MinerOfDuty.SaveDevice.Save("Miner Of Duty Custom Maps", results[i], new EasyStorage.FileAction(
                                delegate(Stream toSave)
                                {
                                    toSave.Write(new byte[] { 6, 9, 69, 69 }, 0, 4);
                                    toSave.Write(convertedStream.ToArray(), 0, (int)convertedStream.Length);
                                }));
                            convertedStream.Dispose();
                            MinerOfDuty.SaveDevice.Load("Miner Of Duty Custom Maps", results[i], LoadMapInfo);
                        }
                        if (mapinfo != null)
                            maps[i] = mapinfo;
                        else
                        {
                            MinerOfDuty.SaveDevice.Delete("Miner Of Duty Custom Maps", results[i]);
                            FindMaps();
                            return;
                        }
                    }
                }
                else
                    maps = null;

                MessageBox.CloseMessageBox();
                try
                {
                    MinerOfDuty.SaveDevice.DeviceDisconnected -= SaveDevice_DeviceDisconnected;
                }
                catch (Exception) { }

                System.GC.Collect();

                Sort();
                selectedIndex = 0;
                offset = 0;

                Thread.Sleep(125);
                state = State.Main;
            }
            catch (NullReferenceException)
            {
            }
            catch (IOException)
            {
                Show();
            }
            catch (InvalidOperationException)
            {
                Show();
            }
            catch (Exception e)
            {
                state = State.Refreshing;
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

        void SaveDevice_DeviceDisconnected(object sender, EasyStorage.SaveDeviceEventArgs e)
        {
            MinerOfDuty.SaveDevice.DeviceDisconnected -= SaveDevice_DeviceDisconnected;
            Show();
        }

        private void ReshowMessageBox(int selected)
        {
            MessageBox.ShowMessageBox(ReshowMessageBox, new string[] { "OKAY" },
                     0, new string[] { "MAP(S) MUST BE CONVERTED.", "PLEASE WAIT." });
        }

        private string filename;
        private MapMetaInfo mapinfo;

        private MemoryStream convertedStream = null;
        private void LoadMapInfo(Stream s)
        {
            MemoryStream ms = new MemoryStream();
            mapinfo = null;
            convertedStream = null;
            try
            {
                int byteA = s.ReadByte();
                int byteB = s.ReadByte();
                int byteC = s.ReadByte();
                int byteD = s.ReadByte();

                if ((byteA == 6 && byteB == 9 && byteC == 69 && byteD == 69) == false)
                {
                    //we need to convert file 
                    ReshowMessageBox(0);
                    
                    //so load and decode with lzma
                    s.Position = 0;
                    SevenZip.LzmaAlone.Decode(s, ms);

                    ms.Position = 0;
                    MemoryStream converted = new MemoryStream();
                    LZF.lzf.Compress(ms, converted);
                    converted.Position = 0;

                    s.Close();

                    convertedStream = converted;
                    return;

                    //MinerOfDuty.SaveDevice.Save("Miner Of Duty Custom Maps", filename, new EasyStorage.FileAction(
                    //    delegate(Stream toSave)
                    //    {
                    //        toSave.Write(new byte[] { 6, 9, 69, 69 }, 0, 4); 
                    //        toSave.Write(converted.ToArray(), 0, (int)converted.Length);
                    //    }));


                    //MinerOfDuty.SaveDevice.Load("Miner Of Duty Custom Maps", filename, LoadMapInfo);

                }
                else
                {
                    //we can go on and load whatever
                    MemoryStream input = new MemoryStream();
                    
                    byte[] buffer = new byte[s.Length - 4];
                    s.Read(buffer, 0, buffer.Length);//the stream should have already read the first 4 bytes

                    input.Write(buffer, 0, buffer.Length);
                    input.Position = 0;
                    LZF.lzf.Decompress(input, ms);

                    ms.Position = 0;
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        int version = br.ReadInt16();

                        string author = br.ReadString();
                        string mapname = br.ReadString();
                        long time = br.ReadInt64();


                        if (version >= 3)
                        {
                            GameModes g = (GameModes)br.ReadByte();
                            mapinfo = new MapMetaInfo(author, mapname, time, filename, g);

                        }
                        else
                            mapinfo = new MapMetaInfo(author, mapname, time, filename, LobbyCode.GameModes.CustomTDM);
                    }

                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
                System.Diagnostics.Debug.WriteLine("Corrupted File or Something");
            }
        }

        private void Sort()
        {
            indices = new int[maps.Length];
            Dictionary<int, string> mapnames = new Dictionary<int, string>();
            for (int i = 0; i < maps.Length; i++)
            {
                mapnames.Add(i, maps[i].MapName);
            }

            Dictionary<int, string> items = (from entry in mapnames
                        orderby entry.Value ascending
                        select entry).ToDictionary(pair => pair.Key, pair => pair.Value);

            indices = items.Keys.ToArray<int>();
        }

        public void Show()
        {
            Thread t = new Thread(FindMaps);
            t.IsBackground = false;
            t.Start();
            state = State.Refreshing;
        }

        private void SaveDevice_DeleteCompleted(object sender, EasyStorage.FileActionCompletedEventArgs args)
        {
            MinerOfDuty.SaveDevice.DeleteCompleted -= SaveDevice_DeleteCompleted;

            Show();
        }

        private void AreYouSureDelete(int selected)
        {
            if (selected == 0)
            {
                MinerOfDuty.SaveDevice.DeleteCompleted += SaveDevice_DeleteCompleted;
                MinerOfDuty.SaveDevice.DeleteAsync("Miner Of Duty Custom Maps", maps[indices[selectedIndex]].FileName);
                state = State.Deleting;
            }
            Audio.PlaySound(Audio.SOUND_UICLICK);
        }

        public void Update(short timePassedInMilliseconds)
        {
            if (delay > 0)
                delay -= timePassedInMilliseconds;

            if (state == State.Main)
            {
                if (Input.WasButtonPressed(Buttons.A))
                {
                    if(indices != null && maps != null && maps.Length > 0)
                        mapEdit.Invoke(maps[indices[selectedIndex]]);
                }
                else if (Input.WasButtonPressed(Buttons.X))
                {
                    if (indices != null && maps != null && maps.Length > 0)
                    {
                        MessageBox.ShowMessageBox(AreYouSureDelete, new string[] { "YES, DELETE", "NO" }, 1, new string[] { "ARE YOUR SURE YOU", "WANT TO DELETE IT?" });
                    }
                }
                else if (Input.WasButtonPressed(Buttons.B))
                {
                    state = State.Main;
                    back.Invoke(this);
                }
                else if (Input.IsThumbstickOrDPad(Input.Direction.Up) && delay <= 0)
                {
                    if (--selectedIndex < 0)
                        selectedIndex = 0;
                    if (selectedIndex - offset < 0)
                        offset--;
                    delay = 175;
                }
                else if (Input.IsThumbstickOrDPad(Input.Direction.Down) && delay <= 0)
                {
                    if (++selectedIndex >= maps.Length)
                        selectedIndex = maps.Length - 1;
                    if (selectedIndex - offset > 9)
                        offset++;
                    delay = 175;
                }
            }
            else
            {
                dotDelay += timePassedInMilliseconds;
                if (dotDelay > 1000)
                {
                    dot++;
                    dotDelay = 0;
                }
                if (dot == 4)
                    dot = 0;
            }

            if (selectedIndex == -1)
                selectedIndex = 0;
        }

        public void Draw(SpriteBatch sb)
        {
            if (state == State.Main)
            {
                sb.Draw(Resources.LobbyBackgroundTexture, Vector2.Zero, Color.White);
                if (maps != null && indices != null)
                {
                    
                    sb.DrawString(Resources.Font, "MAP NAME", new Vector2(165, 130), Color.White);
                    sb.DrawString(Resources.Font, "AUTHOR", new Vector2(490, 130), Color.White);
                    sb.DrawString(Resources.Font, "LAST EDITED", new Vector2(715 + 80, 130), Color.White);

                    Vector2 startPos = new Vector2(180, 130 + Resources.Font.LineSpacing * 1.3f);

                    for (int i = offset; i < indices.Length && i < offset + 10; i++)
                    {
                        MapMetaInfo map = maps[indices[i]];
                        sb.DrawString(Resources.DescriptionFont, map.MapName, startPos, selectedIndex == i ? Color.Green : Color.White);
                        sb.DrawString(Resources.DescriptionFont, map.Author, startPos + new Vector2(320, 0), selectedIndex == i ? Color.Green : Color.White);
                        sb.DrawString(Resources.DescriptionFont, map.TimeEdited.ToString(), startPos + new Vector2(625, 0), selectedIndex == i ? Color.Green : Color.White);
                        startPos.Y += Resources.NameFont.LineSpacing * .92f;
                    }

                    if (offset > 0)
                        sb.DrawString(Resources.TitleFont, "^", new Vector2(120, 325), Color.White, 0, Resources.Font.MeasureString("^") / 2f, 2, SpriteEffects.None, 0);
                    if (10 + offset < maps.Length)
                        sb.DrawString(Resources.TitleFont, "^", new Vector2(120, 425), Color.White, MathHelper.Pi, Resources.Font.MeasureString("^") / 2f, 2, SpriteEffects.None, 0);

                    sb.DrawString(Resources.DescriptionFont, "(A) EDIT  (X) DELETE  (B) BACK", new Vector2(140, 595), Color.White);

                    if(maps.Length == 0)
                        sb.DrawString(Resources.DescriptionFont, selectedIndex + " / " + indices.Length, new Vector2(1150 - Resources.DescriptionFont.MeasureString(selectedIndex + " / " + indices.Length).X, 595), Color.White);
                    else
                        sb.DrawString(Resources.DescriptionFont, (selectedIndex + 1) + " / " + indices.Length, new Vector2(1150 - Resources.DescriptionFont.MeasureString((selectedIndex + 1) + " / " + indices.Length).X, 595), Color.White);
                }


            }
            else
            {
                sb.Draw(Resources.MainMenuTexture, Vector2.Zero, Color.White);
                string text = "";
                if (state == State.Deleting)
                    text = "DELETING";
                else if (state == State.Refreshing)
                    text = "REFRESHING";

                sb.Draw(Resources.MessageBoxBackTexture, new Vector2(640 - (Resources.MessageBoxBackTexture.Width / 2), 320 - (Resources.MessageBoxBackTexture.Height / 2)), Color.White);
                sb.DrawString(Resources.Font, text + (dot == 1 ? "." : dot == 2 ? ".." : dot == 3 ? "..." : ""), new Vector2(640 - (Resources.Font.MeasureString(text).X / 2f), 320 - (Resources.Font.LineSpacing / 2f)), Color.White);
            }
        }
    }

    public class MapSearchPlayList : IMenuOwner
    {
        public void SelectFirst()
        {

        }
        public MapMetaInfo[] maps;
        private int[] indices;
        public enum State { Main, Refreshing }
        private State state;
        private Menu.BackPressed back;
        private int delay;
        private int dot, dotDelay;
        private int selectedIndex;
        private int offset;

        public bool tag = false;

        public delegate void HostMap(MapMetaInfo mapToHost);
        private HostMap mapEdit;
        public bool isPrivate;

        public MapSearchPlayList(Menu.BackPressed back, HostMap mapEdit)
        {
            state = State.Main;
            this.back = back;
            this.mapEdit = mapEdit;
            selectedIndex = 0;
        }

        public void FindMaps()
        {
            FindMaps(null);
        }

        void SaveDevice_DeviceDisconnected(object sender, EasyStorage.SaveDeviceEventArgs e)
        {
            MinerOfDuty.SaveDevice.DeviceDisconnected -= SaveDevice_DeviceDisconnected;
            Show();
        }

        public void FindMaps(EventHandler callback)
        {
            try
            {
                state = State.Refreshing;

                MinerOfDuty.SaveDevice.DeviceDisconnected += SaveDevice_DeviceDisconnected;
                string[] results = MinerOfDuty.SaveDevice.GetFiles("Miner Of Duty Custom Maps");

                if (results != null)
                {
                    maps = new MapMetaInfo[results.Length];
                    for (int i = 0; i < results.Length; i++)
                    {
                        filename = results[i];
                        MinerOfDuty.SaveDevice.Load("Miner Of Duty Custom Maps", results[i], LoadMapInfo); 
                        if (convertedStream != null)
                        {
                            MinerOfDuty.SaveDevice.Delete("Miner Of Duty Custom Maps", results[i]);
                            MinerOfDuty.SaveDevice.Save("Miner Of Duty Custom Maps", results[i], new EasyStorage.FileAction(
                                delegate(Stream toSave)
                                {
                                    toSave.Write(new byte[] { 6, 9, 69, 69 }, 0, 4);
                                    toSave.Write(convertedStream.ToArray(), 0, (int)convertedStream.Length);
                                }));
                            convertedStream.Dispose();
                            MinerOfDuty.SaveDevice.Load("Miner Of Duty Custom Maps", results[i], LoadMapInfo);
                        }
                        if (mapinfo != null)
                            maps[i] = mapinfo;
                        else
                        {
                            MinerOfDuty.SaveDevice.Delete("Miner Of Duty Custom Maps", results[i]);
                            FindMaps();
                            return;
                        }
                    }
                }
                else
                    maps = null;

                try
                {
                    MinerOfDuty.SaveDevice.DeviceDisconnected -= SaveDevice_DeviceDisconnected;
                }
                catch (Exception) { }

                Sort();
                selectedIndex = 0;
                offset = 0;
                System.GC.Collect();
                Thread.Sleep(125);
                state = State.Main;

                if (callback != null)
                    callback.Invoke(this, null);
            }
            catch (NullReferenceException)
            {
            }
            catch (IOException)
            {
                Show();
            }
            catch (InvalidOperationException)
            {
                Show();
            }
            catch (Exception e)
            {
                state = State.Refreshing;
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

        private void ReshowMessageBox(int selected)
        {
            MessageBox.ShowMessageBox(ReshowMessageBox, new string[] { "OKAY" },
                     0, new string[] { "MAP(S) MUST BE CONVERTED.", "PLEASE WAIT." });
        }

        private string filename;
        private MapMetaInfo mapinfo;
        private MemoryStream convertedStream = null;
        private void LoadMapInfo(Stream s)
        {
            MemoryStream ms = new MemoryStream();
            mapinfo = null;
            convertedStream = null;
            try
            {
                int byteA = s.ReadByte();
                int byteB = s.ReadByte();
                int byteC = s.ReadByte();
                int byteD = s.ReadByte();

                if ((byteA == 6 && byteB == 9 && byteC == 69 && byteD == 69) == false)
                {
                    //we need to convert file 
                    ReshowMessageBox(0);

                    //so load and decode with lzma
                    s.Position = 0;
                    SevenZip.LzmaAlone.Decode(s, ms);

                    ms.Position = 0;
                    MemoryStream converted = new MemoryStream();
                    LZF.lzf.Compress(ms, converted);
                    converted.Position = 0;

                    s.Close();

                    convertedStream = converted;
                    return;

                    //MinerOfDuty.SaveDevice.Save("Miner Of Duty Custom Maps", filename, new EasyStorage.FileAction(
                    //    delegate(Stream toSave)
                    //    {
                    //        toSave.Write(new byte[] { 6, 9, 69, 69 }, 0, 4); 
                    //        toSave.Write(converted.ToArray(), 0, (int)converted.Length);
                    //    }));


                    //MinerOfDuty.SaveDevice.Load("Miner Of Duty Custom Maps", filename, LoadMapInfo);

                }
                else
                {
                    //we can go on and load whatever
                    MemoryStream input = new MemoryStream();

                    byte[] buffer = new byte[s.Length - 4];
                    s.Read(buffer, 0, buffer.Length);//the stream should have already read the first 4 bytes

                    input.Write(buffer, 0, buffer.Length);
                    input.Position = 0;
                    LZF.lzf.Decompress(input, ms);

                    ms.Position = 0;
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        int version = br.ReadInt16();

                        string author = br.ReadString();
                        string mapname = br.ReadString();
                        long time = br.ReadInt64();


                        if (version >= 3)
                        {
                            GameModes g = (GameModes)br.ReadByte();
                            mapinfo = new MapMetaInfo(author, mapname, time, filename, g);

                        }
                        else
                            mapinfo = new MapMetaInfo(author, mapname, time, filename, LobbyCode.GameModes.CustomTDM);
                    }

                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
                System.Diagnostics.Debug.WriteLine("Corrupted File or Something");
            }
        }

        private void Sort()
        {
            indices = new int[maps.Length];
            Dictionary<int, string> mapnames = new Dictionary<int, string>();
            for (int i = 0; i < maps.Length; i++)
            {
                mapnames.Add(i, maps[i].MapName);
            }

            Dictionary<int, string> items = (from entry in mapnames
                                             orderby entry.Value ascending
                                             select entry).ToDictionary(pair => pair.Key, pair => pair.Value);

            indices = items.Keys.ToArray<int>();
        }

        public void Show()
        {
            Thread t = new Thread(FindMaps);
            t.IsBackground = false;
            t.Start();
            state = State.Refreshing;
        }


        int lastInvoked = 0;
        public void Update(short timePassedInMilliseconds)
        {
            if (delay > 0)
                delay -= timePassedInMilliseconds;

            if (lastInvoked > 0)
                lastInvoked -= timePassedInMilliseconds;

            if (state == State.Main)
            {
                if (Input.WasButtonPressed(Buttons.A))
                {
                    if (indices != null && maps != null && maps.Length > 0 && lastInvoked <= 0)
                    {
                        mapEdit.Invoke(maps[indices[selectedIndex]]);
                        lastInvoked = 3000;
                    }
                }
                else if (Input.WasButtonPressed(Buttons.B))
                {
                    state = State.Main;
                    back.Invoke(this);
                }
                else if (Input.IsThumbstickOrDPad(Input.Direction.Up) && delay <= 0)
                {
                    if (--selectedIndex < 0)
                        selectedIndex = 0;
                    if (selectedIndex - offset < 0)
                        offset--;
                    delay = 175;
                }
                else if (Input.IsThumbstickOrDPad(Input.Direction.Down) && delay <= 0)
                {
                    if (++selectedIndex >= maps.Length)
                        selectedIndex = maps.Length - 1;
                    if (selectedIndex - offset > 9)
                        offset++;
                    delay = 175;
                }
            }
            else
            {
                dotDelay += timePassedInMilliseconds;
                if (dotDelay > 1000)
                {
                    dot++;
                    dotDelay = 0;
                }
                if (dot == 4)
                    dot = 0;
            }

            if (selectedIndex == -1)
                selectedIndex = 0;
        }

        public void Draw(SpriteBatch sb)
        {
            if (state == State.Main)
            {
                sb.Draw(Resources.LobbyBackgroundTexture, Vector2.Zero, Color.White);
                if (maps != null && indices != null)
                {

                    sb.DrawString(Resources.Font, "MAP NAME", new Vector2(165, 130), Color.White);
                    sb.DrawString(Resources.Font, "AUTHOR", new Vector2(490, 130), Color.White);
                    sb.DrawString(Resources.Font, "LAST EDITED", new Vector2(715 + 80, 130), Color.White);

                    Vector2 startPos = new Vector2(180, 130 + Resources.Font.LineSpacing * 1.3f);

                    for (int i = offset; i < indices.Length && i < offset + 10; i++)
                    {
                        MapMetaInfo map = maps[indices[i]];
                        sb.DrawString(Resources.DescriptionFont, map.MapName, startPos, selectedIndex == i ? Color.Green : Color.White);
                        sb.DrawString(Resources.DescriptionFont, map.Author, startPos + new Vector2(320, 0), selectedIndex == i ? Color.Green : Color.White);
                        sb.DrawString(Resources.DescriptionFont, map.TimeEdited.ToString(), startPos + new Vector2(625, 0), selectedIndex == i ? Color.Green : Color.White);
                        startPos.Y += Resources.NameFont.LineSpacing * .92f;
                    }

                    if (offset > 0)
                        sb.DrawString(Resources.TitleFont, "^", new Vector2(120, 325), Color.White, 0, Resources.Font.MeasureString("^") / 2f, 2, SpriteEffects.None, 0);
                    if (10 + offset < maps.Length)
                        sb.DrawString(Resources.TitleFont, "^", new Vector2(120, 425), Color.White, MathHelper.Pi, Resources.Font.MeasureString("^") / 2f, 2, SpriteEffects.None, 0);

                    sb.DrawString(Resources.DescriptionFont, "(A) HOST MAP (B) BACK", new Vector2(140, 595), Color.White);

                    if (maps.Length == 0)
                        sb.DrawString(Resources.DescriptionFont, selectedIndex + " / " + indices.Length, new Vector2(1150 - Resources.DescriptionFont.MeasureString(selectedIndex + " / " + indices.Length).X, 595), Color.White);
                    else
                        sb.DrawString(Resources.DescriptionFont, (selectedIndex + 1) + " / " + indices.Length, new Vector2(1150 - Resources.DescriptionFont.MeasureString((selectedIndex + 1) + " / " + indices.Length).X, 595), Color.White);
                }


            }
            else
            {
                sb.Draw(Resources.MainMenuTexture, Vector2.Zero, Color.White);
                sb.Draw(Resources.MessageBoxBackTexture, new Vector2(640 - (Resources.MessageBoxBackTexture.Width / 2), 320 - (Resources.MessageBoxBackTexture.Height / 2)), Color.White);
                sb.DrawString(Resources.Font, "REFRESHING" + (dot == 1 ? "." : dot == 2 ? ".." : dot == 3 ? "..." : ""), new Vector2(640 - (Resources.Font.MeasureString("REFRESHING").X / 2f), 320 - (Resources.Font.LineSpacing / 2f)), Color.White);
            }
        }
    }
}

