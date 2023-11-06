using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Miner_Of_Duty.Game
{
    public class WeaponDropManager
    {
        public struct WeaponID
        {
            public string Name;
            public short Number;

            public WeaponID(string name, short number)
            {
                Name = name;
                Number = number;
            }

            public static bool operator ==(WeaponID a, WeaponID b)
            {
                if (a.Name == b.Name)
                    if (a.Number == b.Number)
                        return true;
                return false;
            }

            public static bool operator !=(WeaponID a, WeaponID b)
            {
                if (a.Name == b.Name)
                    if (a.Number == b.Number)
                        return false;
                return true;
            }
        }

        public abstract class WeaponPickupable
        {
            public Vector3 Position;
            public WeaponID ID;
            public byte WeaponID;
            public abstract bool IsAlive { get; }
            public abstract void Update(short miliTime);
            public abstract void Render(Camera camera);
            public abstract Gun CreateWeapon(Gun.BurstFireDelgate delegatee);
            public WeaponPickupable(short id, string name)
            {
                ID = new WeaponID(name, id);
            }
        }

        public class WeaponDrop : WeaponPickupable
        {
            public int AmmoLeft;
            public bool ExtendedMags;
            public override bool IsAlive { get { return showTime > 0; } }
            private int showTime; //in milliseconds

            public WeaponDrop(Vector3 pos, byte weaponId, int ammoLeft, bool extendedMags, WeaponID id)
                : base(id.Number, id.Name)
            {
                Position = pos;
                WeaponID = weaponId;
                AmmoLeft = ammoLeft;
                ExtendedMags = extendedMags;
                showTime = 30000;
            }

            public int TakeAmmo(int ammoCurrentlyInTheGun)
            {
                if (AmmoLeft + ammoCurrentlyInTheGun > GunType.GunTypes[WeaponID].MaxAmmo)
                {
                    AmmoLeft = AmmoLeft - (GunType.GunTypes[WeaponID].MaxAmmo - ammoCurrentlyInTheGun);
                    return GunType.GunTypes[WeaponID].MaxAmmo - ammoCurrentlyInTheGun;
                }
                else
                {
                    int ammoLeft = AmmoLeft;
                    AmmoLeft = 0;
                    return ammoLeft;
                }
            }

            public override Gun CreateWeapon(Gun.BurstFireDelgate delegatee)
            {
                return new Gun(WeaponID, delegatee, ExtendedMags, AmmoLeft);//or whatever
            }

            public void SwitchOut(byte weaponId, int ammoLeft, bool extendedMags, WeaponID id)
            {
                WeaponID = weaponId;
                AmmoLeft = ammoLeft;
                ExtendedMags = extendedMags;
                ID = id;
                showTime = 30000;
            }

            public void SwitchOut(ref Vector3 pos, byte weaponId, int ammoLeft, bool extendedMags, WeaponID id)
            {
                Position = pos;
                WeaponID = weaponId;
                AmmoLeft = ammoLeft;
                ExtendedMags = extendedMags;
                ID = id;
                showTime = 30000;
            }

            public override void Update(short timeInMilliseconds)
            {
                if (showTime > 0)
                    showTime -= timeInMilliseconds;
            }

            public override void Render(Camera camera)
            {
                if (showTime > 0)
                {
                    foreach (ModelMesh mesh in Resources.GunModels[WeaponID].Meshes)
                    {
                        foreach (Effect effect in mesh.Effects)
                        {
                            effect.CurrentTechnique = effect.Techniques["ModelLightFog"];
                            effect.Parameters["World"].SetValue(Matrix.CreateScale(.65f) * Matrix.CreateTranslation(Position));
                            effect.Parameters["CameraPosition"].SetValue(camera.Position);
                            effect.Parameters["Texture0"].SetValue(Resources.GunModelTextures[WeaponID]);
                            effect.Parameters["View"].SetValue(camera.ViewMatrix);
                            effect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                        }
                        mesh.Draw();
                    }
                }
            }

        }

        public class Spawner : WeaponPickupable
        {
            public override bool IsAlive { get { return showTime <= 0; } }
            private int showTime; //in milliseconds

            public Spawner(Vector3 spawner, byte weaponID, short ID)
                :base(ID, "nullzdz")
            {
                Position = spawner;
                WeaponID = weaponID;
            }

            public override Gun CreateWeapon(Gun.BurstFireDelgate delegatee)
            {
                showTime = 5000;
                return new Gun(WeaponID, delegatee, false, false, false);//or whatever
            }

            /// <summary>
            /// Use for multiplayer
            /// </summary>
            public void WeaponTaken()
            {
                showTime = 5000;
            }

            private static int seed = 0;
            private float rot = (float)(new Random(seed++).NextDouble()) * MathHelper.Pi;
            public override void Update(short timeInMilliseconds)
            {
                if (showTime > 0)
                    showTime -= timeInMilliseconds;

                rot += MathHelper.Pi * (timeInMilliseconds / 1000f);
            }

            public override void Render(Camera camera)
            {
                if (IsAlive)
                {
                    foreach (ModelMesh mesh in Resources.GunModels[WeaponID].Meshes)
                    {
                        foreach (Effect effect in mesh.Effects)
                        {
                            effect.CurrentTechnique = effect.Techniques["ModelLightFog"];
                            effect.Parameters["World"].SetValue(Matrix.CreateScale(.65f) * Matrix.CreateRotationY(rot) * Matrix.CreateTranslation(Position));
                            effect.Parameters["CameraPosition"].SetValue(camera.Position);
                            effect.Parameters["Texture0"].SetValue(Resources.GunModelTextures[WeaponID]);
                            effect.Parameters["View"].SetValue(camera.ViewMatrix);
                            effect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                        }
                        mesh.Draw();
                    }
                }
            }

        }

        protected List<WeaponPickupable> weaponDrops;
        private MultiplayerGame game;

        public WeaponDropManager(MultiplayerGame game)
        {
            weaponDrops = new List<WeaponPickupable>(10);
            this.game = game;
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < weaponDrops.Count; i++)
            {
                weaponDrops[i].Update((short)gameTime.ElapsedGameTime.Milliseconds);
            }
        }

        public void Render(Camera camera)
        {
            for (int i = 0; i < weaponDrops.Count; i++)
            {
                weaponDrops[i].Render(camera);
            }
        }

        ///Must use weaponDrop in the current frame
        public WeaponPickupable CheckForPickup(ref Vector3 position)
        {
            if (weaponDrops.Count == 0)
            {
                return null;
            }

            int bestIndex = -1;
            float bestDistance = 69696969;
            float tmpDis;
            for (int i = 0; i < weaponDrops.Count; i++)
            {
                if (weaponDrops[i].IsAlive && weaponDrops[i].ID.Name != game.Me.Gamertag)
                {
                    Vector3.Distance(ref weaponDrops[i].Position, ref position, out tmpDis);
                    if (tmpDis < 2)
                    {
                        if (tmpDis < bestDistance)
                        {
                            bestDistance = tmpDis;
                            bestIndex = i;
                        }
                    }
                }
            }
            if (bestIndex != -1)
                return weaponDrops[bestIndex];
            else
            {
                return null;
            }
        }

        public void TakeSpawner(short id)
        {
            for (int i = 0; i < weaponDrops.Count; i++)
            {
                if (weaponDrops[i] is Spawner && weaponDrops[i].ID.Number == id)
                    (weaponDrops[i] as Spawner).WeaponTaken();

            }
        }

        public void AddWeaponDrop(ref Vector3 pos, byte weaponId, int ammoLeft, bool extendedMags, WeaponID id)
        {
            for (int i = 0; i < weaponDrops.Count; i++)
            {
                if (weaponDrops[i] is WeaponDrop && weaponDrops[i].IsAlive == false)
                {
                    (weaponDrops[i] as WeaponDrop).SwitchOut(ref pos, weaponId, ammoLeft, extendedMags, id);
                    return;
                }
            }
            weaponDrops.Add(new WeaponDrop(pos, weaponId, ammoLeft, extendedMags, id));
        }

        public void AmmoChange(WeaponID id, int newAmmoLeft)
        {
            for (int i = 0; i < weaponDrops.Count; i++)
            {
                if (weaponDrops[i] is WeaponDrop && (weaponDrops[i] as WeaponDrop).ID == id)
                    (weaponDrops[i] as WeaponDrop).AmmoLeft = newAmmoLeft;
            }
        }

        public void SwitchOut(WeaponID id, byte weaponId, int ammoLeft, bool extendedMags)
        {
            for (int i = 0; i < weaponDrops.Count; i++)
            {
                if (weaponDrops[i] is WeaponDrop && (weaponDrops[i] as WeaponDrop).ID == id)
                    (weaponDrops[i] as WeaponDrop).SwitchOut(weaponId, ammoLeft, extendedMags, id);
            }
        }

        private short lastNumberUsed = 0;
        public WeaponID GenerateID()
        {
            return new WeaponID(game.Me.Gamertag, ++lastNumberUsed);
        }

        public void SaveItems(BinaryWriter bw)
        {
            List<Spawner> spawns = new List<Spawner>();
            for (int i = 0; i < weaponDrops.Count; i++)
            {
                if (weaponDrops[i] is Spawner)
                    spawns.Add(weaponDrops[i] as Spawner);
            }

            bw.Write(spawns.Count);
            for (int i = 0; i < spawns.Count; i++)
            {
                bw.Write(spawns[i].Position.X);
                bw.Write(spawns[i].Position.Y);
                bw.Write(spawns[i].Position.Z);
                bw.Write(spawns[i].WeaponID);
            }

            spawns = null;
        }

        public void LoadSpawners(BinaryReader br)
        {
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                weaponDrops.Add(new Spawner(
                    new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                    br.ReadByte(),
                    (short)i));
            }
        }
    }
}