using GSC.Data.Dto.Screening;


namespace GSC.Respository.Screening
{
    public interface IScreeningProgress
    {
        ScreeningProgressDto GetScreeningProgress(int screeningEntryId, int screeningTemplateId);
    }
}
