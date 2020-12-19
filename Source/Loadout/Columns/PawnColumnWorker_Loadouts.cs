using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    public class PawnColumnWorker_Loadout1 : PawnColumnWorker_Loadout_Multi
    {
        public override int ColumnIdx => 0;
        public override bool DrawBtnManageLoadouts => true;
    }
    public class PawnColumnWorker_Loadout2 : PawnColumnWorker_Loadout_Multi
    {
        public override int ColumnIdx => 1;
        public override bool DrawBtnManageLoadouts => false;
    }
    public class PawnColumnWorker_Loadout3 : PawnColumnWorker_Loadout_Multi
    {
        public override int ColumnIdx => 2;
        public override bool DrawBtnManageLoadouts => false;
    }
    public class PawnColumnWorker_Loadout4 : PawnColumnWorker_Loadout_Multi
    {
        public override int ColumnIdx => 3;
        public override bool DrawBtnManageLoadouts => false;
    }
    public class PawnColumnWorker_Loadout5 : PawnColumnWorker_Loadout_Multi
    {
        public override int ColumnIdx => 4;
        public override bool DrawBtnManageLoadouts => false;
    }
}