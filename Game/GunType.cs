using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;

namespace Miner_Of_Duty.Game
{
    public class GunType
    {
        //type info
        public int Range { get; private set; }//in blocks
        public int EndRange { get; private set; }

        public int StartingAmmo { get; private set; }
        public int MaxAmmo { get; private set; }
        public int ClipSize { get; private set; }

        public int ExtendedClipSize { get; private set; }
        public int MoreStartingAmmo { get; private set; }

        /// <summary>
        /// The cool down before another bullet can be fired, in milliseconds
        /// </summary>
        public int CoolDown { get; private set; }//time in miliseconds
        public int ReloadLoaded { get; private set; }
        public int ReloadUnloaded { get; private set; }

        public float Damage { get; private set; }// 0 - 100 maybe 100 = 1 shot kill
        public float EndDamage { get; private set; }

        public float HeadDmgMultiplier { get; private set; }
        public float BodyDmgMultiplier { get; private set; }
        public float LimbsDmgMultiplier { get; private set; }

        public enum Accuarcy { High, Medium, Low }
        public Accuarcy Accuracy { get; private set; }

        public float GetDamageFromRange(float range)
        {
            if (range < Range)
                return Damage;
            else if (range > EndRange)
                return EndDamage;
            else
                return MathHelper.Lerp(Damage, EndRange, (range - Range) / (EndRange - Range));
        }

        public const float HIGHRECOIL = 8;
        public const float MODERATERECOIL = 3.5f;
        public const float LOWRECOIL = 2;

        public float Recoil { get; private set; }
        //no aim miss to do add a random amount of rotation to the ray transform

        public enum FireTypeEnum { Auto, SemiAuto }
        public FireTypeEnum FireType { get; private set; }

        public bool BurstFire { get; private set; }
        public float BurstFireDelay { get; private set; }
        public int BurstShotsFired { get; private set; } //# in a burst

        private GunType(int range, int endRange,
            int startingAmmo, int maxAmmo, int clipSize,
            int coolDown, int reloadLoaded, int reloadUnloaded,
            float recoil, FireTypeEnum fireType,
            float damage, float endDamage,
            float headDmgMultiplier, float bodyDmgMultiplier, float limbDmgMultiplier, int extendedClipSize, int moreStartingAmmo, Accuarcy acc)
        {
            Range = range;
            EndRange = endRange;

            StartingAmmo = startingAmmo;
            MaxAmmo = maxAmmo;
            ClipSize = clipSize;

            CoolDown = coolDown;
            ReloadLoaded = reloadLoaded;
            ReloadUnloaded = reloadUnloaded;

            Recoil = recoil;

            Damage = damage;
            EndDamage = endDamage;

            HeadDmgMultiplier = headDmgMultiplier;
            BodyDmgMultiplier = bodyDmgMultiplier;
            LimbsDmgMultiplier = limbDmgMultiplier;

            FireType = fireType;

            BurstFire = false;
            BurstFireDelay = 0;
            BurstShotsFired = 0;

            ExtendedClipSize = extendedClipSize;
            MoreStartingAmmo = moreStartingAmmo;

            Accuracy = acc;
        }

        private GunType(int range, int endRange,
            int startingAmmo, int maxAmmo, int clipSize,
            int coolDown, int reloadLoaded, int reloadUnloaded,
            float recoil, FireTypeEnum fireType,
            float damage, float endDamage,
            float headDmgMultiplier, float bodyDmgMultiplier, float limbDmgMultiplier,
            float burstFireDelay, int shotsFired, int extendedClipSize, int moreStartingAmmo, Accuarcy acc)
        {
            Range = range;
            EndRange = endRange;

            StartingAmmo = startingAmmo;
            MaxAmmo = maxAmmo;
            ClipSize = clipSize;

            CoolDown = coolDown;
            ReloadLoaded = reloadLoaded;
            ReloadUnloaded = reloadUnloaded;

            Recoil = recoil;

            Damage = damage;
            EndDamage = endDamage;

            HeadDmgMultiplier = headDmgMultiplier;
            BodyDmgMultiplier = bodyDmgMultiplier;
            LimbsDmgMultiplier = limbDmgMultiplier;

            FireType = fireType;

            BurstFire = true;
            BurstFireDelay = burstFireDelay;
            BurstShotsFired = shotsFired;

            ExtendedClipSize = extendedClipSize;
            MoreStartingAmmo = moreStartingAmmo;

            Accuracy = acc;
        }



