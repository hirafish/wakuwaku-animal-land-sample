namespace BugZoo.Common
{
    public interface IPanelSection
    {
        string SectionTitle { get; }
        void DrawSection();
    }
}
