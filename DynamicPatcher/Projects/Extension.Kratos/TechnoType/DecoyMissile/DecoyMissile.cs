using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class DecoyBullet
    {
        public SwizzleablePointer<BulletClass> Bullet;

        public CoordStruct LaunchPort;

        public int Life;

        public DecoyBullet(Pointer<BulletClass> pBullet, CoordStruct launchPort, int life = 150)
        {
            this.Bullet = new SwizzleablePointer<BulletClass>(pBullet);
            this.LaunchPort = launchPort;
            this.Life = life;
        }

        public bool IsNotDeath()
        {
            if (--Life <= 0)
            {
                // Is death
                if (!Bullet.IsNull)
                {
                    CoordStruct location = Bullet.Ref.Base.Location;
                    Bullet.Ref.Detonate(location);
                    Bullet.Ref.Base.Remove();
                    Bullet.Ref.Base.UnInit();
                }
                Bullet = default;
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return string.Format("{{\"Bullet\": {0}, \"LaunchPort\": {1}, \"Life\": {2}}}", Bullet.Pointer, LaunchPort, Life);
        }

    }

    [Serializable]
    public class DecoyMissile
    {
        public bool Enable;

        // data
        public DecoyMissileData Data;

        public SwizzleablePointer<WeaponTypeClass> Weapon;
        public SwizzleablePointer<WeaponTypeClass> EliteWeapon;

        public SwizzleablePointer<WeaponTypeClass> UseWeapon;
        // public int Damage;
        // public int Burst;
        // public int ROF;
        // public int Distance;

        // control
        public int Delay;
        public int Bullets;
        public int Reloading;
        public bool Fire;

        public bool Elite;

        public List<DecoyBullet> Decoys;

        public DecoyMissile(DecoyMissileData data, Pointer<WeaponTypeClass> pWeapon, Pointer<WeaponTypeClass> pEliteWeapon, bool elite = false)
        {
            this.Enable = true;
            this.Data = data;
            this.Weapon = new SwizzleablePointer<WeaponTypeClass>(pWeapon);
            this.EliteWeapon = new SwizzleablePointer<WeaponTypeClass>(pEliteWeapon);
            if (elite)
            {
                this.UseWeapon = this.EliteWeapon;
            }
            else
            {
                this.UseWeapon = this.Weapon;
            }
            this.Delay = data.Delay;
            this.Bullets = UseWeapon.Ref.Burst;
            this.Reloading = 0;
            this.Fire = data.AlwaysFire;
            this.Elite = elite;
            Decoys = new List<DecoyBullet>();
        }

        public Pointer<WeaponTypeClass> FindWeapon(bool elite)
        {
            if (this.Elite != elite)
            {
                if (elite)
                {
                    this.UseWeapon = this.EliteWeapon;
                }
                else
                {
                    this.UseWeapon = this.Weapon;
                }
                this.Elite = elite;
            }
            return this.UseWeapon;
        }

        public bool DropOne()
        {
            if (--this.Reloading <= 0 && --this.Delay <= 0)
            {
                if (--this.Bullets >= 0)
                {
                    this.Delay = Data.Delay;
                    return true;
                }
                Reload();
            }
            return false;
        }

        public void Reload()
        {
            this.Bullets = this.UseWeapon.Ref.Burst;
            this.Reloading = this.UseWeapon.Ref.ROF;
            this.Fire = this.Data.AlwaysFire;
        }

        public void AddDecoy(Pointer<BulletClass> pDecoy, CoordStruct launchPort, int life)
        {
            if (null == Decoys)
                Decoys = new List<DecoyBullet>();
            DecoyBullet decoy = new DecoyBullet(pDecoy, launchPort, life);
            Decoys.Add(decoy);
        }

        public void ClearDecoy()
        {
            Decoys.RemoveAll((deocy) =>
            {
                return deocy.Life <= 0 || deocy.Bullet.IsNull || !deocy.Bullet.Ref.Base.IsAlive;
            });
        }

        public Pointer<BulletClass> RandomDecoy()
        {
            int count = 0;
            if (null != Decoys && (count = Decoys.Count) > 0)
            {
                int ans = MathEx.Random.Next(count);
                DecoyBullet decoy = Decoys[ans == count ? ans - 1 : ans];
                Decoys.Remove(decoy);
                return decoy.Bullet;
            }
            return Pointer<BulletClass>.Zero;
        }

        public Pointer<BulletClass> CloseEnoughDecoy(CoordStruct pos, double min)
        {
            int index = -1;
            double distance = min;
            for (int i = 0; i < Decoys.Count; i++)
            {
                DecoyBullet decoy = Decoys[i];
                CoordStruct location = pos;
                double x = 0;
                if (!decoy.Bullet.IsNull
                    && (x = pos.DistanceFrom(decoy.Bullet.Ref.Base.Location)) < distance)
                {
                    distance = x;
                    index = i;
                }
            }
            return index >= 0 ? Decoys[index].Bullet.Pointer : default;
        }
    }

}
