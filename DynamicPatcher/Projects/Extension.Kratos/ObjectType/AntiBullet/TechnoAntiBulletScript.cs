using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    [UpdateAfter(typeof(TechnoStatusScript))]
    public class TechnoAntiBulletScript : TechnoScriptable
    {
        public TechnoAntiBulletScript(TechnoExt owner) : base(owner) { }

        public AntiBulletData AntiBulletData => Ini.GetConfig<AntiBulletData>(Ini.RulesDependency, section).Data;

        private TimerStruct delayTimer;

        public override void Awake()
        {
            if (null == AntiBulletData || !AntiBulletData.Enable || NoPassengers() || PrimaryWeaponNoAA())
            {
                // Logger.Log($"{Game.CurrentFrame} [{section}] 关闭 AntiMissile. {NoPassengers()} {PrimaryWeaponNoAA()}");
                AntiBulletData.Enable = false;
                GameObject.RemoveComponent(this);
                return;
            }
        }

        private bool NoPassengers()
        {
            // 没有乘客位
            return AntiBulletData.ForPassengers && pTechno.Ref.Type.Ref.Passengers <= 0;
        }

        private bool PrimaryWeaponNoAA()
        {
            bool noAA = true;
            // 检查单位时候具有主武器
            if (AntiBulletData.Self)
            {
                Pointer<WeaponTypeClass> pPrimary = pTechno.Ref.Type.Ref.get_Primary();
                if (pPrimary.IsNull || pPrimary.Ref.Projectile.IsNull)
                {
                    Logger.LogWarning($"{Game.CurrentFrame} Techno [{section}] has no Primary weapon, disable AntiMissile.");
                }
                else
                {
                    if (noAA = !pPrimary.Ref.Projectile.Ref.AA)
                    {
                        Logger.LogWarning($"{Game.CurrentFrame} Techno [{section}] Primary weapon has no AA, disable AntiMissile.");
                    }
                }
            }
            return noAA && !AntiBulletData.ForPassengers;
        }

        public override void OnUpdate()
        {
            if (!pTechno.IsDeadOrInvisible())
            {
                if (AntiBulletData.Enable)
                {
                    if (delayTimer.Expired())
                    {
                        int scanRange = AntiBulletData.Range;
                        if (pTechno.Ref.Veterancy.IsElite())
                        {
                            scanRange = AntiBulletData.EliteRange;
                        }
                        // Logger.Log($"{Game.CurrentFrame} 开始搜索抛射体，Range = {AntiBulletData.Range}，EliteRange = {AntiBulletData.EliteRange}，Rate = {AntiBulletData.Rate}");
                        ExHelper.FindBulletTargetHouse(pTechno, (pBullet) =>
                        {
                            if (AntiBulletData.ScanAll || pBullet.Ref.Target == pTechno.Convert<AbstractClass>())
                            {
                                // Scan Target
                                if (pTechno.Ref.Base.DistanceFrom(pBullet.Convert<ObjectClass>()) <= scanRange
                                    && pBullet.TryGetStatus(out BulletStatusScript bulletStatus) && bulletStatus.LifeData.Interceptable)
                                {
                                    // 确认目标
                                    // Logger.Log($"{Game.CurrentFrame} 确认目标 {pBullet}");
                                    delayTimer.Start(AntiBulletData.Rate);
                                    if (AntiBulletData.ForPassengers)
                                    {
                                        pTechno.Ref.SetTargetForPassengers(pBullet.Convert<AbstractClass>());
                                    }
                                    if (AntiBulletData.Self && (pTechno.Ref.Target.IsNull || pTechno.Ref.Target.Ref.IsDead()))
                                    {
                                        pTechno.Ref.SetTarget(pBullet.Convert<AbstractClass>());
                                    }
                                    return true;
                                }
                            }
                            return false;
                        });
                    }
                }
            }
        }
    }
}
