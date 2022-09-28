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

namespace Extension.Utilities
{

    public static class BulletTypeHelper
    {

        public static unsafe BulletVelocity GetVelocity(this Pointer<BulletClass> pBullet)
        {
            return GetVelocity(pBullet.Ref.SourceCoords, pBullet.Ref.TargetCoords, pBullet.Ref.Speed);
        }

        public static unsafe BulletVelocity GetVelocity(CoordStruct sourcePos, CoordStruct targetPos, int speed)
        {
            BulletVelocity velocity = new BulletVelocity(targetPos.X - sourcePos.X, targetPos.Y - sourcePos.Y, targetPos.Z - sourcePos.Z);
            velocity *= speed / targetPos.DistanceFrom(sourcePos);
            return velocity;
        }

        public static unsafe BulletVelocity RecalculateBulletVelocity(this Pointer<BulletClass> pBullet)
        {
            CoordStruct targetPos = pBullet.Ref.TargetCoords;
            Pointer<AbstractClass> pTarget = pBullet.Ref.Target;
            if (!pTarget.IsNull)
            {
                targetPos = pTarget.Ref.GetCoords();
            }
            return pBullet.RecalculateBulletVelocity(targetPos);
        }

        public static unsafe BulletVelocity RecalculateBulletVelocity(this Pointer<BulletClass> pBullet, CoordStruct targetPos)
        {
            return pBullet.RecalculateBulletVelocity(pBullet.Ref.Base.Base.GetCoords(), targetPos);
        }

        public static unsafe BulletVelocity RecalculateBulletVelocity(this Pointer<BulletClass> pBullet, CoordStruct sourcePos, CoordStruct targetPos)
        {
            BulletVelocity velocity = new BulletVelocity(targetPos.X - sourcePos.X, targetPos.Y - sourcePos.Y, targetPos.Z - sourcePos.Z);
            velocity *= pBullet.Ref.Speed / targetPos.DistanceFrom(sourcePos);
            pBullet.Ref.Velocity = velocity;
            pBullet.Ref.SourceCoords = sourcePos;
            pBullet.Ref.TargetCoords = targetPos;
            return velocity;
        }



    }
}
