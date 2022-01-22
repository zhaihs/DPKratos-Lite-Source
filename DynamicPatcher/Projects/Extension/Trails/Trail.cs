using DynamicPatcher;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Ext
{


    [Serializable]
    public class Trail
    {

        public TrailType Type;
        public CoordStruct FLH;
        public bool IsOnTurret;
        public List<LandType> OnLandTypes;
        public List<TileType> OnTileTypes;

        private CoordStruct LastLocation;
        private bool canDraw;
        private int initialDelay;
        private TimerStruct delayTimer;

        private bool forceDraw;
        private DrivingState drivingState;

        public Trail(TrailType type)
        {
            this.Type = type;
            this.LastLocation = default;
            if (type.InitialDelay > 0)
            {
                this.canDraw = false;
                this.initialDelay = type.InitialDelay;
            }
            else
            {
                this.canDraw = true;
                this.initialDelay = 0;
            }
            this.forceDraw = false;
            this.drivingState = DrivingState.Moving;
        }

        public void SetDrivingState(DrivingState state)
        {
            this.drivingState = state;
            if (state == DrivingState.Stop)
            {
                forceDraw = true;
            }
        }

        private bool CanDraw()
        {
            if (!canDraw)
            {
                if (initialDelay > 0)
                {
                    delayTimer = new TimerStruct(initialDelay);
                    initialDelay = 0;
                }
                canDraw = delayTimer.Expired();
            }
            return canDraw;
        }

        private bool CheckVertical(CoordStruct sourcePos, CoordStruct targetPos)
        {
            return (Type.IgnoreVertical ? (Math.Abs(sourcePos.X - targetPos.X) > 32 || Math.Abs(sourcePos.Y - targetPos.Y) > 32) : true);
        }

        private bool IsOnLand(CoordStruct sourcePos)
        {
            if (null != OnLandTypes && OnLandTypes.Count > 0)
            {
                Pointer<CellClass> pCell = MapClass.Instance.GetCellAt(sourcePos);
                if (!pCell.IsNull)
                {
                    LandType landType = pCell.Ref.LandType;

                    // Logger.Log("当前格子的地形类型{0}, 瓷砖类型{1}", landType, pCell.Ref.GetTileType());
                    if (OnLandTypes.Contains(landType))
                    {
                        if (null != OnTileTypes && OnTileTypes.Count > 0)
                        {
                            return OnTileTypes.Contains(pCell.Ref.GetTileType());
                        }
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        public void DrawTrail(Pointer<HouseClass> pHouse, CoordStruct sourcePos, CoordStruct createOffset = default)
        {
            if (default != LastLocation)
            {
                CoordStruct targetPos = LastLocation;
                int distance = Type.Distance;
                if (sourcePos.DistanceFrom(targetPos) > distance || forceDraw)
                {
                    if ((CanDraw() && CheckVertical(sourcePos, targetPos)) || forceDraw)
                    {
                        forceDraw = false;
                        if (IsOnLand(sourcePos))
                        {
                            RealDrawTrail(sourcePos, targetPos, pHouse);
                        }
                        drivingState = DrivingState.Moving;
                    }
                    LastLocation = sourcePos;
                }
            }
            else
            {
                LastLocation = sourcePos - createOffset;
            }
        }

        public void RealDrawTrail(CoordStruct sourcePos, CoordStruct targetPos, Pointer<HouseClass> pHouse)
        {
            switch (Type.Mode)
            {
                case TrailMode.LASER:
                    ColorStruct houseColor = default;
                    if (!pHouse.IsNull)
                    {
                        houseColor = pHouse.Ref.LaserColor;
                    }
                    BulletEffectHelper.DrawLine(sourcePos, targetPos, Type.LaserType, Type.LaserType.IsHouseColor ? houseColor : default);
                    break;
                case TrailMode.ELECTIRIC:
                    BulletEffectHelper.DrawBolt(sourcePos, targetPos, Type.IsAlternateColor);
                    break;
                case TrailMode.BEAM:
                    BulletEffectHelper.DrawBeam(sourcePos, targetPos, Type.BeamType);
                    break;
                case TrailMode.PARTICLE:
                    BulletEffectHelper.DrawParticele(sourcePos, targetPos, Type.ParticleSystem);
                    break;
                case TrailMode.ANIM:
                    DrawAnimTrail(sourcePos, pHouse);
                    break;
            }
        }

        public void DrawAnimTrail(CoordStruct sourcePos, Pointer<HouseClass> pHouse)
        {
            string animType = Type.WhileDrivingAnim;
            switch (drivingState)
            {
                case DrivingState.Start:
                    animType = Type.StartDrivingAnim;
                    break;
                case DrivingState.Stop:
                    animType = Type.StopDrivingAnim;
                    break;
            }
            if (!string.IsNullOrEmpty(animType))
            {
                Pointer<AnimTypeClass> pAnimType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(animType);
                if (!pAnimType.IsNull)
                {
                    Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, sourcePos);
                    pAnim.Ref.Owner = pHouse;
                }
            }
        }

    }

}