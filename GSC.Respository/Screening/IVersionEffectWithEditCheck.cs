namespace GSC.Respository.Screening
{
    public interface IVersionEffectWithEditCheck
    {
        void ApplyEditCheck(int projectDesignId, bool isTrial, double versionNumber);
    }
}