        public const byte
            GUNID_COLT45 = 0,
            GUNID_MAGNUM = 1,
            GUNID_VECTOR = 2,
            GUNID_UMP45 = 3,
            GUNID_MP5K = 4,
            GUNID_DOUBLEBARREL = 5,
            GUNID_12GAUGE = 6,
            GUNID_AA12 = 7,
            GUNID_FAL = 8,
            GUNID_AK47 = 9,
            GUNID_M16 = 10,
            GUNID_MINIGUN = 11,
            GUNID_SWORD = 12;
        public static GunType[] GunTypes = new GunType[]
        {
            //range,endragne,startammo,maxammo,clip
            new GunType(16, 32, 40, 64, 8, 50, 1900, 2650, LOWRECOIL + 1, FireTypeEnum.SemiAuto, 40, 20, 1.4f, 1f, .8f, 8, 48, Accuarcy.High), // colt
            new GunType(22, 76, 30, 42, 6, 50, 3000, 3000, LOWRECOIL + 1, FireTypeEnum.SemiAuto, 55, 35, 1.4f, 1f, .8f, 6, 36, Accuarcy.High), // magnum

            new GunType(48,64,120,180,30, 60, 2200, 3000, LOWRECOIL * .6f, FireTypeEnum.Auto, 35, 25, 1.4f, 1, .75f, 45, 150, Accuarcy.Medium), //vecyot
            new GunType(48,64,128,192, 32, 90, 2500, 3200, LOWRECOIL * .6f, FireTypeEnum.Auto, 35, 25, 1.4f, 1, .75f, 48, 160, Accuarcy.Medium), //ump
            new GunType(57,83,120, 180, 30, 70, 2500, 3300, LOWRECOIL * .6f, FireTypeEnum.Auto, 30, 20, 1.4f, 1, .75f, 45, 150, Accuarcy.Medium ), // mp5k

            new GunType(25, 32, 24, 40, 2, 50, 2650, 2650, 6, FireTypeEnum.SemiAuto, 80, 50, 1.4f, 1.2f, 1f, 2, 30, Accuarcy.Low), //double barrel
            new GunType(19,32, 18, 30, 1, 50, 1750, 1750, 6, FireTypeEnum.SemiAuto, 90, 60, 1.4f, 1.2f, 1f, 1, 22, Accuarcy.Low), //single
            new GunType(16, 32, 40, 56, 8, 150, 2770, 3800, 2.5f, FireTypeEnum.Auto, 51.2f, 30, 1.4f, 1.2f, 1f, 8, 48, Accuarcy.Low), //aa12

            new GunType(64,96, 60, 90, 15, 95, 2700, 3460, MODERATERECOIL, FireTypeEnum.SemiAuto, 42, 27.5f, 1.25f, .9f, .8f, 30, 75, Accuarcy.Medium), //FAL
            new GunType(96, 128, 120, 180, 30, 85, 2500, 3450,  LOWRECOIL * .6f, FireTypeEnum.Auto, 35, 30, 1.4f, 1, .8f,40, 150, Accuarcy.Medium), //AK47
            new GunType(96, 128, 120, 180, 30, 130, 2030, 2360, LOWRECOIL * .5f, FireTypeEnum.SemiAuto, 35, 25, 1.4f, 1, .8f, 100, 3, 42, 150, Accuarcy.Medium), //m16
            
            new GunType(100, 128, 192, 256, 30, 85, 2500, 3450,  LOWRECOIL * .6f, FireTypeEnum.Auto, 900, 600, 1.4f, 1, .8f,40, 180, Accuarcy.Medium),//minigun
            new GunType(2, 3, 192, 256, 30, 300, 30,30,  HIGHRECOIL * 2.5f, FireTypeEnum.SemiAuto, 120, 100, 1.4f, 1, .8f,40, 150, Accuarcy.High)//sword
        };

    }

    public class Gun : IUseableInventoryItem
    {
        public GunType GunType { get { return GunType.GunTypes[GunTypeID]; } }
        public byte GunTypeID { get; private set; }

        private bool ballisiticTip;
        public bool ExtendedMags { get; private set; }

        private float coolDownDelta;
        public int CurrentAmmo { get; private set; }
        public int CurrentAmmoInClip { get; private set; }
        private int currentBurstShot;

        public static bool ReloadFaster = false;

        public int ClipSize { get { if (ExtendedMags) return GunType.ExtendedClipSize; else return GunType.ClipSize; } }

        public void AddAmmo(int ammoAmount)
        {
            CurrentAmmo += ammoAmount;
            totalAmmo = CurrentAmmo - CurrentAmmoInClip;
        }

        public float GetDamage(float range)
        {
            if (ballisiticTip)
                return GunType.GetDamageFromRange(range) * 1.25f;
            else
                return GunType.GetDamageFromRange(range);
        }

        public int totalAmmo;

