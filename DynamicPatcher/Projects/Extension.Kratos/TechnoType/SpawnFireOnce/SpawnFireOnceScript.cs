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
    [UpdateBefore(typeof(TechnoStatusScript))]
    public class SpawnFireOnceScript : TechnoScriptable
    {

        public SpawnFireOnceScript(TechnoExt owner) : base(owner) { }

        private SpawnFireOnceData spawnFireOnceData => Ini.GetConfig<SpawnFireOnceData>(Ini.RulesDependency, section).Data;

        private TimerStruct spawnFireOnceDelay;
        private bool spawnFireFlag;

        public override void Awake()
        {
            // I'm not a Spawn or Carrier
            if (pTechno.Ref.SpawnOwner.IsNull && pTechno.Ref.SpawnManager.IsNull)
            {
                GameObject.RemoveComponent(this);
                return;
            }
        }

        public override void OnUpdate()
        {
            if (!pTechno.IsDeadOrInvisible())
            {
                CoordStruct sourcePos = pTechno.Ref.Base.Base.GetCoords();
                // I'm spawn
                Pointer<TechnoClass> pSpawnOwner = pTechno.Ref.SpawnOwner;
                if (!pSpawnOwner.IsNull && pSpawnOwner.TryGetComponent<SpawnFireOnceScript>(out SpawnFireOnceScript ext))
                {
                    if (ext.spawnFireOnceData.FireOnce)
                    {
                        ext.CancelSpawnDest();
                    }
                }

                // I'm carrier
                Pointer<SpawnManagerClass> pSpawn = pTechno.Ref.SpawnManager;
                if (!pSpawn.IsNull)
                {
                    if (pSpawn.Ref.Destination.IsNull && pSpawn.Ref.Target.IsNull)
                    {
                        spawnFireFlag = false;
                    }
                }
            }
        }

        public unsafe void CancelSpawnDest()
        {
            if (!spawnFireFlag)
            {
                spawnFireOnceDelay.Start(spawnFireOnceData.Delay);
                spawnFireFlag = true;
            }
            else
            {
                if (!spawnFireOnceDelay.InProgress())
                {
                    Pointer<SpawnManagerClass> pSpawn = pTechno.Ref.SpawnManager;
                    pSpawn.Ref.Destination = Pointer<AbstractClass>.Zero;
                    pSpawn.Ref.SetTarget(Pointer<AbstractClass>.Zero);
                }
            }
        }

    }
}
