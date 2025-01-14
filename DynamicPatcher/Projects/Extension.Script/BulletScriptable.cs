﻿using Extension.Ext;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Script
{
    public interface IBulletScriptable : IObjectScriptable
    {
        void OnDetonate(Pointer<CoordStruct> pCoords);
    }


    [Serializable]
    public abstract class BulletScriptable : Scriptable<BulletExt>, IBulletScriptable
    {
        public BulletScriptable(BulletExt owner) : base(owner)
        {
        }

        protected Pointer<BulletClass> pBullet => Owner.OwnerObject;
        protected string section => pBullet.Ref.Type.Ref.Base.Base.ID;

        public virtual void OnInit() { }
        public virtual void OnUnInit() { }

        public virtual void OnPut(Pointer<CoordStruct> pLocation, DirType dirType) { }

        [Obsolete("not support OnRemove in BulletScriptable yet", true)]
        public void OnRemove()
        {
            throw new NotSupportedException("not support OnRemove in BulletScriptable yet");
        }
        [Obsolete("not support OnReceiveDamage in BulletScriptable yet", true)]
        public void OnReceiveDamage(Pointer<int> pDamage, int distanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool ignoreDefenses, bool preventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            throw new NotSupportedException("not support OnReceiveDamage in BulletScriptable yet");
        }
        [Obsolete("not support OnReceiveDamage in BulletScriptable yet", true)]
        public void OnReceiveDamage2(Pointer<int> pRealDamage, Pointer<WarheadTypeClass> pWH, DamageState damageState,
            Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            throw new NotSupportedException("not support OnReceiveDamage in BulletScriptable yet");
        }
        [Obsolete("not support OnReceiveDamage in BulletScriptable yet", true)]
        public void OnReceiveDamageDestroy()
        {
            throw new NotSupportedException("not support OnReceiveDamage in BulletScriptable yet");
        }

        public virtual void OnDetonate(Pointer<CoordStruct> pCoords) { }
    }
}