        public delegate void BurstFireDelgate(Gun sender);
        private BurstFireDelgate burstFireDelgate;
        public Gun(byte gunType, BurstFireDelgate burstFireDelgate, bool moreammo, bool extendedmags, bool ballisiticTip)
        {
            GunTypeID = gunType;

            CanReload = true;
            this.burstFireDelgate = burstFireDelgate;

            if (moreammo)
                CurrentAmmo = this.GunType.MoreStartingAmmo;
            else
                CurrentAmmo = this.GunType.StartingAmmo;

            this.ballisiticTip = ballisiticTip;
            this.ExtendedMags = extendedmags;

            totalAmmo = CurrentAmmo - ClipSize;
            CurrentAmmoInClip = ClipSize;
        }

        public Gun(byte gunType, BurstFireDelgate burstFireDelgate, bool extendedmags, int ammoLeft)
        {
            GunTypeID = gunType;

            CanReload = true;
            this.burstFireDelgate = burstFireDelgate;

            CurrentAmmo = ammoLeft;

            this.ExtendedMags = extendedmags;

            CurrentAmmoInClip = 0;

            totalAmmo = CurrentAmmo - ClipSize;
            if (totalAmmo < 0)
                totalAmmo = 0;

            if (CurrentAmmo >= ClipSize)
                CurrentAmmoInClip = ClipSize;
            else
                CurrentAmmoInClip = CurrentAmmo;
        }

        public void MaxFillAmmo()
        {
            CurrentAmmo = GunType.MaxAmmo;
            totalAmmo = CurrentAmmo - ClipSize;
            CurrentAmmoInClip = ClipSize;
        }

        public bool IsTimeToFire()
        {
            if (MinerOfDuty.game.WeaponsEnabled)
                return coolDownDelta <= 0 && reloadTime <= 0;
            else
            {
                MinerOfDuty.game.showWeaponsDisabled = 1000;
                return false;
            }
        }

        public bool NeedsToReload()
        {
            return CurrentAmmoInClip == 0;
        }

        public void Deselected()
        {
            CanReload = true;
            reloadTime = 0;
            coolDownDelta = 0;
            currentBurstShot = 0;
            burstShotDelay = 0;
        }

        public bool CanReload { get; private set; }
        private float reloadTime;
        public void Reload()
        {
            if (CanReload)
            {
                CanReload = false;


                if (ReloadFaster)
                {
                    if (CurrentAmmoInClip == 0)
                        reloadTime = GunType.ReloadUnloaded * .85f;
                    else
                        reloadTime = GunType.ReloadLoaded * .85f;
                }
                else
                {
                    if (CurrentAmmoInClip == 0)
                        reloadTime = GunType.ReloadUnloaded;
                    else
                        reloadTime = GunType.ReloadLoaded;
                }

            }
        }

        public bool OutOfAmmo()
        {
            return CurrentAmmo == 0;
        }

        public void Update(GameTime gameTime)
        {
            if (coolDownDelta >= 0)
                coolDownDelta -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (currentBurstShot != 0)
            {
                burstShotDelay -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (burstShotDelay <= 0)
                {
                    currentBurstShot++;
                    if (currentBurstShot == GunType.BurstShotsFired)
                    {
                        currentBurstShot = 0;
                        burstShotDelay = 0;
                    }
                    else
                        burstShotDelay = GunType.BurstFireDelay;

                    CurrentAmmo -= 1;
                    CurrentAmmoInClip -= 1;
                    burstFireDelgate.Invoke(this);
                }
            }

            if (reloadTime > 0)
            {
                reloadTime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (reloadTime < 0)
                {
                    CanReload = true;
                    totalAmmo = CurrentAmmo - ClipSize;
                    if (totalAmmo < 0)
                        totalAmmo = 0;

                    if (CurrentAmmo >= ClipSize)
                        CurrentAmmoInClip = ClipSize;
                    else
                        CurrentAmmoInClip = CurrentAmmo;
                }
            }
        }

        private float burstShotDelay;

        public void Fire()
        {

            if (GunTypeID == GunType.GUNID_SWORD)
            {
                coolDownDelta = GunType.CoolDown;
                Audio.PlaySound(Audio.SOUND_SWORD);
                return;
            }

            if (MinerOfDuty.game.player.useInvincibleityYeahBitchSpeellingISForLosers == false)
            {
                CurrentAmmo -= 1;
                CurrentAmmoInClip -= 1;
            }

            if (GunType.BurstFire == false)
            {
                coolDownDelta = GunType.CoolDown;
            }
            else
            {
                coolDownDelta = GunType.CoolDown + (GunType.BurstFireDelay * GunType.BurstShotsFired);
                currentBurstShot++;
                burstShotDelay = GunType.BurstFireDelay;
            }
        }

        public bool IsTool
        {
            get { return false; }
        }

        public bool IsGoggle
        {
            get { return false; }
        }

        public bool IsGun
        {
            get { return true; }
        }
    }
}