namespace CombatExtended.ExtendedLoadout
{
    /// <summary>
    /// Implemented PostClose in Dialog_ManageLoadouts class. Used for update Slots cache in Loadout_Multi
    /// </summary>
    public class Dialog_ManageLoadouts_Extended : Dialog_ManageLoadouts
    {
        public Dialog_ManageLoadouts_Extended(Loadout loadout) : base(loadout)
        {
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