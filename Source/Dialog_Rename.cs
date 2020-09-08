namespace ChangeDresser
{
    class Dialog_Rename : Verse.Dialog_Rename
    {
        private Building_Dresser Dresser;

        public Dialog_Rename(Building_Dresser dresser) : base()
        {
            this.Dresser = dresser;
        }

        protected override void SetName(string name)
        {
            Dresser.Name = name;
        }
    }
}
