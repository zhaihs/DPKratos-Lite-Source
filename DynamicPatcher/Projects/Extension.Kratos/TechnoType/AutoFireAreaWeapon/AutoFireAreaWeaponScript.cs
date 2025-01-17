using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{


    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    [UpdateAfter(typeof(TechnoStatusScript))]
    public class AutoFireAreaWeaponScript : TechnoScriptable
    {
        public AutoFireAreaWeaponScript(TechnoExt owner) : base(owner) { }

        private AutoFireAreaWeaponData data => Ini.GetConfig<AutoFireAreaWeaponData>(Ini.RulesDependency, section).Data;

        private TimerStruct reloadTimer;
        private TimerStruct initialDelayTimer;

        public override void Awake()
        {
            Pointer<WeaponStruct> pWeapon = IntPtr.Zero;
            if (data.WeaponIndex < 0
                || (pWeapon = pTechno.Ref.GetWeapon(data.WeaponIndex)).IsNull || pWeapon.Ref.WeaponType.IsNull)
            {
                GameObject.RemoveComponent(this);
                return;
            }
            initialDelayTimer.Start(data.InitialDelay);
        }

        public override void OnUpdate()
        {
            if (!pTechno.IsDeadOrInvisible())
            {
                if (initialDelayTimer.Expired() && reloadTimer.Expired())
                {
                    Pointer<WeaponStruct> pWeapon = pTechno.Ref.GetWeapon(data.WeaponIndex);
                    // 检查弹药消耗
                    int technoAmmo = pTechno.Ref.Ammo;
                    int weaponAmmo = Ini.GetConfig<WeaponTypeData>(Ini.RulesDependency, pWeapon.Ref.WeaponType.Ref.Base.ID).Data.Ammo;
                    if (technoAmmo >= 0)
                    {
                        int leftAmmo = technoAmmo - weaponAmmo;
                        if (data.CheckAmmo || data.UseAmmo)
                        {
                            if (leftAmmo < 0)
                            {
                                return;
                            }
                        }
                        if (data.UseAmmo)
                        {
                            pTechno.Ref.Ammo = leftAmmo;
                            pTechno.Ref.ReloadNow();
                        }
                    }
                    reloadTimer.Start(pWeapon.Ref.WeaponType.Ref.ROF);
                    if (data.TargetToGround)
                    {
                        CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                        if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell) && !pCell.IsNull)
                        {
                            pTechno.Ref.Fire_IgnoreType(pCell.Convert<AbstractClass>(), data.WeaponIndex);
                        }
                    }
                    else
                    {
                        pTechno.Ref.Fire_IgnoreType(pTechno.Convert<AbstractClass>(), data.WeaponIndex);
                    }

                }
            }
        }

    }
}