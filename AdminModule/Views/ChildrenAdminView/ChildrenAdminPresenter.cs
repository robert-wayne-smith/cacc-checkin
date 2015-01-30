namespace CACCCheckIn.Modules.Admin.Views
{
    public class ChildrenAdminPresenter
    {
        public IChildrenAdminView View { get; set; }

        public ChildrenAdminPresenter(IChildrenAdminView view)
        {
            this.View = view;
        }
    }
}
