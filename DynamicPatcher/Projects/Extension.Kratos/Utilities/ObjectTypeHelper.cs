using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Utilities
{

    public static class ObjectTypeHelper
    {
        #region 类型装换
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CastToBullet(this Pointer<ObjectClass> pObject, out Pointer<BulletClass> pBullet)
        {
            return pObject.CastIf(AbstractType.Bullet, out pBullet);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CastToBuilding(this Pointer<ObjectClass> pObject, out Pointer<BuildingClass> pBuilding)
        {
            return pObject.CastIf(AbstractType.Building, out pBuilding);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CastToBuilding(this Pointer<TechnoClass> pTechno, out Pointer<BuildingClass> pBuilding)
        {
            return pTechno.Convert<ObjectClass>().CastToBuilding(out pBuilding);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CastToInfantry(this Pointer<ObjectClass> pObject, out Pointer<InfantryClass> pInfantry)
        {
            return pObject.CastIf(AbstractType.Infantry, out pInfantry);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CastToInfantry(this Pointer<TechnoClass> pTechno, out Pointer<InfantryClass> pInfantry)
        {
            return pTechno.Convert<ObjectClass>().CastToInfantry(out pInfantry);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CastToUnit(this Pointer<ObjectClass> pObject, out Pointer<UnitClass> pUnit)
        {
            return pObject.CastIf(AbstractType.Unit, out pUnit);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CastToUnit(this Pointer<TechnoClass> pTechno, out Pointer<UnitClass> pUnit)
        {
            return pTechno.Convert<ObjectClass>().CastToUnit(out pUnit);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CastToAircraft(this Pointer<ObjectClass> pObject, out Pointer<AircraftClass> pAircraft)
        {
            return pObject.CastIf(AbstractType.Aircraft, out pAircraft);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CastToAircraft(this Pointer<TechnoClass> pTechno, out Pointer<AircraftClass> pAircraft)
        {
            return pTechno.Convert<ObjectClass>().CastToAircraft(out pAircraft);
        }
        #endregion

        #region 状态检查
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDead(this Pointer<TechnoClass> pTechno)
        {
            return pTechno.IsNull || pTechno.Convert<ObjectClass>().IsDead() || pTechno.Ref.IsCrashing || pTechno.Ref.IsSinking;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDead(this Pointer<ObjectClass> pObject)
        {
            return pObject.IsNull || pObject.Ref.Health <= 0 || !pObject.Ref.IsAlive;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInvisible(this Pointer<TechnoClass> pTechno)
        {
            return pTechno.IsNull || pTechno.Convert<ObjectClass>().IsInvisible();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInvisible(this Pointer<ObjectClass> pObject)
        {
            return pObject.IsNull || pObject.Ref.InLimbo; // || !pObject.Ref.IsVisible;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCloaked(this Pointer<TechnoClass> pTechno, bool includeCloaking = true)
        {
            return pTechno.IsNull || pTechno.Ref.CloakStates == CloakStates.Cloaked || !includeCloaking || pTechno.Ref.CloakStates == CloakStates.Cloaking;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDeadOrInvisible(this Pointer<TechnoClass> pTarget)
        {
            return pTarget.IsDead() || pTarget.IsInvisible();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDeadOrInvisible(this Pointer<BulletClass> pBullet)
        {
            Pointer<ObjectClass> pObject = pBullet.Convert<ObjectClass>();
            return pObject.IsDead() || pObject.IsInvisible();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDeadOrInvisible(this Pointer<ObjectClass> pObject)
        {
            return pObject.IsDead() || pObject.IsInvisible();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDeadOrInvisibleOrCloaked(this Pointer<TechnoClass> pTechno)
        {
            return pTechno.IsDeadOrInvisible() || pTechno.IsCloaked();
        }

        public static bool InAir(this Pointer<TechnoClass> pTechno, bool stand = false)
        {
            if (!pTechno.IsNull)
            {
                if (stand)
                {
                    return pTechno.Ref.Base.GetHeight() > Game.LevelHeight * 2;
                }
                return pTechno.Ref.Base.Base.IsInAir();
            }
            return false;
        }
        #endregion

        public static bool IsBulletAndGetAttackAndHouse(this Pointer<ObjectClass> pObject, out Pointer<TechnoClass> pOwner, out Pointer<HouseClass> pHouse)
        {
            bool isBullet = false;
            if (isBullet = pObject.CastToBullet(out Pointer<BulletClass> pBullet))
            {
                pOwner = pBullet.Ref.Owner;
                pHouse = pBullet.GetHouse();
            }
            else
            {
                Pointer<TechnoClass> pTechno = pObject.Convert<TechnoClass>();
                pOwner = pTechno;
                pHouse = pTechno.Ref.Owner;
            }
            return isBullet;
        }

        public static bool CanHit(this Pointer<BuildingClass> pBuilding, int targetZ, bool blade = false, int zOffset = 0)
        {
            if (!blade)
            {
                int height = pBuilding.Ref.Type.Ref.Height;
                int sourceZ = pBuilding.Ref.Base.Base.Base.GetCoords().Z;
                // Logger.Log("Building Height {0}", height);
                return targetZ <= (sourceZ + height * Game.LevelHeight + zOffset);
            }
            return blade;
        }
    }
}
