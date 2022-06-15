using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.ExtendedLoadout
{
    /// <summary>
    /// Implemented PostClose in Dialog_ManageLoadouts class. Used for update Slots cache in Loadout_Multi
    /// </summary>
    [HotSwappable]
    public class Dialog_ManageLoadouts_Extended : Dialog_ManageLoadouts
    {
        private Pawn? _pawn;
        private Loadout? _pawnLoadout;
        private Vector2 _cardSize;

        public Dialog_ManageLoadouts_Extended(Loadout loadout) : base(loadout)
        {
        }

        public Dialog_ManageLoadouts_Extended(Pawn pawn, Loadout loadout) : base(loadout)
        {
            _pawn = pawn;
            _pawnLoadout = loadout;
            _cardSize = CharacterCardUtility.PawnCardSize(pawn);
            DbgLog.Msg($"cardSize: {_cardSize}");
        }

        public override Vector2 InitialSize
        {
            get
            {
                var ceSize = base.InitialSize;
                return _pawn == null ? ceSize : new Vector2(ceSize.x + _cardSize.x + 50f, Mathf.Max(ceSize.y, _cardSize.y));
            }
        }

        public override void DoWindowContents(Rect canvas)
        {
            if (_pawn != null)
            {
                var ceSize = base.InitialSize;
                var cardRect = canvas.RightPartPixels(_cardSize.x);//new Rect(canvas.x + ceSize.x, canvas.y + ceSize.y, _cardSize.x, _cardSize.y);
                CharacterCardUtility.DrawCharacterCard(cardRect, _pawn);
                canvas = canvas.LeftPartPixels(ceSize.x);
                // DbgLog.Msg($"ceSize: {ceSize}, cardRect: {cardRect}, pawn: {_pawn}");

                // reset selected pawn after change loadout
                if (CurrentLoadout != _pawnLoadout)
                {
                    _pawn = null;
                }
            }
            
            base.DoWindowContents(canvas);
        }

        /// <summary>
        /// Notify all loadouts multi Dialog_ManageLoadouts closed
        /// </summary>
        public override void PostClose()
        {
            base.PostClose();

            foreach (var loadoutMulti in LoadoutMulti_Manager.LoadoutsMulti)
            {
                loadoutMulti.NotifyLoadoutChanged();
            }
        }
    }
}